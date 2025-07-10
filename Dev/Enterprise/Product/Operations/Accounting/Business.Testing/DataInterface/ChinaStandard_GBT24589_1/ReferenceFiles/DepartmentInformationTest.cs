using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(DepartmentInformation))]
	public class DepartmentInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T107", DepartmentInformation.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DepartmentInformation();
		}
	}

	[TestedType(typeof(DepartmentInformationCollection))]
	public class DepartmentInformationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DepartmentInformationCollection>
	{
		public void TestAddDefaultElements()
		{
			GlbDepartment tDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			tDepartment.GE_Code = "10";
			tDepartment.GE_Desc = "Test Dept.";
			GlbDepartment parent = Factory.NewWithValidTestData<GlbDepartment>();
			parent.GE_Code = "11";
			GlbDepartment child = Factory.NewWithValidTestData<GlbDepartment>();
			child.GE_Code = "12";
			child.GE_GE = parent.PK;
			Factory.Save();
			DepartmentInformationCollection collection = new DepartmentInformationCollection(Factory);
			AssertEquals("10", collection[0].DepartmentCode);
			AssertEquals("Test Dept.", collection[0].DepartmentName);
			AssertEquals("", collection[0].ParentDepartmentCode);
			AssertEquals("11", collection[1].DepartmentCode);
			AssertEquals("", collection[1].ParentDepartmentCode);
			AssertEquals("12", collection[2].DepartmentCode);
			AssertEquals("11", collection[2].ParentDepartmentCode);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DepartmentInformation();
		}

		protected override DepartmentInformationCollection GetCollectionToTest()
		{
			return new DepartmentInformationCollection(Factory);
		}
	}
}
