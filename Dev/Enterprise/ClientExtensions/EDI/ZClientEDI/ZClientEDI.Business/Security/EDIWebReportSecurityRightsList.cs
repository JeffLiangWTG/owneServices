using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI
{
	public class EDIWebReportSecurityRightsList : ReportsWebSecurityRights
	{
		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = (factory) => { return new EDIWebReportSecurityRightsList(factory); };
		}

		protected EDIWebReportSecurityRightsList(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override StmMenuItemCollection GetWebReports(BusinessObjectFactory factory)
		{
			var reports = new StmMenuItemCollection(factory);
			var query = new ZQuery(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.WebReports);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "RepHRReports");
			reports.Load(query);
			return reports;
		}
	}
}

