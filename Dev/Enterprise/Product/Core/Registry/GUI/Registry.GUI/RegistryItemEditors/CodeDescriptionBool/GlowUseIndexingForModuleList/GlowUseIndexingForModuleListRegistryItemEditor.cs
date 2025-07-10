using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.Registry.GUI
{
	class GlowUseIndexingForModuleListRegistryItemEditor : RegistryItemEditor
	{
		public GlowUseIndexingForModuleListRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var result = new CodeDescriptionBoolWithExtraColumnControl(true);
			result.SetupExtraColumn(Res.GetData("76ff5848-8d90-45f2-8945-70b039f8f8de", "Beta"), true);
			result.SetupColumns(ResString.GetMultilingualString("D03307FB-7FFD-4EA0-BC0B-16D862657F7E", "Enabled"), true, false);
			result.SetupEditMode(true, forceDescriptionColumnReadOnly: true);
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			var codeDescriptions = ((IDataBoundControl)editorPane).DataSource as CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection;
			return codeDescriptions?.OfType<CodeDescriptionBoolDisallowNewCodeReadOnly>().Where(p => p.Bool)
				.Select(p => p.Code.ToString()).ToArray();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var modules = (string[])value;

			var codeDescriptions = new CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection(50);
			var moduleIdentifierList = GlowModuleToCW1ModuleConverter.GetAllModuleIDs();

			foreach (var moduleIdentifier in moduleIdentifierList.OrderBy(m => m.ExtendedDescription))
			{
				var moduleId = moduleIdentifier.ID.ToString();
				if (!codeDescriptions.ContainsCode(moduleId))
				{
					var item = codeDescriptions.AddNew();

					item.Code = moduleId;
					item.Bool = modules.Contains(moduleId);
					item.Description = moduleIdentifier.ExtendedDescription;
					item.String1 = GlowModuleToCW1ModuleConverter.CheckIsVerifiedModule(moduleIdentifier) ? string.Empty : (NoResString)"✔️";
				}
			}

			var codeDescriptionBoolControl = (CodeDescriptionBoolWithExtraColumnControl)editorPane;
			codeDescriptionBoolControl.SetDataBinding(codeDescriptions, null);
		}
	}
}
