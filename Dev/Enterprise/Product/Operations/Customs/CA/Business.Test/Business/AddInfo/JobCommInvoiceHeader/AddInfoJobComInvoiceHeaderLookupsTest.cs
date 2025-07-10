using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoJobComInvoiceHeaderLookupsTest : CAAddInfoLookupsTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			return new AddInfoJobComInvoiceHeader(invoice.JZ_AddInfoInfo);
		}

		public void TestMoreProperties()
		{
			AssertEquals(typeof(OrgHeaderCollection), addInfoInvoiceHeader.Lookups.Organisations.GetType());
			AssertEquals(typeof(USStatesList), addInfoInvoiceHeader.Lookups.StatesOfExport.GetType());
			AssertEquals(typeof(TimeLimitUnitCodes), addInfoInvoiceHeader.Lookups.TimeLimitUnits.GetType());
			AssertEquals(typeof(ZZRefCarrierCombinedCollection), addInfoInvoiceHeader.Lookups.CarrierCodes.GetType());
		}

		public void TestStatesOfOrigin()
		{
			addInfoInvoiceHeader.Parent.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(typeof(CanadianProvinceList), addInfoInvoiceHeader.Lookups.StatesOfOrigin.GetType());
			addInfoInvoiceHeader.Parent.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(typeof(USStatesList), addInfoInvoiceHeader.Lookups.StatesOfOrigin.GetType());
			addInfoInvoiceHeader.Parent.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(typeof(CodeDescriptionPairList), addInfoInvoiceHeader.Lookups.StatesOfOrigin.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			addInfoInvoiceHeader.Parent.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(typeof(CanadianProvinceList), addInfoInvoiceHeader.Lookups.StatesOfOrigin.GetType());
			addInfoInvoiceHeader.Parent.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(typeof(CanadianProvinceList), addInfoInvoiceHeader.Lookups.StatesOfOrigin.GetType());
			addInfoInvoiceHeader.Parent.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(typeof(CanadianProvinceList), addInfoInvoiceHeader.Lookups.StatesOfOrigin.GetType());
		}

		public void TestCheckCA_TreatmentCodeWhenFTZfieldEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry("02", "02", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("28", "28", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("29", "29", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("05", "05", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("11", "11", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("32", "32", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("08", "08", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("24", "24", Core.Constants.CountryCodes.Canada);

			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "02");
			helper.AddCountry(tradeGroup, "A!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "28");
			helper.AddCountry(tradeGroup, "A!");
			helper.AddCountry(tradeGroup, "B!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "29");
			helper.AddCountry(tradeGroup, "A!");

			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "05");
			helper.AddCountry(tradeGroup, "B!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "11");
			helper.AddCountry(tradeGroup, "B!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "32");
			helper.AddCountry(tradeGroup, "B!");

			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "08");
			helper.AddCountry(tradeGroup, "C!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "24");
			helper.AddCountry(tradeGroup, "C!");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RN_NKDefaultOrigin = "A!";
			invoiceHeader.CA_RN_NKExport = "B!";

			invoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("Show common valid Tariff Treatments Codes Count - 1", 1, invoiceHeader.AddInfoLookups.TreatmentCodes.Count);
			AssertEquals("Show common valid Tariff Treatments Codes - 28", "28", invoiceHeader.AddInfoLookups.TreatmentCodes.CodesAsString);

			invoiceHeader.CA_TradeZone = "106D";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "C!";
			AssertEquals("Show valid Treatments Codes based on COO - 2", 2, invoiceHeader.AddInfoLookups.TreatmentCodes.Count);
			AssertEquals("Show valid Tariff Treatments Codes - 08, 24", "08, 24", invoiceHeader.AddInfoLookups.TreatmentCodes.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			addInfoInvoiceHeader = (AddInfoJobComInvoiceHeader)GetNewAddInfo();
		}

		AddInfoJobComInvoiceHeader addInfoInvoiceHeader;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
	}
}
