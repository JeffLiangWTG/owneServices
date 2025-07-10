using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessagesWrappers.Send;
using Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC;
using Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaGMessageBuilderManager : MessageBuilderManager<DeltaGJobDeclarationMessageSendingObject>
	{
		public DeltaGMessageBuilderManager(EU.Business.ErrorCollector errorCollector)
		{
			this.errorCollectorItem = Argument.NotNull(errorCollector, "errorCollector cannot be null");
		}

		public override IMessageBuilderBase NewMessageBuilder(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			// Create a builder name : DCImp+ACTION CODE + MessageBuilder: the builder manage is inherits from a base class and can override part of Populate functions
			var messageType = objectToSend.MessageType;
			var cusEntryHeader = objectToSend.Header;
			var isImport = cusEntryHeader.IsImport;

			if (cusEntryHeader.Declaration.IsDeltaC)
			{
				if (isImport)
				{
					return MessageBuilderDeltaCImport(objectToSend, messageType.ToUpper());
				}
				else
				{
					return MessageBuilderDeltaCExport(objectToSend, messageType.ToUpper());
				}
			}
			else if (cusEntryHeader.Declaration.IsDeltaD)
			{
				if (isImport)
				{
					return MessageBuilderDeltaDImport(objectToSend, messageType.ToUpper());
				}
				else
				{
					return MessageBuilderDeltaDExport(objectToSend, messageType.ToUpper());
				}
			}

			throw new NotImplementedException("CW1 doesn't yet support building message type " + messageType);
		}

		IMessageBuilderBase MessageBuilderDeltaCImport(DeltaGJobDeclarationMessageSendingObject objectToSend, ZString messageType)
		{
			deltaBuilderType = MessageTypeList.Codes.IMC;

			var importWrapper = GetDeltaCImportWrapper(objectToSend, messageType);
			var transactionType = Messaging.MessageBuilders.TransactionTypes.Original;

			if (messageType == EntryActionCodeList.Codes.ANT)
			{
				return new DCANTSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.VAL)
			{
				return new DCVALSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.MAP)
			{
				return new DCMAPSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.ANA)
			{
				return new DCANASendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.VAA)
			{
				return new DCVAASendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.EAV)
			{
				return new DCEAVSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.INV)
			{
				return new DCINVSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.CMP)
			{
				return new DCCMPSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.RPS)
			{
				return new DCRPSSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.REC)
			{
				return new DCRECSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.VAR)
			{
				return new DCVARSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.ANR)
			{
				return new DCANRSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			else
			{
				throw new NotImplementedException("CW1 doesn't yet support building message type Delta G1 Droit Commun " + messageType);
			}
		}

		IMessageBuilderBase MessageBuilderDeltaCExport(DeltaGJobDeclarationMessageSendingObject objectToSend, ZString messageType)
		{
			deltaBuilderType = MessageTypeList.Codes.EXC;

			var exportWrapper = GetDeltaCExportWrapper(objectToSend, messageType);
			var transactionType = Messaging.MessageBuilders.TransactionTypes.Original;

			if (messageType == EntryActionCodeList.Codes.ANT)
			{
				return new DCANTSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.MAP)
			{
				return new DCMAPSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.ANA)
			{
				return new DCANASendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.VAA)
			{
				return new DCVAASendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.EAV)
			{
				return new DCEAVSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.INV)
			{
				return new DCINVSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.CMP)
			{
				return new DCCMPSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.RPS)
			{
				return new DCRPSSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.REC)
			{
				return new DCRECSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.VAR)
			{
				return new DCVARSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.ANR)
			{
				return new DCANRSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.VAL)
			{
				return new DCVALSendExpMessageBuilder(exportWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			else
			{
				throw new NotImplementedException("CW1 doesn't yet support building message type Delta G1 Droit Commun " + messageType);
			}
		}

		IMessageBuilderBase MessageBuilderDeltaDImport(DeltaGJobDeclarationMessageSendingObject objectToSend, ZString messageType)
		{
			deltaBuilderType = MessageTypeList.Codes.IMD;

			var importWrapper = GetDeltaDImportWrapper(objectToSend);
			var transactionType = Messaging.MessageBuilders.TransactionTypes.Original;

			if (messageType == EntryActionCodeList.Codes.VAL)
			{
				return new DDVALSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.D2M)
			{
				return new DDD2MSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.ANT)
			{
				return new DDANTSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.MDV)
			{
				return new DDMDVSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.RPS)
			{
				return new DDRPSSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.MDA)
			{
				return new DDMDASendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.VAA)
			{
				return new DDVAASendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.ANN)
			{
				return new DDANNSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.REC)
			{
				return new DDRECSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.INV)
			{
				return new DDINVSendImpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			else
			{
				throw new NotImplementedException("CW1 doesn't yet support building message type Delta G2 Import " + messageType);
			}
		}

		IMessageBuilderBase MessageBuilderDeltaDExport(DeltaGJobDeclarationMessageSendingObject objectToSend, ZString messageType)
		{
			deltaBuilderType = MessageTypeList.Codes.EXD;

			var importWrapper = GetDeltaDExportWrapper(objectToSend);
			var transactionType = Messaging.MessageBuilders.TransactionTypes.Original;

			if (messageType == EntryActionCodeList.Codes.VAL)
			{
				return new DDVALSendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.D2M)
			{
				return new DDD2MSendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.ANT)
			{
				return new DDANTSendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.MDV)
			{
				return new DDMDVSendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.RPS)
			{
				return new DDRPSSendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.MDA)
			{
				return new DDMDASendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.VAA)
			{
				return new DDVAASendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.ANN)
			{
				return new DDANNSendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.REC)
			{
				return new DDRECSendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			if (messageType == EntryActionCodeList.Codes.INV)
			{
				return new DDINVSendExpMessageBuilder(importWrapper, errorCollectorItem, transactionType, objectToSend.SequenceNumber);
			}
			else
			{
				throw new NotImplementedException("CW1 doesn't yet support building message type Delta G2 Export " + messageType);
			}
		}

		DCSendImpMessageWrapper GetDeltaCImportWrapper(DeltaGJobDeclarationMessageSendingObject objectToSend, ZString messageType)
		{
			switch (messageType)
			{
				case EntryActionCodeList.Codes.EAV:
					return new DCEAVSendImpMessageWrapper(objectToSend, errorCollectorItem);
				case EntryActionCodeList.Codes.VAA:
					return new DCVAASendImpMessageWrapper(objectToSend, errorCollectorItem);
				case EntryActionCodeList.Codes.VAR:
					return new DCVARSendImpMessageWrapper(objectToSend, errorCollectorItem);
				default:
					return new DCSendImpMessageWrapper(objectToSend, errorCollectorItem);
			}
		}

		DCSendExpMessageWrapper GetDeltaCExportWrapper(DeltaGJobDeclarationMessageSendingObject objectToSend, ZString messageType)
		{
			switch (messageType)
			{
				case EntryActionCodeList.Codes.EAV:
					return new DCEAVSendExpMessageWrapper(objectToSend, errorCollectorItem);
				case EntryActionCodeList.Codes.VAA:
					return new DCVAASendExpMessageWrapper(objectToSend, errorCollectorItem);
				case EntryActionCodeList.Codes.VAR:
					return new DCVARSendExpMessageWrapper(objectToSend, errorCollectorItem);
				default:
					return new DCSendExpMessageWrapper(objectToSend, errorCollectorItem);
			}
		}

		DDSendImpMessageWrapper GetDeltaDImportWrapper(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			return new DDSendImpMessageWrapper(objectToSend, errorCollectorItem);
		}

		DDSendExpMessageWrapper GetDeltaDExportWrapper(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			return new DDSendExpMessageWrapper(objectToSend, errorCollectorItem);
		}

		public override ZString BuilderType
		{
			get { return deltaBuilderType; }
		}

		readonly EU.Business.ErrorCollector errorCollectorItem;
		ZString deltaBuilderType = ZString.Empty;
	}
}
