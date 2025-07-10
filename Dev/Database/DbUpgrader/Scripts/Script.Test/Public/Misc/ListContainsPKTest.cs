using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(ListContainsPK))]
	class ListContainsPKTest : DbCreateScriptTest
	{
		public void TestListContainsPK()
		{
			var pk = Guid.NewGuid();

			AssertEquals("When the pk is at the beginning of the list", true, ListContainsPK($"{pk},{Guid.NewGuid()},{Guid.NewGuid()}", pk));
			AssertEquals("When the pk is at the end of the list", true, ListContainsPK($"{Guid.NewGuid()},{Guid.NewGuid()},{pk}", pk));
			AssertEquals("When the pk is in the the list", true, ListContainsPK($"{Guid.NewGuid()},{pk},{Guid.NewGuid()}", pk));
			AssertEquals("When the pk is not in the the list", false, ListContainsPK($"{Guid.NewGuid()},{Guid.NewGuid()}", pk));
			AssertEquals("When the pk is null", false, ListContainsPK($"{Guid.NewGuid()},{Guid.NewGuid()}", null));
		}

		bool ListContainsPK(string list, Guid? pk)
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.ListContainsPK(@list, @pk)"))
			{
				command.AddParameter("@list", SqlDbType.NVarChar, list);
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, (object)pk ?? DBNull.Value);

				var data = DataUtils.GetDataTableFromCommand(command);
				return data.AsEnumerable().Select(row => (int)row["ContainsPK"]).FirstOrDefault() == 1;
			}
		}
	}
}

