using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromPrintStatement : FreightWrapper
	{
		public FreightWrapperFromPrintStatement(BusinessObject businessObject, BusinessObjectFactory factory)
			: base(businessObject, factory)
		{
			this.businessObject = (PrintStatement)businessObject;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return businessObject.PK;
		}

		#region Implementation

		readonly PrintStatement businessObject;

		#endregion
	}
}
