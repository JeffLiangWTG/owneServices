using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class ShipmentLinkingMessagesSupporter : Integration.Customs.CA.IShipmentLinkingMessagesSupporter, IDisposable
	{
		public ShipmentLinkingMessagesSupporter(ICommonShipment shipment)
		{
			this.shipment = shipment as CommonShipment;
			Argument.NotNull(this.shipment, "shipment", "shipment should not be null and should be a CommonShipment");

			this.originalCargoControlNumber = GetCargoControlNumber(this.shipment);
			this.shipment.Factory.Saved += Factory_Saved;
		}

		#region Properties

		readonly CommonShipment shipment;
		ZString originalCargoControlNumber;

		BusinessObjectFactory Factory
		{
			get
			{
				return shipment.Factory;
			}
		}

		List<EDIMessage> clonedRNSStatusMessages;
		List<EDIMessage> linkedRNSStatusMessages;
		List<EDIMessage> linkedForwardedManifestMessages;

		#endregion

		#region Hook/UnHook CusEntryNumberChanged Event

		void HookCusEntryNumberChangedEvent()
		{
			originalCargoControlNumber = GetCargoControlNumber(shipment);
			clonedRNSStatusMessages = new List<EDIMessage>();
			linkedRNSStatusMessages = new List<EDIMessage>();
			linkedForwardedManifestMessages = new List<EDIMessage>();

			foreach (CusEntryNumber cusEntryNumber in shipment.Numbers)
			{
				cusEntryNumber.CE_EntryTypeInfo.ValueChanged += CusEntryNumberChanged;
				cusEntryNumber.CE_EntryNumInfo.ValueChanged += CusEntryNumberChanged;
			}

			shipment.Numbers.CountChanged += CusEntryNumberChanged;
			shipment.Numbers.CountChanged += Numbers_CountChanged;
		}

		void UnHookCusEntryNumberChangedEvent()
		{
			shipment.Numbers.CountChanged -= CusEntryNumberChanged;
			shipment.Numbers.CountChanged -= Numbers_CountChanged;

			foreach (CusEntryNumber cusEntryNumber in shipment.Numbers)
			{
				cusEntryNumber.CE_EntryTypeInfo.ValueChanged -= CusEntryNumberChanged;
				cusEntryNumber.CE_EntryNumInfo.ValueChanged -= CusEntryNumberChanged;
			}

			clonedRNSStatusMessages = null;
			linkedRNSStatusMessages = null;
			linkedForwardedManifestMessages = null;
		}

		void Numbers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var cusEntryNumber = (CusEntryNumber)e.BizObject;

			if (e.ItemAdded)
			{
				cusEntryNumber.CE_EntryTypeInfo.ValueChanged += CusEntryNumberChanged;
				cusEntryNumber.CE_EntryNumInfo.ValueChanged += CusEntryNumberChanged;
			}
			else if (e.ItemRemoved)
			{
				cusEntryNumber.CE_EntryTypeInfo.ValueChanged -= CusEntryNumberChanged;
				cusEntryNumber.CE_EntryNumInfo.ValueChanged -= CusEntryNumberChanged;
			}
		}

		void CusEntryNumberChanged(object sender, EventArgs e)
		{
			ZString currentCargoControlNumber = GetCargoControlNumber(shipment);

			if (currentCargoControlNumber != originalCargoControlNumber)
			{
				OnCargoControlNumberChanged(currentCargoControlNumber);

				originalCargoControlNumber = currentCargoControlNumber;
			}
		}

#if DEBUG
		protected virtual
#endif
		void OnCargoControlNumberChanged(ZString cargoControlNumber)
		{
			UnlinkMessages();

			if (!cargoControlNumber.IsEmpty)
			{
				LinkRNSStatusMessagesByCCN(cargoControlNumber);
				LinkForwardedManifestMessagesByCCN(cargoControlNumber);
			}
		}

		#endregion

		#region Linking RNS Status Messages

		void LinkRNSStatusMessagesByCCN(ZString cargoControlNumber)
		{
			foreach (var matchingMessage in GetMatchingRNSStatusMessagesByCCN(cargoControlNumber))
			{
				LinkMessage(matchingMessage);
			}
		}

#if DEBUG
		protected
#endif
		EDIMessage[] GetMatchingRNSStatusMessagesByCCN(string cargoControlNumber)
		{
			ZQuery query = GetMatchingRNSStatusMesssageByCCNQuery(cargoControlNumber);

			return Factory.Load<EDIMessage>(query);
		}

		ZQuery GetMatchingRNSStatusMesssageByCCNQuery(string cargoControlNumber)
		{
			var result = new ZDBOnlyQuery(typeof(EDIMessage));
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);

			var messageTypeFilter = new ZQuery();

			var sentUnattachedRNSRequests = new ZQuery();
			sentUnattachedRNSRequests.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.RNSRequest);
			sentUnattachedRNSRequests.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			sentUnattachedRNSRequests.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
			messageTypeFilter.AddToFilter(sentUnattachedRNSRequests, JoinCondition.Or);

			var receivedUnattachedMessages = new ZQuery();
			receivedUnattachedMessages.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.EDIRelease);
			receivedUnattachedMessages.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			receivedUnattachedMessages.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
			messageTypeFilter.AddToFilter(receivedUnattachedMessages, JoinCondition.Or);

			var receivedAttachedNonErrorMessages = new ZQuery();
			receivedAttachedNonErrorMessages.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.EDIRelease);
			receivedAttachedNonErrorMessages.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			receivedAttachedNonErrorMessages.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, SQLComparisonOperator.NotEqual, null);
			receivedAttachedNonErrorMessages.AddToFilter(EDIMessageSchema.EM_LinkTable, SQLComparisonOperator.NotEqual, CommonShipment.Schema.TableName);
			receivedAttachedNonErrorMessages.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, SQLComparisonOperator.NotEqual, shipment.PK);
			var errorTypes = new[] { EDIReleaseImportEntryStatusList.Codes.Error, EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, EDIReleaseImportEntryStatusList.Codes.SyntaxError };
			receivedAttachedNonErrorMessages.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, errorTypes);
			messageTypeFilter.AddToFilter(receivedAttachedNonErrorMessages, JoinCondition.Or);

			result.AddToFilter(messageTypeFilter);
			result.AddToFilter(EDIMessageSchema.EM_ApplicationReference, cargoControlNumber.Replace(" ", ""));
			result.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Queued);
			result.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded);

			return result;
		}

		#endregion

		#region Linking Forwarded Manifest Messages

		void LinkForwardedManifestMessagesByCCN(string cargoControlNumber)
		{
			foreach (var matchingMessage in GetMatchingForwardedManifestMessagesByCCN(cargoControlNumber))
			{
				LinkMessage(matchingMessage);
			}
		}

#if DEBUG
		protected
#endif
		EDIMessage[] GetMatchingForwardedManifestMessagesByCCN(string cargoControlNumber)
		{
			ZQuery query = GetMatchingForwardedManifestMesssageByCCNQuery(cargoControlNumber);

			return Factory.Load<EDIMessage>(query);
		}

		ZQuery GetMatchingForwardedManifestMesssageByCCNQuery(string cargoControlNumber)
		{
			var result = new ZDBOnlyQuery(typeof(EDIMessage));
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAACI);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.ACIHouseBill);
			result.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageTypeList.Codes.ManifestForwardHouse);
			result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			result.AddToFilter(EDIMessageSchema.EM_ApplicationReference, cargoControlNumber.Replace(" ", ""));
			result.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Queued);
			result.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded);
			result.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
			result.AddSubQuery(EDIMessageQueryHelper.GetGenAddOnTextQuery(SQLComparisonOperator.NotEqual, SecondaryNotifyPartyTypeList.Codes.CustomsBroker, EDIMessage.Schema.SNPType), JoinCondition.And);
			return result;
		}

		#endregion

		#region Link/Unlink Messages

		void LinkMessage(EDIMessage matchingMessage)
		{
			EDIMessage message;

			if (matchingMessage.EM_LinkTable.IsEmpty && matchingMessage.EM_LinkUniqueID.IsEmpty)
			{
				message = matchingMessage;
				linkedRNSStatusMessages.Add(message);
			}
			else
			{
				message = CloneMessage(matchingMessage);
				clonedRNSStatusMessages.Add(message);
			}

			shipment.Messages.Add(message);
		}

		EDIMessage CloneMessage(EDIMessage source)
		{
			var message = (EDIMessage)source.Clone();
			foreach (var note in source.Notes.GetAllNotes())
			{
				message.Notes.Add(note.Clone());
			}

			return message;
		}

		void UnlinkMessages()
		{
			foreach (var message in clonedRNSStatusMessages)
			{
				message.EM_EI = ZGuid.Empty;
				shipment.Messages.RemoveAndDelete(message);
			}

			foreach (var message in linkedRNSStatusMessages)
			{
				shipment.Messages.Remove(message);
				message.EM_LinkedObject = null;
			}

			foreach (var message in linkedForwardedManifestMessages)
			{
				shipment.Messages.Remove(message);
				message.EM_LinkedObject = null;
			}

			clonedRNSStatusMessages.Clear();
			linkedRNSStatusMessages.Clear();
			linkedForwardedManifestMessages.Clear();
		}

		#endregion

		#region GetCargoControlNumber

		static ZString GetCargoControlNumber(CommonShipment shipment)
		{
			var cusEntryNumbers = shipment.Numbers.Find(num => num.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Canada
					&& num.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN);

			if (cusEntryNumbers.Count() == 1)
			{
				return RemoveSpaces(cusEntryNumbers.First().CE_EntryNum);
			}
			else
			{
				return ZString.Empty;
			}
		}

		static ZString RemoveSpaces(ZString value)
		{
			return value.Replace(" ", "");
		}

		#endregion

		#region IShipmentLinkingMessagesSupporter

		public void HookShipment()
		{
			HookCusEntryNumberChangedEvent();
		}

		public void UnHookShipment()
		{
			UnHookCusEntryNumberChangedEvent();
		}

		#endregion

		#region AfterSave

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				clonedRNSStatusMessages?.Clear();
				linkedRNSStatusMessages?.Clear();
				linkedForwardedManifestMessages?.Clear();
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			UnHookCusEntryNumberChangedEvent();
		}

		#endregion
	}
}
