using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE170TransitOperationProvider : IIE170TransitOperation
	{
		public IE170TransitOperationProvider(NctsDepartureMovementHeader movementHeader)
		{
			MovementHeader = Argument.NotNull(movementHeader, nameof(MovementHeader));
		}
		public readonly NctsDepartureMovementHeader MovementHeader;

		public DateTime LimitDate => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(MovementHeader.BM_ExportDate, removeMillisecond: true);

		public string LRN => MovementHeader.BM_PaperlessInbondNum;

		public bool ReducedDatasetIndicator => MovementHeader.BM_ReducedDatasetIndicator;
	}
}
