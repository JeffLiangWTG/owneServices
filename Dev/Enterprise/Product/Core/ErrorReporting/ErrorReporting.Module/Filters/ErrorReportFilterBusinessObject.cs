using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ErrorReporting.Module.Filters;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ErrorReporting.Module
{
	public class ErrorReportFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var typeFilter = filters.AddTextFilter("Report Type", GetReportTypeQuery, new ErrorReportTypeList());
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("ErrorReporting|ErrorReportFilter|ReportType", "Report Type");
			typeFilter.Category = FilterCategories.ModesAndTypes;

			var statusFilter = filters.AddTextFilter("Transmit Status", StmErrorReportSchema.QER_TransmitStatus, new StmErrorReportTransmitStatus());
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("ErrorReporting|ErrorReportFilter|TransmitStatus", "Transmit Status");
			statusFilter.Category = FilterCategories.ModesAndTypes;

			return filters;
		}

		ZQuery GetReportTypeQuery(ZString reportType)
		{
			var query = new ZQuery();

			if (!reportType.IsEmpty)
			{
				query.AddToFilter(StmErrorReportSchema.QER_ReportType, SQLComparisonOperator.Equal, int.Parse(reportType));
			}

			return query;
		}
	}
}
