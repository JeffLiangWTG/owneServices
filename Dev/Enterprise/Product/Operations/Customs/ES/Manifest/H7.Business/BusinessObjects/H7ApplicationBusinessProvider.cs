using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ES.Manifest.H7.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class H7ApplicationBusinessProvider : EU.H7.Business.H7ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new ZString[] { Core.Constants.CountryCodes.Spain };

		protected override Type MessageSendingObjectType => typeof(H7MessageSendingObject);

		protected override Type UploadDocumentsMessageSendingObjectParentType => typeof(UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>);

		protected override Type DocumentRequestMessageSendingObjectParentType => typeof(DocumentRequestSendingActionParent<DocumentRequestSendingAction>);

		public override IProcessor GetMessageProcessorDependOnTriggerAction(ZString actionType, EU.H7.Business.AsycudaManifestHeader manifestHeader) => actionType == WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration
			? new ESH7SendCustomsDeclarationMessageProcessor(manifestHeader) : new ESH7SendG3CustomsDeclarationMessageProcessor(manifestHeader);

		public override bool SupportsSendG3CustomsDeclaration => true;

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new ESH7AsycudaForCustomsDeclarationDataObjectWriter(manager, helper);
		}

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.Spain;
	}
}
