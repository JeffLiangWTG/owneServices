using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class APPaymentApprovalWithAuthorisationCollection : PaymentApprovalBaseCollection
	{
		public APPaymentApprovalWithAuthorisationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new APPaymentApprovalWithAuthorisation this[int index]
		{
			get { return (APPaymentApprovalWithAuthorisation)Elements[index]; }
		}

		public new APPaymentApprovalWithAuthorisation AddNew()
		{
			return (APPaymentApprovalWithAuthorisation)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(APPaymentApprovalWithAuthorisation);
		}
	}
}
