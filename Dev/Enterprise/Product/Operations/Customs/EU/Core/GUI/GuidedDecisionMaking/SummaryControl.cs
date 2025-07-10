using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class SummaryControl : ZUserControl
	{
		public SummaryControl()
		{
			InitializeComponent();
			gDMBasicLayoutPanel.UpdateLayout(GetGDMBasicLayout());
			this.SetReadOnlyIncludingChildren(true);

			meursingCalculateResultPanel.AllowOverlap(gDMBasicLayoutPanel);
		}

		public IPanelLayoutProvider GetGDMBasicLayout() => gDMBasicLayout ?? (gDMBasicLayout = GetGDMBasicLayoutCore());

		protected virtual IPanelLayoutProvider GetGDMBasicLayoutCore() => new GDMBasicLayout();
		IPanelLayoutProvider gDMBasicLayout;

		public void PopulateSummaryControls(GuidedDecisionMakingBasic guidedDecisionMakingBasic)
		{
			PopulateMeursingCalculateResult(guidedDecisionMakingBasic);
			additionalCodesContainerPanel.PopulateAdditionalCodeControls(guidedDecisionMakingBasic);
			additionalCodesContainerPanel.Visible = guidedDecisionMakingBasic.AdditionalCodes.Cast<GuidedDecisionMakingAdditionalCode>().Any(c => c.IsTicked);

			documentConditionsContainerPanel.PopulateDocumentConditionControls(guidedDecisionMakingBasic);
			documentConditionsContainerPanel.Visible = guidedDecisionMakingBasic.SelectedDocuments.Any();

			vatContainerPanel.PopulateVATControl(guidedDecisionMakingBasic);
			vatContainerPanel.Visible = guidedDecisionMakingBasic.IsVATApplicable && guidedDecisionMakingBasic.VATApplicabilities.Cast<GuidedDecisionMakingVAT>().Any(v => v.IsTicked);

			try
			{
				this.SuspendDrawing();
				var table = GetLayoutPanel();
				table.Controls.Add(additionalCodesContainerPanel);
				table.Controls.Add(documentConditionsContainerPanel);
				table.Controls.Add(vatContainerPanel);
			}
			finally
			{
				this.ResumeDrawing();
			}
		}

		void PopulateMeursingCalculateResult(GuidedDecisionMakingBasic guidedDecisionMakingBasic)
		{
			this.meursingCalculateResultPanel.Controls.Clear();
			if (guidedDecisionMakingBasic.IsMeursingApplicable)
			{
				ControlDpiScalingHelper.SetHeight(this.meursingCalculateResultPanel, 30, true);
				var meursingResultDropEdit = new MeursingResultUserControl();
				this.meursingCalculateResultPanel.Controls.Add(meursingResultDropEdit);
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(this.meursingCalculateResultPanel, 0, false);
			}
		}

		const string flowLayoutPanelName = "SummaryControl_FlowLayoutPanel";

		FlowLayoutPanel GetLayoutPanel()
		{
			var table = this.FindSingleOrDefault<FlowLayoutPanel>(x => x.Name == flowLayoutPanelName);
			if (table is null)
			{
				table = new KFlowLayoutPanel
				{
					Name = flowLayoutPanelName,
					FlowDirection = FlowDirection.TopDown,
					WrapContents = false,
					Margin = Padding.Empty,
					Padding = Padding.Empty,
					AutoSize = true,
					Location = ControlDpiScalingHelper.NewScaledPoint(0, 270, true),
				};
				Controls.Add(table);
			}
			return table;
		}
	}
}
