using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromTransactionHeader : FreightWrapper
	{
		public FreightWrapperFromTransactionHeader(BusinessObject businessObject, BusinessObjectFactory factory)
			: base(businessObject, factory)
		{
			this.businessObject = (TransactionHeader)businessObject;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return businessObject.PK;
		}

		#region Implementation

		readonly TransactionHeader businessObject;

		#endregion
	}
}
