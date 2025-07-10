using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionOptionForm))]
	class CodeDescriptionOptionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var parent = new CodeDescriptionOptionCollectionParent(Factory, instruction.SpecialBusinessIdentifiers);
			return new CodeDescriptionOptionForm(parent);
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		public void TestOptionsGrid()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.CargoAttributes.AddNew("11");
			using (var form = new CodeDescriptionOptionForm(new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes), true))
			{
				form.Show();
				var grid = (ZGrid)form.Controls.Find("OptionsGrid", true).First();
				Assert(!grid.Columns.Contains(nameof(CodeDescriptionOption.Code)));
			}

			using (var form = new CodeDescriptionOptionForm(new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes)))
			{
				form.Show();
				var grid = (ZGrid)form.Controls.Find("OptionsGrid", true).First();
				Assert(grid.Columns.Contains(nameof(CodeDescriptionOption.Code)));
			}
		}

		public void TestOKButtonClick()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.CargoAttributes.AddNew("11");
			invoiceLine.CargoAttributes.AddNew("13");
			invoiceLine.CargoAttributes.AddNew("15");
			using (var form = new CodeDescriptionOptionForm(new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes)))
			{
				form.Show();
				var grid = (ZGrid)form.Controls.Find("OptionsGrid", true).First();
				grid.SelectAllElements();
				var items = grid.SelectedElements.Cast<CodeDescriptionOption>();
				items.First(option => option.Code == "11").Selected = false;
				items.First(option => option.Code == "13").Selected = false;
				items.First(option => option.Code == "15").Selected = false;
				items.First(option => option.Code == "12").Selected = true;
				items.First(option => option.Code == "16").Selected = true;
				items.First(option => option.Code == "26").Selected = true;
				items.First(option => option.Code == "30").Selected = true;
				var okButton = (ZButton)form.Controls.Find("zOKButton", true).First();
				okButton.PerformClick();
			}

			AssertEquals(4, invoiceLine.CargoAttributes.Count);
			Assert(invoiceLine.CargoAttributes.ContainsCode("12"));
			Assert(invoiceLine.CargoAttributes.ContainsCode("16"));
			Assert(invoiceLine.CargoAttributes.ContainsCode("26"));
			Assert(invoiceLine.CargoAttributes.ContainsCode("30"));
			Assert(invoiceLine.HasChanges);
		}

		public void TestCancelButtonClick()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.CargoAttributes.AddNew("11");
			invoiceLine.CargoAttributes.AddNew("13");
			invoiceLine.CargoAttributes.AddNew("15");
			using (var form = new CodeDescriptionOptionForm(new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes)))
			{
				form.Show();
				var grid = form.Controls.Find("OptionsGrid", true).First() as ZGrid;
				grid.SelectAllElements();
				var items = grid.SelectedElements.Cast<CodeDescriptionOption>();
				items.First(option => option.Code == "11").Selected = false;
				items.First(option => option.Code == "13").Selected = false;
				items.First(option => option.Code == "15").Selected = false;
				items.First(option => option.Code == "12").Selected = true;
				items.First(option => option.Code == "16").Selected = true;
				items.First(option => option.Code == "26").Selected = true;
				items.First(option => option.Code == "30").Selected = true;
				var zCancelButton = form.Controls.Find("ZCancelButton", true).First() as ZButton;
				zCancelButton.PerformClick();
			}

			AssertEquals(3, invoiceLine.CargoAttributes.Count);
			Assert(invoiceLine.CargoAttributes.ContainsCode("11"));
			Assert(invoiceLine.CargoAttributes.ContainsCode("13"));
			Assert(invoiceLine.CargoAttributes.ContainsCode("15"));
		}
	}
}
