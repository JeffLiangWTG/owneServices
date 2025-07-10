using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class GuaranteesUserControl : EU.GUI.GuaranteesUserControl
	{
		public GuaranteesUserControl()
		{
			ModifyColumns();
		}

		void ModifyColumns()
		{
			GuaranteesGrid.ColumnStyles.Remove(GuaranteesGrid.GetColumnStyle(EU.Business.Declaration.GuaranteeForDeclaration.Schema.EntryInstructionID));
		}

		protected override void GridColumnStyleDecider()
		{
			GuaranteesGrid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_BondAmount).IsUnavailable = false;
			GuaranteesGrid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_BondAmount).Width = 140;
			GuaranteesGrid.SetColumnCaption(Business.NctsGuarantee.Schema.PW_BondAmount, Res.GetString("A749AF3A-2DE5-4604-BDBE-9491115BB082", "Liability Amount"));
			GuaranteesGrid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_RX_NKCurrency).IsUnavailable = true;
			GuaranteesGrid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_ValidityLimitation).IsUnavailable = true;
			GuaranteesGrid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_BondFiledPort).IsUnavailable = true;
			GuaranteesGrid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_HolderIdentification).IsUnavailable = true;
			var passwordStyle = (ZArchitecture.ZTextBoxColumnStyleInfo)GuaranteesGrid.GetColumnStyle(Business.NctsGuarantee.Schema.PW_Password);
			passwordStyle.IsUnavailable = false;
			passwordStyle.CaptionResourceString = Res.GetData("971A0E22-761D-4E7F-86A6-24211C6480CD", "PIN", "Access Code", "PIN, Password or Access Code");
			passwordStyle.PasswordChar = '*';
		}

		protected override void AddAndRemoveColumns()
		{
			base.AddAndRemoveColumns();

			var liabilityFractionColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			liabilityFractionColumnStyleInfo.ColumnName = Business.NctsGuarantee.Schema.PW_SuretyCode;
			liabilityFractionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			GuaranteesGrid.ColumnStyles.Add(liabilityFractionColumnStyleInfo);

			var liabilityAmountOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			liabilityAmountOverrideColumnStyleInfo.ColumnName = Business.NctsGuarantee.Schema.PW_Override;
			liabilityAmountOverrideColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			liabilityAmountOverrideColumnStyleInfo.IsUnavailable = true;
			GuaranteesGrid.ColumnStyles.Add(liabilityAmountOverrideColumnStyleInfo);
		}

		protected override void OnAfterFirstBinding(System.EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetOverrideColumnAvailabilityBasedOnConfiguration();
		}

		void SetOverrideColumnAvailabilityBasedOnConfiguration()
		{
			GuaranteesGrid.SetAvailability(NctsHeader?.Configuration.GuaranteeConfiguration.OverrideSupport(NctsHeader) ?? ZBool.False, Business.NctsGuarantee.Schema.PW_Override);
		}

		NctsHeader NctsHeader => DataSource as NctsHeader;
	}
}
