using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class DateOfIssueAndExpiryUserControlTest : TestCaseWithFactory
	{
		public void TestDateOfIssueDateEdit()
		{
			var dateEdit = control.FindSingle<ZDateEdit>("DateOfIssueDateEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_DateOfIssue), dateEdit.GetBindingMember());
				AssertEquals("TabIndex", 0, dateEdit.TabIndex);
			});
		}

		public void TestDateOfExpiryDateEdit()
		{
			var dateEdit = control.FindSingle<ZDateEdit>("DateOfExpiryDateEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.SupportingDocuments) + "." + nameof(SupportingDocument.CSI_DateOfExpiry), dateEdit.GetBindingMember());
				AssertEquals("TabIndex", 1, dateEdit.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DateOfIssueAndExpiryUserControl();
		}
		DateOfIssueAndExpiryUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
