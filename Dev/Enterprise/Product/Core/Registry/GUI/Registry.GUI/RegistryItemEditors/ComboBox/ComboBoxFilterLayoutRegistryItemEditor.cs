using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	public class ComboBoxFilterLayoutRegistryItemEditor : RegistryItemEditor
	{
		public ComboBoxFilterLayoutRegistryItemEditor(IRegistryEditorInfo editorInfo, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(null)
		{
			this.EditorInfo = (ComboBoxFilterLayoutRegistryEditorInfo)editorInfo;
			this.Fallback = fallback;
			this.Factory = factory;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			MyDropEdit result = new MyDropEdit();
			DropEditBusinessObject myBusinessObject = new DropEditBusinessObject(GetListOfModuleFilterLayouts());
			result.CharacterCasing = CharacterCasing.Normal;
			result.BindToList = "List";
			result.SetDataBinding(myBusinessObject, "Value");
			return result;
		}

		public CodeDescriptionPairList GetListOfModuleFilterLayouts()
		{
			ZQuery filterLayoutsQuery = new ZQuery();
			filterLayoutsQuery.AddToFilter(StmModuleFilterSchema.S9_IsPublished, ZBool.True);
			filterLayoutsQuery.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, null);
			filterLayoutsQuery.AddToFilter(StmModuleFilterSchema.S9_ModuleID, EditorInfo.ModuleName);
			filterLayoutsQuery.AddToFilter(StmModuleFilterSchema.S9_GC, Fallback.CompanyPK(true));
			StmModuleFilter[] filters = Factory.Load<StmModuleFilter>(filterLayoutsQuery);
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (StmModuleFilter filterLayout in filters)
			{
				result.AddPair(filterLayout.S9_FilterNameMultilingual);
			}
			result.AddPair(ResString.GetMultilingualString("61af966a-0128-4f91-b059-541564e55670", "System Default Layout"));
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return (string)((MyDropEdit)editorPane).DataSource.Value;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((MyDropEdit)editorPane).DataSource.Value = (string)value;
			((MyDropEdit)editorPane).Text = (string)value;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((MyDropEdit)editorPane).DataSource.ReadOnly = !enabled;
		}

		readonly ComboBoxFilterLayoutRegistryEditorInfo EditorInfo;
		readonly FallbackLevel Fallback;
		readonly BusinessObjectFactory Factory;

		#region MyDropEdit

		protected class MyDropEdit : ZDropEdit
		{
			public new DropEditBusinessObject DataSource
			{
				get { return (DropEditBusinessObject)base.DataSource; }
			}
		}

		#endregion
	}
}
