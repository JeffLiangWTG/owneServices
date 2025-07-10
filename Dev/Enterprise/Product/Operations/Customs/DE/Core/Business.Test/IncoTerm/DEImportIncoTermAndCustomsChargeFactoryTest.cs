using System.IO;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using TransportTypeList = Enterprise.Customs.Business.TransportTypeList;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DEImportIncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Count", 13, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		public override void TestGetAllCharges()
		{
			AssertEquals("There should be 26 charges", 26, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(CustomsChargeTypeList.Codes.Discount, CustomsChargeCodeProvider.Discount);
			AssertGetCharge(ChargeTypeList.Codes.StatisticalValue, ChargeCodeProvider.StatisticalValue);
		}

		public override void TestFactoryType()
		{
			AssertType<DEImportIncoTermAndCustomsChargeFactory>(incoTermAndChargeFactory);
		}

		public override void TestSetupToEUBorderCharge()
		{
			var importIncoTermAndChargeFactory = (DEImportIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew(ImportChargeCodeList.Codes._010);
			importIncoTermAndChargeFactory.SetupToEUBorderCharge(invoiceCharge, 100, Core.Constants.CurrencyCodes.EuropeanUnion);
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew(ImportChargeCodeList.Codes._010);
			importIncoTermAndChargeFactory.SetupToEUBorderCharge(groupInvoiceCharge, 200, Core.Constants.CurrencyCodes.UnitedKingdom);

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceCharge.J7_IsDutiable", true, invoiceCharge.J7_IsDutiable);
				AssertEquals("InvoiceCharge.J7_IsStatisticalValueApplicable", true, invoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("InvoiceCharge.J7_IsGSTApplicable", true, invoiceCharge.J7_IsGSTApplicable);
				AssertEquals("InvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount", false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("InvoiceCharge.J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("GroupInvoiceCharge.J7_IsDutiable", true, groupInvoiceCharge.J7_IsDutiable);
				AssertEquals("GroupInvoiceCharge.J7_IsStatisticalValueApplicable", true, groupInvoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("GroupInvoiceCharge.J7_IsGSTApplicable", true, groupInvoiceCharge.J7_IsGSTApplicable);
				AssertEquals("GroupInvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount", false, groupInvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("GroupInvoiceCharge.J7_IsIncludedInITOT", false, groupInvoiceCharge.J7_IsIncludedInITOT);
			});
		}

		public void TestSetupToEUBorderCharge_IsFreightChargeToEUBorderByAirInsideEU()
		{
			var importIncoTermAndChargeFactory = (DEImportIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
			var invoiceCharge = invoice.Charges.AddNew(ImportChargeCodeList.Codes._010);
			importIncoTermAndChargeFactory.SetupToEUBorderCharge(invoiceCharge, 100, Core.Constants.CurrencyCodes.EuropeanUnion);
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew(ImportChargeCodeList.Codes._010);
			importIncoTermAndChargeFactory.SetupToEUBorderCharge(groupInvoiceCharge, 200, Core.Constants.CurrencyCodes.UnitedKingdom);

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceCharge.J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("InvoiceCharge.J7_IsStatisticalValueApplicable", false, invoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("InvoiceCharge.J7_IsGSTApplicable", false, invoiceCharge.J7_IsGSTApplicable);
				AssertEquals("InvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount", false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("InvoiceCharge.J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("GroupInvoiceCharge.J7_IsDutiable", false, groupInvoiceCharge.J7_IsDutiable);
				AssertEquals("GroupInvoiceCharge.J7_IsStatisticalValueApplicable", false, groupInvoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("GroupInvoiceCharge.J7_IsGSTApplicable", false, groupInvoiceCharge.J7_IsGSTApplicable);
				AssertEquals("GroupInvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount", false, groupInvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("GroupInvoiceCharge.J7_IsIncludedInITOT", false, groupInvoiceCharge.J7_IsIncludedInITOT);
			});
		}

		public override void TestSetupAfterEUBorderCharge()
		{
			var importIncoTermAndChargeFactory = (DEImportIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew(ImportChargeCodeList.Codes._014);
			importIncoTermAndChargeFactory.SetupAfterEUBorderCharge(invoiceCharge, 100, Core.Constants.CurrencyCodes.EuropeanUnion);
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew(ImportChargeCodeList.Codes._014);
			importIncoTermAndChargeFactory.SetupAfterEUBorderCharge(groupInvoiceCharge, 200, Core.Constants.CurrencyCodes.UnitedKingdom);

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceCharge.J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("InvoiceCharge.J7_IsStatisticalValueApplicable", false, invoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("InvoiceCharge.J7_IsGSTApplicable", true, invoiceCharge.J7_IsGSTApplicable);
				AssertEquals("InvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount", true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("InvoiceCharge.J7_IsIncludedInITOT", true, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("GroupInvoiceCharge.J7_IsDutiable", false, groupInvoiceCharge.J7_IsDutiable);
				AssertEquals("GroupInvoiceCharge.J7_IsStatisticalValueApplicable", false, groupInvoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("GroupInvoiceCharge.J7_IsGSTApplicable", true, groupInvoiceCharge.J7_IsGSTApplicable);
				AssertEquals("GroupInvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount", true, groupInvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("GroupInvoiceCharge.J7_IsIncludedInITOT", true, groupInvoiceCharge.J7_IsIncludedInITOT);
			});
		}

		public void TestSetupAfterEUBorderCharge_IsFreightChargeAfterEUBorderByAirOutsideEU()
		{
			var importIncoTermAndChargeFactory = (DEImportIncoTermAndCustomsChargeFactory)incoTermAndChargeFactory;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;
			var invoiceCharge = invoice.Charges.AddNew(ImportChargeCodeList.Codes._014);
			importIncoTermAndChargeFactory.SetupAfterEUBorderCharge(invoiceCharge, 100, Core.Constants.CurrencyCodes.EuropeanUnion);
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew(ImportChargeCodeList.Codes._014);
			importIncoTermAndChargeFactory.SetupAfterEUBorderCharge(groupInvoiceCharge, 200, Core.Constants.CurrencyCodes.UnitedKingdom);

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceCharge.J7_IsDutiable", false, invoiceCharge.J7_IsDutiable);
				AssertEquals("InvoiceCharge.J7_IsStatisticalValueApplicable", false, invoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("InvoiceCharge.J7_IsGSTApplicable", true, invoiceCharge.J7_IsGSTApplicable);
				AssertEquals("InvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount", false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("InvoiceCharge.J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
				AssertEquals("GroupInvoiceCharge.J7_IsDutiable", false, groupInvoiceCharge.J7_IsDutiable);
				AssertEquals("GroupInvoiceCharge.J7_IsStatisticalValueApplicable", false, groupInvoiceCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("GroupInvoiceCharge.J7_IsGSTApplicable", true, groupInvoiceCharge.J7_IsGSTApplicable);
				AssertEquals("GroupInvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount", false, groupInvoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("GroupInvoiceCharge.J7_IsIncludedInITOT", false, groupInvoiceCharge.J7_IsIncludedInITOT);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => Path.Combine(BaseSourcePath, TestHelper.BusinessTestDirectory, @"IncoTerm\Testing\DEImportIncoTermAndCustomsChargeConfiguration.csv");

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Germany + Common.Shared.SharedJobMessageTypeList.Codes.Import;

		protected override string InsuranceChargeCode => ImportChargeCodeList.Codes._012;

		public void TestFreightToEUBorderDescription()
		{
			AssertEquals(ImportChargeCodeList.Descriptions._010, incoTermAndChargeFactory.GetCharge(ImportChargeCodeList.Codes._010).Description);
		}
	}
}
