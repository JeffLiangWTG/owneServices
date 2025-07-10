using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public class GBCustomsRequestTests : TestCaseWithFactory
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestNew()
		{
			ICSMessageSenderTestHelper.TestProcess(null, manifest =>
			{
				var providerType = ProviderType.ICSGB;
				var credentials = new Credentials
				{
					Key = "HYECMT.GB999999999888.111"
				};

				var messages = manifest.Messages.OfType<EDIMessage>();

				AssertNewGBCustomsRequest(ICSExtensions.NewGBCustomsRequest(messages.Single(x => x.EM_MessageSubType == "NEW"))
					, ServiceType.Create, providerType, "MAN001", credentials, "123");

				AssertNewGBCustomsRequest(ICSExtensions.NewGBCustomsRequest(messages.Single(x => x.EM_MessageSubType == "AMD"))
					, ServiceType.Update, providerType, "MAN001", credentials, "123");

				AssertNewGBCustomsRequest(ICSExtensions.NewGBCustomsRequest(messages.Single(x => x.EM_MessageSubType == "CAN"))
					, ServiceType.Delete, providerType, "MAN001", credentials, null);
			});
		}

		public void TestNewICSNI()
		{
			RunTest<AsycudaManifestHeader>(ProviderType.ICSNI, EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland);
		}

		public void TestNewICSGB()
		{
			RunTest<AsycudaManifestHeaderSS>(ProviderType.ICSGB, EDIMessage.ApplicationCodes.GbMessageICSGreatBritain);
		}

		static void AssertNewGBCustomsRequest(GBCustomsRequest request, ServiceType serviceType, ProviderType providerType, ZString jobNumber, Credentials credentials, ZString? serviceReference)
		{
			AssertEquals("Service", serviceType, request.Service);
			AssertEquals("Provider", providerType, request.Provider);
			AssertEquals("JobNumber", jobNumber, request.JobNumber);
			AssertEquals("Credentials.Key", credentials.Key, request.Credentials.Key);
			AssertEquals("ServiceReference", serviceReference, request.ServiceReference);
			AssertEquals("ContentType", "XML", request.ContentType);
			AssertEquals("Version", "1.0", request.Version);
		}

		void RunTest<T>(ProviderType expectedProviderType, ZString sourceMessageAppCode)
			where T : AsycudaManifestHeaderBase
		{
			var manifestHeader = Factory.NewWithValidTestData<T>();
			manifestHeader.AMA_JobReference = "MAN001";
			manifestHeader.RegistrationNumber = "123";
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = sourceMessageAppCode;
			message.EM_LinkedObject = manifestHeader;
			message.EM_MessageSubType = Constants.ICSMessageSubTypes.NEW;
			var request = ICSExtensions.NewGBCustomsRequest(message);
			AssertEquals("Should create GBCustomsRequest with Provider=ICSNI for GIN message", expectedProviderType, request.Provider);
		}
	}
}
