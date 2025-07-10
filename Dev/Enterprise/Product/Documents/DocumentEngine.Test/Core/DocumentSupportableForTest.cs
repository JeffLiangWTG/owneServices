using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.Public.Testing
{
	public class DocumentSupportableForTest : IDocumentSupportable
	{
		public DocumentSupportableForTest(BusinessObject bizo)
		{
			this.bizo = bizo;
		}

		public DocumentSupporter DocumentSupporter
		{
			get
			{
				return new DocumentSupporterForTest(bizo);
			}
		}

		public string TableName
		{
			get { return string.Empty; }
		}

		readonly BusinessObject bizo;
	}
}
