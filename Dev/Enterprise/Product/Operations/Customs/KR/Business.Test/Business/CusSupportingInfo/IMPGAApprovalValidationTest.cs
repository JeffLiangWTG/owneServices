using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class IMPGAApprovalValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2023, 11, 29)]
		public void TestCheckCSI_DateOfIssue()
		{
			gaApproval.CSI_DateOfIssue = ZDateTime.Empty;
			AssertHasMessageErrorContaining(gaApproval.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_DateOfIssue = ZDateTime.Today;
			AssertNoMessageErrors(gaApproval.CSI_DateOfIssueInfo);

			gaApproval.CSI_DateOfIssue = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrors(gaApproval.CSI_DateOfIssueInfo);

			gaApproval.CSI_DateOfIssue = ZDateTime.Today.AddDays(1);
			AssertHasMessageErrorContaining(gaApproval.CSI_DateOfIssueInfo, IMPGAApprovalValidation.ApprovalDateMessageErr);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;
			entry.EntryNumber = "1234567890I";
			var entryNum1 = entry.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2023, 12, 1);
			invoiceline.JI_CL = entry.MergedLines.AddNew().PK;
			gaApproval.Validation.ValidateCSI_DateOfIssue();
			AssertNoMessageErrors(gaApproval.CSI_DateOfIssueInfo);

			entryNum1.CE_IssueDate = new ZDateTime(2023, 11, 1);
			gaApproval.Validation.ValidateCSI_DateOfIssue();
			AssertHasMessageErrorContaining(gaApproval.CSI_DateOfIssueInfo, IMPGAApprovalValidation.ApprovalDateMessageErr);
		}

		public void TestCheckCSI_Code()
		{
			gaApproval.CSI_Code = "";
			AssertNoMessageErrors(gaApproval.CSI_CodeInfo);

			gaApproval.CSI_Code = "Z";
			AssertHasMessageErrorContaining(gaApproval.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			gaApproval.CSI_Code = ImportRequirementTypeCodeList.Codes._1;
			AssertNoMessageErrors(gaApproval.CSI_CodeInfo);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			gaApproval.CSI_SubType = ImportRequirementTypeCodeList.Codes._1;

			gaApproval.CSI_ReferenceNumber = "";
			AssertHasMessageErrorContaining(gaApproval.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_ReferenceNumber = "82455884";
			AssertNoMessageErrors(gaApproval.CSI_ReferenceNumberInfo);

			gaApproval.CSI_SubType = ImportRequirementTypeCodeList.Codes._2;

			gaApproval.CSI_ReferenceNumber = "";
			AssertHasMessageErrorContaining(gaApproval.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_ReferenceNumber = "82455884";
			AssertNoMessageErrors(gaApproval.CSI_ReferenceNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceline = invoice.InvoiceLines.AddNew();
			gaApproval = invoiceline.GAApprovalDataCollection.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceline;
		GAApproval gaApproval;
	}
}
