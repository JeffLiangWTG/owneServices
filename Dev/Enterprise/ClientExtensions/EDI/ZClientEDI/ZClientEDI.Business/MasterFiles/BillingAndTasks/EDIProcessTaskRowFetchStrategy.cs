using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[Immutable]
	public class EDIProcessTaskRowFetchStrategy : ProcessTaskRowFetchStrategy
	{
		protected override void FetchForLoadCore(BusinessObjectFactory factory, DataRow[] rows)
		{
			base.FetchForLoadCore(factory, rows);

			foreach (var row in rows)
			{
				if (IncidentMainSchema.Constants.Prefix == (string)row[ProcessTasksSchema.P9_ParentTableCode.Name])
				{
					var parentId = new ZGuid(row[ProcessTasksSchema.P9_ParentID.Name]);

					var query = new ZQuery(IncidentMainSchema.PK, parentId);
					factory.AddFetchHint(IncidentMainSchema.Instance, query);
				}
			}
		}
	}
}
