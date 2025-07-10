using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderWorkflowInformationProvider : IWorkflowInformationProvider
	{
		public CusExitHeaderWorkflowInformationProvider(CusExitHeader exitHeader)
		{
			this.exitHeader = exitHeader;
		}
		readonly CusExitHeader exitHeader;

		public ZString Destination => ZString.Empty;
		public ZString Origin => ZString.Empty;
		public TrackingConstants.BusinessContext BusinessContext => TrackingConstants.BusinessContext.NoBusinessContext;
		public IEnumerable<ZGuid> Companies => new[] { exitHeader.Company.PK };
	}
}
