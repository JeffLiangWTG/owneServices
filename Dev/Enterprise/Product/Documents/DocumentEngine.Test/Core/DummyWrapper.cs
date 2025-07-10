using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyWrapper : DocumentWrapper
	{
		public DummyWrapper(BusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public ZString JobNumber { get; set; }
	}
}
