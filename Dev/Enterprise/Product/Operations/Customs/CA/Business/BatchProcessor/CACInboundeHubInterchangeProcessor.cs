namespace Enterprise.Customs.CA.Business.BatchProcessor
{
	using System.IO;
	using System.Threading;
	using System.Xml;
	using CargoWise.Customs.CA.MessageDefinitions.CAD.Outbound;
	using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMDN_AH;
	using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMDN_CB;
	using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMSOA;
	using CargoWise.Customs.Shared.MessageContracts;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Xml;

	public class CACInboundeHubInterchangeProcessor : InboundInterchangeProcessor, ICustomsServiceTaskProcess
	{
		public CACInboundeHubInterchangeProcessor()
			: base()
		{
		}

		public void ExecuteBatchForDebug()
		{
			ExecuteBatch(CancellationToken.None);
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { EDIInterchange.ApplicationCodes.CACustoms, EDIInterchange.ApplicationCodes.CAACI, EDIInterchange.ApplicationCodes.CAEXP, EDIInterchange.ApplicationCodes.CAIMP }; }
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			var bodyText = interchange.EI_BodyText;
			if (CanDeserialize<DocumentMetaData>(bodyText))
			{
				return new CADInboundMessageCreator<CADMessage>(Logger, EDIInterchange.ApplicationCodes.CAIMP, MessageTypeList.Codes.CommercialAccountingDeclaration);
			}
			else if (CanDeserialize<Zcarmdnoticeah>(bodyText))
			{
				return new CADInboundMessageCreator<CARMDailyNoticeMessage>(Logger, interchangeType: MessageTypeList.Codes.CARMDailyNotice, messageSubType: CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter);
			}
			else if (CanDeserialize<Zcarmdnoticecb>(bodyText))
			{
				return new CADInboundMessageCreator<CARMDailyNoticeMessage>(Logger, interchangeType: MessageTypeList.Codes.CARMDailyNotice, messageSubType: CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeBroker);
			}
			else if (CanDeserialize<Zcarmsoa>(bodyText))
			{
				return new CADInboundMessageCreator<CARMStatementOfAccountMessage>(Logger, interchangeType: MessageTypeList.Codes.CARMStatementOfAccount);
			}
			return new CAInboundMessageCreator(Logger);
		}

		public override bool IsInterchangeNotDeleted(EDIInterchange interchange)
		{
			return !interchange.IsDeleted && interchange.EI_Status != EDIInterchange.Status.Error;
		}

		protected override System.Type TypeOfInterchangeToCreate()
		{
			return typeof(CAEDIInterchange);
		}

		protected override bool IsNoBranchFilter
		{
			get { return true; }
		}

		static ZBool CanDeserialize<T>(ZString messageContent)
		{
			using (var stream = new StringReader(messageContent))
			{
				using (var reader = new XmlTextReader(stream))
				{
					var serializer = ZXmlSerializer.New(typeof(T));
					bool result;
					try
					{
						result = serializer.CanDeserialize(reader);
					}
					catch (XmlException)
					{
						result = false;
					}

					return result;
				}
			}
		}

		#region InboundMessageCreator Class

		class CAInboundMessageCreator : IInboundMessageCreator
		{
			public CAInboundMessageCreator(LoggingInformation logger)
			{
				this.logger = logger;
			}
			readonly LoggingInformation logger;

			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				logger.Log(string.Format("Processing interchange #{0}, To '{1}'", interchange.EI_InterchangeNum, interchange.EI_To));

				var interchangeText = interchange.EI_InterchangeText;

				if (interchangeText.Contains("CUSRES:D:96A:UN:UN'BGM+"))
				{
					logger.LogWarning("Repairing mal-formed headers. This is a setup issue at CBSA. Contact CBSA and request that the extra ':UN' be removed from release responses.");
					interchangeText = interchange.EI_HeaderText;
					interchange.EI_HeaderText = interchangeText.Replace("UN+D:96A:UN'", "UN+D:96A'").Replace("CUSRES:D:96A:UN:UN'BGM+", "CUSRES:D:96A:UN'BGM+");
					interchangeText = interchange.EI_BodyText;
					interchange.EI_BodyText = interchangeText.Replace("UN+D:96A:UN'", "UN+D:96A'").Replace("CUSRES:D:96A:UN:UN'BGM+", "CUSRES:D:96A:UN'BGM+");
				}
				ZString from = interchange.EI_From;
				ZString interchangeNum = interchange.EI_InterchangeNum;

				interchangeText = interchange.EI_InterchangeText;
				var characterSet = EDIInterchange.GetCharacterSetFromInterchangeString(interchangeText);
				var interchangeDetails = EDIInterchange.GetInterchangeDetailsFromString(interchange.Factory, interchangeText, BatchProcessorUtilities.GetApplicationCode(interchangeText), false, true, characterSet);
				interchange.EI_HeaderText = interchangeDetails.HeaderText;
				interchange.EI_BodyText = interchangeDetails.BodyText;
				interchange.EI_FooterText = interchangeDetails.FooterText;
				interchange.EI_From = interchangeDetails.From;
				interchange.EI_InterchangeNum = interchange.PreparationDateTime.ToString("yyyyMMddHHmm") + interchangeDetails.InterchangeNum.PadLeft(14, '0');
				interchange.EI_ApplicationCode = interchangeDetails.ApplicationCode;

				if (interchange.ExistingInterchangeMatchingToFromAndInterchangeNum != null)
				{
					logger.LogWarning(string.Format("Inbound interchange #{0} ignored, as it is a duplicate", interchange.EI_InterchangeNum));
					interchange.EI_Status = EDIInterchange.Status.Error;
					interchange.EI_From = from;
					interchange.EI_InterchangeNum = interchangeNum;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					interchange.Logs.AddNew(Events.EditedARecord, "DUPLICATE");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else
				{
					logger.DebugLog(string.Format("CA interchange #{0} added to DB", interchange.EI_InterchangeNum));
					interchange.EI_InterchangeType = BatchProcessorUtilities.GetMessageType(interchange.EI_InterchangeText);
					interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived(false);
					logger.DebugLog(string.Format("Messages spawned from interchange #{0}", interchange.EI_InterchangeNum));
				}
			}
		}

		class CADInboundMessageCreator<T> : IInboundMessageCreator where T : Business.EDIMessage
		{
			public CADInboundMessageCreator(LoggingInformation logger, string applicationCode = "", string interchangeType = "", string messageSubType = "")
			{
				this.logger = logger;
				this.applicationCode = applicationCode;
				this.interchangeType = interchangeType;
				this.messageSubType = messageSubType;
			}

			readonly LoggingInformation logger;
			readonly ZString applicationCode;
			readonly ZString interchangeType;
			readonly ZString messageSubType;

			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				var originInterchangeNum = interchange.EI_InterchangeNum;
				logger.Log(string.Format("Processing interchange #{0}, To '{1}'", originInterchangeNum, interchange.EI_To));

				var canCreateMessage = true;
				if (typeof(T) == typeof(CADMessage))
				{
					var documentMetaData = XmlObjectSerializer.Deserialize<DocumentMetaData>(interchange.EI_BodyText);
					if (documentMetaData.Response != null && documentMetaData.Response.IssueDateTime != null)
					{
						interchange.EI_InterchangeNum = documentMetaData.Response.IssueDateTime.DateTimeString + originInterchangeNum.PadLeft(14, '0');
						if (interchange.ExistingInterchangeMatchingToFromAndInterchangeNum != null)
						{
							logger.LogWarning(string.Format("Inbound interchange #{0} ignored, as it is a duplicate", interchange.EI_InterchangeNum));
							interchange.EI_Status = EDIInterchange.Status.Error;
							interchange.EI_InterchangeNum = originInterchangeNum;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							interchange.Logs.AddNew(Events.EditedARecord, "DUPLICATE");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							canCreateMessage = false;
						}
					}
				}
				if (canCreateMessage)
				{
					((CAEDIInterchange)interchange).CreateMessageFromInterchange(typeof(T), messageSubType);
					if (!applicationCode.IsEmpty)
					{
						interchange.EI_ApplicationCode = applicationCode;
					}
					if (!interchangeType.IsEmpty)
					{
						interchange.EI_InterchangeType = interchangeType;
					}
					logger.DebugLog($"{interchangeType} {messageSubType} message generated from #{interchange.EI_InterchangeNum}");
				}
			}
		}
	}

	#endregion
}
