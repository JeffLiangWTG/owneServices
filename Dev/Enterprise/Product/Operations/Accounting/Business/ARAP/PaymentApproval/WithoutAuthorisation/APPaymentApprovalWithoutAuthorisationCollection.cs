using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class APPaymentApprovalWithoutAuthorisationCollection : PaymentApprovalBaseCollection
	{
		public APPaymentApprovalWithoutAuthorisationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new APPaymentApprovalWithoutAuthorisation this[int index]
		{
			get { return (APPaymentApprovalWithoutAuthorisation)Elements[index]; }
		}

		public new APPaymentApprovalWithoutAuthorisation AddNew()
		{
			return (APPaymentApprovalWithoutAuthorisation)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(APPaymentApprovalWithoutAuthorisation);
		}
	}
}
