using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

sealed class EntryCreationStrategyTest : EU.Business.Declaration.Testing.EntryCreationStrategyTest
{
	public void TestMergeKeyForLine()
	{
		var invoice = declaration.Invoices.AddNew();
		var invLineImportAndGeneric = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
		var invLineExport = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Export);
		var invLineExportUCC6 = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Export);
		var mergeStrategy = declaration.CreateEntryCreationStrategy();
		CombineAssertions(() =>
		{
			Assert("DI_DG is in the merge key", mergeStrategy.GetKeyForLine(invLineExport).Contains(invLineExport.UNDGs.FirstItemForBinding[0].DI_DG));
			Assert("JI_StateOrRegionOfOrigin is in the merge key", mergeStrategy.GetKeyForLine(invLineExport).Contains(invLineExport.JI_StateOrRegionOfOrigin));

			Assert("JI_FormattedTariff is in the merge key", mergeStrategy.GetKeyForLine(invLineImportAndGeneric).Contains(invLineImportAndGeneric.JI_FormattedTariff));
			var suppCode1 = invLineImportAndGeneric.JI_SupplementaryCode1;
			var suppCode2 = invLineImportAndGeneric.JI_SupplementaryCode2;
			var suppCode3 = invLineImportAndGeneric.AdditionalSupplementaryCodes.AsString;
			var mergeKey = mergeStrategy.GetKeyForLine(invLineImportAndGeneric);
			Assert("JI_SupplementaryCode1 and JI_SupplementaryCode2 are in the merge key", mergeKey.Contains(new ZString($"{suppCode1}_{suppCode2}_{suppCode3}")));
			Assert("JI_CountryOfOrigin is in the merge key", mergeKey.Contains(invLineImportAndGeneric.JI_CountryOfOrigin));
			Assert("JI_FormattedProcedure is in the merge key", mergeKey.Contains(invLineImportAndGeneric.JI_FormattedProcedure));
			Assert("JI_CustomsSecondUnitQty is in the merge key", mergeKey.Contains(invLineImportAndGeneric.JI_CustomsSecondUnitQty));
			Assert("JI_CustomsThirdUnitQty is in the merge key", mergeKey.Contains(invLineImportAndGeneric.JI_CustomsThirdUnitQty));
			Assert("JI_ConcessionOrder is in the merge key", mergeKey.Contains(invLineImportAndGeneric.JI_ConcessionOrder));
			Assert("JI_PrimaryPreference is in the merge key", mergeKey.Contains(invLineImportAndGeneric.JI_PrimaryPreference));
			Assert("ZG_REAProductCode is in the merge key", mergeKey.Contains(invLineImportAndGeneric.ZG_REAProductCode));
			Assert("ZG_ExciseExemption is in the merge key", mergeKey.Contains(invLineImportAndGeneric.ZG_ExciseExemption));
			Assert("JI_ZZF_NKTaxType is in the merge key", mergeKey.Contains(invLineImportAndGeneric.JI_ZZF_NKTaxType));
			Assert("ZG_AIEMType is in the merge key", mergeKey.Contains(invLineImportAndGeneric.ZG_AIEMType));
			Assert("ZG_TotalRetailPrice is in the merge key", mergeKey.Contains(invLineImportAndGeneric.ZG_TotalRetailPrice));
			Assert("ZG_ExciseCode is in the merge key", mergeKey.Contains(invLineImportAndGeneric.ZG_ExciseCode));
			Assert("ZG_IsREADirectConsumption is in the merge key", mergeKey.Contains(invLineImportAndGeneric.ZG_IsREADirectConsumption));
			Assert("ZG_HasNonRecycledPlastics is in the merge key", mergeKey.Contains(invLineImportAndGeneric.ZG_HasNonRecycledPlastics));
			Assert("ZG_GlobalWarmingPotential is in the merge key", mergeKey.Contains(invLineImportAndGeneric.ZG_GlobalWarmingPotential));

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				cusEntryInstruction.CEI_SubStyle = "A";
				invLineExportUCC6.JI_CEI = cusEntryInstruction.PK;
				mergeStrategy = declaration.CreateEntryCreationStrategy();
				mergeKey = mergeStrategy.GetKeyForLine(invLineExportUCC6);
				Assert("Consignor - JI_OA_ExporterAddress is in the merge key", mergeKey.Contains(invLineExportUCC6.JI_OA_ExporterAddress));
			}
		});
	}

	public void TestExciseExemptionEmptyEquals0()
	{
		var invoice = declaration.Invoices.AddNew();
		var invLine1 = invoice.JobComInvoiceLines.AddNew();
		var invLine2 = invoice.JobComInvoiceLines.AddNew();

		invLine1.ZG_ExciseExemption = "0";
		invLine2.ZG_ExciseExemption = "1";

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		CombineAssertions(() =>
		{
			AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("2 Expected CusEntryLines after merge", 2, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);

			invLine1.ZG_ExciseExemption = "0";
			invLine2.ZG_ExciseExemption = ZString.Empty;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("1 Expected CusEntryLines after merge", 1, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestSortedKeys()
	{
		var invoice = declaration.Invoices.AddNew();
		var invLine1 = invoice.JobComInvoiceLines.AddNew();
		var invLine2 = invoice.JobComInvoiceLines.AddNew();
		invLine1.JI_SupplementaryCode1 = "First";
		invLine1.JI_SupplementaryCode2 = "Second";
		invLine2.JI_SupplementaryCode1 = "Second";
		invLine2.JI_SupplementaryCode2 = "First";

		var mergeStrategy = declaration.CreateEntryCreationStrategy();
		var keyForLine1 = mergeStrategy.GetKeyForLine(invLine1);
		var keyForLine2 = mergeStrategy.GetKeyForLine(invLine2);
		AssertEquals("Mergekeys are equals", keyForLine1, keyForLine2);
	}

	readonly string[] genericKeysToTest = { "JI_SupplementaryCode1", "JI_SupplementaryCode2", "JI_FormattedTariff",
									"JI_CountryOfOrigin", "JI_FormattedProcedure", "JI_CustomsSecondUnitQty", "JI_CustomsThirdUnitQty" };

	readonly string[] importKeysToTest = { "JI_ConcessionOrder", "JI_PrimaryPreference", "ZG_REAProductCode", "ZG_ExciseExemption", "JI_ZZF_NKTaxType", "ZG_AIEMType",
									"ZG_ExciseCode" };

	readonly string[] exportKeysToTest = { "JI_StateOrRegionOfOrigin" };

	public void TestKeysGenerateDifferentMergeKeys()
	{
		CombineAssertions(() =>
		{
			var mergeStrategy = declaration.CreateEntryCreationStrategy();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "123a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var invoice = declaration.Invoices.AddNew();
			var invLine1 = GetInvoiceLineForTesting(invoice, ZString.Empty);
			AssertKeysAreDifferent(invLine1, genericKeysToTest);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invLine1 = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
			AssertKeysAreDifferent(invLine1, importKeysToTest);

			var differentInvoiceImport = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
			differentInvoiceImport.ZG_TotalRetailPrice = 10m;
			AssertNotEquals("ZG_TotalRetailPrice shouldn't be equals", mergeStrategy.GetKeyForLine(invLine1), mergeStrategy.GetKeyForLine(differentInvoiceImport));

			differentInvoiceImport = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
			differentInvoiceImport.ZG_IsREADirectConsumption = true;
			AssertNotEquals("ZG_IsREADirectConsumption shouldn't be equals", mergeStrategy.GetKeyForLine(invLine1), mergeStrategy.GetKeyForLine(differentInvoiceImport));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invLine1 = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Export);
			AssertKeysAreDifferent(invLine1, exportKeysToTest);

			var differentInvoice = GetInvoiceLineForTesting(invoice, Customs.Business.JobMessageTypeList.Codes.Export);
			differentInvoice.UNDGs.FirstItemForBinding[0].LinkDefault(subs);
			AssertNotEquals(invLine1, differentInvoice);
		});
	}

	public void AssertKeysAreDifferent(JobComInvoiceLine invLine1, string[] keysToTest)
	{
		var messageType = invLine1.MessageType;
		var invoice = invLine1.InvoiceHeader;
		var mergeStrategy = declaration.CreateEntryCreationStrategy();
		foreach (var key in keysToTest)
		{
			var differentInvoice = GetInvoiceLineForTesting(invoice, messageType);
			if (key == nameof(differentInvoice.JI_ZZF_NKTaxType))
			{
				var taxOrFeeDetailEntity = new EU.Business.Declaration.TaxOrFeeDetailEntity();
				taxOrFeeDetailEntity.Code = new ZString("B");
				differentInvoice.Lookups.TaxOrFeeDetailEntities.Add(taxOrFeeDetailEntity);
				differentInvoice.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			}
			else
			{
				differentInvoice.GetType().GetProperty(key).SetValue(differentInvoice, new ZString("B"));
			}
			AssertNotEquals($"{key} shouldn't be equals", mergeStrategy.GetKeyForLine(invLine1), mergeStrategy.GetKeyForLine(differentInvoice));
		}
	}

	public void TestJI_ProcedureWhenIsOutofOutwardProcessing()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP");
		procedure1.ZZ6_OutofOutwardProcessing = "Y";
		var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP");
		procedure2.ZZ6_OutofOutwardProcessing = "N";

		var mergeStrategy = declaration.CreateEntryCreationStrategy();

		var invoice = declaration.Invoices.AddNew();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invLine1 = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
		invLine1.JI_Procedure = "1111111";
		var invLine2 = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
		invLine2.JI_Procedure = "1111111";
		var invLine3 = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
		invLine3.JI_Procedure = "2222222";
		var invLine4 = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
		invLine4.JI_Procedure = "2222222";
		var invLine5 = GetInvoiceLineForTesting(invoice, JobMessageTypeList.Codes.Import);
		invLine5.JI_Procedure = "2222222";

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		CombineAssertions(() =>
		{
			AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("3 Expected CusEntryLines after merge", 3, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);

			AssertNotEquals("invLine1 is not merged with other lines so MergedLineNumber is not the same as invLine2", invLine1.MergedLineNumber, invLine2.MergedLineNumber);
			AssertNotEquals("invLine1 is not merged with other lines so MergedLineNumber is not the same as invLine3", invLine1.MergedLineNumber, invLine3.MergedLineNumber);
			AssertNotEquals("invLine1 is not merged with other lines so MergedLineNumber is not the same as invLine4", invLine1.MergedLineNumber, invLine4.MergedLineNumber);
			AssertNotEquals("invLine1 is not merged with other lines so MergedLineNumber is not the same as invLine5", invLine1.MergedLineNumber, invLine5.MergedLineNumber);

			AssertNotEquals("invLine2 is not merged with other lines so MergedLineNumber is not the same as invLine1", invLine2.MergedLineNumber, invLine1.MergedLineNumber);
			AssertNotEquals("invLine2 is not merged with other lines so MergedLineNumber is not the same as invLine3", invLine2.MergedLineNumber, invLine3.MergedLineNumber);
			AssertNotEquals("invLine2 is not merged with other lines so MergedLineNumber is not the same as invLine4", invLine2.MergedLineNumber, invLine4.MergedLineNumber);
			AssertNotEquals("invLine2 is not merged with other lines so MergedLineNumber is not the same as invLine5", invLine2.MergedLineNumber, invLine5.MergedLineNumber);

			AssertEquals("invLine3 is merged with other lines so MergedLineNumber is the same as invLine4", invLine3.MergedLineNumber, invLine4.MergedLineNumber);
			AssertEquals("invLine4 is merged with other lines so MergedLineNumber is the same as invLine5", invLine4.MergedLineNumber, invLine5.MergedLineNumber);
		});
	}

	public void TestPreviousDocuments()
	{
		var invoice = declaration.Invoices.AddNew();
		var invoice2 = declaration.Invoices.AddNew();

		var invLineDec1 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		var invLineDec2 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		var invLineHeader1 = GetInvoiceLineForTesting(invoice2, ZString.Empty);
		var invLineHeader2 = GetInvoiceLineForTesting(invoice2, ZString.Empty);
		var invLine1 = GetInvoiceLineForTesting(invoice2, ZString.Empty);
		var invLine2 = GetInvoiceLineForTesting(invoice2, ZString.Empty);
		declaration.PreviousDocuments.AddNew().CSI_Code = "AA";
		invoice2.PreviousDocuments.AddNew().CSI_Code = "BB";
		invLine1.PreviousDocuments.AddNew().CSI_Code = "CC";
		invLine2.PreviousDocuments.AddNew().CSI_Code = "CC";

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		CombineAssertions(() =>
		{
			AssertEquals("1 Expected CusEntryHeader after merge", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("3 Expected CusEntryLines after merge", 3, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestSupportingDocuments()
	{
		var invoice = declaration.Invoices.AddNew();
		var invLine1 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		var invLine2 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		var invLine3 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		var invLine4 = GetInvoiceLineForTesting(invoice, ZString.Empty);

		for (int i = 0; i < 20; i++)
		{
			var document = declaration.SupportingDocuments.AddNew();
			document.CSI_Code = $"DOC{i}";

			document = invoice.SupportingDocuments.AddNew();
			document.CSI_Code = $"DOC{i + 60}";
		}
		for (int i = 0; i < 30; i++)
		{
			var document = invLine1.SupportingDocuments.AddNew();
			document.CSI_Code = $"DC{i}";

			document = invLine2.SupportingDocuments.AddNew();
			document.CSI_Code = $"DC{i + 60}";

			document = invLine4.SupportingDocuments.AddNew();
			document.CSI_Code = $"DC{i}";
		}
		for (int i = 0; i < 10; i++)
		{
			var document = invLine3.SupportingDocuments.AddNew();
			document.CSI_Code = $"TST{i}";
		}
		CombineAssertions(() =>
		{
			AssertEquals("Prereq Declaration has 20 Supporting Documents", 20, declaration.SupportingDocuments.Count);
			AssertEquals("Prereq Invoice has 20 Supporting Documents", 20, invoice.SupportingDocuments.Count);
			AssertEquals("Prereq InvLine1 has 30 Supporting Documents", 30, invLine1.SupportingDocuments.Count);
			AssertEquals("Prereq InvLine2 has 30 Supporting Documents", 30, invLine2.SupportingDocuments.Count);
			AssertEquals("Prereq InvLine3 has 10 Supporting Documents", 10, invLine3.SupportingDocuments.Count);
			AssertEquals("Prereq InvLine4 has 30 Supporting Documents", 30, invLine4.SupportingDocuments.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Available EntryLines equals to 3 ((1 & 4); 2; 3)", 3, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestSupportingDocumentsEXS()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		var invoice = declaration.Invoices.AddNew();
		var invLine1 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		invLine1.JI_CEI = entryInstruction.PK;
		var invLine2 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		invLine2.JI_CEI = entryInstruction.PK;
		var invLine3 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		invLine3.JI_CEI = entryInstruction.PK;
		var invLine4 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		invLine4.JI_CEI = entryInstruction.PK;

		for (int i = 0; i < 2; i++)
		{
			var document = declaration.SupportingDocuments.AddNew();
			document.CSI_Code = $"DOC{i}";

			document = invoice.SupportingDocuments.AddNew();
			document.CSI_Code = $"DOC{i + 6}";
		}
		for (int i = 0; i < 6; i++)
		{
			var document = invLine1.SupportingDocuments.AddNew();
			document.CSI_Code = $"DC{i}";

			document = invLine2.SupportingDocuments.AddNew();
			document.CSI_Code = $"DC{i + 6}";

			document = invLine4.SupportingDocuments.AddNew();
			document.CSI_Code = $"DC{i}";
		}
		for (int i = 0; i < 9; i++)
		{
			var document = invLine3.SupportingDocuments.AddNew();
			document.CSI_Code = $"TST{i}";
		}
		CombineAssertions(() =>
		{
			AssertEquals("Prereq Declaration has 2 Supporting Documents", 2, declaration.SupportingDocuments.Count);
			AssertEquals("Prereq Invoice has 2 Supporting Documents", 2, invoice.SupportingDocuments.Count);
			AssertEquals("Prereq InvLine1 has 6 Supporting Documents", 6, invLine1.SupportingDocuments.Count);
			AssertEquals("Prereq InvLine2 has 6 Supporting Documents", 6, invLine2.SupportingDocuments.Count);
			AssertEquals("Prereq InvLine3 has 9 Supporting Documents", 9, invLine3.SupportingDocuments.Count);
			AssertEquals("Prereq InvLine4 has 6 Supporting Documents", 6, invLine4.SupportingDocuments.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Available EntryLines equals to 3 ((1 & 4); 2; 3) cause 1 & 4 has less than 10 documents", 3, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);

			for (int i = 0; i < 6; i++)
			{
				var document = invLine1.SupportingDocuments.AddNew();
				document.CSI_Code = $"DCT{i}";

				document = invLine4.SupportingDocuments.AddNew();
				document.CSI_Code = $"DCT{i}";
			}
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Available EntryLines equals to 3 (1; 2; 3; 4) cause 1 & 4 has more than 10 documents", 4, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestVIN()
	{
		var invoice = declaration.Invoices.AddNew();
		for (var i = 0; i < 100; i++)
		{
			var invLine = invoice.InvoiceLines.AddNew();
			var vehicle = invLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = $"VIN{i}";
		}

		CombineAssertions(() =>
		{
			AssertEquals("Prereq 100 InvoiceLines availables", 100, invoice.InvoiceLines.Count);
			var mergeKeyForLine1 = declaration.CreateEntryCreationStrategy().GetKeyForLine(invoice.InvoiceLines[0]);
			var mergeKeyForLine2 = declaration.CreateEntryCreationStrategy().GetKeyForLine(invoice.InvoiceLines[99]);
			AssertEquals("All keys are equals", mergeKeyForLine1, mergeKeyForLine2);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("1 Entry created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("2 EntryLines created", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("First EntryLine has 99 lines", 99, declaration.CustomsEntryHeaders[0].AllEntryLines[0].InvoiceLines.Count);
			AssertEquals("Second EntryLine has 1 line", 1, declaration.CustomsEntryHeaders[0].AllEntryLines[1].InvoiceLines.Count);
		});
	}

	public void TestVINMultipleInvoicesLines()
	{
		var invoice = declaration.Invoices.AddNew();
		for (var i = 0; i < 99; i++)
		{
			var invLine = invoice.InvoiceLines.AddNew();
			var vehicle = invLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = $"VIN{i}";
		}

		CombineAssertions(() =>
		{
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Less 99 vehicles, 1 EntryHeader created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Less 99 vehicles, 1 EntryLine created", 1, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("Less 99 vehicles, first EntryLine number", (ZShort)1, declaration.CustomsEntryHeaders[0].AllEntryLines[0].CL_LineNumber);

			var invLine2 = invoice.InvoiceLines.AddNew();
			var vehicle2 = invLine2.Vehicles.AddNew();
			vehicle2.CVH_VehicleIdentificationNumber = "VIN1";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("More 99 vehicles, 1 EntryHeader created", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("More 99 vehicles, 2 EntryLine created", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("More 99 vehicles, First EntryLine number", (ZShort)1, declaration.CustomsEntryHeaders[0].AllEntryLines[0].CL_LineNumber);
			AssertEquals("More 99 vehicles, Second EntryLine number", (ZShort)2, declaration.CustomsEntryHeaders[0].AllEntryLines[1].CL_LineNumber);
		});
	}

	public void TestVINWhenSecondMergeAndStatusIsNotPDI()
	{
		var invoice = declaration.Invoices.AddNew();
		var invLine = invoice.InvoiceLines.AddNew();
		for (var i = 0; i < 50; i++)
		{
			var vehicle = invLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = $"VIN{i}";
		}

		CombineAssertions(() =>
		{
			AssertEquals("Prereq 50 vehicles in the invoice line", 50, invLine.Vehicles.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("1 Entry created", 1, declaration.CustomsEntryHeaders.Count);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("1 EntryLine created", 1, entryHeader.AllEntryLines.Count);
			AssertEquals("EntryLine has 1 line", 1, entryHeader.AllEntryLines[0].InvoiceLines.Count);

			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			entryHeader.CH_BGMReference = "ES001";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNoRowError("Merging existing entry having status PDI when there are no changes", invLine, "Entry Declared (ES001) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.");
			AssertEquals("1 Entry created when PDI", 1, declaration.CustomsEntryHeaders.Count);
			entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("1 EntryLine created when PDI", 1, entryHeader.AllEntryLines.Count);
			AssertEquals("EntryLine has 1 line when PDI", 1, entryHeader.AllEntryLines[0].InvoiceLines.Count);

			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNoRowError("Merging existing entry having status PDA when there are no changes", invLine, "Entry Declared (ES001) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.");
			AssertEquals("1 Entry created when PDA", 1, declaration.CustomsEntryHeaders.Count);
			entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("1 EntryLine created when PDA", 1, entryHeader.AllEntryLines.Count);
			AssertEquals("EntryLine has 1 line when PDA", 1, entryHeader.AllEntryLines[0].InvoiceLines.Count);
		});
	}

	public void TestPackagesTypes()
	{
		var invoice = declaration.Invoices.AddNew();
		declaration.JE_MasterBill = "X";
		var bill = declaration.PrimaryMasterBill;
		var invLine1 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		invLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = false;
		var invLine2 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		invLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = false;
		for (int i = 0; i < 9; i++)
		{
			var package = bill.PackingGroups[0].Packages.AddNew();
			package.CW_PackType = $"A{i}";
			invLine1.PackagesForInvoiceLinesForBindingOnly[i + 1].IsLinked = true;
		}
		var package2 = bill.PackingGroups[0].Packages.AddNew();
		invLine2.PackagesForInvoiceLinesForBindingOnly[10].IsLinked = true;
		package2.CW_PackType = $"AX";

		CombineAssertions(() =>
		{
			AssertEquals("Prereq Available Packages equals to 9 for invLine1", 9, invLine1.PackagesPivot.Count);
			AssertEquals("Prereq Available Packages equals to 1 for invLine2", 1, invLine2.PackagesPivot.Count);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Available EntryLines equals to 2 due to max packages types per entryLine are 9", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestPackagesTypesEXS()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		var invoice = declaration.Invoices.AddNew();
		declaration.JE_MasterBill = "X";
		var bill = declaration.PrimaryMasterBill;
		var invLine1 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		invLine1.JI_CEI = entryInstruction.PK;
		invLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = false;
		var invLine2 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		invLine2.JI_CEI = entryInstruction.PK;
		invLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = false;
		for (int i = 0; i < 99; i++)
		{
			var package = bill.PackingGroups[0].Packages.AddNew();
			package.CW_PackType = $"A{i}";
			invLine1.PackagesForInvoiceLinesForBindingOnly[i + 1].IsLinked = true;
		}
		var package2 = bill.PackingGroups[0].Packages.AddNew();
		invLine2.PackagesForInvoiceLinesForBindingOnly[100].IsLinked = true;
		package2.CW_PackType = $"AX";

		CombineAssertions(() =>
		{
			AssertEquals("Prereq Available Packages equals to 99 for invLine1", 99, invLine1.PackagesPivot.Count);
			AssertEquals("Prereq Available Packages equals to 1 for invLine2", 1, invLine2.PackagesPivot.Count);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Available EntryLines equals to 2 due to max packages types per entryLine are 99 for EXS", 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestMaxContainers()
	{
		declaration.JE_MasterBill = "X";
		var invoice = declaration.Invoices.AddNew();
		var invLine1 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		var invLine2 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		var invLine3 = GetInvoiceLineForTesting(invoice, ZString.Empty);

		for (int i = 0; i < 55; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = $"CONTAINER{i}";
			container.InvoiceLinePivotCollection.Add(invLine1.ContainersPivot.AddPivotFor(container));

			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = $"CONTAINER{i + 60}";

			container.InvoiceLinePivotCollection.Add(invLine2.ContainersPivot.AddPivotFor(container));
			if (i < 30)
			{
				container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = $"CONTAINER{i + 6000}";

				container.InvoiceLinePivotCollection.Add(invLine3.ContainersPivot.AddPivotFor(container));
			}
		}
		CombineAssertions(() =>
		{
			AssertEquals("Prereq Available Containers equals to 55 for invLine1", 55, invLine1.ContainersPivot.Count);
			AssertEquals("Prereq Available Containers equals to 55 for invLine2", 55, invLine2.ContainersPivot.Count);

			AssertEquals("Prereq Available Containers equals to 30 for invLine3", 30, invLine3.ContainersPivot.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Available EntryLines equals to 2 due to containers split the entry on 2"
				, 2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public void TestMaxContainersWithDuplicatedContainersOnInvLines()
	{
		var invoice = declaration.Invoices.AddNew();
		var invLine1 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		var invLine2 = GetInvoiceLineForTesting(invoice, ZString.Empty);
		for (int i = 0; i < 55; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = $"CONTAINER{i}";

			invLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(container.CO_ContainerNumber).IsForInvoiceLine = true;
			invLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(container.CO_ContainerNumber).IsForInvoiceLine = true;
		}
		CombineAssertions(() =>
		{
			AssertEquals("Prereq Available Containers equals to 55 for invLine1", 55, invLine1.ContainersPivot.Count);
			AssertEquals("Prereq Available Containers equals to 55 for invLine2", 55, invLine2.ContainersPivot.Count);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Available EntryLines equals to 1 due to containers are equals on both lines", 1, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
		});
	}

	public override void TestCanCreateEntryLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var instrunction = Factory.New<CusEntryInstruction>();
		instrunction.CEI_JE = declaration.PK;
		entryHeader.CH_CEI_Instruction = instrunction.PK;
		invoiceLine.JI_CEI = instrunction.PK;
		CombineAssertions(() =>
		{
			var strategy = new EntryCreationStrategy(declaration);
			AssertEquals(true, strategy.CanCreateEntryLine(entryHeader, invoiceLine));

			entryHeader.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.Cleared;
			AssertEquals(false, strategy.CanCreateEntryLine(entryHeader, invoiceLine));
			AssertEquals(true, invoiceLine.ShouldKeepNotAllowCreateNewEntryLineError);
			AssertHasRowErrorContaining(invoiceLine, "Cannot add new Entry lines to a Declared or Canceled Entry");
			invoiceLine.Validation.ValidateAll();
			AssertHasRowErrorContaining(invoiceLine, "Cannot add new Entry lines to a Declared or Canceled Entry");

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(true, strategy.CanCreateEntryLine(entryHeader2, invoiceLine));
			AssertEquals(false, invoiceLine.ShouldKeepNotAllowCreateNewEntryLineError);
			invoiceLine.Validation.ValidateAll();
			AssertNoRowErrorContaining(invoiceLine, "Cannot add new Entry lines to a Declared or Canceled Entry");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
	}

	JobDeclaration declaration;

	protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest() => declaration;

	JobComInvoiceLine GetInvoiceLineForTesting(JobComInvoiceHeader invoice, ZString messageType)
	{
		var invLine = invoice.JobComInvoiceLines.AddNew();
		invLine.JI_FormattedTariff = "AAE";
		invLine.JI_SupplementaryCode1 = "AAA";
		invLine.JI_SupplementaryCode2 = "AAB";
		invLine.JI_CountryOfOrigin = "AF";
		invLine.JI_FormattedProcedure = "AAG";
		invLine.JI_CustomsSecondUnitQty = "AAH";
		invLine.JI_CustomsThirdUnitQty = "AAI";
		invLine.ZG_CommercialReference = "ZZFF";
		invLine.JI_OA_ExporterAddress = new ZGuid("fff12345-6789-ffff-ffff-fff123456789");
		if (messageType == JobMessageTypeList.Codes.Import)
		{
			invLine.JI_ConcessionOrder = "AAJ";
			invLine.JI_PrimaryPreference = "AAK";
			invLine.ZG_REAProductCode = "AAL";
			var taxOrFeeDetailEntity = new EU.Business.Declaration.TaxOrFeeDetailEntity();
			taxOrFeeDetailEntity.VATCode = new ZString("AZ");
			taxOrFeeDetailEntity.AdditionalCode = "AAB";
			invLine.Lookups.TaxOrFeeDetailEntities.Add(taxOrFeeDetailEntity);
			invLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			invLine.ZG_AIEMType = "AC";
			invLine.ZG_ExciseCode = "AD";
			invLine.ZG_TotalRetailPrice = 2m;
			invLine.ZG_IsREADirectConsumption = false;
		}
		else if (messageType == JobMessageTypeList.Codes.Export)
		{
			DGSubstanceTestHelper.CreateIfDoesntExist("AAM", variant.ToString(), "IMO");
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "AAM" + variant.ToString();
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			invLine.UNDGs.FirstItemForBinding[0].LinkDefault(subs);
			variant++;

			invLine.JI_StateOrRegionOfOrigin = "AN";
		}
		return invLine;
	}

	static int variant = 1;
}
