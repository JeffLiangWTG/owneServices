using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class AdditionalCodesContainerPanel : ZPanel
	{
		public AdditionalCodesContainerPanel()
		{
			InitializeComponent();
		}

		public void PopulateAdditionalCodeControls(GuidedDecisionMakingBasic guidedDecisionMakingBasic)
		{
			if (guidedDecisionMakingBasic != null)
			{
				try
				{
					this.SuspendDrawing();

					var table = GetEmptyLayoutPanel();
					var additionalCodeControls = new List<AdditionalCodesControl>();
					if (guidedDecisionMakingBasic.AdditionalCodes.Any())
					{
						var groupedAdditionalCodes = guidedDecisionMakingBasic.GroupedAdditionalCodes;

						if (!InSummary)
						{
							foreach (var additionalCodeGroup in groupedAdditionalCodes)
							{
								var additionalCodesControl = new AdditionalCodesControl(additionalCodeGroup.Key, additionalCodeGroup.ToList(), InSummary);
								additionalCodeControls.Add(additionalCodesControl);
								table.Controls.Add(additionalCodesControl);
							}
						}
						else
						{
							var tickedAdditionalCodes = guidedDecisionMakingBasic.GroupedAdditionalCodes.SelectMany(g => g.Where(a => a.IsTicked)).ToList();
							var additionalCodesControl = new AdditionalCodesControl(Res.GetString("0585e3d2-5ce5-4b8a-b3e9-02145549c31d", "Additional Codes"), tickedAdditionalCodes, InSummary);
							additionalCodeControls.Add(additionalCodesControl);
							table.Controls.Add(additionalCodesControl);
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

		const string flowLayoutPanelName = "AdditionalCodesContainerPanel_FlowLayoutPanel";

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
