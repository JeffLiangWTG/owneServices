using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DFOPGAHeaderAddInfo))]
	sealed class DFOPGAHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DFOPGAHeaderAddInfo(Factory.NewWithValidTestData<DFOPGAHeader>().B7_AddInfoDataInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			var expectedErrorMessage = "If the commodity being imported has been genetically modified or genetically engineered, the life stage must be provided.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;

			var dfo = invoiceLine.DFOPGAHeader;
			dfo.CA_ABIProgramInd = YesNoList.Codes.Yes;
			dfo.CA_HasGeneticModification = true;
			dfo.RunPreSaveValidation();
			AssertHasMessageErrorContaining(dfo.CA_LifeStagePropagateInfo, expectedErrorMessage);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_DFOInd = YesNoList.Codes.Yes;

			var dfo2 = invoiceLine2.DFOPGAHeader;
			dfo2.CA_ABIProgramInd = YesNoList.Codes.Yes;
			dfo2.CA_HasGeneticModification = true;
			dfo2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(dfo2.CA_LifeStagePropagateInfo, expectedErrorMessage);
		}
	}
}
