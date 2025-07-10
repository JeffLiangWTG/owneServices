using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusVehicle
		{
			ZGuid PK { get; }
			ZString CVH_BrandName { get; set; }
			ZString CVH_CarType { get; set; }
			ZString CVH_CatalyticConverterType { get; set; }
			ZInt CVH_ClusterKey { get; set; }
			ZString CVH_Color { get; set; }
			ZString CVH_DataModel { get; set; }
			ZDate CVH_DateOfCurrentRegistration { get; set; }
			ZDate CVH_DateOfFirstRegistration { get; set; }
			ZByte CVH_Doors { get; set; }
			ZString CVH_DriveSide { get; set; }
			ZShort CVH_EngineCapacity { get; set; }
			ZString CVH_EngineCapacityUQ { get; set; }
			ZByte CVH_Gears { get; set; }
			ZString CVH_IMEINo { get; set; }
			ZBool CVH_IsDamaged { get; set; }
			ZBool CVH_IsUsed { get; set; }
			ZDate CVH_ManufacturedDate { get; set; }
			ZInt CVH_Mileage { get; set; }
			ZString CVH_MileageUQ { get; set; }
			ZString CVH_ModelName { get; set; }
			ZString CVH_ModelYear { get; set; }
			ZGuid CVH_ParentID { get; set; }
			ZString CVH_ParentTableCode { get; set; }
			ZDecimal CVH_Payload { get; set; }
			ZString CVH_PayloadUQ { get; set; }
			ZString CVH_RegistrationNumber { get; set; }
			ZString CVH_RN_NKCountryOfManufacture { get; set; }
			ZByte CVH_Seats { get; set; }
			ZString CVH_SerialNumber { get; set; }
			ZString CVH_SpecificationStandard { get; set; }
			ZString CVH_SupplyMethod { get; set; }
			ZDateTime CVH_SystemCreateTimeUtc { get; set; }
			ZString CVH_SystemCreateUser { get; set; }
			ZDateTime CVH_SystemLastEditTimeUtc { get; set; }
			ZString CVH_SystemLastEditUser { get; set; }
			ZString CVH_Transmission { get; set; }
			ZString CVH_VehicleIdentificationNumber { get; set; }
		}
	}
}
