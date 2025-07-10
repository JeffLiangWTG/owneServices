using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		public void TestPerformCountrySpecificOperationAfterMergeAfterCalculateDutyGenerateAdditionalInformation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_DeclarationReference = "ISW_TEST";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.OnlyFreeText;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV3";
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;

			declaration.DoMerge();
			AssertNullOrEmpty(instruction.AdditionalInformation);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.DoMerge();
			AssertContains("Processo: ISW_TEST", instruction.AdditionalInformation);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "IMP_TEST";
			declaration.DoMerge();
			AssertContains("Processo: IMP_TEST", instruction.AdditionalInformation);
		}

		public void TestExportCH_MessageType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInvoice1 = declaration.Invoices.AddNew();
			testInvoice1.JZ_InvoiceNumber = "INV3";
			var invoiceLine11 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = testInst.PK;
			invoiceLine11.JI_Tariff = "1";
			invoiceLine11.JI_PreviousEntryLineNumber = 10;
			var invoiceLine12 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = testInst.PK;
			invoiceLine12.JI_Tariff = "1";
			invoiceLine12.JI_PreviousEntryLineNumber = 11;
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			AssertEquals("CH_MessageType should be", MessageTypeList.Codes.CDE, cusEntryHeader.CH_MessageType);
		}

		public void TestEntryInstructionAutoSplitOnMerged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			for (var idx = 0; idx <= CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense; idx++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "10010" + idx.ToString().PadLeft(2, '0');
				invoiceLine.JI_CL = entryLine.PK;
			}

			AssertEquals("Precondition:", 1, declaration.CustomsEntryInstructions.Count);

			declaration.DoMerge();
			AssertEquals("Auto split on Merging", 81, declaration.CustomsEntryInstructions.Count);

			declaration.DoMerge();
			AssertEquals("DO NOT do auto split again", 81, declaration.CustomsEntryInstructions.Count);
		}

		public void TestClearEmptyFeesOnMerged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew().JI_CEI = instruction1.PK;
			invoice.InvoiceLines.AddNew().JI_CEI = instruction1.PK;
			invoice.InvoiceLines.AddNew().JI_CEI = instruction2.PK;
			invoice.InvoiceLines.AddNew().JI_CEI = instruction2.PK;

			foreach (var entryLine in declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(x => x.MergedLines))
			{
				entryLine.Fees.AddOrUpdate("AAA", 0m);
				entryLine.Fees.AddOrUpdate("BBB", 0m);
			}

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			foreach (var entryLine in declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(x => x.MergedLines))
			{
				AssertEquals("Empty fees deleted", 0, entryLine.Fees.Count);
			}
		}

		public void TestTotalValueImportEntryTwoLines()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 1200m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 600m;
			invoiceLine2.JI_NetWeight = 100m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var cusEntryHeaders = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Total SUF Fee should be equal", 192.79m, cusEntryHeaders.MergedLines.Sum(t => t.Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee)));
			});
		}

		public void TestTotalValueImportSiscomexEntryEightLines()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 2800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.JI_NetWeight = 100m;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 400m;
			invoiceLine3.JI_NetWeight = 100m;

			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "1";
			invoiceLine4.JI_LinePrice = 400m;
			invoiceLine4.JI_NetWeight = 100m;

			var invoiceLine5 = invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "1";
			invoiceLine5.JI_LinePrice = 300m;
			invoiceLine5.JI_NetWeight = 100m;

			var invoiceLine6 = invoice.InvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "1";
			invoiceLine6.JI_LinePrice = 200m;
			invoiceLine6.JI_NetWeight = 100m;

			var invoiceLine7 = invoice.InvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "1";
			invoiceLine7.JI_LinePrice = 300m;
			invoiceLine7.JI_NetWeight = 100m;

			var invoiceLine8 = invoice.InvoiceLines.AddNew();
			invoiceLine8.JI_Tariff = "1";
			invoiceLine8.JI_LinePrice = 100m;
			invoiceLine8.JI_NetWeight = 100m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var cusEntryHeaders = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Total SUF Fee should be equal", 354.76m, cusEntryHeaders.MergedLines.Sum(t => t.Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee)));
			});
		}

		public void TestGenerateEntriesForIMP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 50m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeaderSUF = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_MessageType == MessageTypeList.Codes.SUF);
			var entryHeaderCDI = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_MessageType == MessageTypeList.Codes.CDI);

			CombineAssertions("First Merge", () =>
			{
				AssertEquals("Two entries generated", 2, declaration.CustomsEntryHeaders.Count);
				AssertNotNull("CDI entry genereated", entryHeaderCDI);
				AssertNotNull("SUF entry genereated", entryHeaderSUF);
				AssertEquals("Two CDI entry line genereated", 2, entryHeaderCDI.MergedLines.Count);
				AssertEquals("One SUF entry line genereated", 1, entryHeaderSUF.MergedLines.Count);
			});
			CombineAssertions("First Merge", () =>
			{
				AssertNotNull("invoiceLine1.JI_CL set to CDI entry Line", entryHeaderCDI.MergedLines.FindByPK(invoiceLine1.JI_CL));
				AssertNotNull("invoiceLine2.JI_CL set to CDI entry Line", entryHeaderCDI.MergedLines.FindByPK(invoiceLine2.JI_CL));
				AssertEquals("AdditionalEntryLineLink added on invoiceLine1 link to SUF entry line", entryHeaderSUF.MergedLines[0].PK, invoiceLine1.AdditionalEntryLineLinks.Single().BU_CL);
				AssertEquals("AdditionalEntryLineLink added on invoiceLine2 link to SUF entry line", entryHeaderSUF.MergedLines[0].PK, invoiceLine2.AdditionalEntryLineLinks.Single().BU_CL);
				AssertEquals("CDI First Entry Line CL_CustomsValue should be", 100m, entryHeaderCDI.MergedLines[0].CL_CustomsValue);
				AssertEquals("CDI Second Entry Line CL_CustomsValue should be", 50m, entryHeaderCDI.MergedLines[1].CL_CustomsValue);
				AssertEquals("SUF Entry Line CL_CustomsValue should be", 150m, entryHeaderSUF.MergedLines[0].CL_CustomsValue);
			});

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CombineAssertions("Second Merge", () =>
			{
				AssertEquals("CDI First Entry Line CL_CustomsValue should be", 100m, entryHeaderCDI.MergedLines[0].CL_CustomsValue);
				AssertEquals("CDI Second Entry Line CL_CustomsValue should be", 50m, entryHeaderCDI.MergedLines[1].CL_CustomsValue);
				AssertEquals("SUF Entry Line CL_CustomsValue should be", 150m, entryHeaderSUF.MergedLines[0].CL_CustomsValue);
			});
		}

		public void TestUpdateEntryHeaderCustomsPostedStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CEI = declaration.CustomsEntryInstructions.AddNew().PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CEI = declaration.CustomsEntryInstructions.AddNew().PK;
			Factory.Save();

			var merger = new LineMerger(declaration);

			merger.DoMerge();
			var entryHeader1 = declaration.ActiveEntryHeaders.FormalEntries.First();
			var entryHeader2 = declaration.ActiveEntryHeaders.FormalEntries.Last();
			CombineAssertions(() =>
			{
				AssertEquals("EntryHeader 1 CH_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryHeader1.CH_CustomsPostedStatus);
				AssertEquals("EntryHeader 2 CH_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryHeader2.CH_CustomsPostedStatus);
			});

			entryHeader1.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entryHeader2.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			Factory.Save();
			merger.DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("EntryHeader 1 CH_CustomsPostedStatus", CustomsPostedStatusList.Codes.Accepted, entryHeader1.CH_CustomsPostedStatus);
				AssertEquals("EntryHeader 2 CH_CustomsPostedStatus", CustomsPostedStatusList.Codes.Accepted, entryHeader2.CH_CustomsPostedStatus);
			});

			entryHeader1.EntryInstruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.SystemGeneratedAndFreeText;
			entryHeader1.EntryInstruction.AdditionalInformationManual = "Additional Information 1";
			merger.DoMerge();
			AssertEquals("CH_CustomsPostedStatus changed to Update Pending", CustomsPostedStatusList.Codes.UpdatePending, entryHeader1.CH_CustomsPostedStatus);
			AssertEquals("CH_CustomsPostedStatus not changed", CustomsPostedStatusList.Codes.Accepted, entryHeader2.CH_CustomsPostedStatus);

			entryHeader2.EntryInstruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.SystemGeneratedAndFreeText;
			entryHeader2.EntryInstruction.AdditionalInformationManual = "Additional Information 1";
			merger.DoMerge();
			AssertEquals("CH_CustomsPostedStatus keeps Update Pending", CustomsPostedStatusList.Codes.UpdatePending, entryHeader1.CH_CustomsPostedStatus);
			AssertEquals("CH_CustomsPostedStatus changed to Update Pending", CustomsPostedStatusList.Codes.UpdatePending, entryHeader2.CH_CustomsPostedStatus);
		}

		public void TestUpdateEntryLinesCustomsPostedStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;

			AssertCustomsPostedStatus(CustomsPostedStatusList.Codes.Active, CustomsPostedStatusList.Codes.Active);

			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine2.JI_InvoiceQuantity = 100m;
			AssertCustomsPostedStatus(CustomsPostedStatusList.Codes.Active, CustomsPostedStatusList.Codes.Active);
			Factory.Save();

			entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entryLine2.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;

			AssertCustomsPostedStatus(CustomsPostedStatusList.Codes.Accepted, CustomsPostedStatusList.Codes.Accepted);

			invoiceLine1.JI_InvoiceQuantity = 200m;
			AssertCustomsPostedStatus(CustomsPostedStatusList.Codes.UpdatePending, CustomsPostedStatusList.Codes.Accepted);
			invoiceLine2.JI_InvoiceQuantity = 200m;
			AssertCustomsPostedStatus(CustomsPostedStatusList.Codes.UpdatePending, CustomsPostedStatusList.Codes.UpdatePending);

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			AssertCustomsPostedStatus(CustomsPostedStatusList.Codes.UpdatePending, CustomsPostedStatusList.Codes.UpdatePending);
			AssertEquals("EntryLine3 CL_CustomsPostedStatus should be", CustomsPostedStatusList.Codes.Active, invoiceLine3.CusEntryLine.CL_CustomsPostedStatus);

			void AssertCustomsPostedStatus(string expectedEntryLine1CustomsPostedStatus, string expectedEntryLine2CustomsPostedStatus)
			{
				merger.DoMerge();
				CombineAssertions(() =>
				{
					AssertEquals("EntryLine1 CL_CustomsPostedStatus should be", expectedEntryLine1CustomsPostedStatus, entryLine1.CL_CustomsPostedStatus);
					AssertEquals("EntryLine2 CL_CustomsPostedStatus should be", expectedEntryLine2CustomsPostedStatus, entryLine2.CL_CustomsPostedStatus);
				});
			}
		}

		public void TestReloadSiscomexUsageFeesAfterMerge()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_Tariff = "00000000";

			declaration.DoMerge();

			var entryHeaderCDI = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_MessageType == MessageTypeList.Codes.CDI);

			AssertEquals(1, entryHeaderCDI.SiscomexUsageFees.Count);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.JI_Tariff = "11111111";

			declaration.DoMerge();

			AssertEquals(2, entryHeaderCDI.SiscomexUsageFees.Count);
		}

		#region Override

		protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

		protected override Type[] ExpectedEntryCreationStrategiesType => new[]
		{
			typeof(ExportEntryCreationStrategy),
			typeof(ImportEntryCreationStrategy),
			typeof(ImportLicenseEntryCreationStrategy),
			typeof(ImportSiscomexEntryCreationStrategy),
			typeof(LPCOEntryCreationStrategy),
			typeof(ImportUsageFeeEntryCreationStrategy)
		};

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override string DefaultMessageTypeForTesting => BRJobMessageTypeList.Codes.ImportSiscomex;

		protected override bool AllowDeleteEntryLineForRegistedEntry => false;

		#endregion
	}
}
