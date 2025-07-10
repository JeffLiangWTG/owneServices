using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class ActiveBorderTransportMeansWrapper : IActiveBorderTransportMeans
	{
		ActiveBorderTransportMeansWrapper(NctsCommonMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}
		readonly NctsCommonMovementHeader movementHeader;

		public static ActiveBorderTransportMeansWrapper New(NctsCommonMovementHeader movementHeader) => movementHeader == null ? null : new ActiveBorderTransportMeansWrapper(movementHeader);

		public string CustomsOfficeAtBorderReference => customsOfficeAtBorderReference ?? (customsOfficeAtBorderReference = movementHeader.BM_CustomsOfficeAtBorder);
		string customsOfficeAtBorderReference;

		public string TypeOfIdentification => typeOfIdentification ?? (typeOfIdentification = movementHeader.BM_ActiveBorderIdentificationType);
		string typeOfIdentification;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = movementHeader.BM_TOLCarrierID);
		string identificationNumber;

		public string Nationality => nationality ?? (nationality = movementHeader.BM_RN_NKTOLCarrierNationality);
		string nationality;

		public string ConveyenceReferenceNumber => conveyenceReferenceNumber ?? (conveyenceReferenceNumber = movementHeader.BM_ConveyanceNumber);
		string conveyenceReferenceNumber;
	}
}
