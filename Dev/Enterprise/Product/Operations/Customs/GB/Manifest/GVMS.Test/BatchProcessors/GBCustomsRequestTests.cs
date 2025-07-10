using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	class GBCustomsRequestTests : TestCaseWithFactory
	{
		public void TestNew_Manifest()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ImporterA";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB999999999888", Core.Constants.CountryCodes.UnitedKingdom);
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = orgHeader.PK;
			Factory.Save();

			GVMSMessageSenderTestHelper.CreateCredentialsForTest(Factory);

			foreach (var gateway in GVMSMessageSenderTestHelper.GetGateways().Where(x => !x.IsEmpty))
			{
				var manifestHeader = Factory.New<AsycudaManifestHeader>();
				manifestHeader.AMA_JobReference = "MAN001";
				manifestHeader.RegistrationNumber = "123";
				manifestHeader.AMA_GB = aaaBranch.PK;

				var newMessage = (GVMSEDIMessage)manifestHeader.Messages.AddNew(typeof(GVMSEDIMessage));
				newMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
				newMessage.EM_MessageSubType = "NEW";
				newMessage.EM_MessageNum = "1";
				newMessage.EM_MessageOwner = "ABC";
				newMessage.EM_LinkedObject = manifestHeader;

				var amendMessage = (GVMSEDIMessage)manifestHeader.Messages.AddNew(typeof(GVMSEDIMessage));
				amendMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
				amendMessage.EM_MessageSubType = "AMD";
				amendMessage.EM_MessageNum = "2";
				amendMessage.EM_MessageOwner = "ABC";
				amendMessage.EM_LinkedObject = manifestHeader;

				var cancelMessage = (GVMSEDIMessage)manifestHeader.Messages.AddNew(typeof(GVMSEDIMessage));
				cancelMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
				cancelMessage.EM_MessageSubType = "CAN";
				cancelMessage.EM_MessageNum = "3";
				cancelMessage.EM_MessageOwner = "ABC";
				cancelMessage.EM_LinkedObject = manifestHeader;

				var finaliseMessage = (GVMSEDIMessage)manifestHeader.Messages.AddNew(typeof(GVMSEDIMessage));
				finaliseMessage.EM_MessageText = "<MetaData><inventoryLinkingConsolidationRequest/></MetaData>";
				finaliseMessage.EM_MessageSubType = "FIN";
				finaliseMessage.EM_MessageNum = "4";
				finaliseMessage.EM_MessageOwner = "ABC";
				finaliseMessage.EM_LinkedObject = manifestHeader;

				Factory.Save();

				var providerType = GBCustomsRequest.GetProviderType(gateway);
				var credentials = new Credentials
				{
					Key = "EDIDAT.GB999999999888.111"
				};

				AssertNewGBCustomsRequest(GBCustomsRequest.New(newMessage), ServiceType.Create, providerType, manifestHeader.AMA_JobReference, credentials, manifestHeader.RegistrationNumber);

				AssertNewGBCustomsRequest(GBCustomsRequest.New(amendMessage), ServiceType.Update, providerType, manifestHeader.AMA_JobReference, credentials, manifestHeader.RegistrationNumber);

				AssertNewGBCustomsRequest(GBCustomsRequest.New(cancelMessage), ServiceType.Delete, providerType, manifestHeader.AMA_JobReference, credentials, manifestHeader.RegistrationNumber);

				AssertNewGBCustomsRequest(GBCustomsRequest.New(finaliseMessage), ServiceType.Finalise, providerType, manifestHeader.AMA_JobReference, credentials, manifestHeader.RegistrationNumber);
			}
		}

		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestNew()
		{
			GVMSMessageSenderTestHelper.TestProcess(null, manifest =>
			{
				var providerType = GBCustomsRequest.GetProviderType(GatewayList.Codes.GVMS);
				var credentials = new Credentials
				{
					Key = "EDIDAT.GB999999999888.111"
				};

				AssertNewGBCustomsRequest(GBCustomsRequest.New(manifest.Messages.OfType<EDIMessage>().Single(x => x.EM_MessageSubType == "NEW"))
					, ServiceType.Create, providerType, "MAN001", credentials, "123");

				AssertNewGBCustomsRequest(GBCustomsRequest.New(manifest.Messages.OfType<EDIMessage>().Single(x => x.EM_MessageSubType == "AMD"))
					, ServiceType.Update, providerType, "MAN001", credentials, "123");

				AssertNewGBCustomsRequest(GBCustomsRequest.New(manifest.Messages.OfType<EDIMessage>().Single(x => x.EM_MessageSubType == "CAN"))
					, ServiceType.Delete, providerType, "MAN001", credentials, "123");

				AssertNewGBCustomsRequest(GBCustomsRequest.New(manifest.Messages.OfType<EDIMessage>().Single(x => x.EM_MessageSubType == "FIN"))
					, ServiceType.Finalise, providerType, "MAN001", credentials, "123");
			});
		}

		ZString CreateRequest(AsycudaManifestHeader manifest, string subType)
			=> GBCustomsRequest.New(manifest.Messages.OfType<EDIMessage>().Single(message => message.EM_MessageSubType == subType)).Serialize();

		void AssertSerializedGBCustomsRequest(string serializedCustomsRequest, string service)
		{
			var providerType = GBCustomsRequest.GetProviderType(GatewayList.Codes.GVMS);
			var provider = providerType.ToString();
			var credentialsNode = @"  <Credentials Key=""EDIDAT.GB999999999888.111"" />";
			var expectedXml = $@"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>{provider}</Provider>
  <Service>{service}</Service>
{credentialsNode}
  <JobNumber>MAN001</JobNumber>
  <ServiceReference>123</ServiceReference>
  <Version>1.0</Version>
  <ContentType>json</ContentType>
</GBCustomsRequest>";
			AssertXmlEquals(
				expectedXml,
				serializedCustomsRequest);
		}

		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestSerialize()
		{
			GVMSMessageSenderTestHelper.TestProcess(null, manifest =>
			{
				AssertSerializedGBCustomsRequest(CreateRequest(manifest, "NEW"), "Create");
				AssertSerializedGBCustomsRequest(CreateRequest(manifest, "AMD"), "Update");
				AssertSerializedGBCustomsRequest(CreateRequest(manifest, "CAN"), "Delete");
				AssertSerializedGBCustomsRequest(CreateRequest(manifest, "FIN"), "Finalise");
			});
		}

		static void AssertNewGBCustomsRequest(GBCustomsRequest request, ServiceType serviceType, ProviderType providerType, ZString jobNumber, Credentials credentials, ZString serviceReference)
		{
			AssertEquals("Service", serviceType, request.Service);
			AssertEquals("Provider", providerType, request.Provider);
			AssertEquals("JobNumber", jobNumber, request.JobNumber);
			AssertEquals("Credentials.Key", credentials.Key, request.Credentials.Key);
			AssertEquals("ServiceReference", serviceReference, request.ServiceReference);
			AssertEquals("ContentType", "json", request.ContentType);
			AssertEquals("Version", "1.0", request.Version);
		}

		public void TestServiceType()
		{
			CombineAssertions(() =>
			{
				AssertServiceType<EDIMessage>("NEW", ProviderType.GVMS, ServiceType.Create);
				AssertServiceType<EDIMessage>("AMD", ProviderType.GVMS, ServiceType.Update);
				AssertServiceType<EDIMessage>("CAN", ProviderType.GVMS, ServiceType.Delete);
				AssertServiceType<EDIMessage>("FIN", ProviderType.GVMS, ServiceType.Finalise);
			});
		}

		void AssertServiceType<T>(ZString subType, ProviderType provider, ServiceType expectedServiceType) where T : EDIMessage
		{
			var msg = (EDIMessage)Factory.New(typeof(T));
			msg.EM_MessageSubType = subType;
			var actual = GBCustomsRequest.GetServiceType(msg);

			AssertEquals(System.FormattableString.Invariant($"Message Type: {typeof(T).Name} Provider: {provider}"), expectedServiceType, actual);
		}

		static void AssertXmlEquals(string expectedXml, string actualXml)
		{
			var expected = NormalizeNamespaces(XElement.Parse(expectedXml));
			var actual = NormalizeNamespaces(XElement.Parse(actualXml));

			if (!XNode.DeepEquals(expected, actual))
			{
				throw new Exception("XML content does not match.\n\nExpected:\n" + expected + "\n\nActual:\n" + actual);
			}

			AssertEquals(
				"XML content does not match.\n\nExpected:\n" + expected + "\n\nActual:\n" + actual,
				expected: true,
				actual: XNode.DeepEquals(expected, actual));
		}

		static XElement NormalizeNamespaces(XElement element, XNamespace defaultNamespace = null)
		{
			var currentNs = element.Name.Namespace;
			var effectiveNs = currentNs != XNamespace.None ? currentNs : defaultNamespace ?? XNamespace.None;

			return new XElement(
				effectiveNs + element.Name.LocalName,
				element.Attributes().Where(a => !a.IsNamespaceDeclaration),
				element.Nodes().Select(n =>
				{
					if (n is XElement child)
					{
						return NormalizeNamespaces(child, effectiveNs);
					}
					else if (n is XText text)
					{
						return new XText(text.Value.Trim());
					}
					else
					{
						return n;
					}
				})
			);
		}
	}
}
