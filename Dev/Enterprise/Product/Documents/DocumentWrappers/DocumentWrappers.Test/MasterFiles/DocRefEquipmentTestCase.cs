using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocRefEquipment))]
	public class DocRefEquipmentTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocRefEquipment.New(RefEquipment, Factory), DocRefEquipment.New(Factory, RefEquipment.PK) };
		}

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
			RefContainer container = Factory.New<RefContainer>();
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
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
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
		DocRefEquipment EquipmentWrapper;
		protected override void SetUp()
		{
			RefEquipment = Factory.New<RefEquipment>();
			EquipmentWrapper = DocRefEquipment.New(RefEquipment, Factory);
			base.SetUp();
		}
	}
}
