using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Business.IncidentManager.Business
{
	internal class IncidentTriageChecklistItemFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public IncidentTriageChecklistItemFetchStrategy(IncidentTriageChecklistItem triage)
			: base(triage)
		{
			this.triage = triage;
		}
		readonly IncidentTriageChecklistItem triage;

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
