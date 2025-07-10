using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using MLCAI = Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11
{
	public class McpRRA01RRA11AndRRA06ApplicationMessageProcessor : ApplicationTypeMessageProcessor
	{
		public McpRRA01RRA11AndRRA06ApplicationMessageProcessor(LoggingInformation logger, ILogger serviceLogger, IEDocsDelayedSaver eDocsSaver)
			: base(logger)
		{
			this.serviceLogger = serviceLogger;
			Logger.OnLogInfoAdded += Logger_OnLogInfoAdded;
			EDocsSaver = eDocsSaver;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			serviceLogger.Log(logType, log);
		}

		protected override string ApplicationCodeCore
		{
			get { return ApplicationCodeList.Codes.GbMcpRra01AndRra11; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return ApplicationCodeList.Descriptions.GbMcpRra01AndRra11; }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			string messageText = message.EM_MessageText;
			if (string.IsNullOrEmpty(messageText))
			{
				string errorMessage = string.Format(CultureInfo.CurrentCulture,
					"EdiMessage.EM_MessageText was empty. MessagePK: {0}, ApCode: {1}, AppReference: {2}, EM_EI: {3}",
					message.PK.ToGuid().ToString(),
					message.EM_ApplicationCode,
					message.EM_ApplicationReference,
					message.EM_EI.ToGuid().ToString());

				if (message.EM_EI != ZGuid.Empty)
				{
					errorMessage += System.Environment.NewLine +
									string.Format(CultureInfo.CurrentCulture,
										"PK: {0}, From: {1}, To: {2}, AppCode: {3}, Body: {4}, Header: {5}, Footer: {6}, IntNum: {7}",
										message.Interchange.PK.ToGuid().ToString(),
										message.Interchange.EI_From,
										message.Interchange.EI_To,
										message.Interchange.EI_ApplicationCode,
										message.Interchange.EI_BodyText,
										message.Interchange.EI_HeaderText,
										message.Interchange.EI_FooterText,
										message.Interchange.EI_InterchangeNum);
				}

				throw new ArgumentException(errorMessage);
			}

			RRA01AndRRA11Message rraMessage;
			if (messageText.StartsWith("=RRA06", System.StringComparison.OrdinalIgnoreCase))
			{
				rraMessage = new RRA06Message(messageText);  // load and understand the EDI message
			}
			else
			{
				rraMessage = new RRA01AndRRA11Message(messageText);  // load and understand the EDI message
			}

			rraMessage.SetUpInstanceOfMessageFromString();

			serviceLogger.Log(LogType.Information, "Processing RRA message: " + messageText);

			CusEntryHeader entryHeader = null;
			var holdAction = MatchOrHoldAction.Unknown;
			if (!rraMessage.CHIEFEntryNumber.IsEmpty && !rraMessage.CHIEFEntryDate.IsEmpty)
			{
				var messageEntryNumFormatted = rraMessage.EntryNumber;
				var messageEntryDateTime = rraMessage.ChopUpDateFromString(rraMessage.CHIEFEntryDate + rraMessage.CHIEFEntryTime);
				var entryHeaders = GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate(messageEntryNumFormatted, message.Factory, messageEntryDateTime);

				if (entryHeaders.Length == 1)
				{
					entryHeader = entryHeaders[0];
					holdAction = rraMessage.IsChiefEntryNumber ? MatchOrHoldAction.MatchedByEntryNumber : MatchOrHoldAction.MatchedByCdsEntryNumber;
				}
			}
			if (entryHeader == null)
			{
				entryHeader = rraMessage.GetEntryHeaderFromUcnUsingMucr(message);
				holdAction = MatchOrHoldAction.MatchedByMucr;
			}

			if (entryHeader != null)
			{
				// UCN in RRA message is that of a MUCR on an entry - probably not amalgamated
				message.EM_Status = EDIMessage.Status.Received;
				message.EM_LinkedObject = entryHeader;
				message.EM_GB = entryHeader.Branch.PK;
				ApplyOrRemoveHoldOrClear(entryHeader, rraMessage);
				serviceLogger.Log(LogType.Information, "Updated entry from " + rraMessage.MessageType + " - " + entryHeader.EntryNumber);
			}
			else
			{
				// The RRA01 or RRA11 may mention an individual UCN that's not recorded as the MUCR of an entry - find from those already mentioned in an RRA12
				var cusAddInfoForUcn = FindUcnCusAddInfoFromIndividualUcn(rraMessage.UCN, message.Factory);
				if (cusAddInfoForUcn != null)
				{
					entryHeader = message.Factory.Load<CusEntryHeader>(cusAddInfoForUcn.B7_ParentID);
					if (entryHeader != null)
					{
						entryHeader.MaritimeUcnsThatAreHeld.RemoveAndDelete(cusAddInfoForUcn);
						if (entryHeader.MaritimeUcnsThatAreHeld.Count == 0)
						{
							// That was the last hold lifted - update entry to CLEAR
							ApplyOrRemoveHoldOrClear(entryHeader, rraMessage); // RRA Message's implementation of IPortAuthorityHoldApplicationProvider is that it's always CLEAR
							serviceLogger.Log(LogType.Information, "Updated entry from " + rraMessage.MessageType + " message using UCN from prior RRA12 - " + entryHeader.EntryNumber);
							holdAction = MatchOrHoldAction.LastHoldRemoved;
						}
						else
						{
							// Still holds exist
							serviceLogger.Log(LogType.Information, "Updated entry from " + rraMessage.MessageType + " message using UCN from prior RRA12 but holds still exist - " + entryHeader.EntryNumber);
							holdAction = MatchOrHoldAction.HoldsStillExist;
							// To be released, though, we must have been customs cleared; record this fact
							if (entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length == 0)
							{
								entryHeader.Logs.AddNew(Events.CustomsCleared, "Cleared via " + rraMessage.MessageType, rraMessage.DateOfStatusEvent.ToOffset());
							}
						}
						message.EM_GB = entryHeader.Branch.PK;
						message.EM_Status = EDIMessage.Status.Received;
						message.EM_LinkedObject = entryHeader;
					}
				}
			}

			if (entryHeader == null) // still
			{
				serviceLogger.Log(LogType.Warning, "Could not update dbo.CusEntryHeader since no matching entry header was found.");
				message.EM_Status = EDIMessage.Status.Failed;
			}

			McpRRA01RRA11AndRRA06EmailResponseProcessor emailResponseProcessor = new McpRRA01RRA11AndRRA06EmailResponseProcessor(holdAction, Logger);
			EmailDef responseSent = emailResponseProcessor.SendEmailResponse(entryHeader, rraMessage, message.Factory);
			message.EM_MessageInterpretation = emailResponseProcessor.SimpleHtmlBody;
			serviceLogger.Log(LogType.Information, "Finished processing " + rraMessage.MessageType + " and sent email to: " + responseSent.Recipients.RecipientsAsDelimitedString(", "));
			AddRraToEdocs(emailResponseProcessor.SimpleHtmlBody, entryHeader, rraMessage.MessageType);
			if (message.Interchange != null)
			{
				message.Interchange.EI_Status = EDIInterchange.Status.Received;
			}
		}

		void ApplyOrRemoveHoldOrClear(CusEntryHeader entryHeader, RRA01AndRRA11Message message)
		{
			message.ApplyHoldsOrClear(entryHeader);
			message.ProcessShipments(entryHeader);
		}

		void AddRraToEdocs(string body, CusEntryHeader entry, string rraMessageType)
		{
			if (entry != null)
			{
				try
				{
					var docManagerInfo = ((IDocManagerSupport)entry.Declaration).DocManagerInfo;
					var fileName = $"Release-removal advice for {CargoWise.IO.MakeFilenameSafe.MakeSafe(entry.EntryNumber)}.{rraMessageType}.htm";
					var printFile = docManagerInfo.AddFileOrDocument(ZBlob.FromAscii(body), fileName, Core.Constants.RefDocTypes.ReleaseRemovalAdvice, overwriteExistingFileIfNotImageFile: false);
					printFile.Description = string.Format(CultureInfo.CurrentCulture, RRA01AndRRA11.McpRRA01RRA11AndRRA06EmailResponseProcessor.Title, rraMessageType);
					_ = entry.Logs.AddNew(Events.DocumentAllocated, StmALogEventSourceExtensions.GenerateEventReference(Core.Constants.RefDocTypes.ReleaseRemovalAdvice, printFile.UniqueKey));
					docManagerInfo.MasterFactory.Save();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					serviceLogger.Information($"The entry document failed to be generated due to the following error: {ex.Message}");
				}
			}
		}

		MLCAI.CusAddInfo<MaritimeUcnThatIsHeld> FindUcnCusAddInfoFromIndividualUcn(UniqueConsignmentNumber uniqueConsignmentNumber, BusinessObjectFactory factory)
		{
			MLCAI.CusAddInfo<MaritimeUcnThatIsHeld> result = null;
			var queryWithRawUcn = new ZQuery(CusAddInfoSchema.B7_Type, MLCAI.CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld);
			queryWithRawUcn.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
			var queryWithTruncatedUcn = queryWithRawUcn.DeepClone();
			var helperRaw = Enterprise.Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, uniqueConsignmentNumber.Raw, CusAddInfoSchema.B7_AddInfoData, "UCN");
			var helperTruncated = Enterprise.Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, uniqueConsignmentNumber.ProperlyTruncatedUCN, CusAddInfoSchema.B7_AddInfoData, "UCN");
			queryWithRawUcn.AddToFilter(helperRaw);
			queryWithTruncatedUcn.AddToFilter(helperTruncated);
			var addInfos = factory.Load<MLCAI.CusAddInfo<MaritimeUcnThatIsHeld>>(queryWithRawUcn);
			if (addInfos.Length == 0)
			{
				// try again with other UCN style
				addInfos = factory.Load<MLCAI.CusAddInfo<MaritimeUcnThatIsHeld>>(queryWithRawUcn);
			}
			if (addInfos.Length == 1)  // if no hits or too many hits... do not update... but this should not happen (he says)
			{
				result = addInfos[0];
			}
			return result;
		}

		readonly ILogger serviceLogger;
		protected readonly IEDocsDelayedSaver EDocsSaver;
	}

	enum MatchOrHoldAction
	{
		Unknown,
		LastHoldRemoved,
		HoldsStillExist,
		MatchedByMucr,
		MatchedByEntryNumber,
		MatchedByCdsEntryNumber,
	}
}
