using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	class ICSMessageSenderTestHelper
	{
		public static void TestProcess(Action process)
		{
			TestProcess(process, manifest => manifest.Factory.Load<EDIInterchange>(new ZQuery { OrderBy = EDIInterchange.Schema.EI_InterchangeNum }));
		}

		public static void TestProcess(Action process, Func<AsycudaManifestHeader, EDIInterchange[]> getInterchanges)
		{
			foreach (var gateway in GetGateways())
			{
				HaveATest(gateway, process, getInterchanges);
			}
		}

		public static void TestProcess(Action process, Action<AsycudaManifestHeader> doSomeAssertions)
		{
			foreach (var gateway in GetGateways())
			{
				HaveATest(gateway, process, doSomeAssertions);
			}
		}

		public static string GetEncryptedPasswordFromGBCustomsRequestXml(ZString xml)
		{
			const string PasswordNodeXPath = "//*[local-name()='GBCustomsRequest']/*[local-name()='Credentials']/*[local-name()='Password']";
			var encryptedPassword = string.Empty;
			var xmlDoc = new XmlDocument();
			if (xmlDoc != null && !xml.IsEmpty)
			{
				xmlDoc.LoadXml(xml);
				encryptedPassword = xmlDoc.DocumentElement?.SelectNodes(PasswordNodeXPath)?.Cast<XmlNode>().FirstOrDefault()?.InnerText;
			}
			return encryptedPassword;
		}

		static void HaveATest(ZString gateway, Action process, Func<AsycudaManifestHeader, EDIInterchange[]> getInterchanges)
		{
			HaveATest(gateway, process, manifest =>
			{
				AssertionWithHtml.CombineAssertions(() =>
				{
					var interchanges = getInterchanges.Invoke(manifest);
					Assertion.AssertEquals("NumberOfInterchanges", 3, interchanges.Length);

					AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == "NEW")
						, "<MetaData><Manifest/></MetaData>"
						, ZString.Empty
						, EDIInterchange.Status.eHubQueued
						, "Create"
						, gateway);
					AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == "AMD")
						, "<MetaData><Manifest/></MetaData>"
						, ZString.Empty
						, EDIInterchange.Status.eHubQueued
						, "Update"
						, gateway);
					AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == "CAN")
						, "<MetaData><Manifest/></MetaData>"
						, ZString.Empty
						, EDIInterchange.Status.eHubQueued
						, "Delete"
						, gateway);
				});
			});
		}

		static void HaveATest(ZString gateway, Action process, Action<AsycudaManifestHeader> doSomeAssertions)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			var factory = new BusinessObjectFactory();

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN001";
			manifestHeader.RegistrationNumber = "123";

			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "ImporterA";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);
			var aaaBranch = factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = orgHeader.PK;
			factory.Save();

			CreateCredentialsForTest(factory);

			manifestHeader.AMA_GB = aaaBranch.PK;

			var newMessage = factory.New<IcsSsGreatBritainEDIMessage>();
			newMessage.EM_ReceiveTransmit = "RCV";
			newMessage.EM_MessageText = "<MetaData><Manifest/></MetaData>";
			newMessage.EM_MessageSubType = "NEW";
			newMessage.EM_MessageNum = "1";
			newMessage.EM_LinkedObject = manifestHeader;

			var amendMessage = factory.New<IcsSsGreatBritainEDIMessage>();
			amendMessage.EM_ReceiveTransmit = "RCV";
			amendMessage.EM_MessageText = "<MetaData><Manifest/></MetaData>";
			amendMessage.EM_MessageSubType = "AMD";
			amendMessage.EM_MessageNum = "2";
			amendMessage.EM_LinkedObject = manifestHeader;

			var cancelMessage = factory.New<IcsSsGreatBritainEDIMessage>();
			cancelMessage.EM_ReceiveTransmit = "RCV";
			cancelMessage.EM_MessageText = "<MetaData><Manifest/></MetaData>";
			cancelMessage.EM_MessageSubType = "CAN";
			cancelMessage.EM_MessageNum = "3";
			cancelMessage.EM_LinkedObject = manifestHeader;

			manifestHeader.Messages.Add(newMessage);
			manifestHeader.Messages.Add(amendMessage);
			manifestHeader.Messages.Add(cancelMessage);
			factory.Save();

			newMessage.EM_ReceiveTransmit = "TRX";
			amendMessage.EM_ReceiveTransmit = "TRX";
			cancelMessage.EM_ReceiveTransmit = "TRX";
			factory.Save();

			process?.Invoke();

			doSomeAssertions?.Invoke(manifestHeader);

			Reset();
		}

		static void CreateCredentialsForTest(BusinessObjectFactory factory)
		{
			TestDataHelper.CreateCredentials(factory, GlbCompany.CurrentCompany.PK, "111", "GB111111111111", PasswordTypesList.Codes.CDS, ZDateTime.Today.AddDays(10)); // Not a match for EORI
			TestDataHelper.CreateCredentials(factory, GlbCompany.CurrentCompany.PK, "222", "GB999999999888", PasswordTypesList.Codes.CDS, ZDateTime.Today.AddDays(10)); // Match for EORI but not first when ordered by badge
			TestDataHelper.CreateCredentials(factory, GlbCompany.CurrentCompany.PK, "111", "GB999999999888", PasswordTypesList.Codes.CDS, ZDateTime.Today.AddDays(10)); // Match for EORI and first when ordered by expiry date
		}

		static void Reset()
		{
			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(EDIInterchange.Schema.TableName);
		}

		static void AssertInterchange(EDIInterchange interchange, ZString bodyText, ZString footerText, ZString status, string service, string gateway)
		{
			var providerType = ProviderType.ICSGB;
			var provider = providerType.ToString();
			var credentialsNode = @"  <Credentials Key=""HYECMT.GB999999999888.111"" />";

			var headerText = $@"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>{provider}</Provider>
  <Service>{service}</Service>
{credentialsNode}
  <JobNumber>MAN001</JobNumber>";
			headerText += interchange.EI_InterchangeType != "NEW" && interchange.EI_InterchangeType != "AMD" ? "" : @"
  <ServiceReference>123</ServiceReference>";
			headerText += @"
  <Version>1.0</Version>
  <ContentType>XML</ContentType>
</GBCustomsRequest>";

			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Header", headerText, interchange.EI_HeaderText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Body", bodyText, interchange.EI_BodyText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Footer", footerText, interchange.EI_FooterText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Status", status, interchange.EI_Status);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.EI_To", "GBCustoms", interchange.EI_To);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.EI_From", "HYECMT.GB999999999888.111", interchange.EI_From);
		}

		public static IEnumerable<ZString> GetGateways()
		{
			yield return "ICS";
		}
	}
}
