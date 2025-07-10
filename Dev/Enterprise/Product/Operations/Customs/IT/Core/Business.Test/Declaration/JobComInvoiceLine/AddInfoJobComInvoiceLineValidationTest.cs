using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestParent()
	{
		var addInfo = new AddInfoJobComInvoiceLine(Factory.New<JobComInvoiceLine>().JI_AddInfoInfo);
		AssertNotNull("Validation Parent should not be null", addInfo.Validation.Parent);
	}

	public void TestCheckZG_SteelType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		invoiceLine.AddInfoValidation.ValidateZG_SteelType();
		AssertNoMessageErrors(invoiceLine);
		AssertNoWarnings(invoiceLine);

		invoiceLine.ZG_SteelType = "~";
		AssertHasError(invoiceLine.ZG_SteelTypeInfo, "Invalid Steel Type.");

		invoiceLine.ZG_SteelType = "5";
		AssertHasWarning(invoiceLine.ZG_SteelTypeInfo, "Steel Type is normally 1-4.");

		invoiceLine.ZG_SteelType = "A";
		AssertHasMessageError(invoiceLine.ZG_SteelTypeInfo, "Steel Type must be numeric.");

		invoiceLine.ZG_SteelType = SteelTypeList.Codes._1;
		AssertNoNotifications(invoiceLine.ZG_SteelTypeInfo);
	}

	public void TestCheckZG_PortTaxRate()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.HarbourCommodityCode, "Commodity Code");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, UniversalReferenceConstants.RefCusCodeListTypes.HarbourCommodityCode, "A1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		Factory.Save();

		using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_RL_NKFinalDestination = "ITVCE";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.ZG_PortTaxRate = "";
			AssertHasMessageErrorContaining(invoiceLine.ZG_PortTaxRateInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.ZG_PortTaxRate = "A1";
			AssertNoMessageErrorContaining(invoiceLine.ZG_PortTaxRateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.ZG_PortTaxRateInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.ZG_PortTaxRate = "XX";
			AssertNoMessageErrorContaining(invoiceLine.ZG_PortTaxRateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.ZG_PortTaxRateInfo, ListValidation.InvalidCodeMessageError);
		}
	}

	public void TestCheckZG_PortTaxRate_NewPortTaxes()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();
		var expectedMessageError = "A Port Tax Rate cannot be determined for the Port you entered (ITVCE).";

		CombineAssertions(() =>
		{
			using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invLine = SetUpDeclarationAndLineForTaxes(declaration);
				invLine.ZG_PortTaxRate = "XX";
				declaration.DoMerge();

				AssertHasMessageErrorContaining("Fees calculated, port is not correctly determined", invLine.ZG_PortTaxRateInfo, expectedMessageError);

				invLine.ZG_PortTaxRate = "A3";
				declaration.DoMerge();
				AssertNoMessageErrorContaining("Fees calculated, port is correctly determined", invLine.ZG_PortTaxRateInfo, expectedMessageError);
			}

			using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invLine = SetUpDeclarationAndLineForTaxes(declaration);
				invLine.ZG_PortTaxRate = "XX";

				AssertNoMessageErrorContaining("No Fees calculated, port is not determined, no message error", invLine.ZG_PortTaxRateInfo, expectedMessageError);

				declaration.DoMerge();
				AssertNoMessageErrorContaining("Fees calculated, port is not determined by the registry", invLine.ZG_PortTaxRateInfo, expectedMessageError);
			}
		});
	}

	public void TestCheckZG_PortTaxRate_SupportingDocumentForPortTaxes()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();
		var expectedMessageError = "A Supporting Document of Type 39YY and Reference --ITVCE is required for Port Taxes.";

		CombineAssertions(() =>
		{
			using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invLine = SetUpDeclarationAndLineForTaxes(declaration);

				AssertNoMessageErrorContaining("No Fees calculated, no supporting document needed", invLine.ZG_PortTaxRateInfo, expectedMessageError);

				declaration.DoMerge();
				AssertNoMessageErrorContaining("Fees calculated, supporting document auto-calculated", invLine.ZG_PortTaxRateInfo, expectedMessageError);

				invLine.SupportingDocuments.RemoveAndDeleteAll();
				invLine.AddInfoValidation.ValidateZG_PortTaxRate();
				AssertHasMessageErrorContaining("Fees calculated, supporting document needed", invLine.ZG_PortTaxRateInfo, expectedMessageError);
			}

			using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invLine = SetUpDeclarationAndLineForTaxes(declaration);

				AssertNoMessageErrorContaining("No Fees calculated, no supporting document needed", invLine.ZG_PortTaxRateInfo, expectedMessageError);

				declaration.DoMerge();
				invLine.SupportingDocuments.RemoveAndDeleteAll();
				AssertNoMessageErrorContaining("Fees calculated, supporting document not needed by the registry", invLine.ZG_PortTaxRateInfo, expectedMessageError);
			}
		});
	}

	JobComInvoiceLine SetUpDeclarationAndLineForTaxes(JobDeclaration declaration)
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_TransportMode = "SEA";
		declaration.JE_RL_NKPortOfArrival = "ITVCE";
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Tariff = "80";
		invoiceLine.ZG_PortTaxRate = "A3";
		invoiceLine.JI_Weight = 10;
		invoiceLine.JI_WeightUQ = "T";
		return invoiceLine;
	}
}
