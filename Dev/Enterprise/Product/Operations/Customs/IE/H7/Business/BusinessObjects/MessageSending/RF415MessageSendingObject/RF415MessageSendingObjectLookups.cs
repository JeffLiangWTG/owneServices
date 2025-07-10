using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415MessageSendingObjectLookups : ZLookups
	{
		public RF415MessageSendingObjectLookups(RF415MessageSendingObject parent)
			: base(parent)
		{
		}

		public ICollection CustomsOfficesList => Parent.Bill.Header.Lookups.CustomsOffices;

		public CodeDescriptionPairList LegalBasisList => RefCusCodeListTypes.GetCachedList(Factory, Parent.DataGrouping, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.LegalBasisCode, ZDateTime.Today);

		public CodeDescriptionPairList RefundTypeList => GetRefundTypeList();

		protected virtual CodeDescriptionPairList GetRefundTypeList() => new RF415RefundTypes();

		protected new RF415MessageSendingObject Parent => (RF415MessageSendingObject)base.Parent;
	}
}
