using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALAESExportOperationWrapper : IEALAESExportOperation
	{
		public EALAESExportOperationWrapper(CusExitReport exitReport)
		{
			this.exitReport = Argument.NotNull(exitReport, nameof(exitReport));
			exitConsignment = Argument.NotNull(exitReport.Consignment, nameof(exitReport.Consignment));
		}
		readonly CusExitReport exitReport;
		readonly CusExitConsignment exitConsignment;

		public ZBool StoringFlag => RequireStoring(exitReport.CER_TransportMode);

		public ZBool DiscrepanciesExist => exitReport.CER_Calc_Discrepancies;

		public ZString MRN => exitConsignment.CXC_MovementReference;

		ZBool RequireStoring(ZString transportMode) => !notRequireStoringList.Contains(transportMode);

		readonly ZString[] notRequireStoringList = new ZString[] { TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail };
	}
}
