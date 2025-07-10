using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TCPGAHeaderAddInfo))]
	sealed class TCPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			var header = invoiceLine.TCPGAHeader;
			return new TCPGAHeaderAddInfo(header.B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;

			var tc = invoiceLine.TCPGAHeader;
			tc.CA_TPRProgramInd = YesNoList.Codes.Yes;
			tc.RunPreSaveValidation();
			AssertHasMessageErrorContaining(tc.CA_ProductClassInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_TCInd = YesNoList.Codes.Yes;

			var tc2 = invoiceLine2.TCPGAHeader;
			tc2.CA_TPRProgramInd = YesNoList.Codes.Yes;
			tc2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(tc2.CA_ProductClassInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
