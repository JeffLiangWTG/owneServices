using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.Colombia };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes() => COCustomsDataRegistry.Instance.EnableCOManifests.Value ? new COManifestTypes().All : (Array.Empty<IManifestType>());

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
			=> new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			=> new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);
	}
}
