using System.Text;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderDocumentSupporter))]
	sealed class AsycudaManifestHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProviders()
		{
			var providers = ManifestHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(MessageDocumentSupporter.MismatchInformationDocument), null);
			AssertEquals("Provider for Mismatch Information", 2, providers.Length);
		}

		public void TestMismatchInformationIsSupported()
		{
			AssertEquals(true, ManifestHeader.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.MismatchInformationDocument)));
		}

		public void TestGetFilterValue()
		{
			AssertEquals("Y", ManifestHeader.DocumentSupporter.GetFilterValue(DocumentFilters.IsJPDiscrepancyNoticeDocumentSupport));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var mismatchInformationMessageData = Encoding.ASCII.GetBytes(TestDataHelper.GetResourceStream("MismatchInformationMessage.txt"));
			var mismatchInformationMessage1 = header.Messages.AddNew();
			mismatchInformationMessage1.EM_ReceiveTransmit = "RCV";
			mismatchInformationMessage1.EM_MessageData = mismatchInformationMessageData;
			var mismatchInformationMessage2 = header.Messages.AddNew();
			mismatchInformationMessage2.EM_ReceiveTransmit = "RCV";
			mismatchInformationMessage2.EM_MessageData = mismatchInformationMessageData;
			return header;
		}

		AsycudaManifestHeader ManifestHeader => manifestHeader ??= (AsycudaManifestHeader)GetDocumentSupportableBusinessObject();
		AsycudaManifestHeader manifestHeader;
	}
}
