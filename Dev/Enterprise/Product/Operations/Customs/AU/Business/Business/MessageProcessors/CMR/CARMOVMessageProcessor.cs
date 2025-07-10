using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CARMOVMessageProcessor : CMRMessageResponseProcessor
	{
		public CARMOVMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.CARMOV, "Carrier Movement Advice (CARMOV)")
		{
		}

		#region Implementation

		protected override string DoPreProcessingReturningStatus(EDIMessage message)
		{
			return EDIMessage.Status.PreProcessedOK;
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			incomingMessage = (CMRCUSRESMessage)message;
			cUSRES = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet());

			ZString status = incomingMessage.GetStatus();
			if (status == "DO NOT LOA")
			{
				status = "DO NOT LOAD";//fix this status
			}

			if (status != "LOAD" || Env.Registry.AUCustoms.AlertCarrierMovementLoad)
			{
				SendStatusNotification(status, incomingMessage.Report);
			}

			return EDIMessage.Status.Received;
		}

		protected void SendStatusNotification(string status, string report)
		{
			EmailDef email = new EmailDef();
			email.Subject = "CARMOV - " + status + " Advice Received";
			email.Body = report;
			GlbBranch branchToSendTo = GetBranchGuidForCTOID(incomingMessage.Factory, GetCTOEstablishmentID());
			if (incomingMessage != null)
			{
				email.Attachments.Add(new AttachmentDef("IncomingMessage.edi", Encoding.ASCII.GetBytes(ConvertToNiceOutput(incomingMessage.EM_MessageText))));
			}

			email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(Env.Registry.AUCustoms.GetCarrierMovementAdviceGroupForBranchOrCurrentCompany(branchToSendTo == null ? Guid.Empty : branchToSendTo.PK.ToGuid()), Env.Registry.RawRegistry.AUCCarrierMovementAdviceGroup);
			SendReport(email);
		}

		protected internal GlbBranch GetBranchGuidForCTOID(BusinessObjectFactory factory, string cTOID)
		{
			if (cTOID != null)
			{
				ZQuery cTOCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID);
				cTOCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, cTOID);
				cTOCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				foreach (OrgCusCode code in factory.Load<OrgCusCode>(cTOCodeFilter))
				{
					if (code.Header != null && (code.Header.OH_IsSeaCTO || code.Header.OH_IsAirCTO) && !code.Header.OH_RL_NKClosestPort.IsEmpty)
					{
						ZQuery branchFilter = new ZQuery();
						branchFilter.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
						branchFilter.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, code.Header.OH_RL_NKClosestPort);
						var result = factory.LoadTop1<GlbBranch>(branchFilter);
						if (result != null)
						{
							return result;
						}
					}
				}
			}
			return null;
		}

		protected internal string GetCTOEstablishmentID()
		{
			foreach (LOCSegment lOC in cUSRES.LOC)
			{
				if (lOC.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.Terminal)
				{
					return lOC.LocationIdentification.LocationNameCode;
				}
			}
			return null;
		}

		#endregion
	}
}
