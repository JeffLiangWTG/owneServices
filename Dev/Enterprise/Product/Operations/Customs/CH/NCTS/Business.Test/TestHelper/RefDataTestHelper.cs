using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using RefCusCodeList = Enterprise.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public sealed class RefDataTestHelper
{
	public RefDataTestHelper(BusinessObjectFactory factory)
	{
		Factory = factory;
		Helper = new UniversalReferenceTestDataHelper(factory);
	}
	BusinessObjectFactory Factory { get; }
	UniversalReferenceTestDataHelper Helper { get; }

	const string DefaultDataGrouping = Core.Constants.CountryCodes.Switzerland;

	public CodeListBuilder CreateCodeList(string codeType, string dataGrouping = DefaultDataGrouping)
	{
		return new CodeListBuilder(this, codeType, dataGrouping);
	}

	public abstract class BaseBuilder
	{
		protected BaseBuilder(RefDataTestHelper helper, string dataGrouping)
		{
			Helper = helper;
			DataGrouping = dataGrouping;
		}

		protected RefDataTestHelper Helper { get; }
		protected string DataGrouping { get; }
		protected UniversalReferenceTestDataHelper UniveralHelper => Helper.Helper;
	}

	public sealed class CodeListBuilder : BaseBuilder
	{
		internal CodeListBuilder(RefDataTestHelper helper, string codeType, string dataGrouping) : base(helper, dataGrouping)
		{
			CodeType = codeType;
			UniveralHelper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			UniveralHelper.CreateNewOrGetExistingCusCodeType(codeType, $"CodeType-{codeType}", dataGrouping);
		}
		string CodeType { get; }

		RefCusCodeList currentCodeList;

		public CodeListBuilder CreateCode(string code)
		{
			currentCodeList = UniveralHelper.CreateCusCodeList(DataGrouping, CodeType, code, $"Code-{code}", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			return this;
		}

		public CodeListBuilder WithDescription(string description)
		{
			currentCodeList.ZZD_Description = description;
			return this;
		}

		public CodeListBuilder WithAttribute(string name, string value)
		{
			UniveralHelper.CreateCusCodeListAttribute(currentCodeList.PK, name, value);
			return this;
		}

		public CodeListBuilder WithValidity(ZDateTime startDate, ZDateTime endDate)
		{
			currentCodeList.ZZD_StartDate = startDate;
			currentCodeList.ZZD_EndDate = endDate;
			return this;
		}
	}

	public TariffBuilder CreateTariffs(string tariffType, string dataGrouping = DefaultDataGrouping)
	{
		var tariffTypePK = Helper.CreateNewOrGetExistingTariffType(dataGrouping, tariffType).PK;
		Factory.Save();
		return new TariffBuilder(this, dataGrouping, tariffTypePK);
	}

	public sealed class TariffBuilder : BaseBuilder
	{
		internal TariffBuilder(RefDataTestHelper helper, string dataGrouping, ZGuid tariffTypePK) : base(helper, dataGrouping)
		{
			TariffTypePK = tariffTypePK;
		}
		ZGuid TariffTypePK { get; }

		public TariffBuilder CreateTariff(string tariffCode)
		{
			UniveralHelper.CreateTariff(DataGrouping, TariffTypePK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, $"Tariff-{tariffCode}");
			return this;
		}
	}

	public void CreateTariffsForTransit()
	{
		var wcoTariffTypePK = UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, Constants.TariffTypes.HarmonizedSystem).PK;
		Helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, wcoTariffTypePK, "710121", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "natural pearls unworked");
		Helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, wcoTariffTypePK, "710122", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "natural pearls worked");
		Helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, wcoTariffTypePK, "840110", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "nuclear reactors");

		var chTariffTypePK = UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Switzerland, Constants.TariffTypes.Export).PK;
		Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "121200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "straw");
		Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "12130091", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "straw unprepared");
		Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "12149011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "hay");
		Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "84061000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "steam turbines");
		Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "84061000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "steam turbines extra");
		Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Switzerland, chTariffTypePK, "04069099001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "cheese");

		Factory.Save();
	}

	public void CreateGlobalCodeN0296List()
	{
		const string codeType = CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N0296;

		Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		Helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "XXX", "Authorised economic operators AEO (during TP)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "A20", "Express consignments in the context of exit summary declarations", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();
	}

	public void CreateNctsBondTypeList()
	{
		const string codeType = CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.NCTSBondType;

		Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		Helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "0", "Guarantee waiver", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "1", "Comprehensive guarantee", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "2", "Individual guarantee", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "3", "Individual guarantee in cash", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();
	}

	public void CreateNctsDeclarationTypeList()
	{
		const string codeType = CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.NCTSDeclarationType;

		Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		Helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5, "Mixed consignments comprising both goods to be placed under external Union transit procedure", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure, "Goods not having the customs status of Union goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure, "Goods having the customs status of Union goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories, "Goods required to move under the internal Union transit procedure", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, "National Transit Switzerland", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();
	}

	public void CreateTypeCodeList()
	{
		const string CH = Core.Constants.CountryCodes.Switzerland;
		const string DE = Core.Constants.CountryCodes.Germany;
		const string HouseLevel = "House";
		const string DC44E = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		const string DC44N = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DocumentType;
		const string TD44N = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N;
		const string AR44N = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N;
		const string AI44N = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N;
		const string AI44E = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation;

		var refDataTestHelper = new RefDataTestHelper(Factory);
		var eun = Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland, "Switzerland", eun);
		Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);

		Factory.Save();

		refDataTestHelper.CreateCodeList(DC44E, CH).CreateCode("CHEH").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Header);
		refDataTestHelper.CreateCodeList(DC44E, CH).CreateCode("CHE1").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);
		refDataTestHelper.CreateCodeList(DC44E, CH).CreateCode("CHE2").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);
		refDataTestHelper.CreateCodeList(DC44E, DE).CreateCode("DEE1").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);

		refDataTestHelper.CreateCodeList(DC44N, CH).CreateCode("INTH").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Header);
		refDataTestHelper.CreateCodeList(DC44N, CH).CreateCode("INT1").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);
		refDataTestHelper.CreateCodeList(DC44N, CH).CreateCode("INT2").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);
		refDataTestHelper.CreateCodeList(DC44N, DE).CreateCode("DEN1").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item);

		refDataTestHelper.CreateCodeList(TD44N, CH).CreateCode("N235H").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, HouseLevel);
		refDataTestHelper.CreateCodeList(AR44N, CH).CreateCode("Y900").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, HouseLevel);
		refDataTestHelper.CreateCodeList(AI44N, CH).CreateCode("V1013").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, HouseLevel);
		refDataTestHelper.CreateCodeList(AI44E, CH).CreateCode("A1100").WithAttribute(RefCusCodeListAttributeTypes.Codes.Level, HouseLevel);

		Factory.Save();
	}
}
