using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.CusRes;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief
{
	public abstract class JobDeclarationMessageManagerFrontEnd : Business.GBJobDeclarationMessageManager
	{
		public JobDeclarationMessageManagerFrontEnd(IMessageGenerator<EU.Business.Declaration.CusEntryHeader> transmissionGenerator, JobDeclaration declaration)
			: base(declaration, transmissionGenerator)
		{
		}

		protected override void SaveFactoryAfterSendingMessages(ISendsMessagesToCustoms sender, CancellationToken token)
		{
			var logger = new Logger();
			var uploaderInterchangeSender = GetNewGbCusdecUploaderInterchangeSender(logger);
			uploaderInterchangeSender.SetMaximumBatchSize(1);
			bool keepLooping = true;
			int numberOfLoops = 0;
			while (keepLooping && numberOfLoops < 100) // sanity check - no one will have more than 99 entries per declaration, and this stops some awful infinite loop possibility)
			{
				numberOfLoops++;
				try
				{
					// Upload data to CSP and "save" result
					uploaderInterchangeSender.ExecuteBatch(token); // If there is enough time left, this can send multiple interchanges in one call, but in debug mode the determination of the amount fo time left is controlled by BaseInterchangeSender.TimedOutBehaviour.Value = BaseInterchangeSender.TimedOutBehaviourForTest.IMMEDIATE, i.e. only ever have time for one interchange per batch. Gotcha!
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					MarkOutgoingMessagesAndInterchangesAsFailed(uploaderInterchangeSender, logger);
					logger.Log(LogType.Error, "Unhandled exception while uploading data and receiving response - " + ex.Message);
					keepLooping = false;
					break;
				}

				if (uploaderInterchangeSender.NumberOfOperationsAttempted == uploaderInterchangeSender.NumberOfOperationsSucceeded)
				{
					// Process response
					ZString oldEntryNumber;
					ZString oldRouteAndSoeAndIcs;
					GetEntryNumberAndRoutingDetails(out oldEntryNumber, out oldRouteAndSoeAndIcs, null);
					var query = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					query.FetchOnlyFromLocalCache = true;
					query.AddToFilter(EDIMessageSchema.EM_Status, StatusMeaningTransientPostDownloadPreProcessGUI);
					query.AddToFilter(EDIMessageSchema.EM_EI, (from EDIInterchange i in uploaderInterchangeSender.ResponseInterchanges select i.PK));
					foreach (var responseMsg in declaration.Factory.Load<GbEDIMessage>(query))
					{
						try
						{
							new CusResResponseProcessor(logger, null).ProcessMessage(responseMsg);
							logger.Log(LogType.Information, GetOverallPopupDisplayText(ref oldEntryNumber, ref oldRouteAndSoeAndIcs, responseMsg));
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							logger.Log(LogType.Error, "Unhandled exception while processing response - " + ex.Message);
							keepLooping = false;
							break;
						}
					}
				}
				else
				{
					MarkOutgoingMessagesAndInterchangesAsFailed(uploaderInterchangeSender, logger);
					logger.Log(LogType.Error, "Upload failed or response did not contain any data");
					break;
				}
				keepLooping = uploaderInterchangeSender.NumberOfOperationsAttempted > 0;
			}

			try
			{
				declaration.Factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				MarkOutgoingMessagesAndInterchangesAsFailed(uploaderInterchangeSender, logger);
				logger.Log(LogType.Error, "Saving changes failed after upload/processing. The received message should be visible on the messages grid, please take a screenshot of its details before closing this form.");
				logger.Log(LogType.Error, ex.Message);
			}

			if (logger.HasAnyErrorMessage)
			{
				sender.NotifyUserOfAnInvalidOperation(logger.ToStringWithExtraNewLine());
			}
			else
			{
				if (declaration.IsImport)
				{
					CountOfImportDeclarationsThisSession++;
				}

				if (declaration.IsExport)
				{
					CountOfExportDeclarationsThisSession++;
				}

				((ISendsMessagesToCustomsExtraMembers)sender).NotifyUserOfASuccessfulOperation(logger.ToStringWithExtraNewLine(), "Messaging Operation Completed");
			}
			declaration.RefreshBindingIncludingChildren();
			foreach (var e in declaration.CustomsEntryHeaders)
			{
				e.RefreshBindingIncludingChildren();
			}
		}

		ZString GetOverallPopupDisplayText(ref ZString oldEntryNumber, ref ZString oldRouteAndSoeAndIcs, EDIMessage responseMsg)
		{
			ZString newRouteAndSoeAndIcs;
			ZString newEntryNumber;
			GetEntryNumberAndRoutingDetails(out newEntryNumber, out newRouteAndSoeAndIcs, responseMsg);
			var acknowledgedOrRejected = (IsMessageRejectedType27(responseMsg)) ? "REJECTED. " : "";
			var messageDetails = string.Format("{4}DUCR: {5}. Message Details: {0}/{1}/{2} #{3}", responseMsg.EM_ApplicationReference, responseMsg.EM_MessageType, responseMsg.EM_MessageSubType, responseMsg.EM_MessageNum, acknowledgedOrRejected, ((Business.Declaration.CusEntryHeader)responseMsg.EM_LinkedObject).CH_BGMReference);
			if (IsMessageRejectedType27(responseMsg))
			{
				messageDetails += "\r\n\r\n" + CusResEdifactParser.GetRejectionSummaryFromHtmlInterpretation(responseMsg.EM_MessageInterpretation);
			}
			var overAllDisplay = ZString.Empty;
			if (oldEntryNumber != newEntryNumber)
			{
				overAllDisplay = "Entry number: " + declaration.DeclarationNumber;
			}
			if (oldRouteAndSoeAndIcs != newRouteAndSoeAndIcs)
			{
				overAllDisplay += newRouteAndSoeAndIcs;
			}
			overAllDisplay += " " + messageDetails;
			return overAllDisplay.TrimStart();
		}

		static bool IsMessageRejectedType27(EDIMessage responseMsg)
		{
			return responseMsg.EM_MessageSubType == "27";
		}

		void GetEntryNumberAndRoutingDetails(out ZString entryNumber, out ZString routeAndSoeAndIcs, EDIMessage responseMsg)
		{
			var entry = responseMsg == null ? null : (Business.Declaration.CusEntryHeader)responseMsg.EM_LinkedObject;
			var roe = entry == null ? declaration.JE_GBRouteOfEntry : entry.CH_RouteOfEntry;
			var ics = entry == null ? declaration.ZG_ImportClearanceStatusICS : entry.CH_ImportClearanceStatusICS;
			var soe = entry == null ? declaration.ZG_StyleOfEntrySOE : entry.CH_StyleOfEntrySOE;

			entryNumber = declaration.DeclarationNumber;
			routeAndSoeAndIcs = string.Format(" Route '{0}', ICS '{1}', SOE '{2}'", roe, ics, soe);
		}

		void MarkOutgoingMessagesAndInterchangesAsFailed(GbCusdecUploaderInterchangeSender uploaderInterchangeSender, Logger logger)
		{
			foreach (var outgoingInterchange in uploaderInterchangeSender.OutgoingInterchanges)
			{
				if (outgoingInterchange != null)
				{
					outgoingInterchange.EI_Status = EDIInterchange.Status.Failed;
					foreach (EDIMessage message in outgoingInterchange.ContainedMessages)
					{
						message.EM_Status = EDIMessage.Status.Failed;
					}
				}
			}
		}

		protected abstract GbCusdecUploaderInterchangeSender GetNewGbCusdecUploaderInterchangeSender(Logger logger);

		public const string StatusMeaningTransientPostDownloadPreProcessGUI = "GUI";
		public const string StatusMeaningPreUploadQueued = "GQQ";
	}
}
