using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocDummyBusinessObjectWithOverrideSupporter : DocumentCommandTest.DocDummyBusinessObject
	{
		public DocDummyBusinessObjectWithOverrideSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override DocumentSupporter DocumentSupporter => new OverrideDummyBusinessObjectDocumentSupporter(this);
	}
}
