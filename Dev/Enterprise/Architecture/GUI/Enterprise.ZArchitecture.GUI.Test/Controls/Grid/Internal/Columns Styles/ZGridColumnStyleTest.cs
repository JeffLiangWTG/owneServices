using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Core.Forms.Testing
{
	sealed class ZGridColumnStyleTest : TestCaseWithDummy
	{
		public void TestColumnWidthForHeader()
		{
			using (var style = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo("Column1", 150)))
			{
				style.Alignment = HorizontalAlignment.Left;
				AssertEquals(150 - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZGridColumnStyle.WithOfNonWritibleAreaInColumnHeader), style.ColumnWidthForHeader);

				style.Alignment = HorizontalAlignment.Center;
				AssertEquals(150 - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZGridColumnStyle.WithOfNonWritibleAreaInColumnHeader), style.ColumnWidthForHeader);

				style.Alignment = HorizontalAlignment.Right;
				AssertEquals(150, style.ColumnWidthForHeader);
			}
		}

		public void TestCopyIsSortableFromColumnStyleInfo()
		{
			var styleInfo = new ZTextBoxColumnStyleInfo("Column1", 150);
			styleInfo.IsSortable = false;
			using (var columnStyle = new ZTextBoxColumnStyle(styleInfo))
			{
				AssertEquals(false, columnStyle.IsSortable);
			}

			styleInfo.IsSortable = true;
			using (var columnStyle = new ZTextBoxColumnStyle(styleInfo))
			{
				AssertEquals(true, columnStyle.IsSortable);
			}
		}

		public void TestGetValueAsText()
		{
			using (var style = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo()))
			{
				Dummy.Z0_Number = 123;
				AssertEquals("123", style.FormatValueObject(Dummy, Dummy.Z0_Number));
			}
		}

		public void TestGetEmptyColumnValue()
		{
			using (var style = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo()))
			{
				// Test value types.
				style.PropertyDescriptor = TypeDescriptor.GetProperties(Dummy.GetType())["Z0_Number"];
				var emptyValue = style.GetEmptyColumnValue_Exposed();
				AssertEquals("Value types should always return empty.", default(ZInt), emptyValue);
				// Test string types.
				style.PropertyDescriptor = TypeDescriptor.GetProperties(Dummy.GetType())["Z0_Description"];
				emptyValue = style.GetEmptyColumnValue_Exposed();
				AssertEquals("String types should always return empty.", default(ZString), emptyValue);
				// Test class types.
				style.PropertyDescriptor = TypeDescriptor.GetProperties(Dummy.GetType())["PKSchemaColumn"];
				AssertExceptionThrown<InvalidOperationException>("Class types should throw exception.", () => style.GetEmptyColumnValue_Exposed());
				// Test null type.
				style.PropertyDescriptor = null;
				AssertExceptionThrown<InvalidOperationException>("Null type should throw exception.", () => style.GetEmptyColumnValue_Exposed());
			}
		}
	}
}
