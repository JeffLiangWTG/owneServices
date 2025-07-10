using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromPrintSummary : FreightWrapper
	{
		public FreightWrapperFromPrintSummary(BusinessObject businessObject, BusinessObjectFactory factory)
			: base(businessObject, factory)
		{
			this.businessObject = (PrintSummary)businessObject;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return businessObject.PK;
		}

		#region Implementation

		readonly PrintSummary businessObject;

		#endregion
	}
}
