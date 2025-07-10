using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(EntryLineAdditionalDataUserControl))]
	sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		public void TestDutyAndTaxDetailsUserControl()
		{
			using (var form = new Form())
			using (var userControl = new EntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				AssertEquals(typeof(EntryLineTaxAndConfirmedFeeUserControl), userControl.DutyAndTaxDetails.GetType());
			}
		}
	}
}

