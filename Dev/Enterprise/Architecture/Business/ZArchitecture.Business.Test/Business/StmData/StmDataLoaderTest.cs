using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmData.Loader))]
	sealed class StmDataLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var stmData1 = CreateStmData("stmData1", EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentDepartment.PK);
			var stmData2 = CreateStmData("stmData2", EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentDepartment.PK);
			var stmData3 = CreateStmData("stmData3", EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentDepartment.PK);
			var stmData4 = CreateStmData("stmData4", Guid.NewGuid(), EnvProxy.Instance.CurrentDepartment.PK);
			var stmData5 = CreateStmData("stmData5", EnvProxy.Instance.CurrentUser.PK, Guid.NewGuid());

			var names = new[] { "stmData1", "stmData2", "stmData4", "stmData5" };
			var results = new StmData.Loader(Factory).Load(names, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentDepartment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { stmData1, stmData2 }, results);
		}

		public void TestLoadTop1()
		{
			StmData stmDataCurrentUser = CreateStmData("A", EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentDepartment.PK);
			StmData stmDataCurrentUser2 = CreateStmData("A", EnvProxy.Instance.CurrentUser.PK, Guid.Empty);
			StmData stmDataDiffUser = CreateStmData("A", Guid.NewGuid(), EnvProxy.Instance.CurrentDepartment.PK);
			StmData stmDataDiffName = CreateStmData("B", EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentDepartment.PK);

			AssertEquals(stmDataCurrentUser, new StmData.Loader(Factory).LoadTop1("A", EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentDepartment.PK));
			AssertEquals(stmDataCurrentUser2, new StmData.Loader(Factory).LoadTop1("A", EnvProxy.Instance.CurrentUser.PK, Guid.Empty));
		}

		StmData CreateStmData(string name, Guid owner, Guid departmentGUID)
		{
			StmData result = Factory.New<StmData>();

			result.SD_Name = name;
			result.SD_Owner = owner;
			result.SD_DepartmentGuid = departmentGUID;
			return result;
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new StmData.Loader(Factory);
		}
	}
}
