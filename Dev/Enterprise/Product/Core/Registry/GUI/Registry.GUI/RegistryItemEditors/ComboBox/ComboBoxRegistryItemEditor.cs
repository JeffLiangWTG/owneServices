using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public class ComboBoxRegistryItemEditor : RegistryItemEditor
	{
		public ComboBoxRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo) : base(dataType)
		{
			this.editorInfo = (ComboBoxRegistryEditorInfo)editorInfo;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			MyDropEdit result = null;
			ZUserControl wrapper = null;
			try
			{
				var bizObj = GetNewDropEditBusinessObject();
				result = new MyDropEdit();

				result.CharacterCasing = CharacterCasing.Normal;
				result.BindToList = "List";
				result.BindTo = "Value";
				result.ShowDescriptionBox = editorInfo.ShowComboDescription;
				result.ShowInDropDown = editorInfo.ShowComboDescription ? ZDropEdit.ShowInDropDownList.ShowCodeAndDescription : ZDropEdit.ShowInDropDownList.OnlyShowCode;
				result.Dock = DockStyle.Fill;

				wrapper = new ZUserControl();
				wrapper.Size = (result.Size + result.Margin.Size);
				wrapper.Controls.Add(result);
				wrapper.SetDataBinding(bizObj, string.Empty);

				return wrapper;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result?.Dispose();
				wrapper?.Dispose();
				throw;
			}
		}

		protected virtual DropEditBusinessObject GetNewDropEditBusinessObject()
		{
			return new DropEditBusinessObject(editorInfo.LookUpList);
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return GetBizo(editorPane).Value.ToString();
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var bizo = GetBizo(editorPane);
			bizo.Value = (string)value;
			bizo.HasChanges = false;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			editorPane.SetReadOnly(!enabled);
		}

		static internal DropEditBusinessObject GetBizo(Control editorPane)
			=> (DropEditBusinessObject)((ZUserControl)editorPane).CurrentDataItem;

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}

		protected ComboBoxRegistryEditorInfo editorInfo;

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
