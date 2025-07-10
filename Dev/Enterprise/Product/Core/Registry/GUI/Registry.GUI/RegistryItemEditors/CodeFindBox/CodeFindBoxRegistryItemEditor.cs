using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public class CodeFindBoxRegistryItemEditor : RegistryItemEditor
	{
		public CodeFindBoxRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType)
		{
			if (editorInfo == null)
			{
				throw new ArgumentNullException(nameof(editorInfo));
			}
			this.EditorInfo = editorInfo;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			CodeFindBoxBusinessObject bizObj = new CodeFindBoxBusinessObject(EditorInfo);

			ZCodeFindBox result = new ZCodeFindBox();
			result.ModuleID = bizObj.ModuleID;
			result.BindToList = "ChoicesCollection";
			result.SetDataBinding(bizObj, "SelectedCode");

			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((CodeFindBoxBusinessObject)((IDataBoundControl)editorPane).DataSource).SelectedCode.ToString();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((CodeFindBoxBusinessObject)((IDataBoundControl)editorPane).DataSource).SelectedCode = new ZString(value);
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((ZCodeFindBox)editorPane).Enabled = enabled;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}

		public override string GetCustomValidation(Control editorPane)
		{
			CodeFindBoxBusinessObject bizObj = (CodeFindBoxBusinessObject)((IDataBoundControl)editorPane).DataSource;
			return bizObj.SelectedCodeInfo.HasMessageErrors() ? Res.GetString("94e6e922-f531-4606-bbba-7a8f7b274cf2", "Please enter a valid code.") : "";
		}

		readonly IRegistryEditorInfo EditorInfo;
	}
}
