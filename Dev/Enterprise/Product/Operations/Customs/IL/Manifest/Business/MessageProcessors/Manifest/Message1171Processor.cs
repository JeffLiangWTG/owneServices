using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	sealed class Message1171Processor : ILBranchCustomsApplicationTypeMessageProcessorBase
	{
		public Message1171Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("A882C918-C058-464D-B002-6A4FBB1FB0E0", "IL Manifest Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new[] { (ZString)ILMessageTypeList.Codes.MAN };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new[] { (ZString)ILEDIMessageSubTypeList.Codes.ForwarderManifestResponse };

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is ILMAN171ResponseMessage ilManMessage
				&& ilManMessage.MessageDataObject is MessageDataObject<MnMsg4SendManifestFeedBackMessage> messageDataObject
				&& messageDataObject.MessageData is MnMsg4SendManifestFeedBackMessage responseMessage)
			{
				var countryCode = ilManMessage.GetCountryCodeSafe();
				var header = ilManMessage.EM_LinkedObject as AsycudaManifestHeader;
				if (header == null)
				{
					return;
				}

				var requestedSupportingDocumentErrors = GetRequestedSupportingDocument(header, responseMessage) ?? new List<RequestedSupportingDocument>();
				var firstCouldNotLocated = requestedSupportingDocumentErrors.FirstOrDefault(r => r.Bill == null);
				if (firstCouldNotLocated != null)
				{
					return;
				}

				UpdateMessageStatus(header);
				UpdateManifestHeaderCustomsStatus(header, responseMessage, countryCode);
				UpdateBillCustomsStatus(header, responseMessage, countryCode);
				UpdateRequestedSupportingDocument(header, requestedSupportingDocumentErrors);
				ilManMessage.EM_Status = EDIMessage.Status.ProcessedOK;

				base.ProcessMessageCore(ilManMessage);
			}
		}

		protected override ControllerID ControllerIDForEmail => ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest;

		protected override string MessageTypeInSubject => Res.GetString("241DBDEC-3CB8-4124-9CF6-7C915C8F6AC4", "Manifest Message");

		protected override bool ShouldSendNotificationEmail() => ILCustomsDataRegistry.Instance.ILGPMGroupNotification.Value.SendMode != Core.Constants.EmailTo.NoEmails;

		protected override string GetJobNumber(IEDIMessageCollectionOwner owner) => ((AsycudaManifestHeader)owner).AMA_JobReference;

		protected override ZBool GetShouldSendErrorEmailsOnly(IEDIMessageCollectionOwner owner, ILEDIMessage message)
		{
			var header = (AsycudaManifestHeader)owner;
			var branchPK = header.Branch.PK;
			IGlbBranch messageBranch;
			if (branchPK.IsValid)
			{
				messageBranch = message.Factory.Load<IGlbBranch>(branchPK);
			}
			else
			{
				messageBranch = MasterFiles.Business.GlbBranch.CurrentBranch;
			}

			var supportMessageSuppressRegistry = ILCustomsDataRegistry.Instance.ILMANGroupNotification as ISupportMessageSuppressRegistry;
			return supportMessageSuppressRegistry?.ShouldSEndErrorsOnly(messageBranch.GB_GC, messageBranch.PK, ZGuid.Empty) ?? false;
		}

		void UpdateRequestedSupportingDocument(AsycudaManifestHeader header, IEnumerable<RequestedSupportingDocument> requestedSupportingDocuments)
		{
			foreach (var item in requestedSupportingDocuments)
			{
				var bill = item.Bill;

				var validationCode = item.ValidationCode;
				var requestSupportingDocument =
					bill.SupportingDocuments.Cast<CusSupportingInfo>().FirstOrDefault(sd => sd.CSI_Code == validationCode.ListUri)
					?? bill.SupportingDocuments.AddNew();

				requestSupportingDocument.CSI_Code = validationCode.ListUri;
				requestSupportingDocument.CSI_ReferenceNumber2 = validationCode.ListId;
				requestSupportingDocument.CSI_Status = RequestedSupportingStatusList.Codes.REQ;
				requestSupportingDocument.CSI_AdditionalDescription = new ZString(validationCode.Name).Left(requestSupportingDocument.CSI_AdditionalDescriptionInfo.MaxLength);
			}
		}

		static IEnumerable<RequestedSupportingDocument> GetRequestedSupportingDocument(AsycudaManifestHeader header, MnMsg4SendManifestFeedBackMessage responseMessage)
		{
			return responseMessage.Response?.Error
						?.Where(err => err.ValidationCode.Value == IL.Business.Constants.SupportingDocument.CustomValidationCode && err.ValidationCode.ListId != null)
						.Where(err => err.Pointer.Any(p => p.DocumentSectionCode.Value == WCOPointer.Header))
						.Where(err => err.Pointer.Any(p => p.DocumentSectionCode.Value == WCOPointer.Consignment && p.SequenceNumeric.HasValue))
						.Select(err =>
						{
							var billSequenceNumber = err.Pointer.First(p => p.DocumentSectionCode.Value == WCOPointer.Consignment && p.SequenceNumeric.HasValue).SequenceNumeric;
							return new RequestedSupportingDocument()
							{
								BillSequenceNumber = billSequenceNumber,
								ValidationCode = err.ValidationCode,
								Bill = header.Bills.Cast<AsycudaBill>().FirstOrDefault(b => b.ABL_SequenceNumber == billSequenceNumber)
							};
						});
		}

		AsycudaManifestHeader GetLinkedAsycudaManifestHeader(BusinessObjectFactory factory, MnMsg4SendManifestFeedBackMessage responseMessage, ZString countryCode)
		{
			var manifestNumber = responseMessage.Response?.FunctionalReferenceId?.Value;

			if (!manifestNumber.IsNullOrEmpty() && manifestNumber.StartsWith("I") && manifestNumber.Length == 16 && manifestNumber.Substring(1).All(c => char.IsDigit(c)))
			{
				return FindRoadManifest(factory, countryCode, manifestNumber);
			}

			var firstIL2IdValue = responseMessage.Response
				?.Declaration
				?.Consignment?.FirstOrDefault()
				?.TransportContractDocument
				?.FirstOrDefault(x => x.TypeCode.Value == Constants.IsraeliCustoms.ManifestTransportContractDocumentId)?.Id?.Value;

			if (manifestNumber.IsEmpty() || firstIL2IdValue.IsEmpty())
			{
				return null;
			}

			return FindOceanManifest(factory, countryCode, manifestNumber, firstIL2IdValue);
		}

		static AsycudaManifestHeader FindOceanManifest(BusinessObjectFactory factory, ZString countryCode, string manifestNumber, string firstIL2IdValue)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, countryCode);
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestNumber, manifestNumber);

			var billSubQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);

			var csiSubQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, AsycudaBillSchema.Constants.Prefix);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_SubType, AdditionalInfoSubTypeList.Codes.TransportDocument);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Code, Constants.IsraeliCustoms.ManifestTransportContractDocumentId);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, firstIL2IdValue);

			billSubQuery.AddSubQuery(csiSubQuery, JoinCondition.And);

			headerQuery.AddSubQuery(billSubQuery, JoinCondition.And);

			return factory.LoadTop1<AsycudaManifestHeader>(headerQuery);
		}

		static AsycudaManifestHeader FindRoadManifest(BusinessObjectFactory factory, ZString countryCode, string manifestNumber)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, countryCode);
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestNumber, manifestNumber);
			return factory.LoadTop1<AsycudaManifestHeader>(headerQuery);
		}

		void UpdateMessageStatus(AsycudaManifestHeader header)
			=> header.AMA_MessageStatus = PNTSMessageStatusList.Codes.Acknowledged;

		void UpdateManifestHeaderCustomsStatus(AsycudaManifestHeader header, MnMsg4SendManifestFeedBackMessage responseMessage, ZString countryCode)
		{
			var statusNameCode = responseMessage.Response?.Status.FirstOrDefault(sts => sts.Pointer?.Any(p => p.DocumentSectionCode.Value == WCOPointer.Header) ?? false)?.NameCode?.Value;
			if (!statusNameCode.IsEmpty())
			{
				header.RegistrationStatus = statusNameCode;
			}
		}

		void UpdateBillCustomsStatus(AsycudaManifestHeader header, MnMsg4SendManifestFeedBackMessage responseMessage, ZString countryCode)
		{
			foreach (var bill in header.Bills)
			{
				var statusNameCode = responseMessage.Response?.Status
					.FirstOrDefault(sts => sts.Pointer?.Count == 1 &&
						sts.Pointer[0].DocumentSectionCode.Value == WCOPointer.Consignment && sts.Pointer[0].SequenceNumeric == bill.ABL_SequenceNumber)
					?.NameCode?.Value;
				if (!statusNameCode.IsEmpty())
				{
					bill.ABL_BillStatus = statusNameCode;
				}
			}
		}

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObjectCore(EDIMessage message)
		{
			var ilManMessage = message as ILMAN171ResponseMessage;

			if (ilManMessage == null)
			{
				return (message.EM_GB, null, MessageProcessors.UnexpectedMessage);
			}

			var countryCode = ilManMessage.GetCountryCodeSafe();
			var responseMessage = ((MessageDataObject<MnMsg4SendManifestFeedBackMessage>)ilManMessage.MessageDataObject).MessageData;

			var header = GetLinkedAsycudaManifestHeader(ilManMessage.Factory, responseMessage, countryCode);

			if (header == null)
			{
				return (message.EM_GB, null, Constants.Message1171Processor.ManifestNotFound);
			}

			var requestedSupportingDocumentErrors = GetRequestedSupportingDocument(header, responseMessage) ?? new List<RequestedSupportingDocument>();
			var firstCouldNotLocated = requestedSupportingDocumentErrors.FirstOrDefault(r => r.Bill == null);
			if (firstCouldNotLocated != null)
			{
				return (message.EM_GB, null, Constants.Message1171Processor.GetRequestedSupportingDocumentNotFound(firstCouldNotLocated.BillSequenceNumber.Value));
			}

			return (header.Branch.PK, header, (NoResString)ZString.Empty);
		}
	}

	class RequestedSupportingDocument
	{
		public decimal? BillSequenceNumber { get; internal set; }
		public ErrorValidationCodeType ValidationCode { get; internal set; }
		public AsycudaBill Bill { get; internal set; }
	}
}
