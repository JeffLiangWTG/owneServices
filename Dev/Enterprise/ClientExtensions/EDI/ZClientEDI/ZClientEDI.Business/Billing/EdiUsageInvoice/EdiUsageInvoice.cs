using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiUsageInvoice : AutoEdiUsageInvoice
	{
		public EdiUsageInvoice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var today = CargoWise.Types.ZDateTime.Today.Date;
			EUI_PeriodStart = today.AddDays(1 - today.Day);
		}
#endif
	}
}

