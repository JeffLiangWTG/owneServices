using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public class ReportsFilterLookups : CommonFilterLookups
	{
		public ReportsFilterLookups(ReportsFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public IBusinessObjectCollection Reporters
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public IBusinessObjectCollection Traders
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public ICodeDescriptionPairList Flows
		{
			get { return new ReportFlowCodeDescriptionPairList(); }
		}

		public ICodeDescriptionPairList Status
		{
			get { return new ReportStatusCodeDescriptionPairList(); }
		}
	}
}
