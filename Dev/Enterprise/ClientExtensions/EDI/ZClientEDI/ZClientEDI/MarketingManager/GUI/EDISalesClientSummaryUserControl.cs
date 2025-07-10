using Enterprise.MasterFiles.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class EDISalesClientSummaryUserControl : SalesClientSummaryUserControl
	{
		#region Construction

		public static new EDISalesClientSummaryUserControl New()
		{
			return new EDISalesClientSummaryUserControl();
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public EDISalesClientSummaryUserControl()
			: base()
		{
			CustomisedLabels();
		}

		void CustomisedLabels()
		{
#pragma warning disable RST001 // Suppress to make the ResourceString source generator work
			this.OM_CMAcheivableClientRevenueBoundCalcEdit.CaptionResourceString = Res.GetData("CB879A14-FAEF-4F03-8776-A7B25B943D40", EDIDataRegistry.Instance.AchievableBusinessLabel.Value.ToString());
			this.OM_CMNoOfEmployeesCalcEdit.CaptionResourceString = Res.GetData("611D4563-C3AC-4A10-A1B7-6B04B3426F44", EDIDataRegistry.Instance.NumberOfEmployeesLabel.Value.ToString());
			this.OM_CMAmountOfBusinessWonBoundCalcEdit.CaptionResourceString = Res.GetData("4715A9B7-E265-4E3A-BBF9-59AD9202976D", EDIDataRegistry.Instance.AmountOfBusinessWonLabel.Value.ToString());
			this.OM_CMTotalClientRevenueBoundCalcEdit.CaptionResourceString = Res.GetData("1D3B6DCA-961A-43A3-AD86-3DDF17EE5708", EDIDataRegistry.Instance.TotalClientRevenueLabel.Value.ToString());
			this.OM_CMWarehouseRevenueBoundCalcEdit.CaptionResourceString = Res.GetData("BC509EF9-C9B5-40BA-A538-6B9B4170C844", EDIDataRegistry.Instance.WarehouseRevenueLabel.Value.ToString());
			this.OM_CMConsultingRevenueCalcEdit.CaptionResourceString = Res.GetData("C85ADD7C-2430-459C-BB92-6F3733DEC648", EDIDataRegistry.Instance.ConsultingRevenueLabel.Value.ToString());
			this.OM_CMPaidUpCapitalCalcEdit.CaptionResourceString = Res.GetData("3274C359-E0B0-4FFB-A3F1-CC33FC8D528A", EDIDataRegistry.Instance.PaidUpCapitalLabel.Value.ToString());
#pragma warning restore RST001
		}
	}
}
