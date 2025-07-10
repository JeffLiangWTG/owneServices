using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(EquipmentWrapper))]
	sealed class EquipmentWrapperTest : GenericWrapperTest
	{
		public void TestTwoWayInfo()
		{
			RefEquipment.RQ_2WayInfo = "2sidesofthecoin";
			AssertEquals("2sidesofthecoin", EquipmentWrapper.TwoWayInfo);
		}

		public void TestCubicUnit()
		{
			RefEquipment.RQ_CubicUnit = "M3";
			AssertEquals("M3", EquipmentWrapper.CubicUnit);
		}

		public void TestDescription()
		{
			RefEquipment.RQ_Description = "Somebody's Nobody";
			AssertEquals("Somebody's Nobody", EquipmentWrapper.Description);
		}

		public void TestEquipmentType()
		{
			var container = Factory.New<RefContainer>();
			container.RC_Code = "QQQ";
			RefEquipment.RQ_RC_RoadContainerType = container.PK;
			AssertEquals("QQQ", EquipmentWrapper.EquipmentType);
		}

		public void TestGateTransponder1()
		{
			RefEquipment.RQ_GateTransponder1 = "Transponster";
			AssertEquals("Transponster", EquipmentWrapper.GateTransponder1);
		}

		public void TestGateTransponder2()
		{
			RefEquipment.RQ_GateTransponder2 = "Transponster";
			AssertEquals("Transponster", EquipmentWrapper.GateTransponder2);
		}

		public void TestGateTransponder3()
		{
			RefEquipment.RQ_GateTransponder3 = "Transponster";
			AssertEquals("Transponster", EquipmentWrapper.GateTransponder3);
		}

		public void TestStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Staff";
			RefEquipment.RQ_GS_NKPreferredDriver = staff.GS_Code;
			AssertEquals("Staff", EquipmentWrapper.Staff.FullName);
		}

		public void TestRegistration()
		{
			AssertEquals("", EquipmentWrapper.Registration);
			RefEquipment.RQ_Registration = "REG 492";
			AssertEquals("REG 492", EquipmentWrapper.Registration);
		}

		public void TestShortCode()
		{
			RefEquipment.RQ_ShortCode = "NOTSHORT";
			AssertEquals("NOTSHORT", EquipmentWrapper.ShortCode);
		}

		public void TestTollPass()
		{
			RefEquipment.RQ_TollPass = "NOPASS";
			AssertEquals("NOPASS", EquipmentWrapper.TollPass);
		}

		public void TestWeightUnit()
		{
			RefEquipment.RQ_WeightUnit = "KG";
			AssertEquals("KG", EquipmentWrapper.WeightUnit);
		}

		public void TestTotalCubic()
		{
			RefEquipment.RQ_CubicCapacity = 1000M;
			AssertEquals(1000M, EquipmentWrapper.TotalCubic);
		}

		public void TestTotalWeight()
		{
			RefEquipment.RQ_WeightCapacity = 2000M;
			AssertEquals(2000M, EquipmentWrapper.TotalWeight);
		}

		RefEquipment RefEquipment;
		EquipmentWrapper EquipmentWrapper;

		protected override void SetUp()
		{
			RefEquipment = Factory.New<RefEquipment>();
			EquipmentWrapper = new EquipmentWrapper(RefEquipment, Factory);
			base.SetUp();
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new EquipmentWrapper(RefEquipment, Factory);
			AssertEquals(ZString.Empty, wrapperEmpty.TwoWayInfo);
			AssertEquals(ZString.Empty, wrapperEmpty.CubicUnit);
			AssertEquals(ZString.Empty, wrapperEmpty.Description);
			AssertEquals(ZString.Empty, wrapperEmpty.EquipmentType);
			AssertEquals(ZString.Empty, wrapperEmpty.GateTransponder1);
			AssertEquals(ZString.Empty, wrapperEmpty.GateTransponder2);
			AssertEquals(ZString.Empty, wrapperEmpty.GateTransponder3);
			AssertEquals(ZString.Empty, wrapperEmpty.EquipmentType);
			AssertEquals(null, wrapperEmpty.Staff);
			AssertEquals(ZBool.True, wrapperEmpty.IsActive);
			AssertEquals(ZBool.True, wrapperEmpty.IsVehicle);
			AssertEquals(ZString.Empty, wrapperEmpty.Registration);
			AssertEquals(ZString.Empty, wrapperEmpty.ShortCode);
			AssertEquals(ZString.Empty, wrapperEmpty.TollPass);
			AssertEquals(ZString.Empty, wrapperEmpty.WeightUnit);
			AssertEquals(0m, wrapperEmpty.TotalCubic);
			AssertEquals(0m, wrapperEmpty.TotalWeight);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)
Staff :  is null";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var equipment = Factory.New<RefEquipment>();
			return new EquipmentWrapper(equipment, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Equipment
======================================================================
Name                                    Type
----------------------------------------------------------------------
Staff                                   StaffMember
CubicUnit                               String
Description                             String
EquipmentType                           String
GateTransponder1                        String
GateTransponder2                        String
GateTransponder3                        String
IsActive                                Bool
IsVehicle                               Bool
Registration                            String
ShortCode                               String
TollPass                                String
TotalCubic                              Decimal
TotalWeight                             Decimal
TwoWayInfo                              String
WeightUnit                              String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var equipment = Factory.New<RefEquipment>();
			return new EquipmentWrapper(equipment, Factory);
		}
	}
}
