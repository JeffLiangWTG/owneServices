using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocDepartment))]
	public class DocDepartmentTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocDepartment.New(Department, Factory)
				};
		}

		public void TestMode()
		{
			AssertEquals("", DepartmentWrapper.Mode);
			Department.GE_Mode = "Air";
			AssertEquals("Air", DepartmentWrapper.Mode);
			Department.GE_Mode = "Sea";
			AssertEquals("Sea", DepartmentWrapper.Mode);
		}

		public void TestActivity()
		{
			AssertEquals("", DepartmentWrapper.Activity);
			Department.GE_Activity = "Customs";
			AssertEquals("Customs", DepartmentWrapper.Activity);
			Department.GE_Activity = "Depot CFS";
			AssertEquals("Depot CFS", DepartmentWrapper.Activity);
		}

		public void TestDirection()
		{
			AssertEquals("", DepartmentWrapper.Direction);
			Department.GE_Direction = "Export";
			AssertEquals("Export", DepartmentWrapper.Direction);
			Department.GE_Direction = "Import";
			AssertEquals("Import", DepartmentWrapper.Direction);
		}

		public void TestSystemDept()
		{
			AssertEquals("", DepartmentWrapper.SystemDept);
			Department.GE_SystemCode = true;
			AssertEquals("(System Department)", DepartmentWrapper.SystemDept);
		}

		public void TestAir()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Air);
			Department.GE_Air = ZBool.True;
			Assert(DepartmentWrapper.Air);
		}

		public void TestCode()
		{
			AssertEquals("", DepartmentWrapper.Code);
			Department.GE_Code = "CEA";
			AssertEquals("CEA", DepartmentWrapper.Code);
			Department.GE_Code = "CIA";
			AssertEquals("CIA", DepartmentWrapper.Code);
		}

		public void CustomsBrokerage()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.CustomsBrokerage);
			Department.GE_CustomsBrokerage = ZBool.True;
			Assert(DepartmentWrapper.CustomsBrokerage);
		}

		public void TestDepotCFS()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.DepotCFS);
			Department.GE_DepotCFS = ZBool.True;
			Assert(DepartmentWrapper.DepotCFS);
		}

		public void TestDesc()
		{
			AssertEquals("", DepartmentWrapper.Desc);
			Department.GE_Desc = "Test Department Desc";
			AssertEquals("Test Department Desc", DepartmentWrapper.Desc);
		}

		public void TestDocmestic()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Domestic);
			Department.GE_Domestic = ZBool.True;
			Assert(DepartmentWrapper.Domestic);
		}

		public void TestExport()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Export);
			Department.GE_Export = ZBool.True;
			Assert(DepartmentWrapper.Export);
		}

		public void TestFridayWorkingHours()
		{
			AssertEquals("", DepartmentWrapper.FridayWorkingHours);
			Department.WorkTimes.FridayWorkingHours = "         *****      ";
			AssertEquals("         *****", DepartmentWrapper.FridayWorkingHours);
		}

		public void TestDepartment()
		{
			AssertNull(DepartmentWrapper.Department);
			var departmentB = Factory.New<GlbDepartment>();
			departmentB.GE_Code = "DEA";
			Department.GE_GE = departmentB.PK;
			AssertNotNull(DepartmentWrapper.Department);
			AssertEquals("DEA", DepartmentWrapper.Department.Code);
		}

		public void TestImport()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Import);
			Department.GE_Import = ZBool.True;
			Assert(DepartmentWrapper.Import);
		}

		public void TestInternationalFreight()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.InternationalFreight);
			Department.GE_InternationalFreight = ZBool.True;
			Assert(DepartmentWrapper.InternationalFreight);
		}

		public void TestIsActive()
		{
			Assert(DepartmentWrapper.IsActive);
			Department.GE_IsActive = ZBool.False;
			AssertEquals(ZBool.False, DepartmentWrapper.IsActive);
		}

		public void TestLineHaul()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.LineHaul);
			Department.GE_LineHaul = ZBool.True;
			Assert(DepartmentWrapper.LineHaul);
		}

		public void TestLocalTransport()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.LocalTransport);
			Department.GE_LocalTransport = ZBool.True;
			Assert(DepartmentWrapper.LocalTransport);
		}

		public void TestMisc()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Misc);
			Department.GE_Misc = ZBool.True;
			Assert(DepartmentWrapper.Misc);
		}

		public void TestMondayWorkingHours()
		{
			AssertEquals("", DepartmentWrapper.MondayWorkingHours);
			Department.WorkTimes.MondayWorkingHours = "*****      ";
			AssertEquals("*****", DepartmentWrapper.MondayWorkingHours);
		}

		public void TestNonDirectional()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.NonDirectional);
			Department.GE_NonDirectional = ZBool.True;
			Assert(DepartmentWrapper.NonDirectional);
		}

		public void TestNonTransport()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.NonTransport);
			Department.GE_NonTransport = ZBool.True;
			Assert(DepartmentWrapper.NonTransport);
		}

		public void TestPost()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Post);
			Department.GE_Post = ZBool.True;
			Assert(DepartmentWrapper.Post);
		}

		public void TestRail()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Rail);
			Department.GE_Rail = ZBool.True;
			Assert(DepartmentWrapper.Rail);
		}

		public void TestRoad()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Road);
			Department.GE_Road = ZBool.True;
			Assert(DepartmentWrapper.Road);
		}

		public void TestSaturdayWorkingHours()
		{
			AssertEquals("", DepartmentWrapper.SaturdayWorkingHours);
			Department.WorkTimes.SaturdayWorkingHours = "*      ";
			AssertEquals("*", DepartmentWrapper.SaturdayWorkingHours);
		}

		public void TestSea()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Sea);
			Department.GE_Sea = ZBool.True;
			Assert(DepartmentWrapper.Sea);
		}

		public void TestSundayWorkingHours()
		{
			AssertEquals("", DepartmentWrapper.SundayWorkingHours);
			Department.WorkTimes.SundayWorkingHours = "*****";
			AssertEquals("*****", DepartmentWrapper.SundayWorkingHours);
		}

		public void TestSystemCode()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.SystemCode);
			Department.GE_SystemCode = ZBool.True;
			Assert(DepartmentWrapper.SystemCode);
		}

		public void TestThursdayWorkingHours()
		{
			AssertEquals("", DepartmentWrapper.ThursdayWorkingHours);
			Department.WorkTimes.ThursdayWorkingHours = "   *****   ";
			AssertEquals("   *****", DepartmentWrapper.ThursdayWorkingHours);
		}

		public void TestTuesdayWorkingHours()
		{
			AssertEquals("", DepartmentWrapper.TuesdayWorkingHours);
			Department.WorkTimes.TuesdayWorkingHours = "                *******************";
			AssertEquals("                *******************", DepartmentWrapper.TuesdayWorkingHours);
		}

		public void TestWarehouse()
		{
			AssertEquals(ZBool.False, DepartmentWrapper.Warehouse);
			Department.GE_Warehouse = ZBool.True;
			Assert(DepartmentWrapper.Warehouse);
		}

		public void TestWednesdayWorkingHours()
		{
			AssertEquals("", DepartmentWrapper.WednesdayWorkingHours);
			Department.WorkTimes.WednesdayWorkingHours = "                *******************             ";
			AssertEquals("                *******************", DepartmentWrapper.WednesdayWorkingHours);
		}

		public void TestDepartmentChargeCodes()
		{
			Department.GE_Code = "CEA";
			var dept1 = Department.DeptCharges.AddNew();
			var acc1 = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			acc1.AC_Desc = "Desc";
			dept1.GD_AC = acc1.PK;
			AssertEquals("DepartmentChargeCodes should contain 1 DocChargeCode", 1, DepartmentWrapper.DepartmentChargeCodes.Count);
			AssertEquals("Description of charge code", "Desc", DepartmentWrapper.DepartmentChargeCodes[0].Desc);
		}

		public void TestDepartmentPK()
		{
			AssertEquals(Department.PK, DepartmentWrapper.DepartmentPK);
		}

		#region Implementation

		protected override void SetUp()
		{
			Department = Factory.New<GlbDepartment>();
			DepartmentWrapper = DocDepartment.New(Department, Factory);
			AssertNotNull("Department wrapper not null", DepartmentWrapper);
			base.SetUp();
		}

		DocDepartment DepartmentWrapper;
		GlbDepartment Department;

		#endregion
	}
}
