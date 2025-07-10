using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Business
{
	sealed class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			if (columns.Any(x => x.ColumnName == nameof(JobComInvoiceLine.Observations)))
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			}

			if (columns.Any(x => x.ColumnName == nameof(JobComInvoiceLine.VehicleMileage) || x.ColumnName == nameof(JobComInvoiceLine.VehicleMileageUQ) || x.ColumnName == nameof(JobComInvoiceLine.VehicleVIN)))
			{
				Factory.AddFetchHint(CusVehicleSchema.CVH_ParentID, BusinessObject.PK);
			}
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusVehicleSchema.CVH_ParentID, BusinessObject.PK);
		}
	}
}
