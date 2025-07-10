using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;
using CusEntryLine = Enterprise.Customs.KR.Business.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.KR.Business.CusEntryLineFee;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class Import5FEMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle()
		{
			var builder = new Import5FEMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 28);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.SubmissionDate), list[2].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentVersion), list[3].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentTypeDescription), list[4].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.DeclarantType), list[5].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ReasonCode), list[6].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentReason), list[7].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FaultParty), list[8].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FaultPartyOtherDescription), list[9].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.TotalAmendedItemsCount), list[10].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.TotalAmendedTaxCount), list[11].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.BeforeTotalDutyTaxAmount), list[12].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AfterTotalDutyTaxAmount), list[13].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.DutyTaxDifference), list[14].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.BeforeCustomsValue), list[15].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AfterCustomsValue), list[16].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.CustomsValueDifference), list[17].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.DutyPenaltyCause), list[18].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ApplyDutyPenaltyReduction), list[19].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.TaxPenaltyCause), list[20].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionIndicator), list[21].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionReasonCode), list[22].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionReason), list[23].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionReqSequence), list[24].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionAmount), list[25].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyPaymentReasonCode), list[26].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.RefundRequestSubmissionYN), list[27].ColumnName);
		}

		public void TestGetAdditionalTabPages()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var wrapper = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FE, MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
			var builder = new Import5FEMessageSendingFormBuilder();
			using var form = new MessageSendingActionForm(wrapper, builder);
			var messageGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
			var tabPages = builder.GetAdditionalTabPages(messageGrid);
			AssertNotNull(tabPages);

			AssertEquals(3, tabPages.Length);
			AssertEquals("Entry Details", tabPages[0].CaptionResourceString.Caption);
			AssertEquals("Duty && Tax Details", tabPages[1].CaptionResourceString.Caption);
			AssertEquals("Refund Request", tabPages[2].CaptionResourceString.Caption);
			AssertType(typeof(ImportAmendmentEntryDetailsUserControl), tabPages[0].Controls[0]);
			AssertType(typeof(Import5FEMessageDutyTaxDetailsUserControl), tabPages[1].Controls[0]);
			AssertType(typeof(Import5FEMessageRefundRequestUserControl), tabPages[2].Controls[0]);

			foreach (var control in tabPages)
			{
				control.Dispose();
			}
		}

		public void TestTabPageVisibilityChanged()
		{
			var entry1 = new TestDataSetupHelper(Factory).GetEntry929FullData(PaymentMethodCodeList.Codes._00, ZBool.True);
			var declaration = entry1.Declaration;
			var amendmentEntryLine = entry1.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 2);
			amendmentEntryLine.CL_CustomsValue = 0;
			var amendmentEntryLineFee = amendmentEntryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "DTY");
			amendmentEntryLineFee.CF_ChargeAmount = 0;

			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			var entryNum1 = entry2.EntryNumbers.AddNew();
			entryNum1.CE_IssueDate = new ZDateTime(2013, 01, 01);
			entryNum1.CE_ExpiryDate = new ZDateTime(2012, 01, 01);
			entryNum1.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum1.CE_EntryNum = "4062001070010U";
			var entryLine = entry2.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "0208100001";
			entryLine.CL_CustomsValue = 999999999990m;
			entryLine.CL_ValueForVAT = 11m;

			var entryLine1cud = entryLine.Fees.AddNew();
			entryLine1cud.CF_ChargeType = "DTY";
			entryLine1cud.CF_ChargeAmount = 999999999999m;

			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "0208122223";
			entryLine2.CL_CustomsValue = 9m;
			entryLine2.CL_ValueForVAT = 11m;

			var entryLine2cud = entryLine2.Fees.AddNew();
			entryLine2cud.CF_ChargeType = "DTY";
			entryLine2cud.CF_ChargeAmount = 999999999999m;
			Factory.Save();

			var wrapper = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FE, MessageFunctionCode.Amendment) as JobDeclarationAmendmentMessageSendingObjectParent;
			var builder = new Import5FEMessageSendingFormBuilder();
			using var form = new MessageSendingActionForm(wrapper, builder);
			form.Visible = true;
			form.Show();
			using var tabControl = form.FindSingle<ZTabControl>("ValidationErrorsTabControl");
			var refundRequestTabPage = tabControl.AllTabPages.FirstOrDefault(x => x.Name == "RefundRequestTabPage");
			AssertNotNull(refundRequestTabPage);
			Assert(!((ZTabPage)refundRequestTabPage).TabVisible);
			var sendingObject = wrapper.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>().First();
			sendingObject.RefundRequestSubmissionYN = YesNoList.Codes.Yes;
			Assert(((ZTabPage)refundRequestTabPage).TabVisible);

			var messageGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
			messageGrid.CurrentRowIndex = 1;
			Assert(!((ZTabPage)refundRequestTabPage).TabVisible);

			messageGrid.CurrentRowIndex = 0;
			Assert(((ZTabPage)refundRequestTabPage).TabVisible);

			sendingObject.RefundRequestSubmissionYN = YesNoList.Codes.No;
			Assert(!((ZTabPage)refundRequestTabPage).TabVisible);
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	sealed class Import5FEMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var wrapper = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FE);
			var sendingObject = wrapper.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>().First();
			sendingObject.RefundRequestSubmissionYN = YesNoList.Codes.Yes;
			return new MessageSendingActionForm(wrapper, new Import5FEMessageSendingFormBuilder());
		}
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_RefundType = RefundTypeList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders[0];

			var header = new ImportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			invoiceLine.JI_LinePrice = 100000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		JobDeclaration declaration;
	}
}
