using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AE.Registry;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
{
	public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

	protected override IReadOnlyList<ZString> CreateCountryCodes() => new[] { (ZString)Core.Constants.CountryCodes.UnitedArabEmirates };

	public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

	public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

	protected override IReadOnlyList<IManifestType> CreateManifestTypes()
	{
		if (AECustomsRegistry.Instance.EnableUAESeaExportManifest.Value)
		{
			return new[] { new AEManifestTypes().ACI };
		}
		return Array.Empty<IManifestType>();
	}

	protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager) => new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

	protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) => new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);

	public override ZString PackedItemTariffDataGrouping => Core.Constants.Customs.Universal.RefDataGrouping.Codes.GulfCooperationCouncil;
}
