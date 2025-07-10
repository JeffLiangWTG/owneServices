using System.Linq;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaScanWizardDataSource : ScanWizardDataSource
	{
		public SeaScanWizardDataSource(ScanCusSCAOceanBill hostBO)
			: base(hostBO)
		{ }

		public new ScanCusSCAOceanBill HostBO
		{
			get { return (ScanCusSCAOceanBill)base.HostBO; }
		}

		protected override ShipmentSelectorLineCollection CreateShipmentSelectorCollection()
		{
			ShipmentSelectorLineCollection result = null;
			if (!HostBO.IsStandAlone)
			{
				SeaShipmentSelectorLine shipmentSelector;
				result = new ShipmentSelectorLineCollection(Factory);

				var allHouseBills = HostBO.GetChildBills();
				var hLSHOuseBills = allHouseBills.Where(x => x.ShipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy).ToList();

				var selectedUnderbond = HostBO.SelectedUnderbond;

				foreach (IScanMasterBillProvider standAloneOceanBill in HostBO.MasterBill.GetMatchingOceanBillsForContainers(true, new[] { selectedUnderbond.ContainerNumber }))
				{
					var hLSHouseBill = hLSHOuseBills.FirstOrDefault(x => x.HouseBill == standAloneOceanBill.MasterHouseBill);
					shipmentSelector = new SeaShipmentSelectorLine((CusSCAOceanBill)standAloneOceanBill);
					if (hLSHouseBill != null)
					{
						shipmentSelector.SetShipmentAdditionalInformation(hLSHouseBill.ConsigneeName, hLSHouseBill.ConsignorName, hLSHouseBill.GoodsDescription);
					}
					foreach (var houseBill in standAloneOceanBill.GetChildBills(selectedUnderbond))
					{
						shipmentSelector.AddStandardHouseBill(houseBill);
					}
					if (shipmentSelector.HouseBills.Any())
					{
						result.Add(shipmentSelector);
					}
				}
				shipmentSelector = new SeaShipmentSelectorLine(HostBO.MasterBill);
				foreach (var houseBill in allHouseBills)
				{
					if (houseBill.ShipmentType == Core.Constants.ShipmentTypes.StandardHouse)
					{
						shipmentSelector.AddStandardHouseBill(houseBill);
					}
				}
				if (shipmentSelector.HouseBills.Any())
				{
					result.Add(shipmentSelector);
				}
			}
			return result;
		}

		protected override UnderbondSelectorLine GetUnderbondSelectorLine(CusUnderbond underbond)
		{
			return new SeaUnderbondSelectorLine(underbond);
		}

		public CusSCAOceanBill GetOceanBillToAddSurplusConsignment()
		{
			if (HostBO.IsStandAlone)
			{
				return HostBO.MasterBill;
			}
			else
			{
				return HostBO.MasterBill.GetMatchingOceanBillsForContainers(true, new[] { HostBO.SelectedUnderbond.ContainerNumber }).FirstOrDefault();
			}
		}
	}
}
