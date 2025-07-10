using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE044UnloadingRemarkProvider : IIE044UnloadingRemark
	{
		public IE044UnloadingRemarkProvider(NctsHeader nctsHeader)
		{
			this.header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			this.arrivalMovementHeader = header.ArrivalMovementHeader;
		}
		readonly NctsHeader header;
		readonly NctsArrivalMovementHeader arrivalMovementHeader;

		public bool Conform => arrivalMovementHeader.BM_NoChangesToReport;

		public bool UnloadingCompletion => arrivalMovementHeader.BM_UnloadingCompleted;

		public DateTime UnloadingDate => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(arrivalMovementHeader.BM_UnloadingDate.ToZDateTime());

		public bool HasStateOfSeals => header.ArrivalHeaderContainers.Count > 0 && header.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(p => p.TotalSealCount > 0);
		public bool StateOfSeals => arrivalMovementHeader.BM_StateOfSealsBoolean;

		public string UnloadingRemark => arrivalMovementHeader.BM_UnloadingRemarks;

		public string MRN => header.MovementReferenceNumber;
	}
}
