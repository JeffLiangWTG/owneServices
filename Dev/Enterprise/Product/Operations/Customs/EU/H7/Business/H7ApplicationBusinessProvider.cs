using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.H7.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.H7.Business
{
	public class H7ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		protected override IReadOnlyList<ZString> CreateCountryCodes()
		{
			var countryCodeList = new List<ZString>();
			foreach (var country in factory.GetEuropeanUnionForCustomsMembers())
			{
				if (!countriesWithDedicatedH7Solution.Contains(country))
				{
					countryCodeList.Add(country);
				}
			}

			return countryCodeList;
		}

		readonly HashSet<string> countriesWithDedicatedH7Solution = new()
		{
			Core.Constants.CountryCodes.Ireland,
			Core.Constants.CountryCodes.UnitedKingdom,
			Core.Constants.CountryCodes.Spain,
			Core.Constants.CountryCodes.France,
			Core.Constants.CountryCodes.Italy,
		};

		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected override IReadOnlyList<ASYCUDA.Business.IManifestType> CreateManifestTypes()
		{
			return new EUH7ManifestTypes().All;
		}

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
		{
			return new EUH7AsycudaManifestHeaderDataObjectWriter(manager);
		}

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new EUH7AsycudaForCustomsDeclarationDataObjectWriter(manager, helper);
		}

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode)
		{
			return new EUH7AsycudaManifestDataObjectReaderHelper(countryCode, factory);
		}

		protected override ASYCUDA.Business.UniversalDataTransfer.AsycudaBillEventContextReader GetAsycudaBillEventContextReaderCore(ASYCUDA.Business.AsycudaBill bill)
		{
			return new UniversalDataTransfer.AsycudaBillEventContextReader(bill);
		}

		public virtual string InboundEDIMessageApplicationCode => EDIMessage.ApplicationCodes.EUH7;

		#region Message Sending

		public BaseMessageSendingObjectParent GetNewMessageSendingObjectParent(AsycudaManifestHeader manifestHeader)
		{
			var ctorType = MessageSendingObjectParentType.GetConstructor([typeof(AsycudaManifestHeader)]);
			return ctorType.Invoke([manifestHeader]) as BaseMessageSendingObjectParent;
		}

		Type MessageSendingObjectParentType => typeof(MessageSendingObjectParent<>).MakeGenericType(MessageSendingObjectType);

		protected virtual Type MessageSendingObjectType => typeof(MessageSendingObject);

		public BaseMessageSendingObjectParent GetNewUploadDocumentsMessageSendingObjectParent(AsycudaManifestHeader manifestHeader)
		{
			var ctorType = UploadDocumentsMessageSendingObjectParentType.GetConstructor([typeof(AsycudaManifestHeader)]);
			return ctorType.Invoke([manifestHeader]) as BaseMessageSendingObjectParent;
		}

		protected virtual Type UploadDocumentsMessageSendingObjectParentType => typeof(UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>);

		public BaseMessageSendingObjectParent GetNewDocumentRequestMessageSendingObjectParent(AsycudaManifestHeader manifestHeader)
		{
			var ctorType = DocumentRequestMessageSendingObjectParentType.GetConstructor([typeof(AsycudaManifestHeader)]);
			return ctorType.Invoke([manifestHeader]) as BaseMessageSendingObjectParent;
		}

		protected virtual Type DocumentRequestMessageSendingObjectParentType => typeof(DocumentRequestSendingActionParent<DocumentRequestSendingAction>);

		public virtual IProcessor GetMessageProcessorDependOnTriggerAction(ZString actionType, AsycudaManifestHeader manifestHeader) => actionType == WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration
			? new EUH7SendCustomsDeclarationMessageProcessor(manifestHeader) : null;

		public virtual bool SupportsSendG3CustomsDeclaration => false;

		public override ZString PackedItemTariffDataGrouping => ZString.Empty;

		public override ZString PackedItemTariffType => UniversalReferenceConstants.CusTariffTypes.ImportTariff;

		#endregion
	}
}
