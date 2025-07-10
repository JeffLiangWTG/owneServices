#if DEBUG

using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class DepartmentChooser
	{
		public ZGuid GetDepartmentCore_ForTestOnly(JobInvoicingConsumerType consumerType, ZString origin, ZString destination, ZString transportMode, ZString containerMode, bool isImport)
			=> GetDepartmentCore(consumerType, origin, destination, transportMode, containerMode, isImport);
	}
}

#endif
