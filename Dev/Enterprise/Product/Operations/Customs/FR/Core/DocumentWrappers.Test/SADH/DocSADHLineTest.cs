using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;

sealed class DocSADHLineTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHLineTest
{
	public void TestBox2ConsignorForEAD()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var consignor = Factory.New<OrgHeader>();
		consignor.OH_FullName = "Consignor";
		consignor.MainAddress.Address1 = "Consignor address1";
		consignor.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
		consignor.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
		invoiceLine.JI_OA_ExporterAddress = consignor.MainAddress.PK;

		var line = DocSADHLine.New(entryLine, Factory);
		AssertEquals(@"Consignor
Consignor address1
FR12345678900001", line.Box2ConsignorForEAD);
	}

	public void TestBox8ConsigneeForEAD()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var consignee = Factory.New<OrgHeader>();
		consignee.OH_FullName = "Consignee";
		consignee.MainAddress.Address1 = "Consignee address1";
		consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
		consignee.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
		invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;

		var line = DocSADHLine.New(entryLine, Factory);
		AssertEquals(@"Consignee
Consignee address1
FR12345678900001", line.Box8ConsigneeForEAD);
	}

	public void TestBox34CountryOfOrigin_ShouldBePopulated_WhenCurrentDeclarationIsImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = "CN";

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		var sADHLine = DocSADHLine.New(entryLine, Factory);
		AssertEquals("Box34CountryOfOrigin should be captured from entryLine.CountryOfOriginCode when direction of current declaration is IMP.", "CN", sADHLine.Box34CountryOfOrigin);
	}

	public void TestBox34CountryOfOrigin_ShouldBeEmpty_WhenCurrentDeclarationIsExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = "CN";

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		var sADHLine = DocSADHLine.New(entryLine, Factory);
		AssertEquals("Box34CountryOfOrigin should always be empty when direction of current declaration is EXP.", ZString.Empty, sADHLine.Box34CountryOfOrigin);
	}

	public void TestBox34StateOfOrigin_ShouldBePopulated_WhenCurrentDeclarationIsImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_StateOrRegionOfOrigin = "JSP";

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		var sADHLine = DocSADHLine.New(entryLine, Factory);
		AssertEquals("Box34StateOfOrigin should be captured from entryLine.StateOrRegionOfOrigin when direction of current declaration is IMP.", "JSP", sADHLine.Box34StateOfOrigin);
	}

	public void TestBox34StateOfOrigin_ShouldBeEmpty_WhenCurrentDeclarationIsExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_StateOrRegionOfOrigin = "JSP";

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		var sADHLine = DocSADHLine.New(entryLine, Factory);
		AssertEquals("Box34StateOfOrigin should always be empty when direction of current declaration is EXP.", ZString.Empty, sADHLine.Box34StateOfOrigin);
	}

	public void TestBox31ContentsWhenFallbackIsActive()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Description = "FULL-LENGTH OR KNEE-LENGTH STOCKINGS, SOCKS AND OTHER HOSIERY, INCL. FOOTWEAR WITHOUT APPLIED SOLES, OF WOOL OR FINE ANIMAL HAIR, KNITTED OR CROCHETED (EXCL. GRADUATED COMPRESSION HOSIERY, PANTYHOSE AND TIGHTS, WOMEN''S FULL-LENGTH OR KNEE-LENGTH STOCKINGS, MEASURING PER SINGLE YARN < 67 DECITEX, AND HOSIERY FOR BABIES)";

		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		var cusEntryLine = Factory.New<CusEntryLine>();
		cusEntryHeader.AllEntryLines.Add(cusEntryLine);
		invoiceLine.JI_CL = cusEntryLine.PK;
		cusEntryLine.CL_Description = invoiceLine.JI_Description;

		declaration.CusContainers.AddNew().CO_ContainerNumber = "OOCL3219032";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "OOCL3127895";

		foreach (Customs.Business.NonPersistentCusContainer container in invoiceLine.ContainersForInvoiceLinesForBindingOnly)
		{
			container.IsForInvoiceLine = true;
		}
		declaration.JE_MasterBill = "X";
		var bill = declaration.PrimaryMasterBill;
		var cw1 = bill.PackingGroups[0].Packages[0];
		cw1.CW_PackQty = 10;
		cw1.CW_PackType = "PK";
		cw1.CW_MarksAndNos = "RED";
		var cw2 = bill.PackingGroups[0].Packages.AddNew();
		cw2.CW_PackQty = 11;
		cw2.CW_PackType = "BA";
		cw2.CW_MarksAndNos = "BLUE";

		var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
		packing1.IsLinked = true;
		packing1.PackQty = 6;

		var packing2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
		packing2.IsLinked = true;
		packing2.PackQty = 7;

		var line = DocSADHLine.New(cusEntryLine, Factory);

		var fallbackSetting = new FallbackSettings();

		fallbackSetting.End = ZDateTime.Empty;
		fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
		FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

		Factory.Save();

		// Note only first 280 chars of desc are shown
		string expected = @"FULL-LENGTH OR KNEE-LENGTH STOCKINGS, SOCKS AND OTHER HOSIERY, INCL. FOOTWEAR WITHOUT APPLIED SOLES, OF WOOL OR FINE ANIMAL HAIR, KNITTED OR CROCHETED (EXCL. GRADUATED COMPRESSION HOSIERY, PANTYHOSE AND TIGHTS, WOMEN''S FULL-LENGTH OR KNEE-LENGTH STOCKINGS, MEASURING PER SINGLE Y
Nb et nature des colis : 6 - PK
Marques et numéros : RED
Nb et nature des colis : 7 - BA
Marques et numéros : BLUE
Numéro conteneurs : OOCL3219032, OOCL3127895";
		AssertEquals("entryLine.Box31Contents", expected, line.Box31PackagesAndDescriptionOfGoods);

		packing2.IsLinked = false;
		line = DocSADHLine.New(cusEntryLine, Factory);
		AssertContains(@"Nb et nature des colis : 6 - PK
Marques et numéros : RED
Numéro conteneurs :", line.Box31PackagesAndDescriptionOfGoods);
	}

	[TestDate(2022, 11, 9)]
	public void TestBox44VatInfoCalculation()
	{
		var eURCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.EuropeanUnion);
		var aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
		var cNYCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.China);

		CurrencyConverterTestHelper.SetExchangeRate(Factory, eURCurrency, 1m);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, aUDCurrency, 10m);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, cNYCurrency, 20m);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_IncoTerm = "FOB";
		invoice.JZ_InvoiceAmount = 10000m;
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		var charge1 = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 1000.10m, invoice.JobDeclaration.LocalCurrencyCode);
		charge1.J7_IsIncludedInITOT = true;

		var charge2 = invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 500.50m, invoice.JobDeclaration.LocalCurrencyCode);
		charge1.J7_IsIncludedInITOT = true;

		var charge3 = invoice.Charges.AddNew("CEE", 20.50m, Core.Constants.CurrencyCodes.UnitedStates);
		charge3.J7_IsIncludedInITOT = true;
		var charge4 = invoice.Charges.AddNew("CEE", 30.5m, Core.Constants.CurrencyCodes.UnitedStates);
		charge4.J7_IsIncludedInITOT = true;

		var charge5 = invoice.Charges.AddNew("CNE", 10m, Core.Constants.CurrencyCodes.China);
		charge5.J7_IsIncludedInITOT = true;
		var charge6 = invoice.Charges.AddNew("CNE", 10m, Core.Constants.CurrencyCodes.China);
		charge6.J7_IsIncludedInITOT = true;
		var charge7 = invoice.Charges.AddNew("CNE", 30m, Core.Constants.CurrencyCodes.Australia);
		charge7.J7_IsIncludedInITOT = true;

		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_LinePrice = 6000m;

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_LinePrice = 2000m;

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_LinePrice = 2000m;

		declaration.ResumeApportionment();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		invoiceLine1.JI_CL = entryLine.PK;
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		invoiceLine2.JI_CL = entryLine2.PK;

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine3 = entryHeader2.MergedLines.AddNew();
		entryLine3.CL_LineNumber = 1;
		invoiceLine3.JI_CL = entryLine3.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Apportioned charges OTH for line1", 600.06m, invoiceLine1.ApportionedCharges.GetCharge(charge1.ChargeKey).Amount);
			AssertEquals("Apportioned charges OTH for line2", 200.02m, invoiceLine2.ApportionedCharges.GetCharge(charge1.ChargeKey).Amount);
			AssertEquals("Apportioned charges OTH for line3", 200.02m, invoiceLine3.ApportionedCharges.GetCharge(charge1.ChargeKey).Amount);

			AssertEquals("Apportioned charges OFT for line1", 300.30m, invoiceLine1.ApportionedCharges.GetCharge(charge2.ChargeKey).Amount);
			AssertEquals("Apportioned charges OFT for line2", 100.10m, invoiceLine2.ApportionedCharges.GetCharge(charge2.ChargeKey).Amount);
			AssertEquals("Apportioned charges OFT for line3", 100.10m, invoiceLine3.ApportionedCharges.GetCharge(charge2.ChargeKey).Amount);

			var line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("EntryHeader1: First Entryline of Import has VatInfoCalculation data", "OTH:800.08EUR OFT:400.40EUR CEE:40.80USD CNE:3.20EUR ", line.Box44VatInfoCalculation);
			var line2 = DocSADHLine.New(entryLine2, Factory);
			AssertEquals("EntryHeader1: Other Entryline has NO VatInfoCalculation data", ZString.Empty, line2.Box44VatInfoCalculation);
			var line3 = DocSADHLine.New(entryLine3, Factory);
			AssertEquals("EntryHeader2: First Entryline of Import has VatInfoCalculation data", "OTH:200.02EUR OFT:100.10EUR CEE:10.20USD CNE:0.80EUR ", line3.Box44VatInfoCalculation);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var newEntryLine = newFactory.Load<CusEntryLine>(entryLine.PK);
			var newLine = DocSADHLine.New(newEntryLine, newFactory);
			AssertEquals("Should keep decimal places style in new fatory", "OTH:800.08EUR OFT:400.40EUR CEE:40.80USD CNE:3.20EUR ", newLine.Box44VatInfoCalculation);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("No VatInfoCalculation data for Export", ZString.Empty, line.Box44VatInfoCalculation);
		});
	}

	public void TestBox44VatInfoCalculation_ApportionedCharges()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_IncoTerm = "FOB";
		invoice.JZ_InvoiceAmount = 10000m;
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		var charge1 = invoice.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
		charge1.J7_IsIncludedInITOT = true;

		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_LinePrice = 8000m;

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_LinePrice = 2000m;

		declaration.ResumeApportionment();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		invoiceLine1.JI_CL = entryLine.PK;
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		invoiceLine2.JI_CL = entryLine2.PK;
		AssertEquals("Apportioned charges OTH for line1", 800m, invoiceLine1.ApportionedCharges.GetCharge(charge1.ChargeKey).Amount);
		AssertEquals("Apportioned charges OTH for line2", 200m, invoiceLine2.ApportionedCharges.GetCharge(charge1.ChargeKey).Amount);
		var line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("EntryHeader1: First Entryline of Import has VatInfoCalculation data", "RLF:1000.0EUR ", line.Box44VatInfoCalculation);
	}

	public void TestBox44VatInfoCalculation_NonApportionedCharges()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_IncoTerm = "FOB";
		invoice.JZ_InvoiceAmount = 10000m;
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_LinePrice = 8000m;
		var charge1 = invoiceLine1.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 800.0m, invoice.JobDeclaration.LocalCurrencyCode);
		charge1.J7_IsIncludedInITOT = true;

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_LinePrice = 2000m;
		var charge2 = invoiceLine2.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 200.0m, invoice.JobDeclaration.LocalCurrencyCode);
		charge2.J7_IsIncludedInITOT = true;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		invoiceLine1.JI_CL = entryLine.PK;
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		invoiceLine2.JI_CL = entryLine2.PK;
		AssertEquals("Apportioned charges OTH for line1", 800m, invoiceLine1.Charges.GetCharge(charge1.ChargeKey).Amount);
		AssertEquals("Apportioned charges OTH for line2", 200m, invoiceLine2.Charges.GetCharge(charge2.ChargeKey).Amount);
		var line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("EntryHeader1: First Entryline of Import has VatInfoCalculation data", "RLF:1000.0EUR ", line.Box44VatInfoCalculation);
	}

	public void TestBox46StatisticalValue()
	{
		var entryLineMock = Factory.NewMoq<CusEntryLine>();
		entryLineMock.Protected().Setup<ZBool>("CusEntryLinesConfirmedValueHasBeenPopulated").Returns(true);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES050;

		var entryLine = entryLineMock.Object;
		entryLine.CL_CH = entryHeader.PK;
		entryLine.CL_ConfirmedStatisticalValue = 3m;
		entryLine.CL_StatisticalValue = 4m;

		var wrapper = DocSADHLine.New(entryLine, Factory);
		AssertEquals(3m, wrapper.Box46StatisticalValue);

		var entryLineMock2 = Factory.NewMoq<CusEntryLine>();
		var entryLine2 = entryLineMock2.Object;
		entryLine2.CL_CH = entryHeader.PK;
		entryLine2.CL_ConfirmedStatisticalValue = 3m;
		entryLine2.CL_StatisticalValue = 4m;

		entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES040;
		wrapper = DocSADHLine.New(entryLine2, Factory);
		AssertEquals(4m, wrapper.Box46StatisticalValue);
	}

	public override void TestBox31Contents()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Description = "FULL-LENGTH OR KNEE-LENGTH STOCKINGS, SOCKS AND OTHER HOSIERY, INCL. FOOTWEAR WITHOUT APPLIED SOLES, OF WOOL OR FINE ANIMAL HAIR, KNITTED OR CROCHETED (EXCL. GRADUATED COMPRESSION HOSIERY, PANTYHOSE AND TIGHTS, WOMEN''S FULL-LENGTH OR KNEE-LENGTH STOCKINGS, MEASURING PER SINGLE YARN < 67 DECITEX, AND HOSIERY FOR BABIES)";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		invoiceLine.JI_CL = entryLine.PK;
		entryLine.CL_Description = invoiceLine.JI_Description;

		declaration.CusContainers.AddNew().CO_ContainerNumber = "OOCL3219032";
		declaration.CusContainers.AddNew().CO_ContainerNumber = "OOCL3127895";

		foreach (Customs.Business.NonPersistentCusContainer container in invoiceLine.ContainersForInvoiceLinesForBindingOnly)
		{
			container.IsForInvoiceLine = true;
		}
		declaration.JE_MasterBill = "X";
		var bill = declaration.PrimaryMasterBill;
		var cw1 = bill.PackingGroups[0].Packages[0];
		cw1.CW_PackQty = 10;
		cw1.CW_PackType = "PK";
		cw1.CW_MarksAndNos = "RED";
		var cw2 = bill.PackingGroups[0].Packages.AddNew();
		cw2.CW_PackQty = 11;
		cw2.CW_PackType = "BA";
		cw2.CW_MarksAndNos = "BLUE";

		var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
		packing1.IsLinked = true;
		packing1.PackQty = 6;

		var packing2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
		packing2.IsLinked = true;
		packing2.PackQty = 7;

		DocSADHLine line = DocSADHLine.New(entryLine, Factory);

		// Note only first 280 chars of desc are shown
		string expected = @"FULL-LENGTH OR KNEE-LENGTH STOCKINGS, SOCKS AND OTHER HOSIERY, INCL. FOOTWEAR WITHOUT APPLIED SOLES, OF WOOL OR FINE ANIMAL HAIR, KNITTED OR CROCHETED (EXCL. GRADUATED COMPRESSION HOSIERY, PANTYHOSE AND TIGHTS, WOMEN''S FULL-LENGTH OR KNEE-LENGTH STOCKINGS, MEASURING PER SINGLE Y
Nb et nature des colis : 6 - PK
Marques et numéros : RED
Nb et nature des colis : 7 - BA
Marques et numéros : BLUE
Numéro conteneurs : OOCL3219032, OOCL3127895";
		AssertEquals("entryLine.Box31Contents", expected, line.Box31PackagesAndDescriptionOfGoods);

		packing2.IsLinked = false;
		line = DocSADHLine.New(entryLine, Factory);
		expected = @"FULL-LENGTH OR KNEE-LENGTH STOCKINGS, SOCKS AND OTHER HOSIERY, INCL. FOOTWEAR WITHOUT APPLIED SOLES, OF WOOL OR FINE ANIMAL HAIR, KNITTED OR CROCHETED (EXCL. GRADUATED COMPRESSION HOSIERY, PANTYHOSE AND TIGHTS, WOMEN''S FULL-LENGTH OR KNEE-LENGTH STOCKINGS, MEASURING PER SINGLE Y
Nb et nature des colis : 6 - PK
Marques et numéros : RED
Numéro conteneurs : OOCL3219032, OOCL3127895";
		AssertContains(expected, line.Box31PackagesAndDescriptionOfGoods);
	}

	public override void TestLineFieldsForTheC88()
	{
		JobDeclaration declaration;
		CusEntryInstruction entryinstruction;
		JobComInvoiceLine invoiceLine;
		GenerateDeclarationForTestingProperties(out declaration, out entryinstruction, out invoiceLine);

		var merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_CEI_Instruction = entryinstruction.PK;

		var entryLine = entryHeader.MergedLines[0];
		entryLine.CL_StatisticalValue = 123.46;

		DocSADHLine line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("32 Item Number", 1, line.Box32ItemNumber);
		AssertEquals("33 Commodity Code", "1234567890", line.Box33CommodityCode);
		AssertEquals("33 EC Supplement", "E1", line.Box33ECSupplement);
		AssertEquals("33 EC Supplement2", "E2", line.Box33ECSupplement2);
		AssertEquals("34a Country of Origin", "AU", line.Box34CountryOfOrigin);
		AssertEquals("34b State of Origin", "20", line.Box34StateOfOrigin);
		AssertEquals("35 Gross Mass in KGs", "45.359", line.Box35GrossWeightInKG);
		AssertEquals("36 Preference", "100", line.Box36Preference);
		AssertEquals("37 Procedure Code", "1071 F61", line.Box37Procedure);
		AssertEquals("38 Net Mass in KGs", "2.500", line.Box38NetWeightInKG);
		AssertEquals("39 Quota", "YAY", line.Box39Quota);
		AssertEquals("41 Supplementary Units", "15.340[ABC]", line.Box41SupplementaryUnits);
		AssertEquals("41 Supplementary Quantity", "15.340", line.Box41SupplementaryQty);
		AssertEquals("41 Supplementary UQ", "ABC", line.Box41SupplementaryUQDescription);
		AssertEquals("42 Item Price", entryLine.TotalLinePrice.ToString(), line.Box42ItemPrice);
		AssertEquals("43 Valuation Method", ValuationMethodList.Codes._1, line.Box43ValuationMethod);
		AssertEquals("45 Adjustment", "[A] 12.4%", line.Box45Adjustment);
		AssertEquals("46 Statistical Value", 123m, line.Box46StatisticalValue);
		AssertEquals("46 Statistical Value Currency Code", "EUR", line.Box46CurrencyCode);
		AssertEquals("49 Warehouse", "00001099", line.Box49Warehouse);

		invoiceLine.JI_Weight = 0;
		entryLine = entryHeader.MergedLines[0];
		line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("35 Gross Mass in KGs should be blank not zero", "", line.Box35GrossWeightInKG);

		entryLine.CL_StatisticalValue = 0.45;
		line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("46 Statistical Value", 0m, line.Box46StatisticalValue);

		entryLine.CL_StatisticalValue = 0.52;
		line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("46 Statistical Value", 1m, line.Box46StatisticalValue);

		entryLine.CL_StatisticalValue = 1.45;
		line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("46 Statistical Value", 1m, line.Box46StatisticalValue);
	}

	void GenerateDeclarationForTestingProperties(out JobDeclaration declaration, out CusEntryInstruction entryinstruction, out JobComInvoiceLine invoiceLine)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var procedure = helper.CreateOrFindExistingRefCusProcedure("FR", "IM", "10", "71", "F61", "", "IMP", "10P");
		procedure.ZZ6_IntoWarehouse = "Y";
		procedure.ZZ6_OutOfWarehouse = "N";

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
		helper.CreatePreferenceForCountryAndGrouping("100", "Normal Third Country Tariff Duty (Including Ceilings)", "FR", "EUN");
		Factory.Save();

		OrgHeader warehouse = Factory.NewWithValidTestData<OrgHeader>();
		OrgCusCode warehouseID = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A1234567GB", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France));
		warehouseID.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_PaymentMethod = "A";
		declaration.JE_DefermentAccountNumber = "1234567";
		declaration.ZG_VATDeferType = "B";
		declaration.ZG_VATDeferNumber = "9876543";

		entryinstruction = declaration.CustomsEntryInstructions.AddNew();
		var warehouseAddress = Factory.New<OrgAddress>();
		entryinstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
		var cusAuthorisationHeader = warehouseAddress.Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
		cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		cusAuthorisationHeader.CPH_Type = Enterprise.Customs.FR.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
		cusAuthorisationHeader.CPH_OH_PermitHolder = warehouseAddress.Header?.PK ?? ZGuid.Empty;
		cusAuthorisationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;

		var authorisation = entryinstruction.ToWarehouseAuthorisation;
		authorisation.CPH_Number = "00001099";
		authorisation.CPH_RN_NKCountryCode = "FR";

		var rule = authorisation.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = "USE";
		rule.CPR_ValueFrom = "U";

		var rule2 = authorisation.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = "TST";
		rule2.CPR_ValueFrom = "T";

		JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader.JZ_InvoiceAmount = 1234.00m;
		invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";

		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "1234567890";
		invoiceLine.JI_CountryOfOrigin = "AU";
		invoiceLine.JI_StateOrRegionOfOrigin = "20";
		invoiceLine.JI_Weight = 100.00m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Pounds;
		invoiceLine.JI_PrimaryPreference = "100";
		invoiceLine.JI_NetWeight = 50.00m;
		invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Ounces;
		invoiceLine.JI_CustomsQuantity = 2.5m;
		invoiceLine.JI_ConcessionOrder = "YAY";
		invoiceLine.JI_CustomsSecondQuantity = 15.34m;
		invoiceLine.JI_CustomsSecondUnitQty = "ABC";
		invoiceLine.JI_SupplementaryCode1 = "S001";
		invoiceLine.JI_SupplementaryCode2 = "S002";
		var additionalSupplementaryCode3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		additionalSupplementaryCode3.CY_Code = "E1";
		additionalSupplementaryCode3.CY_Order = 3;
		var additionalSupplementaryCode4 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		additionalSupplementaryCode4.CY_Code = "E2";
		additionalSupplementaryCode4.CY_Order = 4;
		invoiceLine.JI_LinePrice = 1234.00m;
		invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
		invoiceLine.ZG_ValueAdjustmentCode = "A";
		invoiceLine.JI_ValuationMarkup = 12.4m;
		invoiceLine.ZG_StatisticalValueManualOverride = true;
		invoiceLine.ZG_StatisticalValue = 123.45m;
		invoiceLine.ZG_PrincipalsRepresentativeName = "FREDDY MERCURY";
		invoiceLine.ZG_RL_NKPrincipalsRepresentativeCity = "FRLON";
		invoiceLine.JI_CEI = entryinstruction.PK;
		invoiceLine.JI_Procedure = "1071F61";
	}

	public void TestBox44Contents()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var frGroup = helper.CreateNewOrGetExistingDataGrouping("FR");

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var addInfCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HDR1", "COZ WE WANT TO", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HNY1", "HONEY TO THE BEE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { addInfCodeType }, "HIT99", "RANDOM SONG TITLE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9100", "I HAVE NO CLUE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9120", "9120 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "1071F61";
			invoiceLine.JI_CustomsThirdQuantity = 291.94;
			invoiceLine.JI_CustomsThirdUnitQty = "005";
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryinstruction.PK;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			declaration.JE_OH_Importer = importer.PK;

			var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
			euAddInfo.ZO_UseFr3FiscalRepresentation = true;

			var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
			fiscalReferenceOrganisation.OH_Code = "FISCALREP";

			var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
			fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
			fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
			fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
			fiscalReferenceOrganisationAddress.OA_PostCode = "333";
			fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

			var fiscalReference = entryinstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = "FR3";
			fiscalReference.CFR_Reference = "FR33562024100133";
			fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;

			var warehouseAddress = Factory.New<OrgAddress>();
			entryinstruction.CEI_OA_Warehouse = warehouseAddress.PK;

			var cusAuthorisationHeader = warehouseAddress.Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationHeader.CPH_Type = Enterprise.Customs.FR.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			cusAuthorisationHeader.CPH_OH_PermitHolder = ZGuid.Empty;
			cusAuthorisationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;
			cusAuthorisationHeader.CPH_Number = "00001099";

			var suppDocHeader = invoiceHeader.SupportingDocuments.AddNew();
			suppDocHeader.CSI_Code = "HDR1";
			suppDocHeader.CSI_ReferenceNumber = "BILLY";
			suppDocHeader.CSI_SubType = "X";
			suppDocHeader.CSI_Quantity = 99;
			suppDocHeader.CSI_Description = "COZ WE WANT TO";

			var addInfoHeader = invoiceHeader.AdditionalInfos.AddNew();
			addInfoHeader.CSI_Code = "HIT99";
			addInfoHeader.CSI_Description = "RANDOM SONG TITLE";

			var suppDocGroup = declaration.SupportingDocuments.AddNew();
			suppDocGroup.CSI_Code = "HNY1";
			suppDocGroup.CSI_ReferenceNumber = "PIPER";
			suppDocGroup.CSI_SubType = "2";
			suppDocGroup.CSI_Quantity = 22;
			suppDocGroup.CSI_Description = "HONEY TO THE BEE";
			suppDocGroup.CSI_IsDTP = true;

			var addInfoGroup = declaration.AdditionalInfos.AddNew();
			addInfoGroup.CSI_Code = "BOB21";
			addInfoGroup.CSI_Description = "MADE SENSE AT THE TIME";

			var addInfo1 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "GEN13";
			addInfo1.CSI_Description = "PLENTY OF COATS";

			var addInfo2 = invoiceLine.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "PAL01";

			var cana1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V911";

			var cana2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V910";

			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "9100";
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";
			document1.CSI_DateOfIssue = new ZDate(2021, 12, 10);

			var document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "9120";
			document2.CSI_ReferenceNumber = "45982309";
			document2.CSI_Description = "9120 Test";
			document2.CSI_IsDTP = true;

			var line = DocSADHLine.New(entryLine, Factory);
			string expected = @"
Mention(s) Spéciale(s): BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; PAL01
CANA(s) : V910; V911
Document(s) joint(s) : HDR1 BILLY; 9100 3278923 10/12/2021
Disposition(s) tarifaire(s) particulière(s) : HNY1; 9120
Taxations spécifiques : quantité : 291.94 - code mesurage : 005".TrimStart(new[] { '\r', '\n' });
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);

			entryLine.CL_LineNumber = 2;
			expected = @"Mention(s) Spéciale(s): GEN13-PLENTY OF COATS; PAL01";
			line = DocSADHLine.New(entryLine, Factory);
			AssertContains("entryLine.Box44Contents for non-first pages", expected, line.Box44AddInfoAndDocuments);
		}
	}

	public void TestBox44SpecificRegime_ImpWithRules()
	{
		JobDeclaration declaration;
		CusEntryLine entryLine;
		Customs.Business.CusAuthorisationHeader authHeader;
		GenerateDataToTestEconomicRegimeInBox44(out declaration, out entryLine, out authHeader);

		var line = DocSADHLine.New(entryLine, Factory);

		var expectedIMP = @"
Régime Economique :
° Autorisation Économique (entrée) : TST_ATH_001	° Pays d’Autorisation : GB
° Montant garanti : 19.90
° Délai d’apurement : 9
Nature du perfectionnement, de la transformation ou de l’utilisation des marchandises : N1
Description technique des marchandises et des produits compensateurs ou transformés et les moyens de les identifier : CN Code 123456
Codes relatifs aux conditions économiques conformément à l’annexe 70 : C1
Bureau d’apurement : O1
Lieu de perfectionnement, de transformation ou d’utilisation : L1
Formalités de transfert proposées : T1 Description".TrimStart(new[] { '\r', '\n' });
		AssertEquals("IMP Box44AddInfoAndDocuments", expectedIMP, line.Box44AddInfoAndDocuments);
	}

	public void TestBox44SpecificRegime_ImpWithOutRules()
	{
		JobDeclaration declaration;
		CusEntryLine entryLine;
		Customs.Business.CusAuthorisationHeader authHeader;
		GenerateDataToTestEconomicRegimeInBox44(out declaration, out entryLine, out authHeader);
		declaration.JE_MessageType = "IMP";
		var rules = authHeader.CusAuthorisationRules.Where(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.STO);
		rules.DeleteAll();

		var line = DocSADHLine.New(entryLine, Factory);

		var expectedIMP = @"
Régime Economique :
° Autorisation Économique (entrée) : TST_ATH_001	° Pays d’Autorisation : GB
° Montant garanti : 19.90
Nature du perfectionnement, de la transformation ou de l’utilisation des marchandises : N1
Description technique des marchandises et des produits compensateurs ou transformés et les moyens de les identifier : CN Code 123456
Codes relatifs aux conditions économiques conformément à l’annexe 70 : C1
Bureau d’apurement : O1
Lieu de perfectionnement, de transformation ou d’utilisation : L1
Formalités de transfert proposées : T1 Description".TrimStart(new[] { '\r', '\n' });
		AssertEquals("IMP Box44AddInfoAndDocuments", expectedIMP, line.Box44AddInfoAndDocuments);
	}

	public void TestBox44SpecificRegime_Exp()
	{
		JobDeclaration declaration;
		CusEntryLine entryLine;
		Customs.Business.CusAuthorisationHeader authHeader;
		GenerateDataToTestEconomicRegimeInBox44(out declaration, out entryLine, out authHeader);
		declaration.JE_MessageType = "EXP";

		var line = DocSADHLine.New(entryLine, Factory);
		var expectedEXP = @"
Régime Economique :
° Autorisation Économique (sortie) : TST_ATH_001	° Pays d’Autorisation : GB".TrimStart(new[] { '\r', '\n' });
		AssertEquals("EXP Box44AddInfoAndDocuments", expectedEXP, line.Box44AddInfoAndDocuments);
	}

	void GenerateDataToTestEconomicRegimeInBox44(out JobDeclaration declaration, out CusEntryLine entryLine, out Customs.Business.CusAuthorisationHeader authHeader)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Description, euGrouping);
		Factory.Save();
		var euDtyRateType = helper.CreateCusRateType(euGrouping.ZZZ_DataGrouping, Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
		Factory.Save();
		helper.LoadOrCreateNewCusRateCode(Factory, Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, euDtyRateType.PK);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.AUTDC, MapDirectionList.Codes.BTH, "Authorisation document code", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "OPO", "C019", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
		helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "IPO", "C601", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
		helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "TEA", "C516", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
		helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CWP", "C517", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
		helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CW1", "C518", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
		helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CW2", "C519", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
		helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "EUS", "N990", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
		Factory.Save();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var customer = Factory.NewWithValidTestData<OrgHeader>();
		customer.OH_RL_NKClosestPort = "GBLON";
		authHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
		authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
		authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
		authHeader.CPH_OH_PermitHolder = customer.PK;
		authHeader.CPH_Number = "1234";
		authHeader.CPH_PermitDescription = "CN Code 123456";
		authHeader.CPH_StartDate = ZDate.Today;
		authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		authHeader.CPH_IsActive = true;

		var autRule = authHeader.CusAuthorisationRules.AddNew();
		autRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
		autRule.CPR_ValueFrom = "TST_ATH_001";

		var usage = entryInstruction.CusAuthorizationUsages.AddNew();
		usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
		usage.AGC_Number = "1234";
		usage.AGC_OH_Owner = customer.PK;

		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.NAT, "N1");
		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.CON, "C1");
		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.OFC, "O1").CPR_Description = "Square";
		CreateCusAuthorisationRule(authHeader, Enterprise.Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "L1").CPR_Description = "Hidden";
		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.TRA, "T1").CPR_Description = "T1 Description";
		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.INF, "I1");
		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.STO, "9");
		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCD, "100");
		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCV, "5");
		CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCP, "15");
		entryLine.Fees.AddOrUpdate(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 12); // Duty: 12.46m
		entryLine.Fees.AddOrUpdate(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 29); // VAT: 29.12m
		entryLine.Fees.AddOrUpdate("A387", 43).NationalFeeTypeCode = "A387";  // ParaFiscal: 43.34m
	}

	public override void TestBox44FiscalReference()
	{
		var declaration = Factory.New<JobDeclaration>();

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_Procedure = "1071F61";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var importer = Factory.New<OrgHeader>();
		importer.OH_Code = "IMPORTER";
		declaration.JE_OH_Importer = importer.PK;

		var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
		euAddInfo.ZO_UseFr3FiscalRepresentation = true;

		var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
		fiscalReferenceOrganisation.OH_Code = "FISCALREP";

		var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
		fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
		fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
		fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
		fiscalReferenceOrganisationAddress.OA_PostCode = "333";
		fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		fiscalReference.CFR_Code = "FR3";
		fiscalReference.CFR_Reference = "FR33562024100133";
		fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;

		var wrapper = DocSADHLine.New(entryLine, Factory);
		AssertContains("FISCAL ADDRESS1", wrapper.Box44FiscalReference.ToString());
		AssertContains("FISCAL ADDRESS2", wrapper.Box44FiscalReference.ToString());
		AssertContains("FISCAL CITY", wrapper.Box44FiscalReference.ToString());
		AssertContains("333", wrapper.Box44FiscalReference.ToString());
		AssertContains("FRANCE", wrapper.Box44FiscalReference.ToString());

		Assert(wrapper.ShowBox44FiscalReference);

		AssertEquals("FR33562024100133", wrapper.Box44FiscalReferenceNumber);
	}

	public override void TestBox47Taxes()
	{
		var docLine = GetSADHLineForBox47Taxes();
		var taxCollection = docLine.Box47Taxes;
		AssertNotNull(taxCollection);
		AssertEquals("TaxCollection count", 2, taxCollection.Count);
		AssertEquals("TaxCollection[1].G4_Type", "B00", taxCollection[1].G4_Type);
		AssertEquals("TaxCollection[1].Box47b", "1266", taxCollection[1].Box47b);
		AssertEquals("TaxCollection[1].G4_Amount_InDeclarationCurrency", "221", taxCollection[1].G4_Amount_InDeclarationCurrency);
		AssertEquals("TaxCollection[1].TaxType", "    /B00", taxCollection[1].TaxType);
		AssertEquals("TaxCollection[1.TaxMethodOfPayment", "/R", taxCollection[1].TaxMethodOfPayment);

		AssertEquals("TaxCollection[0].G4_Type", "A00", taxCollection[0].G4_Type);
		AssertEquals("TaxCollection[0].Box47b", "1106", taxCollection[0].Box47b);
		AssertEquals("TaxCollection[0].G4_Amount_InDeclarationCurrency", "30", taxCollection[0].G4_Amount_InDeclarationCurrency);
		AssertEquals("TaxCollection[0].TaxType", "A445/A00", taxCollection[0].TaxType);
		AssertEquals("TaxCollection[0].TaxMethodOfPayment", "C/R", taxCollection[0].TaxMethodOfPayment);
	}

	protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHLine GetSADHLineForBox47Taxes()
	{
		JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_PaymentMethod = "R";
		Factory.Save();

		var invHeader = declaration.Invoices.AddNew();
		var invLine1 = invHeader.InvoiceLines.AddNew();
		var invLine2 = invHeader.InvoiceLines.AddNew();
		invLine1.JI_Tariff = "abc";
		invLine2.JI_Tariff = "abc";

		var tax1Vat = invLine1.Taxes.AddNew();
		var tax1Dty = invLine1.Taxes.AddNew();
		var tax2Vat = invLine2.Taxes.AddNew();
		var tax2Dty = invLine2.Taxes.AddNew();
		tax1Vat.Data.G4_Type = "B00";
		tax1Dty.Data.G4_Type = "A00";
		tax2Vat.Data.G4_Type = "B00";
		tax2Dty.Data.G4_Type = "A00";
		invLine1.JI_LinePrice = 1000m;
		invLine2.JI_LinePrice = 105.89m;

		var merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders[0];

		var entryLine = entryHeader.MergedLines[0];

		var lineFee1 = entryLine.ConfirmedFees.AddOrUpdate("A00", 0m);
		var lineFee2 = entryLine.ConfirmedFees.AddOrUpdate("B00", 0m);
		lineFee1.NationalFeeTypeCode = "A445";
		lineFee1.CF_MethodOfPayment = "1";

		lineFee1.CF_BaseValue = 1105.89m;
		lineFee1.CF_ChargeAmount = 29.85m;

		lineFee2.CF_BaseValue = 1266.03m;
		lineFee2.CF_ChargeAmount = 221.45m;

		entryHeader.MergedLines[0].CL_CustomsValue = 1105.89m;

		tax1Vat.Data.G4_Amount = "200.34";
		tax1Vat.Data.G4_BaseAmount = 1144.81m;

		tax2Vat.Data.G4_Amount = "21.21";
		tax2Vat.Data.G4_BaseAmount = 121.22m;

		tax1Dty.Data.G4_Amount = "26.99";
		tax1Dty.Data.G4_BaseAmount = 1000m;

		tax2Dty.Data.G4_Amount = "2.86";
		tax2Dty.Data.G4_BaseAmount = 105.89m;
		return DocSADHLine.New(entryLine, Factory);
	}

	public void TestTotalLineLiquidations()
	{
		var docLine = GetSADHLineForTotalLineLiquidations();
		AssertEquals("Total line Liquidation should reflect sum of fees with method of payment 1 or 2 for the concerned article.", "251", docLine.TotalLineLiquidation);
	}

	DocSADHLine GetSADHLineForTotalLineLiquidations()
	{
		const string methodOfPayment1 = "1";
		const string methodOfPayment2 = "2";
		const string methodOfPayment3 = "3";
		const string methodOfPayment4 = "4";
		JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_PaymentMethod = "R";
		Factory.Save();

		var invHeader = declaration.Invoices.AddNew();
		var invLine1 = invHeader.InvoiceLines.AddNew();

		var merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		var linefee1 = AddCusEntryLineFee(Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, methodOfPayment1, 29.85m);
		var linefee2 = AddCusEntryLineFee(Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, methodOfPayment2, 221.45m);
		var linefee3 = AddCusEntryLineFee(Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Miscellaneous, methodOfPayment3, 18.45m);
		var linefee4 = AddCusEntryLineFee(Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise, methodOfPayment4, 12.77m);

		CusEntryLineFee AddCusEntryLineFee(ZString feeType, ZString methodOfPayment, ZDecimal chargeAmount)
		{
			var linefee = entryLine.ConfirmedFees.AddOrUpdate(feeType, 0m);
			linefee.CF_MethodOfPayment = methodOfPayment;
			linefee.CF_ChargeAmount = chargeAmount;
			return linefee;
		}

		return DocSADHLine.New(entryLine, Factory);
	}

	public override void TestBox48DeferredPayment()
	{
		Assert(true);
	}

	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return DocSADHLine.New(entryLine, Factory);
	}
	public override void TestSortBox47Taxes()
	{
		var lineWrapper = GetSADHLineForBox47TaxesOrder();
		var box47TaxTypes = lineWrapper.Box47Taxes.Cast<DocSADHLineTax>().Select(x => x.G4_Type).ToArray();
		AssertArrayEqualsByElements(ExpectedTaxTypesOrder, box47TaxTypes);
	}

	public override void TestBox40Contents()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var documentOnHeader = invoiceHeader.PreviousDocuments.AddNew();
		documentOnHeader.CSI_SubType = "X";
		documentOnHeader.CSI_Code = "280";
		documentOnHeader.CSI_ReferenceNumber = "ABCDEFG";

		var documentOnFirstLine = invoiceLine.PreviousDocuments.AddNew();
		documentOnFirstLine.CSI_SubType = EU.Business.PreviousDocumentClassList.Codes.PreviousDocument;
		documentOnFirstLine.CSI_Code = "380";
		documentOnFirstLine.CSI_ReferenceNumber = "3421789012";
		documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);

		var documentOnSecondLine = invoiceLine.PreviousDocuments.AddNew();
		documentOnSecondLine.CSI_SubType = "Y";
		documentOnSecondLine.CSI_Code = "CLE";
		documentOnSecondLine.CSI_ReferenceNumber = "20070701-120-A12345E";
		documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);

		var documentOnGroup = declaration.PreviousDocuments.AddNew();
		documentOnGroup.CSI_SubType = "A";
		documentOnGroup.CSI_Code = "123";
		documentOnGroup.CSI_ReferenceNumber = "987654321";

		var line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("entryLine.Box40PreviousDocuments", "123-A-987654321; 280-X-ABCDEFG; 380-Z-3421789012-02/01/2000; CLE-Y-20070701-120-A12345E-03/01/2000", line.Box40PreviousDocuments);
	}

	public void TestBox35And38Are3decimal()
	{
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceLine invoiceLine;
		GenerateDeclarationForTestingProperties(out declaration, out entryInstruction, out invoiceLine);

		var merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var entryLine = entryHeader.MergedLines[0];
		entryLine.CL_StatisticalValue = 123.46;

		DocSADHLine line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("38 Net Mass in KGs", "2.500", line.Box38NetWeightInKG);
		AssertEquals("35 Gross Mass in KGs", "45.359", line.Box35GrossWeightInKG);

		invoiceLine.JI_Weight = 2m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

		invoiceLine.JI_CustomsQuantity = 5m;

		entryLine = entryHeader.MergedLines[0];
		line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("38 Net Mass in KGs", "5.000", line.Box38NetWeightInKG);
		AssertEquals("35 Gross Mass in KGs", "2.000", line.Box35GrossWeightInKG);

		invoiceLine.JI_Weight = 0;
		entryLine = entryHeader.MergedLines[0];
		line = DocSADHLine.New(entryLine, Factory);
		AssertEquals("35 Gross Mass in KGs should be blank not zero", "", line.Box35GrossWeightInKG);
	}

	protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHLine GetSADHLineForBox47TaxesOrder()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		foreach (var taxType in TaxTypesToTestOrder)
		{
			entryLine.ConfirmedFees.AddNew().G4_Type = taxType;
		}
		return DocSADHLine.New(entryLine, Factory);
	}

	protected override void SetUpForOutwardProcedure()
	{
		zzzDataGrouping = "FR";
		procedureCode = "10";
		previousProcedureCode = "71";
		concession = "F61";
		country = Core.Constants.CountryCodes.France;
		customsRegNo = "FR33159700500064";
		shipmentType = "IMP";
		group = "10P";
	}

	protected override void SetUpForInwardProcedure()
	{
		zzzDataGrouping = "FR";
		procedureCode = "10";
		previousProcedureCode = "71";
		concession = "F61";
		country = Core.Constants.CountryCodes.France;
		customsRegNo = "FR33159700500064";
		shipmentType = "IMP";
		group = "10P";
	}

	protected override ZString ExpectedBox49WarehouseInward => "FR33159700500064";
	protected override ZString ExpectedBox49WarehouseOutward => "FR33159700500064";

	protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHLine GetNewDocSADHLine(EU.Business.Declaration.CusEntryLine entryLine)
	{
		return DocSADHLine.New((CusEntryLine)entryLine, Factory);
	}

	internal static Customs.Business.CusAuthorisationRule CreateCusAuthorisationRule(Customs.Business.CusAuthorisationHeader authHeader, string code, string value)
	{
		var rule = authHeader.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = code;
		rule.CPR_ValueFrom = value;
		return rule;
	}

	public override void TestBox44FiscalReferenceNumberLabel()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var wrapper = DocSADHLine.New(entryLine, Factory);
		AssertEquals("Représentant Fiscal ou Mandataire:", wrapper.Box44FiscalReferenceNumberLabel);
	}

	public override void TestBox44FiscalReferenceNumberSummary()
	{
		var declaration = Factory.New<JobDeclaration>();

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var wrapper = DocSADHLine.New(entryLine, Factory);
		AssertEquals("Empty as ShowBox44FiscalReference is false.", ZString.Empty, wrapper.Box44FiscalReferenceNumberSummary);
		entryLine.CL_LineNumber = 1;
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_Procedure = "1071F61";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var importer = Factory.New<OrgHeader>();
		importer.OH_Code = "IMPORTER";
		declaration.JE_OH_Importer = importer.PK;

		var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
		euAddInfo.ZO_UseFr3FiscalRepresentation = true;

		var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
		fiscalReferenceOrganisation.OH_Code = "FISCALREP";

		var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
		fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
		fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
		fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
		fiscalReferenceOrganisationAddress.OA_PostCode = "333";
		fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		fiscalReference.CFR_Code = "FR3";
		fiscalReference.CFR_Reference = "FR33562024100133";
		fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;

		wrapper = DocSADHLine.New(entryLine, Factory);
		AssertContains("FISCAL ADDRESS1", wrapper.Box44FiscalReference.ToString());
		AssertContains("FISCAL ADDRESS2", wrapper.Box44FiscalReference.ToString());
		AssertContains("FISCAL CITY", wrapper.Box44FiscalReference.ToString());
		AssertContains("333", wrapper.Box44FiscalReference.ToString());
		AssertContains("FRANCE", wrapper.Box44FiscalReference.ToString());

		Assert(wrapper.ShowBox44FiscalReference);

		AssertEquals("Box44FiscalReferenceNumberLabel + Box44FiscalReferenceNumber + Box44FiscalReference + Box44FiscalReference.Country.Code", "Représentant Fiscal ou Mandataire: FR33562024100133 " + wrapper.Box44FiscalReference + " FR", wrapper.Box44FiscalReferenceNumberSummary);
	}

	public void TestBox44AddInfoAndDocumentsAggregated_SecondBoxIsEmpty()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out _, out _, out _, out _, out var line, out _, false, true, false);

			string expected = @"
Mention(s) Spéciale(s): BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; 
CANA(s) : V910; V911
Document(s) joint(s) : HDR1 BILLY; 9100 3278923 10/12/2021
Disposition(s) tarifaire(s) particulière(s) : HNY1; 9120
Taxations spécifiques : quantité : 291.94 - code mesurage : 005".TrimStart(new[] { '\r', '\n' });
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);

			AssertEquals("Box44Aggregated should be empty as Box44Contents has a decent size.", ZString.Empty, line.Box44AddInfoAndDocumentsEnlargedBox);
		}
	}

	public void TestBox44AddInfoAndDocumentsAggregated_SecondBoxIsNotEmpty()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out var document1, out var document2, out var document3, out var document4, out var line, out _, true, true, true);

			document1.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "i");
			document2.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "a");
			document3.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "b");
			document4.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "c");

			document1.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "i");
			document2.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "a");
			document3.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "b");
			document4.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "c");

			var expected = @"
Mention(s) Spéciale(s): BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; 
CANA(s) : V910; V911
Document(s) joint(s) : HDR1 BILLY; 9100 iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii 10/12/2021; 9100 bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb 10/12/2021
Disposition(s) tarifaire(s) particulière(s) : HNY1; 9120; 9120
Taxations spécifiques : quantité : 291.94 - code mesurage : 005".TrimStart(new[] { '\r', '\n' });
			AssertEquals("Box44Aggregated should not be empty, as Box44Contents is originally too big. ", expected, line.Box44AddInfoAndDocumentsEnlargedBox);

			expected = @"
Régime Economique :
° Autorisation Économique (entrée) : TST_ATH_001	° Pays d’Autorisation : GB
° Montant garanti : 19.90
° Délai d’apurement : 9
Nature du perfectionnement, de la transformation ou de l’utilisation des marchandises : N1
Description technique des marchandises et des produits compensateurs ou transformés et les moyens de les identifier : CN Code 123456
Codes relatifs aux conditions économiques conformément à l’annexe 70 : C1
Bureau d’apurement : O1
Lieu de perfectionnement, de transformation ou d’utilisation : L1
Formalités de transfert proposées : T1 Description".TrimStart(new[] { '\r', '\n' });
			AssertEquals("entryLine.Box44Contents is split as the text is too big.", expected, line.Box44AddInfoAndDocuments);
		}
	}

	void GenerateValueForBox44AddInfoAndDocumentsAggregated(out SupportingDocument document1, out SupportingDocument document2, out SupportingDocument document3, out SupportingDocument document4, out DocSADHLine line, out DocSADHLine line2, bool needFiscalReference, bool needVat, bool needeconomicRegime)
	{
		using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var frGroup = helper.CreateNewOrGetExistingDataGrouping("FR");
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var addInfCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HDR1", "COZ WE WANT TO", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HNY1", "HONEY TO THE BEE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { addInfCodeType }, "HIT99", "RANDOM SONG TITLE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9100", "I HAVE NO CLUE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9120", "9120 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			if (needVat)
			{
				var eURCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.EuropeanUnion);
				var aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
				var cNYCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.China);

				CurrencyConverterTestHelper.SetExchangeRate(Factory, eURCurrency, 1m);
				CurrencyConverterTestHelper.SetExchangeRate(Factory, aUDCurrency, 10m);
				CurrencyConverterTestHelper.SetExchangeRate(Factory, cNYCurrency, 20m);
			}

			Factory.Save();

			if (needeconomicRegime)
			{
				var euDtyRateType = helper.CreateCusRateType(frGroup.ZZZ_DataGrouping, Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
				Factory.Save();
				helper.LoadOrCreateNewCusRateCode(Factory, Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, euDtyRateType.PK);
				helper.CreateCusMapType(RefCusMapTypeList.Codes.AUTDC, MapDirectionList.Codes.BTH, "Authorisation document code", false);
				helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "OPO", "C019", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
				helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "IPO", "C601", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
				helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "TEA", "C516", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
				helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CWP", "C517", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
				helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CW1", "C518", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
				helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CW2", "C519", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
				helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "EUS", "N990", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
				Factory.Save();
			}

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			if (needVat || needeconomicRegime)
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			}
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "1071F61";
			invoiceLine.JI_CustomsThirdQuantity = 291.94;
			invoiceLine.JI_CustomsThirdUnitQty = "005";

			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "1071F61";
			invoiceLine2.JI_CustomsThirdQuantity = 291.94;
			invoiceLine2.JI_CustomsThirdUnitQty = "005";

			if (needVat)
			{
				invoiceHeader.JZ_IncoTerm = "FOB";
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

				var charge1 = invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 1000.10m, invoiceHeader.JobDeclaration.LocalCurrencyCode);
				charge1.J7_IsIncludedInITOT = true;

				var charge2 = invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 500.50m, invoiceHeader.JobDeclaration.LocalCurrencyCode);
				charge2.J7_IsIncludedInITOT = true;

				var charge3 = invoiceHeader.Charges.AddNew("CEE", 20.50m, Core.Constants.CurrencyCodes.UnitedStates);
				charge3.J7_IsIncludedInITOT = true;
				var charge4 = invoiceHeader.Charges.AddNew("CEE", 30.5m, Core.Constants.CurrencyCodes.UnitedStates);
				charge4.J7_IsIncludedInITOT = true;

				var charge5 = invoiceHeader.Charges.AddNew("CNE", 10m, Core.Constants.CurrencyCodes.China);
				charge5.J7_IsIncludedInITOT = true;
				var charge6 = invoiceHeader.Charges.AddNew("CNE", 10m, Core.Constants.CurrencyCodes.China);
				charge6.J7_IsIncludedInITOT = true;
				var charge7 = invoiceHeader.Charges.AddNew("CNE", 30m, Core.Constants.CurrencyCodes.Australia);
				charge7.J7_IsIncludedInITOT = true;

				invoiceLine.JI_LinePrice = 6000m;
				invoiceLine2.JI_LinePrice = 2000m;
			}

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			if (needeconomicRegime)
			{
				entryInstruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			}

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			declaration.JE_OH_Importer = importer.PK;

			if (needFiscalReference)
			{
				var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
				euAddInfo.ZO_UseFr3FiscalRepresentation = true;
				var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
				fiscalReferenceOrganisation.OH_Code = "FISCALREP";

				var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
				fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
				fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
				fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
				fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
				fiscalReferenceOrganisationAddress.OA_PostCode = "333";
				fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

				var fiscalReference = entryInstruction.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = "FR3";
				fiscalReference.CFR_Reference = "FR33562024100133";
				fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;
			}

			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;

			var cusAuthorisationHeader = warehouseAddress.Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationHeader.CPH_Type = Enterprise.Customs.FR.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			cusAuthorisationHeader.CPH_OH_PermitHolder = ZGuid.Empty;
			cusAuthorisationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;
			cusAuthorisationHeader.CPH_Number = "00001099";

			var suppDocHeader = invoiceHeader.SupportingDocuments.AddNew();
			suppDocHeader.CSI_Code = "HDR1";
			suppDocHeader.CSI_ReferenceNumber = "BILLY";
			suppDocHeader.CSI_SubType = "X";
			suppDocHeader.CSI_Quantity = 99;
			suppDocHeader.CSI_Description = "COZ WE WANT TO";

			var addInfoHeader = invoiceHeader.AdditionalInfos.AddNew();
			addInfoHeader.CSI_Code = "HIT99";
			addInfoHeader.CSI_Description = "RANDOM SONG TITLE";

			var suppDocGroup = declaration.SupportingDocuments.AddNew();
			suppDocGroup.CSI_Code = "HNY1";
			suppDocGroup.CSI_ReferenceNumber = "PIPER";
			suppDocGroup.CSI_SubType = "2";
			suppDocGroup.CSI_Quantity = 22;
			suppDocGroup.CSI_Description = "HONEY TO THE BEE";
			suppDocGroup.CSI_IsDTP = true;

			var addInfoGroup = declaration.AdditionalInfos.AddNew();
			addInfoGroup.CSI_Code = "BOB21";
			addInfoGroup.CSI_Description = "MADE SENSE AT THE TIME";

			var addInfo1 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "GEN13";
			addInfo1.CSI_Description = "PLENTY OF COATS";

			var addInfo2 = invoiceLine2.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "PAL01";

			var cana1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V911";

			var cana2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V910";

			document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "9100";
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";
			document1.CSI_DateOfIssue = new ZDate(2021, 12, 10);

			document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "9120";
			document2.CSI_ReferenceNumber = "45982309";
			document2.CSI_Description = "9120 Test";
			document2.CSI_IsDTP = true;

			var addInfo3 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "GEN13";
			addInfo1.CSI_Description = "PLENTY OF COATS";

			var addInfo4 = invoiceLine2.AdditionalInfos.AddNew();
			addInfo4.CSI_Code = "PAL01";

			var cana3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana3.CY_Code = "V911";

			var cana4 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana4.CY_Code = "V910";

			document3 = invoiceLine.SupportingDocuments.AddNew();
			document3.CSI_Code = "9100";
			document3.CSI_ReferenceNumber = "3278923";
			document3.CSI_SubType = "1";
			document3.CSI_Quantity = 12;
			document3.CSI_Description = "I HAVE NO CLUE";
			document3.CSI_DateOfIssue = new ZDate(2021, 12, 10);

			document4 = invoiceLine.SupportingDocuments.AddNew();
			document4.CSI_Code = "9120";
			document4.CSI_ReferenceNumber = "45982309";
			document4.CSI_Description = "9120 Test";
			document4.CSI_IsDTP = true;

			if (needeconomicRegime)
			{
				var customer = Factory.NewWithValidTestData<OrgHeader>();
				customer.OH_RL_NKClosestPort = "GBLON";
				var authHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
				authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
				authHeader.CPH_OH_PermitHolder = customer.PK;
				authHeader.CPH_Number = "1234";
				authHeader.CPH_PermitDescription = "CN Code 123456";
				authHeader.CPH_StartDate = ZDate.Today;
				authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
				authHeader.CPH_IsActive = true;

				var autRule = authHeader.CusAuthorisationRules.AddNew();
				autRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
				autRule.CPR_ValueFrom = "TST_ATH_001";

				var usage = entryInstruction.CusAuthorizationUsages.AddNew();
				usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
				usage.AGC_Number = "1234";
				usage.AGC_OH_Owner = customer.PK;

				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.NAT, "N1");
				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.CON, "C1");
				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.OFC, "O1").CPR_Description = "Square";
				CreateCusAuthorisationRule(authHeader, Enterprise.Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "L1").CPR_Description = "Hidden";
				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.TRA, "T1").CPR_Description = "T1 Description";
				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.INF, "I1");
				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.STO, "9");
				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCD, "100");
				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCV, "5");
				CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCP, "15");
				entryLine.Fees.AddOrUpdate(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 12); // Duty: 12.46m
				entryLine.Fees.AddOrUpdate(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 29); // VAT: 29.12m
				entryLine.Fees.AddOrUpdate("A387", 43).NationalFeeTypeCode = "A387";  // ParaFiscal: 43.34m
			}

			line = DocSADHLine.New(entryLine, Factory, true);
			line2 = DocSADHLine.New(entryLine, Factory, false);
		}
	}

	ZString GenerateBigString(int numberOfChar, string charToAdd)
	{
		var result = new ZString();
		for (int i = 0; i < numberOfChar; i++)
		{
			result += charToAdd;
		}
		return result;
	}

	public void TestNeedASecondPageForBox44_false()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out _, out _, out _, out _, out var firstLine, out var secondLine, false, true, false);

			AssertEquals("Prerequisite: Box44AddInfoAndDocumentsEnlargedBox is empty and text length is shorter than max length.", true, firstLine.Box44AddInfoAndDocumentsEnlargedBox.IsEmpty && (firstLine.Box44AddInfoAndDocuments.Length + firstLine.Box44FiscalReferenceNumberSummary.Length + firstLine.Box44VatInfoCalculation.Length < Box44AddInfoAndDocumentsHelper.Box44MaxLength));
			AssertEquals("Box44AddInfoAndDocumentsEnlargedBox is empty and didn't exceed max length then NeedASecondPageForBox44 is false.", false, firstLine.NeedASecondPageForBox44);

			AssertEquals("Prerequisite: Box44AddInfoAndDocumentsEnlargedBox is empty and text length is shorter than max length.", true, secondLine.Box44AddInfoAndDocumentsEnlargedBox.IsEmpty && (firstLine.Box44AddInfoAndDocuments.Length + firstLine.Box44VatInfoCalculation.Length < Box44AddInfoAndDocumentsHelper.Box44MaxLength));
			AssertEquals("Box44AddInfoAndDocumentsEnlargedBox is empty and didn't exceed max length then NeedASecondPageForBox44 is false.", false, firstLine.NeedASecondPageForBox44);
		}
	}

	public void TestNeedASecondPageForBox44_true()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out var document1, out var document2, out var document3, out var document4, out var firstLine, out var secondLine, false, true, true);

			document1.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "i");
			document2.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "a");
			document3.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "b");
			document4.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "c");

			document1.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "i");
			document2.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "a");
			document3.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "b");
			document4.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "c");
			Factory.ClearCachedValue<ZString[]>("box44GlobalContent");
			AssertEquals("Box44AddInfoAndDocumentsEnlargedBox is not empty then NeedASecondPageForBox44 is true.", true, firstLine.NeedASecondPageForBox44);
			AssertEquals("Box44AddInfoAndDocumentsEnlargedBox is not empty then NeedASecondPageForBox44 is true.", true, secondLine.NeedASecondPageForBox44);
		}
	}

	public void TestSwitchBox44_NoSwitchOn()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out _, out _, out _, out _, out var line, out _, false, false, false);
			Assert("prerequisite : Box44AddInfoAndDocuments is 307, Box44FiscalReferenceNumberSummary is 0 and Box44VatInfoCalculation is 0:", line.Box44AddInfoAndDocuments.Length + line.Box44FiscalReferenceNumberSummary.Length + line.Box44VatInfoCalculation.Length < Box44AddInfoAndDocumentsHelper.Box44MaxLength);
			AssertEquals("prerequisite : Box44AddInfoAndDocumentsEnlargedBox is empty:", 0, line.Box44AddInfoAndDocumentsEnlargedBox.Length);

			AssertEquals("Every switch is false => SwitchFiscalReferenceToSecondBox44.", false, line.SwitchFiscalReferenceToEnlargedBox44);
			AssertEquals("Every switch is false => SwitchBox44VatInfoCalculationForBox44.", false, line.SwitchVatInfoCalculationToEnlargedBox44);
			AssertEquals("Every switch is false => SwitchVatInfoCalculationToEnlargedBox44ForTripleBox44Page.", false, line.SwitchVatInfoCalculationToEnlargedBox44ForTripleBox44Page);
		}
	}

	public void TestSwitchFiscalReferenceToEnlargedBox44()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out _, out _, out _, out _, out var firstLine, out var secondLine, true, false, true);

			Assert("prerequisite : Text is superior than the max length:", firstLine.Box44AddInfoAndDocuments.Length + firstLine.Box44FiscalReferenceNumberSummary.Length + firstLine.Box44VatInfoCalculation.Length > Box44AddInfoAndDocumentsHelper.Box44MaxLength);
			AssertEquals("SwitchFiscalReferenceToSecondBox44 is true.", true, firstLine.SwitchFiscalReferenceToEnlargedBox44);

			AssertEquals("SwitchFiscalReferenceToSecondBox44 would always be false for all except for first line.", false, secondLine.SwitchFiscalReferenceToEnlargedBox44);
		}
	}

	public void TestSwitchVatInfoCalculationToEnlargedBox44_On()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out var document1, out var document2, out var document3, out var document4, out var line, out _, false, true, true);
			document1.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "i");
			document2.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "a");
			document3.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "b");
			document4.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "c");

			document1.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "i");
			document2.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "a");
			document3.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "b");
			document4.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "c");

			Assert("prerequisite: Text is superior than the max length: :", line.Box44AddInfoAndDocuments.Length + line.Box44FiscalReferenceNumberSummary.Length + line.Box44VatInfoCalculation.Length > Box44AddInfoAndDocumentsHelper.Box44MaxLength);

			AssertEquals("SwitchBox44VatInfoCalculationForBox44 is true.", true, line.SwitchVatInfoCalculationToEnlargedBox44);
		}
	}

	public void TestSwitchVatInfoCalculationToEnlargedBox44ForTripleBox44Page_On()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out var document1, out var document2, out var document3, out var document4, out _, out var secondLine, false, true, true);

			Assert("prerequisite: Text is superior than the max length: :", secondLine.Box44AddInfoAndDocuments.Length + secondLine.Box44VatInfoCalculation.Length > Box44AddInfoAndDocumentsHelper.Box44MaxLength);

			AssertEquals("SwitchBox44VatInfoCalculationForBox44 is true.", true, secondLine.SwitchVatInfoCalculationToEnlargedBox44);
		}
	}

	public void TestSwitchBox44_AddInfoAndDocumentsToSecondSwitchOn()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out var document1, out var document2, out var document3, out var document4, out var line, out _, false, false, true);

			document1.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "i");
			document2.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "a");
			document3.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "b");
			document4.CSI_ReferenceNumber = GenerateBigString(document1.CSI_ReferenceNumberInfo.MaxLength, "c");

			document1.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "i");
			document2.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "a");
			document3.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "b");
			document4.CSI_Description = GenerateBigString(document1.CSI_DescriptionInfo.MaxLength, "c");

			Assert("prerequisite : Box44AddInfoAndDocumentsAggregated is not empty :", line.Box44AddInfoAndDocumentsEnlargedBox.Length + line.Box44AddInfoAndDocuments.Length > Box44AddInfoAndDocumentsHelper.Box44MaxLength);

			AssertEquals("SwitchFiscalReferenceToSecondBox44 is true when SwitchBox44AddInfoAndDocumentsToSecondBox44 is true.", true, line.SwitchFiscalReferenceToEnlargedBox44);
			AssertEquals("SwitchBox44VatInfoCalculationForBox44 is true when SwitchBox44AddInfoAndDocumentsToSecondBox44 is true.", true, line.SwitchVatInfoCalculationToEnlargedBox44);
		}
	}
}
