using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoJobComInvoiceLineLookupsTest : CAAddInfoLookupsTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
		}

		public void TestRemissionTypeList()
		{
			AssertEquals(typeof(RemissionTypeList), parent.Lookups.RemissionTypeList.GetType());
			AssertEquals(7, parent.Lookups.RemissionTypeList.Count);

			var declaration = parent.Declaration;
			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var remissionTypeLie = parent.Lookups.RemissionTypeList;
			AssertEquals(typeof(RemissionTypeList), remissionTypeLie.GetType());
			AssertEquals(3, remissionTypeLie.Count);
			AssertEquals(true, remissionTypeLie.ContainsCode(RemissionTypeList.Codes.DutiesReliefProgramLicense));
			AssertEquals(true, remissionTypeLie.ContainsCode(RemissionTypeList.Codes.OrderInCouncil));
			AssertEquals(true, remissionTypeLie.ContainsCode(RemissionTypeList.Codes.Permit));
		}

		public void TestTreatmentCodesWithFTZEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry("02", "02", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("28", "28", Core.Constants.CountryCodes.Canada);
			helper.CreatePreferenceForCountry("29", "29", Core.Constants.CountryCodes.Canada);

			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "02");
			helper.AddCountry(tradeGroup, "A!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "28");
			helper.AddCountry(tradeGroup, "A!");
			helper.AddCountry(tradeGroup, "B!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "29");
			helper.AddCountry(tradeGroup, "A!");
			helper.AddCountry(tradeGroup, "B!");

			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "05");
			helper.AddCountry(tradeGroup, "B!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "11");
			helper.AddCountry(tradeGroup, "B!");
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "32");
			helper.AddCountry(tradeGroup, "B!");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var countryOfOrigin = Factory.New<CACountryPreference>();
			countryOfOrigin.CA_CountryCode = "A!";
			countryOfOrigin.CA_ValidTariffTreatments = "02,28,29";
			invoiceHeader.JZ_RN_NKDefaultOrigin = countryOfOrigin.CA_CountryCode;
			var countryOfExport = Factory.New<CACountryPreference>();
			countryOfExport.CA_CountryCode = "B!";
			countryOfExport.CA_ValidTariffTreatments = "05,11,28,29,32";
			invoiceHeader.CA_RN_NKExport = countryOfExport.CA_CountryCode;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			AssertEquals("Test for valid TT count (no FTZ)", 2, invoiceLine.AddInfoLookups.TreatmentCodes.Count);
			AssertEquals("Test for valid TTs (no FTZ)", "28, 29", invoiceLine.AddInfoLookups.TreatmentCodes.CodesAsString);
			invoiceHeader.CA_TradeZone = "123A";
			AssertEquals("Test for valid TT count", 3, invoiceLine.AddInfoLookups.TreatmentCodes.Count);
			AssertEquals("Test for valid TTs", "02, 28, 29", invoiceLine.AddInfoLookups.TreatmentCodes.CodesAsString);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(USStatesList), parent.Lookups.StatesOfExport.GetType());
			parent.Parent.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(typeof(CanadianProvinceList), parent.Lookups.StatesOfOrigin.GetType());
			parent.Parent.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(typeof(USStatesList), parent.Lookups.StatesOfOrigin.GetType());
			parent.Parent.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(typeof(CodeDescriptionPairList), parent.Lookups.StatesOfOrigin.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			parent.Parent.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(typeof(CanadianProvinceList), parent.Lookups.StatesOfOrigin.GetType());
			parent.Parent.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(typeof(CanadianProvinceList), parent.Lookups.StatesOfOrigin.GetType());
			parent.Parent.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(typeof(CanadianProvinceList), parent.Lookups.StatesOfOrigin.GetType());
		}

		public void TestDefaultOrigins()
		{
			AssertEquals(typeof(RefCountryCollection), parent.Lookups.DefaultOrigins.GetType());
		}

		public void TestCFIACountryOfSourceList()
		{
			AssertEquals(typeof(RefCountryCollection), parent.Lookups.CFIACountryOfSourceList.GetType());
		}

		public void TestCFIAStateOfSourceList()
		{
			AssertEquals(typeof(USStatesList), parent.Lookups.StatesOfExport.GetType());
			parent.Parent.CA_CFIACountryOfSource = Core.Constants.CountryCodes.Canada;
			AssertEquals(typeof(CanadianProvinceList), parent.Lookups.CFIAStateOfSourceList.GetType());
			parent.Parent.CA_CFIACountryOfSource = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(typeof(USStatesList), parent.Lookups.CFIAStateOfSourceList.GetType());
			parent.Parent.CA_CFIACountryOfSource = Core.Constants.CountryCodes.Australia;
			AssertEquals(typeof(CodeDescriptionPairList), parent.Lookups.CFIAStateOfSourceList.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			parent = (AddInfoJobComInvoiceLine)GetNewAddInfo();
		}

		AddInfoJobComInvoiceLine parent;
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
