using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(csfn_CusSCAOceanBillContainerModes))]
	class csfn_CusSCAOceanBillContainerModesTest : DbCreateScriptTest
	{
		public void TestEndToEnd()
		{
			TestConnection.ExecuteScalar(@"
insert into dbo.CusSCAOceanBill(CB_PK, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) values ('F6CFE777-67AE-4CC7-A318-EF5A9D31B83B', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
insert into dbo.CusSCAOceanBill(CB_PK, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) values ('73CA2F12-ED31-4946-9286-8D29855D8C00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
insert into dbo.CusSCAOceanBill(CB_PK, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) values ('B7DE1277-8997-415A-8A02-FD5C8D0288FD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
insert into dbo.CusSCAOceanBill(CB_PK, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) values ('2247A122-51F3-4489-B973-0D49A7149E3D', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

insert into dbo.CusSCAContainer(CN_PK, CN_CB, CN_ContainerMode, CN_SystemCreateTimeUtc, CN_SystemCreateUser, CN_SystemLastEditTimeUtc, CN_SystemLastEditUser) 
	values('D97C9EFC-79A5-4C7F-8524-C16876533D2C', 'F6CFE777-67AE-4CC7-A318-EF5A9D31B83B', 'FCL', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

insert into dbo.CusSCAContainer(CN_PK, CN_CB, CN_ContainerMode, CN_SystemCreateTimeUtc, CN_SystemCreateUser, CN_SystemLastEditTimeUtc, CN_SystemLastEditUser) 
	values('C9F88E2B-B400-4142-BB9D-BFDE74E8B56F', '73CA2F12-ED31-4946-9286-8D29855D8C00', 'FCX', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
insert into dbo.CusSCAContainer(CN_PK, CN_CB, CN_ContainerMode, CN_SystemCreateTimeUtc, CN_SystemCreateUser, CN_SystemLastEditTimeUtc, CN_SystemLastEditUser) 
	values('074A79B9-2A0C-4C52-A4C5-062FC4F47268', '73CA2F12-ED31-4946-9286-8D29855D8C00', 'FCX', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

insert into dbo.CusSCAContainer(CN_PK, CN_CB, CN_ContainerMode, CN_SystemCreateTimeUtc, CN_SystemCreateUser, CN_SystemLastEditTimeUtc, CN_SystemLastEditUser) 
	values('00A95DDD-9338-4403-BC84-5C82D8E9752C', 'B7DE1277-8997-415A-8A02-FD5C8D0288FD', 'FCL', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
insert into dbo.CusSCAContainer(CN_PK, CN_CB, CN_ContainerMode, CN_SystemCreateTimeUtc, CN_SystemCreateUser, CN_SystemLastEditTimeUtc, CN_SystemLastEditUser) 
	values('5299188A-894B-4B66-A99A-FC3156DE3EDC', 'B7DE1277-8997-415A-8A02-FD5C8D0288FD', 'FCX', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
"
);

			var modesDataTable = DataUtils.GetDataTableFromQuery(TestConnection, @"
select CB_PK, CB_ContainerMode
from csfn_CusSCAOceanBillContainerModes()");

			var modes = new Dictionary<Guid, string>();
			foreach (DataRow row in modesDataTable.Rows)
			{
				modes.Add((Guid)row["CB_PK"], (string)row["CB_ContainerMode"]);
			}

			AssertEquals(4, modes.Keys.Count);
			AssertEquals("FCL", modes[Guid.Parse("F6CFE777-67AE-4CC7-A318-EF5A9D31B83B")]);
			AssertEquals("FCX", modes[Guid.Parse("73CA2F12-ED31-4946-9286-8D29855D8C00")]);
			AssertEquals("MULTI", modes[Guid.Parse("B7DE1277-8997-415A-8A02-FD5C8D0288FD")]);
			AssertEquals("EMPTY", modes[Guid.Parse("2247A122-51F3-4489-B973-0D49A7149E3D")]);
		}
	}
}
