using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportMergeStrategyTest : EntryCreationStrategyTest
	{
		public void TestB3MergeByAfterCARMR2Activated()
		{
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CarmR2, Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;

				declaration.Invoices.DeleteAll();
				invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "1";
				invoice.CA_TimeLimit = 2;
				invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
				invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Canada;

				var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				line.JI_Tariff = "2203.00.60 00";

				var line2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				line2.JI_Tariff = "2203.00.60 00";

				var lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();

				AssertEntryHeader(declaration.B3EntryHeader, MessageTypeList.Codes.CommercialAccountingDeclaration, 2);
			}
		}

		#region TestMergeKey

		public void TestCLVSMergeKey()
		{
			SetupForLVSMerge();

			var strategy = new ImportMergeStrategy(declaration, MessageTypeList.Codes.B3CUSDEC);
			var line = declaration.InvoiceLines[0];
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			line.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			line.CA_USStateOfExport = USStatesList.Codes.Arkansas;
			var keyForLine = strategy.GetKeyForLine(line);
			AssertConsolidatedLineKey(line, keyForLine);
		}

		public void TestIM2Merge()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			var strategy = new ImportMergeStrategy(declaration, MessageTypeList.Codes.B3CUSDEC);
			var line = declaration.InvoiceLines[0];
			var keyForLine = strategy.GetKeyForLine(line);
			AssertEquals("B3 Keys Count", 19, keyForLine.Keys.Count);
		}

		public void TestB3XMerge()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var strategy = new ImportMergeStrategy(declaration, MessageTypeList.Codes.B3CUSDEC);
			var line = declaration.InvoiceLines[0];
			var keyForLine = strategy.GetKeyForLine(line);
			AssertEquals("B3 Keys Count", 18, keyForLine.Keys.Count);
		}

		public void TestB3MergeKey()
		{
			var i = 0;
			var strategy = new ImportMergeStrategy(declaration, MessageTypeList.Codes.B3CUSDEC);
			var line = declaration.InvoiceLines[0];
			var keyForLine = strategy.GetKeyForLine(line);

			AssertEquals("B3 Keys Count", 18, keyForLine.Keys.Count);
			AssertKey(keyForLine, i++, (ZString)MessageTypeList.Codes.B3CUSDEC);
			AssertKey(keyForLine, i++, line.JI_Tariff);
			AssertKey(keyForLine, i++, line.InvoiceHeader.PK);
			AssertKey(keyForLine, i++, line.JI_Tariff);
			AssertKey(keyForLine, i++, line.CA_99TariffCode);
			AssertKey(keyForLine, i++, line.CA_ValueForDutyCode);
			AssertKey(keyForLine, i++, line.CA_AuthorityNumber);
			AssertKey(keyForLine, i++, line.CA_TRSNumber);
			AssertKey(keyForLine, i++, line.CA_CalculationMethod);
			AssertKey(keyForLine, i++, line.JI_ParentID);
			AssertKey(keyForLine, i++, line.EffectiveCountryAndStateOfOrigin);
			AssertKey(keyForLine, i++, line.EffectiveTreatmentCode);
			AssertKey(keyForLine, i++, line.JI_CustomsUnitQty);
			AssertKey(keyForLine, i++, line.JI_CustomsSecondUnitQty);
			AssertKey(keyForLine, i++, line.JI_CustomsThirdUnitQty);
			AssertKey(keyForLine, i++, line.DutyAndTaxManager.SIMADuties.First().C1_ExemptCode);

			foreach (var duty in line.DutiesAndTaxes.Where(a => a.IsTax).OrderBy(a => a, new DutyAndTaxComparer()))
			{
				AssertKey(keyForLine, i++, duty.C1_Code);
				AssertKey(keyForLine, i++, duty.C1_ExemptCode);
			}

			i = 0;
			line.CA_CustomsValueOvr = true;
			keyForLine = strategy.GetKeyForLine(line);

			AssertEquals("B3 Keys Count", 3, keyForLine.Keys.Count);
			AssertKey(keyForLine, i++, (ZString)MessageTypeList.Codes.B3CUSDEC);
			AssertKey(keyForLine, i++, line.JI_Tariff);
			AssertKey(keyForLine, i, line.PK);
		}

		public void TestEDIReleaseMergeKey()
		{
			var i = 0;
			var strategy = new ImportMergeStrategy(declaration, MessageTypeList.Codes.EDIRelease);
			var line = declaration.InvoiceLines[0];
			var keyForLine = strategy.GetKeyForLine(line);
			AssertEquals("EDIRelease Keys Count", 2, keyForLine.Keys.Count);
			AssertKey(keyForLine, i++, (ZString)MessageTypeList.Codes.EDIRelease);
			AssertKey(keyForLine, i, line.PK);
		}

		#region TestLowValueShipmentsMergeKey

		public void TestLowValueShipmentsMergeKey()
		{
			SetupForLVSMerge();

			var strategy = new ImportMergeStrategy(declaration, MessageTypeList.Codes.B3CUSDEC);
			var line = declaration.InvoiceLines[0];
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			line.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			line.CA_USStateOfExport = USStatesList.Codes.Arkansas;
			var keyForLine = strategy.GetKeyForLine(line);
			AssertConsolidatedLineKey(line, keyForLine);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			keyForLine = strategy.GetKeyForLine(line);
			AssertSpecialAuthorityLineKeyWithoutRemission(line, keyForLine);

			line.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			line.CA_AuthorityNumber = ZString.Empty;
			keyForLine = strategy.GetKeyForLine(line);
			AssertRemissionLineKeyWithoutSpecialAuthority(line, keyForLine);

			line.CA_CustomsValueOvr = true;
			line.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			keyForLine = strategy.GetKeyForLine(line);
			AssertEquals("Keys count of normal LVS line with overrid", 7, keyForLine.Keys.Count);
			line.CA_AuthorityNumber = "123456";
			keyForLine = strategy.GetKeyForLine(line);
			AssertLineKeyWithOverridenDutiesAndTaxes(line, keyForLine);
			line.CA_AuthorityNumber = ZString.Empty;
			keyForLine = strategy.GetKeyForLine(line);
			AssertEquals("Keys count of LVS line without casual tariff", 7, keyForLine.Keys.Count);
		}

		static void AssertConsolidatedLineKey(JobComInvoiceLine line, MergeKey keyForLine)
		{
			var i = 0;
			AssertEquals("Keys count of LVS consolidated line", 8, keyForLine.Keys.Count);
			AssertKey(keyForLine, i++, (ZString)MessageTypeList.Codes.B3CUSDEC);
			AssertKey(keyForLine, i++, line.InvoiceHeader.CA_TimeLimit);
			AssertKey(keyForLine, i++, line.InvoiceHeader.CA_TimeLimitCode);
			AssertKey(keyForLine, i++, line.InvoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertKey(keyForLine, i++, line.EffectiveTreatmentCode);
			AssertKey(keyForLine, i++, line.EffectiveCountryAndStateOfOrigin);
			AssertKey(keyForLine, i++, line.EffectiveCountryAndStateOfExport);
			AssertKey(keyForLine, i, line.PK);
		}

		static void AssertSpecialAuthorityLineKeyWithoutRemission(JobComInvoiceLine line, MergeKey keyForLine)
		{
			var i = 0;
			AssertEquals("Keys count of LVS special authority line without remission", 14, keyForLine.Keys.Count);
			AssertKey(keyForLine, i++, (ZString)MessageTypeList.Codes.B3CUSDEC);
			AssertKey(keyForLine, i++, line.InvoiceHeader.CA_TimeLimit);
			AssertKey(keyForLine, i++, line.InvoiceHeader.CA_TimeLimitCode);
			AssertKey(keyForLine, i++, line.InvoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertKey(keyForLine, i++, line.EffectiveTreatmentCode);
			AssertKey(keyForLine, i++, line.CA_AuthorityNumber);
			AssertKey(keyForLine, i++, line.JI_CustomsUnitQty);
			AssertKey(keyForLine, i++, line.JI_CustomsSecondUnitQty);
			AssertKey(keyForLine, i++, line.JI_CustomsThirdUnitQty);
			AssertKey(keyForLine, i++, line.JI_Tariff);
			AssertKey(keyForLine, i++, line.CA_99TariffCode);
		}

		static void AssertRemissionLineKeyWithoutSpecialAuthority(JobComInvoiceLine line, MergeKey keyForLine)
		{
			var i = 0;
			AssertEquals("Keys count of LVS remission line without special authority", 10, keyForLine.Keys.Count);
			AssertKey(keyForLine, i++, (ZString)MessageTypeList.Codes.B3CUSDEC);
			AssertKey(keyForLine, i++, line.InvoiceHeader.CA_TimeLimit);
			AssertKey(keyForLine, i++, line.InvoiceHeader.CA_TimeLimitCode);
			AssertKey(keyForLine, i++, line.InvoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertKey(keyForLine, i++, line.EffectiveTreatmentCode);
			AssertKey(keyForLine, i++, line.CA_CalculationMethod);
			AssertKey(keyForLine, i++, line.JI_ParentID);
		}

		static void AssertLineKeyWithOverridenDutiesAndTaxes(JobComInvoiceLine line, MergeKey keyForLine)
		{
			var i = 0;
			AssertEquals("Keys count of LVS line with overriden duties and taxes", 7, keyForLine.Keys.Count);
			AssertKey(keyForLine, i++, (ZString)MessageTypeList.Codes.B3CUSDEC);
			AssertKey(keyForLine, i++, line.InvoiceHeader.CA_TimeLimit);
			AssertKey(keyForLine, i++, line.InvoiceHeader.CA_TimeLimitCode);
			AssertKey(keyForLine, i++, line.InvoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertKey(keyForLine, i++, line.EffectiveTreatmentCode);
			AssertKey(keyForLine, i++, line.PK);
		}

		static void AssertKey(MergeKey keyForLine, int index, IZType value)
		{
			Assert(string.Format("Value should not be empty for Key '{0}'", index), !value.IsEmpty);
			AssertEquals(string.Format("Key '{0}'", index), value, keyForLine.Keys[index]);
		}

		#endregion

		#endregion

		#region TestImportMerge

		public void TestImportMerge()
		{
			//Default Import Merge (EDIRelease = NotMerge, B3 = B3 Data Merge)
			new LineMerger(declaration).DoMerge();

			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.EDIRelease, 4);
			AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.B3CUSDEC, 3);

			//Nothing changed after re-merge 
			var entries = from CusEntryHeader entry in declaration.CustomsEntryHeaders
						  select new { entry.PK, Lines = from line in entry.MergedLines select line.PK };
			new LineMerger(declaration).DoMerge();

			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.EDIRelease, 4);
			AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.B3CUSDEC, 3);
			var i = 0;
			foreach (var entry in entries)
			{
				AssertEquals("Same entry header", entry.PK, declaration.CustomsEntryHeaders[i].PK);
				var j = 0;
				foreach (var linePk in entry.Lines)
				{
					AssertEquals("Same entry line", linePk, declaration.CustomsEntryHeaders[i].MergedLines[j++].PK);
				}
				i++;
			}

			//Default Import Merge with one Invoice
			FillInvoiceLine((JobComInvoiceLine)invoice.InvoiceLines.AddNew(), Tariff1);
			declaration.Invoices.Delete(invoice1);
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.EDIRelease, 4);
			AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.B3CUSDEC, 2);

			//Merge By Classification, Tariff and Description for B3
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariffAndDescription;
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.EDIRelease, 4);
			AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.B3CUSDEC, 3);
			AssertEquals(declaration.JE_MergeBy, OrgConstants.MergeInvoiceLines.NotMerge);

			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices;
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.EDIRelease, 4);
			AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.B3CUSDEC, 2);
			AssertEquals(declaration.JE_MergeBy, OrgConstants.MergeInvoiceLines.NotMerge);
		}

		public void TestB3EntryLineNumeration()
		{
			var line = (JobComInvoiceLine)invoice.InvoiceLines[1];
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Chile;

			line = (JobComInvoiceLine)invoice.InvoiceLines[2];
			line.CA_99TariffCode = Tariff1;
			line.JI_CountryOfOrigin = Constants.CountryCodes.Canada;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.CA_ValueForDutyCode = "12";

			FillInvoiceLine((JobComInvoiceLine)invoice.InvoiceLines.AddNew(), Tariff1);

			new LineMerger(declaration).DoMerge();

			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.EDIRelease, 6);
			AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.B3CUSDEC, 5);

			var entryLines = declaration.CustomsEntryHeaders[1].AllEntryLines;
			entryLines.Sort(CusEntryLine.Schema.CL_LineNumber);

			var b3Wrapper = new B3ImportMessageWrapper(declaration.CustomsEntryHeaders[1]);
			AssertEquals("SubHeaders", 4, b3Wrapper.PositiveB3SubHeaders.Count());
			AssertEntryLine(false, entryLines[0], 1, 1, b3Wrapper, "USD", "2203006000", 1, 5);
			AssertEntryLine(false, entryLines[1], 2, 1, b3Wrapper, "USD", "2203006000", 4);
			AssertEntryLine(false, entryLines[2], 3, 2, b3Wrapper, "USD", "2203006000", 2);
			AssertEntryLine(false, entryLines[3], 4, 3, b3Wrapper, "USD", "2203006000", 3);
			AssertEntryLine(false, entryLines[4], 5, 4, b3Wrapper, "AUD", "2203006000", 1);
		}

		public void TestLineNumberAssignerForCAD()
		{
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "ABC";
				importer.OH_FullName = "ABC INC.";
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "XYZ";
				supplier.OH_FullName = "XYZ INC.";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.JE_MessageSubType = CADEntryTypeList.Codes.Confirming;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				declaration.JE_OH_Supplier = supplier.PK;
				declaration.JE_OH_Importer = importer.PK;

				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "111";
				invoice1.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Canada;

				var line1 = FillInvoiceLine(invoice1.JobComInvoiceLines.AddNew(), 1, Core.Constants.CountryCodes.Japan);
				var line2 = FillInvoiceLine(invoice1.JobComInvoiceLines.AddNew(), 2, Core.Constants.CountryCodes.China);
				var line3 = FillInvoiceLine(invoice1.JobComInvoiceLines.AddNew(), 3, Core.Constants.CountryCodes.Japan, x => x.CA_TreatmentCode = TariffTreatmentCodes.Codes.General);
				var line4 = FillInvoiceLine(invoice1.JobComInvoiceLines.AddNew(), 4, Core.Constants.CountryCodes.Japan);
				var line5 = FillInvoiceLine(invoice1.JobComInvoiceLines.AddNew(), 5, Core.Constants.CountryCodes.Japan, x => x.JI_Tariff = "2020202020");
				var line6 = FillInvoiceLine(invoice1.JobComInvoiceLines.AddNew(), 6, Core.Constants.CountryCodes.Japan, x => x.CA_99TariffCode = "9901");

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceNumber = "222";
				invoice2.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Canada;

				var line7 = FillInvoiceLine(invoice2.JobComInvoiceLines.AddNew(), 1, Core.Constants.CountryCodes.Japan);
				var line8 = FillInvoiceLine(invoice2.JobComInvoiceLines.AddNew(), 2, Core.Constants.CountryCodes.Taiwan);

				new LineMerger(declaration).DoMerge();
				CombineAssertions(() =>
				{
					AssertEquals("Invoice 1, line 1", "1", line1.JI_B3LineNumber);
					AssertEquals("Invoice 1, line 2", "2", line2.JI_B3LineNumber);
					AssertEquals("Invoice 1, line 3", "3", line3.JI_B3LineNumber);
					AssertEquals("Invoice 1, line 4", "4", line4.JI_B3LineNumber);
					AssertEquals("Invoice 1, line 5", "5", line5.JI_B3LineNumber);
					AssertEquals("Invoice 1, line 6", "6", line6.JI_B3LineNumber);
					AssertEquals("Invoice 2, line 1", "7", line7.JI_B3LineNumber);
					AssertEquals("Invoice 2, line 2", "8", line8.JI_B3LineNumber);
				});
			}
		}

		public void TestLowValueShipmentsMerge()
		{
			SetupForLVSMerge();

			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 6);

			var entryLines = from CusEntryLine line in declaration.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLine(entryLines.ElementAt(0), 1, 1, 1);
			AssertEntryLine(entryLines.ElementAt(1), 2, 1, 1);
			AssertEntryLine(entryLines.ElementAt(2), 3, 1, 2);
			AssertEntryLine(entryLines.ElementAt(3), 4, 1, 5);
			AssertEntryLine(entryLines.ElementAt(4), 5, 2, 3);
			AssertEntryLine(entryLines.ElementAt(5), 6, 3, 4);

			invoice1.CA_TimeLimit = 1;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 6);

			entryLines = from CusEntryLine line in declaration.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLine(entryLines.ElementAt(0), 1, 1, 1);
			AssertEntryLine(entryLines.ElementAt(1), 2, 1, 2);
			AssertEntryLine(entryLines.ElementAt(2), 3, 1, 5);
			AssertEntryLine(entryLines.ElementAt(3), 4, 2, 3);
			AssertEntryLine(entryLines.ElementAt(4), 5, 3, 4);
			AssertEntryLine(entryLines.ElementAt(5), 6, 4, 1);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			invoice1.CA_TimeLimit = 2;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 6);

			entryLines = from CusEntryLine line in declaration.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLine(entryLines.ElementAt(0), 1, 1, 1);
			AssertEntryLine(entryLines.ElementAt(1), 2, 1, 1);
			AssertEntryLine(entryLines.ElementAt(2), 3, 1, 2);
			AssertEntryLine(entryLines.ElementAt(3), 4, 1, 5);
			AssertEntryLine(entryLines.ElementAt(4), 5, 2, 3);
			AssertEntryLine(entryLines.ElementAt(5), 6, 3, 4);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 5);
			AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.EDIRelease, 6);

			entryLines = from CusEntryLine line in declaration.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLine(entryLines.ElementAt(0), 1, 1, 1, 2);
			AssertEntryLine(entryLines.ElementAt(1), 2, 1, 5);
			AssertEntryLine(entryLines.ElementAt(2), 3, 2, 3);
			AssertEntryLine(entryLines.ElementAt(3), 4, 3, 4);
			AssertEntryLine(entryLines.ElementAt(4), 5, 4, 1);
		}

		public void TestLowValueShipmentsMergeWithAdditionalInvoices()
		{
			SetupForLVSMergeWithAdditionalInvoices();
			declaration1.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration1.DoMerge();
			Factory.Save();

			AssertEquals(1, declaration1.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration1.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 1);
			var entryLines = from CusEntryLine line in declaration1.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLineForLVX(entryLines.ElementAt(0), 1, 1, 1);

			declaration.ResumeApportionment();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 6);

			entryLines = from CusEntryLine line in declaration.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLine(entryLines.ElementAt(0), 1, 1, 1);
			AssertEntryLine(entryLines.ElementAt(1), 2, 1, 1);
			AssertEntryLine(entryLines.ElementAt(2), 3, 1, 2);
			AssertEntryLine(entryLines.ElementAt(3), 4, 1, 5);
			AssertEntryLine(entryLines.ElementAt(4), 5, 2, 3);
			AssertEntryLine(entryLines.ElementAt(5), 6, 3, 4);

			invoice1.CA_TimeLimit = 1;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 6);
			AssertEntryLine(entryLines.ElementAt(0), 1, 1, 1);
			AssertEntryLine(entryLines.ElementAt(1), 2, 2, 1);
			AssertEntryLine(entryLines.ElementAt(2), 3, 2, 2);
			AssertEntryLine(entryLines.ElementAt(3), 4, 2, 5);
			AssertEntryLine(entryLines.ElementAt(4), 5, 3, 3);
			AssertEntryLine(entryLines.ElementAt(5), 6, 4, 4);

			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			invoice1.CA_TimeLimit = 2;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			declaration.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 6);

			entryLines = from CusEntryLine line in declaration.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLine(entryLines.ElementAt(0), 1, 1, 1);
			AssertEntryLine(entryLines.ElementAt(1), 2, 1, 1);
			AssertEntryLine(entryLines.ElementAt(2), 3, 1, 2);
			AssertEntryLine(entryLines.ElementAt(3), 4, 1, 5);
			AssertEntryLine(entryLines.ElementAt(4), 5, 2, 3);
			AssertEntryLine(entryLines.ElementAt(5), 6, 3, 4);

			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				if (invoiceLine.Declaration == declaration)
				{
					Assert("consolidated LVS jobs JI_CL should be null", invoiceLine.JI_CL.IsEmpty);
				}
				else
				{
					Assert("LVX jobs JI_CL should not be null", !invoiceLine.JI_CL.IsEmpty);
					AssertEquals(1, invoiceLine.AdditionalEntryLineLinks.Count);
				}
			}

			AssertEquals(1, declaration1.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration1.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 1);
			entryLines = from CusEntryLine line in declaration1.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLineForLVX(entryLines.ElementAt(0), 1, 1, 1);

			LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(invoice1, declaration);
			var invoiceLine1 = invoice1.InvoiceLines[0];
			AssertEquals(0, invoiceLine1.AdditionalEntryLineLinks.Count);
			Assert("LVX jobs JI_CL should not be null", !invoiceLine1.JI_CL.IsEmpty);

			declaration.DoMerge();
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 5);

			entryLines = from CusEntryLine line in declaration.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLine(entryLines.ElementAt(0), 1, 1, 1);
			AssertEntryLine(entryLines.ElementAt(1), 2, 1, 2);
			AssertEntryLine(entryLines.ElementAt(2), 3, 1, 5);
			AssertEntryLine(entryLines.ElementAt(3), 4, 2, 3);
			AssertEntryLine(entryLines.ElementAt(4), 5, 3, 4);
		}

		public void TestLVSForConsolidationMerge()
		{
			SetupForLVSMerge();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			declaration.JE_MergeBy = B3MergeByList.Codes.NotMerge;
			declaration.CA_MergeBy = B3MergeByList.Codes.NotMerge;
			declaration.Invoices.RemoveFromRelationship(invoice1);

			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEntryHeader(declaration.CustomsEntryHeaders[0], MessageTypeList.Codes.B3CUSDEC, 5);

			var entryLines = from CusEntryLine line in declaration.CustomsEntryHeaders[0].AllEntryLines orderby line.CL_LineNumber select line;
			AssertEntryLineForLVX(entryLines.ElementAt(0), 1, 1, 1);
			AssertEntryLineForLVX(entryLines.ElementAt(1), 2, 1, 2);
			AssertEntryLineForLVX(entryLines.ElementAt(2), 3, 1, 5);
			AssertEntryLineForLVX(entryLines.ElementAt(3), 4, 2, 3);
			AssertEntryLineForLVX(entryLines.ElementAt(4), 5, 3, 4);
		}

		public void TestAfterCreateOrGetEntryHeader_LegacyEntry_Deactive()
		{
			var lineMerger = new LineMerger(declaration);
			lineMerger.DoMerge();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var link1 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			link1.BU_JI = invoice1.JobComInvoiceLines[0].PK;
			link1.BU_CL = entryLine1.PK;
			lineMerger.DoMerge();

			CombineAssertions("CAD entry is deleted", () =>
			{
				AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.B3CUSDEC, 3);
				Assert("EntryHeader", entry1.IsDeleted);
				Assert("EntryLine", entryLine1.IsDeleted);
				Assert("AdditionalInvoiceLineEntryLineLink", link1.IsDeleted);
			});
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CarmR2, Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				var entryLine2 = entry2.AllEntryLines.AddNew();
				var link2 = Factory.New<AdditionalInvoiceLineEntryLineLink>();
				link2.BU_JI = invoice1.JobComInvoiceLines[0].PK;
				link2.BU_CL = entryLine2.PK;
				lineMerger.DoMerge();

				CombineAssertions("B3C entry is deleted", () =>
				{
					AssertEntryHeader(declaration.CustomsEntryHeaders[1], MessageTypeList.Codes.CommercialAccountingDeclaration, 4);
					Assert("EntryHeader", entry2.IsDeleted);
					Assert("EntryLine", entryLine2.IsDeleted);
					Assert("AdditionalInvoiceLineEntryLineLink", link2.IsDeleted);
				});
			}
		}

		void AssertEntryHeader(CusEntryHeader entryHeader, string messageType, int mergedLinesCount)
		{
			AssertEquals("CH_BGMReference", declaration.TransactionNumber.ToString(), entryHeader.CH_BGMReference);
			AssertEquals("MessageType", messageType, entryHeader.CH_MessageType);
			AssertEquals("MergedLines.Count", mergedLinesCount, entryHeader.MergedLines.Count);
			AssertEquals("AllEntryLines.Count", mergedLinesCount, entryHeader.AllEntryLines.Count);
		}

		static void AssertEntryLine(CusEntryLine entryLine, ZShort entryLineNo, ZInt subHeaderNo, params ZInt[] invLinesNumbers)
		{
			AssertEntryLine(false, entryLine, entryLineNo, subHeaderNo, null, ZString.Empty, ZString.Empty, invLinesNumbers);
		}

		static void AssertEntryLineForLVX(CusEntryLine entryLine, ZShort entryLineNo, ZInt subHeaderNo, params ZInt[] invLinesNumbers)
		{
			AssertEntryLine(true, entryLine, entryLineNo, subHeaderNo, null, ZString.Empty, ZString.Empty, invLinesNumbers);
		}

		static void AssertEntryLine(bool isForLVX, CusEntryLine entryLine, ZShort entryLineNo, ZInt subHeaderNo, B3ImportMessageWrapper wrapper, ZString expectedCurrency, ZString expectedHSCode, params ZInt[] invLinesNumbers)
		{
			AssertEquals("CL_LineNumber", entryLineNo, entryLine.CL_LineNumber);
			var b3SubHeaderNumber = isForLVX ? entryLine.RandomLine.B3SubHeaderNumberForLVX : entryLine.RandomLine.CA_B3SubHeaderNumber;
			AssertEquals("B3SubHeaderNumber", subHeaderNo, b3SubHeaderNumber);
			AssertEquals("InvoiceLines Count", invLinesNumbers.Length, entryLine.InvoiceLines.Count);

			var i = 0;
			foreach (var line in entryLine.InvoiceLines.OfType<BaseJobComInvoiceLine>().OrderBy(line => line.JI_LineNo))
			{
				AssertEquals("JI_LineNo", invLinesNumbers[i++], line.JI_LineNo);
			}

			if (wrapper != null)
			{
				AssertEquals("SubHeader currency", expectedCurrency, wrapper.PositiveB3SubHeaders.ElementAt(subHeaderNo - 1).CurrencyCode);
			}

			if (!expectedHSCode.IsEmpty)
			{
				AssertEquals("HS Code", expectedHSCode, entryLine.CL_AdValoremTariff);
			}
		}

		#endregion

		public void TestLowValueShipmentsMerge_Amount()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			declaration.JE_OH_Importer = importer.PK;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;

			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			lvxJob.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			var invoice1 = lvxJob.Invoices.AddNew();
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice1, declaration);
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice1.CA_RN_NKExport = Constants.CountryCodes.Australia;
			invoice1.JZ_RN_NKDefaultOrigin = Constants.CountryCodes.Australia;
			invoice1.JZ_RX_NKInvoice_Currency = "CAD";
			invoice1.JZ_InvoiceAmount = 2000;
			invoice1.JZ_IncoTerm = Constants.IncoTerms.CostInsuranceAndFreight;

			var line = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			line.JI_LineNo = 1;
			line.JI_Tariff = "4501.10.00 00";
			line.CA_99TariffCode = "9920";
			line.CA_ValueForDutyCode = "13";
			line.JI_LinePrice = 2000;
			line.CA_AuthorityNumber = "AAAAA";
			line.CA_TRSNumber = "TTTT";
			line.CA_CustomsValue = 2000;
			line.CA_CVforCurrConv = 2000;

			var tax = line.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax.C1_Code = "001";
			tax.C1_RateType = RateTypes.Codes.AdValorem;
			tax.C1_Rate = 5;
			tax.C1_Amount = 100;

			var duty = line.DutiesAndTaxes.AddNew();
			duty.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			duty.C1_RateType = RateTypes.Codes.Free;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice2.CA_RN_NKExport = Constants.CountryCodes.Australia;
			invoice2.JZ_RN_NKDefaultOrigin = Constants.CountryCodes.Australia;
			invoice2.JZ_RX_NKInvoice_Currency = "CAD";
			invoice2.JZ_InvoiceAmount = 700;
			invoice2.JZ_IncoTerm = Constants.IncoTerms.CostInsuranceAndFreight;

			line = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			line.JI_LineNo = 1;
			line.JI_Tariff = "1703.10.10 00";
			line.CA_ValueForDutyCode = "13";
			line.JI_CustomsQuantity = 0.06;
			line.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.MetricTon;
			line.JI_LinePrice = 400;
			line.CA_CustomsValue = 400;
			line.CA_CVforCurrConv = 400;

			tax = line.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax.C1_Code = "001";
			tax.C1_RateType = RateTypes.Codes.AdValorem;
			tax.C1_Rate = 5;
			tax.C1_Amount = 22.5;

			duty = line.DutiesAndTaxes.AddNew();
			duty.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			duty.C1_RateType = RateTypes.Codes.AdValorem;
			duty.C1_Rate = 12.5;
			duty.C1_Amount = 50;

			line = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			line.JI_LineNo = 2;
			line.JI_Tariff = "1703.10.10 00";
			line.CA_ValueForDutyCode = "13";
			line.JI_CustomsQuantity = 0.05;
			line.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.MetricTon;
			line.JI_LinePrice = 300;
			line.CA_CustomsValue = 300;
			line.CA_CVforCurrConv = 300;

			tax = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			tax.C1_Code = "001";
			tax.C1_RateType = RateTypes.Codes.AdValorem;
			tax.C1_Rate = 5;
			tax.C1_Amount = 16.88;
			line.DutiesAndTaxes.ApplySort(DutyAndTax.Schema.C1_TaxType, ListSortDirection.Descending);

			duty = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_RateType = RateTypes.Codes.AdValorem;
			duty.C1_Rate = 12.5;
			duty.C1_Amount = 37.5;

			declaration.ResumeApportionment();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(2700m, entryHeader.CustomsValue);
			AssertEquals(2700m, entryHeader.TransactionValue);
			AssertEquals(87.5m, entryHeader.TotalDutyAmount);
			AssertEquals(139.38m, entryHeader.GSTAmount);
			AssertEquals(226.88m, entryHeader.TotalAmountPayable);
			AssertEquals(3, entryHeader.MergedLines.Count);

			var line1 = entryHeader.MergedLines[0];
			AssertEquals("4501.10.00 00", line1.FormattedTariff);
			AssertEquals(2000m, line1.TotalLinePrice.Amount);
			AssertEquals(0m, line1.CustomsQuantity);

			var line2 = entryHeader.MergedLines[1];
			AssertEquals("1703.10.10 00", line2.FormattedTariff);
			AssertEquals(400m, line2.TotalLinePrice.Amount);
			AssertEquals(0.06m, line2.CustomsQuantity);

			var line3 = entryHeader.MergedLines[2];
			AssertEquals("1703.10.10 00", line3.FormattedTariff);
			AssertEquals(300m, line3.TotalLinePrice.Amount);
			AssertEquals(0.05m, line3.CustomsQuantity);
		}

		#region Implementation

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "APART";
			part.OP_Desc = "A typical part";

			var relationImporter = part.RelatedOrganisations.AddNew();
			relationImporter.OU_OH = importer.PK;
			relationImporter.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var relationSupplier = part.RelatedOrganisations.AddNew();
			relationSupplier.OU_OH = supplier.PK;
			relationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;
			var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.DutiesAndTaxes.ApplySort(DutyAndTax.Schema.C1_TaxType, ListSortDirection.Descending);

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.JI_Description = "TOYS";
			FillInvoiceLine((JobComInvoiceLine)invoice.InvoiceLines.AddNew(), Tariff2);

			invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "2";
			invoice1.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Australia;
			FillInvoiceLine((JobComInvoiceLine)invoice1.InvoiceLines.AddNew(), Tariff1);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		void SetupForLVSMerge()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_ContainerMode = ZString.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.Invoices.DeleteAll();

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.CA_TimeLimit = 2;
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Canada;

			invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "2";
			invoice1.CA_TimeLimit = 2;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			invoice1.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Canada;

			var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			line.CA_RN_NKExport = USStatesList.Codes.Wyoming;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			line.CA_RN_NKExport = USStatesList.Codes.Alabama;

			line = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff2);
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff2);
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			line.JI_CountryOfOrigin = Constants.CountryCodes.UnitedStates;
			line.JI_StateOrRegionOfOrigin = USStatesList.Codes.Alabama;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff2);
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			line.JI_CountryOfOrigin = Constants.CountryCodes.UnitedStates;
			line.JI_StateOrRegionOfOrigin = USStatesList.Codes.Texas;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.CA_AuthorityNumber = "65412";
		}

		void SetupForLVSMergeWithAdditionalInvoices()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_ContainerMode = ZString.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.Invoices.DeleteAll();

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			invoice1 = declaration1.LVXInvoiceHeader;
			invoice1.JZ_InvoiceNumber = "2";
			invoice1.CA_TimeLimit = 2;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice1, declaration);

			var line = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff2);
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;
			Factory.Save();

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			invoice = declaration.Invoices.AddNew();
			invoice1 = declaration.Invoices[0];
			invoice.JZ_InvoiceDisplaySequence = 1;
			invoice1.JZ_InvoiceDisplaySequence = 2;
			invoice.JZ_InvoiceNumber = "1";
			invoice.CA_TimeLimit = 2;
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			line.CA_RN_NKExport = USStatesList.Codes.Wyoming;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			line.CA_RN_NKExport = USStatesList.Codes.Alabama;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff2);
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			line.JI_CountryOfOrigin = Constants.CountryCodes.UnitedStates;
			line.JI_StateOrRegionOfOrigin = USStatesList.Codes.Alabama;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff2);
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Iceland;
			line.JI_CountryOfOrigin = Constants.CountryCodes.UnitedStates;
			line.JI_StateOrRegionOfOrigin = USStatesList.Codes.Texas;

			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			FillInvoiceLine(line, Tariff1);
			line.CA_AuthorityNumber = "65412";
		}

		void FillInvoiceLine(JobComInvoiceLine line, string tariff)
		{
			line.JI_PartNo = part.OP_PartNum;
			line.JI_ParentID = new ZGuid("963207EB-A5EB-44DA-AC5E-831042737513");
			line.JI_Tariff = "2203.00.60 00";
			line.CA_99TariffCode = tariff;
			line.CA_ValueForDutyCode = "13";
			line.CA_AuthorityNumber = "12345";
			line.CA_TRSNumber = "54321";
			line.JI_CountryOfOrigin = Constants.CountryCodes.UnitedStates;
			line.JI_StateOrRegionOfOrigin = USStatesList.Codes.Alabama;
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			line.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Piece;
			line.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			line.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.Gram;
			line.DutiesAndTaxes.DeleteAll();
			var duty = line.DutiesAndTaxes.AddNew();
			duty.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			duty.C1_Override = false;

			duty = line.DutiesAndTaxes.AddNew();
			duty.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			duty.C1_ExemptCode = SIMACodes.Codes.C31;

			var tax = line.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.C1_Code = "EX1";
			tax.C1_ExemptCode = ExciseTaxExemptionCodes.Codes.C94;
			tax.C1_Override = false;
		}

		JobComInvoiceLine FillInvoiceLine(JobComInvoiceLine line, ZShort lineNo, ZString countryOfOrigin, Action<JobComInvoiceLine> additionalAction = null)
		{
			line.JI_LineNo = lineNo;
			line.JI_Tariff = "1010101010";
			line.JI_InvoiceQuantity = 1m;
			line.JI_LinePrice = 2m;
			line.JI_Description = $"LINE {lineNo} DESCRIPTION";
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments;
			line.JI_CountryOfOrigin = countryOfOrigin;
			additionalAction?.Invoke(line);
			return line;
		}

		const string Tariff1 = "9945";
		const string Tariff2 = "9933";
		JobDeclaration declaration;
		JobDeclaration declaration1;
		JobComInvoiceHeader invoice;
		JobComInvoiceHeader invoice1;
		OrgSupplierPart part;

		#endregion
	}
}
