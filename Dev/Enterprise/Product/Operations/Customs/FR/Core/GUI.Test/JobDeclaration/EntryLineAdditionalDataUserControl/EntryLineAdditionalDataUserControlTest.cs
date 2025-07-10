using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		public void TestDutyAndTaxDetailsIncludesConfirmedFees()
		{
			using (var form = new ZForm())
			using (var importEntryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(importEntryLineAdditionalDataUserControl);
				form.Show();
				var control = importEntryLineAdditionalDataUserControl.FindSingle<ZDynamicControlCreationUserControl>("DutyAndTaxDetails");
				AssertEquals("Using Calculated and Confirmed Fees", typeof(EntryLineTaxAndConfirmedFeeUserControl), control.UserControlType);
			}
		}

		public void TestFeesGridAvailability()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new EntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var feesGrid = (ZGrid)control.Controls.Find("EntryLineDutyAndTaxGrid", true).SingleOrDefault();
					AssertNotNull("The EntryLineDutyAndTaxGrid should be showing.", feesGrid);
					AssertNotNull("User control should have a National Type column.", feesGrid.GetColumnStyle(CusEntryLineFee.Schema.NationalFeeTypeCode));
				});
			}
		}
	}
}
