using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new UCCCustomsChargeTypeList();

			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Additions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Deductions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
			if (dec.IsImport)
			{
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InternationalFreight);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.UnloadingOfGoods);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.PortTransitFee);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TerminalHandlingCharge);
			}
			else
			{
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InternationalFreightExp);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TransportCostsUntilESBorder);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InsuranceUntilESBorder);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.OtherInternationalPayments);
				customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.OtherNationalPayments);
			}
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		public void TestDefault_OH_Supplier()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OA_SupplierAddress = ZGuid.BrettsGuid;
			invoice.JZ_OH_Supplier = org.PK;

			AssertEquals("OH_Supplier original value", org.PK, invoice.JZ_OH_Supplier);

			invoice.JZ_OA_SupplierAddress = ZGuid.Empty;

			AssertEquals("OH_Supplier value not changed", org.PK, invoice.JZ_OH_Supplier);
		}

		public void TestAdditionalInfos()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<AdditionalInfoCollection>(invoice.AdditionalInfos);
		}

		public void TestGroupCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<JobComInvApportionedChargeCollection<InvoiceApportionCharge>>(invoice.GroupCharges);
		}

		public void TestCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<InvoiceChargeCollection<InvoiceCharge>>(invoice.Charges);
		}

		public void TestSupportingDocuments()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertType<SupportingDocumentCollection>(invoice.SupportingDocuments);
		}

		public void TestMaxSupportingDocuments()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			AssertEquals("EnableMaxCountValidationWithMessageError is disabled", 99, invoice.MaxSupportingDocuments);
		}

		public void TestGetSupportingDocumentsMaxCountReduction()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("0 documents have been added to the dec or invoice, the reduction is 0", 0, invoice.GetSupportingDocumentsMaxCountReduction());

				var doc = dec.SupportingDocuments.AddNew();
				doc.CSI_Code = "doc";
				AssertEquals("A new document has been added to the dec, the reduction is 1", 1, invoice.GetSupportingDocumentsMaxCountReduction());

				var newInvoice = Factory.New<JobComInvoiceHeader>();
				AssertEquals("Header hasn't Declaration", null, newInvoice.JobDeclaration);
				AssertEquals("When the invoice has no declaratin the reduction is 0", 0, newInvoice.GetSupportingDocumentsMaxCountReduction());
			});
		}

		public void TestNeedAtLeastOneInvoiceSupportingDocument()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = dec.Invoices.AddNew();
			var supportingDocument = invoice.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = "AAA";
				AssertEquals("AAA", ZBool.True, invoice.NeedAtLeastOneInvoiceSupportingDocument);

				supportingDocument.CSI_Code = "N380";
				AssertEquals("N380", ZBool.False, invoice.NeedAtLeastOneInvoiceSupportingDocument);

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				var entryHeader = dec.CustomsEntryHeaders[0];
				entryHeader.ValidationMode = ValidationModes.None;
				supportingDocument.CSI_Code = "AAA";
				AssertEquals("AAA with validation mode All", ZBool.True, invoice.NeedAtLeastOneInvoiceSupportingDocument);

				entryHeader.ValidationMode = ValidationModes.PDI;
				supportingDocument.CSI_Code = "AAA";
				AssertEquals("AAA with validation mode PDI", ZBool.False, invoice.NeedAtLeastOneInvoiceSupportingDocument);
			});
		}

		public void TestEnableMaxCountValidationWithMessageError()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			for (int i = 0; i < 20; i++)
			{
				var doc = dec.SupportingDocuments.AddNew();
				doc.CSI_Code = $"doc{i}";
			}
			for (int i = 0; i < 79; i++)
			{
				var doc = invoice.SupportingDocuments.AddNew();
				doc.CSI_Code = $"dc{i}";
			}

			CombineAssertions(() =>
			{
				AssertEquals(20, dec.SupportingDocuments.Count);
				AssertEquals(79, invoice.SupportingDocuments.Count);
				AssertNoRowMessageErrorContaining(dec.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
				AssertNoRowMessageErrorContaining(invoice.SupportingDocuments[78], "Customs will not accept a declaration with more than 99 documents per line");

				var newDoc = invoice.SupportingDocuments.AddNew();
				newDoc.CSI_Code = "mydoc";
				AssertEquals(20, dec.SupportingDocuments.Count);
				AssertEquals(80, invoice.SupportingDocuments.Count);
				AssertNoRowMessageErrorContaining(dec.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
				AssertHasRowMessageErrorContaining(invoice.SupportingDocuments[79], "Customs will not accept a declaration with more than 99 documents per line");
			});
		}

		public void TestEnableMaxCountValidationWithMessageErrorDynamicCounting()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			for (int i = 0; i < 20; i++)
			{
				var doc = dec.SupportingDocuments.AddNew();
				doc.CSI_Code = $"doc{i}";
			}
			for (int i = 0; i < 79; i++)
			{
				var doc = invoice.SupportingDocuments.AddNew();
				doc.CSI_Code = $"dc{i}";
			}

			CombineAssertions(() =>
			{
				AssertEquals(20, dec.SupportingDocuments.Count);
				AssertEquals(79, invoice.SupportingDocuments.Count);
				AssertNoRowMessageErrorContaining(dec.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
				AssertNoRowMessageErrorContaining(invoice.SupportingDocuments[78], "Customs will not accept a declaration with more than 99 documents per line");

				var newDoc = dec.SupportingDocuments.AddNew();
				newDoc.CSI_Code = "mydoc";
				AssertEquals(21, dec.SupportingDocuments.Count);
				AssertEquals(79, invoice.SupportingDocuments.Count);
				invoice.SupportingDocuments.RunPreSaveValidation();
				AssertNoRowMessageErrorContaining(dec.SupportingDocuments[20], "Customs will not accept a declaration with more than 99 documents per line");
				AssertHasRowMessageErrorContaining(invoice.SupportingDocuments[78], "Customs will not accept a declaration with more than 99 documents per line");
			});
		}

		public void TestAddNewSupportingDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("No supporting documents in invocieLine", false, invoice.SupportingDocuments.Any());

				invoice.AddNewSupportingDocument("AAA", "reference");
				AssertEquals("1 supporting documents in invoice after calling method", 1, invoice.SupportingDocuments.Count);
				AssertEquals("SupportingDocument CSI_Code", "AAA", invoice.SupportingDocuments[0].CSI_Code);
				AssertEquals("SupportingDocument CSI_ReferenceNumber", "reference", invoice.SupportingDocuments[0].CSI_ReferenceNumber);
			});
		}

		public override void TestChargeTypeList()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

				var customsChargeTypeList = GetExpectedCustomsChargeTypeList();
				AssertEquals("Import Expected Charge Code List", customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

				dec = (JobDeclaration)GetNewDeclaration();
				dec.JE_MessageType = MessageTypeList.Codes.Export;
				commonInvoice = dec.Invoices.AddNew();
				chargeTypeList1 = commonInvoice.ChargeTypeList;
				customsChargeTypeList = GetExpectedCustomsChargeTypeList();
				AssertEquals("Export Expected Charge Code List", customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
			});
		}

		public override void TestCFRCalculationWithFreightAdjustedFlag()
		{
			dec = (JobDeclaration)GetNewDeclaration();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice.JZ_InvoiceAmount = 10500m;
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = OverseasFreightCode;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			var adjustedOFT = invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = OverseasFreightCode;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsIncludedInITOT = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);

			oFT.J7_IsIncludedInITOT = true;
			adjustedOFT.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10500m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 10500m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 10500m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestHasT2LOrT2CLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			CombineAssertions(() =>
			{
				AssertEquals("HasT2LOrT2CLine is false when invoice lines are both A", false, invoice.HasT2LOrT2CLine());

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				AssertEquals("HasT2LOrT2CLine is true when one invoice line is T2L", true, invoice.HasT2LOrT2CLine());

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				AssertEquals("HasT2LOrT2CLine is false when invoice lines are A/B", false, invoice.HasT2LOrT2CLine());

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertEquals("HasT2LOrT2CLine is true when one invoice line is T2C", true, invoice.HasT2LOrT2CLine());
			});
		}

		public void TestHasAnyDiffEXSEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			CombineAssertions(() =>
			{
				AssertEquals("All invoice lines are EXS", false, invoice.HasAnyDiffEXSEntry());

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				AssertEquals("Invoice lines are T2L/EXS", true, invoice.HasAnyDiffEXSEntry());

				invoiceLine1.JI_CEI = ZGuid.Empty;
				AssertEquals("Entry Instruction T2L exists but is not associated", false, invoice.HasAnyDiffEXSEntry());
			});
		}

		public void TestHasAnyDiffT2CAndT2LAndEXSEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Invoice lines are both T2L", false, invoice.HasAnyDiffT2CAndT2LAndEXSEntry());

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertEquals("Invoice lines are both T2C", false, invoice.HasAnyDiffT2CAndT2LAndEXSEntry());

				entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				AssertEquals("Invoice lines are both EXS", false, invoice.HasAnyDiffT2CAndT2LAndEXSEntry());

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("One invoice line is A", true, invoice.HasAnyDiffT2CAndT2LAndEXSEntry());

				invoiceLine1.JI_CEI = ZGuid.Empty;
				AssertEquals("Entry Instruction A exists but is not associated", false, invoice.HasAnyDiffT2CAndT2LAndEXSEntry());
			});
		}

		public void TestHasAnyDiffT2CAndT2lAndEXSAndBAndCEntry()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = ZGuid.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("Entry is only one and is A", true, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				AssertEquals("Entry is only one and is EXS but no associated", true, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());
				invoiceLine1.JI_CEI = entryInstruction.PK;

				AssertEquals("Entry is only one and is EXS and it is associated", false, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertEquals("Entry is only one and is T2C", false, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("there are two Entry and A is not associated", false, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction1.PK;
				AssertEquals("there are two Entry and A is associated", true, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("there are two Entry and all have A", true, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				AssertEquals("there are two Entry and one have EXS", true, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertEquals("there are two Entry and one have EXS and other T2C", false, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertEquals("there are two Entry and one have T2C", true, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				AssertEquals("there are two Entry and one have B", true, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;
				AssertEquals("there are two Entry and one have C and other B", false, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.C;
				AssertEquals("there are two Entry and one have C", true, invoice.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry());
			});
		}

		public void TestJZ_InvoiceCurrExRateIsNotReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("JZ_InvoiceCurrExRateInfo is not ReadOnly", false, invoice.JZ_InvoiceCurrExRateInfo.ReadOnly);

				invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
				AssertEquals("JZ_InvoiceCurrExRateInfo is not ReadOnly if there is Currency set", false, invoice.JZ_InvoiceCurrExRateInfo.ReadOnly);
			});
		}

		public void TestGetNewCurrencyConverter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			AssertType<CurrencyConverterWithFixedExchangeRatesDataProvider>("CurrencyConverter is the ES CurrencyConverterWithFixedExchangeRatesDataProvider", invoice.CurrencyConverter);
		}

		public void TestJZ_OH_Buyer() => CombineAssertions(() =>
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OA_BuyerAddress = ZGuid.BrettsGuid;
			invoice.JZ_OH_Buyer = org.PK;

			AssertEquals("JZ_OH_Buyer original value", org.PK, invoice.JZ_OH_Buyer);
			AssertEquals("JZ_OH_Buyer equals to JZ_OA_BuyerAddress_ZAddress.OrgPK", invoice.JZ_OA_BuyerAddress_ZAddress.OrgPK, invoice.JZ_OH_Buyer);

			invoice.JZ_OA_SupplierAddress = ZGuid.Empty;

			AssertEquals("JZ_OH_Buyer value not changed", org.PK, invoice.JZ_OH_Buyer);
			AssertEquals("JZ_OH_Buyer equals to JZ_OA_BuyerAddress_ZAddress.OrgPK", invoice.JZ_OA_BuyerAddress_ZAddress.OrgPK, invoice.JZ_OH_Buyer);
		});

		public void TestJZ_OH_Buyer_CaptionKeyExportUCC6() => CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OH_BuyerInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption", "Buyer", captionResourceString.Caption);
			AssertEquals("FullDescription", "[13 09 016 000] Buyer Name", captionResourceString.FullDescription);
		});

		public void TestJZ_InvoiceAmount_CaptionKeyExportUCC6()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption", "Invoice Amount", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Inv. Amount", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Inv. Amount", captionResourceString.ShortCaption);
			});
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Spain;

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection<InvoiceCharge>);

		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<JobDeclaration>();
		}

		JobDeclaration dec;
	}
}
