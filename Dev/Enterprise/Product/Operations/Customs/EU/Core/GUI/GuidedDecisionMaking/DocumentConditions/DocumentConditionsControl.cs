using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class DocumentConditionsControl : ZUserControl
	{
		public DocumentConditionsControl(GuidedDecisionMakingCondition condition)
			: this(condition, Enumerable.Empty<GuidedDecisionMakingConditionDetail>(), false)
		{
		}

		public DocumentConditionsControl(GuidedDecisionMakingCondition condition, IEnumerable<GuidedDecisionMakingConditionDetail> guidedDecisionMakingConditionDetail, bool isInSummary)
		{
			Condition = Argument.NotNull(condition, nameof(condition));
			this.guidedDecisionMakingConditionDetail = isInSummary ? guidedDecisionMakingConditionDetail.GroupBy(x => x.Code).Select(x => x.First()) : guidedDecisionMakingConditionDetail;
			if (!this.IsDesignMode())
			{
				SetDataBinding(Condition, "");
			}

			InitializeComponent();

			DetailControls = new List<DocumentConditionDetailControl>();
			PopulateDocumentConditionControls(isInSummary);
		}

		GuidedDecisionMakingCondition Condition { get; }

		List<DocumentConditionDetailControl> DetailControls { get; }

		readonly IEnumerable<GuidedDecisionMakingConditionDetail> guidedDecisionMakingConditionDetail;

		void PopulateDocumentConditionControls(bool isInSummary)
		{
			try
			{
				this.SuspendDrawing();

				var table = GetEmptyLayoutPanel();
				var conditionDetails = (guidedDecisionMakingConditionDetail?.Any() ?? false) ? guidedDecisionMakingConditionDetail : Condition.ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>();
				if (conditionDetails.Any() && !isInSummary)
				{
					var headerControl = new DocumentConditionHeaderControl(Condition);
					headerControl.Dock = DockStyle.Fill;
					table.Controls.Add(headerControl);
				}

				foreach (var conditionDetail in conditionDetails.OrderBy(x => x.Code))
				{
					var conditionDetailControl = new DocumentConditionDetailControl(conditionDetail, isInSummary);
					conditionDetailControl.BindingSource.SetDataBinding(conditionDetail, "");
					DetailControls.Add(conditionDetailControl);
					table.Controls.Add(conditionDetailControl);
				}
			}
			finally
			{
				this.ResumeDrawing();
			}
		}

		FlowLayoutPanel GetEmptyLayoutPanel()
		{
			var table = this.FindSingleOrDefault<FlowLayoutPanel>(x => x.Name == FlowLayoutPanelName);
			if (table is null)
			{
				table = new FlowLayoutPanel
				{
					Name = FlowLayoutPanelName,
					Dock = DockStyle.Fill,
					Size = ClientSize,
					FlowDirection = FlowDirection.TopDown,
					WrapContents = false,
					Margin = Padding.Empty,
					Padding = Padding.Empty,
					AutoSize = true,
				};
				DocumentConditionDetailsGroupBox.Controls.Add(table);
			}
			table.Controls.RemoveAndDisposeAll();

			return table;
		}

		ZString FlowLayoutPanelName => "DocumentConditionsControl_" + Condition.ConditionType;
	}
}
