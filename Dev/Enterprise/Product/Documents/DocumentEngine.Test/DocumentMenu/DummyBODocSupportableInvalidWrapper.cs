using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DummyBODocSupportableInvalidWrapper : DummyBODocSupportable
	{
		public DummyBODocSupportableInvalidWrapper(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return new DummyDocumentSupporterWithInvalidWrapper(this); }
		}
	}
}
