using System.Collections.Generic;
using CargoWise.Customs.IL.MessageDefinitions.MAN.RES_821.MN_NG_8241_Cargo_Message;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	sealed class Message8241Processor : ILBranchCustomsApplicationTypeMessageProcessorBase
	{
		public Message8241Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("1144DC55-1B4F-448E-B170-105B111D6837", "IL Manifest Query Response Message");

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.ILCustoms;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new [] { (ZString)ILMessageTypeList.Codes.MAN };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new [] { (ZString)ILEDIMessageSubTypeList.Codes.ManifestQueryResponse };

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is ILMAN821ResponseMessage ilManQueryMessage
				&& ilManQueryMessage.MessageDataObject is MessageDataObject<MnNg8241CargoMessage> messageDataObject
				&& messageDataObject.MessageData is MnNg8241CargoMessage responseMessage)
			{
				var countryCode = ilManQueryMessage.GetCountryCodeSafe();
				var header = (ilManQueryMessage.EM_LinkedObject as AsycudaManifestHeader) ?? GetLinkedAsycudaManifestHeader(ilManQueryMessage.Factory, responseMessage, countryCode);
				if (header == null)
				{
					return;
				}

				ilManQueryMessage.EM_Status = EDIMessage.Status.ProcessedOK;

				base.ProcessMessageCore(ilManQueryMessage);
			}
		}

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObjectCore(EDIMessage message)
		{
			var ilManQueryMessage = message as ILMAN821ResponseMessage;

			if (ilManQueryMessage == null)
			{
				return (message.EM_GB, null, MessageProcessors.UnexpectedMessage);
			}

			var countryCode = ilManQueryMessage.GetCountryCodeSafe();
			var responseMessage = ((MessageDataObject<MnNg8241CargoMessage>)ilManQueryMessage.MessageDataObject).MessageData;

			var header = GetLinkedAsycudaManifestHeader(ilManQueryMessage.Factory, responseMessage, countryCode);
			if (header == null)
			{
				return (message.EM_GB, null, Constants.Message8241Processor.GetManifestNotFound($"{responseMessage.Manifest?.Manifestnumber}," + $" {nameof(ilManQueryMessage.EM_MessageNum)}" + $" {ilManQueryMessage.EM_MessageNum.ToString()}," + $" {nameof(ilManQueryMessage.EM_MessageType)}" + $" {ilManQueryMessage.EM_MessageType}," + $" {nameof(ilManQueryMessage.EM_MessageSubType)}" + $" {ilManQueryMessage.EM_MessageSubType}"));
			}

			return (header.Branch.PK, header, (NoResString)ZString.Empty);
		}

		protected override ControllerID ControllerIDForEmail => ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest;

		protected override string MessageTypeInSubject => Res.GetString("B7A16BE9-F569-4E91-A1E8-A316B9A7A168", "Manifest Query Message");

		protected override string GetJobNumber(IEDIMessageCollectionOwner owner) => ((AsycudaManifestHeader)owner).AMA_JobReference;

		AsycudaManifestHeader GetLinkedAsycudaManifestHeader(BusinessObjectFactory factory, MnNg8241CargoMessage responseMessage, ZString countryCode)
		{
			var manifestNumber = responseMessage.Manifest?.Manifestnumber;

			return FindManifest(factory, countryCode, manifestNumber);
		}

		static AsycudaManifestHeader FindManifest(BusinessObjectFactory factory, ZString countryCode, string manifestNumber)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, countryCode);
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestNumber, manifestNumber);
			return factory.LoadTop1<AsycudaManifestHeader>(headerQuery);
		}
	}
}
