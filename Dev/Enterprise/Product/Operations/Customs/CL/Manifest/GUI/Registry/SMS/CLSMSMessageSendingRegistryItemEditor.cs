using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	public class CLSMSMessageSendingRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CLSMSMessageSendingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CLSMSMessageSendingRegistryItemUserControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
