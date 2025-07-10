using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseImportDeclarationMessageProcessor : CMRMessageResponseProcessor
	{
		public const string HeldStatus = "HELD";
		public const string ClearStatus = "CLEAR";
		public const string FinalisedStatus = "FINALISED";
		public const string WithdrawnStatus = "WITHDRAWN";
		public const string ProcessingStatus = "PROCESSING";
		public const string RejectedStatus = "REJECTED";

		public BaseImportDeclarationMessageProcessor(LoggingInformation logger, ZString messageCode, ZString messageName)
			: base(logger, messageCode, messageName)
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return !statusType.Contains(HeldStatus) && !statusType.Contains(RejectedStatus); }
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();
			if (result)
			{
				if (entryHeader == null)
				{
					if (incomingMessage.EM_MessageType != CMRMessage.CMRMessageTypes.SAM)
					{
						Logger.LogWarning("The incoming message is not responding to an entry header.  Can't continue.");
					}
					result = false;
				}
				else
				{
					SetEntryHeaderStatus();
					SetEntryHeaderNumber();

					if (consolidatedDeclaration != null)
					{
						entryHeader.DeriveConsolidatedStatus();
						consolidatedDeclaration.SyncStatusAfterMessageProcessing(incomingMessage);
					}
				}
			}
			return result;
		}

		#region Entry Number

		void SetEntryHeaderNumber()
		{
			CMRCUSRESMessage message = incomingMessage;

			if (message != null && entryHeader.CH_EntryStatus != CMRImportEntryAdvice.Processing.Code)
			{
				ZString entryNumber = message.EntryNumber;

				if (!entryNumber.IsEmpty)
				{
					entryHeader.EntryNumber = entryNumber;
				}
			}
		}

		#endregion

		#region Entry Header Status

		protected virtual void SetEntryHeaderStatus()
		{
			if (cUSRES != null && entryHeader != null)
			{
				foreach (FTXSegment fTX in cUSRES.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails)
					{
						ZString newEntryStatus = GetStatus(fTX.TextLiteral.FreeTextValue2);
						if (newEntryStatus != CMRImportEntryAdvice.Finalised.Code || entryHeader.CH_EntryStatus != CMRImportEntryAdvice.ATDReceived.Code)
						{
							entryHeader.CH_EntryStatus = newEntryStatus;
						}
						break;
					}
				}

				SetIsSubjectToRedLineProcessing();
				entryHeader.Declaration.JE_EntryStatus = entryHeader.Declaration.SummaryEntryStatusCalculator.SummaryEntryStatus;
			}
		}

		protected ZString GetStatus(ZString statusDescription)
		{
			ZString result = ZString.Empty;

			switch (statusDescription.ToUpper())
			{
				case HeldStatus:
					result = CMRImportEntryAdvice.Held.Code;
					break;

				case ClearStatus:
					result = CMRImportEntryAdvice.Clear.Code;
					break;

				case FinalisedStatus:
					result = CMRImportEntryAdvice.Finalised.Code;
					break;

				case WithdrawnStatus:
					result = CMRImportEntryAdvice.Withdrawn.Code;
					break;

				case ProcessingStatus:
					result = CMRImportEntryAdvice.Processing.Code;
					break;

				case RejectedStatus:
					result = CMRImportEntryAdvice.Rejected.Code;
					break;
			}

			return result;
		}

		#region Subject To Red Line Processing

		protected void SetIsSubjectToRedLineProcessing()
		{
			if (!incomingMessage.IsRejected)
			{
				entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = false;
			}

			if (entryHeader.CH_EntryStatus == CMRImportEntryAdvice.Held.Code || entryHeader.CH_EntryStatus == CMRImportEntryAdvice.Withdrawn.Code)
			{
				foreach (SegmentGroup4 group4 in cUSRES.Group4)
				{
					if (SegmentContainsSubjectToRedLineProcessing(group4))
					{
						entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = true;
						break;
					}
				}
			}
		}

		bool SegmentContainsSubjectToRedLineProcessing(SegmentGroup4 group4)
		{
			bool result = false;

			foreach (FTXSegment fTX in group4.FTX)
			{
				ZString freeTextValue = fTX.TextLiteral.FreeTextValue1;

				if (freeTextValue.Contains("RED LINE"))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region E-mail Groups

		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				if (entryHeader != null)
				{
					return Env.Registry.AUCustoms.EdificeSendAcknowledgementsToGroupForBranch(entryHeader.Declaration.RegistryCompanyPK, entryHeader.Declaration.BranchOfEmailGroupRegistry);
				}
				else
				{
					return Env.Registry.AUCustoms.EdificeSendAcknowledgementsToGroup;
				}
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return Env.Registry.AUCustoms.EdificeSendAcknowledgements; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				if (entryHeader != null)
				{
					return Env.Registry.AUCustoms.EdificeSendImpedimentsToGroupForBranch(entryHeader.Declaration.RegistryCompanyPK, entryHeader.Declaration.BranchOfEmailGroupRegistry);
				}
				else
				{
					return Env.Registry.AUCustoms.EdificeSendImpedimentsToGroup;
				}
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return Env.Registry.AUCustoms.EdificeSendImpediments; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get
			{
				if (entryHeader != null)
				{
					return Env.Registry.AUCustoms.EdificeSendErrorsToGroupForBranch(entryHeader.Declaration.RegistryCompanyPK, entryHeader.Declaration.BranchOfEmailGroupRegistry);
				}
				else
				{
					return Env.Registry.AUCustoms.EdificeSendErrorsToGroup;
				}
			}
		}

		protected override ZString ErrorEmailMode
		{
			get { return Env.Registry.AUCustoms.EdificeSendErrors; }
		}

		#endregion
	}
}
