using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.ICS
{
	public abstract class ApplicationBusinessProviderBase : ASYCUDA.Business.ApplicationBusinessProvider
	{
		protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.UnitedKingdom };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			if (RegistryEnabled)
			{
				return new ICSManifestTypes().All.Where(x => x.Code == ManifestType).ToList().AsReadOnly();
			}

			return System.Array.Empty<IManifestType>();
		}

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected abstract ZString ManifestType { get; }

		protected abstract ZBool RegistryEnabled { get; }

		public override MessagingProvider MessagingProvider => new ICSMessagingProvider();

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
		{
			return new AsycudaManifestHeaderDataObjectWriter<EU.Manifest.Business.AsycudaManifestHeader>(manager);
		}

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new AsycudaForCustomsDeclarationDataObjectWriter<EU.Manifest.Business.AsycudaBill, EU.Manifest.Business.AsycudaPack, AsycudaPackedItem>(manager, helper);
		}
	}
}
