using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Riba.Testing
{
	[TestedType(typeof(BackDatePostForm))]
	public class BackDatePostFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			BackDatePostHolder holder = new BackDatePostHolder();
			return new BackDatePostForm(holder);
		}

		[TestDate(2016, 08, 09)]
		public void TestCancelButton_Click()
		{
			var holder = new BackDatePostHolder();
			using (BackDatePostForm form = new BackDatePostForm(holder))
			{
				form.Show();
				form.CancelButton_Click_ForTestOnly(null, null);
				AssertEquals(new ZDate(2016, 08, 09), holder.BackPostDate);
				AssertEquals(new ZDate(2016, 08, 09), holder.BackInvoiceDate);
			}
		}

		[TestDate(2016, 08, 09)]
		public void TestOKButton_Click()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();
			var holder = new BackDatePostHolder();
			holder.BackPostDate = ZDate.Invalid;
			using (BackDatePostForm form = new BackDatePostForm(holder))
			{
				form.Show();
				form.OKButton_Click_ForTestOnly(null, null);
				AssertEquals("Please choose a valid date", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				holder.BackPostDate = ZDate.Today;
				form.OKButton_Click_ForTestOnly(null, null);
			}
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion
	}
}
