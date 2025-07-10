using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Testing
{
	class MessageResponseSemaphoreHelperTest : TestCaseWithFactory
	{
		public void TestCreateSemaphoreForDeclaration()
		{
			using var helper = new MessageResponseSemaphoreHelper();
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			helper.CreateSemaphoreForDeclaration(dec1);

			var results = new List<string>(GetAllMessageResponseSemaphoresFromDatabase());

			AssertEquals(1, results.Count);
			AssertContains(dec1.PK.ToString(), results[0]);
		}

		public void TestRemoveSemaphoreForDeclaration()
		{
			using var helper = new MessageResponseSemaphoreHelper();
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			helper.CreateSemaphoreForDeclaration(dec1);
			helper.CreateSemaphoreForDeclaration(dec2);

			MessageResponseSemaphoreHelper.RemoveSemaphoreForDeclaration(dec1);

			var results = new List<string>(GetAllMessageResponseSemaphoresFromDatabase());

			AssertEquals(1, results.Count);
			AssertContains(dec2.PK.ToString(), results[0]);
		}

		public void TestSemaphoreExistsForDeclaration()
		{
			using var helper = new MessageResponseSemaphoreHelper();
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			helper.CreateSemaphoreForDeclaration(dec1);

			CombineAssertions(() =>
			{
				AssertEquals("SemaphoreExistsForDeclaration dec1", expected: true, MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(dec1));
				AssertEquals("SemaphoreExistsForDeclaration dec2", expected: false, MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(dec2));
			});
		}

		IEnumerable<string> GetAllMessageResponseSemaphoresFromDatabase()
		{
			using var cmd = Db.Connection.Command("select SS_LockInfo from dbo.StmServiceSemaphore where SS_ServiceClass = @category");
			cmd.AddParameterBasedOnDbColumn("@category", "MSG", StmServiceSemaphoreSchema.SS_ServiceClass);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				yield return (string)reader[0];
			}
		}
	}
}
