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
	public partial class VATControl : ZUserControl
	{
		public VATControl(IEnumerable<GuidedDecisionMakingVAT> vats, ZString groupTitle, bool isInSummary = false)
		{
			InitializeComponent();
			this.isInSummary = isInSummary;
			VATs = Argument.NotNull(vats, nameof(vats));
			VATDetailControls = new List<VATDetailControl>();
			if (!groupTitle.IsEmpty)
			{
				VATControlGroupBox.Text = groupTitle;
			}
			PopulateVATDetailControls();
		}

		readonly bool isInSummary;

		IEnumerable<GuidedDecisionMakingVAT> VATs { get; }

		List<VATDetailControl> VATDetailControls { get; }

		void PopulateVATDetailControls()
		{
			try
			{
				this.SuspendDrawing();

				var table = GetEmptyLayoutPanel();
				foreach (var vat in VATs)
				{
					var vatDetailControl = new VATDetailControl(vat, this, isInSummary);
					vatDetailControl.BindingSource.SetDataBinding(vat, "");
					VATDetailControls.Add(vatDetailControl);
					table.Controls.Add(vatDetailControl);
				}
			}
			finally
			{
				this.ResumeDrawing();
			}
		}

		internal void TickExclusiveAdditionalCodeDetailControl(ZString code)
		{
			if (VATDetailControls.Any())
			{
				foreach (var vatDetailControl in VATDetailControls)
				{
					vatDetailControl.Tick(vatDetailControl.BoundCodeText == code);
				}
			}
		}

		ZString FlowLayoutPanelName => "VATControl_FlowLayoutPanel";

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
				VATControlGroupBox.Controls.Add(table);
			}
			table.Controls.RemoveAndDisposeAll();
			return table;
		}
	}
}
