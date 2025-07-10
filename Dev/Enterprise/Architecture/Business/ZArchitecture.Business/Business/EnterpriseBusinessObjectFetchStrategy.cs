using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class EnterpriseBusinessObjectFetchStrategy : BusinessObjectFetchStrategy
	{
		public EnterpriseBusinessObjectFetchStrategy(EnterpriseBusinessObject businessObject)
			: base(businessObject)
		{
		}

		new EnterpriseBusinessObject BusinessObject
		{
			get { return (EnterpriseBusinessObject)base.BusinessObject; }
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			if (BusinessObject.IsInDatabase && BusinessObject.SupportsNotes)
			{
				BusinessObject.Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			}
		}

		protected override void FetchForFactorySaveBeforeTransactionCore()
		{
			base.FetchForFactorySaveBeforeTransactionCore();

			if (BusinessObject.IsInDatabase && BusinessObject is IWorkflowProviderCore && BusinessObject.HasChanges)
			{
				BusinessObject.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, BusinessObject.PK);
			}
		}
	}
}
