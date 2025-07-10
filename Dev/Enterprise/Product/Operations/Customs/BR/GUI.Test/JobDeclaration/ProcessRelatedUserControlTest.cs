using System;
using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ProcessRelatedUserControlTest : TestCase
	{
		public void TestNumbersUserControl()
		{
			using (var control = new ProcessRelatedUserControl())
			{
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZGrid), "NumbersGrid"));
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZDropEdit), "DropEditType"));
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZTextBox), "TextBoxNumber"));
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZTextBox), "TextBoxInfo"));
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZDateEdit), "DateEditIssued"));
			}
		}

		public void TestColumnsAvailable()
		{
			using (var form = new ProcessRelatedUserControl())
			{
				var nodesGrid = (ZGrid)form.Controls.Find("NumbersGrid", true)[0];
				AssertNotNull("Grid should NOT be Null", nodesGrid);
				AssertEquals("Columns Count", 5, nodesGrid.ColumnStyles.Count);
				CombineAssertions("Column Styles", () =>
				{
					var columnInfo = nodesGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryType);
					AssertNotNull("CE_EntryType NOT be Null", ProcessRelatedNumber.Schema.CE_EntryType);
					AssertEquals("CE_EntryType Caption should be", "Type", columnInfo.CaptionResourceString.Caption);
					Assert("CE_EntryType should be Visible", columnInfo.IsVisible);

					columnInfo = nodesGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryNum);
					AssertNotNull("CE_EntryNum NOT be Null", ProcessRelatedNumber.Schema.CE_EntryNum);
					AssertEquals("CE_EntryNum Caption should be", "Number", columnInfo.CaptionResourceString.Caption);
					Assert("CE_EntryNum should be Visible", columnInfo.IsVisible);

					columnInfo = nodesGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryLineReference);
					AssertNotNull("CE_EntryLineReference NOT be Null", ProcessRelatedNumber.Schema.CE_EntryLineReference);
					AssertEquals("CE_EntryLineReference Caption should be", "Info", columnInfo.CaptionResourceString.Caption);
					Assert("CE_EntryLineReference should be Visible", columnInfo.IsVisible);

					columnInfo = nodesGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_IssueDate);
					AssertNotNull("CE_IssueDate NOT be Null", ProcessRelatedNumber.Schema.CE_IssueDate);
					AssertEquals("CE_IssueDate Caption should be", "Issued Date", columnInfo.CaptionResourceString.Caption);
					Assert("CE_IssueDate should be Visible", columnInfo.IsVisible);

					columnInfo = nodesGrid.GetColumnStyle(ProcessRelatedNumber.Schema.AdditionalReferenceNumberTypeDescription);
					AssertNotNull("AdditionalReferenceNumberTypeDescription NOT be Null", ProcessRelatedNumber.Schema.AdditionalReferenceNumberTypeDescription);
					AssertEquals("AdditionalReferenceNumberTypeDescription Caption should be", "Description", columnInfo.CaptionResourceString.Caption);
					Assert("AdditionalReferenceNumberTypeDescription should NOT be Visible", !columnInfo.IsVisible);
				});
			}
		}
	}

	static class ControlExtensions
	{
		public static bool ControlHasChildOrIsType(this Control control, Type type)
		{
			bool result = (control.GetType() == type);
			foreach (Control c in control.Controls)
			{
				result = result || ControlHasChildOrIsType(c, type);
			}
			return result;
		}

		public static bool ControlHasChildOrIsTypeNamed(this Control control, Type type, string name)
		{
			bool result = ((control.GetType() == type) && (control.Name == name));
			foreach (Control c in control.Controls)
			{
				result = result || ControlHasChildOrIsTypeNamed(c, type, name);
			}
			return result;
		}
	}
}
