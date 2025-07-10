using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.GUI
{
	class CNDeclarationDeadlineWarningThresholdEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CNDeclarationDeadlineWarningThresholdEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CNDeclarationDeadlineWarningThresholdUserControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
