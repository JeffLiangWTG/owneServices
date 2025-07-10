using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusVehicle))]
	class CusVehicleTest : CusVehicleAbstractTest
	{
		protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);
		protected override Type ExpectedValidationType => typeof(CusVehicleValidation);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewObject(factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewObject(Factory);
		protected override BusinessObject GetNewBusinessObject() => GetNewObject(Factory);

		CusVehicle GetNewObject(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			var vehicle = Factory.New<CusVehicle>();
			vehicle.CVH_ParentID = declaration.PK;
			vehicle.CVH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			vehicle.CVH_DataModel = Enterprise.Core.Constants.CountryCodes.KoreaSouth;
			vehicle.CVH_ModelName = "ABC";
			vehicle.CVH_VehicleIdentificationNumber = "123";
			vehicle.CVH_ModelYear = "2002";

			return vehicle;
		}
	}
}
