using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromPaymentBatch : FreightWrapper
	{
		public FreightWrapperFromPaymentBatch(AccPaymentBatch businessObjectToWrap, BusinessObjectFactory factory)
			: base(businessObjectToWrap, factory)
		{
			businessObject = businessObjectToWrap;
		}

		readonly AccPaymentBatch businessObject;

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return businessObject.PK;
		}
	}
}
