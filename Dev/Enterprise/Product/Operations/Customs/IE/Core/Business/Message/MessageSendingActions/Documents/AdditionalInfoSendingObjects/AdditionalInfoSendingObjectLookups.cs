using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business
{
	public class AdditionalInfoSendingObjectLookups : ZLookups
	{
		public AdditionalInfoSendingObjectLookups(AdditionalInfoSendingObject parent) : base(parent) { }

		new AdditionalInfoSendingObject Parent => (AdditionalInfoSendingObject)base.Parent;

		public ICollection DocumentTypeList
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Parent.DataGroupgingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Now);
			}
		}

		public ICollection CL010CountryCodes => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDateTime.Now);
	}
}
