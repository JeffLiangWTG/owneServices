using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.H7.Business.BusinessObjects.MessageSending
{
	public class AdditionalInfoSendingObjectLookups : ZLookups
	{
		public AdditionalInfoSendingObjectLookups(AdditionalInfoSendingObject parent) : base(parent) { }

		new AdditionalInfoSendingObject Parent => (AdditionalInfoSendingObject)base.Parent;

		public ICollection DocumentTypeList
		{
			get
			{
				var dataGroupingCode = ((IDataGroupingProvider)Parent)?.DataGrouping ?? ZString.Empty;
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Now);
			}
		}
	}
}
