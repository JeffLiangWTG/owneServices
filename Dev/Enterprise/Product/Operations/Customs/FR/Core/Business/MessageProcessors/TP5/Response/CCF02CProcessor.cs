using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.FR.MessageDefinitions.TP5;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF02C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CCF02CProcessor : TP5BaseProcessor<Ccf02CType>
	{
		public CCF02CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.StatusUpdateNotification;

		protected override ZString GetNewMessageStatus(Ccf02CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewDepartureStatus(Ccf02CType messageObject)
		{
			var status = messageObject.TransitOperation?.Statut;
			var newDepartureStatus = ZString.Empty;

			switch (status)
			{
				case Statuts.Anticipee:
					newDepartureStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
					break;
				case Statuts.NonLibPourTrans:
					newDepartureStatus = NCTS5DepartureCustomsStatusList.Codes.RejectedAtOrigin;
					break;
				case Statuts.SousControle:
					newDepartureStatus = NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
					break;
			}

			return newDepartureStatus;
		}

		protected override ZString GetNewPhase(Ccf02CType messageObject) => messageObject.TransitOperation?.Statut == Statuts.SousControle ? EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Declaration : ZString.Empty;

		protected override void DoExtraProcessing(NctsHeader header, Ccf02CType messageObject, EDIMessage inboundMessage)
		{
			base.DoExtraProcessing(header, messageObject, inboundMessage);

			if (header != null && messageObject != null && MeetsFrontierConditions(header, messageObject))
			{
				var sendingObject = ObjectFactory.Get<ITP5MessageSendingObject>($"{Core.Constants.CountryCodes.France}.{nameof(ITP5MessageSendingObject)}", header);
				sendingObject.MessageType = TP5MessageTypeList.Codes.CC170C;
				var errorCollector = new EU.Business.ErrorCollector();
				var messageSender = ObjectFactory.Get<ITP5MessageSender>($"{Core.Constants.CountryCodes.France}.{nameof(ITP5MessageSender)}", sendingObject, errorCollector);

				if (!messageSender.Send(true, out var result))
				{
					Logger.LogWarning($"IE170 message throws an error during the process of Entry {header.JobReferenceNumber} and has not been sent. Errors: {result}");
				}
			}
		}

		bool MeetsFrontierConditions(NctsHeader header, Ccf02CType messageObject)
		{
			return MeetsMovementTypeCondition()
				&& MeetsMessageCondition()
				&& MeetsTransportModeCondition()
				&& MeetsPortOfDispatchCondition()
				&& MeetsDepartureAndTransitOfficesCondition();

			bool MeetsMovementTypeCondition() => header.IsDepartureMovement;

			bool MeetsMessageCondition() => messageObject?.TransitOperation?.Statut == Statuts.NotificationEmbarquement;

			bool MeetsTransportModeCondition() => header.MovementHeader.BM_ExportTransportMode == EU.Business.ModeOfTransportList.Codes._1_SeaTransport
					|| header.MovementHeader.BM_ExportTransportMode == EU.Business.ModeOfTransportList.Codes._3_RoadTransport;

			bool MeetsPortOfDispatchCondition()
			{
				var isNorthernIreland = new RefUNLOCO.Loader(header.Factory).Load(header.PortOfDispatch)?.IsInNorthernIreland ?? false;
				return header.PortOfDispatch.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
					|| header.PortOfDispatch.StartsWith(Core.Constants.CountryCodes.Ireland)
					|| isNorthernIreland;
			}

			bool MeetsDepartureAndTransitOfficesCondition() => GetIsIntelligentBorder((EU.Business.EuOfficeCode)header.MovementHeader.DepartureCustomsOffice)
																|| header.MovementHeader.TransitCustomsOfficeCodeList.Cast<EU.Business.EuOfficeCode>().Any(s => GetIsIntelligentBorder(s));

			ZBool GetIsIntelligentBorder(EU.Business.EuOfficeCode officeCode)
			{
				var attrValue = officeCode.Office?.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsIntelligentBorder);

				if (attrValue?.IsEmpty == true)
				{
					attrValue = null;
				}

				return ZBool.ParseSafe(attrValue ?? "N", ZBool.False);
			}
		}
	}
}
