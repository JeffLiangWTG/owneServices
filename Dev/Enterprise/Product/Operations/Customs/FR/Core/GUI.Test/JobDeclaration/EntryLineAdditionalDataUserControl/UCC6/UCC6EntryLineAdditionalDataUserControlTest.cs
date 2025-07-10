using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Testing
{
	sealed class UCC6EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		public void TestCalculationResultsControl()
		{
			using (var form = new ZForm())
			using (var control = new UCC6EntryLineAdditionalDataUserControl())
			{
				var uCC6EntryLineCalculationResultsControl = (UCC6EntryLineCalculationResultsControl)control.Controls.Find("UCC6EntryLineCalculationResultsControl", true).SingleOrDefault();
				AssertNotNull("UCC6EntryLineCalculationResultsControl should be showing.", uCC6EntryLineCalculationResultsControl);
			}
		}

		public void TestDutyAndTaxDetailsUserControlType()
		{
			using (var control = new UCC6EntryLineAdditionalDataUserControl())
			{
				AssertEquals(typeof(UCC6EntryLineTaxAndConfirmedFeeUserControl), control.DutyAndTaxDetails.UserControlType);
			}
		}
	}
}
