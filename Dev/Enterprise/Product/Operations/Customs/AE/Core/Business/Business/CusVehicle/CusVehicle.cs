using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class CusVehicle : Customs.Business.CusVehicle, Integration.Customs.AE.ICusVehicle
{
	public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public JobComInvoiceLine InvoiceLine => base.Parent as JobComInvoiceLine;

	public JobDeclaration Declaration => (JobDeclaration)InvoiceLine?.Declaration;

	protected override Customs.Business.CusVehicleLookups GetNewLookups() => new CusVehicleLookups(this);

	public new CusVehicleLookups Lookups => (CusVehicleLookups)base.Lookups;

	protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);

	#region Engine

	public override EngineRelationshipType EngineRelationship => EngineRelationshipType.One;

	protected override ICusEngineCollection<Customs.Business.CusEngine, Customs.Business.CusVehicle> GetNewCusEngineCollection() => new CusEngineCollection<CusEngine, CusVehicle>(this);

	public CusEngine Engine => (CusEngine)base.FirstEngine;

	#endregion

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_VehicleIdentificationNumber", Caption = "Chassis Number", ShortCaption = "Chassis No.")]
	public override ZString CVH_VehicleIdentificationNumber { get => base.CVH_VehicleIdentificationNumber; set => base.CVH_VehicleIdentificationNumber = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_BrandName", Caption = "Brand")]
	[List(nameof(Lookups) + "." + nameof(CusVehicleLookups.VehicleBrandList))]
	public override ZString CVH_BrandName { get => base.CVH_BrandName; set => base.CVH_BrandName = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_ModelName", Caption = "Model")]
	public override ZString CVH_ModelName { get => base.CVH_ModelName; set => base.CVH_ModelName = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_Seats", Caption = "Passenger Capacity", ShortCaption = "Passenger Cap.")]
	public override ZByte CVH_Seats { get => base.CVH_Seats; set => base.CVH_Seats = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_Payload", Caption = "Carriage Capacity", ShortCaption = "Carriage Cap.")]
	public override ZDecimal CVH_Payload { get => base.CVH_Payload; set => base.CVH_Payload = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_PayloadUQ", Caption = "Unit")]
	[List(nameof(Lookups) + "." + nameof(CusVehicleLookups.PayloadUQList))]
	public override ZString CVH_PayloadUQ { get => base.CVH_PayloadUQ; set => base.CVH_PayloadUQ = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_ModelYear", Caption = "Year Built", MediumCaption = "Year", ShortCaption = "Yr.")]
	public override ZString CVH_ModelYear { get => base.CVH_ModelYear; set => base.CVH_ModelYear = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_Color", Caption = "Color")]
	public override ZString CVH_Color { get => base.CVH_Color; set => base.CVH_Color = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_IsUsed", Caption = "Is Used")]
	public override ZBool CVH_IsUsed { get => base.CVH_IsUsed; set => base.CVH_IsUsed = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_CarType", Caption = "Type")]
	[List(nameof(Lookups) + "." + nameof(CusVehicleLookups.CarTypeList))]
	public override ZString CVH_CarType { get => base.CVH_CarType; set => base.CVH_CarType = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_DriveSide", Caption = "Drive")]
	[List(nameof(Lookups) + "." + nameof(CusVehicleLookups.DriveSideList))]
	public override ZString CVH_DriveSide { get => base.CVH_DriveSide; set => base.CVH_DriveSide = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusVehicle|CVH_SpecificationStandard", Caption = "Specification Standard", MediumCaption = "Spec. Standard", ShortCaption = "Spec. Std.")]
	[List(nameof(Lookups) + "." + nameof(CusVehicleLookups.SpecificationStandardList))]
	public override ZString CVH_SpecificationStandard { get => base.CVH_SpecificationStandard; set => base.CVH_SpecificationStandard = value; }
}

