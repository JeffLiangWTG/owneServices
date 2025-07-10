using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class DocumentConditionsContainerPanel : ZPanel
	{
		public DocumentConditionsContainerPanel()
		{
			InitializeComponent();
		}

		public void PopulateDocumentConditionControls(GuidedDecisionMakingBasic guidedDecisionMakingBasic)
		{
			if (guidedDecisionMakingBasic != null)
			{
				try
				{
					this.SuspendDrawing();

					var table = GetEmptyLayoutPanel();
					var documentConditionControls = new List<DocumentConditionsControl>();
					var documentConditions = guidedDecisionMakingBasic.DocumentConditions.Cast<GuidedDecisionMakingCondition>();
					if (!InSummary)
					{
						foreach (var documentCondition in documentConditions)
						{
							var documentConditionsControl = new DocumentConditionsControl(documentCondition);
							documentConditionControls.Add(documentConditionsControl);
							table.Controls.Add(documentConditionsControl);
						}
					}
					else
					{
						var tickedDocumentConditionDetails = documentConditions.SelectMany(x => x.ConditionDetails.Where(y => y.IsTicked));
						if (tickedDocumentConditionDetails.Any())
						{
							var documentConditionsControl = new DocumentConditionsControl(guidedDecisionMakingBasic.DocumentConditions[0], tickedDocumentConditionDetails, isInSummary: true);
							var documentConditionDetailsGroupBox = documentConditionsControl.Controls.Find("DocumentConditionDetailsGroupBox", false).First();
							documentConditionDetailsGroupBox.Text = Res.GetString("02f5dc4f-beb4-47e4-b317-d29a63f9b0a0", "Document Conditions");

							documentConditionControls.Add(documentConditionsControl);
							table.Controls.Add(documentConditionsControl);
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

		const string flowLayoutPanelName = "DocumentConditionsContainerPanel_FlowLayoutPanel";

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
