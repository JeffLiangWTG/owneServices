using System;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF02A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using FrUniversalReferenceConstants = Enterprise.Customs.FR.Business.UniversalReferenceConstants;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCCF02AProcessor : DTBaseProcessor<Ccf02AType>
	{
		public DTCCF02AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CCF02A processor";

		protected override ZString GetNewMessageStatus(Ccf02AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDepartureStatus(Ccf02AType messageObject)
		{
			if (messageObject.Heahea != null)
			{
				switch (messageObject.Heahea.StaNot2)
				{
					case Statuts.Anticipee:
						return NctsTransitStatusList.Codes.DeclarationAccepted;
					case Statuts.ValideeAnticipee:
						return NctsTransitStatusList.Codes.DeclarationMrnAllocated;
					case Statuts.GarantieInvalide:
						return NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
					case Statuts.NonLibPourTrans:
						return NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;
					case Statuts.SousControle:
						return NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
				}
			}
			return ZString.Empty;
		}

		protected override ZString GetNewArrivalStatus(Ccf02AType messageObject)
		{
			if (messageObject.Heahea != null)
			{
				switch (messageObject.Heahea.StaNot2)
				{
					case Statuts.SousControle:
						return NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
				}
			}
			return ZString.Empty;
		}

		protected override ZString GetNewDetailedDepartureStatus(Ccf02AType messageObject)
		{
			if (messageObject.Heahea != null)
			{
				switch (messageObject.Heahea.StaNot2)
				{
					case Statuts.DemandeRectif:
						return NctsDetailedStatusList.Codes.AmendmentRequestAcknowledge;
					case Statuts.DemandeInvalid:
						return NctsDetailedStatusList.Codes.CancellationRequestAcknowledge;
					case Statuts.GarantieSousEnrg:
						return NctsDetailedStatusList.Codes.PendingGuaranteesRegistration;
					case Statuts.AttenteGarantie:
						return NctsDetailedStatusList.Codes.PendingGuarantee;
					case Statuts.Anticipee:
						return NctsDetailedStatusList.Codes.Anticipated;
					case Statuts.NotifEmbarquement:
						return NctsDetailedStatusList.Codes.BoardingNotification;
					default:
						return NctsDetailedStatusList.Codes.Unknown;
				}
			}
			return NctsDetailedStatusList.Codes.Unknown;
		}

		protected override ZString GetNewDetailedArrivalStatus(Ccf02AType messageObject)
		{
			if (messageObject.Heahea != null)
			{
				switch (messageObject.Heahea.StaNot2)
				{
					case Statuts.NotifArriveeDest:
						return NctsDetailedStatusList.Codes.ArrivalNotification;
					case Statuts.LibereeDest:
						return NctsDetailedStatusList.Codes.ReleasedAtDestination;
					case Statuts.AvisAntArrivDem:
						return NctsDetailedStatusList.Codes.PreArrivalNotificationRequest;
					default:
						return NctsDetailedStatusList.Codes.Unknown;
				}
			}
			return NctsDetailedStatusList.Codes.Unknown;
		}

		protected override ZString GetMessageInterpretation(Ccf02AType messageObject)
		{
			var sb = new ZStringBuilder();
			if (messageObject.Heahea != null)
			{
				if (!string.IsNullOrEmpty(messageObject.Heahea.EveNot3))
				{
					sb.Append(GetParagraphInterpretation(FormattableString.Invariant($"({messageObject.Heahea.EveNot3})")));
				}
				sb.Append(GetParagraphInterpretation(messageObject.Heahea.CusGuaNumNot5));
				sb.Append(GetParagraphInterpretation(messageObject.Heahea.ComNot6));
				if (NctsHeader?.IsDepartureMovement ?? false)
				{
					sb.Append(GetDepartureStatusInterpretation(messageObject));
					sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
				}
				if (NctsHeader?.IsArrivalMovement ?? false)
				{
					sb.Append(GetArrivalStatusInterpretation(messageObject));
					sb.Append(GetDetailedArrivalStatusInterpretation(messageObject));
				}
			}
			return sb.ToString();
		}

		protected override void GenerateDocuments(EDIMessage inboundMessage)
		{
			if (NctsHeader != null)
			{
				if (MeetsFrontierConditions())
				{
					((NctsHeaderDocumentSupporter)NctsHeader.DocumentSupporter).WatermarkText = (NoResString)"Provisoire - Temporary";
					new NctsTadEdocSaver(NctsHeader).RenderTadAndStoreInEdocs(NctsHeader);
				}
			}
		}

		bool MeetsFrontierConditions()
		{
			return MeetsMovementTypeCondition()
				&& MeetsMessageStatusCondition()
				&& MeetsTransportModeCondition()
				&& MeetsPortOfDispatchCondition()
				&& MeetsDepartureAndTransitOfficesCondition();

			bool MeetsMovementTypeCondition()
				=> NctsHeader.IsDepartureMovement;

			bool MeetsMessageStatusCondition()
				=> NctsHeader.DetailedDepartureStatusCode == NctsDetailedStatusList.Codes.Anticipated;

			bool MeetsTransportModeCondition()
				=> NctsHeader.MovementHeader.BM_ExportTransportMode == ModeOfTransportList.Codes._1_SeaTransport
					|| NctsHeader.MovementHeader.BM_ExportTransportMode == ModeOfTransportList.Codes._3_RoadTransport;

			bool MeetsPortOfDispatchCondition()
			{
				var isNorthernIreland = new RefUNLOCO.Loader(NctsHeader.Factory).Load(NctsHeader.PortOfDispatch)?.IsInNorthernIreland ?? false;
				return NctsHeader.PortOfDispatch.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
					|| NctsHeader.PortOfDispatch.StartsWith(Core.Constants.CountryCodes.Ireland)
					|| isNorthernIreland;
			}

			bool MeetsDepartureAndTransitOfficesCondition()
				=> GetIsIntelligentBorder((EuOfficeCode)NctsHeader.DepartureCustomsOffice)
					|| NctsHeader.TransitCustomsOfficeCodeList.Cast<EuOfficeCode>().Any(s => GetIsIntelligentBorder(s));

			ZBool GetIsIntelligentBorder(EuOfficeCode officeCode)
			{
				var attrValue = officeCode.Office?.GetAttribute(FrUniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsIntelligentBorder);

				if (attrValue?.IsEmpty == true)
				{
					attrValue = null;
				}

				return ZBool.ParseSafe(attrValue ?? "N", ZBool.False);
			}
		}
	}
}
