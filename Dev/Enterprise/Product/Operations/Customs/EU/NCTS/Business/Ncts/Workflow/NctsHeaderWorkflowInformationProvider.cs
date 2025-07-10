using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderWorkflowInformationProvider : IWorkflowInformationProvider
	{
		public NctsHeaderWorkflowInformationProvider(NctsHeader header)
		{
			this.header = header;
		}
		readonly NctsHeader header;

		IEnumerable<ZGuid> IWorkflowInformationProvider.Companies => new ZGuid[] { header.Company.PK };

		ZString IWorkflowInformationProvider.Destination => header.PlaceOfUnloadingCode;

		ZString IWorkflowInformationProvider.Origin => header.MovementHeader.BM_RL_NKForeignDestPort;

		MasterFiles.Tracking.TrackingConstants.BusinessContext IWorkflowInformationProvider.BusinessContext => MasterFiles.Tracking.TrackingConstants.BusinessContext.NoBusinessContext;
	}
}
