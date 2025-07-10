using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TNNNCTS5DigitizedDocumentWrapper : AnnexDocCommonWrapper, ITNNNCTSDigitizedDocument
	{
		public TNNNCTS5DigitizedDocumentWrapper(NctsHeader nctsHeaderDepartureTNN, NctsArrivalMovementHeader arrivalMovementHeader, IeDoc document, ZString docDescription) : base(document, docDescription)
		{
			this.nctsHeaderDepartureTNN = Argument.NotNull(nctsHeaderDepartureTNN, nameof(nctsHeaderDepartureTNN));
			this.arrivalMovementHeader = Argument.NotNull(arrivalMovementHeader, nameof(arrivalMovementHeader));
		}
		readonly NctsHeader nctsHeaderDepartureTNN;
		readonly NctsArrivalMovementHeader arrivalMovementHeader;

		protected override ZString ReferenceNumberCore => nctsHeaderDepartureTNN.TNNDocumentType == ESNCTS5ArrivalTNNTypeList.Codes.TransitAccompanyingDocumentTad ? arrivalMovementHeader.Header.MovementReferenceNumber : document.FileNameOnly;

		public ZString DocumentType => nctsHeaderDepartureTNN.TNNDocumentType;

		public ZString DocumentLocation
		{
			get
			{
				var additionalId = arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier;
				return additionalId.Length > 10 ? additionalId.SubstringSafe(4) : additionalId;
			}
		}
	}
}
