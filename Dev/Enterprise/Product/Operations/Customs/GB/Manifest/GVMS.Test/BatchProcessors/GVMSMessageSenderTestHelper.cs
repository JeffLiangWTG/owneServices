using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	class GVMSMessageSenderTestHelper
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
					Assertion.AssertEquals("NumberOfInterchanges", 4, interchanges.Length);

					AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == "NEW")
						, "<MetaData><Manifest/></MetaData>"
						, ZString.Empty
						, "HQU"
						, "Create"
						, gateway);
					AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == "AMD")
						, "<MetaData><Manifest/></MetaData>"
						, ZString.Empty
						, "HQU"
						, "Update"
						, gateway);
					AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == "CAN")
						, "<MetaData><Manifest/></MetaData>"
						, ZString.Empty
						, "HQU"
						, "Delete"
						, gateway);
					AssertInterchange(interchanges.Single(x => x.EI_InterchangeType == "FIN")
						, "<MetaData><Manifest/></MetaData>"
						, ZString.Empty
						, "HQU"
						, "Finalise"
						, gateway);
				});
			});
		}

		static void HaveATest(ZString gateway, Action process, Action<AsycudaManifestHeader> doSomeAssertions)
		{
			var factory = new BusinessObjectFactory();

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN001";
			manifestHeader.RegistrationNumber = "123";

			manifestHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);

			CreateCredentialsForTest(factory);

			var newMessage = factory.New<GVMSEDIMessage>();
			newMessage.EM_ReceiveTransmit = "RCV";
			newMessage.EM_MessageText = "<MetaData><Manifest/></MetaData>";
			newMessage.EM_MessageSubType = "NEW";
			newMessage.EM_MessageNum = "1";
			newMessage.EM_MessageOwner = "ABC";
			newMessage.EM_LinkedObject = manifestHeader;

			var amendMessage = factory.New<GVMSEDIMessage>();
			amendMessage.EM_ReceiveTransmit = "RCV";
			amendMessage.EM_MessageText = "<MetaData><Manifest/></MetaData>";
			amendMessage.EM_MessageSubType = "AMD";
			amendMessage.EM_MessageNum = "2";
			amendMessage.EM_MessageOwner = "ABC";
			amendMessage.EM_LinkedObject = manifestHeader;

			var cancelMessage = factory.New<GVMSEDIMessage>();
			cancelMessage.EM_ReceiveTransmit = "RCV";
			cancelMessage.EM_MessageText = "<MetaData><Manifest/></MetaData>";
			cancelMessage.EM_MessageSubType = "CAN";
			cancelMessage.EM_MessageNum = "3";
			cancelMessage.EM_MessageOwner = "ABC";
			cancelMessage.EM_LinkedObject = manifestHeader;

			var finaliselMessage = factory.New<GVMSEDIMessage>();
			finaliselMessage.EM_ReceiveTransmit = "RCV";
			finaliselMessage.EM_MessageText = "<MetaData><Manifest/></MetaData>";
			finaliselMessage.EM_MessageSubType = "FIN";
			finaliselMessage.EM_MessageNum = "4";
			finaliselMessage.EM_MessageOwner = "ABC";
			finaliselMessage.EM_LinkedObject = manifestHeader;

			manifestHeader.Messages.Add(newMessage);
			manifestHeader.Messages.Add(amendMessage);
			manifestHeader.Messages.Add(cancelMessage);
			manifestHeader.Messages.Add(finaliselMessage);
			factory.Save();

			newMessage.EM_ReceiveTransmit = "TRX";
			amendMessage.EM_ReceiveTransmit = "TRX";
			cancelMessage.EM_ReceiveTransmit = "TRX";
			finaliselMessage.EM_ReceiveTransmit = "TRX";
			factory.Save();

			process?.Invoke();

			doSomeAssertions?.Invoke(manifestHeader);

			Reset();
		}

		static void Reset()
		{
			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(EDIInterchange.Schema.TableName);
		}

		static void AssertInterchange(EDIInterchange interchange, ZString bodyText, ZString footerText, ZString status, string service, string gateway)
		{
			var providerType = GBCustomsRequest.GetProviderType(gateway);
			var provider = providerType.ToString();
			var credentialsNode = @"  <Credentials Key=""EDIDAT.GB999999999888.111"" />";

			var headerText = $@"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>{provider}</Provider>
  <Service>{service}</Service>
{credentialsNode}
  <JobNumber>MAN001</JobNumber>
  <ServiceReference>123</ServiceReference>
  <Version>1.0</Version>
  <ContentType>json</ContentType>
</GBCustomsRequest>";

			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Header", headerText, interchange.EI_HeaderText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Body", bodyText, interchange.EI_BodyText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Footer", footerText, interchange.EI_FooterText);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.Status", status, interchange.EI_Status);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.EI_To", "GBCustoms", interchange.EI_To);
			Assertion.AssertEquals($"{interchange.EI_InterchangeNum}.{interchange.EI_InterchangeType}.EI_To", "EDIDAT.GB999999999888.111", interchange.EI_From);
		}

		public static IEnumerable<ZString> GetGateways()
		{
			yield return GatewayList.Codes.GVMS;
		}

		public static void CreateCredentialsForTest(BusinessObjectFactory factory)
		{
			TestDataHelper.CreateCredentials(factory, GlbCompany.CurrentCompany.PK, "111", "GB111111111111", PasswordTypesList.Codes.CDS, ZDateTime.Today.AddDays(10)); // Not a match for EORI
			TestDataHelper.CreateCredentials(factory, GlbCompany.CurrentCompany.PK, "222", "GB999999999888", PasswordTypesList.Codes.CDS, ZDateTime.Today.AddDays(10)); // Match for EORI but not first when ordered by badge
			TestDataHelper.CreateCredentials(factory, GlbCompany.CurrentCompany.PK, "111", "GB999999999888", PasswordTypesList.Codes.CDS, ZDateTime.Today.AddDays(10)); // Match for EORI and first when ordered by expiry date
		}
	}
}
