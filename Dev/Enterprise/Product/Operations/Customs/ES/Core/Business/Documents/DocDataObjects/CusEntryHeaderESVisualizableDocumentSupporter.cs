using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects
{
	public sealed class CusEntryHeaderESVisualizableDocumentSupporter : CusEntryHeaderEUVisualizableDocumentSupporter
	{
		public CusEntryHeaderESVisualizableDocumentSupporter(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override string GetDocProviderKey()
		{
			return Core.Constants.CountryCodes.Spain;
		}
	}
}
