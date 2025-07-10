using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Testing
{
	public static class RefDataHelper
	{
		public static void AddCustomsUnit(BusinessObjectFactory factory, string unit)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, unit, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			factory.Save();
		}

		public static RefCusProcedure AddProcedure(BusinessObjectFactory factory, string shipmentType, string code, string previousCode, string concession, string intoWarehouse, string outOfWarehouse)
		{
			new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			var procedure = factory.New<RefCusProcedure>();
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
			procedure.ZZ6_ShipmentType = shipmentType;
			procedure.ZZ6_ProcedureCode = code;
			procedure.ZZ6_PreviousProcedureCode = previousCode;
			procedure.ZZ6_Concession = concession;
			procedure.ZZ6_IntoWarehouse = intoWarehouse;
			procedure.ZZ6_OutOfWarehouse = outOfWarehouse;
			procedure.ZZ6_Description = code + previousCode + concession;
			return procedure;
		}

		public static void SetUpRefUNLOCO(BusinessObjectFactory factory, ZString code, ZString stateCode, ZString stateDescription, ZString countryCode)
		{
			var state1 = CreateNewOrGetExistingRefCountryStates(factory, stateCode, stateDescription, countryCode);
			CreateNewOrGetExistingRefUNLOCO(factory, code).RL_RW = state1.PK;
		}

		public static RefUNLOCO CreateNewOrGetExistingRefUNLOCO(BusinessObjectFactory factory, ZString code)
		{
			var result = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = factory.New<RefUNLOCO>();
				result.RL_Code = code;
			}
			return result;
		}

		public static RefCountryStates CreateNewOrGetExistingRefCountryStates(BusinessObjectFactory factory, ZString code, ZString description, ZString countryCode)
		{
			var result = new RefCountryStates.Loader(factory).LoadRefCountryStatesFromCode(code, countryCode);
			if (result == null)
			{
				result = factory.NewWithValidTestData<RefCountryStates>();
				result.RW_Code = code;
				result.RW_Description = description;
				result.RW_RN_NKCountryCode = countryCode;
			}
			return result;
		}

		public static RefCusProcedure CreateInwardCusProcedure(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "10", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			factory.Save();
			return procedure;
		}

		public static RefCusProcedure CreateOutwardCusProcedure(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "42", "71", "C33", "", "IMP", "42P");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			factory.Save();
			return procedure;
		}
	}
}
