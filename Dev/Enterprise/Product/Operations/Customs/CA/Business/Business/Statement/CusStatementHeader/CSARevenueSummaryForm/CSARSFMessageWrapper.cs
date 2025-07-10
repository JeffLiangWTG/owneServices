using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFMessageWrapper : ICSARevenueSummaryForm
	{
		public CSARSFMessageWrapper(CusStatementHeader cusStatementHeader)
		{
			this.cSARSF = cusStatementHeader;
			this.previousSubmisstionDate = cusStatementHeader.B2_DueDate;
			this.previousStatus = cusStatementHeader.B2_Status;
		}

		readonly CusStatementHeader cSARSF;
		readonly ZDateTime previousSubmisstionDate;
		readonly ZString previousStatus;

		#region Functions

		public void PreProcessBeforeSendMessage(MessageSubTypes messageSubType)
		{
			cSARSF.B2_DueDate = ZDateTime.Now;
			if (messageSubType == MessageSubTypes.Create)
			{
				cSARSF.B2_Status = MessageStatusList.Codes.AwaitingOriginal;
			}
			else if (messageSubType == MessageSubTypes.Change)
			{
				cSARSF.B2_Status = MessageStatusList.Codes.AwaitingChange;
			}
			else
			{
				cSARSF.B2_Status = MessageStatusList.Codes.Unknown;
			}
		}

		public void UndoPreProcess()
		{
			cSARSF.B2_DueDate = previousSubmisstionDate;
			cSARSF.B2_Status = previousStatus;
		}

		#endregion

		#region ICAEDIFACTMessageAttachee

		bool ICAEDIFACTMessageAttachee.IsCancelled => false;

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get
			{
				return messageStatus.IsValid ? messageStatus : ZString.Empty;
			}
			set
			{
				messageStatus = value;
			}
		}
		ZString messageStatus;
		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get
			{
				return jobStatus.IsValid ? jobStatus : ZString.Empty;
			}
			set
			{
				jobStatus = value;
			}
		}
		ZString jobStatus;

		bool IEDIFACTMessageAttachee.HasChanges => false;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		ZString IEDIFACTMessageAttachee.JobIdentification => MessageTypeList.Codes.CSARevenueSummaryForm + "-" + cSARSF.B2_StatementNumber;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => cSARSF;

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new Enterprise.Messaging.Business.EDIMessageCollection(cSARSF, cSARSF.Factory);
				}
				return messages;
			}
		}
		Enterprise.Messaging.Business.EDIMessageCollection messages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => cSARSF.Factory;

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
		}

		#endregion

		#region ICSARevenueSummaryForm

		ZDateTime ICSARevenueSummaryForm.DocumentMessageDateTime
		{
			get
			{
				return cSARSF.B2_DueDate;
			}
		}

		ZDateTime ICSARevenueSummaryForm.RSFMonth
		{
			get
			{
				return cSARSF.B2_PeriodEndDate;
			}
		}

		ZDateTime ICSARevenueSummaryForm.PeriodStartDateTime
		{
			get
			{
				return cSARSF.B2_PeriodStartDate;
			}
		}

		ZDateTime ICSARevenueSummaryForm.PeriodEndDateTime
		{
			get
			{
				return cSARSF.B2_PeriodEndDate;
			}
		}

		ZString ICSARevenueSummaryForm.BusinessNumber
		{
			get
			{
				return cSARSF.B2_ImporterCustomsID;
			}
		}

		ZString ICSARevenueSummaryForm.StatementNumber
		{
			get
			{
				return cSARSF.B2_StatementNumber;
			}
		}

		ZDecimal ICSARevenueSummaryForm.VFD
		{
			get
			{
				return ZDecimal.Zero;
			}
		}

		IEnumerable<ICSARSFItem> ICSARevenueSummaryForm.Debits
		{
			get
			{
				return cSARSF.Debits.ToArray().Cast<ICSARSFItem>();
			}
		}

		IEnumerable<ICSARSFItem> ICSARevenueSummaryForm.Credits
		{
			get
			{
				return cSARSF.Credits.ToArray().Cast<ICSARSFItem>();
			}
		}

		IEnumerable<ICSARSFItem> ICSARevenueSummaryForm.InterimPayments
		{
			get
			{
				return cSARSF.InterimPayments.ToArray().Cast<ICSARSFItem>();
			}
		}

		IEnumerable<ICSARSFItem> ICSARevenueSummaryForm.CustomsAssessments
		{
			get
			{
				return cSARSF.CustomsAssessments.ToArray().Cast<ICSARSFItem>();
			}
		}

		ZDecimal ICSARevenueSummaryForm.TotalPayment
		{
			get
			{
				return cSARSF.DebitsTotal + cSARSF.CreditsTotal + cSARSF.InterimPaymentsTotal + cSARSF.CustomsAssessmentsTotal;
			}
		}

		#endregion
	}
}
