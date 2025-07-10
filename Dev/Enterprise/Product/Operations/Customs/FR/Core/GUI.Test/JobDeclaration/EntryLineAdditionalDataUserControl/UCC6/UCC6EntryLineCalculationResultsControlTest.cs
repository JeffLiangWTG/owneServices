using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Testing
{
	sealed class UCC6EntryLineCalculationResultsControlTest : TestCaseWithFactory
	{
		public void TestFeesCalculatedByCWGridAvailability()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new UCC6EntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var feesGrid = (ZGrid)control.Controls.Find("FeesCalculatedByCWGrid", true).SingleOrDefault();
					AssertNotNull("FeesCalculatedByCWGrid should be showing.", feesGrid);
					AssertNotNull("FeesCalculatedByCWGrid should have column 'Category'.", feesGrid.GetColumnStyle(nameof(CusEntryLineCalculatedFee.Category)));
					AssertNotNull("FeesCalculatedByCWGrid should have column 'Code'.", feesGrid.GetColumnStyle(nameof(CusEntryLineCalculatedFee.ChargeType)));
					AssertNotNull("FeesCalculatedByCWGrid should have column 'Amount'.", feesGrid.GetColumnStyle(nameof(CusEntryLineCalculatedFee.Amount)));
					AssertNotNull("FeesCalculatedByCWGrid should have column 'Currency'.", feesGrid.GetColumnStyle(nameof(CusEntryLineCalculatedFee.Currency)));
				});
			}
		}

		public void TestFeesConfirmedByCustomsGridAvailability()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new UCC6EntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var feesGrid = (ZGrid)control.Controls.Find("FeesConfirmedByCustomsGrid", true).SingleOrDefault();
					AssertNotNull("FeesConfirmedByCustomsGrid should be showing.", feesGrid);
					AssertNotNull("FeesConfirmedByCustomsGrid should have column 'Description'.", feesGrid.GetColumnStyle(nameof(CusEntryLineConfirmedFee.Description)));
					AssertNotNull("FeesConfirmedByCustomsGrid should have column 'Currency'.", feesGrid.GetColumnStyle(nameof(CusEntryLineConfirmedFee.Currency)));
					AssertNotNull("FeesConfirmedByCustomsGrid should have column 'Amount'.", feesGrid.GetColumnStyle(nameof(CusEntryLineConfirmedFee.Amount)));
				});
			}
		}
	}
}
