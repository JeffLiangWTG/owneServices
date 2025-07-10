using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class ArrivalTransitOperationWrapper : IArrivalTransitOperation
	{
		ArrivalTransitOperationWrapper(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		readonly NctsHeader nctsHeader;

		public static ArrivalTransitOperationWrapper New(NctsHeader nctsHeader) => nctsHeader?.ArrivalMovementHeader == null ? null : new ArrivalTransitOperationWrapper(nctsHeader);

		NctsArrivalMovementHeader movementHeader => cachedMovementHeader ?? (cachedMovementHeader = nctsHeader.ArrivalMovementHeader);
		NctsArrivalMovementHeader cachedMovementHeader;

		public string EORIOperateurBeneficiaireAgrement => eORIOperateurBeneficiaireAgrement ?? (eORIOperateurBeneficiaireAgrement = GetEORIOperateurBeneficiaireAgrement());
		string eORIOperateurBeneficiaireAgrement;

		string GetEORIOperateurBeneficiaireAgrement() => nctsHeader.DestinationTrader?.Address.GetEORI() ?? string.Empty;

		public string MRN => mrn ?? (mrn = nctsHeader.MovementReferenceNumber);
		string mrn;

		public DateTime ArrivalNotificationDateAndTime => movementHeader.BM_ArrivalDate.ToDateTime();

		public bool SimplifiedProcedure => !movementHeader?.AuthorizationCode.IsEmpty ?? false;

		public bool IncidentFlag => nctsHeader.BH_ExportFlag == YesNoList.Codes.Yes;

		public string DestinationDouaniere => ZString.Empty; //Filed is added in WI00710803 ;

		public string OtherThingsToReport => movementHeader.OtherThingsToReport;
	}
}
