using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ESGuaranteesUserControl : GuaranteesUserControl
	{
		public ESGuaranteesUserControl()
		{
		}

		protected new JobDeclaration Declaration => CurrentDataItem != null && CurrentDataItem is JobDeclaration declaration ? declaration : null;

		protected override void GridColumnStyleDecider()
		{
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondAmount).IsVisible = false;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_RX_NKCurrency).IsVisible = false;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondFiledPort).IsVisible = false;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondNumber2).IsVisible = false;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_ValidityLimitation).IsVisible = false;

			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondType).IsMandatory = false;
		}

		protected override void AddAndRemoveColumns()
		{
			var entryInstructionDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo("EntryInstruction+CEI_Description", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80));
			entryInstructionDescriptionColumnStyleInfo.CaptionResourceString = Res.GetData("4963EA51-1901-43D4-85AD-559A62CA6420", "Entry Instruction Desc.");
			GuaranteesGrid.ColumnStyles.Add(entryInstructionDescriptionColumnStyleInfo);
		}
	}
}
