using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public interface IMaintenancePercentages
	{
		ZDecimal OldSeatPercent { get; }
		ZDecimal NewSeatPercent { get; }
	}

	public class MaintenanceModule : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MaintenanceModule(IMaintenancePercentages percentages, ZString moduleCode, ClientLicencePriceItem priceItem)
		{
			using (SuspendSettingHasChanges())
			{
				this.moduleCode = moduleCode;
				this.priceItem = priceItem;
				this.percentages = percentages;
			}
		}

		public MaintenanceModule()
		{
		}

		readonly ZString moduleCode;
		readonly ClientLicencePriceItem priceItem;
		readonly IMaintenancePercentages percentages;

		#region Properties

		public ZInt UserCount { get; set; }
		public ZInt OldUserCount { get; set; }

		public ZDecimal UnitPrice
		{
			get { return priceItem != null ? priceItem.L7_Price : ZDecimal.Zero; }
		}

		public ZDecimal TotalPrice
		{
			get { return UnitPrice * UserCount; }
		}

		public ZString ModuleCode
		{
			get { return moduleCode; }
		}

		public ClientLicencePriceItem PriceItem
		{
			get { return priceItem; }
		}

		public ZShort Order
		{
			get { return priceItem != null ? priceItem.L7_Order : ZShort.Zero; }
		}

		public ZDecimal OldSeatMaintenance
		{
			get
			{
				return OldUserCount * UnitPrice * percentages.OldSeatPercent / 100m;
			}
		}

		public ZDecimal NewSeatMaintenance
		{
			get
			{
				return (UserCount - OldUserCount) * UnitPrice * percentages.NewSeatPercent / 100m;
			}
		}

		public ZDecimal TotalMaintenance
		{
			get { return NewSeatMaintenance + OldSeatMaintenance; }
		}

		#endregion
	}
}

