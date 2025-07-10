#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class JobCostingPlugInDataRetriever
	{
		public IJobCostingPlugIn HostPlugIn_ForTestOnly
		{
			get { return HostPlugIn; }
		}

		public BusinessObjectFactory Factory_ForTestOnly
		{
			get { return Factory; }
		}
	}
}

#endif
