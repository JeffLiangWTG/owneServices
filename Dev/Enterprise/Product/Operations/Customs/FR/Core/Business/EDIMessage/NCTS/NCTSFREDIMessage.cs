using System.Data;
using CargoWise.Customs.FR.MessageDefinitions.TP5;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class NCTSFREDIMessage : FREDIMessage
	{
		public NCTSFREDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.TP5;
		}

		protected override MessageDataObject GenerateMessageDataObject()
		{
			switch (EM_MessageSubType)
			{
				case TP5ResponseMessageSubTypeList.Codes.DeclarationAcceptance:
					return new CC004CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.InvalidationDecision:
					return new CC009CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.DiscrepanciesAtDestination:
					return new CC019CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.NotificationToAmendDeclaration:
					return new CC022CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.GoodsReleaseNotification:
					return new CC025CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.MrnAllocated:
					return new CC028CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.ReleasedForTransit:
					return new CC029CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.RecoveryNotification:
					return new CC035CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.UnloadingPermission:
					return new CC043CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.GuaranteeNotValid:
					return new CC055CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.WriteOffNotification:
					return new CC045CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.RejectionFromOfficeOfDeparture:
					return new CC056CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.RejectionFromOfficeOfDestination:
					return new CC057CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.RequestOnNonArrivedMovement:
					return new CC140CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.ForwardedIncidentNotificationToEd:
					return new CC182CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.FunctionalRejection:
					return new CD906CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.XmlNack:
					return new CC917CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.PositiveAcknowledge928:
					return new CC928CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.StatusUpdateNotification:
					return new CCF02CMessageDataObject(this);
				case TP5ResponseMessageSubTypeList.Codes.PositiveAcknowledgeF03:
					return new CCF03CMessageDataObject(this);
				default:
					return null;
			}
		}

		public static ZString GetOriginalStatus(Statuts? status)
		{
			return status switch
			{
				Statuts.Anticipee => "ANTICIPEE",
				Statuts.AttenteGarantie => "ATTENTE_GARANTIE",
				Statuts.AvisAntArrivDem => "AVIS_ANT_ARRIV_DEM",
				Statuts.DemandeInvalid => "DEMANDE_INVALID",
				Statuts.DemandeRectif => "DEMANDE_RECTIF",
				Statuts.DiffNonResolues => "DIFF_NON_RESOLUES",
				Statuts.GarantieSousEnrg => "GARANTIE_SOUS_ENRG",
				Statuts.InfoRecouvrement => "INFO_RECOUVREMENT",
				Statuts.LibereeDestination => "LIBEREE_DESTINATION",
				Statuts.NonLibPourTrans => "NON_LIB_POUR_TRANS",
				Statuts.NotifArriveeDest => "NOTIF_ARRIVEE_DEST",
				Statuts.NotificationEmbarquement => "NOTIFICATION_EMBARQUEMENT",
				Statuts.RechercheEngagee => "RECHERCHE_ENGAGEE",
				Statuts.RecouvrRecommande => "RECOUVR_RECOMMANDE",
				Statuts.SousControle => "SOUS_CONTROLE",
				Statuts.ValideeAnticipee => "VALIDEE_ANTICIPEE",
				null => ZString.Empty,
				_ => ZString.Empty,
			};
		}
	}
}
