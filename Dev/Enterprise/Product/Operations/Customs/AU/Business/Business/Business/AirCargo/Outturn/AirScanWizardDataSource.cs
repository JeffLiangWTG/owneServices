using System;
using System.Linq;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirScanWizardDataSource : ScanWizardDataSource
	{
		public AirScanWizardDataSource(ScanCusMAWB scanCusMAWB)
			: base(scanCusMAWB)
		{
		}

		public void RefreshUnderbondStatuses()
		{
			if (UnderbondSelectorLineCollection != null)
			{
				UnderbondSelectorLineCollection.Cast<UnderbondSelectorLine>().ToList().ForEach(l => l.UpdateStatuses());
			}
		}

		public void RefreshShipmentStatuses()
		{
			if (ShipmentSelectorLineCollection != null)
			{
				ShipmentSelectorLineCollection.Cast<AirShipmentSelectorLine>().ToList().ForEach(l => l.UpdateStatuses(HostBO.SelectedUnderbond));
			}
		}

		public void ClearShipmentSelection()
		{
			ShipmentSelectorLineCollection.Cast<AirShipmentSelectorLine>().ToList().ForEach(l => l.IncludeInScan = false);
		}

		protected override UnderbondSelectorLine GetUnderbondSelectorLine(CusUnderbond underbond)
		{
			return new AirUnderbondSelectorLine(underbond);
		}

		protected override ShipmentSelectorLineCollection CreateShipmentSelectorCollection()
		{
			if (HostBO.IsStandAlone)
			{
				return null;
			}

			var result = new ShipmentSelectorLineCollection(HostBO.Factory);

			var standardShipmentSelector = new AirShipmentSelectorLine();
			standardShipmentSelector.CreateStandardLine();

			if (HostBO.GetChildBills() == null)
			{
				throw new InvalidOperationException("GetChildBills is null");
			}

			foreach (IScanHouseBillProvider houseBill in HostBO.GetChildBills())
			{
				if (houseBill.ShipmentType == Core.Constants.ShipmentTypes.StandardHouse)
				{
					standardShipmentSelector.AddStandardHouseBill(houseBill);
				}
				else
				{
					var shipmentSelector = new AirShipmentSelectorLine();
					shipmentSelector.CreateStandAloneLine(houseBill);
					shipmentSelector.UpdateStatuses(HostBO.SelectedUnderbond);
					result.Add(shipmentSelector);
				}
			}

			if (standardShipmentSelector.HouseBills.Any())
			{
				standardShipmentSelector.UpdateStatuses(HostBO.SelectedUnderbond);
				result.Add(standardShipmentSelector);
			}
			return result;
		}
	}
}
