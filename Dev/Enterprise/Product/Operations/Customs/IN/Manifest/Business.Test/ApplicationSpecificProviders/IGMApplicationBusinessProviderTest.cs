using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(IGMApplicationBusinessProvider))]
sealed class IGMApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<IGMApplicationBusinessProvider, IGMAsycudaManifestHeader>
{
	public override void TestAsycudaManifestHeaderType()
	{
		var header = CreateNewManifest();
		AssertEquals(typeof(IGMAsycudaManifestHeader), header.ApplicationBusinessProvider.AsycudaManifestHeaderType);
	}

	public void TestManifestTypes_Default()
	{
		var header = CreateNewManifest();
		var provider = header.ApplicationBusinessProvider;
		AssertEquals($"IGM supported by registry", true, provider.ManifestTypes.Any(x => x.Code == INManifestTypes.Codes.IGM));
		AssertEquals($"IN included in countries supporting IGM", true, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty).Any(code => code == Core.Constants.CountryCodes.India));
	}

	protected override IEnumerable<IManifestType> ExpectedManifestTypes => new[] { new INManifestTypes().IGM };

	protected override Type ExpectedMessagingProviderType => null;

	protected override Type ExpectedFeatureProviderType => typeof(IGMFeatureProvider);

	protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);

	protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<IGMAsycudaManifestHeader>);
}
