using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AGS.Business
{
	internal class AGSStmALogValueObjectDataAdapter : StmALogValueObjectDataAdapter
	{
		public AGSStmALogValueObjectDataAdapter(BusinessObject logParent, string errorContext, EventsWithSourceType triggerEvent)
			: base(logParent, errorContext, triggerEvent)
		{
		}

		public new static AGSStmALogValueObjectDataAdapter New(BusinessObject logParent, string errorContext)
		{
			return new AGSStmALogValueObjectDataAdapter(logParent, errorContext, EventsWithSourceType.Empty);
		}

		public new static AGSStmALogValueObjectDataAdapter New(BusinessObject logParent, string errorContext, EventsWithSourceType triggerEvent)
		{
			return (logParent == null) ? null : new AGSStmALogValueObjectDataAdapter(logParent, errorContext, triggerEvent);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static AGSStmALogValueObjectDataAdapter OverriddenNewMethod(BusinessObject logParent, string errorContext, EventsWithSourceType triggerEvent)
		{
			return AGSStmALogValueObjectDataAdapter.New(logParent, errorContext, triggerEvent);
		}

		protected override bool ShouldExportDataImportEvent(StmALog log)
		{
			return log.SL_Table == JobShipmentSchema.Constants.TableName;
		}
	}
}
