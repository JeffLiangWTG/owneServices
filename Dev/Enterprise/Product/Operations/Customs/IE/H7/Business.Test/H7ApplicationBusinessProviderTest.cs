using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.H7.Business.UniversalDataTransfer;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(H7ApplicationBusinessProvider))]
	sealed class H7ApplicationBusinessProviderTest : EU.H7.Business.Testing.H7ApplicationBusinessProviderTest<H7ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV1";
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "HELLO";
			pack.PackedItems.AddNew();
			return header;
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes =>
			new IManifestType[] { new ManifestType(
			EUH7ManifestTypes.Codes.EH7,
			EUH7ManifestTypes.Descriptions.EH7,
			new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road },
			new[] { ApplicationCodeTypeList.Codes.EuH7V1, ApplicationCodeTypeList.Codes.EuH7V2 },
			MessageLevel.Bill,
			ShipmentTypeList.Import23Only()
		) };

		protected override string ExpectedInboundEDIMessageApplicationCode => EDIMessage.ApplicationCodes.IECustomsImport;
		protected override Type ExpectedUploadDocumentSendingObjectParentType => typeof(UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>);
		protected override IEnumerable<string> ExpectedCountryCodes => new[] { Core.Constants.CountryCodes.Ireland };

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(IEH7AsycudaForCustomsDeclarationDataObjectWriter);

		protected override ZString ExpectedPackedItemTariffDataGrouping => CountryCodes.Ireland;
	}
}
