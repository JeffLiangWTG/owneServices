using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	internal class IncidentTriageFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public IncidentTriageFetchStrategy(IncidentTriage triage)
			: base(triage)
		{
			this.triage = triage;
		}
		readonly IncidentTriage triage;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				if (column.ColumnName.StartsWith("PublishedDescription", StringComparison.Ordinal))
				{
					Factory.AddFetchHint(StmNoteSchema.ST_ParentID, triage.PK);
				}
			}
		}
	}
}
