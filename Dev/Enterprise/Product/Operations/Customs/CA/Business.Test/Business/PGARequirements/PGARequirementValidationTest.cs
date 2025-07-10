using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PGARequirementValidationTest : TestCaseWithFactory
	{
		public void TestTariffDoesNotRequirePGA()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "3824700308");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "3825000308";

			var expectedMessage = "The Tariff does not indicate that this PGA reporting is required.";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var requirement = new PGARequirement(Factory, PGACodes.Codes.CFIA, pgaProvider);
			requirement.ProgramCodeRequirements[0].DeclareYes = true;
			requirement.Validation.ValidatePGARequired();

			AssertHasRowWarning("Should have the expected warning message as there is no matched PGA HS code.", requirement, expectedMessage);

			invoiceLine.JI_Tariff = "3824700308";
			requirement.Validation.ValidatePGARequired();

			AssertNoRowWarningContaining("Should not have the expected warning message as there is a matched PGA HS code.", requirement, expectedMessage);
		}
	}
}
