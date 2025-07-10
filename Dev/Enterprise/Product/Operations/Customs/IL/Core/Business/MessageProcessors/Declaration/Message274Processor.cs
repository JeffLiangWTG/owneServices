using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	public class Message274Processor : ILBranchCustomsApplicationTypeMessageProcessorBase
	{
		public Message274Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("00868244-2B67-4896-9D80-1D3C4EE33F7F", "IL Import Declaration Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { ILMessageTypeList.Codes.DEC };

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is ILDEC274ResponseMessage ilDecMessage
				&& ilDecMessage.MessageDataObject is ILDEC274ResponseMessageDataObject messageDataObject
				&& messageDataObject.GetImportDeclarationResponseAdaptor() is ImportDeclarationResponseAdaptor responseProvider)
			{
				var externalDeclarationID = responseProvider.ExternalDeclarationID;
				var linkedEntryHeader = GetLinkedEntryHeader(ilDecMessage, externalDeclarationID);

				if (!responseProvider.IsResponsePresent)
				{
					UpdateMessageStatus(ilDecMessage, EDIMessage.Status.ProcessedOK, noteText: Res.GetString("D27E63F9-0A3B-4183-BD5B-BC86688BCB83", "Declaration data was not updated since message was rejected by customs"));
					return;
				}
				var message274EntryLineFeesProcessor = new Message274TaxProcessor(linkedEntryHeader);
				message274EntryLineFeesProcessor.UpdateEntryLineConfirmedFees(responseProvider.GoodsShipmentList);
				message274EntryLineFeesProcessor.UpdateConfirmedCharges(responseProvider.DeclarationDutyTaxFeeList);

				UpdateCustomsStatus(linkedEntryHeader, responseProvider.CustomsStatusNameCode);
				UpdateDeclarationVersionID(linkedEntryHeader, responseProvider.DeclarationVersionID);
				UpdateCustomsDeclarationNumber(linkedEntryHeader, responseProvider.CustomsDeclarationNumber);
				UpdateMessageStatus(ilDecMessage, EDIMessage.Status.ProcessedOK);
			}
		}

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObjectCore(EDIMessage message)
		{
			var ilDecMessage = message as ILDEC274ResponseMessage;
			if (ilDecMessage == null)
			{
				return (message.Branch.PK, null, Constants.MessageProcessors.UnexpectedMessage);
			}

			var responseAdaptor = ((ILDEC274ResponseMessageDataObject)ilDecMessage.MessageDataObject).GetImportDeclarationResponseAdaptor();

			var externalDeclarationID = responseAdaptor.ExternalDeclarationID;
			if (externalDeclarationID.IsEmpty)
			{
				return (message.Branch.PK, null, Constants.Message274Processor.MessageInvalid);
			}

			var linkedEntryHeader = GetLinkedEntryHeader(ilDecMessage, externalDeclarationID);

			if (linkedEntryHeader == null)
			{
				return (message.Branch.PK, null, Constants.Message274Processor.GetCouldNotLocateEntryWithReference(externalDeclarationID));
			}

			var taxProcessor = new Message274TaxProcessor(linkedEntryHeader);
			if (!taxProcessor.DoEntryLineKeysMatch(responseAdaptor.GoodsShipmentList))
			{
				return (message.Branch.PK, null, Constants.Message274Processor.EntryLineFeeKeysDontMatch);
			}

			return (linkedEntryHeader.Branch.PK, linkedEntryHeader, (NoResString)ZString.Empty);
		}

		CusEntryHeader GetLinkedEntryHeader(ILDEC274ResponseMessage ilDecMessage, string externalDeclarationID)
		{
			if (ilDecMessage.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				return entryHeader;
			}

			return !externalDeclarationID.IsNullOrEmpty()
				? LoadEntryHeader(ilDecMessage.Factory, externalDeclarationID, ilDecMessage.GetCountryCodeSafe())
				: null;
		}

		void UpdateCustomsDeclarationNumber(CusEntryHeader loadedEntryHeader, string customsDeclarationNumber)
			=> loadedEntryHeader.EntryNumber = customsDeclarationNumber;

		void UpdateDeclarationVersionID(CusEntryHeader loadedEntryHeader, string declarationVersionID)
		{
			loadedEntryHeader.CH_VersionID = DeclarationVersionIDConverter.ToEntryHeaderVersionID(declarationVersionID);
		}

		void UpdateCustomsStatus(CusEntryHeader loadedEntryHeader, string nameCode)
		{
			loadedEntryHeader.CH_EntryStatus = nameCode;
			loadedEntryHeader.CH_Status = EDIMessageStatusList.Codes.Acknowledged;
		}

		CusEntryHeader LoadEntryHeader(BusinessObjectFactory factory, ZString bgmReference, ZString countryCode)
		{
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_RN_NKCountryCode, countryCode);

			var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			jobDeclarationQuery.AddSubQuery(branchQuery, JoinCondition.And);

			var entryQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			entryQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, bgmReference);
			entryQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);

			return factory.LoadTop1<CusEntryHeader>(entryQuery);
		}
	}
}
