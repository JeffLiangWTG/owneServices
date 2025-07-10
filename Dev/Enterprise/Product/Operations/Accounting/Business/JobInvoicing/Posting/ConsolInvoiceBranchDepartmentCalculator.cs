using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public static class ConsolInvoiceBranchDepartmentCalculator
	{
		public static ZGuid GetDepartment(IJobCostingPlugIn consol, BusinessObjectFactory factory)
		{
			return DepartmentChooser.New(factory).GetDepartment(JobInvoicingConsumerTypes.Shipment, GetOrigin(consol), GetDestination(consol), GetTransportMode(consol), GetContainerMode(consol));
		}

		public static GlbBranch GetBranch(IJobCostingPlugIn consol, ZGuid companyPK, BusinessObjectFactory factory)
		{
			return consol.FindBranchFromConsolAgentsAndJobHeaders(companyPK, factory);
		}

		static string GetOrigin(IJobCostingPlugIn consol)
		{
			return consol != null ? consol.CostSupporter.PortOfLoading : ZString.Empty;
		}

		static string GetDestination(IJobCostingPlugIn consol)
		{
			return consol != null ? consol.CostSupporter.PortOfDischarge : ZString.Empty;
		}

		static string GetTransportMode(IJobCostingPlugIn consol)
		{
			return consol != null ? consol.CostSupporter.TransportMode : ZString.Empty;
		}

		static string GetContainerMode(IJobCostingPlugIn consol)
		{
			return consol != null ? consol.CostSupporter.ConsolMode : ZString.Empty;
		}
	}
}
