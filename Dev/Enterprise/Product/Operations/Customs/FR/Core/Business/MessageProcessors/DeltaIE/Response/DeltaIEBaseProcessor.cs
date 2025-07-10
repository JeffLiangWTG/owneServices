using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class DeltaIEBaseProcessor<T> : ApplicationTypeMessageProcessor
	{
		protected DeltaIEBaseProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override ZQuery MessageFilterCore
		{
			get
			{
				var messageFilter = base.MessageFilterCore;
				messageFilter.AddToFilter(EDIMessageSchema.EM_MessageSubType, GetMessageSubType());
				return messageFilter;
			}
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR DeltaIE Base Processor";

		protected sealed override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.DEC };

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.FRCustomsMessage;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var messageText = message.EM_MessageText;
			var messageObject = JsonHelper.DeserializeMessageWithNeverRequiredContractResolver<T>(messageText);

			if (messageObject != null)
			{
				var (header, errorMessage) = GetLinkedCusEntryHeader(message.Factory, messageObject, message.GetCountryCodeSafe());
				if (header != null)
				{
					message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
					message.EM_LinkedObject = header;
					var isFirstTimeProcessing = header.CH_EntryReleaseDate.IsEmpty;

					UpdateDeclaration(header, messageObject);
					DoExtraProcessing(header, message, isFirstTimeProcessing);
				}
				else
				{
					message.EM_Status = EDIMessageStatusList.Codes.Discarded;
					var note = message.Notes.AddNew();
					note.ST_NoteText = errorMessage;
				}
			}
		}

		protected virtual void UpdateDeclaration(CusEntryHeader entryHeader, T messageObject)
		{
			var crn = GetNewCRN(messageObject);
			if (!string.IsNullOrEmpty(crn))
			{
				if (entryHeader.CRN.IsEmpty)
				{
					entryHeader.CRN = crn;
					var issueDate = GetNewIssueDate(messageObject);
					var newCrnEntryNum = entryHeader.CRNEntryNumber;
					if (!issueDate.IsEmpty && (newCrnEntryNum.CE_IssueDate.IsEmpty || newCrnEntryNum.CE_IssueDate < issueDate))
					{
						newCrnEntryNum.CE_IssueDate = issueDate;
					}
				}

				var expiryDate = GetNewExpiryDate(messageObject);
				var crnEntryNum = entryHeader.CRNEntryNumber;
				if (!expiryDate.IsEmpty && (crnEntryNum.CE_ExpiryDate.IsEmpty || crnEntryNum.CE_ExpiryDate < expiryDate))
				{
					crnEntryNum.CE_ExpiryDate = expiryDate;
				}
			}

			var mrn = GetNewMRN(messageObject);
			if (!string.IsNullOrEmpty(mrn) && entryHeader.MRN.IsEmpty)
			{
				entryHeader.MRN = mrn;

				var issueDate = GetNewIssueDate(messageObject);
				var mrnEntryNum = entryHeader.MRNEntryNumber;
				if (!issueDate.IsEmpty && (mrnEntryNum.CE_IssueDate.IsEmpty || mrnEntryNum.CE_IssueDate < issueDate))
				{
					mrnEntryNum.CE_IssueDate = issueDate;
				}
			}

			var messageStatus = GetNewMessageStatus();
			if (!messageStatus.IsEmpty)
			{
				entryHeader.CH_Status = GetNewMessageStatus();
			}
			var entryStatus = GetNewEntryStatus();
			if (!entryStatus.IsEmpty && entryStatus != entryHeader.CH_EntryStatus)
			{
				entryHeader.CH_EntryStatus = entryStatus;

				var eventTimeString = GetEntryStatusChangedTimeString(messageObject);
				ZDateTime.TryParseExact(eventTimeString, out var eventTime, (NoResString)"yyyy-MM-ddTHH:mm:ss");
				entryHeader.Logs.AddNew(Events.CustomsEntryStatus, $"{entryStatus}", eventTime.IsValid ? new ZDateTimeOffset(eventTime) : ZDateTimeOffset.Now);
			}

			var entryNumber = GetEntryNumber(messageObject);
			if (!string.IsNullOrEmpty(entryNumber))
			{
				entryHeader.EntryNumber = entryNumber;
			}

			var cusEntryNumber = entryHeader.CusEntryNumber;
			if (cusEntryNumber != null && cusEntryNumber.CE_IssueDate.IsEmpty)
			{
				cusEntryNumber.CE_IssueDate = GetNewCustomsEntryIssueDate(messageObject);
			}

			var releaseDate = GetNewReleaseDate(messageObject);
			if (!releaseDate.IsEmpty && (entryHeader.CH_EntryReleaseDate.IsEmpty || entryHeader.CH_EntryReleaseDate < releaseDate))
			{
				entryHeader.CH_EntryReleaseDate = releaseDate;
			}

			UpdateFees(entryHeader, messageObject);
			SetEvent(entryHeader, messageObject);
		}

		protected virtual void UpdateFees(CusEntryHeader entryHeader, T messageObject) { }

		protected void DeleteConfirmedFees(CusEntryHeader entryHeader)
		{
			entryHeader.ConfirmedCharges.RemoveAndDeleteAll();

			foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
			{
				entryLine.ConfirmedFees.RemoveAndDeleteAll();
			}
		}

		protected void UpdateEntryCharges<TSummary>(
			CusEntryHeader entryHeader,
			IEnumerable<TSummary> dutiesAndTaxesSummaries,
			Func<TSummary, string> getTaxType,
			Func<TSummary, double> getChargeAmount,
			Func<TSummary, string> getNationalTaxType,
			Func<TSummary, string> getTaxStatus)
		{
			if (dutiesAndTaxesSummaries != null)
			{
				foreach (var charge in dutiesAndTaxesSummaries)
				{
					var entryHeaderCharge = entryHeader.ConfirmedCharges.AddNew();
					entryHeaderCharge.C1_ChargeType = getTaxType(charge);
					entryHeaderCharge.C1_ChargeAmount = getChargeAmount(charge);
					entryHeaderCharge.NationalFeeTypeCode = getNationalTaxType(charge);
					entryHeaderCharge.TaxStatus = getTaxStatus(charge);
				}
			}
		}

		protected void UpdateEntryGuaranteeAmount(CusEntryHeader entryHeader, double? guaranteedAmount)
		{
			if (guaranteedAmount.HasValue)
			{
				entryHeader.CH_ConfirmedGuaranteeAmount = guaranteedAmount.Value;
			}
		}

		protected virtual void DoExtraProcessing(CusEntryHeader entryHeader, EDIMessage inboundMessage, bool isFirstTimeProcessing)
		{
		}

		protected virtual ZString GetNewCRN(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewMRN(T messageObject) => ZString.Empty;

		protected virtual ZDateTime GetNewIssueDate(T messageObject) => ZDateTime.Empty;

		protected virtual ZDateTime GetNewExpiryDate(T messageObject) => ZDateTime.Empty;

		protected abstract ZString GetLRNFromResponseMessage(T messageObject);

		protected abstract ZString GetNewMessageStatus();

		protected abstract ZString GetNewEntryStatus();

		protected virtual ZString GetEntryStatusChangedTimeString(T messageObject) => ZString.Empty;

		protected abstract ZString GetMessageSubType();

		protected virtual ZString GetEntryNumber(T messageObject) => ZString.Empty;

		protected virtual ZDateTime GetNewCustomsEntryIssueDate(T messageObject) => ZDateTime.Empty;

		protected virtual ZDateTime GetNewReleaseDate(T messageObject) => ZDateTime.Empty;

		protected (CusEntryHeader EntryHeader, ZString ErrorMessage) GetLinkedCusEntryHeader(BusinessObjectFactory factory, T messageObject, ZString country)
		{
			CusEntryNumber entryNumber = null;

			if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.LocalReferenceNumber)
			{
				var lrn = GetLRNFromResponseMessage(messageObject);
				if (!lrn.IsEmpty)
				{
					entryNumber = GetCusEntryNumberFromLocatingReference(factory, lrn, country);
				}
			}
			if (entryNumber == null)
			{
				if (EntryHeaderLocatingReferenceType() == CusEntryNumberTypes.Standard.MovementReferenceNumber)
				{
					var mrn = GetNewMRN(messageObject);
					if (!mrn.IsEmpty)
					{
						entryNumber = GetCusEntryNumberFromLocatingReference(factory, mrn, country);
					}
				}
			}

			return ((CusEntryHeader)entryNumber?.Parent, EntryHeaderErrorMessage());
		}

		protected CusEntryNumber GetCusEntryNumberFromLocatingReference(BusinessObjectFactory factory, ZString locatinReference, ZString country) => CusEntryNumber.Load(factory, EntryHeaderLocatingReferenceType(), locatinReference, country).FirstOrDefault();

		protected virtual void SetEvent(CusEntryHeader entryHeader, T messageObject) { }

		protected ZDateTime ParseUtcDateTimeString(string utcDateStr)
		{
			var result = ZDateTime.Empty;

			if (!string.IsNullOrEmpty(utcDateStr))
			{
				if (utcDateStr.Contains("T"))
				{
					var datetimeSafe = new ZString(utcDateStr).Replace("T", " ").Replace("-", "/");
					ZDateTime.TryParseExact(datetimeSafe, out result, (NoResString)"yyyy/MM/dd HH:mm:ss");
				}
				else
				{
					var datetimeSafe = new ZString(utcDateStr).Replace("-", "/");
					ZDateTime.TryParseExact(datetimeSafe, out result, "yyyy/MM/dd");
				}
			}
			return result;
		}

		protected virtual ZString EntryHeaderLocatingReferenceType() => CusEntryNumberTypes.Standard.LocalReferenceNumber;

		protected ZString EntryHeaderErrorMessage()
		{
			return $"Couldn't locate Job using provided {EntryHeaderLocatingReferenceType()}#";
		}

		protected ZString GetEmailsubject(string declaration, string failingRegister)
		{
			var registerReference = failingRegister != ZString.Empty ? (ZString)Res.GetString("0135E5C9-857C-4A43-90B4-5DA0C90ACD19", " IST Reference: {0}", failingRegister) : ZString.Empty;
			var declarationReference = declaration != ZString.Empty ? (ZString)Res.GetString("1EE49ABE-084D-4F0D-BB83-36F9DB472AA4", " Declaration: {0}", declaration) : ZString.Empty;
			return Res.GetString("44715E53-B0CC-476D-AB1E-9D7B827D1731", "Warning: New Delta I response received.{0}{1}", declarationReference, registerReference);
		}

		protected ZString GetEmailBody(string messageBody) => Res.GetString("873219EE-FFFB-4570-AB85-92B4FB39C580", "A Delta I response has been received. {0}", messageBody);

		protected IRegistryItem GetEmailGroupRegistryItem() => FRCustomsDataRegistry.Instance.DeltaGResponseNotificationGroup;
	}
}
