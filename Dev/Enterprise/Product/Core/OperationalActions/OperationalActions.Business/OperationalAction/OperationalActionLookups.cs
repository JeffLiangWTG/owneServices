using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionLookups : StmMenuItemLookups
	{
		public OperationalActionLookups(OperationalAction parent)
			: base(parent) { }

		public StmEventCollection Events
		{
			get
			{
				if (events == null)
				{
					events = new StmEventCollection(Factory);
					events.ApplySort(StmEventSchema.Constants.SE_Code, System.ComponentModel.ListSortDirection.Ascending);
				}
				return events;
			}
		}

		StmEventCollection events;
	}
}
