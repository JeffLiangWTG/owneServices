using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	public class DummyBODocSupportableWithDocTypeCode : DummyBODocSupportable
	{
		public DummyBODocSupportableWithDocTypeCode(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return new DummyDocumentSupporterWithDocTypeCode(this); }
		}
	}
}
