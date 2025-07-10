using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	sealed class StmDataIGridLayoutStorageImpTest : TestCaseWithFactory
	{
		public void TestImplementation()
		{
			var data = Factory.New<StmData>();
			data.SD_Name = "Blah";
			data.SD_BinaryValue = CargoWise.Types.ZBlob.FromAscii("TEST");

			var imp = StmDataGridLayoutStorage.New(data);
			AssertEquals("Blah", imp.GridLayoutKey);
			AssertEquals(StmDataGridLayoutStorage.DefaultLayoutName, imp.ColumnLayoutName);
			AssertEquals(CargoWise.Types.ZBlob.FromAscii("TEST"), imp.ColumnLayoutData);

			AssertEquals(false, imp.IsDeleteAllowed);
			AssertEquals(false, imp.IsPublished);
			AssertEquals(false, imp.IsRenameAllowed);
			AssertEquals(false, imp.IsSystemDefined);
			AssertEquals(false, imp.SaveColumnLayout);

			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ imp.SaveColumnLayout = false; });
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ imp.ColumnLayoutName = ""; });
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ ((IGridLayoutStorage)imp).Delete(); });
		}

		public void TestPK_ShouldComeFromStmData()
		{
			var data = Factory.New<StmData>();
			var layoutStorage = StmDataGridLayoutStorage.New(data);
			AssertEquals(data.PK, layoutStorage.PK);
		}
	}
}
