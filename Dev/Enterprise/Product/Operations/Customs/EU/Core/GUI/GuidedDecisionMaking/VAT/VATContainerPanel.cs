using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class VATContainerPanel : ZPanel
	{
		public VATContainerPanel()
		{
			InitializeComponent();
		}

		public void PopulateVATControl(GuidedDecisionMakingBasic guidedDecisionMakingBasic)
		{
			if (guidedDecisionMakingBasic != null)
			{
				try
				{
					this.SuspendDrawing();

					var table = GetEmptyLayoutPanel();
					VATControl vatControl = null;
					var vatApplicabilities = guidedDecisionMakingBasic.VATApplicabilities.Cast<GuidedDecisionMakingVAT>();
					if (!InSummary)
					{
						vatControl = new VATControl(vatApplicabilities, ZString.Empty);
						table.Controls.Add(vatControl);
					}
					else
					{
						var tickedVATApplicabilities = guidedDecisionMakingBasic.VATApplicabilities.Where(v => v.IsTicked);
						if (tickedVATApplicabilities.Any())
						{
							vatControl = new VATControl(tickedVATApplicabilities, (NoResString)vatControlGroupBoxCaption, InSummary);
							table.Controls.Add(vatControl);
						}
					}
				}
				finally
				{
					this.ResumeDrawing();
				}
			}
		}

		public bool InSummary { get; set; }

		const string flowLayoutPanelName = "VATContainerPanel_FlowLayoutPanel";

		const string vatControlGroupBoxCaption = "VAT";

		FlowLayoutPanel GetEmptyLayoutPanel()
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
				};
				Controls.Add(table);
			}
			table.Controls.RemoveAndDisposeAll();
			table.Size = ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			return table;
		}
	}
}
