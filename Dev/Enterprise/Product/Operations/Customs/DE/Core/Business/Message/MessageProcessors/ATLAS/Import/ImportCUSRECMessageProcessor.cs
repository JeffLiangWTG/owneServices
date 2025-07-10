using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportCUSRECMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<ICUSREC>, ICUSREC>
	{
		public ImportCUSRECMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("6C165F1C-9AD4-4C58-A8A3-6A5485BF6C89", "Import CUSREC Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSREC> message)
		{
			BusinessObject result = null;
			var dataProvider = message.DataProvider;
			if (dataProvider != null)
			{
				var referencedMessageIdentifier = dataProvider.ReferencedMessageIdentifier;
				if (!referencedMessageIdentifier.IsEmpty)
				{
					result = GetLinkedObjectFromOriginalMessage(message.Factory, referencedMessageIdentifier);
				}
				else
				{
					result = GetEntryHeaderFromMRN(message.Factory, dataProvider.MRN, dataProvider.ReferenceNumber);
				}
			}
			return result;
		}

		protected override bool UpdateWarehouseCore
		{
			get
			{
				var entryHeader = CurrentMessage?.EM_LinkedObject as CusEntryHeader;
				if (entryHeader != null)
				{
					var entryInstruction = entryHeader.EntryInstruction;
					var dataProvider = CurrentMessage?.DataProvider;
					var referenceNumber = dataProvider != null ? dataProvider.ReferenceNumber : string.Empty;
					var referenceNumberPrefix = referenceNumber.LeftEmptyIfNull(3);
					var entryHeaderIsIntoWarehouseWarehousing = entryHeader.IsIntoWarehouseWarehousing && !entryInstruction.CEI_OA_Warehouse2.IsEmpty;
					var declaration = entryHeader.Declaration;
					var declarationIsOutwardOrderImported = declaration.IsOutwardOrderImported && IsATEWithAuthorisationREL1(referenceNumberPrefix, entryInstruction);
					return entryHeader.CH_EntryStatus == UniversalReferenceConstants.EntryStatus.TX7 && (entryHeaderIsIntoWarehouseWarehousing || entryHeader.IsOutOfWarehouseWarehousing || declarationIsOutwardOrderImported)
						|| (declaration.IsWarehouseAdjustment && entryHeader.CH_EntryStatus == UniversalReferenceConstants.EntryStatus.RC2 && declaration.AtLeastOneEntryInstructionHasFromWarehouse);
				}
				else
				{
					return false;
				}
			}
		}

		protected override bool ShouldPublishWhsOutwardAcceptEventCore(CusEntryHeader entryHeader)
		{
			var dataProvider = CurrentMessage?.DataProvider;
			var referenceNumber = dataProvider != null ? dataProvider.ReferenceNumber : string.Empty;
			var referenceNumberPrefix = referenceNumber.LeftEmptyIfNull(3);
			return entryHeader.CH_EntryStatus == UniversalReferenceConstants.EntryStatus.TX7 && IsATEWithAuthorisationREL1(referenceNumberPrefix, entryHeader.EntryInstruction);
		}

		bool IsATEWithAuthorisationREL1(ZString referenceNumberPrefix, CusEntryInstruction entryInstruction) => entryInstruction != null &&
																												referenceNumberPrefix == UniversalReferenceConstants.ATLASReferenceNumberIdentifier.ATE &&
																												entryInstruction.HasCusAuthorizationUsageWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSREC> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			var entryInstruction = entryHeader.EntryInstruction;
			var cusReconEntry = entryHeader.GetCusReconEntry();
			var dataProvider = message.DataProvider;
			var referenceNumber = dataProvider.ReferenceNumber;
			var mrn = dataProvider.MRN;
			var isHeaderError = dataProvider.NotificationSeverity.Any(x => x == NotificationTypeList.Codes.Error) || (string.IsNullOrEmpty(referenceNumber) && string.IsNullOrEmpty(mrn));
			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });

			message.EM_Status = EDIMessage.Status.ProcessedOK;

			if (isHeaderError)
			{
				entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.REJ;
				entryHeader.CH_Status = EDIMessage.Status.Rejected;

				if (entryHeader.MovementReferenceNumber.IsEmpty)
				{
					cusReconEntry?.Delete();
				}
			}
			else
			{
				var cusEntryNum = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				cusEntryNum.CE_EntryNum = referenceNumber ?? mrn;
				cusEntryNum.CE_IssueDate = dataProvider.RegistrationDate;

				var referenceOrMrnIsTemporary = RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber(referenceNumber) ||
					RegistrationNumberValidationHelper.IsRegistrationNumberAWorkingNumber(mrn);
				entryHeader.CH_EntryStatus = referenceOrMrnIsTemporary ? UniversalReferenceConstants.EntryStatus.RC1 : UniversalReferenceConstants.EntryStatus.RC2;
				entryHeader.CH_Status = EDIMessage.Status.Received;

				if (cusReconEntry != null && IsATEWithAuthorisationREL1(referenceNumber.LeftEmptyIfNull(3), entryInstruction))
				{
					UpdateCusReconEntryAndLines(cusReconEntry, referenceNumber, entryInstruction.CEI_LocalClearanceDate.Date);
					UpdateCusEntryAndLines(entryHeader);
					BondedWarehousingHelper.SetWarehouseTransactionStatusForImportMessageProcessing(entryHeader);
				}

				if (entryHeader.Declaration.JE_MessageType == DEJobMessageTypeList.Codes.WarehouseAdjustment)
				{
					foreach (var goodsItem in dataProvider.GoodsItems)
					{
						if (int.TryParse(goodsItem.SequenceNumber, out var seqenceNumber))
						{
							if (entryHeader.AllEntryLines.SingleOrDefault(l => l.CL_LineNumber == seqenceNumber) is CusEntryLine entryLine)
							{
								entryLine.ZG_CustomsStatus = rejectSeverities.Contains(goodsItem.NotificationSeverity) ? UniversalReferenceConstants.EntryStatus.RL4 : UniversalReferenceConstants.EntryStatus.RC2;
							}
						}
					}
					entryHeader.AllEntryLines.Where(el => el.ZG_CustomsStatus.IsEmpty).ForEach(el => el.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RC2);
				}
			}

			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, entryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());
			var declaration = entryHeader.Declaration;
			SkipSnapshotUpdate(declaration);
			SendEmail(message, declaration, entryHeader, dataProvider);
		}

		void SendEmail(AtlasInboundEDIMessage<ICUSREC> message, JobDeclaration declaration, CusEntryHeader entryHeader, ICUSREC dataProvider)
		{
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, declaration
				, Res.GetString("E04F7EE1-25FF-4CC4-B600-07ECD3D35A24", "Import CUSREC – Customs Receipt Message")
				, GetEmailBody(declaration, dataProvider)
				, false
				, message.Branch
				, declaration
				, () => entryHeader.Messages.LastSentOutgoingMessage);
		}

		static void UpdateCusReconEntryAndLines(CusReconEntry cusReconEntry, ZString referenceNumber, ZDate localClearanceDate)
		{
			cusReconEntry.CRE_OriginalEntryNumber = referenceNumber;
			cusReconEntry.CRE_EntryDate = localClearanceDate;
			cusReconEntry.Logs.AddNew(Events.CustomsEntryStatus, UniversalReferenceConstants.EntryStatus.TX7, ZDateTime.Now.ToOffset());

			foreach (var line in cusReconEntry.CusReconEntryLines)
			{
				line.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX7;
			}
		}

		void UpdateCusEntryAndLines(CusEntryHeader entry)
		{
			entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX7;

			foreach (var line in entry.AllEntryLines.Cast<CusEntryLine>())
			{
				line.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX7;
			}
		}

		static ZString GetEmailBody(JobDeclaration declaration, ICUSREC provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("FBEE3CCE-2D9E-4517-96CB-D7567D4B8571", "Your Import Declaration for Job {0} received a Customs Receipt Message. For details please follow the link to the job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var referenceNumber = provider.ReferenceNumber;
			var mrn = provider.MRN;
			var localReferenceNumber = provider.LocalReferenceNumber;
			var registrationDate = provider.RegistrationDate;

			if (!string.IsNullOrEmpty(referenceNumber) || !string.IsNullOrEmpty(mrn) || !localReferenceNumber.IsEmpty || !registrationDate.IsEmpty)
			{
				var tableCreator = new HtmlTableCreator();
				if (!string.IsNullOrEmpty(mrn))
				{
					tableCreator.WriteRow(Res.GetString("26DD0F76-B8DF-482B-95EA-C9DE76153562", "MRN"), mrn);
				}

				if (!string.IsNullOrEmpty(referenceNumber))
				{
					tableCreator.WriteRow(Res.GetString("6C7A2416-6873-49CC-842C-CB1A4704119B", "Registration Number"), referenceNumber);
				}

				if (!localReferenceNumber.IsEmpty)
				{
					tableCreator.WriteRow(Res.GetString("33666D5C-4A1B-47F1-9BA5-9EF4C369B128", "Local Reference Number"), localReferenceNumber);
				}

				if (!registrationDate.IsEmpty)
				{
					tableCreator.WriteRow(Res.GetString("67508418-A23E-4FAF-8D2A-3A9FB987A604", "Registration Date"), registrationDate.ToString("dd.MM.yyyy"));
				}
				htmlBody.Append(tableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}

		static readonly HashSet<string> rejectSeverities = new HashSet<string>() { NotificationTypeList.Codes.Warning, NotificationTypeList.Codes.Error };
	}
}
