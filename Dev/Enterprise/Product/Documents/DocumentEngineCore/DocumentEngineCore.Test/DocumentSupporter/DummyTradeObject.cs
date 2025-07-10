using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DummyTradeObject : DummyBusinessObject, IDocumentSupportable
	{
		public DummyTradeObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return new DummyTradeObjectDocumentSupporter(this); }
		}
	}
}
