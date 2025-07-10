using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[TestExcludeBusinessObjectsAllHaveTestCasesAttribute]
	class CalcEditWrapper<T> : NonPersistentBusinessObject where T : IZType
	{
		T _value;
		public T Value
		{
			get { return _value; }
			set { SetNonPersistentPropertyValue(ValueInfo, ref _value, value); }
		}

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));
	}

	public class CalcEditRegistryItemEditor : RegistryItemEditor
	{
		public CalcEditRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType)
		{
			EditorInfo = (NumericRegistryEditorInfo)editorInfo;
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			var wrapper = IsIntDataType ? (NonPersistentBusinessObject)new CalcEditWrapper<ZInt>() : new CalcEditWrapper<ZDecimal>();
			var calc = new ZCalcEdit { Decimals = EditorInfo.DecimalPlaces, BindTo = (NoResString)"Value", Dock = DockStyle.Fill };
			calc.TextChanged += (o, e) => wrapper.HasChanges = true; // Update on every click, not just on focus lost

			var parent = new ZUserControl { Size = ControlDpiScalingHelper.NewScaledSize(calc.Size + calc.Margin.Size, true) };
			parent.Controls.Add(calc);
			parent.SetDataBinding(wrapper, "");
			return parent;
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			var control = (ZUserControl)editorPane;
			if (IsIntDataType)
			{
				return (int)((CalcEditWrapper<ZInt>)control.CurrentDataItem).Value;
			}
			else
			{
				return (decimal)((CalcEditWrapper<ZDecimal>)control.CurrentDataItem).Value;
			}
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var control = (ZUserControl)editorPane;
			if (IsIntDataType)
			{
				((CalcEditWrapper<ZInt>)control.CurrentDataItem).Value = (int)value;
			}
			else
			{
				((CalcEditWrapper<ZDecimal>)control.CurrentDataItem).Value = (decimal)value;
			}
		}

		public override void SetEditorPaneLayout(Control editorPane, int width, int height)
		{
			int decimalPlaces = (EditorInfo.DecimalPlaces == 0) ? 0 : 1 + EditorInfo.DecimalPlaces;
			int length = IsIntDataType ? 9 : 29 + decimalPlaces;
			string maxSize = new string('0', length);

			using (var graphic = Graphics.FromHwnd(editorPane.Handle))
			{
				float minimumWidth = graphic.MeasureString(maxSize, editorPane.Font).Width;
				ControlDpiScalingHelper.SetWidth(ref editorPane, (int)minimumWidth + ControlDpiScalingHelper.ScaleToCurrentDpiX(10), false);
			}
		}

		bool IsIntDataType
		{
			get { return DataType is IntRegistryDataType || DataType is RegistryItemDependentIntRegistryDataType; }
		}

		readonly NumericRegistryEditorInfo EditorInfo;
	}
}
