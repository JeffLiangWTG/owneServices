using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRMessageBuilder : IMessageBuilder
	{
		public CMRMessageBuilder() { }

		public CMRMessageBuilder(ZString messageOwnerSiteID)
			: this()
		{
			this.MessageOwnerSiteID = messageOwnerSiteID;
		}

		public ZString MessageOwnerSiteID
		{
			get
			{
				if (fMessageOwnerSiteID.IsEmpty)
				{
					if (GlbCompany.CurrentCompany == null
						|| GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
					{
						ErrorReporter.ReportOnce("CustomsRegistrationNo", "Current Company Customs Registration No.  This must be set before accessing this builder.  Customs Site ID should be passed into the builder if Current Company is not available");
					}
				}
				return fMessageOwnerSiteID;
			}
			set
			{
				fMessageOwnerSiteID = value;
			}
		}
		ZString fMessageOwnerSiteID;

		public IMessageBuilderResult PopulateMessages()
		{
			var messageBuilderResult = new MessageBuilderResult();
			var builderResult = new BuilderResult(null, Array.Empty<string>(), null);
			builderResult.Message = PopulateMessagesReturningResult();
			messageBuilderResult.AddBuilderResult(builderResult);
			return messageBuilderResult;
		}

		public EDIMessage PopulateMessagesReturningResult()
		{
			EDIMessage messageToSend = null;
			try
			{
				CheckCertificatesIfNeeded();
				var messageText = this.MessageText;
				messageToSend = Messages.AddNew(TypeOfMessage);
				messageToSend.EM_MessageSubType = MessageSubTypeCode;
				messageToSend.EM_MessageText = messageText;
				if (IsBureau)
				{
					messageToSend.EM_MessageOwner = MessageOwnerSiteID;
				}

				SetAdditionalEDIMessageDetails(messageToSend);
			}
			catch
			{
				messageToSend?.Delete();
				throw;
			}
			return messageToSend;
		}

		protected internal virtual ZString MessageInterpretation => ZString.Empty;

		protected virtual bool IsBureau => false;

		protected virtual void SetAdditionalEDIMessageDetails(EDIMessage message)
		{
		}

		public ZString MessageText
		{
			get
			{
				GenerateMessageText();
				return EdifactMessage.ToString(new UNOCCMRCharacterSet());
			}
		}

		public ZString[] GeneratedMessageStrings
		{
			get { return new ZString[] { MessageText }; }
		}

		public EDIMessageCollection Messages;
		public Common.MessageBuilders.MessageSubTypes MessageSubType;

		internal bool IsWithdrawal => MessageSubType == Common.MessageBuilders.MessageSubTypes.Withdraw;

		public ISendsMessagesToCustoms MessageInitiator;
		public bool SetStatusToPending;
		public bool SetSplitMessageIdentifier;
		public int VersionOffset;

		internal ZString LastOriginalBGMReference
		{
			get
			{
				var lastOriginalMessage = this.LastOriginalMessage;
				return lastOriginalMessage is CMRMessage ? ((CMRMessage)lastOriginalMessage).BGMReference : ZString.Empty;
			}
		}

		internal EDIMessage LastOriginalMessage
		{
			get
			{
				EDIMessage result = null;
				foreach (EDIMessage message in Messages)
				{
					if (message.EM_MessageType == EM_MessageType && message.EM_MessageSubType == CMRMessage.MessageSubTypes.Original)
					{
						if (result == null)
						{
							result = message;
						}
						else
						{
							if (message.EM_SystemCreateTimeUtc > result.EM_SystemCreateTimeUtc)
							{
								result = message;
							}
						}
					}
				}
				return result;
			}
		}

		#region Implementation

		internal void CheckCertificatesIfNeeded()
		{
			if (MessageInitiator != null)
			{
				var factory = Messages?.Factory ?? new BusinessObjectFactory();

				var result = "";
				foreach (var error in new CMRUtilities().GetCertificateErrors(factory))
				{
					result += error + "\r\n";
				}

				if (result.Length > 0)
				{
					MessageInitiator.WarnUserAboutSomething("There are one or more problems with the cryptographic registry items:\r\n\r\n" + result, "Warning");
				}
				else
				{
					Env.Registry.AUCustoms.LastDateCertificatesChecked = DateTime.MinValue;
				}
			}
		}

		protected internal abstract void GenerateMessageText();

		protected virtual int GetNumberOfOriginals(EDIMessageCollection messages, ZString messageCode, Common.MessageBuilders.MessageSubTypes messageSubType)
		{
			var originalsInDB = 0;
			foreach (var message in messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CMR, new ZString[] { messageCode }, EDIMessage.Direction.Transmit))
			{
				if (message.EM_MessageSubType == CMRMessage.MessageSubTypes.Original && message.IsInDatabase)
				{
					originalsInDB++;
				}
			}
			return messageSubType == Common.MessageBuilders.MessageSubTypes.Create ? originalsInDB + 1 : originalsInDB;
		}

		protected void PopulateUNH()
		{
			MessageUtilities.PopulateUNH(UNH, EDIMessage.MessageNumberPlaceHolder, UNHMessageType, MessageVersionNumberList.DraftVersionUnEdifactDirectory, MessageReleaseNumberList.Release1999B, ControllingAgencyList.UnCefact);
		}

		internal void PopulateBGM()
		{
			MessageUtilities.PopulateBGM(BGM, DocumentNameCode, DocumentName, BGMReference, Version.ToString(), MessageFunctionCode);
		}

		protected internal virtual int Version
		{
			get
			{
				return Messages.GetMatchingMessages(CMRMessage.ApplicationCodes.CMR, new ZString[] { EM_MessageType }, CMRMessage.Direction.Transmit).Length + VersionOffset + 1;
			}
		}

		protected int CurrentVersion
		{
			get { return Version - 1; }
		}

		internal ZString BGMReference
		{
			get
			{
				var result = ZString.Empty;
				if (MessageSubType != Common.MessageBuilders.MessageSubTypes.Create)
				{
					result = LastOriginalBGMReference;
				}

				if (result.IsEmpty)
				{
					var numberOfOriginals = GetNumberOfOriginals(Messages, EM_MessageType, MessageSubType);
					result = EDIMessage.SendersReferencePlaceHolder + "/" + Env.Registry.PhysicalServerID + numberOfOriginals;
				}
				return result;
			}
		}

		protected void PopulateUNT()
		{
			MessageUtilities.PopulateUNT(UNT, EdifactMessage.CountIncludingUNT.ToString(), UNH.MessageReferenceNumber);
		}

		protected MessageFunctionCodeList MessageFunctionCode
		{
			get
			{
				MessageFunctionCodeList result = null;
				switch (MessageSubType)
				{
					case Common.MessageBuilders.MessageSubTypes.Create:
						result = MessageFunctionCodeList.Original;
						break;
					case Common.MessageBuilders.MessageSubTypes.Replace:
						result = MessageFunctionCodeList.Replace;
						break;
					case Common.MessageBuilders.MessageSubTypes.Withdraw:
						result = MessageFunctionCodeList.Withdraw;
						break;
					case Common.MessageBuilders.MessageSubTypes.Change:
						result = MessageFunctionCodeList.Change;
						break;
					case Common.MessageBuilders.MessageSubTypes.ReplaceHeader:
						result = MessageFunctionCodeList.ReplaceHeadingSectionOnly;
						break;
					case Common.MessageBuilders.MessageSubTypes.Request:
						result = MessageFunctionCodeList.Request;
						break;
				}
				return result;
			}
		}

		protected ZString MessageSubTypeCode
		{
			get
			{
				var result = ZString.Empty;
				if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Create)
				{
					result = CMRMessage.MessageSubTypes.Original;
				}
				else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Replace)
				{
					result = CMRMessage.MessageSubTypes.Amendment;
				}
				else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Change)
				{
					result = CMRMessage.MessageSubTypes.Change;
				}
				else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Withdraw)
				{
					result = CMRMessage.MessageSubTypes.Withdraw;
				}
				else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.ReplaceHeader)
				{
					result = CMRMessage.MessageSubTypes.ReplaceHeader;
				}
				else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Request)
				{
					result = CMRMessage.MessageSubTypes.Request;
				}

				return result;
			}
		}

		protected internal abstract UNTSegment UNT { get; }
		protected internal abstract BGMSegment BGM { get; }
		protected internal abstract UNHSegment UNH { get; }
		protected internal abstract SegmentGroup EdifactMessage { get; }
		protected internal abstract MessageTypeList UNHMessageType { get; }
		protected internal abstract ZString EM_MessageType { get; }
		protected internal abstract ZString DocumentName { get; }
		protected internal abstract DocumentNameCodeList DocumentNameCode { get; }
		protected internal abstract Type TypeOfMessage { get; }

		#endregion
	}
}
