using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEAirCargoMasterForm : AirCargoMasterForm
	{
		public UPEAirCargoMasterForm(CusMAWB businessEntity)
			: base(businessEntity)
		{
		}

		protected override BaseACAStandAloneUserControl NewACAStandAloneUserControl()
		{
			BaseACAStandAloneUserControl result = new UPECMRACAStandAloneUserControl();

			ZCheckBoxColumnStyleInfo requiresConsigneeMatchColumn = new ZCheckBoxColumnStyleInfo();
			requiresConsigneeMatchColumn.ColumnName = UPECusHAWB.Schema.RequiresConsigneeMatch;
			requiresConsigneeMatchColumn.Caption = "Req. Consignee Match";
			requiresConsigneeMatchColumn.IsVisible = false;
			result.HouseBillsModuleButtonGrid.ColumnStyles.Add(requiresConsigneeMatchColumn);

			ZCheckBoxColumnStyleInfo requiresConsignorMatchColumn = new ZCheckBoxColumnStyleInfo();
			requiresConsignorMatchColumn.ColumnName = UPECusHAWB.Schema.RequiresConsignorMatch;
			requiresConsignorMatchColumn.Caption = "Req. Consignor Match";
			requiresConsignorMatchColumn.IsVisible = false;
			result.HouseBillsModuleButtonGrid.ColumnStyles.Add(requiresConsignorMatchColumn);

			return result;
		}

		public override bool IsResizableByTabPageAllowed => true;
	}
}
