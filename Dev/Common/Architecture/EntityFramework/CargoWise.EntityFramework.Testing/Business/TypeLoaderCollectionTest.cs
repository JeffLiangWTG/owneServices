using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TypeLoaderCollectionTest : TestCaseWithFactory
	{
		public void TestLoadBusinessObject()
		{
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			TypeLoaderCollection loader = new TypeLoaderCollection(typeof(DummyBusinessObject));
			AssertEquals(bizo, loader.LoadBusinessObject(Factory, DummyBizoSchema.Constants.Prefix, bizo.PK));
			AssertEquals(null, loader.LoadBusinessObject(Factory, DummyBizoSchema.Constants.Prefix, ZGuid.NewZGuid()));
			AssertEquals(null, loader.LoadBusinessObject(Factory, "", ZGuid.NewZGuid()));
		}

		public void TestHasLoaderFor()
		{
			Type dummyType = typeof(DummyBusinessObject);
			TypeLoaderCollection loader = new TypeLoaderCollection();
			AssertEquals("HasLoaderFor", false, loader.HasLoaderFor(dummyType));
			loader.Add(new TypeLoader(dummyType));
			AssertEquals("HasLoaderFor", true, loader.HasLoaderFor(dummyType));
			AssertEquals("HasLoaderFor", true, loader.HasLoaderFor(DummyBizoSchema.Constants.Prefix));
			AssertEquals("HasLoaderFor", false, loader.HasLoaderFor(typeof(DummyDependantBusinessObject)));

			loader.Add(new Type[] { typeof(DummyPivot), typeof(DummyDependantBusinessObject) });
			AssertEquals("HasLoaderFor", true, loader.HasLoaderFor(typeof(DummyPivot)));
			AssertEquals("HasLoaderFor", true, loader.HasLoaderFor(DummyPivotSchema.Constants.Prefix));
			AssertEquals("HasLoaderFor", true, loader.HasLoaderFor(typeof(DummyDependantBusinessObject)));
			AssertEquals("HasLoaderFor", true, loader.HasLoaderFor(DummyDependentBizoSchema.Constants.Prefix));

			loader = new TypeLoaderCollection(typeof(DummyBusinessObject), typeof(DummyPivot));
			AssertEquals("HasLoaderFor", false, loader.HasLoaderFor(StmModuleFilterSchema.Constants.PK));
			AssertEquals("HasLoaderFor", true, loader.HasLoaderFor(DummyBizoSchema.Constants.Prefix));
			AssertEquals("HasLoaderFor", true, loader.HasLoaderFor(DummyPivotSchema.Constants.Prefix));
		}

		public void TestSetTablePrefixAndPK()
		{
			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			TypeLoaderCollection loader = new TypeLoaderCollection(typeof(DummyBusinessObject));
			loader.SetTablePrefixAndPK(null, bizo.Z0_VarCharMaxInfo, bizo.Z0_GuidInfo);
			AssertEquals("Table", ZString.Empty, bizo.Z0_VarCharMax);
			AssertEquals("ID", ZGuid.Empty, bizo.Z0_Guid);

			DummyBusinessObject bizo2 = Factory.New<DummyBusinessObject>();
			loader.SetTablePrefixAndPK(bizo2, bizo.Z0_VarCharMaxInfo, bizo.Z0_GuidInfo);
			AssertEquals("Table", DummyBizoSchema.Constants.Prefix, bizo.Z0_VarCharMax);
			AssertEquals("ID", bizo2.PK, bizo.Z0_Guid);
		}
	}
}
