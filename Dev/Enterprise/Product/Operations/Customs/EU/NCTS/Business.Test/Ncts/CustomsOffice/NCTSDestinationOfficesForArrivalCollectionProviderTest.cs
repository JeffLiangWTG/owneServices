using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSDestinationOfficesForArrivalCollectionProvider))]
	class NCTSDestinationOfficesForArrivalCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(EUCustomsOfficeCodeCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		protected override int ExpectedMaxLength => 10;

		public void TestCollection()
		{
			SetupCustomsOffices();
			var collection = (CustomsOfficeCodeCollection)Provider.Collection;
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "DE003478", "IT008734", "GB001234", "TR001234", "XI001234" }, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		void SetupCustomsOffices()
		{
			const string IrelandXI = "XI";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey");
			helper.CreateNewOrGetExistingDataGrouping(IrelandXI, "Ireland (XI)");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, "Australia");

			var eutcp = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eutcp, Core.Constants.CountryCodes.Turkey);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE NCTSOfficeOfDeparture", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE003478", "GERMAN OFFICE NCTSOfficeOfDestination", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT008734", "ITALIAN OFFICE NCTSOfficeOfDestination", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT009278", "ITALIAN OFFICE NCTSOfficeOfDeparture", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB001234", "UK OFFICE NCTSOfficeOfDestination", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB009876", "UK OFFICE NCTSOfficeOfDeparture", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR001234", "TR OFFICE NCTSOfficeOfDestination", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR009876", "TR OFFICE NCTSOfficeOfDeparture", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

			helper.CreateCusCodeListWithAttribute(IrelandXI, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XI001234", "XI OFFICE NCTSOfficeOfDestination", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(IrelandXI, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XI009876", "XI OFFICE NCTSOfficeOfDeparture", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AU001234", "AU OFFICE NCTSOfficeOfDestination", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

			Factory.Save();
		}
	}
}
