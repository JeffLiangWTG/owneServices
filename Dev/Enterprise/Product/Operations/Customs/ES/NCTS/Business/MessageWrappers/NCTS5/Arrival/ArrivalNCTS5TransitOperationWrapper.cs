using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5TransitOperationWrapper : NCTS5CommonTransitOperationMRNWrapper, IArrivalNCTSTransitOperation
	{
		public ArrivalNCTS5TransitOperationWrapper(NctsHeader nctsHeader) : base(nctsHeader)
		{
			arrivalMovementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}
		readonly NctsArrivalMovementHeader arrivalMovementHeader;

		public ZBool SimplifiedProcedure => arrivalMovementHeader.IsSimplifiedNctsProcedure;

		public ZBool IncidentFlag => nctsHeader.BH_ExportFlag == ExportFlagYes;

		const string ExportFlagYes = "Y";
	}
}
