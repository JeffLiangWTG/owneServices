using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ECCCPGAHeaderAddInfo))]
	sealed class ECCCPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ECCCPGAHeaderAddInfo(Factory.NewWithValidTestData<ECCCPGAHeader>().B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.RunPreSaveValidation();
			AssertHasMessageErrorContaining(eccc.CA_ProcessCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_ECCCInd = YesNoList.Codes.Yes;

			var eccc2 = invoiceLine2.ECCCPGAHeader;
			eccc2.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(eccc2.CA_ProcessCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
