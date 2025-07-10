using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusContainerValidationBaseOnlyTest : CusContainerValidationTest<JobDeclaration>
	{
		public void TestCheckCO_ContainerNumber_LinkedInvLineWarning()
		{
			var warning = "A container should be linked to at least one package line. Please go to the Packaging tab -> sub tab Packing Details and select appropriate container(s)";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT1234560";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			AssertHasWarning("Container No. has correct warning", container.CO_ContainerNumberInfo, warning);

			var pivot = invoiceLine.ContainersPivot.AddNew();
			pivot.C2_CO = container.PK;
			container.Validation.ValidateCO_ContainerNumber();
			AssertNoWarning("Container No. - no warning on linked invoice lines", container.CO_ContainerNumberInfo, warning);
		}

		public void TestCheckCO_Seal()
		{
			const string errorMessage = "Please enter Seal Number";
			var container = Factory.New<CusContainer>();
			CombineAssertions(() =>
			{
				container.CO_Seal = string.Empty;
				AssertNoErrorContaining("Seal number is empty", container.CO_SealInfo, errorMessage);

				container.AdditionalSeals.AddNew();
				container.Validation.ValidateCO_Seal();
				AssertHasErrorContaining("Seal number is empty but there is an additional seal", container.CO_SealInfo, errorMessage);

				container.CO_Seal = "S01";
				AssertNoErrorContaining("Seal number is not empty", container.CO_SealInfo, errorMessage);
			});
		}

		public void TestCheckCO_SecondSeal()
		{
			const string errorMessage = "Please enter Second Seal No";
			var container = Factory.New<CusContainer>();
			CombineAssertions(() =>
			{
				container.CO_SecondSeal = string.Empty;
				AssertNoErrorContaining("Second Seal number is empty", container.CO_SecondSealInfo, errorMessage);

				container.AdditionalSeals.AddNew();
				container.Validation.ValidateCO_SecondSeal();
				AssertHasErrorContaining("Second Seal number is empty but there is an additional seal", container.CO_SecondSealInfo, errorMessage);

				container.CO_SecondSeal = "S02";
				AssertNoErrorContaining("Second Seal number is not empty", container.CO_SecondSealInfo, errorMessage);
			});
		}
	}
}
