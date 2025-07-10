using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	class DateEditWrapper : NonPersistentBusinessObject
	{
		ZDateTime _value;
		public ZDateTime Value
		{
			get { return _value; }
			set { SetNonPersistentPropertyValue(ValueInfo, ref _value, value); }
		}

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));
	}

	public class DateEditRegistryItemEditor : RegistryItemEditor
	{
		public DateEditRegistryItemEditor(IRegistryDataType dataType, DateTimeRegistryEditorInfo editorInfo)
			: base(dataType)
		{
			this.editorInfo = editorInfo;
		}

		public DateTimeRegistryEditorInfo EditorInfo
		{
			get { return editorInfo; }
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var wrapper = new DateEditWrapper();
			RegistryZDateEdit dateEdit = null;
			ZUserControl parent = null;
			try
			{
				dateEdit = new RegistryZDateEdit();
				dateEdit.BindTo = "Value";
				dateEdit.Dock = DockStyle.Fill;
				dateEdit.DateTimeFormat = editorInfo.DateTimeFormat;
				dateEdit.TextChanged += (o, e) => wrapper.HasChanges = true; // Update on every click, not just on focus lost

				parent = new ZUserControl();
				parent.Size = dateEdit.Size + dateEdit.Margin.Size;
				parent.Controls.Add(dateEdit);
				parent.SetDataBinding(wrapper, "");

				return parent;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				dateEdit?.Dispose();
				parent?.Dispose();
				throw;
			}
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			var control = (ZUserControl)editorPane;
			var selectedDate = ((DateEditWrapper)control.CurrentDataItem).Value;
			return selectedDate.IsValid ? (object)selectedDate.ToDateTime() : DateTime.MinValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var control = (ZUserControl)editorPane;
			var valueToSet = (value == null || value.Equals(DateTime.MinValue)) ? ZDateTime.Empty : new ZDateTime(value);
			((DateEditWrapper)control.CurrentDataItem).Value = valueToSet;
		}

		public override string GetCustomValidation(Control editorPane)
		{
			Argument.NotNull(editorPane, "EditorPane");
			var control = (ZUserControl)editorPane;
			var value = ((DateEditWrapper)control?.CurrentDataItem)?.Value ?? ZDateTime.Empty;
			return !value.IsEmpty && !value.IsValid ? Res.GetString("089A115D-AA15-4446-95B3-B39A62869D4F", "Please select a valid date.") : "";
		}

		readonly DateTimeRegistryEditorInfo editorInfo;

		#region class RegistryZDateEdit

		class RegistryZDateEdit : ZDateEdit
		{
			protected override void OnLeave(EventArgs e)
			{
				PushValue();
				base.OnLeave(e);
			}
		}

		#endregion
	}
}
