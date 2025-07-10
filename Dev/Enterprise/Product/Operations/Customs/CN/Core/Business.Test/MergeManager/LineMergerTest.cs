using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business.Testing
{
	class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(ParentEntryCreationStrategy), typeof(ChildEntryCreationStrategy) };

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		public void TestOnMerged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			Factory.Save();

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;

			var invoice = declaration.Invoices.AddNew();
			for (var idx = 0; idx < 51; idx++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "10010" + idx.ToString().PadLeft(2, '0');
			}
			new LineMerger(declaration).DoMerge();

			AssertEquals(true, instruction.RowMessageErrors.All(messageError => messageError.Message != "There are more than 50 Entry Lines on this Entry Instruction, please split this Entry Instruction to make sure that there are no more than 50 Entry Lines on each Entry Header."));
		}

		public void TestDoMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;

			declaration.DoMerge();
			var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(2, entryHeaders.Length);

			var cusEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			var recEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals(instruction1.PK, cusEntryHeader.CH_CEI_Instruction);
			AssertEquals(instruction2.PK, recEntryHeader.CH_CEI_Instruction);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.DoMerge();
			entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(2, entryHeaders.Length);

			cusEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			recEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals(instruction1.PK, recEntryHeader.CH_CEI_Instruction);
			AssertEquals(instruction2.PK, cusEntryHeader.CH_CEI_Instruction);

			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.DoMerge();
			entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(1, entryHeaders.Length);

			cusEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			recEntryHeader = entryHeaders.FirstOrDefault(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals(instruction1.PK, cusEntryHeader.CH_CEI_Instruction);
			AssertNull(recEntryHeader);

			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.DoMerge();
			entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(1, entryHeaders.Length);

			cusEntryHeader = entryHeaders.FirstOrDefault(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			recEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals(instruction1.PK, recEntryHeader.CH_CEI_Instruction);
			AssertNull(cusEntryHeader);
		}

		public void TestCalculateCustomsValue()
		{
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usdCurrency.ExchangeRates.DeleteAll();
			var rate = usdCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_SellRate = 6.72;
			Factory.Save();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CURR", "currency");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CURR", "USD", "USD", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = "NON";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "0110";
			declaration.CustomsEntryInstructions.AddNew().CEI_Style = "0110";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_IncoTerm = "FOB";
			var invoiceLine1 = CreateInvoiceLine(invoiceHeader, instruction1, 20000.12345m);
			AddCharge(invoiceLine1, "OFT", 1.23m);
			AddCharge(invoiceLine1, "ONS", 2.34m);
			AddCharge(invoiceLine1, "RYT", 3.45m);

			var invoiceLine2 = CreateInvoiceLine(invoiceHeader, instruction1, 30000.23456m);
			AddCharge(invoiceLine2, "OFT", 3.45m);
			AddCharge(invoiceLine2, "ONS", 4.56m);
			declaration.DoMerge();

			var entryHeaderEnt = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(header => header.IsEntering);
			var entryHeaderExt = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(header => header.IsExiting);
			var entryLinesEnt = entryHeaderEnt.AllEntryLines.Cast<CusEntryLine>().OrderBy(line => line.CL_CustomsValue).ToArray();
			var entryLinesExt = entryHeaderExt.AllEntryLines.Cast<CusEntryLine>().OrderBy(line => line.CL_CustomsValue).ToArray();

			AssertEquals(134442m, entryLinesEnt[0].CL_CustomsValue);
			AssertEquals(201663m, entryLinesEnt[1].CL_CustomsValue);
			AssertEquals(134401m, entryLinesExt[0].CL_CustomsValue);
			AssertEquals(201602m, entryLinesExt[1].CL_CustomsValue);

			invoiceLine1.Charges.RemoveAndDeleteAll();
			invoiceLine2.Charges.RemoveAndDeleteAll();

			invoiceHeader.JZ_IncoTerm = "CIF";
			AddCharge(invoiceLine1, "OFT", 4.321m);
			AddCharge(invoiceLine1, "ONS", 0, 5);
			AddCharge(invoiceLine2, "OFT", 5.432m);
			AddCharge(invoiceLine2, "ONS", 0, 5);
			declaration.DoMerge();

			entryHeaderEnt = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(header => header.IsEntering);
			entryHeaderExt = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(header => header.IsExiting);
			entryLinesEnt = entryHeaderEnt.AllEntryLines.Cast<CusEntryLine>().OrderBy(line => line.CL_CustomsValue).ToArray();
			entryLinesExt = entryHeaderExt.AllEntryLines.Cast<CusEntryLine>().OrderBy(line => line.CL_CustomsValue).ToArray();

			AssertEquals(134430m, entryLinesEnt[0].CL_CustomsValue);
			AssertEquals(201638m, entryLinesEnt[1].CL_CustomsValue);
			AssertEquals(127683m, entryLinesExt[0].CL_CustomsValue);
			AssertEquals(191517m, entryLinesExt[1].CL_CustomsValue);
		}

		static BaseJobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader parent, CusEntryInstruction instruction, decimal linePrice = 0m)
		{
			var result = parent.InvoiceLines.AddNew();
			result.JI_CEI = instruction.PK;
			result.JI_LinePrice = linePrice;
			return result;
		}

		static void AddCharge(BaseJobComInvoiceLine parent, string type, decimal amount = 0, decimal percentage = 0)
		{
			var result = parent.Charges.AddNew();
			result.J7_ChargeType = type;
			result.J7_Amount = amount;
			result.J7_Percentage = percentage;
		}
	}
}
