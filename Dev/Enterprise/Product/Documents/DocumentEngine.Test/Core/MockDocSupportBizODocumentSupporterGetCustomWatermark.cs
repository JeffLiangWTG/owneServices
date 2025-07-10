using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class MockDocSupportBizODocumentSupporterGetCustomWatermark : MockDocSupportBizODocumentSupporter
	{
		public MockDocSupportBizODocumentSupporterGetCustomWatermark(BusinessObject businessObject) : base(businessObject)
		{
		}

		public MockDocSupportBizODocumentSupporterGetCustomWatermark(MockDocSupportBizO bizO, DocumentWrapper[] wrappers, short copyCount) : base(bizO, wrappers, copyCount)
		{
		}

		public override MultilingualString GetCustomWatermarkText(IDocumentCommand documentCommand, IBODocDataProvider docDataProvider) => (NoResString)$"UT-{documentCommand?.SU_MenuName}";
	}
}
