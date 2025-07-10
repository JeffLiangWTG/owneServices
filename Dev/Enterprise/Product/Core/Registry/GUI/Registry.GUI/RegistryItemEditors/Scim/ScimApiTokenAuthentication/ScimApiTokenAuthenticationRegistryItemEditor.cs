using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ScimApiTokenAuthenticationRegistryItemEditor : RegistryItemEditor
	{
		public ScimApiTokenAuthenticationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : this(dataType)
		{
		}

		public ScimApiTokenAuthenticationRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new ScimApiTokenAuthenticationControl();
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			base.EnableEditorPaneCore(editorPane, enabled);
			((ScimApiTokenAuthenticationControl)editorPane).ReadOnly = !enabled;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((ScimApiTokenAuthenticationControl)editorPane).Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((ScimApiTokenAuthenticationControl)editorPane).Value = (string)value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		public override string GetCustomValidation(Control editorPane)
		{
			if (string.IsNullOrEmpty(((ScimApiTokenAuthenticationControl)editorPane).Value))
			{
				return Res.GetString("282B2C5D-C1C2-4F6F-B6F9-5F4C421CB930", "Cannot save an empty API Token. Please generate a new token before saving.");
			}

			return string.Empty;
		}
	}
}
