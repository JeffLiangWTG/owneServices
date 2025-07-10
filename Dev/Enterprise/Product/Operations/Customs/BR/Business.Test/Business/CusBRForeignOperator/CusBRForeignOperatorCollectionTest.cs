using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusBRForeignOperatorCollection))]
	class CusBRForeignOperatorCollectionTest : ActiveBusinessObjectCollectionTestCase<CusBRForeignOperatorCollection>
	{
		protected override CusBRForeignOperatorCollection GetCollectionToTest() => new CusBRForeignOperatorCollection(Factory);

		public void TestConstructorWithOwner()
		{
			var owner1 = Factory.New<OrgHeader>();
			var owner2 = Factory.New<OrgHeader>();
			var foreignOperator = Factory.New<OrgHeader>();
			var foreignOperator2 = Factory.New<OrgHeader>();
			var cusBRForeignOperator = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_OH_Owner = owner1.PK;
			cusBRForeignOperator.BFR_OH_ForeignOperator = foreignOperator.PK;
			var cusBRForeignOperator2 = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator2.BFR_OH_Owner = owner1.PK;
			cusBRForeignOperator2.BFR_OH_ForeignOperator = foreignOperator2.PK;
			var cusBRForeignOperator3 = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator3.BFR_OH_Owner = owner2.PK;
			cusBRForeignOperator3.BFR_OH_ForeignOperator = foreignOperator.PK;
			var cusBRForeignOperator4 = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator4.BFR_OH_Owner = owner2.PK;

			AssertEquals(2, new CusBRForeignOperatorCollection(Factory, owner1.PK).Count);
			AssertEquals(1, new CusBRForeignOperatorCollection(Factory, owner2.PK).Count);
		}

		public void TestListProvider()
		{
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			owner1.OH_Code = "owner1";

			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			owner2.OH_Code = "owner2";

			var foreignOperator = Factory.NewWithValidTestData<OrgHeader>();
			foreignOperator.OH_Code = "fore123";

			var foreignOperator2 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOperator2.OH_Code = "fore22";

			var cusBRForeignOperator = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_OH_Owner = owner1.PK;
			cusBRForeignOperator.BFR_OH_ForeignOperator = foreignOperator.PK;

			var cusBRForeignOperator2 = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator2.BFR_OH_Owner = owner1.PK;
			cusBRForeignOperator2.BFR_OH_ForeignOperator = foreignOperator2.PK;

			var cusBRForeignOperator3 = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator3.BFR_OH_Owner = owner2.PK;

			Factory.Save();

			var collection = new CusBRForeignOperatorCollection(Factory) as IFindBoxListProvider;
			AssertEquals("Collection should find with code 'fore123'", cusBRForeignOperator.PK, collection.PrimaryKeyFromCode("fore123"));
			AssertEquals("Collection should find with code 'fore22'", cusBRForeignOperator2.PK, collection.PrimaryKeyFromCode("fore22"));
		}
	}
}
