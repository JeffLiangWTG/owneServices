using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public class GuidFindBoxRegistryItemEditor : RegistryItemEditor
	{
		public GuidFindBoxRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallback) : base(dataType)
		{
			this.EditorInfo = editorInfo;
			this.Fallback = fallback;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			GuidFindBoxBusinessObject myBusinessObject = new GuidFindBoxBusinessObject(Fallback, EditorInfo);

			MyGuidFindBox result = new MyGuidFindBox();
			result.ModuleID = myBusinessObject.ModuleID;
			result.BindToList = "ChoicesCollection";
			result.SetDataBinding(myBusinessObject, "SelectedPK");
			result.IsPrimaryKeyFromCodeRequired = IsPrimaryKeyFromCodeRequired;
			return result;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			ZGuid value = ((MyGuidFindBox)editorPane).DataSource.SelectedPK;
			Guid result = (value.IsEmpty || !value.IsValid) ? Guid.Empty : value.ToGuid();
			return result;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((MyGuidFindBox)editorPane).DataSource.SelectedPK = (Guid)value;
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((MyGuidFindBox)editorPane).Enabled = enabled;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}

		public override string GetCustomValidation(Control editorPane)
		{
			string errorMessage() =>
				(EditorInfo as IGuidFindBoxRegistryEditorInfo)?.CustomErrorMessage?.ToString()
				?? Res.GetString("532EF97A-870D-4FE7-AD73-0AAF57900A0B", "Please select a valid selection.");

			Argument.NotNull(editorPane, "EditorPane");

			var ds = (editorPane as MyGuidFindBox)?.DataSource;
			var pk = ds == null
				? ZGuid.Invalid
				: ds.SelectedPK;

			return !pk.IsEmpty && !pk.IsValid
				? errorMessage()
				: string.Empty;
		}

		public FallbackLevel Fallback;
		protected IRegistryEditorInfo EditorInfo;

		public bool IsPrimaryKeyFromCodeRequired { get; set; }
		#region MyGuidFindBox

		protected class MyGuidFindBox : ZGuidFindBox
		{
			public new GuidFindBoxBusinessObject DataSource
			{
				get { return (GuidFindBoxBusinessObject)base.DataSource; }
			}
		}

		#endregion
	}
}
