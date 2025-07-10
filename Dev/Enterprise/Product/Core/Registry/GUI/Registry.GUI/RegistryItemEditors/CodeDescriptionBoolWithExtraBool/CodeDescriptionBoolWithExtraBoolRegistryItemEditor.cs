using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionBoolWithExtraBoolRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CodeDescriptionBoolWithExtraBoolRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: this(dataType, fallbackLevel, factory, DefaultFirstColumnName, string.Empty)
		{
		}

		public CodeDescriptionBoolWithExtraBoolRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory, string secondColumnName)
			: this(dataType, fallbackLevel, factory, DefaultFirstColumnName, secondColumnName)
		{
		}

		public CodeDescriptionBoolWithExtraBoolRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory, string firstColumnName, string secondColumnName)
			: base(dataType, fallbackLevel, factory)
		{
			EditorInfo = new CodeDescriptionBoolWithExtraBoolRegistryEditorInfo(firstColumnName, true, secondColumnName, true);
		}

		public CodeDescriptionBoolWithExtraBoolRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, fallbackLevel, null)
		{
			EditorInfo = (CodeDescriptionBoolWithExtraBoolRegistryEditorInfo)editorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			CodeDescriptionBoolWithExtraBoolControl result = new CodeDescriptionBoolWithExtraBoolControl();
			if (EditorInfo != null)
			{
				result.SetupBoolColumn(EditorInfo.BoolColumnCaption, EditorInfo.IsBoolColumnVisible, EditorInfo.Bool2ColumnCaption, true);
				result.SetupEditMode(EditorInfo.IsOnlyBoolColumnEditable);
			}
			return result;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		internal CodeDescriptionBoolWithExtraBoolRegistryEditorInfo EditorInfo { get; }
		static string DefaultFirstColumnName => Res.GetString("2DC67FF6-4D0E-46B0-89AE-69DB49E31FFA", "Default");
	}
}
