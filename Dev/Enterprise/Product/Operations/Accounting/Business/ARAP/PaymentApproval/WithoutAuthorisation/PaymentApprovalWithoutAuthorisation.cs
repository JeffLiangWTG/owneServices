using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public abstract class PaymentApprovalWithoutAuthorisation : PaymentApprovalBase
	{
		public PaymentApprovalWithoutAuthorisation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal override bool IsAllowedToPost
		{
			get { return true; }
			set { }
		}
	}
}
