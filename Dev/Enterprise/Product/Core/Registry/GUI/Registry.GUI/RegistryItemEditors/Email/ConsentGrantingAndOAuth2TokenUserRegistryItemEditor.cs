using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ConsentGrantingAndOAuth2TokenUserRegistryItemEditor : RegistryItemEditor
	{
		public ConsentGrantingAndOAuth2TokenUserRegistryItemEditor(IRegistryItem registryItem) : base(registryItem?.DataType)
		{
			if (registryItem is Ms365OAuth2TokenRegistryItem ms365OAuth2TokenRegistryItem)
			{
				this.registryItem = ms365OAuth2TokenRegistryItem;
			}
			else
			{
				ErrorReporter.ReportOnce("ConsentGrantingAndOAuth2TokenUserRegistryItemEditor", $"registryItem should be type of Ms365OAuth2TokenRegistryItem as ConsentGrantingAndOAuth2TokenUserControl is ApplicationId relevant. registryItem Type:{registryItem?.GetType()?.FullName}");
			}
		}

		readonly Ms365OAuth2TokenRegistryItem registryItem;

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new ConsentGrantingAndOAuth2TokenUserControl(registryItem.EmailType, registryItem.Ms365OAuth2TenantId, registryItem.Ms365ApplicationIdForIncoming, registryItem.UseGraphApiForIncoming);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((ConsentGrantingAndOAuth2TokenUserControl)editorPane).Token;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((ConsentGrantingAndOAuth2TokenUserControl)editorPane).Token = (Ms365OAuth2Token)value;
		}
	}
}
