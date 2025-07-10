using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusVehicle))]
sealed class CusVehicleTest : Customs.Business.Testing.CusVehicleAbstractTest
{
	public void TestInvoiceLine()
	{
		var parent = (JobComInvoiceLine)Factory.Load(vehicle.CVH_ParentTableCode, vehicle.CVH_ParentID);
		AssertEquals(parent, vehicle.InvoiceLine);
	}

	public void TestDeclaration()
	{
		var parent = (JobComInvoiceLine)Factory.Load(vehicle.CVH_ParentTableCode, vehicle.CVH_ParentID);
		AssertEquals(parent.Declaration, vehicle.Declaration);
	}

	public void TestTypeDecider()
	{
		AssertEquals(typeof(CusVehicle), Factory.New(typeof(Customs.Business.CusVehicle)).GetType());
	}

	public void TestEnginesType()
	{
		AssertType(typeof(CusEngineCollection<CusEngine, CusVehicle>), vehicle.Engines);
	}

	public void TestEngine()
	{
		AssertSame(vehicle.FirstEngine, vehicle.Engine);
	}

	public void TestCVH_VehicleIdentificationNumber() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_VehicleIdentificationNumberInfo);
		AssertEquals("Chassis Number", info.Caption);
		AssertEquals("Chassis No.", info.ShortCaption);
	});

	public void TestCVH_BrandName()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_BrandNameInfo);
		AssertEquals("Brand", info.Caption);
	}

	public void TestCVH_ModelName()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_ModelNameInfo);
		AssertEquals("Model", info.Caption);
	}

	public void TestCVH_Seats() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_SeatsInfo);
		AssertEquals("Passenger Capacity", info.Caption);
		AssertEquals("Passenger Cap.", info.ShortCaption);
	});

	public void TestCVH_Payload() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_PayloadInfo);
		AssertEquals("Carriage Capacity", info.Caption);
		AssertEquals("Carriage Cap.", info.ShortCaption);
	});

	public void TestCVH_PayloadUQ()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_PayloadUQInfo);
		AssertEquals("Unit", info.Caption);
	}

	public void TestCVH_Color()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_ColorInfo);
		AssertEquals("Color", info.Caption);
	}

	public void TestCVH_ModelYear() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_ModelYearInfo);
		AssertEquals("Year Built", info.Caption);
		AssertEquals("Year", info.MediumCaption);
		AssertEquals("Yr.", info.ShortCaption);
	});

	public void TestCVH_IsUsed()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_IsUsedInfo);
		AssertEquals("Is Used", info.Caption);
	}

	public void TestCVH_CarType()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_CarTypeInfo);
		AssertEquals("Type", info.Caption);
	}

	public void TestCVH_DriveSide()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_DriveSideInfo);
		AssertEquals("Drive", info.Caption);
	}

	public void TestCVH_SpecificationStandard() => CombineAssertions(() =>
	{
		var info = DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_SpecificationStandardInfo);
		AssertEquals("Specification Standard", info.Caption);
		AssertEquals("Spec. Standard", info.MediumCaption);
		AssertEquals("Spec. Std.", info.ShortCaption);
	});

	protected override EngineRelationshipType ExpectedEngineRelationship => EngineRelationshipType.One;

	protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);

	protected override Type ExpectedValidationType => typeof(CusVehicleValidation);

	protected override BusinessObject GetVehicleParent(BusinessObjectFactory factory) => (JobComInvoiceLine)factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();

	protected override void SetUp()
	{
		base.SetUp();
		vehicle = (CusVehicle)GetNewBusinessObject();
	}

	CusVehicle vehicle;
}

