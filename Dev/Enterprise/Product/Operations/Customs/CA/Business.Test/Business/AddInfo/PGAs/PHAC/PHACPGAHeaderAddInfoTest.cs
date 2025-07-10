using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(PHACPGAHeaderAddInfo))]
	sealed class PHACPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PHACPGAHeaderAddInfo(Factory.NewWithValidTestData<PHACPGAHeader>().B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_PHACInd = YesNoList.Codes.Yes;

			var phac = invoiceLine.PHACPGAHeader;
			phac.CA_HAPProgramInd = YesNoList.Codes.Yes;
			phac.RunPreSaveValidation();
			AssertHasMessageErrorContaining(phac.CA_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_PHACInd = YesNoList.Codes.Yes;

			var phac2 = invoiceLine2.PHACPGAHeader;
			phac2.CA_HAPProgramInd = YesNoList.Codes.Yes;
			phac2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(phac2.CA_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
