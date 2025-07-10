using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Xsl;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Billing.Integration;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.HttpXmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Messaging.Business
{
	[CodeProperty(EDIMessage.Schema.EM_MessageNum), DescriptionProperty(EDIMessage.Schema.EM_MessageNum)]
	[SystemDefinedValues]
	[ShouldDisplayInUtcTimeForEditAndCreateLogFields]
	public class EDIMessage : AutoEDIMessage, IEDIMessage, IDocManagerSupport, IBranchProvider, IResetToQueuedStatusSupporter, ISavingProvider<EDIMessage>
	{
		#region Schema
		public new abstract class Schema : AutoEDIMessage.Schema
		{
			public const string EM_User = "EM_User";
			public const string EM_DateTimeInterchangeSent = "EM_DateTimeInterchangeSent";
			public const string EM_FormattedMessageText = "EM_FormattedMessageText";
			public const string EM_InterchangeNumber = "EM_InterchangeNumber";
			public const string EM_InterchangeStatus = "EM_InterchangeStatus";
			public const string HumanReadableMessage = "HumanReadableMessage";
			public const string EM_MessageDescriptionFromSubType = "EM_MessageDescriptionFromSubType";
			public const string EM_SendOrReceiveHumanReadable = "EM_SendOrReceiveHumanReadable";
			public const string EM_MessageSubTypeDescription = "EM_MessageSubTypeDescription";
			public const string EM_MessageInterpretation = "EM_MessageInterpretation";
			public const string EM_MessageDateTime = "EM_MessageDateTime";
			public const string EM_MessageTextShort = "EM_MessageTextShort";
			public const string EM_InterchangeSender = "EM_InterchangeSender";
			public const string EM_InterchangeReceiver = "EM_InterchangeReceiver";
			public const string EM_SendingUser = "EM_SendingUser";
		}

		#endregion

		public EDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			fFactoryNewSequenceNumber = Interlocked.Increment(ref nextFactoryNewSequenceNumber);
			InitializeCreationStackTrace();
		}

		protected virtual void InitializeCreationStackTrace()
		{
			creationStackTrace = new StackTrace().ToString();
		}

		public static readonly TypeDecider TypeDecider = new EDIMessageTypeDecider();

		public static void MarkMessageAsFailed(ZGuid messagePK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			EDIMessage message = factory.Load<EDIMessage>(messagePK);
			if (message != null)
			{
				message.EM_Status = EDIMessage.Status.Failed;
				factory.Save();
			}
		}

		#region FactoryNewSequenceNumber

		internal long FactoryNewSequenceNumber
		{
			get
			{
				if (IsInDatabase)
				{
					throw new InvalidOperationException("FactoryNewSequenceNumber should not be called after EDIMessage is saved");
				}
				return fFactoryNewSequenceNumber;
			}
		}
		readonly long fFactoryNewSequenceNumber;

		[ThreadSafe]
		static long nextFactoryNewSequenceNumber = -1;

		#endregion

		protected override bool SupportsCloneCore() => true;

		#region BusinessObjectOverrides

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for ErrorReporter message")]
		public override void Delete()
		{
			if (!IsDeleted)
			{
				string reasonWhyMessageCannotBeDeleted;
				if (IsInDatabase && ReportDelete(out reasonWhyMessageCannotBeDeleted))
				{
					ReportError(string.Format(CultureInfo.InvariantCulture, "The previously persisted EDIMessage shouldn't be deleted. Application Code {0}", EM_ApplicationCode), reasonWhyMessageCannotBeDeleted + " " + DiagnosticDetails);
				}
				else if (Interchange != null && Interchange.IsInDatabase)
				{
					ReportError(string.Format(CultureInfo.InvariantCulture, "The EDIMessage linked to a previously persisted EDIInterchange shouldn't be deleted. Application Code {0}", EM_ApplicationCode), DiagnosticDetails);
				}
			}
			base.Delete();
		}

		void ReportError(string key, string message)
		{
#if DEBUG
			if (IsDeletingInTest)
			{
				return;
			}
#endif
			ErrorReporter.ReportOnce(key, message);
		}

		bool ReportDelete(out string reason)
		{
			return EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive ? ReportDeleteForReceiveMessage(out reason) : ReportDeleteForTransmitMessage(out reason);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for ErrorReporter message")]
		bool ReportDeleteForReceiveMessage(out string reason)
		{
			string serviceTaskName = null;
			switch (EM_ApplicationCode)
			{
				case ApplicationCodeList.Codes.XMS:
					if (EM_MessageType == EDIMessageTypeList.Codes.XMS)
					{
						serviceTaskName = "Standard XML Message or Event";
					}
					else
					{
						serviceTaskName = "Unlisted XML message";
					}
					break;
				case ApplicationCodeList.Codes.SYS:
					switch (EM_MessageSubType)
					{
						case SystemMessageList.Codes.CustomerServiceResponse:
						case SystemMessageList.Codes.ReferenceDataUpdate:
						case SystemMessageList.Codes.TranslationFeedbackEntry:
						case SystemMessageList.Codes.TranslationFeedbackUpdate:
							serviceTaskName = "System Message";
							break;
						default:
							serviceTaskName = "Unlisted System message";
							break;
					}
					break;
				case ApplicationCodeList.Codes.UniversalDataMessaging:
					if (EM_MessageType == EDIMessageTypeList.Codes.XDC)
					{
						switch (EM_MessageSubType)
						{
							case EDIMessageSubTypeList.Codes.XmlUniversalSchedule:
								serviceTaskName = "Universal Schedule Messaging Inbound";
								break;
							case EDIMessageSubTypeList.Codes.XmlUniversalShipment:
							case EDIMessageSubTypeList.Codes.XmlUniversalEvent:
							case EDIMessageSubTypeList.Codes.XmlUniversalTransaction:
								serviceTaskName = "Universal Shipment/Event/Transaction Messaging Inbound";
								break;
							default:
								serviceTaskName = "Unlisted Universal Message";
								break;
						}
					}
					else
					{
						serviceTaskName = "Unlisted Universal Message";
					}
					break;
				case ApplicationCodeList.Codes.NativeDataMessaging:
					if (EM_MessageType == EDIMessageTypeList.Codes.XDC)
					{
						serviceTaskName = "Native Data Messaging Inbound";
					}
					else
					{
						serviceTaskName = "Unlisted Native Message";
					}
					break;
			}

			if (serviceTaskName != null)
			{
				switch (EM_Status)
				{
					case EDIMessage.Status.Queued:
						reason = string.Format(CultureInfo.InvariantCulture, "You are attempting to delete an EDIMessage that has been queued for the {0} service task.", serviceTaskName);
						break;
					case EDIMessage.Status.Received:
					case EDIMessage.Status.ProcessedOK:
						reason = string.Format(CultureInfo.InvariantCulture, "You are attempting to delete an EDIMessage that has already been processed by the {0} and changes have been applied to the system. It is required for support purposes.", serviceTaskName);
						break;
					case EDIMessage.Status.Error:
					case EDIMessage.Status.Failed:
						reason = string.Format(CultureInfo.InvariantCulture, "You are attempting to delete an EDIMessage that has already been unsuccessfully processed by the {0} service task. It is required for support purposes.", serviceTaskName);
						break;
					default:
						reason = string.Format(CultureInfo.InvariantCulture, "You are attempting to delete an EDIMessage that is meant to be processed by the {0} service task. Message status is {1}.", serviceTaskName, EM_Status);
						break;
				}

				return true;
			}

			reason = null;
			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for ErrorReporter message")]
		bool ReportDeleteForTransmitMessage(out string reason)
		{
			switch (EM_Status)
			{
				case EDIMessage.Status.Sent:
					reason = "You are attempting to delete an EDIMessage that has been successfully sent. It is required for support purposes.";
					break;
				case EDIMessage.Status.Acknowledged:
					reason = "You are attempting to delete an EDIMessage that has been successfully acknowledged. It is required for support purposes.";
					break;
				case EDIMessage.Status.Queued:
					reason = "You are attempting to delete an EDIMessage that has been successfully queued.";
					break;
				case EDIMessage.Status.Failed:
					reason = "You are attempting to delete an EDIMessage that failed to be sent successfully. It is required for support purposes.";
					break;
				default:
					reason = string.Format(CultureInfo.InvariantCulture, "You are attempting to delete an EDIMessage with the message status {0}.", EM_Status);
					break;
			}
			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "string format")]
		public string DiagnosticDetails
		{
			get
			{
				var messageDataBuilder = new ZStringBuilder();
				foreach (SchemaColumn column in EDIMessageSchema.All)
				{
					if (column == null)
					{
						continue;
					}

					var property = GetType().GetProperty(column.Name);
					if (property != null && !property.GetCustomAttributes(typeof(BusinessObjectTestExclude), false).Any())
					{
						var value = property.GetValue(this, null);
						if (value != null && !string.IsNullOrEmpty(value.ToString()) && !IsEmptyByteArray(value))
						{
							messageDataBuilder.Append(string.Format("{0}: {1}", ZPropertyInfo.GetFriendlyColumnNameShared(column.Name), value));
						}
					}
				}
				ZStringBuilder interchangeDataBuilder = null;
				if (Interchange != null)
				{
					interchangeDataBuilder = new ZStringBuilder();
					foreach (SchemaColumn column in EDIInterchangeSchema.All)
					{
						if (column == null)
						{
							continue;
						}

						var property = GetType().GetProperty(column.Name);
						if (property != null)
						{
							var value = property.GetValue(Interchange, null);
							if (value != null && !string.IsNullOrEmpty(value.ToString()) && !IsEmptyByteArray(value))
							{
								interchangeDataBuilder.Append(string.Format("{0}: {1}", ZPropertyInfo.GetFriendlyColumnNameShared(column.Name), value));
							}
						}
					}
				}
				if (Interchange == null)
				{
					return string.Format("{{EDIMessage:{{{0}}}}}", messageDataBuilder.ToStringWithDelimiterBetweenAppends(", "));
				}
				else
				{
					return string.Format("{{EDIMessage:{{{0}}}   EDIInterchange:{{{1}}}}}", messageDataBuilder.ToStringWithDelimiterBetweenAppends(", "), interchangeDataBuilder.ToStringWithDelimiterBetweenAppends(", "));
				}
			}
		}

		bool IsEmptyByteArray(object value)
		{
			var bytes = value as byte[];
			if (bytes == null)
			{
				return false;
			}

			return bytes.Length == 0;
		}

		#endregion

		#region Related BusinessObjects
		#region Interchange
		public EDIInterchange Interchange
		{
			get { return (EDIInterchange)Factory.Load(typeof(EDIInterchange), EM_EI); }
		}
		#endregion

		#region LinkedEDIMessage
		public EDIMessage LinkedEDIMessage
		{
			get
			{
				if (EM_ReceiveTransmit == Direction.Transmit)
				{
					return Factory.Load<EDIMessage>(EM_EM_RequestMessage);
				}
				else
				{
					var query = new ZQuery(EDIMessageSchema.EM_EM_RequestMessage, PK);
					return Factory.LoadTop1<EDIMessage>(query);
				}
			}
		}
		#endregion

		#region LinkedEDIMessageNumber
		public ZString LinkedEDIMessageNumber => LinkedEDIMessage?.EM_MessageNum ?? ZString.Empty;

		public ZPropertyInfo LinkedMessageNumberInfo
		{
			get { return GetZPropertyInfo(nameof(LinkedEDIMessageNumber)); }
		}
		#endregion

		#region MessageSentLog
		protected StmALog MessageSentLog
		{
			get
			{
				if (fMessageSentLog == null)
				{
					fMessageSentLog = StmALogEntryLocator.Instance.GetLastPostEventOfType(this,
						AutoEvents.DeclarationQueued,
						AutoEvents.DeclarationAmendmentQueued,
						AutoEvents.DeclarationCancellationQueued); //JobDeclaration
					if (fMessageSentLog == null && IsTransmitMessage)
					{
						fMessageSentLog = StmALogEntryLocator.Instance.GetLastPostEventOfType(this, AutoEvents.AddedARecordToTheSystem); //CusEntryHeader
					}
				}
				return fMessageSentLog;
			}
		}
		StmALog fMessageSentLog;
		#endregion

		#region UserWhoQueuedThisRecord
		public GlbStaff UserWhoQueuedThisRecord
		{
			get
			{
				if (fUserWhoQueuedThisRecord == null)
				{
					fUserWhoQueuedThisRecord = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, EM_SystemCreateUser);
				}
				return fUserWhoQueuedThisRecord;
			}
		}
		GlbStaff fUserWhoQueuedThisRecord;
		#endregion

		public GlbCompany Company
		{
			get { return Branch?.Company; }
		}

		#endregion

		#region Constants Used for EM_Status, EM_ApplicationCode and EM_Direction
		#region Status
		public class Status : EDIMessageStatusList.Codes
		{
		}
		#endregion

		#region ApplicationCodes
		public class ApplicationCodes : EDIInterchange.ApplicationCodes
		{
		}
		#endregion

		#region Direction
		public sealed class Direction : EDIInterchange.Direction
		{
		}
		#endregion
		#endregion

		#region New and Overridden Properties

		public virtual string CollationKey
		{
			get { return EM_MessageOwner; }
		}

		public ZString ApplicationCodeWithDescription
		{
			get { return Lookups.ApplicationList.GetWithDescription(EM_ApplicationCode); }
		}

		public virtual bool UsesPlaceHolders
		{
			get
			{
				return !(EM_ApplicationCode == ApplicationCodeList.Codes.UniversalDataQuery
					|| EM_ApplicationCode == ApplicationCodeList.Codes.UniversalDataMessaging
					|| EM_ApplicationCode == ApplicationCodeList.Codes.NativeDataMessaging);
			}
		}

		#region EM_LinkedObject
		public BusinessObject EM_LinkedObject
		{
			get
			{
				if (fEM_LinkedObject == null)
				{
					if (RegisteredLinkedObjectTypes.TryGetValue(EM_LinkTable, out var typeFunc))
					{
						var bizoType = typeFunc.Invoke();
						fEM_LinkedObject = Factory.Load(bizoType, EM_LinkUniqueID);
						AfterNewObjectIsLinked(fEM_LinkedObject);
					}
				}
				return fEM_LinkedObject;
			}
			set
			{
				if (value != null)
				{
					var linkableValue = (ILinkable)value;  // Business object vs Business object wrapper
					EM_LinkTable = linkableValue.LinkTableName;
					EM_LinkUniqueID = linkableValue.LinkPK;
				}
				else
				{
					EM_LinkTable = null;
					EM_LinkUniqueID = ZGuid.Empty;
				}
				fEM_LinkedObject = value;
				AfterNewObjectIsLinked(value);
			}
		}
		BusinessObject fEM_LinkedObject;

		protected virtual void AfterNewObjectIsLinked(BusinessObject newBizObj)
		{
		}
		#endregion

		#region HumanReadableMessage
		public ZString HumanReadableMessage
		{
			get
			{
				return EM_MessageText.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }), "'")
					.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }), "+")
					.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }), ":")
					.Replace("'", "'\r\n");
			}
		}

		public ZPropertyInfo HumanReadableMessageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.HumanReadableMessage); }
		}
		#endregion

		#region EM_FormattedMessageText
		[ResourceStringData("F4C2BB53-68A0-4B25-8B03-6E49CD4B5096", Caption = "Message Text")]
		public virtual ZString EM_FormattedMessageText
		{
			get
			{
				return EM_FormattedMessageTextReader.ReadToEnd();
			}
		}

		public ZPropertyInfo EM_FormattedMessageTextInfo
		{
			get { return GetZPropertyInfo(Schema.EM_FormattedMessageText); }
		}

		public TextReader EM_FormattedMessageTextReader
		{
			get
			{
				if (formattedMessageStream == null)
				{
					formattedMessageStream = CreateFormattedMessageStream();
				}

				try
				{
					formattedMessageStream.Position = 0;
				}
				catch (ObjectDisposedException)
				{
					formattedMessageStream = CreateFormattedMessageStream();
				}

				return new StreamReader(formattedMessageStream);
			}
		}

		Stream formattedMessageStream;

		Stream CreateFormattedMessageStream()
		{
			Stream formattedStream = GetMessageStream();

			if (MessageStreamFormatter != null && EM_MessageType != EDIMessageTypeList.Codes.XDC && EM_MessageType != EDIMessageTypeList.Codes.XMS)
			{
				MessageStreamFormatter.FormatStream(ref formattedStream);
			}

			formattedStream.Position = 0;

			return formattedStream;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Disposal pattern for failed setup of returned disposable.")]
		protected Stream GetMessageStream()
		{
			// TODO: Consumers should be processing the Database stream direct, rather than creating a copy here.
			// However, fixing the usages of this will be much work.
			Stream result = null;
			try
			{
				result = new VirtualMemoryStream(LargeMessageHelper.BufferSize);

				using (TextReader reader = GetEM_MessageTextReader())
				{
					var writer = new StreamWriter(result);
					writer.AddStream(reader);
				}
				result.Position = 0;
			}
			catch
			{
				try
				{
					result.Dispose();
				}
				catch { }
				throw;
			}

			return result;
		}

		protected virtual IStreamFormatter MessageStreamFormatter
		{
			get
			{
				return IsUniversalDataMessagingApplicationCode() ? new EDIMessageStreamFormatterForXml() : new EDIMessageStreamFormatter();
			}
		}

		#endregion

		#region EM_InterchangeNumber
		public ZString EM_InterchangeNumber
		{
			get { return Interchange != null ? Interchange.EI_InterchangeNum : ZString.Empty; }
		}

		public ZPropertyInfo EM_InterchangeNumberInfo
		{
			get { return GetZPropertyInfo(Schema.EM_InterchangeNumber); }
		}
		#endregion

		#region EM_InterchangeStatus
		public ZString EM_InterchangeStatus
		{
			get { return Interchange != null ? Interchange.EI_Status : ZString.Empty; }
		}

		public ZPropertyInfo EM_InterchangeStatusInfo
		{
			get { return GetZPropertyInfo(Schema.EM_InterchangeStatus); }
		}
		#endregion

		#region EM_DateTimeInterchangeSent

		public
#if DEBUG
 virtual
#endif
 ZDateTime EM_DateTimeInterchangeSent
		{
			get
			{
				if (fEM_DateTimeInterchangeSent.IsEmpty)
				{
					ZDateTime? interchangeSentUtc = null;
					EDIInterchange interchange = this.Interchange;

					if (interchange != null)
					{
						if (interchange.EI_ReceiveTransmit == EDIInterchange.Direction.Receive)
						{
							StmALog deliveredLog = interchange.Logs.MostRecentLogByEventTime(Events.Delivered);
							if (deliveredLog != null)
							{
								interchangeSentUtc = deliveredLog.SL_PostedTimeUtc;
							}
						}

						if (interchangeSentUtc == null)
						{
							interchangeSentUtc = interchange.EI_SystemCreateTimeUtc;
						}
					}

					if (interchangeSentUtc == null)
					{
						if (EM_ReceiveTransmit == Direction.Receive)
						{
							interchangeSentUtc = EM_SystemCreateTimeUtc;
						}
						else
						{
							var readyLog = StmALogEntryLocator.Instance.GetLastPostEventOfType(this, AutoEvents.InterchangeReady); // outgoing
							if (readyLog != null)
							{
								interchangeSentUtc = readyLog.SL_PostedTimeUtc;
							}
						}
					}

					if (interchangeSentUtc != null && !interchangeSentUtc.Value.IsEmpty)
					{
						fEM_DateTimeInterchangeSent = Env.Time.GetLocalTimeFromUtc(interchangeSentUtc.Value.ToDateTime());
					}
				}

				return fEM_DateTimeInterchangeSent;
			}
		}
		ZDateTime fEM_DateTimeInterchangeSent;

		public ZPropertyInfo EM_DateTimeInterchangeSentInfo
		{
			get { return GetZPropertyInfo(Schema.EM_DateTimeInterchangeSent); }
		}

		#endregion

		#region EM_MessageDateTime
		public ZDateTime EM_MessageDateTime
		{
			get
			{
				if (eM_MessageDateTime.IsEmpty && !EM_SystemCreateTimeUtc.IsEmpty)
				{
					eM_MessageDateTime = Env.Time.GetLocalTimeFromUtc(EM_SystemCreateTimeUtc.ToDateTime());
				}
				return eM_MessageDateTime;
			}
		}
		ZDateTime eM_MessageDateTime;

		// Needed for right-clicking on messages in grids to Export To Excel, otherwise it barfs
		public ZPropertyInfo EM_MessageDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.EM_MessageDateTime); }
		}

#if DEBUG
		public void ResetMessageDateTimeForTesting()
		{
			eM_MessageDateTime = ZDateTime.Empty;
		}
#endif
		#endregion

		#region EM_User

		public ZString EM_User
		{
			get
			{
				if (IsTransmitMessage && UserWhoQueuedThisRecord != null)
				{
					return UserWhoQueuedThisRecord.GS_FullName;
				}
				return Interchange != null ? Interchange.EI_From : ZString.Empty;
			}
		}

		public ZPropertyInfo EM_UserInfo
		{
			get { return GetZPropertyInfo(Schema.EM_User); }
		}
		#endregion

		#region EM_SendingUser

		public ZString EM_SendingUser
		{
			get
			{
				return IsTransmitMessage && UserWhoQueuedThisRecord != null ? UserWhoQueuedThisRecord.GS_FullName : ZString.Empty;
			}
		}

		#endregion

		#region EM_CreateUserFullName
		public ZString EM_CreateUserFullName
		{
			get
			{
				return UserWhoQueuedThisRecord != null ? UserWhoQueuedThisRecord.GS_FullName : ZString.Empty;
			}
		}
		#endregion

		#region EM_InterchangeSender

		public ZString EM_InterchangeSender
		{
			get
			{
				return Interchange != null ? Interchange.EI_From : ZString.Empty;
			}
		}

		#endregion

		#region EM_InterchangeReceiver

		public ZString EM_InterchangeReceiver
		{
			get
			{
				return Interchange != null ? Interchange.EI_To : ZString.Empty;
			}
		}

		#endregion

		#region EM_MessageDescriptionFromSubType
		public ZString EM_MessageDescriptionFromSubType
		{
			get
			{
				ZString result;
				switch (EM_MessageSubType)
				{
					case "XSM":
					case "SSM":
						result = Res.GetString("2bc1e264-80ab-456e-90a4-83e40150042c", "Sent");
						break;
					case "XCN":
					case "SCN":
						result = Res.GetString("ae05efee-9789-4306-a577-4e531f1b9a49", "Canceled");
						break;
					case "ACK":
						result = Res.GetString("a36329d0-1a07-4908-b383-0355ab8b9908", "Accepted");
						break;
					case "REJ":
						result = Res.GetString("16ea2401-4b20-41af-bf4e-00b5ffa827c3", "Rejected");
						break;
					default:
						result = "";
						break;
				}
				return result;
			}
		}

		public ZPropertyInfo EM_MessageDescriptionFromSubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.EM_MessageDescriptionFromSubType); }
		}
		#endregion

		#region EM_SendOrReceiveHumanReadable
		public ZString EM_SendOrReceiveHumanReadable
		{
			get
			{
				switch (EM_ReceiveTransmit)
				{
					case ReceiveTransmitList.Codes.Internal:
						return ReceiveTransmitList.Descriptions.Internal;
					case ReceiveTransmitList.Codes.Transmit:
						return ReceiveTransmitList.Descriptions.Transmit;
					default:
						return ReceiveTransmitList.Descriptions.Receive;
				}
			}
		}

		public ZPropertyInfo EM_SendOrReceiveHumanReadableInfo
		{
			get { return GetZPropertyInfo(Schema.EM_SendOrReceiveHumanReadable); }
		}
		#endregion

		public ZString MessageTypeWithDescription
		{
			get { return Lookups.MessageTypeList.GetWithDescription(EM_MessageType); }
		}

		#region EM_MessageSubTypeDescription
		public virtual ZString EM_MessageSubTypeDescription
		{
			get
			{
				string result = MessageSubTypeList.GetDescriptionFromCode(EM_MessageSubType);

				if (string.IsNullOrEmpty(result))
				{
					result = EM_MessageSubType;
				}
				return result;
			}
		}

		public ZString MessageSubTypeWithDescription
		{
			get
			{
				var descriptionFallingBackToCodeIfEmpty = EM_MessageSubTypeDescription;
				if (!descriptionFallingBackToCodeIfEmpty.IsEmpty)
				{
					var code = EM_MessageSubType;
					if (descriptionFallingBackToCodeIfEmpty != code)
					{
						return string.Format("{0} - {1}", code, descriptionFallingBackToCodeIfEmpty);
					}
				}

				return descriptionFallingBackToCodeIfEmpty;
			}
		}

		public ZPropertyInfo EM_MessageSubTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EM_MessageSubTypeDescription); }
		}
		#endregion

		#region EM_MessageInterpretation
		[ReadOnly(true)]
		public virtual ZString EM_MessageInterpretation
		{
			get
			{
				var messageText = EM_MessageText;
				var result = MessageInterpretationNoteManager.Value;
				if (result.IsEmpty && IsUniversalDataMessagingApplicationCode())
				{
					var html = ConvertXmlToHtml(messageText);
					if (!html.IsEmpty && html != messageText && html.Length <= EM_MessageInterpretation_MaxLength)
					{
						MessageInterpretationNoteManager.Value = html;
						result = html;
					}
				}
				return result.IsEmpty ? messageText : result;
			}
			set
			{
				MessageInterpretationNoteManager.Value = value;
				EM_MessageInterpretationInfo.RefreshBinding();
			}
		}

#if DEBUG
		public bool HasTriedToConvertXmlToHtmlForTesting { get; protected set; }
#endif

		bool IsUniversalDataMessagingApplicationCode()
		{
			return EM_ApplicationCode == ApplicationCodeList.Codes.UniversalDataMessaging;
		}

		public ZString ConvertXmlToHtml(string xmlToDisplay)
		{
			var valueToSet = xmlToDisplay;  // Use the source verbatim if we have a problem parsing or converting it
			try
			{
#if DEBUG
				HasTriedToConvertXmlToHtmlForTesting = true;
#endif
				using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Messaging.Business.EDIMessage.XmlToHtmlTransformation.txt"))
				{
					using (var xr = XmlReader.Create(s))
					{
						var xct = new XslCompiledTransform();
						xct.Load(xr);
						var sb = new StringBuilder();
						using (var writer = XmlWriter.Create(sb, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment, Indent = true }))
						{
							using (var sr = new StringReader(xmlToDisplay))
							{
								using (var reader = XmlReader.Create(sr))
								{
									reader.MoveToContent();
									xct.Transform(reader, writer);
									valueToSet = sb.ToString();
								}
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }
			catch (OutOfMemoryException) // Well.. what else shall we do?!
			{ }
			return valueToSet;
		}

		public ZPropertyInfo EM_MessageInterpretationInfo
		{
			get { return GetZPropertyInfo(Schema.EM_MessageInterpretation); }
		}

		public int EM_MessageInterpretation_MaxLength
		{
			get { return PredefinedNoteTypes.Instance.MessageInterpretation.TextOnlyMaxLength; }
		}

		public virtual ZString InterchangeEHubID
		{
			get
			{
				return Interchange?.eHubID ?? ZString.Empty;
			}
		}

		protected ProxiedNotePropertyManager MessageInterpretationNoteManager
		{
			get { return messageInterpretationNoteManager ?? (messageInterpretationNoteManager = new ProxiedNotePropertyManager(this, PredefinedNoteTypes.Instance.MessageInterpretation)); }
		}
		ProxiedNotePropertyManager messageInterpretationNoteManager;

		public virtual bool ShouldShowInterpretation
		{
			get { return true; }
		}

		#endregion

		#region EM_MessageNText

		/// <summary>
		/// Don't use EM_MessageNText directly, use EM_MessageText as it has fallback logic for various locations of the data.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never), BusinessObjectTestExclude]
		public override ZString EM_MessageNText
		{
			get
			{
				if (!UsingNTextInternally)
				{
					ErrorReporter.ReportOnce("Don't use EM_MessageNText", "Don't use EM_MessageNText directly, use EM_MessageText as it has fallback logic for various locations of the data.");
				}

				return base.EM_MessageNText;
			}
			set
			{
				if (!UsingNTextInternally)
				{
					ErrorReporter.ReportOnce("Don't use EM_MessageNText", "Don't use EM_MessageNText directly, use EM_MessageText as it has fallback logic for various locations of the data.");
				}

				base.EM_MessageNText = value;
			}
		}

		bool UsingNTextInternally;

		IDisposable AllowNTextUsage()
		{
			return new DisposableAction(() => { UsingNTextInternally = true; }, () => { UsingNTextInternally = false; });
		}

#if DEBUG
		public bool ForceDeprecatedNTextUsageForTesting
		{
			get { return UsingNTextInternally; }
			set { UsingNTextInternally = value; }
		}
#endif

		#endregion

		/// <summary>
		/// This Property can produce memory issue with large file. Use it once per method.
		/// </summary>
		public override ZString EM_MessageText
		{
			get
			{
				using (AllowNTextUsage())
				{
					if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
					{
						if (HasMessageNText)
						{
							return base.EM_MessageNText;
						}
						else if (HasMessageText)
						{
							return base.EM_MessageText;
						}
						else
						{
							return EM_MessageDataAsText;
						}
					}
					else
					{
						return base.EM_MessageText;
					}
				}
			}
			set
			{
				ZString oldValue = EM_MessageText;

				if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
				{
					EM_MessageDataAsText = value;
					if (HasMessageNText)
					{
						using (AllowNTextUsage())
						{
							base.EM_MessageNText = ZString.Empty;
						}
					}
					if (HasMessageText)
					{
						base.EM_MessageText = ZString.Empty;
					}
				}
				else
				{
					base.EM_MessageText = value;
				}

				if (!IsCopying && oldValue != EM_MessageText)
				{
					UpdateMessageRelatedCachedFields();
					messageTextChangeStackTrace = new StackTrace().ToString();
				}

				ReportIfMessageTextIsEmpty(!Globals.IsTest);
			}
		}

		internal ZString EM_MessageDataAsText
		{
			get { return MessageEncoding.UTF8WithoutBOM.GetString(EM_MessageData); }
			set { EM_MessageData = MessageEncoding.UTF8WithoutBOM.GetBytes(value); }
		}

		[ResourceStringData("ADC4BA88-476B-4E50-8805-0575BFCCA6F7", Caption = "Message Text")]
		public virtual ZString EM_MessageTextIndentedXml
		{
			get
			{
				if (emMessageTextIndentedXml == null)
				{
					emMessageTextIndentedXml = TryIndentXml(GetEM_MessageTextReader()) ?? EM_MessageText;
				}

				return emMessageTextIndentedXml;
			}
		}
		string emMessageTextIndentedXml;

		static string TryIndentXml(TextReader xmlToFormatReader)
		{
			string indentedXml;

			try
			{
				var xmlReader = new XmlTextReader(xmlToFormatReader);

				xmlReader.Read();
				bool omitXmlDeclaration = xmlReader.NodeType != XmlNodeType.XmlDeclaration;

				var textWriter = new StringWriter();
				var xmlWriterSettings = new XmlWriterSettings
				{
					Indent = true,
					OmitXmlDeclaration = omitXmlDeclaration
				};
				var xmlWriter = XmlWriter.Create(textWriter, xmlWriterSettings);

				while (!xmlReader.EOF)
				{
					xmlWriter.WriteNode(xmlReader, false);
				}

				xmlWriter.Close();
				indentedXml = textWriter.ToString();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				indentedXml = null;
			}
			finally
			{
				xmlToFormatReader.Close();
			}

			return indentedXml;
		}

		bool HasMessageText
		{
			get
			{
				using (var reader = base.GetEM_MessageTextReader(true))
				{
					return reader.GetString(1).Length > 0;
				}
			}
		}

		bool HasMessageNText
		{
			get
			{
				using (var reader = base.GetEM_MessageNTextReader(true))
				{
					return reader.GetString(1).Length > 0;
				}
			}
		}

		internal bool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed
		{
			get
			{
				var result = ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() || IsXMLMessage || (this.Interchange != null && this.Interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				return result && SystemDataRegistry.Instance.EMMessageDataActive.Value;
			}
		}

		protected virtual ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
		{
			return false;
		}

		public void SetEM_MessageTextOrDataSource(Stream source)
		{
			if (source is SubStreamableStream subStreamableStream)
			{
				subStreamableStream.OnDispose += (s, e) =>
				{
					var row = ((INeedRow)this).Row;
					if ((row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified) && row[EDIMessageSchema.Constants.EM_MessageData] is byte[] blob && LazyLoading.LoadRequired(blob))
					{
						disposeStackTrace = new StackTrace().ToString();
					}
				};
			}

			if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
			{
				SetEM_MessageDataSource(new StreamSource(source));
			}
			else
			{
				SetEM_MessageTextSource(new TextReaderSource(source));
			}
		}

		string creationStackTrace = string.Empty;
		string disposeStackTrace;
		string messageTextChangeStackTrace;

		public bool IsXMLMessage
		{
			get
			{
				var applicationCodes = new HashSet<string>(new[] { ApplicationCodeList.Codes.XMS,
					ApplicationCodeList.Codes.UniversalDataMessaging,
					ApplicationCodeList.Codes.UniversalDataQuery,
					ApplicationCodeList.Codes.NativeDataMessaging,
					ApplicationCodeList.Codes.NativeDataQuery,
					ApplicationCodeList.Codes.SYS,
					ApplicationCodeList.Codes.CustomsWare,
					ApplicationCodeList.Codes.DECustomsAtlasSystem,
					ApplicationCodeList.Codes.DECustomsAesSystem,
					ApplicationCodeList.Codes.DECustomsEmcsSystem,
					ApplicationCodeList.Codes.AsycudaIncoming_GMD_Message,
					ApplicationCodeList.Codes.CNCustomsSingleWindow,
					ApplicationCodeList.Codes.GbCustomsEMCS,
					ApplicationCodeList.Codes.AirCargoAdvanceScreening,
				}
				);

				if (applicationCodes.Contains(EM_ApplicationCode))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public ZString MessageStatusWithDescription
		{
			get { return Lookups.StatusList.GetWithDescription(EM_Status); }
		}

		public bool IsMessageTextTooLong
		{
			get
			{
				using (var reader = GetEM_MessageTextReader(false))
				{
					return reader.GetString(LargeMessageHelper.DetailTextSizeLimit + 1).Length > LargeMessageHelper.DetailTextSizeLimit;
				}
			}
		}

		#region EM_MessageText Related

		public ZString EM_MessageTextShort
		{
			get
			{
				using (var reader = GetEM_MessageTextReader(false))
				{
					return reader.GetString(LargeMessageHelper.ShortTextSizeLimit);
				}
			}
		}

		public virtual ZString EM_MessageTextDetail
		{
			get
			{
				if (messageTextDetail == null)
				{
					using (TextReader reader = EM_FormattedMessageTextReader)
					{
						messageTextDetail = reader.GetString(LargeMessageHelper.DetailTextSizeLimit);
					}
				}
				return messageTextDetail;
			}
		}
		string messageTextDetail;

		#region EM_GC

		[List("Lookups.Companies")]
		public ZGuid EM_GC => Company?.PK ?? ZGuid.Empty;

		#endregion

		#region EM_MessageStream

		public override TextReader GetEM_MessageTextReader(bool closeReaderBetweenReads = false)
		{
			if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
			{
				if (HasMessageNText)
				{
					return base.GetEM_MessageNTextReader(closeReaderBetweenReads);
				}
				else if (HasMessageText)
				{
					return base.GetEM_MessageTextReader(closeReaderBetweenReads);
				}
				else
				{
					if (closeReaderBetweenReads)
					{
						ErrorReporter.ReportOnce("GetEM_MessageTextReader_NoCloseReaderBetweenReadsOnStreams", "Streamed reading from binary columns does not support 'closeReaderBetweenReads'. Cannot use this for Application Code types now stored in EM_MessageData.");
					}
					return new StreamReader(base.GetEM_MessageDataReader(), MessageEncoding.UTF8WithoutBOM);
				}
			}
			else
			{
				return base.GetEM_MessageTextReader(closeReaderBetweenReads);
			}
		}

		public override void SetEM_MessageTextSource(ITextReaderSource source)
		{
			if (ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed)
			{
				if (HasMessageNText)
				{
					base.EM_MessageNText = "";
				}
				if (HasMessageText)
				{
					base.EM_MessageText = "";
				}
				base.SetEM_MessageDataSource(new TextReaderStreamSource(source));
			}
			else
			{
				base.SetEM_MessageTextSource(source);
			}
		}

		/// <summary>
		/// Don't use GetEM_MessageNTextReader directly, use GetEM_MessageTextReader as it has fallback logic for various locations of the data.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never), BusinessObjectTestExclude]
		public override TextReader GetEM_MessageNTextReader(bool closeReaderBetweenReads = false)
		{
			ErrorReporter.ReportOnce("Don't use GetEM_MessageNTextReader", "Don't use GetEM_MessageNTextReader directly, use GetEM_MessageTextReader as it has fallback logic for various locations of the data.");
			return base.GetEM_MessageNTextReader(closeReaderBetweenReads);
		}

		/// <summary>
		/// Don't use SetEM_MessageNTextSource directly, use SetEM_MessageTextSource as it has fallback logic for various locations of the data.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never), BusinessObjectTestExclude]
		public override void SetEM_MessageNTextSource(ITextReaderSource source)
		{
			ErrorReporter.ReportOnce("Don't use SetEM_MessageNTextSource", "Don't use SetEM_MessageNTextSource directly, use SetEM_MessageTextSource as it has fallback logic for various locations of the data.");
			base.SetEM_MessageNTextSource(source);
		}

		#endregion

		#endregion

		public bool IsTransmitMessage
		{
			get { return EM_ReceiveTransmit == Direction.Transmit; }
			set { EM_ReceiveTransmit = value ? Direction.Transmit : Direction.Receive; }
		}

		public ZBool IsAConfirmingEXDMessage
		{
			get
			{
				if (EM_ApplicationCode == "CMR" && EM_MessageType == "EXD")
				{
					try
					{
						object message = new EdifactD99BMessageFactory().GetMessage(new UNOCCMRCharacterSet(), EM_MessageText);
						if (message != null && message.GetType() == typeof(CUSDECMessage))
						{
							CUSDECMessage myCUSDECMessage = (CUSDECMessage)message;
							foreach (GISSegment gIS in myCUSDECMessage.GIS)
							{
								if (gIS.ProcessingIndicator_X.CodeListIdentificationCode == CodeListIdentificationCodeList.FunctionalGroup)
								{
									if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == "Y")
									{
										return true;
									}
								}
							}
						}
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						ErrorReporter.ReportOnce("EDIMessage.IsAConfirmingEXDMessage", "An error occured trying to discover if an EXD message is confirming or not.  MessagePK = " + PK + ", Message Text = " + EM_MessageText + " Exception = " + exception.Message, exception);
					}
				}
				return false;
			}
		}

		public bool IsAnOriginal
		{
			get { return EM_MessageSubType == "ORG" || EM_MessageSubType == "ENT"; }
		}

		/// <summary>
		/// Check EM_Status. It is not if this message has been responded or not.
		/// </summary>
		public bool IsPending
		{
			get { return EM_Status == EDIMessage.Status.Pending; }
		}

		public bool HasHadItsInterchangeAcknowledged
		{
			get
			{
				bool result = false;
				EDIInterchange interchange = this.Interchange;
				if (interchange != null)
				{
					foreach (EDIMessage message in interchange.InterchangeAcknowledgementMessages)
					{
						if (message.EM_MessageSubType == nameof(Core.Constants.AUCTLMessageSubType.ACK))
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public virtual bool AssumeMessageClearIfAcknowledgedAndNoResponse
		{
			get { return false; }
		}
		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d2b1227a-640f-4b6b-8f42-3d9018a37e9d", "EDI Message"); }
		}

		#endregion

		#region Replacement of Number Fountain Place Holders in Message Body with Real Reference Numbers
		#region Constants For Number Fountain Place Holders
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string ContainedChecksumPlaceHolder = "<<CONTAINED CHECKSUM PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string SendersReferencePlaceHolder = "<<SENDERS REFERENCE PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string SendersReferencePlaceHolderHtml = "&lt;&lt;SENDERS REFERENCE PLACE HOLDER&gt;&gt;";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string PrimeEntryNumberPlaceHolder = "<<PRIME ENTRY NUMBER PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string EntryNumberPlaceHolder = "<<ENTRY NUMBER PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Place Holder")]
		public const string EntryNumberPlaceHolderHtml = "&lt;&lt;ENTRY NUMBER PLACE HOLDER&gt;&gt;";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string MessageNumberPlaceHolder = "<<MSGNO PLACEHOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Place Holder")]
		public const string MessageNumberPlaceHolderHtml = "&lt;&lt;MSGNO PLACEHOLDER&gt;&gt;";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string MessageDateTimeCreatePlaceHolder = "<<MESSAGE DATE TIME CREATE PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string UniqueBatchNumberPlaceHolder = "<<UNIQUE BATCH NUMBER PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Place Holder")]
		public const string UniqueBatchNumberPlaceHolderHtml = "&lt;&lt;UNIQUE BATCH NUMBER PLACE HOLDER&gt;&gt;";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string ConsignmentReferenceNumberPlaceHolder = "<<CONSIGNMENT REFERENCE NUMBER PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string AgentReferencePlaceHolder = "<<AGENT REFERENCE PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string OwnerReferencePlaceHolder = "<<OWNER REFERENCE PLACE HOLDER>>";
		public const string SystemCommonAccessReferencePkPlaceholder = "<<SYSCAR>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string DocumentMessageVersionPlaceHolder = "<<DOCUMENT MESSAGE VERSION PLACE HOLDER>>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Place Holder")]
		public const string DocumentMessageVersionPlaceHolderHtml = "&lt;&lt;DOCUMENT MESSAGE VERSION PLACE HOLDER&gt;&gt;";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string FormKeyPlaceHolder = "<#FORMKEY#>";
		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			EDIMessageAfterOnSavingCorruptionCheck.HookupFactory(Factory);
			ReportIfMessageDataDisposed();

			isNewRecord = !IsInDatabase;
			if (IsTransmitMessage && isNewRecord)
			{
				HoldMessageTextIfAvailable();
				GetNumberFountainNumbersAndFillInPlaceHolders();
			}
			FireOnSavingEvent();

			if (!isNewRecord)
			{
				var emStatusInfo = EM_StatusInfo;
				if (emStatusInfo.HasChanges)
				{
					var originalValue = (ZString)emStatusInfo.OriginalValue;
					var currentValue = EM_Status;
					if (originalValue != currentValue)
					{
						Logs.AddNew(Events.StatusUpdated, EDIInterchange.GetUpdatedStatusEventParameters(originalValue, currentValue));
					}
				}
			}

			UpdateMessageRelatedCachedFields();
			ReportIfMessageTextIsEmpty(!Globals.IsTest);
			ReportIfIncorrectNumberIsAllocatedForAMS();
			ReportIfMessageSubTypeNotConfiguredInMessagePurgeSettings();
		}

		ZString holdMessageTextWithPlaceholders;
		void HoldMessageTextIfAvailable()
		{
			if (UsesPlaceHolders && string.IsNullOrEmpty(disposeStackTrace))
			{
				holdMessageTextWithPlaceholders = EM_MessageText;
			}
			else
			{
				holdMessageTextWithPlaceholders = ZString.Empty;
			}
		}

		void UpdateMessageRelatedCachedFields()
		{
			formattedMessageStream?.Dispose();
			formattedMessageStream = null;
			messageTextDetail = null;
		}

		public void FireOnSavingEvent()
		{
			if (Saving != null)
			{
				Saving(this);
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			UpdateMessageRelatedCachedFields();
		}

		protected virtual string MessageNumberPlaceHolderOverride => MessageNumberPlaceHolder;
		protected virtual string SendersReferencePlaceHolderOverride => SendersReferencePlaceHolder;
		protected virtual string UniqueBatchNumberPlaceHolderOverride => UniqueBatchNumberPlaceHolder;

		protected virtual void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			PopulateMessageNumber();

			if (UsesPlaceHolders)
			{
				var messageText = EM_MessageText;
				var isMessageTextChanged = false;

				if (messageText.IndexOf(SendersReferencePlaceHolderOverride, StringComparison.InvariantCulture) != -1)
				{
					messageText = messageText.Replace(SendersReferencePlaceHolderOverride, GetSendersReference());
					isMessageTextChanged = true;
				}

				if (messageText.IndexOf(AgentReferencePlaceHolder, StringComparison.InvariantCulture) != -1)
				{
					var agentsReference = GetAgentReference();
					if (agentsReference != null)
					{
						messageText = messageText.Replace(AgentReferencePlaceHolder, agentsReference);
						isMessageTextChanged = true;
					}
				}

				if (messageText.IndexOf(ContainedChecksumPlaceHolder, StringComparison.InvariantCulture) != -1)
				{
					messageText = messageText.Replace(ContainedChecksumPlaceHolder, GetContainedChecksum(messageText));
					isMessageTextChanged = true;
				}

				if (messageText.IndexOf(OwnerReferencePlaceHolder, StringComparison.InvariantCulture) != -1)
				{
					messageText = messageText.Replace(OwnerReferencePlaceHolder, GetOwnerReference());
					isMessageTextChanged = true;
				}

				if (messageText.IndexOf(EntryNumberPlaceHolder, StringComparison.InvariantCulture) != -1)
				{
					messageText = messageText.Replace(EntryNumberPlaceHolder, GetEntryNumber());
					isMessageTextChanged = true;
				}

				if (ShouldReplaceUniqueBatchNumber && messageText.IndexOf(UniqueBatchNumberPlaceHolderOverride, StringComparison.InvariantCulture) != -1)
				{
					messageText = messageText.Replace(UniqueBatchNumberPlaceHolderOverride, GetBatchNumber());
					isMessageTextChanged = true;
				}

				if (messageText.IndexOf(SystemCommonAccessReferencePkPlaceholder, StringComparison.InvariantCulture) != -1)
				{
					messageText = messageText.Replace(SystemCommonAccessReferencePkPlaceholder, GetPkForCommonAccessReference());
					isMessageTextChanged = true;
				}

				if (messageText.IndexOf(DocumentMessageVersionPlaceHolder, StringComparison.InvariantCulture) != -1)
				{
					messageText = messageText.Replace(DocumentMessageVersionPlaceHolder, GetDocumentMessageVersion());
					isMessageTextChanged = true;
				}

				if (isMessageTextChanged)
				{
					EM_MessageText = messageText;
				}
			}
		}

		void ReportIfMessageDataDisposed()
		{
			if (!string.IsNullOrEmpty(creationStackTrace) && !string.IsNullOrEmpty(disposeStackTrace))
			{
				var message = FormattableString.Invariant($@"Cannot access a disposed object.

EDI Message Creation Stack Trace:
{creationStackTrace}

Dispose Stack Trace:
{disposeStackTrace}");
				ErrorReporter.ReportOnce(message);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "for ErrorReporter message, Exception text")]
		internal void ReportIfInvalidMessageData()
		{
			if ((EM_ApplicationCode == EDIMessage.ApplicationCodes.UniversalDataMessaging || EM_ApplicationCode == EDIMessage.ApplicationCodes.USeBond) && EM_ReceiveTransmit == EDIMessage.Direction.Transmit && EM_MessageNum.IsValid && EM_IsActive)
			{
				// We look for the invalid row rather than the valid row, since there is a slim chance this row already got deleted or something
				var isEmpty = 1 == Db.Connection.ExecuteScalar<int>("select isnull((select top 1 1 from (select * from dbo.EDIMessage where EM_MessageNum = @num and EM_PK = @pk) s where (EM_MessageData is null or EM_MessageData = 0x) and (EM_MessageText is null or EM_MessageText = '')), 0)", p =>
				{
					p.AddParameter("@pk", SqlDbType.UniqueIdentifier, PK.ToGuid());
					p.AddParameter("@num", SqlDbType.VarChar, EM_MessageNum.ToString());
				});

				if (!isEmpty)
				{
					return;
				}

				var key = string.Format(CultureInfo.InvariantCulture, "Invalid values in EDIMessage. EM_ApplicationCode: " + EM_ApplicationCode);
				var type = GetType().FullName;
				var message = FormattableString.Invariant($@"{EDIMessageSchema.EM_MessageData.Name} cannot be empty when {EDIMessageSchema.EM_ReceiveTransmit.Name} = {EM_ReceiveTransmit} and {EDIMessageSchema.EM_ApplicationCode.Name} = {EM_ApplicationCode}.
Type: {type}
Message Number: {EM_MessageNum}
Message Type: {EM_MessageType}
Message SubType: {EM_MessageSubType}
Branch: {Branch?.GB_Code}
Company: {Company?.GC_Code}
EM_MessageNText Length: {base.EM_MessageNText.Length}
Organisation: {EM_OrganisationCode}");

				ErrorReporter.ReportOnce(key, message);
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(message), ((INeedRow)this).Row, ((IDbConnected)Factory).Connection), Factory);
			}
		}

		protected virtual void ReportIfMessageTextIsEmpty(ZBool shouldReport)
		{
			if (shouldReport && ApplicationCodeListForReportIfMessageTextIsEmpty.Contains(EM_ApplicationCode) && IsTransmitMessage && EM_MessageText.IsEmpty)
			{
				var key = string.Format(CultureInfo.InvariantCulture, "Invalid message text in customs message. EM_ApplicationCode: " + EM_ApplicationCode);
				ErrorReporter.ReportOnce(key, $@"Message text cannot be empty when
Type: {this.GetType().FullName}
Message Number: {EM_MessageNum}
Message Type: {EM_MessageType}
Message Sub Type: {EM_MessageSubType}
Branch: {Branch?.GB_Code}
Company: {Company?.GC_Code}
Organisation: {EM_OrganisationCode}
Status: {EM_Status}
Original Message Text: {EM_MessageTextInfo.OriginalValue}
EM_MessageNText Length: {base.EM_MessageNText.Length}
EM_MessageText Length: {base.EM_MessageText.Length}
EM_MessageData Length: {EM_MessageData.Length}

Message Creation Stack Trace:
{creationStackTrace}

Message Text Change Stack Trace:
{messageTextChangeStackTrace}");
			}
		}

		List<string> ApplicationCodeListForReportIfMessageTextIsEmpty
		{
			get
			{
				return Factory.GetCachedValue("ApplicationCodeListForReportIfMessageTextIsEmpty", () =>
				{
					var result = new List<string>();
					result.Add(EDIMessage.ApplicationCodes.USCustomsImport);
					result.Add(EDIMessage.ApplicationCodes.CAIMP);
					result.Add(EDIMessage.ApplicationCodes.CMR);
					return result;
				});
			}
		}

		void ReportIfIncorrectNumberIsAllocatedForAMS()
		{
			if (EM_ApplicationCode == EDIMessage.ApplicationCodes.AMS && IsTransmitMessage && EM_LinkTable == CusInBondMoveHeaderSchema.Constants.TableName && EM_MessageNum.Length < 10)
			{
				ErrorReporter.ReportOnce("Incorrect number allocated for AMS message", $@"Incorrect number allocated for AMS message.

Type For Load: {TypeDecider.GetTypeForLoad(((IBusinessObjectInternals)this).Row, Factory)}

Message Creation Stack Trace:
{creationStackTrace}");
			}
		}

		void ReportIfMessageSubTypeNotConfiguredInMessagePurgeSettings()
		{
			if (EM_ApplicationCode == ApplicationCodeList.Codes.eNett || EM_ApplicationCode == ApplicationCodeList.Codes.GlobalElectronicInvoice || EM_ApplicationCode == ApplicationCodeList.Codes.GlobalElectronicPayment)
			{
				var configuredCodes = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes
					.Cast<ApplicationCodeObj>()
					.FirstOrDefault(x => x.ApplicationCode == EM_ApplicationCode)
					?.MessageTypes;

				if (!configuredCodes.Cast<MessageTypeObj>().Any(x => x.MessageSubType == EM_MessageSubType))
				{
					ErrorReporter.ReportDeveloperExceptionOnce("MessageTypeNotConfiguredInEHubMessagingRegistry", new DeveloperNotificationException(string.Format("Please add {0} to eHubMessagingRegistry where ApplicationCode is {1}", EM_MessageSubType, EM_ApplicationCode)));
				}
			}
		}

		protected virtual ZString GetPkForCommonAccessReference()
		{
			return new ZString(PK.ToString()).KeepAlphanumericCharacters().ToUpper();
		}

		public void AssignMessageNumber()
		{
			PopulateMessageNumber();
		}

		bool messageNumberPopulated;
		protected virtual void PopulateMessageNumber()
		{
			if (!IsInDatabase && !messageNumberPopulated)
			{
				EM_MessageNum = GetMessageReferenceNumber();
				messageNumberPopulated = true;
			}

			if (UsesPlaceHolders)
			{
				EM_MessageText = EM_MessageText.Replace(MessageNumberPlaceHolderOverride, EM_MessageNum);
			}
		}

		public override ZGuid EM_LinkUniqueID
		{
			get => base.EM_LinkUniqueID;
			set
			{
				if (value == ZGuid.Empty)
				{
					base.EM_LinkTable = ZString.Empty;
				}
				base.EM_LinkUniqueID = value;
				CheckCorrectUseOfLinkTable();
			}
		}

		public override ZGuid EM_EI
		{
			get => base.EM_EI;
			set
			{
				base.EM_EI = value;
				CheckCorrectUseOfLinkTable();
			}
		}

		void CheckCorrectUseOfLinkTable()
		{
			if (!this.EM_EI.IsEmpty && this.EM_LinkUniqueID == this.EM_EI)
			{
				var key = string.Format(CultureInfo.InvariantCulture, "EDIInterchange linked through EM_EI and EM_LinkUniqueID For Message Type {0}", EM_MessageType);
				var message = string.Format(CultureInfo.InvariantCulture, "EM_EI and EM_LinkUniqueID have the same GUID. Messages should be added to ContainedMessages. Acknowledgements should be added to InterchangeAcknowledgementMessages. Message Type: {0}, Diagnostics: {1}", EM_MessageType, DiagnosticDetails);
				ErrorReporter.ReportOnce(key, message);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!IsInDatabase && !saveSucceeded && Interchange == null)
			{
				OnNewRecordSavingFailed();
			}

			if (!saveSucceeded && !holdMessageTextWithPlaceholders.IsEmpty)
			{
				EM_MessageText = holdMessageTextWithPlaceholders;
			}
			holdMessageTextWithPlaceholders = ZString.Empty;

			FireOnSavedEvent(saveSucceeded);
		}

		bool isNewRecord;

		public delegate void SavedEventHandler(EDIMessage message, bool saveSucceeded);
		public event SavedEventHandler Saved;
		public event SavingEventHandler<EDIMessage> Saving;

		protected virtual void OnNewRecordSavingFailed()
		{
			if (ClearMessageNumberOnFailureToSave)
			{
				ClearMessageNumber();
			}
		}

		void ClearMessageNumber()
		{
			EM_MessageNum = ZString.Empty;
			messageNumberPopulated = false;
		}

		public bool ClearMessageNumberOnFailureToSave => ClearMessageNumberOnFailureToSaveCore;

		protected virtual bool ClearMessageNumberOnFailureToSaveCore => false;

		void FireOnSavedEvent(bool saveSucceeded)
		{
			if (Saved != null)
			{
				Saved(this, saveSucceeded);
			}
		}

		public string GetLocalTagName()
		{
			string localTagName = string.Empty;
			switch (EM_MessageSubType)
			{
				case EDIMessageSubTypeXMLElementList.Codes.CFSLoadList:
				case EDIMessageSubTypeXMLElementList.Codes.Brokerage:
					localTagName = EDIMessageSubTypeXMLElementList.Descriptions.Consols;
					break;
				case EDIMessageSubTypeXMLElementList.Codes.LocalCartageBooking:
				case EDIMessageSubTypeXMLElementList.Codes.LocalCartageStatus:
					localTagName = EDIMessageSubTypeXMLElementList.Descriptions.CartageJobs;
					break;
				default:
					var subTypeList = new EDIMessageSubTypeXMLElementList();
					localTagName = subTypeList[EM_MessageSubType, StringComparison.Ordinal].Description;
					break;
			}

			return localTagName;
		}

		public IMessageNumberStrategy MessageNumberStrategy;

		#region Overridable Methods to Get Real Reference Numbers
		protected virtual string GetMessageReferenceNumber()
		{
			if (MessageNumberStrategy != null)
			{
				return MessageNumberStrategy.GetMessageReferenceNumber();
			}
			else
			{
				var name = GetType().FullName;
				var message = FormattableString.Invariant($@"You must implement GetMessageReferenceNumber() in your implementation of EDIMessage.
Type: {name}
ApplicationCode: {EM_ApplicationCode}
ApplicationReference: {EM_ApplicationReference}
Status: {EM_Status}
Branch: {Branch?.GB_Code}
Company: {Company?.GC_Code}
Organisation: {EM_OrganisationCode}
Start of message: {EM_FormattedMessageText.SubstringSafe(0, 1024)}
");
				ErrorReporter.ReportOnce("Invalid Message Reference for " + name, message);
				throw new ApplicationException(message);
			}
		}

		protected virtual string GetSendersReference()
		{
			throw new ApplicationException("If you use a SendersReferencePlaceHolder, you must override GetSendersReference in your implementation of EDIMessage");
		}

		protected virtual string GetAgentReference()
		{
			throw new ApplicationException("If you use a AgentReferencePlaceHolder, you must override GetAgentReference in your implementation of EDIMessage");
		}

		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		protected virtual string GetContainedChecksum(string messageText)
		{
			throw new ApplicationException("If you use a ContainedChecksumPlaceHolder, you must override GetContainedChecksum in your implementation of EDIMessage");
		}

		protected virtual string GetOwnerReference()
		{
			throw new ApplicationException("If you use an OwnerReferencePlaceHolder, you must override GetOwnerReference in your implementation of EDIMessage");
		}

		protected virtual string GetEntryNumber()
		{
			throw new ApplicationException("If you use an EntryNumberPlaceHolder, you must override GetEntryNumber in your implementation of EDIMessage");
		}

		protected virtual bool ShouldReplaceUniqueBatchNumber
		{
			get { return true; }
		}

		protected virtual string GetBatchNumber()
		{
			throw new ApplicationException("If you use a UniqueBatchNumberPlaceHolder, you must override GetBatchNumber in your implementation of EDIMessage");
		}

		internal string GetBatchNumberForInterchange()
		{
			return GetBatchNumber();
		}

		protected virtual string GetDocumentMessageVersion()
		{
			throw new ApplicationException("If you use a DocumentMessageVersionPlaceHolder, you must override GetDocumentMessageVersion in your implementation of EDIMessage");
		}

		#endregion
		#endregion

		#region EDIFACT Message Object Management

		public Edifact.Auto.SegmentGroup GetAutoEdifactMessageUsingNamedFactory(MessageFactory factory)
		{
			return GetAutoEdifactMessageUsingNamedFactory(factory, this.CharacterSet);
		}

		public Edifact.Auto.SegmentGroup GetAutoEdifactMessageUsingNamedFactory(MessageFactory factory, UNCharacterSet characterSet)
		{
			return factory.GetMessage(characterSet, this.EM_MessageText);
		}

		public UNCharacterSet CharacterSet
		{
			get
			{
				UNCharacterSet characterSet;
				if (Interchange != null && !Interchange.EI_HeaderText.IsEmpty)
				{
					characterSet = Interchange.CharacterSet;
				}
				else
				{
					var messageText = EM_MessageText;
					UNOACharacterSet uNOA = new UNOACharacterSet();
					if (messageText.SubstringSafe(3, 1) == uNOA.ElementDelimiter)
					{
						characterSet = uNOA;
					}
					else
					{
						UNOBCharacterSet uNOB = new UNOBCharacterSet();
						if (messageText.SubstringSafe(3, 1) == uNOB.ElementDelimiter)
						{
							characterSet = uNOB;
						}
						else
						{
							UNOCCMRCharacterSet uNOC = new UNOCCMRCharacterSet();
							if (messageText.SubstringSafe(3, 1) == uNOC.ElementDelimiter)
							{
								characterSet = uNOC;
							}
							else
							{
								characterSet = null;
							}
						}
					}
				}
				if (characterSet != null)
				{
					characterSet.ReplaceEscapedCharactersWithSpace = false;
				}
				return characterSet;
			}
		}
		#endregion

		public ZString Report
		{
			get { return report ?? (report = GetReportCore()).Value; }
		}
		ZString? report;

		protected virtual ZString GetReportCore()
		{
			return ZString.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_IsActive = true;
			EM_GB = GlbBranch.CurrentBranch.PK;
			EM_GE = GlbDepartment.CurrentDepartment.PK;
			EM_ReceiveTransmit = Direction.Transmit;
			EM_Status = Status.Queued;
		}

		public virtual void ResetToQueuedStatus()
		{
			EDIInterchange interchange = null;
			var receiveTransmit = EM_ReceiveTransmit;
			if (receiveTransmit.Equals(Direction.Receive))
			{
				if (!IsXMLMessage)
				{
					EM_LinkUniqueID = ZGuid.Empty;
					EM_LinkTable = "";

					if (!ResetToQueuedStatusPreservesMessageType)
					{
						EM_MessageType = "";
					}

					if (!ResetToQueuedStatusPreservesMessageSubType)
					{
						EM_MessageSubType = "";
					}
				}
				EM_RetryCount = 0;
				EM_Status = Status.Queued;
			}
			else if (receiveTransmit.Equals(Direction.Transmit) && (interchange = Interchange) != null)
			{
				interchange.ResetToQueuedStatus();
				var interchangeStatus = interchange.EI_Status;
				if (interchangeStatus == EDIInterchange.Status.Queued
					|| interchangeStatus == EDIInterchange.Status.eHubQueued
					|| interchangeStatus == EDIInterchange.Status.eAdaptorQueued)
				{
					EM_Status = EDIMessage.Status.Sent;
				}
			}
			else
			{
				EM_RetryCount = 0;
				EM_Status = Status.Queued;
			}
		}

		protected virtual bool ResetToQueuedStatusPreservesMessageType => false;

		protected virtual bool ResetToQueuedStatusPreservesMessageSubType => false;

		public void CopyPersistentValuesFrom(EDIMessage sourceObject)
		{
			base.CopyPersistentValuesFrom(sourceObject);
		}

		protected virtual CodeDescriptionPairList MessageSubTypeList
		{
			get { return Factory.GetCachedValue<EDIMessageSubTypeList>(); }
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection fNoteTypes = base.NoteTypesCore;

				fNoteTypes.Add(PredefinedNoteTypes.Instance.MessageInterpretation);
				fNoteTypes.Add(PredefinedNoteTypes.Instance.DataImportLogNote);

				return fNoteTypes;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategy.EDIMessageFetchStrategy(this);
		}

		Dictionary<string, Func<Type>> GetRegisterLinkedObjectTypes()
		{
			var regTypes = new Dictionary<string, Func<Type>>
			{
				{ JobDeclarationSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.IBaseJobDeclaration>() },
				{ CusEntryHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.ICusEntryHeader>() },
				{ CusUnderbondSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.ICusUnderbond>() },
				{ CusReconDeclarationSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.IBaseCusReconDeclaration>() },
				{ QuarantineExDocHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.AU.IQuarantineExdocHeader>() },
				{ QuarantineColsHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.AU.IQuarantineColsHeader>() },
				{ CusStorageDocPivotSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.AU.ICusStorageDocPivot>() },
				{ CusOutturnHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.AU.ICusOutturnHeader>() },
				{ CusPermitHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.ICommonCusPermitHeader>() },
				{ CusTempStorageDecSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.EU.ICusTempStorageDec>() },
				{ CusExitReportSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.EUExitControl.ICusExitReport>() },
				{ CusInBondHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.EU.NCTS.ICusInBondHeader>() },
				{ CusInBondMoveHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.ICusInBondMoveHeader>() },
				{ AsycudaBillSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.ManifestBase.IAsycudaBill>() },
				{ AsycudaManifestHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.ManifestBase.IAsycudaManifestHeader>() },
				{ CusSCAContainerSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.Shared.IBaseCusSCAContainer>() },
				{ CusSCAHouseSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.Shared.IBaseCusSCAHouse>() },
				{ CusSCAOceanBillSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.Shared.IBaseCusSCAOceanBill>() },
				{ CusHAWBSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.Shared.ICusHAWB>() },
				{ CusMAWBSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.Shared.ICusMAWB>() },
				{ CusISFHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.US.ISF.ICusISFHeader>() },
#if DEBUG
				{ DummyBizoSchema.Constants.TableName, () => typeof(CargoWise.EntityFramework.Testing.DummyBusinessObject) },
#endif
				{ EDIInterchangeSchema.Constants.TableName, () => typeof(EDIInterchange) },
				{ JobContainerSchema.Constants.TableName, () => ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonContainer>() },
				{ ExportAWBHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Freight.Integration.AWB.IExportAWBHeader>() },
				{ JobConsolSchema.Constants.TableName, () => ObjectFactory.GetType<Forwarding.IForwardingConsol>() },
				{ GlbCompanySchema.Constants.TableName, () => ObjectFactory.GetType<IGlbCompany>() },
				{ CusMiscRequestHeaderSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.KR.ICusMiscRequestHeader>() },
				{ CusPollingTransactionSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.ICusPollingTransaction>() },
				{ GlbExternalPasswordSchema.Constants.TableName, () => ObjectFactory.GetType<IGlbExternalPassword>() },
				{ OrgRelatedPartySchema.Constants.TableName, () => typeof(OrgRelatedParty) },
				{ CusGoodsCatalogSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.ICusGoodsCatalog>() },
				{ HVLVConsignmentSchema.Constants.TableName, () => ObjectFactory.GetType<eTail.Integration.IHVLVConsignment>() },
				{ EDIMessageSchema.Constants.TableName, ObjectFactory.GetType<IEDIMessage> },
				{ JobShipmentSchema.Constants.TableName, () => ObjectFactory.GetType<Forwarding.IForwardingShipment>() },
				{ CusBRForeignOperatorSchema.Constants.TableName, () => ObjectFactory.GetType<Customs.BR.ICusBRForeignOperator>() },
				{ JobVoyageSchema.Constants.TableName, () => ObjectFactory.GetType<Enterprise.Integration.Freight.IJobVoyage>() },
				{ OrgHeaderSchema.Constants.TableName, () => typeof(OrgHeader) },
			};

			foreach (var t in GetAdditionalRegisteredLinkedObjectTypes())
			{
				var tbl = BusinessObjectFactory.GetTableNameFromType(t);

				regTypes[tbl] = () => t;
			}

			return regTypes;
		}
		protected Dictionary<string, Func<Type>> RegisteredLinkedObjectTypes => registeredLinkedObjectTypes ?? (registeredLinkedObjectTypes = GetRegisterLinkedObjectTypes());
		Dictionary<string, Func<Type>> registeredLinkedObjectTypes;

		protected virtual IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes() => new List<Type>();

		protected void RegisterTypeOrThrowAnExceptionIfItCantBeFound(string assemblyName, string typeName)
		{
			var myAssembly = Assembly.Load(assemblyName);
			if (myAssembly == null)
			{
#if DEBUG
				throw new ApplicationException("Couldn't find the assembly: '" + assemblyName + "'");
#endif
			}
			else
			{
				Type typeToAdd = myAssembly.GetType(typeName);
				if (typeToAdd == null)
				{
#if DEBUG
					throw new ApplicationException("Couldn't find the type: '" + typeName + "'  has a namespace been changed?");
#endif
				}
				else
				{
					var tbl = BusinessObjectFactory.GetTableNameFromType(typeToAdd);
					RegisteredLinkedObjectTypes[tbl] = () => typeToAdd;
				}
			}
		}

		[ChildEditable(true)]
		public EDIMessageAttachDependentCollection MessageAttachments
		{
			get
			{
				if (messageAttachments == null)
				{
					messageAttachments = new EDIMessageAttachDependentCollection(this, Factory);
					messageAttachments.Load();
					RegisterEditableChildObject(messageAttachments);
				}

				return messageAttachments;
			}
		}
		EDIMessageAttachDependentCollection messageAttachments;

		#region protected override void FillWithValidTestDataCore(TestBusinessObjectKind Kind, PropertyDescriptor[] PropertyPath)
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (!(this is HttpXmlEDIMessage))
			{
				// only customs EDI Messages require an update of EM_MessageText
				// this should be refactored to extract EM_MessageText update functionality to a
				// separate Customs - specific EDI Message class
				EM_MessageText = MessageNumberPlaceHolderOverride;
			}
			else
			{
				EM_MessageData = new ZBlob(new byte[] { 0x00, 0x01, 0xFF, 0x01 });
			}
			EM_ReceiveTransmit = EDIMessage.Status.Received;
		}
#endif
		#endregion

		#region eNett related properties

		public virtual ZString EM_Ledger
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_TransactionType
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_TransactionNumber
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_JobInvoiceNo
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_ChequeOrReference
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_InvoiceDate
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_PostDate
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_LocalInvoiceAmtInclTax
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_OSInvoiceAmtInclTax
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_Currency
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_OrganisationCode
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EM_OrganisationName
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = GetDocManagerInfoCore()); }
		}
		DocManagerInfo docManagerInfo;

		protected virtual DocManagerInfo GetDocManagerInfoCore() => new DocManagerInfo(this, Core.Constants.DocManagerCodes.EDIMessage);

		#endregion

		#region ISourceInfo Members

		ZString ISourceInfo.DataSource
		{
			get
			{
				return BillingDataSource.None.ToString();
			}
		}

		ZString ISourceInfo.InterfaceName
		{
			get { return "eServices"; } //to be replaced with real interface name from IeHubMessage
		}

		bool ISourceInfo.ShouldSuspendValidation
		{
			get { return true; }
		}

		ZGuid ISourceInfo.EDIMessagePK
		{
			get { return PK; }
		}

		ZGuid ISourceInfo.EDIInterchangePK
		{
			get { return Interchange != null ? Interchange.PK : ZGuid.Empty; }
		}

		ZString ISourceInfo.FileName
		{
			get
			{
				var fileNameNote = Notes.FindByDescription(Res.GetString("03edbd63-b839-4e9e-8960-bfbacb88c502", "File Name")).FirstOrDefault();
				if (fileNameNote != null)
				{
					return fileNameNote.ST_NoteDataAsText;
				}
				return ZString.Empty;
			}
		}

		ZString ISourceInfo.SenderId
		{
			get { return Interchange != null ? Interchange.EI_From : ZString.Empty; }
		}

		#endregion

		#region IEDIMessage Members

		IEDIInterchange IEDIMessage.Interchange
		{
			get { return Interchange; }
		}

		IBranch IEDIMessage.Branch
		{
			get { return Branch; }
		}

		TextReader IEDIMessage.GetEM_MessageTextReader()
		{
			return GetEM_MessageTextReader();
		}

		#endregion

		#region Test Helpers
#if DEBUG

		public int DataImportLogNoteCount
		{
			get
			{
				return Notes.GetAllNotes().Count(note => note is StmNote stmNote &&
					stmNote.ST_NoteType == PredefinedNoteTypes.Instance.DataImportLogNote.DefaultVisibility.ToString());
			}
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateString(ZPropertyInfo property)
			{
				if (!property.Name.Equals(Schema.EM_MessageNText))
				{
					base.PopulateString(property);
				}
			}
		}

		public void DeleteFromTest()
		{
			try
			{
				IsDeletingInTest = true;
				Delete();
			}
			finally
			{
				IsDeletingInTest = false;
			}
		}

		// Is public because is set/cleared by:
		// 1. Enterprise.Customs.ASYCUDA.Business.EDIMessageCollectionNonDependent
		// 2. Enterprise.Freight.Forwarding.AWB.Business.CIMEDIMessageDependentCollection
		public bool IsDeletingInTest { get; set; }
#endif
		#endregion

	}

	public static class CodeDescriptionPairListExtensions
	{
		public static ZString GetWithDescription(this CodeDescriptionPairList list, ZString code)
		{
			var description = list.GetDescriptionFromCode(code);
			if (string.IsNullOrEmpty(description) || description == code)
			{
				return code;
			}

			return string.Format("{0} - {1}", code, description);
		}
	}
}
