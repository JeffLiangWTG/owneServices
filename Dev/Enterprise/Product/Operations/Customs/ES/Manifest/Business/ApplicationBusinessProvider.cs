using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public sealed class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		public override MessagingProvider MessagingProvider => null;

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.Spain;

		public override ZString PackedItemTariffType => Universal.Constants.TariffTypes.Export;

		public override List<SelectionStyle> SelectNomenclatureModes => new List<SelectionStyle> { SelectionStyle.Heading, SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff };

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.Spain };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes() => new ESManifestTypes().All;

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
			=> new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

		protected override AsycudaManifestHeaderDataObjectWriterHelper GetAsycudaManifestHeaderDataObjectWriterHelperCore(ASYCUDA.Business.AsycudaManifestHeader header)
			=> new AsycudaManifestHeaderDataObjectWriterHelper((AsycudaManifestHeader)header);

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			=> new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode)
			=> new AsycudaManifestDataObjectReaderHelper(countryCode, factory);
	}
}
