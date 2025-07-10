using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

public static class WarehouseTestHelper
{
	public static RefCusProcedure CreateInwardCusProcedure(BusinessObjectFactory factory, string shipmentType = "IMP")
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "IM", procedureCode: "71", previousProcedureCode: "00", "F61", "", shipmentType, group: "71");
		procedure.ZZ6_IntoWarehouse = "Y";
		procedure.ZZ6_OutOfWarehouse = "N";
		factory.Save();
		return procedure;
	}

	public static RefCusProcedure CreateOutwardCusProcedure(BusinessObjectFactory factory, string shipmentType = "IMP")
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "IM", procedureCode: "40", previousProcedureCode: "71", "C33", "", shipmentType, group: "40");
		procedure.ZZ6_IntoWarehouse = "N";
		procedure.ZZ6_OutOfWarehouse = "Y";
		factory.Save();
		return procedure;
	}
}
