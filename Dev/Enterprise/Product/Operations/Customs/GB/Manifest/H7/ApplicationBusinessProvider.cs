using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GB.H7.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.H7.Business
{
	public class ApplicationBusinessProvider : H7ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override Type MessageSendingObjectType => typeof(MessageSendingObject);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new ZString[] { Core.Constants.CountryCodes.UnitedKingdom };

		protected override IReadOnlyList<ASYCUDA.Business.IManifestType> CreateManifestTypes()
		{
			return new GBH7ManifestTypes().All;
		}

		protected override Type UploadDocumentsMessageSendingObjectParentType => typeof(UploadDocumentsSendingActionParent);

		public override IProcessor GetMessageProcessorDependOnTriggerAction(ZString actionType, EU.H7.Business.AsycudaManifestHeader manifestHeader) => actionType == WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration
			? new GBH7SendCustomsDeclarationMessageProcessor(manifestHeader) : base.GetMessageProcessorDependOnTriggerAction(actionType, manifestHeader);

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new GBH7AsycudaForCustomsDeclarationDataObjectWriter(manager, helper);
		}

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
