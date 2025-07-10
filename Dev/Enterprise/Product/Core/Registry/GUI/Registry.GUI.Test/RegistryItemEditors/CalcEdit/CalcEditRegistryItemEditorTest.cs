using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	public abstract class CalcEditRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestCalcEditBoundToWrapper()
		{
			using (var control = Editor.NewWinFormsEditorPane())
			{
				AssertNotNull("CalcEdit has bound BusinessObject", ((ZUserControl)control).CurrentDataItem);
				Assert("Bound BusinessObject is of type CalcEditWrapper<T>", ((ZUserControl)control).CurrentDataItem.GetType().GetGenericTypeDefinition() == typeof(CalcEditWrapper<>));
			}
		}

		public void TestChangesToCalcEditUpdatesWrapper()
		{
			var intDataType = new IntRegistryDataType();
			var decDataType = new DecimalRegistryDataType();
			var editorInfo = new NumericRegistryEditorInfo(0);
			var intCalcEditRegistryItemEditor = new CalcEditRegistryItemEditor(intDataType, editorInfo);

			editorInfo = new NumericRegistryEditorInfo(2);
			var decCalcEditRegistryItemEditor = new CalcEditRegistryItemEditor(decDataType, editorInfo);

			using (var control = intCalcEditRegistryItemEditor.NewWinFormsEditorPane())
			{
				AssertType<CalcEditWrapper<ZInt>>("bl1h", ((ZUserControl)control).CurrentDataItem);
			}

			using (var control = decCalcEditRegistryItemEditor.NewWinFormsEditorPane())
			{
				AssertType<CalcEditWrapper<ZDecimal>>("bl2h", ((ZUserControl)control).CurrentDataItem);
			}
		}

		public override void TestEditorPaneLayout()
		{
			using (var editorPane = Editor.NewWinFormsEditorPane())
			{
				Editor.SetEditorPaneLayout(editorPane, 0, 0);
				var graphic = Graphics.FromHwnd(editorPane.Handle);
				int minimumVisibleWidth = (int)graphic.MeasureString(MaxLengthString, editorPane.Font).Width;
				Assert("EditorPane is not wide enough to show the max number of digits", editorPane.Width >= minimumVisibleWidth);
				Assert("EditorPane should not be 15 pixels more than is required to show the max number of digits",
						editorPane.Width <= minimumVisibleWidth + ControlDpiScalingHelper.ScaleToCurrentDpiX(15));
			}
		}

		#region Implementation

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ZUserControl);
		}

		protected abstract int MaxLength { get; }
		protected abstract string MaxLengthString { get; }

		#endregion
	}
}
