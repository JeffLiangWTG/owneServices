using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NotificationUnloadingNCTS5UnloadingRemarksWrapper : IUnloadingRemarksNCTS
	{
		public NotificationUnloadingNCTS5UnloadingRemarksWrapper(NctsArrivalMovementHeader moveHeader)
		{
			arrivalMovement = Argument.NotNull(moveHeader, nameof(moveHeader));
		}
		protected readonly NctsArrivalMovementHeader arrivalMovement;

		public ZBool Conform => arrivalMovement.BM_NoChangesToReport;

		public ZDateTime UnloadingDate => arrivalMovement.BM_UnloadingDate.ToDateTime();

		public ZBool StateOfSeals => arrivalMovement.BM_StateOfSealsBoolean;
		public ZBool StateOfSealsSpecified => arrivalMovement.Header.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(c => !c.TotalSealCount.IsEmpty && !c.BC_UnloadedState.IsUnloadingStateNEW())
											|| arrivalMovement.Header.EnRouteIncidents.Any(i => i.IncidentContainers.Any(c => !c.TotalSealCount.IsEmpty));

		public ZString UnloadingRemark => arrivalMovement.BM_UnloadingRemarks;
	}
}
