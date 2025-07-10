using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMDN_AH;
using CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARMDN_CB;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Customs.CA.Business
{
	public class CARMDailyNoticeDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		public CARMDailyNoticeDocumentWrapper(CusStatementHeader header)
		{
			this.header = header;
			if (header.B2_StatementType == CusStatementHeaderTypes.Codes.Importer)
			{
				CalculateImporterProperties();
			}
			else if (header.B2_StatementType == CusStatementHeaderTypes.Codes.Broker)
			{
				CalculateBrokerProperties();
			}
		}
		readonly CusStatementHeader header;

		void CalculateImporterProperties()
		{
			var lastMessage = header.Messages.LastOrDefault() as EDIMessage;
			isImporter = true;
			if (lastMessage != null)
			{
				var descrilializedMessage = XmlObjectSerializer.Deserialize<Zcarmdnoticeah>(lastMessage.EM_MessageText);
				if (descrilializedMessage?.Header?.Party is CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARM_TYPES.PartyType party)
				{
					OperatingName = party.OpName;
					ProgramName = party.Zzname;
					ProgramAccountNum = party.Account;
					ProgramType = party.ProgType?.Psobtyp ?? string.Empty;
				}
				if (descrilializedMessage?.Summary?.RevDist is CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARM_TYPES.ChargeBreakdownType rev)
				{
					Duties = rev.Duties;
					ExciseTax = rev.Excise;
					ExciseDuties = rev.Exciseduties;
					SIMA = rev.Sima;
					GST = rev.Gst;
					HST = rev.Hst;
					PST = rev.Pst;
					Interest = rev.Interest;
					Penalties = rev.Penalties;
					Payments = rev.Payments;
					Others = rev.Payments;
				}
			}

			var list = new List<DailyNoticeDetailsWrapper>();
			foreach (CusStatementLine line in header.StatementLines)
			{
				list.Add(new DailyNoticeDetailsWrapper(line, header.Factory));
			}

			importerDailyNoticeDetails = new BusinessObjectCollectionWrapper<DailyNoticeDetailsWrapper>(list);
		}

		void CalculateBrokerProperties()
		{
			var lastBrokerMessage = header.Messages.LastOrDefault() as EDIMessage;
			isBroker = true;
			if (lastBrokerMessage != null)
			{
				var descrilializedMessage = XmlObjectSerializer.Deserialize<Zcarmdnoticecb>(lastBrokerMessage.EM_MessageText);
				if (descrilializedMessage?.Header?.Party is CargoWise.Customs.CA.MessageDefinitions.CAD.ZCARM_TYPES.BrokerPartyType party)
				{
					LicenseNumber = party.Idnumber;
					OperatingName = party.OpName;
				}
				if (descrilializedMessage.Summary != null)
				{
					TotalImportationAmount = descrilializedMessage.Summary.TotImp;
				}
			}

			var list = new List<BrokerDailyNoticeImporterWrapper>();
			foreach (CusStatementLineGroup lineGroup in header.LineGroupCollection)
			{
				list.Add(new BrokerDailyNoticeImporterWrapper(lineGroup, header.Factory));
			}

			brokerDailyNoticeDetails = new BusinessObjectCollectionWrapper<BrokerDailyNoticeImporterWrapper>(list);
		}

		public ZBool IsImporterHeader => isImporter;
		ZBool isImporter;

		public ZBool IsBrokererHeader => isBroker;
		ZBool isBroker;

		#region Properties

		public ZDateTime ProcessDate => header.B2_PrintDate;

		public ZString BusinessNumber => header.B2_ImporterCustomsID;

		public ZString LegalName => header.Importer?.OH_FullName ?? ZString.Empty;

		public ZString OperatingName { get; private set; }

		public ZString ProgramName { get; private set; }

		public ZString ProgramAccountNum { get; private set; }

		public ZString LicenseNumber { get; private set; }

		public ZString ProgramType { get; private set; }

		public ZDecimal StatementAmount => header.B2_StatementAmount;

		public ZDecimal PaidAmount => header.B2_PaidAmount;

		public ZDecimal RefundAmount => header.B2_RefundAmount;

		public ZDecimal InterestAmount => header.B2_TotalInterests;

		public ZDecimal OthersAmount => header.B2_TotalOthers;

		public ZDecimal TotalImportationAmount { get; private set; }

		public ZDecimal Duties { get; private set; }

		public ZDecimal ExciseTax { get; private set; }

		public ZDecimal ExciseDuties { get; private set; }

		public ZDecimal SIMA { get; private set; }

		public ZDecimal GST { get; private set; }

		public ZDecimal HST { get; private set; }

		public ZDecimal PST { get; private set; }

		public ZDecimal Interest { get; private set; }

		public ZDecimal Penalties { get; private set; }

		public ZDecimal Payments { get; private set; }

		public ZDecimal Others { get; private set; }

		#endregion

		#region DETAILS

		public BusinessObjectCollectionWrapper<DailyNoticeDetailsWrapper> ImporterDailyNoticeDetails => importerDailyNoticeDetails;
		BusinessObjectCollectionWrapper<DailyNoticeDetailsWrapper> importerDailyNoticeDetails;

		public BusinessObjectCollectionWrapper<BrokerDailyNoticeImporterWrapper> BrokerDailyNoticeDetails => brokerDailyNoticeDetails;
		BusinessObjectCollectionWrapper<BrokerDailyNoticeImporterWrapper> brokerDailyNoticeDetails;

		#endregion

		#region NOTES

		public ZString DailyNoticeNotes
		{
			get
			{
				if (!dailyNoticeNotes.HasValue)
				{
					var headerNotes = header.Notes;
					var recipientNotes = headerNotes.FindByDescription(StatementMessageProcessorHelper.EnglishMessageToRecipient).Union(headerNotes.FindByDescription(StatementMessageProcessorHelper.FrenchMessageToRecipient));

					dailyNoticeNotes = string.Join("\r\n", recipientNotes.Select(x => x.ST_NoteDataAsText));
				}
				return dailyNoticeNotes.Value;
			}
		}
		ZString? dailyNoticeNotes;

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => header.PK;
	}
}
