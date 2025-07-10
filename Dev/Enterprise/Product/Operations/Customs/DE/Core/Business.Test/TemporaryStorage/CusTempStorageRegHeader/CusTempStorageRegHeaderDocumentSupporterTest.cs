using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegHeaderDocumentSupporter))]
	sealed class CusTempStorageRegHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestShowReasonForNotPrinting()
		{
			AssertEquals(false, supporter.ShowReasonForNotPrinting(DataContext.Dummy, null));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<CusTempStorageRegHeader>();

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			supporter = new CusTempStorageRegHeaderDocumentSupporter(header);
		}
		CusTempStorageRegHeader header;
		CusTempStorageRegHeaderDocumentSupporter supporter;
	}
}
