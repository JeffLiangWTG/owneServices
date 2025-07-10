using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public class NotInterfaceConnectorLicencedRegistryItemEditor : RegistryItemEditor
	{
		public NotInterfaceConnectorLicencedRegistryItemEditor() : base(null) { }

		protected override Control NewWinFormsEditorPaneCore()
		{
			var label = new ZLabel();
			try
			{
				label.Dock = DockStyle.Fill;
				label.Text = Res.GetString("d4818181-f0ef-42a9-a971-dc60b65ee722", "This module is no longer available in CW1. This functionality has been superseded by the eAdaptor web service interface. Please contact your sales rep.");
				return label;
			}
#pragma warning disable ENT0001
			catch (Exception)
#pragma warning restore ENT0001
			{
				label.Dispose();
				throw;
			}
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			Value = value;
		}

		object Value { get; set; }
	}
}
