using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ScanWizardDataSource : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected ScanWizardDataSource(ScanMasterBill hostBO)
		{
			this.hostBO = Argument.NotNull(hostBO, "ScanMasterBill");
		}
		readonly ScanMasterBill hostBO;

		protected ScanMasterBill HostBO
		{
			get { return hostBO; }
		}

		#region ShipmentSelectorLineCollection

		public ShipmentSelectorLineCollection ShipmentSelectorLineCollection
		{
			get { return shipmentSelectorLineCollection ?? (shipmentSelectorLineCollection = CreateShipmentSelectorCollection()); }
		}
		ShipmentSelectorLineCollection shipmentSelectorLineCollection;

		protected abstract ShipmentSelectorLineCollection CreateShipmentSelectorCollection();

		#endregion

		#region UnderbondSelectorLineCollection

		public UnderbondSelectorLineCollection UnderbondSelectorLineCollection
		{
			get { return underbondSelectorLineCollection ?? (underbondSelectorLineCollection = CreateUnderbondSelectorCollection()); }
		}
		UnderbondSelectorLineCollection underbondSelectorLineCollection;

		UnderbondSelectorLineCollection CreateUnderbondSelectorCollection()
		{
			var result = new UnderbondSelectorLineCollection(HostBO.Factory);
			var underbonds = HostBO.Underbonds;
			if (underbonds != null)
			{
				foreach (var underbond in underbonds)
				{
					var underbondLine = GetUnderbondSelectorLine(underbond);
					underbondLine.UpdateStatuses();
					result.Add(underbondLine);
				}
			}
			return result;
		}

		protected abstract UnderbondSelectorLine GetUnderbondSelectorLine(CusUnderbond underbond);

		#endregion

		public SurplusOutturnLineCollection SurplusOutturnCollection
		{
			get
			{
				if (surplusOurturnLineCollection == null)
				{
					surplusOurturnLineCollection = new SurplusOutturnLineCollection(Factory);
				}
				return surplusOurturnLineCollection;
			}
		}
		SurplusOutturnLineCollection surplusOurturnLineCollection;

		public bool IsShipmentSelected
		{
			get
			{
				if (ShipmentSelectorLineCollection == null)
				{
					return false;
				}

				foreach (ShipmentSelectorLine shipment in ShipmentSelectorLineCollection)
				{
					if (shipment.IncludeInScan)
					{
						return true;
					}
				}

				return false;
			}
		}
	}
}
