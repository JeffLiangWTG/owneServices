using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class DynamicallyFilterTransferRulesEditorInfoRegistryEditor : RadioButtonRegistryItemEditor
	{
		public DynamicallyFilterTransferRulesEditorInfoRegistryEditor(IRegistryEditorInfo editorInfo, IRegistryDataType dataType) : base(editorInfo, dataType)
		{
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value) => base.SetValueFromEditorPaneCore(GetEditorControl(editorPane), value);
		protected override object GetValueFromEditorPaneCore(Control editorPane) => base.GetValueFromEditorPaneCore(GetEditorControl(editorPane));
		protected override void EnableEditorPaneCore(Control editorPane, bool enabled) => base.EnableEditorPaneCore(GetEditorControl(editorPane), enabled);
		ZUserControl GetEditorControl(Control editorPane) => (ZUserControl)editorPane.Controls[0];

		public override void SetEditorPaneLayout(Control editorPane, int width, int height)
		{
			editorPane.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			ControlDpiScalingHelper.SetHeight(ref editorPane, height, false);
			ControlDpiScalingHelper.SetWidth(ref editorPane, width, false);
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var panel = new FlowLayoutPanel
			{
				AutoSize = true,
				FlowDirection = FlowDirection.TopDown,
			};

			var parent = base.NewWinFormsEditorPaneCore();

			panel.Controls.Add(parent);

			var buttonForceToRunAll = new ZButton
			{
				Name = "buttonForceToRunAll",
				Text = Res.GetString("03AEC3F3-5514-4F9C-A5F4-77803ACB9953", "Process all transfer rules next time BMS runs"),
				UseVisualStyleBackColor = false,
				ReadOnly = BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value
			};

			ControlDpiScalingHelper.SetWidth(buttonForceToRunAll, 300, true);

			buttonForceToRunAll.Click += (s, e) =>
			{
				BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var message = Res.GetString("6AA7C307-D63A-4234-B875-915CB9C475B9", "Next time BMS service task runs it will process all transfer rules links.");
				var caption = Res.GetString("98BB7657-267C-4841-868E-1BE90CB8E351", "Dynamically filter disabled for next BMS run");

				using (ZMessageBox notification = new ZMessageBox(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
				{
					ZFormModaliser.ShowDialogAndDispose(notification);
				}

				buttonForceToRunAll.ReadOnly = true;
			};

			panel.Controls.Add(buttonForceToRunAll);

			return panel;
		}
	}
}
