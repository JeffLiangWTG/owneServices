using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GB.H7.Business.UniversalDataTransfer;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : EU.H7.Business.Testing.H7ApplicationBusinessProviderTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();
			return header;
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes =>
			new IManifestType[]
			{
				new ManifestType(
					EUH7ManifestTypes.Codes.EH7,
					EUH7ManifestTypes.Descriptions.EH7,
					new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road, TransportModes.Mail },
					new[] { ApplicationCodeTypeList.Codes.EuH7 },
					MessageLevel.Bill,
					ShipmentTypeList.Import23Only())
			};

		protected override IEnumerable<string> ExpectedCountryCodes => new[] { Core.Constants.CountryCodes.UnitedKingdom };

		protected override Type ExpectedUploadDocumentSendingObjectParentType => typeof(UploadDocumentsSendingActionParent);

		protected override Type ExpectedSendCustomsDeclarationMessageProcessorType => typeof(GBH7SendCustomsDeclarationMessageProcessor);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(GBH7AsycudaForCustomsDeclarationDataObjectWriter);

		protected override ZString ExpectedPackedItemTariffDataGrouping => CountryCodes.UnitedKingdom;
	}
}
