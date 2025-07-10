using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class PreviousDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection
	{
		public PreviousDocumentCollection(BusinessObject parent) : base(parent)
		{
		}

		public new PreviousDocument this[int i] => (PreviousDocument)base[i];

		public new PreviousDocument AddNew() => (PreviousDocument)base.AddNew();
	}

	public static class PreviousDocumentCollectionExtensions
	{
		public static bool HasMRNPreviousDocuments(this PreviousDocumentCollection collection)
		{
			return collection.Cast<PreviousDocument>().Any(x => x.CSI_Code == Constants.PreviousDocumentTypeCodes.MRN);
		}
	}
}
