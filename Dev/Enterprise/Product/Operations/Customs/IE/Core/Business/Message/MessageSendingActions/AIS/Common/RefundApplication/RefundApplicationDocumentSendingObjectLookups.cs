using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationDocumentSendingObjectLookups : ZLookups
	{
		public RefundApplicationDocumentSendingObjectLookups(RefundApplicationDocumentSendingObject parent) : base(parent) { }

		new RefundApplicationDocumentSendingObject Parent => (RefundApplicationDocumentSendingObject)base.Parent;

		public ICollection DocumentTypeList
		{
			get
			{
				var dataGroupingCode = ((ICanBeImportOrExport)Parent.EntryHeader?.Declaration)?.DataGroupingCode ?? ZString.Empty;
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Today);
			}
		}
	}
}
