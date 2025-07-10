using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DynamicBusinessObjectTest : TestCaseWithDummy
	{
		public void TestCollectionLoad()
		{
			SaveTestData();

			DynamicBusinessObjectCollection list = new DynamicBusinessObjectCollection(Factory);

			list.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num order by Z0_Description", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			AssertEquals("Count", 2, list.Count);

			BusinessObject bO = list[1];
			AssertEquals("DESC", bO["Z0_Description"].ToString().Trim());

			AssertEquals(12, bO["Z0_Number"]);
			AssertEquals(typeof(ZInt), bO["Z0_Number"].GetType());
		}

		void SaveTestData()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "DESC";
			dummy.Z0_Number = 12;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Number = 12;

			Factory.Save();
		}

		public void TestCustomTypeDescriptorInstanceGetProperties()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);

			collection.Load("select OH_PK, OH_Code from dbo.OrgHeader");
			AssertNotNull("Should have the property for OH_Code", collection[0].GetProperties()["OH_Code"]);

			collection.Load("select OH_PK, OH_FullName from dbo.OrgHeader");
			AssertNull("Should not have the non-existant property for OH_Code", collection[0].GetProperties()["OH_Code"]);
			AssertNotNull("Should have the property for OH_FullName", collection[0].GetProperties()["OH_FullName"]);
		}
	}
}
