using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IE.H7.Business
{
	public class H7ApplicationBusinessProvider : EU.H7.Business.H7ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new ZString[] { Core.Constants.CountryCodes.Ireland };

		protected override Type MessageSendingObjectType => typeof(MessageSendingObject);

		public override string InboundEDIMessageApplicationCode => EDIMessage.ApplicationCodes.IECustomsImport;

		protected override Type UploadDocumentsMessageSendingObjectParentType => typeof(UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>);

		public BaseMessageSendingObjectParent GetNewRF415MessageSendingObjectParent(AsycudaManifestHeader manifestHeader)
		{
			return new RF415MessageSendingObjectParent(manifestHeader);
		}

		protected override IReadOnlyList<ASYCUDA.Business.IManifestType> CreateManifestTypes()
		{
			return new IEH7ManifestTypes().All;
		}

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new IEH7AsycudaForCustomsDeclarationDataObjectWriter(manager, helper);
		}

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.Ireland;
	}
}
