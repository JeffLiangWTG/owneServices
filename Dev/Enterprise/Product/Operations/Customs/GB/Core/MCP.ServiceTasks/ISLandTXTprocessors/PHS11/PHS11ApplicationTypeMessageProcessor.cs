using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.PHS11;
using Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.PHS11
{
	public class PHS11ApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		public PHS11ApplicationTypeMessageProcessor(ILogger serviceLogger, PHS11BaseMessageProcessor eDocsSaver)
			: base(new LoggingInformation())
		{
			this.ServiceLogger = serviceLogger;
			EDocsSaver = eDocsSaver;
		}

		protected override void ProcessMessageCore(EDIMessage ediMessage)
		{
			factory = ediMessage.Factory;
			var phs11 = GetPhs11FromEdiMessage(ediMessage);
			var simpleInterpretation = GetSimpleInterpretation(phs11);
			var declaration = FindDeclarationFromPhs11(phs11, ediMessage);
			if (declaration != null)
			{
				DoAllSuccessfulProcessing(declaration, phs11, ediMessage, simpleInterpretation);
			}
			else
			{
				NoMatchingJobProcessing(phs11, simpleInterpretation, ediMessage);
			}
			if (ediMessage.Interchange != null)
			{
				ediMessage.Interchange.EI_Status = EDIInterchange.Status.Received;
			}
		}

		void NoMatchingJobProcessing(PHS11Message phs11, ZString simpleInterpretation, EDIMessage ediMessage)
		{
			var subject = string.Format("Destin8 PHS11 port health status update for UCN {0} could not be matched to a job", phs11.UCN);
			var body = string.Format(@"<p>A port health status update message (PHS11) was received from Destin8, <font color='red'> but no corresponding job could be found in the Broker Software database</font>.</p>
										<p>Some summary details are below. You should contact Destin8 to query the mismatch.</p>
										{0}", simpleInterpretation);
			var emailSender = new HtmlNotificationEmailSender();
			var email = emailSender.CreateEmail(subject, body);
			email.Attachments.Add(new AttachmentDef("Failed PHS11.txt", ZBlob.FromAscii(ediMessage.EM_MessageText)));
			McpRRA01RRA11AndRRA06EmailResponseProcessor.QueueEmail(factory, email, null, new LoggingInformation(), "PHS11", Guid.Empty);
			ServiceLogger.Log(LogType.Warning, "Could not find job from PHS11 for UCN " + phs11.UCN);
			ediMessage.EM_Status = EDIMessage.Status.Failed;
		}

		void DoAllSuccessfulProcessing(JobDeclaration declaration, PHS11Message phs11, EDIMessage ediMessage, ZString simpleInterpretation)
		{
			ediMessage.EM_MessageInterpretation = simpleInterpretation;
			ediMessage.EM_MessageType = ApplicationCodeList.Codes.GbMcpPortHealth;
			ediMessage.EM_MessageSubType = phs11.PortHealthStatus.Left(3);
			ediMessage.EM_Status = EDIMessage.Status.Received;
			ediMessage.EM_GB = declaration.JE_GB;
			declaration.Logs.AddNew(Events.CustomsEntryStatus, "PortHealth-" + phs11.PortHealthStatus, phs11.CreateDateTime.ToOffset());
			if (declaration.CustomsEntryHeaders.Count > 0)
			{
				var entry = declaration.CustomsEntryHeaders[0];
				entry.Messages.Add(ediMessage);
				if (phs11.PortHealthStatus != PortHealthStatusCodes.Codes.ReleasedRel)
				{
					entry.CH_EntryStatus = EntryStatusList.Codes.Hold;
				}
			}
			else
			{
				declaration.Messages.Add(ediMessage);  // not going to be visible 
			}
			RenderIntoEDocsAndOntoPaper(ediMessage);
			SendSuccessEmail(declaration, simpleInterpretation, phs11);
			ServiceLogger.Log(LogType.Information, string.Format("Processed PHS11 for UCN {0}, declaration {1}, match method {2}", phs11.UCN, declaration.JE_DeclarationReference, howFoundJob));
		}

		void RenderIntoEDocsAndOntoPaper(EDIMessage ediMessage)
		{
			var gbEdiMessage = factory.Load<GbEDIMessage>(ediMessage.PK);
			var phs11DocumentPk = PrinterFromEdiMessageHelper_MenuKeys.MCP.PHS11;
			var printHelper = new PrinterFromEdiMessageHelper(gbEdiMessage.Factory, ServiceLogger);
			printHelper.PrintToEdocsAndPaper(phs11DocumentPk, gbEdiMessage, gbEdiMessage, GBCustomsDataRegistry.Instance.PrinterMcpNonChief, true);
		}

		void SendSuccessEmail(JobDeclaration declaration, ZString simpleInterpretation, PHS11Message phs11)
		{
			var hyperlinkedText = McpRRA01RRA11AndRRA06EmailResponseProcessor.MakeHtmlAnchor(McpRRA01RRA11AndRRA06EmailResponseProcessor.GetHyperlinkUriForDeclaration(declaration), declaration.JE_DeclarationReference);
			var subject = string.Format("Destin8 PHS11 port health status update for UCN {0}, declaration {1}", phs11.UCN, declaration.JE_DeclarationReference);
			var body = string.Format(@"<p>A port health status update message (PHS11) was received from Destin8.</p>
										<p>Some summary details are below. Subject to correcting printing and eDocs options, a full version is available on paper and against the declaration's eDocs tab.</p>
										{0}
										<p>To open the job, click here: {1}</p>",
										simpleInterpretation, hyperlinkedText);
			var email = new HtmlNotificationEmailSender().CreateEmail(subject, body, declaration.CompanyPK.ToGuid(), declaration.RegistryBranchPK, null);
			var user = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, declaration.JE_GS_NKCusAgent));
			if (user == null && declaration.CustomsEntryHeaders.Count > 0 && declaration.CustomsEntryHeaders[0].Messages.LastOutgoingNonSystemAndNonNullUserMessage != null)
			{
				user = declaration.CustomsEntryHeaders[0].Messages.LastOutgoingNonSystemAndNonNullUserMessage.UserWhoQueuedThisRecord;
			}
			McpRRA01RRA11AndRRA06EmailResponseProcessor.QueueEmail(declaration.Factory, email, user, new LoggingInformation(), "PHS11", declaration.RegistryBranchPK);
		}

		ZString GetSimpleInterpretation(PHS11Message phs11)
		{
			var table = new HtmlTableCreator(new string[] { "Field", "Value" });
			table.WriteRow("UCN", phs11.UCN);
			table.WriteRow("Unit", phs11.UnitId);
			table.WriteRow("Port Health Status", phs11.PortHealthStatus + " " + new PortHealthStatusCodes().GetDescriptionFromCode(phs11.PortHealthStatus));
			return table.ToHtml();
		}

		JobDeclaration FindDeclarationFromPhs11(PHS11Message phs11, EDIMessage ediMessage)
		{
			/*
			 Contractual specs state:
				  Matching a declaration is as follows.
					1.	Initially try to get a verbatim match on a declaration’s MUCR against the message’s UCN field.  
					2.	If multiple or zero matches in (1), try looking for an individual held UCN – try to match the UCN in the PHS11 against one that has been recorded from a previous RRA12
					3.	If no matches in (2), search all GB import declarations for the agent’s reference number against box 7. 
					4.	If zero or multiple hits after trying these approaches, quit with failure (and email the customs notification group to warn them). 
			 */

			JobDeclaration result = null;
			// 1
			var query = phs11.GetParentQueryFromUcnUsingMucr<JobDeclaration>();
			var declarations = factory.Load<JobDeclaration>(query);
			if (declarations.Length != 1)
			{
				// 2
				var heldUcnsQuery = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld);
				heldUcnsQuery.AddToFilter(Enterprise.Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, phs11.UCN, CusAddInfoSchema.B7_AddInfoData, MaritimeUcnThatIsHeldSchema.NW_UCN.Name.Substring(3)));
				var heldUcn = factory.LoadTop1<CusAddInfo<MaritimeUcnThatIsHeld>>(heldUcnsQuery);
				if (heldUcn != null)
				{
					var entry = factory.Load<CusEntryHeader>(heldUcn.B7_ParentID);
					if (entry != null)
					{
						result = entry.Declaration;
						howFoundJob = HowFoundJob.SubordinateHeldUCN;
					}
				}
				if (result == null)
				{
					// 3
					result = FindDeclarationFromAgentReference(phs11, JobDeclarationSchema.JE_DeclarationReference);  // user did not put a value into JE_AgentRefernece so we set JE_DeclartationReference to CHIEF
					if (result == null)
					{
						result = FindDeclarationFromAgentReference(phs11, JobDeclarationSchema.JE_OwnerRef);  // user's own reference
						howFoundJob = HowFoundJob.AgentReference;
					}
					else
					{
						howFoundJob = HowFoundJob.JobReference;
					}
				}
			}
			else
			{
				result = declarations[0];
				howFoundJob = HowFoundJob.MainUCN;
			}
			return result;
		}

		JobDeclaration FindDeclarationFromAgentReference(PHS11Message phs11, SchemaStringColumn schemaColumName)
		{
			JobDeclaration result = null;
			var declarationRefAsAgentsRefQuery = new ZQuery(schemaColumName, phs11.AgentsReference);  // box7
			declarationRefAsAgentsRefQuery.AddToFilter(GbInterchangeSender.BritishBranches(factory, JobDeclarationSchema.JE_GB));
			declarationRefAsAgentsRefQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import);
			var declarations = factory.Load<JobDeclaration>(declarationRefAsAgentsRefQuery);
			if (declarations.Length == 1)
			{
				result = declarations[0];
			}
			return result;
		}

		PHS11Message GetPhs11FromEdiMessage(EDIMessage message)
		{
			return new PHS11Message(message.EM_MessageText);
		}

		protected override string ApplicationCodeCore
		{
			get { return ApplicationCodeList.Codes.GbMcpPortHealth; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return ApplicationCodeList.Descriptions.GbMcpPortHealth; }
		}

		protected readonly PHS11BaseMessageProcessor EDocsSaver;
		BusinessObjectFactory factory;
		ILogger ServiceLogger { get; set; }

		HowFoundJob howFoundJob;

		enum HowFoundJob
		{
			NotFound,
			MainUCN,
			SubordinateHeldUCN,
			JobReference,
			AgentReference
		}
	}
}
