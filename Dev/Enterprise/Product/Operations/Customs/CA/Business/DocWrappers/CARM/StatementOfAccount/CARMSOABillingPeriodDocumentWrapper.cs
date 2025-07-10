using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMSOA;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Customs.CA.Business
{
	public class CARMSOABillingPeriodDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		public CARMSOABillingPeriodDocumentWrapper(CusStatementHeader header)
		{
			this.header = header;
			GetLegalNameFromMessageWrapper();
		}
		readonly CusStatementHeader header;

		void GetLegalNameFromMessageWrapper()
		{
			var lastMessage = header.Messages.GetLastMessage(EDIMessage.ApplicationCodes.CACustoms, ARLMessageTypes.Codes.StatementOfAccount, EDIMessage.Direction.Receive);
			if (lastMessage != null)
			{
				var descrilializedMessage = XmlObjectSerializer.Deserialize<Zcarmsoa>(lastMessage.EM_MessageText);
				if (descrilializedMessage?.Header?.Party is CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARM_TYPES.PartyType party)
				{
					LegalName = party.NameOrg1;
				}
			}
		}

		#region Properties

		public ZDateTime BillingPeriodStart => header.B2_PeriodStartDate;

		public ZDateTime BillingPeriodEnd => header.B2_PeriodEndDate;

		public ZString BusinessNumber => header.B2_ImporterCustomsID;

		public ZString LegalName { get; private set; }

		public ZString ProgramType => CARMStatementOfAccountStatementTypeList.GetLongCode(header.B2_StatementType);

		public ZDateTime StatementDate => header.B2_PrintDate;

		public ZDateTime DueDate => header.B2_DueDate;

		public ZDecimal StatementAmount => header.B2_StatementAmount;

		public ZDecimal PreviousStatementBalance => header.PreviousStatementBalance;

		public ZDecimal CorrectionsToPreviousStatementBalance => header.CorrectionsLastBalance;

		public ZDecimal PaymentsReceivedAfterPreviousSOA => header.PaymentsAfterLastSOA;

		public ZDecimal Disburesements => header.Disbursements;

		public ZDecimal InterestAndPenaltiesSumTotal => header.InterestSum;

		public ZDecimal CurrentPeriodCharges => header.CurrentPeriodCharges;

		public ZDecimal CurrentPeriodCredit => header.CurrentPeriodCredits;

		public ZDecimal CurrentStatementBalance => header.Total;

		public ZDecimal Duties => header.Duties;

		public ZDecimal Excise => header.Excise;

		public ZDecimal ExciseDuties => header.ExciseDuties;

		public ZDecimal SIMA => header.SIMA;

		public ZDecimal GST => header.GST;

		public ZDecimal HST => header.HST;

		public ZDecimal PST => header.PST;

		public ZDecimal Payments => header.Payments;

		public ZDecimal Others => header.Others;

		#endregion

		#region Program Account

		public BusinessObjectCollectionWrapper<CARMSOAProgramAccountDocumentWrapper> ProgramAccount
		{
			get
			{
				var list = new List<CARMSOAProgramAccountDocumentWrapper>();
				foreach (var bn15 in header.StatementLines.OfType<CusStatementLine>().DistinctBy(x => x.B3_ImporterCustomsID).Select(x => x.B3_ImporterCustomsID))
				{
					list.Add(new CARMSOAProgramAccountDocumentWrapper(header, bn15, header.Factory));
				}

				return new BusinessObjectCollectionWrapper<CARMSOAProgramAccountDocumentWrapper>(list);
			}
		}

		#endregion

		#region NOTES

		public ZString SOANotes
		{
			get
			{
				if (!sOANotes.HasValue)
				{
					var headerNotes = header.Notes;
					var recipientNotes = headerNotes.FindByDescription(StatementMessageProcessorHelper.EnglishMessageToRecipient).Union(headerNotes.FindByDescription(StatementMessageProcessorHelper.FrenchMessageToRecipient));

					sOANotes = string.Join("\r\n", recipientNotes.Select(x => x.ST_NoteDataAsText));
				}
				return sOANotes.Value;
			}
		}
		ZString? sOANotes;

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => header.PK;
	}
}
