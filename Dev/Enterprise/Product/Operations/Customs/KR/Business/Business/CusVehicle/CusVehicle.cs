using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CusVehicle : Customs.Business.CusVehicle, Integration.Customs.KR.ICusVehicle
	{
		public new class Schema : AutoCusVehicle.Schema
		{
			public new const int CVH_ModelNameMaxLength = 40;
		}

		public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("CFF85FFC-A92B-4226-852B-C373E73373CD", Caption = "Name")]
		public override ZString CVH_ModelName { get => base.CVH_ModelName; set => base.CVH_ModelName = value; }
		[ResourceStringData("788CDCD6-37FE-434E-82A7-C2A98B2E9A37", Caption = "VIN Reg. No.")]
		public override ZString CVH_VehicleIdentificationNumber { get => base.CVH_VehicleIdentificationNumber; set => base.CVH_VehicleIdentificationNumber = value; }
		[ResourceStringData("BED862CF-15FD-4931-B193-58A57E52AE70", Caption = "Exhaust Volume")]
		public override ZShort CVH_EngineCapacity { get => base.CVH_EngineCapacity; set => base.CVH_EngineCapacity = value; }
		[ResourceStringData("80C55D84-5793-49C3-9061-81E62497C29C", Caption = "Model Year")]
		public override ZString CVH_ModelYear { get => base.CVH_ModelYear; set => base.CVH_ModelYear = value; }
		[ResourceStringData("C78379EA-2017-4FD8-9C76-90D7C2A02AE8", Caption = "Manufacturing Country")]
		[MaxLength(Schema.CVH_RN_NKCountryOfManufactureMaxLength)]
		public override ZString CVH_RN_NKCountryOfManufacture { get => base.CVH_RN_NKCountryOfManufacture; set => base.CVH_RN_NKCountryOfManufacture = value; }

		[ResourceStringData("710A7920-685E-4110-92B3-74A04B76A9AD", Caption = "Seat Capacity")]
		public override ZByte CVH_Seats { get => base.CVH_Seats; set => base.CVH_Seats = value; }
		[ResourceStringData("88CE3BAF-8155-4FC7-98E4-35FFD5C6A30D", Caption = "First Registration Date")]
		public override ZDate CVH_DateOfFirstRegistration { get => base.CVH_DateOfFirstRegistration; set => base.CVH_DateOfFirstRegistration = value; }

		[ResourceStringData("A909DC5C-F89C-4150-97DA-73257706D079", Caption = "Current Registration Date")]
		public override ZDate CVH_DateOfCurrentRegistration { get => base.CVH_DateOfCurrentRegistration; set => base.CVH_DateOfCurrentRegistration = value; }
		protected override Customs.Business.CusVehicleLookups GetNewLookups() => new CusVehicleLookups(this);
		public new CusVehicleLookups Lookups => (CusVehicleLookups)base.Lookups;
		protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);
		public new CusVehicleValidation Validation => (CusVehicleValidation)base.Validation;
	}
}
