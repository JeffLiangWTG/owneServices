using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummySupportable : DummyBusinessObject, IDocumentSupportable
	{
		public DummySupportable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public DocumentSupporter DocumentSupporter
		{
			get { return new DummySupporter(this); }
		}

		public OrgHeader Client { get; set; }
	}
}
