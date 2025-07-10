using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void Test_CountryOfSupplyListCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "3rd Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "FR", "France", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QP", "High seas", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QQ", "Stores and provisions", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QR", "Stores and provisions within the framework of intra-Union trade", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QS", "Stores and provisions within the framework of extra-Union trade", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QX", "Countries and territories not specified for commercial or military reasons", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "ZZ", "Generic 3rd country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "EU Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "IT", "Italy", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "ES", "Spain", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "YY", "Generic EU country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "CO Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "MQ", "Martinique", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "RE", "Reunion", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "XX", "Generic DROM", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lookups = invoiceLine.AddInfoLookups;
			var list = (ZZRefCusCodeListCombinedCollection)lookups.CountryOfSupplyList;
			list.Load();

			AssertType<ZZRefCusCodeListCombinedCollection>("Export Collection Type", list);
			AssertContainsExactElementsInAnyOrder("Country of Supply list should be based on CO17 + EU17 + EX17 FR only code lists", new ZString[] { "FR", "QP", "QQ", "QR", "QS", "QX", "IT", "ES", "MQ", "RE" }, list.Select(v => v.ZZD_Code).OrderBy(v => v).ToArray());
		}

		public void Test_GetCountriesOfDestinationCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "3rd Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "FR", "France", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QP", "High seas", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QQ", "Stores and provisions", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QR", "Stores and provisions within the framework of intra-Union trade", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QS", "Stores and provisions within the framework of extra-Union trade", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QX", "Countries and territories not specified for commercial or military reasons", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "ZZ", "Generic 3rd country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "EU Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "IT", "Italy", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "ES", "Spain", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "YY", "Generic EU country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "CO Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "MQ", "Martinique", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "RE", "Reunion", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "XX", "Generic DROM", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lookups = invoiceLine.AddInfoLookups;
			var list = (ZZRefCusCodeListCombinedCollection)lookups.CountriesOfDestination;
			list.Load();

			AssertType<ZZRefCusCodeListCombinedCollection>("Export Collection Type", list);
			AssertContainsExactElementsInAnyOrder("Country of destination list should be based on CO17 + EU17 + EX17 FR only code lists", new ZString[] { "FR", "QP", "QQ", "QR", "QS", "QX", "IT", "ES", "MQ", "RE" }, list.Select(v => v.ZZD_Code).OrderBy(v => v).ToArray());
		}

		public void Test_GetCountriesOfDispatchCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "3rd Country of Dispatch");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "FR", "France", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QP", "High seas", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QQ", "Stores and provisions", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QR", "Stores and provisions within the framework of intra-Union trade", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QS", "Stores and provisions within the framework of extra-Union trade", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QX", "Countries and territories not specified for commercial or military reasons", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "ZZ", "Generic 3rd country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "EU Country of Dispatch");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "IT", "Italy", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "ES", "Spain", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "YY", "Generic EU country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "CO Country of Dispatch");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "MQ", "Martinique", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "RE", "Reunion", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "XX", "Generic DROM", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lookups = invoiceLine.AddInfoLookups;
			var list = (ZZRefCusCodeListCombinedCollection)lookups.CountriesOfDispatch;
			list.Load();

			AssertType<ZZRefCusCodeListCombinedCollection>("Export Collection Type", list);
			AssertContainsExactElementsInAnyOrder("Country of Dispatch list should be based on CO17 + EU17 + EX17 FR only code lists", new ZString[] { "FR", "QP", "QQ", "QR", "QS", "QX", "IT", "ES", "MQ", "RE" }, list.Select(v => v.ZZD_Code).OrderBy(v => v).ToArray());
		}
	}
}
