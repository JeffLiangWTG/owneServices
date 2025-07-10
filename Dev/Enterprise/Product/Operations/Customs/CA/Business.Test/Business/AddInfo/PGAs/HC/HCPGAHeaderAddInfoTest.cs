using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(HCPGAHeaderAddInfo))]
	sealed class HCPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new HCPGAHeaderAddInfo(Factory.NewWithValidTestData<HCPGAHeader>().B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var hc = invoiceLine.HCPGAHeader;
			hc.CA_APIProgramInd = YesNoList.Codes.Yes;
			hc.RunPreSaveValidation();
			AssertHasMessageErrorContaining(hc.CA_BatchLotNumberInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_HCInd = YesNoList.Codes.Yes;

			var hc2 = invoiceLine2.HCPGAHeader;
			hc2.CA_APIProgramInd = YesNoList.Codes.Yes;
			hc2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(hc2.CA_BatchLotNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
