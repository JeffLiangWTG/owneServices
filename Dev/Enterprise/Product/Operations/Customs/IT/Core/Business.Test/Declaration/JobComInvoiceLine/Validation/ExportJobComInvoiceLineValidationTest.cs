using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ExportJobComInvoiceLineValidationTest : CommonJobComInvoiceLineValidationTest
{
	public void TestCheckJI_StateOrRegionOfOrigin_ListValidation()
	{
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_StateOrRegionOfOrigin = "XX";
		AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_StateOrRegionOfOrigin = "PD";
		AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckJI_StateOrRegionOfOrigin_MandatoryValidation()
	{
		const string expectedMessageError = "You must enter a province";

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		declaration.JE_GoodsOrigin = "IT";
		invoiceLine.JI_StateOrRegionOfOrigin = "";
		AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);

		invoiceLine.JI_StateOrRegionOfOrigin = "PD";
		AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);

		declaration.JE_GoodsOrigin = "DE";
		invoiceLine.JI_StateOrRegionOfOrigin = "";
		AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);
	}

	public void TestCheckJI_StateOrRegionOfOrigin_ProvinceMustBeEmptyWhenCountryOfDispatchIsNotItaly()
	{
		const string expectedMessageError = "Province must be empty when [15] Country of Dispatch is not Italy";

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		declaration.JE_GoodsOrigin = "";
		invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
		AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);

		declaration.JE_GoodsOrigin = "IT";
		invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
		AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);

		declaration.JE_GoodsOrigin = "DE";
		invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
		AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);

		declaration.JE_GoodsOrigin = "";
		invoiceLine.JI_StateOrRegionOfOrigin = "PD";
		AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);

		declaration.JE_GoodsOrigin = "IT";
		invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
		AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);

		declaration.JE_GoodsOrigin = "DE";
		invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
		AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, expectedMessageError);
	}

	public void TestCheckJI_CEI_EntryInstructionMustHaveSameParticipant()
	{
		var expectedError = "It is not possible to link Invoice Lines of the same Invoice to Entry Instructions with different Participants";

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		var invoiceA = declaration.Invoices.AddNew();
		var invoiceLine1A = invoiceA.InvoiceLines.AddNew();
		var invoiceLine2A = invoiceA.InvoiceLines.AddNew();
		var invoiceB = declaration.Invoices.AddNew();
		var invoiceLine1B = invoiceB.InvoiceLines.AddNew();

		entryInstruction1.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;

		invoiceLine1A.JI_CEI = entryInstruction1.PK;
		invoiceLine2A.JI_CEI = entryInstruction2.PK;
		AssertNoErrorContaining("No error expected when invoice lines of the same Invoice have entry instructions with the same Participant Type", invoiceLine2A.JI_CEIInfo, expectedError);

		invoiceLine2A.JI_CEI = entryInstruction1.PK;
		AssertNoErrorContaining("No error expected when invoice lines of the same Invoice have a single entry instruction", invoiceLine2A.JI_CEIInfo, expectedError);

		entryInstruction2.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		invoiceLine2A.JI_CEI = entryInstruction2.PK;
		AssertHasErrorContaining("Error expected  when invoice lines of the same Invoice have entry instruction with different Participant Types", invoiceLine2A.JI_CEIInfo, expectedError);

		invoiceLine2A.JI_CEI = entryInstruction1.PK;
		invoiceLine1B.JI_CEI = entryInstruction2.PK;
		AssertNoErrorContaining("No error expected when invoice lines of the different Invoice have a entry instruction with different Participant Types", invoiceLine1B.JI_CEIInfo, expectedError);
	}

	public void TestCheckJI_CEI_WhenUCC6Export_B2DeclarationShouldRequireAtLeastOne60YY()
	{
		const string message = "For this Declaration Type and Procedure code, at least one Supporting document of the Entry Line, linked to this Invoice Line, must be of type 60YY";
		declaration.MessageVersion = "XML";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = declaration.CustomsEntryInstructions.AddNew().PK;
		AssertEquals("[PRE-CONDITION] EXP UCC6", expected: true, declaration.IsUCC6AndIsExport);
		AssertNotNull("[PRE-CONDITION] Has instruction", invoiceLine.EntryInstruction);
		AssertEntityValidation(invoiceLine)
			.WhenProperty(x => x.EntryInstruction.CEI_Style, Is.EqualTo("B2"))
			.WhenProperty(x => x.EntryInstruction.CEI_Procedure, Is.EqualToAnyOf("21", "22"))
			.WhenValidating(() => invoiceLine.Validation.ValidateJI_CEI())
			.ShouldCheckThat(x => x.JI_CEIInfo, Has.MessageErrorContaining(message));

		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "60YY";
		AssertSame("[PRE-CONDITION] Same instruction", invoiceLine.EntryInstruction, supportingDocument.Instruction);
		AssertEntityValidation(invoiceLine)
			.WhenProperty(x => x.EntryInstruction.CEI_Style, Is.EqualTo("B2"))
			.WhenProperty(x => x.EntryInstruction.CEI_Procedure, Is.EqualToAnyOf("21", "22"))
			.WhenValidating(() => invoiceLine.Validation.ValidateJI_CEI())
			.ShouldCheckThat(x => x.JI_CEIInfo, Has.NoMessageErrorContaining(message));
	}

	public void TestCheckJI_ValuationCode()
	{
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_ValuationCode = "9";
		AssertHasMessageErrorContaining("When ValuationCode is invalid, for IMP", invoiceLine.JI_ValuationCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
		declaration.JE_MessageType = "EXP";
		invoiceLine.Validation.ValidateJI_ValuationCode();
		AssertNoMessageErrorContaining("When ValuationCode is invalid, for EXP", invoiceLine.JI_ValuationCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckJI_OA_ConsigneeAddressAgainstTransitionPeriod()
	{
		var expectedNameWarningForTransitionPeriod = "Consignee Company Name is longer than 35 characters, it will be truncated in the message.";
		var expectedNameWarningForNonTransitionPeriod = "Consignee Company Name is longer than 70 characters, it will be truncated in the message.";

		var declarant = Factory.New<OrgHeader>();
		var declarantAddress = declarant.MainAddress;
		var invoiceLine = declaration.Invoices
			.AddNew()
			.InvoiceLines
			.AddNew();

		invoiceLine.JI_OA_ConsigneeAddress = declarantAddress.PK;
		var validation = invoiceLine.Validation;
		var consigneeAddressInfo = invoiceLine.JI_OA_ConsigneeAddressInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When is UCC6 and InTransition Period", () =>
				{
					declarant.OH_FullName = "".PadRight(36, 'D');
					validation.ValidateJI_OA_ConsigneeAddress();
					AssertHasWarningContaining(consigneeAddressInfo, expectedNameWarningForTransitionPeriod);

					declarant.OH_FullName = "".PadRight(30, 'D');
					validation.ValidateJI_OA_ConsigneeAddress();
					AssertNoWarningContaining(consigneeAddressInfo, expectedNameWarningForTransitionPeriod);
				});
			}

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				CombineAssertions("When is UCC6 but not In transition period", () =>
				{
					declarant.OH_FullName = "".PadRight(71, 'D');
					validation.ValidateJI_OA_ConsigneeAddress();
					AssertHasWarningContaining(consigneeAddressInfo, expectedNameWarningForNonTransitionPeriod);

					declarant.OH_FullName = "".PadRight(30, 'D');
					validation.ValidateJI_OA_ConsigneeAddress();
					AssertNoWarningContaining(consigneeAddressInfo, expectedNameWarningForNonTransitionPeriod);
				});
			}
		}
	}

	public void TestCheckJI_Weight()
	{
		var invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CheckInvoiceLineMandatoryField(invoiceLine.JI_WeightInfo);
		}
	}

	public void TestCheckJI_Weight_R0221()
	{
		const string expectedMessageError = "[R0221] Gross Weight must be greater than zero";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		var entry = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entry.MergedLines.AddNew();
		var entryLine2 = entry.MergedLines.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When is UCC6", () =>
			{
				invoiceLine1.Validation.ValidateJI_Weight();
				AssertNoMessageErrorContaining(invoiceLine1.JI_WeightInfo, expectedMessageError);

				invoiceLine2
					.PackagesPivot.AddNew()
					.CHC_NumberOfPacks = 10;
				invoiceLine1.Validation.ValidateJI_Weight();
				AssertNoMessageErrorContaining(invoiceLine1.JI_WeightInfo, expectedMessageError);

				DoMerge();

				invoiceLine1.Validation.ValidateJI_Weight();
				invoiceLine2.Validation.ValidateJI_Weight();
				AssertNoMessageErrorContaining(invoiceLine1.JI_WeightInfo, expectedMessageError);
				AssertHasMessageErrorContaining(invoiceLine2.JI_WeightInfo, expectedMessageError);

				invoiceLine2.JI_Weight = 20;
				invoiceLine2.Validation.ValidateJI_Weight();
				AssertNoMessageErrorContaining(invoiceLine2.JI_WeightInfo, expectedMessageError);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			invoiceLine2.JI_Weight = 0;
			invoiceLine2.Validation.ValidateJI_Weight();
			AssertNoMessageErrorContaining(invoiceLine2.JI_WeightInfo, expectedMessageError);
		}

		void DoMerge()
		{
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entry.ResetTotalsAndCachedValues();
		}
	}

	public void TestCheckJI_Weight_R0222()
	{
		const string expectedMessageError = "[R0222] Gross Weight must be zero when Pack Quantity is zero";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var packagePivot = invoiceLine.PackagesPivot.AddNew();
		var entryLine = declaration
			.CustomsEntryHeaders.AddNew()
			.MergedLines.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When is UCC6", () =>
			{
				invoiceLine.Validation.ValidateJI_Weight();
				AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, expectedMessageError);

				invoiceLine.JI_Weight = 10;
				AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, expectedMessageError);

				invoiceLine.JI_CL = entryLine.PK;
				entryLine.InvoiceLines.Add(invoiceLine);

				invoiceLine.Validation.ValidateJI_Weight();
				AssertHasMessageError(invoiceLine.JI_WeightInfo, expectedMessageError);

				packagePivot.CHC_NumberOfPacks = 10;
				invoiceLine.Validation.ValidateJI_Weight();
				AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, expectedMessageError);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			packagePivot.CHC_NumberOfPacks = 0;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, expectedMessageError);
		}
	}

	public void TestCheck_R0224()
	{
		const string expectedMessageError = "[R0224] Total Gross Weight for this entry must be greater or equal than the total Customs Quantity";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		var entry = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entry.MergedLines.AddNew();
		var entryLine2 = entry.MergedLines.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When is UCC6", () =>
			{
				invoiceLine1.JI_Weight = 10;
				invoiceLine2.JI_Weight = 10;
				invoiceLine1.JI_CustomsQuantity = 20;
				invoiceLine2.JI_CustomsQuantity = 20;

				invoiceLine1.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine1, expectedMessageError);

				DoMerge();

				invoiceLine1.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(invoiceLine1, expectedMessageError);

				invoiceLine2.JI_Weight = 100;
				invoiceLine1.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine1, expectedMessageError);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			invoiceLine2.JI_Weight = 1;
			invoiceLine1.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine1, expectedMessageError);
		}

		void DoMerge()
		{
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entry.ResetTotalsAndCachedValues();
		}
	}

	public void TestCheckAdditionalSupplementaryCodes_MaximumNumber()
	{
		const string expectedMessageError = "[E1404] During the transition period, which is active now, only 2 additional codes are allowed";

		var invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When is UCC6 and Transition Period ON", () =>
				{
					invoiceLine.JI_SupplementaryCode1 = "SUP1";
					invoiceLine.JI_SupplementaryCode2 = "SUP2";
					invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP3";
					invoiceLine.Validation.ValidateAll();
					AssertHasRowMessageErrorContaining(invoiceLine, expectedMessageError);

					invoiceLine.JI_SupplementaryCode1 = "";
					invoiceLine.JI_SupplementaryCode2 = "";
					invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP4";
					invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP5";
					invoiceLine.Validation.ValidateAll();
					AssertHasRowMessageErrorContaining(invoiceLine, expectedMessageError);

					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.JI_SupplementaryCode1 = "SUP0";
					invoiceLine.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(invoiceLine, expectedMessageError);
				});
			}

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP4";
				invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP5";
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine, expectedMessageError);
			}
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, expectedMessageError);
		}
	}

	public void TestCheckJI_CustomsQuantity_HandlesRoundOffAndMismatchWarnings()
	{
		const string expectedWarningMessage = "Customs Quantity is usually equal to Net Weight";

		var invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When isUCC6 and Transition Period ON", () =>
				{
					invoiceLine.JI_NetWeight = 656786.555;
					invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;
					invoiceLine.JI_CustomsUnitQty = "KGM";
					invoiceLine.JI_CustomsQuantity = 0.657000m;
					AssertNoWarning("656786.555MG == 0.657000KG", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);
				});

				CombineAssertions("When isUCC6 and Transition Period ON with mismatch", () =>
				{
					invoiceLine.JI_CustomsQuantity = 0.658000m;
					AssertHasWarning("656786.555MG != 0.658000KG", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);
				});
			}
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				CombineAssertions("When isUCC6 and Transition Period OFF", () =>
				{
					invoiceLine.JI_NetWeight = 656786.555;
					invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;
					invoiceLine.JI_CustomsUnitQty = "KGM";
					invoiceLine.JI_CustomsQuantity = 0.656787m;
					AssertNoWarning("656786.555MG == 0.656787KG", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);
				});

				CombineAssertions("When isUCC6 and Transition Period OFF with mismatch", () =>
				{
					invoiceLine.JI_CustomsQuantity = 0.656788m;
					AssertHasWarning("656786.555MG != 0.656788KG", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);
				});
			}
		}
	}

	public void TestCheckUNDGMaximumNumber_DuringTransitionPeriod()
	{
		const string expectedMessageError = "[E1406] During the transition period, which is active now, only 1 Dangerous Goods code is allowed";

		var invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When is UCC6 and Transition Period ON", () =>
				{
					invoiceLine.UNDGs.TryGetOrCreate("3208A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
					invoiceLine.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(invoiceLine, expectedMessageError);

					invoiceLine.UNDGs.TryGetOrCreate("3208C", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
					invoiceLine.Validation.ValidateAll();
					AssertHasRowMessageErrorContaining(invoiceLine, expectedMessageError);
				});
			}

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine, expectedMessageError);
			}
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, expectedMessageError);
		}
	}

	public void TestCheckJI_WeightDecimalPrecision()
	{
		const string expectedMessageError = "[E1109] During the transition period, which is active now, [35] GWT converted in KG cannot have more than 3 decimals";

		var invoiceLine = declaration.Invoices
			.AddNew()
			.InvoiceLines
			.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When is EXP UCC6 and is in Transition Period", () =>
				{
					invoiceLine.JI_Weight = 1.3m;
					invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
					invoiceLine.Validation.ValidateJI_Weight();
					AssertHasMessageErrorContaining(invoiceLine.JI_WeightInfo, expectedMessageError);

					invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
					invoiceLine.Validation.ValidateJI_Weight();
					AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, expectedMessageError);
				});
			}

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
				invoiceLine.Validation.ValidateJI_Weight();
				AssertNoMessageErrorContaining("When is EXP UCC6 but not in Transition Period", invoiceLine.JI_WeightInfo, expectedMessageError);
			}
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageErrorContaining("When is not UCC6 and not in Transition Period", invoiceLine.JI_WeightInfo, expectedMessageError);
		}
	}

	public void TestGetMergedLineMissingPreviousDocumentsMessage()
	{
		var expectedMessage = "This invoice line's merged entry line has no previous documents. Without a previous document the entry may be rejected. Add one to any of the entry line's invoice line or to its entry instruction";
		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();
		var validation = new ExportJobComInvoiceLineValidationForTest(invoiceLine);

		CombineAssertions("GetMergedLineMissingPreviousDocumentsMessage", () =>
		{
			AssertNotEquals("For Non Ucc6 Export", expectedMessage, validation.GetMergedLineMissingPreviousDocumentsMessageExposed());

			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			{
				AssertEquals("For Ucc6 Export", expectedMessage, validation.GetMergedLineMissingPreviousDocumentsMessageExposed());
			}
		});
	}

	public void TestCheckJI_DescriptionMaximumLengthValidation_WhenUCC6IsOnAndTransitionPeriodIsOn()
	{
		const string expectedMessageError = "Goods Description exceeds the maximum allowed length in the declaration message (280 characters). Excess characters will be truncated.";
		const int expectedMaxLength = 280;

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		CombineAssertions("When Declaration is UCC6 ON and AESTP is ON", () =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
			using (TemporarilySetTransitionPeriod(true))
			{
				invoiceLine.JI_Description = new string('0', expectedMaxLength);
				AssertNoWarningContaining(invoiceLine.JI_DescriptionInfo, expectedMessageError);

				invoiceLine.JI_Description += " now maximum length has been exceeded";
				AssertHasWarningContaining(invoiceLine.JI_DescriptionInfo, expectedMessageError);
			}
		});
	}

	public void TestCheckJI_DescriptionMaximumLengthValidation_WhenUCC6IsOnButTransitionPeriodIsOff()
	{
		const string expectedMessageError = "Goods Description exceeds the maximum allowed length in the declaration message (512 characters). Excess characters will be truncated.";
		const int expectedMaxLength = 512;

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		CombineAssertions("When Declaration is UCC6 ON But AESTP is OFF", () =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
			using (TemporarilySetTransitionPeriod(false))
			{
				invoiceLine.JI_Description = new string('0', expectedMaxLength);
				AssertNoWarningContaining(invoiceLine.JI_DescriptionInfo, expectedMessageError);

				invoiceLine.JI_Description += " And More";
				AssertHasWarningContaining(invoiceLine.JI_DescriptionInfo, expectedMessageError);
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
	}

	JobDeclaration declaration;

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	IDisposable TemporarilySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	class ExportJobComInvoiceLineValidationForTest : ExportJobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidationForTest(JobComInvoiceLine parent) : base(parent)
		{
		}

		public string GetMergedLineMissingPreviousDocumentsMessageExposed()
			=> base.GetMergedLineMissingPreviousDocumentsMessage(invoiceHasDocs: true, invoiceSupportPrevDocs: true, declarationHasDocs: true, declarationSupportPrevDocs: true);
	}
}
