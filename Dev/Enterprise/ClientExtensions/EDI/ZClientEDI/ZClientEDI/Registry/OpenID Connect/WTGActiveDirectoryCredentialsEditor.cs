using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using ZClientEDI.Business;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class WTGActiveDirectoryCredentialsEditor : RegistryItemEditor
	{
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		public WTGActiveDirectoryCredentialsEditor(IRegistryDataType dataType)
			: this(dataType, null, null)
		{
		}

		public WTGActiveDirectoryCredentialsEditor(IRegistryDataType dataType, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.factory = factory;
			this.fallbackLevel = fallback;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((WTGActiveDirectoryCredentialsControl)editorPane).Value;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new WTGActiveDirectoryCredentialsControl();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var clone = ((WTGActiveDirectoryCredentials)value).Clone(fallbackLevel, factory);
			((WTGActiveDirectoryCredentialsControl)editorPane).Value = (WTGActiveDirectoryCredentials)clone;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);
			((WTGActiveDirectoryCredentialsControl)editorPane).ReadOnly = !enabled;
		}
	}
}
