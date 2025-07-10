using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CNSCPGAHeaderAddInfo))]
	sealed class CNSCPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CNSCPGAHeaderAddInfo(Factory.NewWithValidTestData<CNSCPGAHeader>().B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;

			var cnsc = invoiceLine.CNSCPGAHeader;
			cnsc.CA_AllProgramInd = YesNoList.Codes.Yes;
			cnsc.RunPreSaveValidation();
			AssertHasMessageErrorContaining(cnsc.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_CNSCInd = YesNoList.Codes.Yes;

			var cnsc2 = invoiceLine2.CNSCPGAHeader;
			cnsc2.CA_AllProgramInd = YesNoList.Codes.Yes;
			cnsc2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(cnsc2.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
