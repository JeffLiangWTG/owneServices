using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class MockDocSupportBizODocumentSupporterCustomWatermark : MockDocSupportBizODocumentSupporter
	{
		public MockDocSupportBizODocumentSupporterCustomWatermark(BusinessObject businessObject) : base(businessObject)
		{
		}

		public MockDocSupportBizODocumentSupporterCustomWatermark(MockDocSupportBizO bizO, DocumentWrapper[] wrappers, short copyCount) : base(bizO, wrappers, copyCount)
		{
		}

		public override MultilingualString CustomWatermarkText => (NoResString)"UT-Watermark";
	}
}
