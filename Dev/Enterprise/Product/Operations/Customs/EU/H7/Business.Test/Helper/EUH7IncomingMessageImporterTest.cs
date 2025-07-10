using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class EUH7IncomingMessageImporterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportEUH7IncomingMessageFromXmlFile()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.SuspendCheckBusinessObjectType();
			manifestHeader.AMA_RN_NKCountry = "IE";
			manifestHeader.AMA_ApplicationCode = "LV1";
			var applicationBusinessProvider = manifestHeader.ApplicationBusinessProvider;
			AssertEquals("Precondition: AsycudaManifestHeader country IE has InboundEDIMessageApplicationCode IEI", "IEI", applicationBusinessProvider.InboundEDIMessageApplicationCode);

			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\EU\H7\Business.Test\TestFile\M416.xml"));
			EUH7IncomingMessageImporter.Import(fileStream, manifestHeader);

			var message = manifestHeader.Messages.Cast<EDIMessage>().Single();
			CombineAssertions("Message details", () => {
				AssertNotNull(message);
				AssertEquals("RCV", message.EM_ReceiveTransmit);
				AssertEquals("QUE", message.EM_Status);
				AssertEquals("416", message.EM_MessageType);
				AssertEquals(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IM416 xmlns=""http://www.ros.ie/schemas/customs/IM416H7"">
  <Declaration>
    <LRN>LRN1</LRN>
  </Declaration>
</IM416>", message.EM_MessageText);
			});
			AssertEquals("Application Code is populated from ApplicationBusinessProvider.InboundEDIMessageApplicationCode", "IEI", message.EM_ApplicationCode);
		}
	}
}
