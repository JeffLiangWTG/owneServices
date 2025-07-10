using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public class HBLAndHAWBBrandingOptionRegistryItemEditor : RegistryItemEditor
	{
		public HBLAndHAWBBrandingOptionRegistryItemEditor(IRegistryDataType dataType)
			: base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new HBLAndHAWBBrandingOptionControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((HBLAndHAWBBrandingOptionControl)editorPane).Value ? HBLAndHAWBBrandingOptionEditorInfo.AgentBranded : HBLAndHAWBBrandingOptionEditorInfo.ClientBranded;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			bool isAgentBranded = ((string)value) == HBLAndHAWBBrandingOptionEditorInfo.AgentBranded;
			((HBLAndHAWBBrandingOptionControl)editorPane).Value = isAgentBranded;
		}

		#region class HBLAndHAWBBrandingOptionControl

		internal class HBLAndHAWBBrandingOptionControl : RadioButtonControl
		{
			public HBLAndHAWBBrandingOptionControl()
			{
				optionGroupBox.Dock = DockStyle.Fill;

				ControlDpiScalingHelper.SetWidth(ref yesRadioButton, 90, true);
				yesRadioButton.Text = HBLAndHAWBBrandingOptionEditorInfo.AgentBranded;

				ControlDpiScalingHelper.SetWidth(ref noRadioButton, 90, true);
				noRadioButton.Text = HBLAndHAWBBrandingOptionEditorInfo.ClientBranded;

				ControlDpiScalingHelper.SetLeft(ref noRadioButton, yesRadioButton.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(10), false);

				ControlDpiScalingHelper.SetWidth(this, 245, true);
			}
		}

		#endregion
	}
}
