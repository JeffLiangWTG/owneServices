using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(ManualReleaseForm))]
	sealed class ManualReleaseFormTest : ZFormBasherTest
	{
		[TestDate(2015, 12, 31, 16, 03, 00)]
		public void TestSaveButtonClick()
		{
			var expectedNoteText = $@"2015-12-23 15:02
TEST RELEASE REASON
{Env.CurrentUser.LoginName}
2015-12-31 16:03";

			var cfsShipment = GetTestCfsShipment();
			using (var form = new ManualReleaseForm(cfsShipment, Factory))
			{
				form.BusinessEntity.ManualReleaseReason = "TEST RELEASE REASON";
				form.BusinessEntity.ManualReleaseDate = new ZDateTime(2015, 12, 23, 15, 02, 00);
				form.Show();
				form.SaveButton.PerformClick();
				var note = cfsShipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.CustomsManualStatus.Description).FirstOrDefault();
				AssertNotNull("CustomsManualStatus note should not be null", note);
				AssertEquals("Note Text", expectedNoteText, note.ST_NoteText);
			}
		}

		[TestDate(2015, 12, 31, 16, 03, 00)]
		public void TestDeleteButtonClick()
		{
			var expectedNoteText = $@"2015-12-23 15:02
TEST RELEASE REASON
{Env.CurrentUser.LoginName}
2015-12-31 15:26";

			var cfsShipment = GetTestCfsShipment();
			cfsShipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsManualStatus.Description, expectedNoteText);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = new ManualReleaseForm(cfsShipment, Factory))
			{
				form.Show();
				form.DeleteButton.PerformClick();
				var note = cfsShipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.CustomsManualStatus.Description).FirstOrDefault();
				AssertNull("CustomsManualStatus note should be null", note);
			}
		}

		public void TestFormText()
		{
			using (var form = new ManualReleaseForm(GetTestCfsShipment(), Factory))
			{
				form.Show();
				AssertEquals("Manual Release", form.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestNoConcurrencyErrorWhenCalculateEstimatedPaymentDueDate()
		{
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.JE_CustomsOffice = "351";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 2499m;
			declaration.JE_DateOfArrival = new ZDateTime(2018, 11, 30, 12, 36, 37);
			Factory.Save();

			using (var form = new ManualReleaseForm(declaration, Factory))
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
				loadedDeclaration.CA_EstimatedPaymentDueDate = new ZDateTime(2018, 12, 8);
				newFactory.Save();

				form.BusinessEntity.ManualReleaseReason = "TEST RELEASE";
				form.BusinessEntity.ManualReleaseDate = new ZDateTime(2019, 2, 8, 15, 02, 00);
				form.Show();
				form.SaveButton.PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			}

			AssertEquals(new ZDateTime(2019, 2, 15), declaration.CA_EstimatedPaymentDueDate);
		}

		protected override Form GetFormToBashCore() => new ManualReleaseForm(GetTestCfsShipment(), Factory);

		CFSShipment GetTestCfsShipment()
		{
			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsConsignor = true;
			shipment.DocAddresses.RemoveAndDeleteAll();
			shipment.DocAddresses.AddNew(testOrg.MainAddress, DocAddressType.ConsignorDocumentaryAddress);
			shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.CustomsManualStatus.Description).DeleteAll();

			return shipment;
		}
	}
}
