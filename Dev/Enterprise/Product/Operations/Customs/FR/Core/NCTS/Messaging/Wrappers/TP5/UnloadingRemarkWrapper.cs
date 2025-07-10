using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class UnloadingRemarkWrapper : IUnloadingRemark
	{
		UnloadingRemarkWrapper(NctsArrivalMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}

		readonly NctsArrivalMovementHeader movementHeader;

		public static UnloadingRemarkWrapper New(NctsArrivalMovementHeader movementHeader) => movementHeader == null ? null : new UnloadingRemarkWrapper(movementHeader);

		public bool Conform => (bool)(movementHeader.BM_NoChangesToReport);

		public bool UnloadingCompletion => (bool)(movementHeader.BM_UnloadingCompleted);

		public DateTime UnloadingDate => movementHeader.BM_UnloadingDate.ToDateTime();

		public string UnloadingRemark => movementHeader.BM_UnloadingRemarks;

		public bool? StateOfSeals => movementHeader.Header.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(container => container.Seals.Count > 0) ? (bool)(movementHeader.BM_StateOfSealsBoolean) : null;
	}
}
