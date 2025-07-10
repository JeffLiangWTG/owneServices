using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.NES;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact.Auto;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.CusRes
{
	/// <summary>
	/// Base class from with CUSRES and CONTRL processors derive
	/// </summary>
	public abstract class CusResHandler_BASE
	{
		public CusResHandler_BASE(ILogger iLogger)
		{
			this.logger = iLogger;
		}

		protected void MarkStatusesOfEntryAsFailedIfLastOutboundMessageWasNotEnquiry()
		{
			if (entry != null)
			{
				// Do both otherwise one HELPFULLY resets the other to AWR
				if (entry.Messages.LastSentOutgoingMessage != null)
				{
					string sentMessageType = entry.Messages.LastSentOutgoingMessage.EM_MessageType;
					if (sentMessageType == Interrogate_DecDucr.FunctionCodeConst   // both DUCR and MUCR here
					|| sentMessageType == Interrogate_DecMucr.FunctionCodeConst
					|| sentMessageType == Interrogate_Dem.FunctionCodeConst
					|| sentMessageType == Interrogate_DevDucr.FunctionCodeConst
					|| sentMessageType == Interrogate_Req.FunctionCodeConst
					|| sentMessageType == Interrogate_Lem.FunctionCodeConst)
					{
						entry.CH_Status = MessageStatusList.Codes.OK;
						return;  // do not update entry to ERR as this will ruin the RTH/ CRQ/ etc status
					}
				}
				if (entry.CH_EntryStatus == EntryStatusList.Codes.AwaitingResponse)
				{
					entry.CH_EntryStatus = EntryStatusList.Codes.SentAndInitiallyRejected;
				}
				entry.CH_Status = MessageStatusList.Codes.SentAndRejected;
			}
		}

		public void DoAllProcessing(EDIMessage inboundMessage)
		{
			DoAllProcessingCore(inboundMessage);
		}

		protected void PerformPreProcessingInitialisation(EDIMessage inboundMessage)
		{
			factory = inboundMessage.Factory;
			incomingMessage = inboundMessage;
			edifactObject = GetEdifactMessage();
			if (edifactObject == null)
			{
				throw new NotSupportedException("Could not make an SegmentGroup from the inbound message. Only CUSRES, CONTRL and UKCTRL messages are supported.");
			}
			responseMsgIdNumber = GetUniqueReferenceNumberFromInboundMessagePreferablySysCar();
		}

		protected virtual void DoAllProcessingCore(EDIMessage inboundMessage)
		{
			PerformPreProcessingInitialisation(inboundMessage);
			if (this.MessageCode == ChiefConstants.CusDecTypeDLU) // this is a CUSRES/27 for DLU, which are not linked to entries
			{
				originalOutgoingMessage = FindMatchingOutboundMessage();
				UpdateDefinitelyFoundEntryAndOutgoingMessageAndDoAllNotifications();
			}
			else
			{
				GetEntryAndGbDeclarationToWhichThisPertainsFromMessageId();
				if (entry == null)
				{
					WarnOfNotFindingEntry();
					incomingMessage.EM_Status = EDIMessage.Status.Failed;
				}
				else
				{
					originalOutgoingMessage = FindMatchingOutboundMessage();
					entry.Messages.Add(incomingMessage);
					incomingMessage.EM_GB = entry.Declaration.Branch.PK;
					UpdateDefinitelyFoundEntryAndOutgoingMessageAndDoAllNotifications();
				}
			}
			ChangeDefaultValuesOnIncomingMessageToSomethingUseful();
		}

		protected abstract bool IsSuccessfulAcknowledgement { get; }
		protected abstract SegmentGroup GetEdifactMessage();
		protected abstract UkResponseMsgIdNumber GetUniqueReferenceNumberFromInboundMessagePreferablySysCar();
		protected abstract void UpdateDefinitelyFoundEntryAndOutgoingMessageAndDoAllNotifications();
		protected abstract ZString ClassOfInboundMessage { get; }
		protected abstract ZString MessageFunction { get; }

		protected virtual ZString MessageCode
		{
			get
			{
				return ZString.Empty;
			}
		}

		/// <summary>
		/// Changes values from EM_ApplicationReference = 0000041 and EM_MessagSubType=XXX to something prettier
		/// </summary>
		protected virtual void ChangeDefaultValuesOnIncomingMessageToSomethingUseful()
		{
			incomingMessage.EM_ApplicationReference = ClassOfInboundMessage;
			incomingMessage.EM_MessageSubType = MessageFunction;
			incomingMessage.EM_MessageType = MessageCode;
			if (incomingMessage.Interchange != null)
			{
				if (incomingMessage.Interchange.EI_Status != JobDeclarationMessageManagerFrontEnd.StatusMeaningTransientPostDownloadPreProcessGUI)
				{
					// Keep EI_Status if the processing is done by user
					incomingMessage.Interchange.EI_Status = EDIInterchange.Status.Received;
				}
			}
		}

		protected void GetEntryAndGbDeclarationToWhichThisPertainsFromMessageId()
		{
			if (responseMsgIdNumber == null || responseMsgIdNumber.Reference.IsEmpty)
			{
				return; //Let the caller handle and log the Null ResponseMsgIdNumber in the EDIFACT message
			}

			switch (responseMsgIdNumber.TypeOfReference)
			{
				case UkResponseMsgIdNumberType.CommonAccessReference:
					FindEntryFromSysCar();
					break;

				case UkResponseMsgIdNumberType.UniqueConsignmentNumberWithPart:
					FindEntryFromUCR();
					break;

				case UkResponseMsgIdNumberType.EntryNumber:
					FindEntryFromEntryNumber();
					break;

				case UkResponseMsgIdNumberType.InterchangeControlReference:
					FindEntryFromInterchangeControlReference();
					break;

				case UkResponseMsgIdNumberType.Box7TdrOwnRefEnt:
					FindEntryFromBox7Reference();
					break;
			}

			if (entry != null)
			{
				gbDeclaration = factory.Load<JobDeclaration>(entry.CH_JE);
			}
		}

		void FindEntryFromSysCar()
		{
			// We have sent the entry's PK in our outgoing message, and chief return it.
			ZGuid supposedEntryPK;
			try
			{
				supposedEntryPK = new ZGuid(new Guid(this.responseMsgIdNumber.Reference));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new Exception("CHIEF did not return to us in SYS-CAR the ID of the entry to which this message pertains.");
			}
			this.entry = factory.Load<CusEntryHeader>(supposedEntryPK);
		}

		void FindEntryFromUCR()
		{
			ZQuery query = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, responseMsgIdNumber.Reference);
			// How to limit to UK?  Hmmm.  Joining to JobDeclaration and limiting on JE_GB is fine in production but not on DAT (it runs under an Aussie company, somehow)
			entry = factory.LoadTop1<CusEntryHeader>(query);
		}

		void FindEntryFromEntryNumber()
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryNum, responseMsgIdNumber.Reference);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeader.Schema.TableName);
			Common.CusEntryNumber cusEntryNum = factory.LoadTop1<Common.CusEntryNumber>(query);
			if (cusEntryNum != null)
			{
				entry = factory.Load<CusEntryHeader>(cusEntryNum.CE_ParentID);
			}
		}

		void FindEntryFromInterchangeControlReference()
		{
			string appCode;
			string interchangeNum;
			try
			{
				// Ref will be e.g. NES/13
				appCode = responseMsgIdNumber.Reference.Split('/')[0];
				interchangeNum = responseMsgIdNumber.Reference.Split('/')[1];
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new Exception("Could not pull interchange number. Unable to find outgoing interchange and therefore entry", ex);
			}

			ZDBOnlyQuery qInt = GetInterchangeLoadQueryWithoutApplicationCode(appCode, interchangeNum);
			qInt.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, appCode);
			EDIInterchange originalOutboundInterchange = null;
			var allCandidateInterchanges = factory.Load<EDIInterchange>(qInt);
			if (allCandidateInterchanges.Length == 0)
			{
				//load again without application code as the previous query was too restrictive. 
				qInt = GetInterchangeLoadQueryWithoutApplicationCode(appCode, interchangeNum);
				allCandidateInterchanges = factory.Load<EDIInterchange>(qInt);
			}
			if (allCandidateInterchanges.Length == 1)
			{
				originalOutboundInterchange = allCandidateInterchanges[0];
			}
			else if (allCandidateInterchanges.Length > 1)  // if the user has two NES badges then outbound interchange numbers are not unique
			{
				originalOutboundInterchange = (from EDIInterchange i
													in allCandidateInterchanges
											   where incomingMessage.Interchange != null && i.EI_From == incomingMessage.Interchange.EI_To
											   select i).FirstOrDefault();
			}

			if (originalOutboundInterchange != null && originalOutboundInterchange.ContainedMessages.Count > 0)
			{
				var linkedObjectHopefullyCusEntryHeader = originalOutboundInterchange.ContainedMessages[0].EM_LinkedObject;
				entry = linkedObjectHopefullyCusEntryHeader as CusEntryHeader;
				if (entry == null)
				{
					logger.Log(LogType.Warning, string.Format("Linked object on interchange {0}'s message {1} found using interchange number and optionally application code {2} was not a CusEntryHeader. Is was {3}", interchangeNum, incomingMessage.EM_MessageNum, appCode, linkedObjectHopefullyCusEntryHeader.GetType().FullName));
				}
			}
		}

		ZDBOnlyQuery GetInterchangeLoadQueryWithoutApplicationCode(string appCode, string interchangeNum)
		{
			ZDBOnlyQuery qInt = new ZDBOnlyQuery(typeof(EDIInterchange));
			qInt.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, interchangeNum);
			ZGuid[] britishBranches = GetBritishBranchesPK(factory);
			if (britishBranches.Length > 0)
			{
				// silly DAT runs against an Aussie branch even though we expressly tell it that it's British.  Pfff.
				qInt.AddToFilter(EDIInterchangeSchema.EI_GB, britishBranches);
			}
			qInt.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			return qInt;
		}

		void FindEntryFromBox7Reference()
		{
			// Reference will either be:  B0001000, ABC123, or  B0001000/ABC123 depending on which version client has

			var query1 = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, responseMsgIdNumber.Reference);
			query1.AddToFilter(JobDeclarationSchema.JE_OwnerRef, ZString.Empty);

			ZQuery query2 = null;
			if (responseMsgIdNumber.Reference.IndexOf("/") > 0)
			{
				query2 = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, responseMsgIdNumber.Reference.Split('/')[0]);
				query2.AddToFilter(JobDeclarationSchema.JE_OwnerRef, SQLComparisonOperator.StartsWith, responseMsgIdNumber.Reference.Split('/')[1]);
			}

			var query3 = new ZQuery(JobDeclarationSchema.JE_OwnerRef, responseMsgIdNumber.Reference);

			if (!RunBox7Query(query1) && (query2 == null || !RunBox7Query(query2)) && !RunBox7Query(query3))
			{
				// We tried and failed :(
			}
		}

		bool RunBox7Query(ZQuery query)
		{
			var result = false;
			query.AddToFilter(JobDeclarationSchema.JE_GB, GetBritishBranchesPK(factory));
			var declarations = factory.Load<JobDeclaration>(query);
			if (declarations.Length == 1 && declarations[0].ActiveEntryHeaders.Count > 0)
			{
				entry = (CusEntryHeader)declarations[0].ActiveEntryHeaders[0];
				result = true;
			}
			return result;
		}

		ZGuid[] GetBritishBranchesPK(BusinessObjectFactory factory)
		{
			var branchLoader = new GlbBranch.Loader(factory);
			List<ZGuid> guids = new List<ZGuid>();
			foreach (GlbBranch oneBritishBranch in branchLoader.LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.UnitedKingdom))
			{
				guids.Add(oneBritishBranch.PK);
			}
			return guids.ToArray();
		}

		protected virtual EDIMessage FindMatchingOutboundMessage()
		{
			EDIMessage messageFound = null;
			ZDateTime maxDateTime = ZDateTime.Empty;
			List<string> appCodesWeCareAbout = new List<string>(); // This is a bit cheeky, but it prevents us from getting confused if user flip-flops between Gems and a port CSP
			appCodesWeCareAbout.Add(ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly);
			appCodesWeCareAbout.Add(ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly);
			appCodesWeCareAbout.Add(NesConstants.ApplicationCode);
			appCodesWeCareAbout.Add(ApplicationCodeList.Codes.GbCcsuk);
			appCodesWeCareAbout.Add(ApplicationCodeList.Codes.Pentant);
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, appCodesWeCareAbout);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			EDIMessage[] messagesOnCusentryheader = this.factory.Load<EDIMessage>(query);
			foreach (EDIMessage messageIterator in messagesOnCusentryheader)
			{
				if (messageIterator.EM_ApplicationCode == ApplicationCodeList.Codes.Pentant && messageIterator.EM_MessageType == "CAR")
				{
					continue;
				}
				if (messageIterator.EM_SystemCreateTimeUtc > maxDateTime || maxDateTime.IsEmpty)
				{
					maxDateTime = messageIterator.EM_SystemCreateTimeUtc;
					messageFound = messageIterator;
				}
			}
			return messageFound;
		}

		protected void SendEmailAndSetReceivedMessageToBeBodyOfEmail(ZString subject, ZString body, JobDeclaration declaration, Guid notificationGroupPk, IRegistryItem notificationRegistryItem)
		{
			SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, body, declaration, ControllerIDs.Customs.JobDeclaration, notificationGroupPk, notificationRegistryItem);
		}

		protected void SendEmailAndSetReceivedMessageToBeBodyOfEmail(ZString subject, ZString body, BusinessObject bizOForHyperlink, ControllerID controllerId, Guid notificationGroupPk, IRegistryItem notificationRegistryItem)
		{
			incomingMessage.EM_MessageInterpretation = MessagePrettierCss.CSS + body;
			if (responseMsgIdNumber.TypeOfReference != UkResponseMsgIdNumberType.Box7TdrOwnRefEnt)
			{
				// Do not send warning emails for being unable to find a single entry when the reference number type is as lowly as a Box7 reference.  Only affects pass-through for messages generated by CSP, and if user has pass-through then they likely have Gems CSV messages too.  The latter will update the job, so we can just fail silently. 
				if (bizOForHyperlink != null && controllerId != null)
				{
					string uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerId, bizOForHyperlink.PK.ToGuid());
					body += string.Format("<BR><BR>To open the job, click here: <a href='{0}'>{1}</a>", uri, bizOForHyperlink.HumanReadableName);
				}

				HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
				IBranchProvider branchProvider = bizOForHyperlink as IBranchProvider;
				EmailDef email = branchProvider != null
														? emailSender.CreateEmail(subject, body, branchProvider.Branch.Company.PK.ToGuid(), branchProvider.Branch.PK.ToGuid(), null)
														: emailSender.CreateEmail(subject, body);

				GlbStaff originalSender = (originalOutgoingMessage != null) ? originalOutgoingMessage.UserWhoQueuedThisRecord : null;
				new Customs.Business.EmailSender(new LoggingInformation()).SendNotification(
					email,
					originalSender,   // user who's doing the job
					GBCustomsDataRegistry.Instance.CustomsResponseNotifications,  // option about whether to send to user, group, group & user, none.
					notificationGroupPk,
					notificationRegistryItem,
					incomingMessage.Factory
					);
			}
		}

		protected string GetReferenceNumberForEmailAndHtml()
		{
			return GetReferenceNumberForEmailAndHtml(entry);
		}

		public static string GetReferenceNumberForEmailAndHtml(CusEntryHeader entry)
		{
			var bOrSNumber = entry.Declaration.JE_DeclarationReference;
			var fullDucrAndPart = entry.CH_BGMReference;
			var sendBothFields = false;
			if (!fullDucrAndPart.Contains(bOrSNumber) || entry.Declaration.ActiveEntryHeaders.Count > 1 || fullDucrAndPart.Contains("/"))
			{
				sendBothFields = true;
			}
			return sendBothFields ? string.Format("{0} [{1}]", bOrSNumber, fullDucrAndPart) : bOrSNumber.ToString();
		}

		protected ZString PutValuesIntohtmlTemplateForEmail(string htmlTemplateResourceFilename, string htmlTables)
		{
			return PutValuesIntohtmlTemplateForEmail(htmlTemplateResourceFilename, htmlTables, GetReferenceNumberForEmailAndHtml());
		}

		protected ZString PutValuesIntohtmlTemplateForEmail(string htmlTemplateResourceFilename, string htmlTables, string jobReference)
		{
			ZString emailTemplateHtml;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream(htmlTemplateResourceFilename))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}

			emailTemplateHtml = emailTemplateHtml.Replace("{JOBDECLARATIONREFERENCE}", jobReference);
			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", htmlTables);
			return emailTemplateHtml;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		void WarnOfNotFindingEntry()
		{
			if (responseMsgIdNumber != null)
			{
				string subject = string.Format("{0} - could not find job for inbound message", responseMsgIdNumber.Reference);
				string body = string.Format("It was not possible to find a Customs Entry with the above ID.  If this job was not done through {0}, please ensure that the CSPs do not send irrelevant data to your {0}.  If this job is a training entry that was not properly deleted from CHIEF, you may need to do so now. <hr>", BrandingFactory.Instance.ProductName);
				body += responseMsgIdNumber.TypeOfReference.ToString() + " = " + responseMsgIdNumber.Reference + "<br>";
				body += "Class=" + ClassOfInboundMessage + "<br>";
				body += "Function=" + MessageFunction + "<br>";
				body += "Code=" + MessageCode + "<hr>";
				body += GetHalfDecentAttemptAtInterpretingEdifact(incomingMessage);
				body += incomingMessage.EM_MessageText;
				SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, body, null, null,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefErrors, "", GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty),
					GBCustomsDataRegistry.Instance.NotificationChiefErrors);
			}
			else
			{
				logger.Log(LogType.Warning, string.Format($"Unable to process message #{incomingMessage.EM_MessageNum}: ResponseMsgIdNumber has not been set in the EDIFACT message. Message Status has been set to Failed."));
			}
		}

		protected virtual string GetHalfDecentAttemptAtInterpretingEdifact(EDIMessage incomingMessage)
		{
			return "";
		}

		protected ForwardingConsol GetConsolFromInboundMessage()
		{
			if (responseMsgIdNumber.TypeOfReference == UkResponseMsgIdNumberType.CommonAccessReference)
			{
				originalOutgoingMessage = factory.Load<EDIMessage>(new ZGuid(responseMsgIdNumber.Reference));
				if (originalOutgoingMessage != null)
				{
					return originalOutgoingMessage.EM_LinkedObject as ForwardingConsol;
				}
			}
			return null;
		}

		protected ILogger logger;
		protected BusinessObjectFactory factory;
		protected UkResponseMsgIdNumber responseMsgIdNumber;
		protected JobDeclaration gbDeclaration;
		protected CusEntryHeader entry;
		protected EDIMessage originalOutgoingMessage;
		protected SegmentGroup edifactObject;
		protected EDIMessage incomingMessage;
	}
}
