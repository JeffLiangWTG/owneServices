using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using CusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportChargesModeOfPayment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: euGroup);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateCusCodeType(CusCodeListTypes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, CusCodeListTypes.EUTransportChargesMethodOfPayment, "A", "Test 1", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, CusCodeListTypes.EUTransportChargesMethodOfPayment, "B", "Test 2", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, CusCodeListTypes.EUTransportChargesMethodOfPayment, "C", "Test 2", yesterday, tomorrow);
			Factory.Save();
			AssertEquals("Should contain CusCodeList items of type MOP from LV and EU", "A, B", lookups.TransportChargesMethodOfPaymentList.CodesAsString);
		}

		public void TestAgreedPlaceCodeList_NotUCC6()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeType(CusCodeListTypes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, CusCodeListTypes.IncoTermKey, "1", "LV Code 1", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, CusCodeListTypes.IncoTermKey, "2", "EUN Code", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, CusCodeListTypes.IncoTermKey, "3", "LV Code 3", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, CusCodeListTypes.IncoTermKey, "4", "DE Code 4", yesterday, tomorrow);
			Factory.Save();

			var agreedPlaceCodeList = (CodeDescriptionPairList)lookups.AgreedPlaceCodeList;
			AssertEquals("Should contain CusCodeList items of type INKEY from LV", "1, 3", agreedPlaceCodeList.CodesAsString);
		}

		public void TestAgreedPlaceCodeList_Type()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invoice = declaration.Invoices.AddNew();
				CombineAssertions(() =>
				{
					invoice.ZG_AgreedPlaceCode = ZString.Empty;
					AssertType<RefUNLOCOCollection>("Type is RefUNLOCOCollection for empty ZG_AgreedPlaceCode", invoice.AddInfoLookups.AgreedPlaceCodeList);

					invoice.ZG_AgreedPlaceCode = "1";
					AssertType<RefUNLOCOCollection>("Type is RefUNLOCOCollection for ZG_AgreedPlaceCode Length = 1", invoice.AddInfoLookups.AgreedPlaceCodeList);

					invoice.ZG_AgreedPlaceCode = "XX";
					AssertType<RefCountryCollection>("Type is RefCountryCollection for ZG_AgreedPlaceCode Length = 2", invoice.AddInfoLookups.AgreedPlaceCodeList);

					invoice.ZG_AgreedPlaceCode = "123";
					AssertType<RefUNLOCOCollection>("Type is RefUNLOCOCollection for ZG_AgreedPlaceCode Length > 2", invoice.AddInfoLookups.AgreedPlaceCodeList);
				});
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var invoice = declaration.Invoices.AddNew();
				invoice.ZG_AgreedPlaceCode = ZString.Empty;
				AssertType<CodeDescriptionPairList>("Type is CodeDescriptionPairList for empty AgreedPlaceCodeSupport disabled", invoice.AddInfoLookups.AgreedPlaceCodeList);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceHeaderAddInfo = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
			lookups = new AddInfoJobComInvoiceHeaderLookups(invoiceHeaderAddInfo);
		}
		AddInfoJobComInvoiceHeaderLookups lookups;
	}
}
