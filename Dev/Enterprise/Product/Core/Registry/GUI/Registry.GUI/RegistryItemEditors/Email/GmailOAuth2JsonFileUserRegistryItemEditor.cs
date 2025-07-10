using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class GmailOAuth2JsonFileUserRegistryItemEditor : RegistryItemEditor
	{
		public GmailOAuth2JsonFileUserRegistryItemEditor(IRegistryItem registryItem) : base(registryItem?.DataType)
		{
			if (registryItem is not GmailOAuth2JsonFileRegistryItem gmailOAuth2JsonFileRegistryItem)
			{
				ErrorReporter.ReportOnce("GmailOAuth2JsonFileUserRegistryItemEditor", $"registryItem should be type of GmailOAuth2JsonFileRegistryItem. registryItem Type:{registryItem?.GetType()?.FullName}");
			}
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new GmailOAuth2JsonFileUserControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((GmailOAuth2JsonFileUserControl)editorPane).JsonFile;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((GmailOAuth2JsonFileUserControl)editorPane).JsonFile = (GmailOAuth2JsonFile)value;
		}
	}
}
