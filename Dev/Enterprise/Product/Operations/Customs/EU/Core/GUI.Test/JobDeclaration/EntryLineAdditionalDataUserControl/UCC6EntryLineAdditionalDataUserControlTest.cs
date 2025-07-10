using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class UCC6EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestDutyAndTaxDetailsIncludesConfirmedFees()
		{
			using (var form = new ZForm())
			using (var importEntryLineAdditionalDataUserControl = new UCC6EntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(importEntryLineAdditionalDataUserControl);
				form.Show();
				var control = importEntryLineAdditionalDataUserControl.FindSingle<ZDynamicControlCreationUserControl>("DutyAndTaxDetails");
				AssertEquals("Using Calculated and Confirmed Fees", typeof(EntryLineTaxAndConfirmedFeeUserControl), control.UserControlType);
			}
		}
	}
}
