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
	public partial class AdditionalCodesControl : ZUserControl
	{
		public AdditionalCodesControl(ZString groupTitle, List<GuidedDecisionMakingAdditionalCode> additionalCodes, bool isInSummary)
		{
			InitializeComponent();

			GroupTitle = Argument.NotNull(groupTitle, nameof(groupTitle));
			this.isInSummary = isInSummary;
			AdditionalCodes = Argument.NotNull(additionalCodes, nameof(additionalCodes));
			CodeDetailControlList = new List<AdditionalCodeDetailControl>();

			if (isInSummary)
			{
				foreach (var code in additionalCodes)
				{
					TickAdditionalCodeDetailControl(code.AdditionalCode);
				}
				AdditionalCodes = AdditionalCodes.GroupBy(x => x.AdditionalCode).Select(g => g.FirstOrDefault()).ToList();
			}
			AdditionalCodesFlowLayoutPanelGroupBox.Text = GroupTitle;
			PopulateAdditionalCodeControls();
		}

		readonly bool isInSummary;

		ZString GroupTitle { get; }

		List<GuidedDecisionMakingAdditionalCode> AdditionalCodes { get; }

		List<AdditionalCodeDetailControl> CodeDetailControlList { get; }

		public void TickExclusiveAdditionalCodeDetailControl(ZString code)
		{
			if (CodeDetailControlList.Any())
			{
				foreach (var codeDetailControl in CodeDetailControlList)
				{
					codeDetailControl.Tick(codeDetailControl.BoundCodeText == code);
				}
			}
		}

		public void TickAdditionalCodeDetailControl(ZString code)
		{
			foreach (var codeDetailControl in CodeDetailControlList)
			{
				if (codeDetailControl.BoundCodeText == code)
				{
					codeDetailControl.Tick(true);
				}
			}
		}

		ZString FlowLayoutPanelName => "AdditionalCodesControl_" + GroupTitle;

		void PopulateAdditionalCodeControls()
		{
			try
			{
				this.SuspendDrawing();

				var table = GetEmptyLayoutPanel();
				foreach (var additionalCode in AdditionalCodes.OrderBy(x => x.AdditionalCode))
				{
					var codeDetailControl = new AdditionalCodeDetailControl(additionalCode, this, isInSummary);
					codeDetailControl.BindingSource.SetDataBinding(additionalCode, "");
					CodeDetailControlList.Add(codeDetailControl);
					table.Controls.Add(codeDetailControl);
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
				AdditionalCodesFlowLayoutPanelGroupBox.Controls.Add(table);
			}
			table.Controls.RemoveAndDisposeAll();
			return table;
		}
	}
}
