using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(NRCanPGAHeaderAddInfo))]
	sealed class NRCanPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NRCanPGAHeaderAddInfo(Factory.NewWithValidTestData<NRCanPGAHeader>().B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;

			var nrCan = invoiceLine.NRCanPGAHeader;
			nrCan.CA_EEFProgramInd = YesNoList.Codes.Yes;
			nrCan.RunPreSaveValidation();
			AssertHasMessageErrorContaining(nrCan.CA_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_NRCanInd = YesNoList.Codes.Yes;

			var nrCan2 = invoiceLine2.NRCanPGAHeader;
			nrCan2.CA_EEFProgramInd = YesNoList.Codes.Yes;
			nrCan2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(nrCan2.CA_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
