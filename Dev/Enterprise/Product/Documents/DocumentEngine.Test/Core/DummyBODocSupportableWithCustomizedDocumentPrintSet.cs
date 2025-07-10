using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.Testing
{
	public sealed class DummyBODocSupportableWithCustomizedDocumentPrintSet : DummyBODocSupportable
	{
		public DummyBODocSupportableWithCustomizedDocumentPrintSet(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
		}

		public override DocumentSupporter DocumentSupporter
		{
			get
			{
				return documentSupporter ?? (documentSupporter = new DummyBODocSupportableDocumentSupporterWithCustomizedDocumentPrintSet(this));
			}
		}
		DocumentSupporter documentSupporter;
	}
}
