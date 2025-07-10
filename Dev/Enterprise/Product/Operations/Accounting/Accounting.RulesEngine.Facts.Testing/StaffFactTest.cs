using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class StaffFactTest : TestCase
	{
		public void TestNullStaff_ThrowsException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new StaffFact(null, null));
		}

		public void TestPK()
		{
			var pk = Guid.NewGuid();
			var staff = Factory.NewWithPrimaryKey<GlbStaff>(pk);

			var staffFact = new StaffFact(staff, null);
			AssertEquals(pk, staffFact.PK);
		}

		public void TestCode()
		{
			var staffCode = "AB";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = staffCode;

			var staffFact = new StaffFact(staff, null);
			AssertEquals(staff.GS_Code, staffFact.Code);
			AssertEquals(staffCode, staffFact.Code);
		}

		public void TestHomeDepartment()
		{
			var staff = Factory.New<GlbStaff>();
			Assert("Precondition", staff.HomeDepartment == null);

			var departmentFactMock = new Mock<IDepartmentFact>();

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			staff.GS_GE_HomeDepartment = department.PK;

			Assert("Precondition", staff.HomeDepartment != null);

			var staffFact = new StaffFact(staff, departmentFactMock.Object);
			AssertNotNull(staffFact.HomeDepartment);
			AssertNotNull(staffFact.HomeDepartment.Fact);
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
