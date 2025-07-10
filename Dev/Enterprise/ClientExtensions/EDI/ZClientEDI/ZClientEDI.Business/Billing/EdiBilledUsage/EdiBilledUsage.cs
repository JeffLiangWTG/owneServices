using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBilledUsage : AutoEdiBilledUsage
	{
		public EdiBilledUsage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ClientLicencePriceItem PriceItem
		{
			get { return Factory.Load<ClientLicencePriceItem>(BU9_L7); }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var today = CargoWise.Types.ZDateTime.Today.Date;
			BU9_PeriodStart = today.AddDays(1 - today.Day);
		}
#endif
	}
}

