namespace Enterprise.Customs.CA.Business
{
	using System.Linq;
	using CargoWise.Application;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Messaging.Business;
	using ServiceManager.Integration.Abstractions;

	partial class CusEntryHeader : IEDIFACTMessageAttachee, IEDIReleaseMessageAttachee, IK84ReportAttachee, IB3MessageProcessorLinkedObject
	{
		#region IEDIFACTMessageAttachee Members

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get { return Declaration; }
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { return Declaration.JE_DeclarationReference; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return CH_Status; }
			set { CH_Status = value; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return CH_EntryStatus; }
			set { CH_EntryStatus = value; }
		}

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get { return Messages; }
		}

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory
		{
			get { return Factory; }
		}

		bool IEDIFACTMessageAttachee.HasChanges
		{
			get
			{
				return Declaration?.HasChanges ?? HasChanges;
			}
		}

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			Messages.Add(message);
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get
			{
				return Declaration?.RefreshValidationBeforeSendMessage ?? true;
			}
		}

		#endregion

		#region IEDIReleaseMessageAttachee Members

		ZDateTime IEDIReleaseMessageAttachee.ReleaseDate
		{
			get { return Declaration.JE_EntryAuthorisationDate; }
			set { Declaration.JE_EntryAuthorisationDate = value; }
		}

		ZString IEDIReleaseMessageAttachee.ReleaseOffice
		{
			get { return Declaration.ReleaseOffice; }
			set { Declaration.CA_ReleaseOffice = value; }
		}

		bool IEDIReleaseMessageAttachee.SettingReleaseDateWithStatusUpdate { get; set; }

		ZString IEDIReleaseMessageAttachee.CargoControlNumbersReleaseStatus
		{
			get
			{
				var result = ZString.Empty;
				var releaseStatuses = new ReleaseStatusCollection(Declaration, false, false).Cast<ReleaseStatus>().Where(o => o.IsPersistent).ToArray();
				if (releaseStatuses.Length > 1)
				{
					var firstReleaseStatus = releaseStatuses.First().RL_ReleaseStatus;
					result = releaseStatuses.Any(releaseStatus => releaseStatus.RL_ReleaseStatus != firstReleaseStatus)
						? (ZString)EDIReleaseImportEntryStatusList.Codes.MultipleCargoControlNumber : firstReleaseStatus;
				}
				return result;
			}
		}

		ZBool IEDIReleaseMessageAttachee.CargoControlNumbersAwaitingReply
		{
			get
			{
				var result = false;
				var releaseStatuses = new ReleaseStatusCollection(Declaration, false);
				if (releaseStatuses.Count > 1)
				{
					var lastSentMessage = Messages.GetLastMessage(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.EDIRelease, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent);
					if (lastSentMessage != null)
					{
						var requestDate = lastSentMessage.EM_SystemCreateTimeUtc;
						result = releaseStatuses.Cast<ReleaseStatus>().Any(releaseStatus => releaseStatus.IsAwaitingReply(requestDate));
					}
				}
				return result;
			}
		}

		#endregion

		#region IK84ReportAttachee Members

		ZDateTime IK84ReportAttachee.StatementDate
		{
			get { return Declaration.CA_K84StatementDate; }
			set { Declaration.CA_K84StatementDate = value; }
		}

		ZDateTime IK84ReportAttachee.AccountingDate
		{
			get { return Declaration.CA_K84AccountingDate; }
			set { Declaration.CA_K84AccountingDate = value; }
		}

		ZDateTime IK84ReportAttachee.ConfirmedDate
		{
			get { return Declaration.CA_ConfirmedDate; }
			set { Declaration.CA_ConfirmedDate = value; }
		}

		ZDateTime IK84ReportAttachee.B2AcceptedDate
		{
			get { return Declaration.CA_B2AcceptedDate; }
			set { Declaration.CA_B2AcceptedDate = value; }
		}

		ZString IK84ReportAttachee.MessageType
		{
			get { return Declaration.JE_MessageType; }
		}

		#endregion

		#region IB3MessageProcessorLinkedObject Members

		ZDateTime IB3MessageProcessorLinkedObject.EntryReleaseDate
		{
			get => CH_EntryReleaseDate;
			set
			{
				CH_EntryReleaseDate = value;
				var declaration = Declaration;
				var isCSAImporter = (OrgImpAddInfo.Get(declaration.Importer))?.ZO_IsCSAApprovedImporter ?? false;
				if (IsB3CorCAD && isCSAImporter)
				{
					declaration.CA_K84AccountingDate = value;
				}
			}
		}

		void IB3MessageProcessorLinkedObject.CancelScheduledB3Message()
		{
			CancelAndDeactivateDeferredB3Message();
		}

		void IB3MessageProcessorLinkedObject.CancelB3LateSendingWarningEvent()
		{
			if (Declaration is JobDeclaration declaration)
			{
				declaration.CancelAllSystemB3LateSendingWarningEvent();
			}
		}

		void IB3MessageProcessorLinkedObject.AddDocumentsToGeneratorQueue()
		{
			if (CheckCDGServiceTaskIsRunning())
			{
				Customs.Business.CustomsStmProcessQueueLoader.LoadOrCreate(this, Customs.Business.CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, CusEntryHeader.B3AsAccountedActionCode);
				Customs.Business.CustomsStmProcessQueueLoader.LoadOrCreate(this, Customs.Business.CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, CusEntryHeader.CACustomsInvoiceActioCode);
			}
		}

		static bool CheckCDGServiceTaskIsRunning()
		{
			return ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask("CDG") == ServiceTaskStatus.AtLeastOneHostIsRunningHealthily;
		}

		#endregion
	}
}
