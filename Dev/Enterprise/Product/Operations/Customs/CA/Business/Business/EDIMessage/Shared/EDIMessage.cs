using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Edifact.Auto;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class EDIMessage : Enterprise.Messaging.Business.EDIMessage, Integration.Customs.CA.IEDIMessage
	{
		public EDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			creationCallStack = new StackTrace().ToString();
		}

		public new class Schema : Enterprise.Messaging.Business.EDIMessage.Schema
		{
			public const string BatchNumber = "BatchNumber";
			public const string CargoControlNumber = "CargoControlNumber";
			public const string TransactionNumber = "TransactionNumber";
			public const string LinkedObjectReference = "LinkedObjectReference";
			public const string RNSProcessingDate = "RNSProcessingDate";
			public const string RNSReleaseDate = "RNSReleaseDate";
			public const string K84StatementDate = "K84StatementDate";
			public const string K84AccountingDate = "K84AccountingDate";
			public const string K84StatementBN9 = "K84StatementBN9";
			public const string PrimaryCargoControlNumber = "PrimaryCargoControlNumber";
			public const string SNPType = "SNPType";
			public const string DocumentMessageVersion = "DocumentMessageVersion";
			public const string SubLocation = "SubLocation";
			public const string CBSAOffice = "CBSAOffice";
			public const string MessageScheduleDescription = "MessageScheduleDescription";
			public const string XMLCustomsMessageType = "XMLCustomsMessageType";
			public const string EDIFACTInterchangeNumber = "EDIFACTInterchangeNumber";
			public const string EDIFACTMessageNumber = "EDIFACTMessageNumber";
			public const string EM_SendWithMessageErrorsFormatted = "EM_SendWithMessageErrorsFormatted";
		}

		public static readonly new TypeDecider TypeDecider = new EDIMessageTypeDecider();

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public EDIMessage LoadTop1WithDirection(ZString applicationCode, ZString messageNum, ZString receiveTransmit)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNum);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, receiveTransmit);
				return Factory.LoadTop1<EDIMessage>(query);
			}

			public EDIMessage LoadTop1(ZString applicationCode, ZString messageType, ZString messageNum)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNum);

				return Factory.LoadTop1<EDIMessage>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(EDIMessage);
			}
		}

		#endregion

		#region New properties

		internal protected SegmentGroup EdifactMessage
		{
			get { return edifactMessage ?? (edifactMessage = GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet())); }
		}
		SegmentGroup edifactMessage;

		#region BatchNumber

		public virtual ZString BatchNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo BatchNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BatchNumber); }
		}
		#endregion

		#region SNPType

		public virtual ZString SNPType
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo SNPTypeInfo
		{
			get { return GetZPropertyInfo(Schema.SNPType); }
		}

		#endregion

		#region PrimaryCargoControlNumber

		public virtual ZString PrimaryCargoControlNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo PrimaryCargoControlNumberInfo
		{
			get { return GetZPropertyInfo(Schema.PrimaryCargoControlNumber); }
		}

		#endregion

		#region CargoControlNumber

		public virtual ZString CargoControlNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo CargoControlNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CargoControlNumber); }
		}

		#endregion

		#region TransactionNumber

		public virtual ZString TransactionNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return GetZPropertyInfo(Schema.TransactionNumber); }
		}

		#endregion

		#region LinkedObjectReference

		public ZString LinkedObjectReference
		{
			get { return ImportLinkedObjectManager.GetLinkedObjectReference(EM_LinkedObject); }
		}

		public ZPropertyInfo LinkedObjectReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.LinkedObjectReference); }
		}

		#endregion

		#region RNSProcessingDate

		[BusinessObjectTestExclude]
		public virtual ZDateTime RNSProcessingDate
		{
			get { return ZDateTime.Empty; }
			set
			{
				if (!Globals.IsTest)
				{
					throw new NotSupportedException();
				}

				RNSProcessingDateInfo.RefreshBinding(RNSProcessingDate);
			}
		}

		public ZPropertyInfo RNSProcessingDateInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.RNSProcessingDate); }
		}

		#endregion

		#region RNSReleaseDate

		[BusinessObjectTestExclude]
		public virtual ZDateTime RNSReleaseDate
		{
			get { return ZDateTime.Empty; }
			set
			{
				if (!Globals.IsTest)
				{
					throw new NotSupportedException();
				}

				RNSReleaseDateInfo.RefreshBinding(RNSReleaseDate);
			}
		}

		public ZPropertyInfo RNSReleaseDateInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.RNSReleaseDate); }
		}

		#endregion

		#region K84StatementDate

		public virtual ZDateTime K84StatementDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo K84StatementDateInfo
		{
			get { return GetZPropertyInfo(Schema.K84StatementDate); }
		}

		#endregion

		#region K84AccountingDate

		public virtual ZDateTime K84AccountingDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo K84AccountingDateInfo
		{
			get { return GetZPropertyInfo(Schema.K84AccountingDate); }
		}

		#endregion

		#region K84StatementBN9

		public virtual ZString K84StatementBN9
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo K84StatementBN9Info
		{
			get { return GetZPropertyInfo(Schema.K84StatementBN9); }
		}

		#endregion

		#region DocumentMessageVersion

		public virtual ZString DocumentMessageVersion
		{
			get { return string.Empty; }
		}

		public ZPropertyInfo DocumentMessageVersionInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentMessageVersion); }
		}

		#endregion

		#region SubLocation

		public virtual ZString SubLocation
		{
			get { return string.Empty; }
		}

		public ZPropertyInfo SubLocationInfo
		{
			get { return GetZPropertyInfo(Schema.SubLocation); }
		}

		#endregion

		#region ReleaseOffice

		public virtual ZString CBSAOffice
		{
			get { return string.Empty; }
		}

		public ZPropertyInfo CBSAOfficeInfo
		{
			get { return GetZPropertyInfo(Schema.CBSAOffice); }
		}

		#endregion

		#region MessageDescription

		public virtual ZString MessageScheduleDescription
		{
			get
			{
				var result = string.Empty;
				if (!EM_HeldUntilDate.IsEmpty)
				{
					var prefix = (EM_IsActive && EM_Status != EDIMessage.Status.Cancelled) ? string.Empty : ResString.GetMultilingualString("7E8B4559-B46A-4B43-B4AC-1DB215E3EE38", "Canceled - ");
					var scheduleTime = ResString.GetMultilingualString("AE695609-51A2-4EBC-8516-407AD41891F3", "Scheduled at {0}", EnvProxy.Instance.Time.GetLocalTimeFromUtc(EM_HeldUntilDate.ToDateTime()));
					result = prefix + scheduleTime;
				}
				return result;
			}
		}

		#endregion

		#region XMLCustomsMessageType

		public virtual ZString XMLCustomsMessageType
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo XMLCustomsMessageTypeInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.XMLCustomsMessageType); }
		}

		#endregion

		#region EDIFACTInterchangeNumber

		public virtual ZInt EDIFACTInterchangeNumber
		{
			get { return ZInt.Zero; }
		}

		public ZPropertyInfo EDIFACTInterchangeNumberInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EDIFACTInterchangeNumber); }
		}

		#endregion

		#region EDIFACTMessageNumber

		public virtual ZInt EDIFACTMessageNumber
		{
			get { return ZInt.Zero; }
		}

		public ZPropertyInfo EDIFACTMessageNumberInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EDIFACTMessageNumber); }
		}

		#endregion

		#endregion

		#region Related Message Properties

		#region EM_RelatedMessageFormattedMessageText

		public ZString EM_RelatedMessageFormattedMessageText
		{
			get
			{
				var relatedMessage = RelatedMessage;
				return relatedMessage == null ? ZString.Empty : relatedMessage.EM_FormattedMessageText;
			}
		}

		public ZPropertyInfo EM_RelatedMessageFormattedMessageTextInfo
		{
			get { return GetZPropertyInfo(nameof(EM_RelatedMessageFormattedMessageText)); }
		}

		#endregion

		#region EM_RelatedMessageInterpretationText

		public ZString EM_RelatedMessageInterpretationText
		{
			get
			{
				var relatedMessage = RelatedMessage;
				return relatedMessage == null ? ZString.Empty : relatedMessage.EM_MessageInterpretation;
			}
		}

		public ZPropertyInfo EM_RelatedMessageInterpretationTextInfo
		{
			get { return GetZPropertyInfo(nameof(EM_RelatedMessageInterpretationText)); }
		}

		#endregion

		#region EM_RelatedMessageCreateTime

		public ZDateTime EM_RelatedMessageCreateTime
		{
			get
			{
				var relatedMessage = RelatedMessage;
				return relatedMessage == null ? ZDateTime.Empty : relatedMessage.EM_SystemCreateTimeUtc;
			}
		}

		public ZPropertyInfo EM_RelatedMessageCreateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(EM_RelatedMessageCreateTime)); }
		}

		#endregion

		#region EM_RelatedMessageCreateUser

		public ZString EM_RelatedMessageCreateUser
		{
			get
			{
				var relatedMessage = RelatedMessage;
				return relatedMessage == null ? ZString.Empty : relatedMessage.EM_User;
			}
		}

		public ZPropertyInfo EM_RelatedMessageCreateUserInfo
		{
			get { return GetZPropertyInfo(nameof(EM_RelatedMessageCreateUser)); }
		}

		#endregion

		#region RelatedMessage

		public EDIMessage RelatedMessage
		{
			get { return IsTransmitMessage ? ResponseMessage : OriginalMessage; }
		}

		public bool HasRelatedMessage
		{
			get { return RelatedMessage != null; }
		}

		#region OriginalMessage

		public virtual EDIMessage OriginalMessage
		{
			get
			{
				if (IsTransmitMessage)
				{
					throw new InvalidOperationException("Original Message is only available for response messages. This message is a transmit");
				}
				if (fOriginalMessage == null && !EM_MessageNum.IsEmpty)//some messages don't have outbound messages to respond to and therefore EM_MessageNum is empty
				{
					fOriginalMessage = new Loader(Factory).LoadTop1WithDirection(EM_ApplicationCode, EM_MessageNum, Direction.Transmit);
				}
				return fOriginalMessage;
			}
		}
		EDIMessage fOriginalMessage;

		#endregion

		#region ResponseMessage

		public EDIMessage ResponseMessage
		{
			get
			{
				if (!IsTransmitMessage)
				{
					return null;
				}
				if (fResponseMessage == null && !EM_MessageNum.IsEmpty)
				{
					fResponseMessage = new Loader(Factory).LoadTop1WithDirection(EM_ApplicationCode, EM_MessageNum, Direction.Receive);
				}
				return fResponseMessage;
			}
		}
		EDIMessage fResponseMessage;

		#endregion

		#endregion

		public ZString EM_SendWithMessageErrorsFormatted
		{
			get
			{
				var result = EM_SendWithMessageErrors ? "Yes" : "No";
				if (EM_ReceiveTransmit != EDIMessage.Direction.Transmit)
				{
					result = "N/A";
				}
				return result;
			}
		}

		#endregion

		#region GetErrorDescription

		CAErrorCodesDescriptionHelper ErrorDescriptionHelper
		{
			get
			{
				if (errorDescriptionHelper == null)
				{
					errorDescriptionHelper = new CAErrorCodesDescriptionHelper();
				}
				return errorDescriptionHelper;
			}
		}
		CAErrorCodesDescriptionHelper errorDescriptionHelper;

		public virtual string GetErrorDescription(string errorCode)
		{
			var result = ErrorDescriptionHelper.GetDescriptionFromCode(Factory, errorCode);

			if (string.IsNullOrEmpty(result))
			{
				result = Res.GetString("960fc52d-9b36-444f-915e-19bdb371b27b", "Unknown error ({0}), please report to CargoWise", errorCode);
			}

			return result;
		}

		#endregion

		#region Overrides

		internal MultilingualString MultilingualMessageSubTypeDescription
		{
			get
			{
				var result = MessageSubTypeList.GetMultilingualDescriptionFromCode(EM_MessageSubType);
				if (string.IsNullOrEmpty(result))
				{
					result = (NoResString)EM_MessageSubType;
				}

				return result;
			}
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new IIDMessageSubTypeList(); }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return new MessageTypeList().GetDescriptionFromCode(EM_MessageType); }
		}

		#region EM_MessageInterpretation

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (messageInterpretation == null && !EM_MessageText.IsEmpty)
				{
					switch (EM_ReceiveTransmit)
					{
						case Direction.Transmit:
							messageInterpretation = GetMessageInterpretationFromNoteWithFallback(TransmitMessageInterpretation);
							break;
						case Direction.Receive:
							messageInterpretation = GetMessageInterpretationFromNoteWithFallback(ReceiveMessageInterpretation);
							break;
						default:
							messageInterpretation = Res.GetString("71417562-9ad4-4937-9409-7919782e721f", "UNKNOWN MESSAGE DIRECTION");
							break;
					}
				}
				return messageInterpretation;
			}
			set
			{
				base.EM_MessageInterpretation = value;
				messageInterpretation = null;
				EM_MessageInterpretationInfo.RefreshBinding();
			}
		}
		string messageInterpretation;

		public override ZString EM_MessageText
		{
			get
			{
				return base.EM_MessageText;
			}
			set
			{
				var oldValue = base.EM_MessageText;
				base.EM_MessageText = value;
				if (oldValue != base.EM_MessageText)
				{
					Factory.ClearCachedValue<ZBool>(IsPostArrivalMessageKey);
				}
			}
		}

		ZString GetMessageInterpretationFromNoteWithFallback(ZString fallback)
		{
			var result = MessageInterpretationNoteManager.Value;
			return !result.IsEmpty ? result : fallback;
		}

		protected virtual ZString TransmitMessageInterpretation
		{
			get { return Res.GetString("481516d8-9d85-4a88-b902-5122c48b56e6", "OUTBOUND MESSAGE"); }
		}

		protected virtual ZString ReceiveMessageInterpretation
		{
			get { return Res.GetString("531d24e2-da18-48f0-9494-71617cc4bb3a", "INBOUND MESSAGE"); }
		}

		public virtual ZString RawMessageInterpretation => ZString.Empty;

		public virtual ZString RawMessage => ZString.Empty;

		#endregion

		#region GetNumberFountainNumbersAndFillInPlaceHolders

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			var entryHeader = EM_LinkedObject as CusEntryHeader;

			if (entryHeader != null)
			{
				entryHeader.FillInCH_BGMReference();

				var originalLength = EM_MessageText.Length;
				if (entryHeader.IsExport)
				{
					EM_MessageText = EM_MessageText.Replace(FormKeyPlaceHolder, entryHeader.CH_BGMReference.PadRight(11));
				}
				if (originalLength != EM_MessageText.Length)
				{
					ErrorReporter.ReportOnce("EDIMessage.GetNumberFountainNumbersAndFillInPlaceHolders", "Replacing placeholders changed message length");
				}
			}

			ReplaceMessageInterpretationPlaceHolders();
		}

		void ReplaceMessageInterpretationPlaceHolders()
		{
			var result = EM_MessageInterpretation
				.Replace(MessageNumberPlaceHolderHtml, EM_MessageNum)
				.Replace(UniqueBatchNumberPlaceHolderHtml, BatchNumber);

			if (result.IndexOf(EntryNumberPlaceHolderHtml) != -1)
			{
				result = result.Replace(EntryNumberPlaceHolderHtml, GetEntryNumber());
			}
			if (result.IndexOf(DocumentMessageVersionPlaceHolderHtml) != -1)
			{
				result = result.Replace(DocumentMessageVersionPlaceHolderHtml, GetDocumentMessageVersion());
			}

			EM_MessageInterpretation = result;
		}

		#endregion

		protected override IStreamFormatter MessageStreamFormatter
		{
			get { return ShouldUseUnformattedMessageText ? null : base.MessageStreamFormatter; }
		}

		protected virtual bool ShouldUseUnformattedMessageText
		{
			get { return true; }
		}

		protected override string GetMessageReferenceNumber()
		{
			if (EM_ApplicationCode == EDIMessage.ApplicationCodes.AMS)
			{
				ErrorReporter.ReportOnce("IncorrectNumberFountainInvokedForMessageAMS", $@"You cannot access CA number fountain for AMS messages.

Current Message Number: {EM_MessageNum}

Type For Load: {TypeDecider.GetTypeForLoad(((IBusinessObjectInternals)this).Row, Factory)}

Message Creation Stack Trace:
{creationCallStack}
");
				return EM_MessageNum;
			}

			return Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", ApplicationCodes.CACustoms).GetNextFormatted(Factory);
		}

		readonly string creationCallStack;

		protected override string GetBatchNumber()
		{
			var clientNetworkId = BatchProcessorUtilities.MailBoxID;
			if (string.IsNullOrEmpty(clientNetworkId))
			{
				var heading = Res.GetString("d99cf6a6-dd48-4ef0-a820-be5c7e9c7fac", "Cannot Save EDI Message");
				var message = Res.GetString("ef7845a5-a135-4746-b289-de5e16f4485c", "Client Network ID hasn't been specified. Please set it up in:") + "\r\n";
				throw new ZCannotSaveException(message + BatchProcessorUtilities.RegistryLocation(CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries), heading);
			}

			ZString numberFountainResult = "000";
			while (numberFountainResult.Right(3) == "000")
			{
				if (numberFountainResult.Length < 3)
				{
					throw new ArgumentException("Number fountain did not return a valid result");
				}

				numberFountainResult = Env.NumberFountains.EDIFACTNumberFountain("M", clientNetworkId, ApplicationCodes.CACustoms).GetNextFormatted(Factory).PadLeft(3, '0');
			}
			return numberFountainResult.Right(3);
		}

		protected override bool ShouldReplaceUniqueBatchNumber
		{
			get { return !CACustomsDataRegistry.Instance.ShouldBatchNumberBeByInterchange.Value; }
		}

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.CACustoms;
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(ForwardingShipment);
			yield return typeof(OrgHeader);
		}

		#endregion

		#region RNS Status & Notices columns
		public virtual ZString ProcessingIndicator
		{
			get { return ZString.Empty; }
		}

		public virtual ZString ServiceOption
		{
			get { return ZString.Empty; }
		}

		public virtual ZString DocumentReference
		{
			get { return ZString.Empty; }
		}

		public virtual ZString ReleaseOffice
		{
			get { return ZString.Empty; }
		}

		public virtual ZString WarehouseCode
		{
			get { return ZString.Empty; }
		}

		public virtual ZString ContainerNumbers
		{
			get { return ZString.Empty; }
		}

		public virtual ZString ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		public virtual ZString StatusDescription
		{
			get { return ZString.Empty; }
		}
		#endregion

		#region IsPostArrivalMessage

		public bool IsPostArrivalMessage()
		{
			return Factory.GetCachedValue<ZBool>(IsPostArrivalMessageKey, () =>
			{
				var result = false;
				if (this is UniversalEventMessage eventMessage)
				{
					var statusCodes = eventMessage.StatusCodes;
					if (statusCodes != null)
					{
						result = statusCodes.Contains(Arrived) || statusCodes.Contains(Reported);
					}
				}
				return result;
			});
		}

		const string Reported = "0010";
		const string Arrived = "0011";

		string IsPostArrivalMessageKey
		{
			get
			{
				return string.Format("EDIMessage|{0}|IsPostArrivalMessage", PK);
			}
		}

		#endregion
	}
}
