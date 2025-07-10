using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DummyDocumentWrapperWithDocTypeCode : DummyDocumentWrapper, IDocTypeCode
	{
		public DummyDocumentWrapperWithDocTypeCode(BusinessObject bizToWrap, BusinessObjectFactory factoryToWrap)
			: base(bizToWrap, factoryToWrap)
		{
		}

		public ZString DocTypeCode { get; set; }
	}
}
