using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	abstract class TelEdgeTestCase : DbCreateScriptTest
	{
		protected virtual void GenerateData()
		{
			using (var command = Db.Connection.Command(@"
INSERT dbo.TelEdge (TE_PK, TE_EntityIdFrom, TE_EntityTableCodeFrom, TE_EntityIdTo, TE_EntityTableCodeTo, TE_TemplateMapping, TE_StartTime, TE_EndTime, TE_SystemCreateTimeUtc, TE_SystemCreateUser, TE_SystemLastEditTimeUtc, TE_SystemLastEditUser) VALUES
	(NEWID(), '00000000-1000-1000-0000-000000000000', 'RQ', '00000000-0000-0000-1111-000000000000', 'TSE', 0, '2017-01-01' , NULL      , GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

	(NEWID(), '00000000-0000-0000-1111-000000000000', 'TSE', '00000000-0000-A000-0000-000000000000', 'TSE', 0, '2017-01-01' , NULL     , GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(NEWID(), '00000000-0000-0000-1111-000000000000', 'TSE', '00000000-0000-B000-0000-000000000000', 'TSE', 0, '2017-01-01' , NULL     , GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	(NEWID(), '00000000-0000-B000-0000-000000000000', 'TSE', '00000000-0000-C000-0000-000000000000', 'TSE', 0, '2017-01-01' , NULL     , GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

	(NEWID(), '00000000-2000-1000-0000-000000000000', 'RQ', '00000000-0000-0000-1111-000000000000', 'TSE', 0, '2017-01-01' , NULL      , GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

	(NEWID(), '00000000-3000-1000-0000-000000000000', 'RQ', '00000000-0000-0000-1111-000000000000', 'TSE', 0, '2017-01-01' , NULL      , GetUtcDate(), '~BP', GetUtcDate(), '~BP')
"))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}
