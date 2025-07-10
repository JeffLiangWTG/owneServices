using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.H7.Business
{
	public class SupportingDocumentLookups : CusSupportingInfoLookups
	{
		public SupportingDocumentLookups(SupportingDocument parent)
			: base(parent)
		{
		}

		public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Parent.DataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Today);

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;
	}
}
