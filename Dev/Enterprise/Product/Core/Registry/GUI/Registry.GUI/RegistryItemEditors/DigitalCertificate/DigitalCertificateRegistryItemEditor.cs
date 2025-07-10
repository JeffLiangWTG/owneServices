using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class DigitalCertificateRegistryItemEditor : RegistryItemEditor
	{
		public DigitalCertificateRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel) : base(dataType)
		{
			this.EditorInfo = (FileUpLoaderX509CertificateRegistryEditorInfo)editorInfo;
			this.EditorInfo.FallbackLevel = fallbackLevel;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return (GlbStaff.CurrentUser.GS_IsDeveloper) ? new DigitalCertificateControlWithExport(EditorInfo) : new DigitalCertificateControl(EditorInfo);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((DigitalCertificateControl)editorPane).FileDataAsBinary;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((DigitalCertificateControl)editorPane).SetFileData((byte[])value);
		}

		readonly FileUpLoaderX509CertificateRegistryEditorInfo EditorInfo;
	}
}
