using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromPaymentApproval : FreightWrapper
	{
		public FreightWrapperFromPaymentApproval(BusinessObject businessObject, BusinessObjectFactory factory)
			: base(businessObject, factory)
		{
			this.businessObject = (PaymentApprovalBase)businessObject;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return businessObject.PK;
		}

		#region Implementation

		readonly PaymentApprovalBase businessObject;

		#endregion
	}
}
