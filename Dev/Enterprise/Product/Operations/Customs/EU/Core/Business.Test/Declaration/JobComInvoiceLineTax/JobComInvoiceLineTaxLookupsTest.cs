using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class JobComInvoiceLineTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxMOPList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var gBImportMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Immediate payment by cash or equivalent (Paper declarations)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var gBExportMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "L", "CAP Export Licence", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			var dEMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "D", "Andere (z.B. Abbuchung vom Konto eines Zollagenten)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(dEMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			var iTMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "T", "Garanzia sul conto dello spedizioniere doganale", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(iTMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var tax = invoiceLine.Taxes.AddNew();

			declaration.Company.SetCountry(Core.Constants.CountryCodes.Germany);
			AssertEquals("D", tax.Lookups.MOPList.CodesAsString);

			declaration.Company.SetCountry(Core.Constants.CountryCodes.Italy);
			AssertEquals("T", tax.Lookups.MOPList.CodesAsString);

			declaration.Company.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("A", tax.Lookups.MOPList.CodesAsString);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("L", tax.Lookups.MOPList.CodesAsString);
		}

		public void TestTaxBaseQuantityUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var europeanUnion = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: europeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: europeanUnion);

			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "EuropeanUnionTestCode", "Test 1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UnitedKingdomTestCode", "Test 2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var tax = invoiceLine.Taxes.AddNew();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AssertContains("Should contain 'EuropeanUnionTestCode'", "EuropeanUnionTestCode", tax.Lookups.BaseQuantityUQList.CodesAsString);
				AssertNotContains("Should not contain 'UnitedKingdomTestCode'", "UnitedKingdomTestCode", tax.Lookups.BaseQuantityUQList.CodesAsString);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertContains("Should contain 'EuropeanUnionTestCode'", "EuropeanUnionTestCode", tax.Lookups.BaseQuantityUQList.CodesAsString);
				AssertContains("Should contain 'UnitedKingdomTestCode'", "UnitedKingdomTestCode", tax.Lookups.BaseQuantityUQList.CodesAsString);
			}
		}

		public virtual void TestSortTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunId);
			Factory.Save();
			helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DUT", "Duty");
			var dut = helper.CreateNewOrGetExistingRateType(countryCode, "DUT", "Duty");
			var msc = helper.CreateNewOrGetExistingRateType(countryCode, "MSC", "Miscellaneous");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "411", msc.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "E00", msc.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var tax = invoiceLine.Taxes.AddNew();
			var typeList = (CodeDescriptionPairList)tax.Lookups.TypeList;
			var indexA00 = typeList.IndexOf(typeList.ToArray().FirstOrDefault(t => t.Code.StartsWith("A00")));
			var indexE00 = typeList.IndexOf(typeList.ToArray().FirstOrDefault(t => t.Code.StartsWith("E00")));
			var index411 = typeList.IndexOf(typeList.ToArray().FirstOrDefault(t => t.Code.StartsWith("411")));
			Assert("A00 and B00 will be used more often than numeric codes, it should look like: A00, A10...B00, B10...411, 412...", indexA00 < indexE00 && indexE00 < index411);
		}

		public void TestUseInternalTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Env.CurrentCompany.Country.Code;
			var euId = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: euId);
			Factory.Save();
			var euImpDtyRateType = helper.CreateCusRateType(euId.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			var euExpDtyRateType = helper.CreateCusRateType(euId.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.ExportTaxes);

			helper.LoadOrCreateNewCusRateCode(Factory, "A00", euImpDtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "C00", euExpDtyRateType.PK);
			var adfmr = helper.LoadOrCreateNewCusRateCode(Factory, "ADFMR", euImpDtyRateType.PK, internalUse: true);
			var adszr = helper.LoadOrCreateNewCusRateCode(Factory, "ADSZR", euImpDtyRateType.PK, internalUse: true);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];
			var cusEntryLineFee = entryLine.Fees.AddNew();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertContains("A00", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
			AssertNotContains("ADFMR", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
			AssertNotContains("ADSZR", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
			AssertNotContains("C00", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertNotContains("A00", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
			AssertNotContains("ADFMR", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
			AssertNotContains("ADSZR", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
			AssertContains("C00", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
		}
	}
}
