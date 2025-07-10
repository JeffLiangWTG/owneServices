using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLineFee()
		{
			var parent = Factory.New<CusEntryLineFee>();
			AssertEquals(parent.Lookups.EntryLineFee, parent);
		}

		public void TestNationalFeeTypeCodeList()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			AssertNullOrEmpty(entryLineFee.Lookups.NationalFeeTypeCodeList.CodesAsString);
		}

		public void TestChargeTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunId);
			helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty, "Duty");
			var dut = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty, "Duty");
			var exp = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.ExportTaxes, "Export Taxes");
			var moe = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.MiscellaneousOnlyForExport, "Miscellaneous Only for Export");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "350", exp.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "149", moe.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryLineFee = entryLine.Fees.AddNew();

			CombineAssertions("When JE_MessageType = EXP", () =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var chargeTypeList = entryLineFee.Lookups.ChargeTypeList;
				AssertEquals("Contains A00", false, chargeTypeList.ContainsCode("A00"));
				AssertEquals("Contains B00", true, chargeTypeList.ContainsCode("B00"));
				AssertEquals("Contains 149", true, chargeTypeList.ContainsCode("149"));
				AssertEquals("Contains 350", true, chargeTypeList.ContainsCode("350"));
			});

			CombineAssertions("When JE_MessageType = IMP", () =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var chargeTypeList = entryLineFee.Lookups.ChargeTypeList;
				AssertEquals("Contains A00", true, chargeTypeList.ContainsCode("A00"));
				AssertEquals("Contains B00", true, chargeTypeList.ContainsCode("B00"));
				AssertEquals("Contains 149", false, chargeTypeList.ContainsCode("149"));
				AssertEquals("Contains 350", false, chargeTypeList.ContainsCode("350"));
			});
		}

		public void TestRateOverrideReasonList()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			AssertEquals("ADD, OVR", entryLineFee.Lookups.RateOverrideReasonList.CodesAsString);
		}

		public void TestMethodOfCalculationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var europeanUnion = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: europeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: europeanUnion);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "EuropeanUnionTestCode", "Test 1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UnitedKingdomTestCode", "Test 2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];
			var cusEntryLineFee = entryLine.Fees.AddNew();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				AssertContains("Should contain 'EuropeanUnionTestCode'", "EuropeanUnionTestCode", cusEntryLineFee.Lookups.MethodOfCalculationList.CodesAsString);
				AssertNotContains("Should not contain 'UnitedKingdomTestCode'", "UnitedKingdomTestCode", cusEntryLineFee.Lookups.MethodOfCalculationList.CodesAsString);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertContains("Should contain 'EuropeanUnionTestCode'", "EuropeanUnionTestCode", cusEntryLineFee.Lookups.MethodOfCalculationList.CodesAsString);
				AssertContains("Should contain 'UnitedKingdomTestCode'", "UnitedKingdomTestCode", cusEntryLineFee.Lookups.MethodOfCalculationList.CodesAsString);
			}
		}

		public void TestMethodOfPaymentList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var gBImportMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Immediate payment by cash or equivalent (Paper declarations)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var gBExportMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "L", "CAP Export Licence", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			var esMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Z", "Costes no prepagados", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(esMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];
			var cusEntryLineFee = entryLine.Fees.AddNew();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				AssertEquals("Z", cusEntryLineFee.Lookups.MethodOfPaymentList.CodesAsString);
			}

			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			merger = new LineMerger(declaration);
			merger.DoMerge();
			entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];
			cusEntryLineFee = entryLine.Fees.AddNew();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				AssertEquals("A", cusEntryLineFee.Lookups.MethodOfPaymentList.CodesAsString);
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				AssertEquals("L", cusEntryLineFee.Lookups.MethodOfPaymentList.CodesAsString);
			}
		}

		public void TestEntryLineFeeTypeVAT()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryLineFee = entryLine.Fees.AddNew();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertContains("Should contain 'B00' when MessageType is 'Export'", "B00", entryLineFee.Lookups.ChargeTypeList.CodesAsString);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertContains("Should contain 'B00' when MessageType is 'Import'", "B00", entryLineFee.Lookups.ChargeTypeList.CodesAsString);
		}
	}
}
