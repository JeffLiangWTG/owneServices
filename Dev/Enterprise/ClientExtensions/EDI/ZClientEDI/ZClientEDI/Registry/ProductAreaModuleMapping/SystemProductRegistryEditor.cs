using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class SystemProductRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public SystemProductRegistryEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType, null, null)
		{
			this.editorInfo = editorInfo as SystemProductRegistryEditorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			var result = new SystemProductRegistryControl();

			result.ModuleCaption = editorInfo.ModuleCaption;
			result.SplitContainer.Panel2Collapsed = !editorInfo.IsSourceModuleMappingsVisible;
			((ZDropEditColumnStyleInfo)result.SourceModuleGrid.GetColumnStyle("Code")).BindToList = GetCodeBindToList();

			result.IsCodeDropDown = editorInfo.ModuleListType == ModuleListType.Cr8 || editorInfo.ModuleListType == ModuleListType.Cr9;
			return result;
		}

		string GetCodeBindToList()
		{
			switch (editorInfo.ModuleListType)
			{
				case ModuleListType.MenuSection:
					return "Lookups.MenuSectionSourceModuleList";

				case ModuleListType.Cr8:
					return "Lookups.Cr8SourceModuleList";

				case ModuleListType.Cr9:
					return "Lookups.Cr9SourceModuleList";

				default:
					return "";
			}
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly SystemProductRegistryEditorInfo editorInfo;
	}
}
