using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.GUI.Testing
{
	public class MatchEPaymentRecipientsControlTest : TestCaseWithFactory
	{
		public void TestFilteredRecipientsPerformSearchAndClear()
		{
			var recipient1 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			recipient1.ABF_BeneficiaryFullName = "Peter Walter";
			recipient1.ABF_BeneficiaryNickName = "Walter";
			recipient1.ABF_RX_NKAccountCurrency = "AUD";
			recipient1.ABF_RN_NKCountryCode = "AU";
			recipient1.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			var recipient2 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			recipient2.ABF_BeneficiaryFullName = "John Smith";
			recipient2.ABF_BeneficiaryNickName = "Smith";
			recipient2.ABF_RX_NKAccountCurrency = "AUD";
			recipient2.ABF_RN_NKCountryCode = "US";
			recipient2.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			var recipient3 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			recipient3.ABF_BeneficiaryFullName = "Jane White";
			recipient3.ABF_BeneficiaryNickName = "White";
			recipient3.ABF_RX_NKAccountCurrency = "USD";
			recipient3.ABF_RN_NKCountryCode = "US";
			recipient3.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			using (var testForm = GetFormForTest())
			{
				using (var control = GetControlForTest())
				{
					testForm.Controls.Add(control);
					testForm.Show();

					var matchedRecipients = (MatchEPaymentRecipients)testForm.BusinessEntity;
					var recipientNameFilter = (ModuleTextFilter)matchedRecipients.RecipientsFilter["Recipient Name"];
					recipientNameFilter.IsActive = true;
					recipientNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
					recipientNameFilter.Property = "P";

					control.FilteredRecipientsFilterControl_PerformSearch(this, new EventArgs());
					AssertEquals(1, ((MatchEPaymentRecipients)testForm.BusinessEntity).FilteredRecipients.Count);
					AssertEquals("Found 1 records that match your criteria.", control.FilteredRecipientsInfoLabel.Text);

					control.FilteredRecipientsFilterControl_ClearButtonClicked(this, new EventArgs());
					AssertEquals(0, ((MatchEPaymentRecipients)testForm.BusinessEntity).FilteredRecipients.Count);
					AssertEquals("Found 0 records that match your criteria.", control.FilteredRecipientsInfoLabel.Text);
				}
			}
		}

		public void TestColumnsAddedCorrectly()
		{
			string[] expectedColumnNames = new string[] { "ABF_BeneficiaryFullName", "ABF_BeneficiaryNickName", "ABF_RX_NKAccountCurrency", "ABF_RN_NKCountryCode", "ABF_SystemLastEditTimeUtc", "ABF_BankName", "ABF_BankBsb", "ABF_BankBranchName", "ABF_BankAccount" };
			using (var form = GetFormForTest())
			{
				using (var userControl = GetControlForTest())
				{
					form.Controls.Add(userControl);
					form.Show();
					var columns = userControl.FilteredRecipientsGrid_ForTestOnly.ColumnStyles.Cast<ZGridColumnInfo>();
					foreach (var expectedColumn in expectedColumnNames)
					{
						Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
						Assert("New column should be visibled.", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
						Assert("New column should be read only.", !columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsReadOnly);
					}
				}
			}
		}

		public void TestRefreshButtonClick()
		{
			var request1 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request1.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request1.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request1.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 15);
			request1.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 15);
			request1.ABR_SystemCreateUser = "AAA";

			var request2 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request2.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request2.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request2.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 16);
			request2.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 16);
			request2.ABR_SystemCreateUser = "BBB";

			var request3 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request3.ABR_GC_Company = TestObjectCreator.NonCurrentCompany.PK;
			request3.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request3.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 18);
			request3.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 18);
			request3.ABR_SystemCreateUser = "CCC";

			var request4 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request4.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request4.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Error;
			request4.ABR_ErrorDescription = "Some error text here";
			request4.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 17);
			request4.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 17);
			request4.ABR_SystemCreateUser = TestObjectCreator.Staff.GS_Code;

			Factory.Save();

			using (var testForm = GetFormForTest())
			{
				using (var control = GetControlForTest())
				{
					testForm.Controls.Add(control);
					testForm.Show();

					var matchedRecipients = (MatchEPaymentRecipients)testForm.BusinessEntity;
					AssertEquals(request4.PK, matchedRecipients.CurrentRequest.PK);
					AssertEquals(request2.PK, matchedRecipients.LastReceivedRequest.PK);

					control.RefreshButton_Click(this, new EventArgs());
					AssertEquals(request4.PK, matchedRecipients.CurrentRequest.PK);
					AssertEquals(request2.PK, matchedRecipients.LastReceivedRequest.PK);

					var request5 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
					request5.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
					request5.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
					request5.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 18);
					request5.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 18);
					request5.ABR_SystemCreateUser = TestObjectCreator.Staff.GS_Code;
					Factory.Save();

					control.RefreshButton_Click(this, new EventArgs());
					AssertEquals(request5.PK, matchedRecipients.CurrentRequest.PK);
					AssertEquals(request5.PK, matchedRecipients.LastReceivedRequest.PK);
				}
			}
		}

		#region Implementation

		MatchEPaymentRecipientsControl GetControlForTest()
		{
			return new MatchEPaymentRecipientsControl();
		}

		AccountingZForm GetFormForTest()
		{
			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			Factory.Save();
			var matchRecipient = new MatchEPaymentRecipients(Factory, accountDetails);
			return new MatchEPaymentRecipientsForm(matchRecipient);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
