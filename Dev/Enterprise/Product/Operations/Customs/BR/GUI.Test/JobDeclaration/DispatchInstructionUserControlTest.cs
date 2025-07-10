using System;
using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class DispatchInstructionUserControlTest : TestCase
	{
		public void TestNumbersUserControl()
		{
			using (var control = new DispatchInstructionUserControl())
			{
				Assert(control.ControlHasChildOrIsTypeNamed2(typeof(ZGrid), "NumbersGrid"));
			}
		}

		public void TestColumnsAvailable()
		{
			using (var form = new DispatchInstructionUserControl())
			{
				var nodesGrid = (ZGrid)form.Controls.Find("NumbersGrid", true)[0];
				AssertNotNull("Grid should NOT be Null", nodesGrid);
				AssertEquals("Columns Count", 3, nodesGrid.ColumnStyles.Count);
				CombineAssertions("Column Styles", () =>
				{
					var columnInfo = nodesGrid.GetColumnStyle(DispatchInstructionNumber.Schema.CE_EntryType);
					AssertNotNull("CE_EntryType NOT be Null", DispatchInstructionNumber.Schema.CE_EntryType);
					AssertEquals("CE_EntryType Caption should be", "Type", columnInfo.CaptionResourceString.Caption);
					Assert("CE_EntryType should be Visible", columnInfo.IsVisible);

					columnInfo = nodesGrid.GetColumnStyle(DispatchInstructionNumber.Schema.CE_EntryNum);
					AssertNotNull("CE_EntryNum NOT be Null", DispatchInstructionNumber.Schema.CE_EntryNum);
					AssertEquals("CE_EntryNum Caption should be", "Number", columnInfo.CaptionResourceString.Caption);
					Assert("CE_EntryNum should be Visible", columnInfo.IsVisible);

					columnInfo = nodesGrid.GetColumnStyle(DispatchInstructionNumber.Schema.AdditionalReferenceNumberTypeDescription);
					AssertNotNull("AdditionalReferenceNumberTypeDescription NOT be Null", DispatchInstructionNumber.Schema.AdditionalReferenceNumberTypeDescription);
					AssertEquals("AdditionalReferenceNumberTypeDescription Caption should be", "Description", columnInfo.CaptionResourceString.Caption);
					Assert("AdditionalReferenceNumberTypeDescription should NOT be Visible", !columnInfo.IsVisible);
				});
			}
		}
	}

	static class DispatchInstructionControlExtensions
	{
		public static bool ControlHasChildOrIsType2(this Control control, Type type)
		{
			bool result = (control.GetType() == type);
			foreach (Control c in control.Controls)
			{
				result = result || ControlHasChildOrIsType2(c, type);
			}
			return result;
		}

		public static bool ControlHasChildOrIsTypeNamed2(this Control control, Type type, string name)
		{
			bool result = ((control.GetType() == type) && (control.Name == name));
			foreach (Control c in control.Controls)
			{
				result = result || ControlHasChildOrIsTypeNamed2(c, type, name);
			}
			return result;
		}
	}
}
