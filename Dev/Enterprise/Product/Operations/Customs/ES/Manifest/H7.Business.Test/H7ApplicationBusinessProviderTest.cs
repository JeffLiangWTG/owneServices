using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Manifest.H7.Business.UniversalDataTransfer;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(H7ApplicationBusinessProvider))]
	sealed class H7ApplicationBusinessProviderTest : EU.H7.Business.Testing.H7ApplicationBusinessProviderTest<H7ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();
			return header;
		}

		protected override IEnumerable<string> ExpectedCountryCodes => new[] { Core.Constants.CountryCodes.Spain };

		protected override Type ExpectedUploadDocumentSendingObjectParentType => typeof(UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>);

		protected override Type ExpectedDocumentRequestSendingObjectParentType => typeof(DocumentRequestSendingActionParent<DocumentRequestSendingAction>);

		protected override Type ExpectedSendCustomsDeclarationMessageProcessorType => typeof(ESH7SendCustomsDeclarationMessageProcessor);

		protected override bool ExpectedSupportsSendG3CustomsDeclaration => true;

		protected override Type ExpectedSendG3CustomsDeclarationMessageProcessorType => typeof(ESH7SendG3CustomsDeclarationMessageProcessor);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(ESH7AsycudaForCustomsDeclarationDataObjectWriter);

		protected override ZString ExpectedPackedItemTariffDataGrouping => CountryCodes.Spain;
	}
}
