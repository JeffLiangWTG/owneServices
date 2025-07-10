using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.H7.Business
{
	public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent)
			: base(parent)
		{
		}

		public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
				Parent.Factory,
				Parent.DataGrouping,
				new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection },
				ZDateTime.Today,
				attributeFilters: null,
				includeParentDataGroupings: false);

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;
	}
}
