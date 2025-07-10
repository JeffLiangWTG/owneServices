using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class ValidateAndSaveImportCollectionInfo : TestCaseWithFactory
	{
		public void TestImportCollectionInfo()
		{
			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_VarCharMax = "TestImportCollectionInfo1";
			Factory.Save();

			IImportCollectionInfo importInfo = new ValidateAndSaveImportCollectionInfo<DummyBusinessObject>()
			{
				DummyBizoSchema.Z0_VarCharMax.Name,
				DummyBizoSchema.Z0_Decimal.Name
			};

			AssertEquals(2, importInfo.Properties.Count());
			AssertEquals(typeof(ZString), importInfo.Properties.ElementAt(0).PropertyType);
			AssertEquals("Z0_VarCharMax", importInfo.Properties.ElementAt(0).MappingName);
			AssertEquals(typeof(ZDecimal), importInfo.Properties.ElementAt(1).PropertyType);
			AssertEquals("Z0_Decimal", importInfo.Properties.ElementAt(1).MappingName);

			AssertEquals(0, importInfo.Collection.Count);

			var bizObj2 = (DummyBusinessObject)importInfo.Collection.AddNew();
			bizObj2.Z0_VarCharMax = "TestImportCollectionInfo2";

			AssertEquals(1, importInfo.Collection.Count);

			importInfo.Collection.Factory.Save();

			AssertEquals(1, Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_VarCharMax, "TestImportCollectionInfo2")).Length);
		}
	}
}
