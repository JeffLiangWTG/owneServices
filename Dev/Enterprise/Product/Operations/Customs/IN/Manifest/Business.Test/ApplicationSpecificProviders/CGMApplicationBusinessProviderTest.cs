using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMApplicationBusinessProvider))]
sealed class CGMApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<CGMApplicationBusinessProvider, CGMAsycudaManifestHeader>
{
	public override void TestAsycudaManifestHeaderType()
	{
		var header = CreateNewManifest();
		AssertEquals(typeof(CGMAsycudaManifestHeader), header.ApplicationBusinessProvider.AsycudaManifestHeaderType);
	}

	public void TestManifestTypes_Default()
	{
		var header = CreateNewManifest();
		var provider = header.ApplicationBusinessProvider;
		AssertEquals($"CGM supported by registry", true, provider.ManifestTypes.Any(x => x.Code == INManifestTypes.Codes.CGM));
		AssertEquals($"IN included in countries supporting CGM", true, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty).Any(code => code == Core.Constants.CountryCodes.India));
	}

	protected override IEnumerable<IManifestType> ExpectedManifestTypes => new[] { new INManifestTypes().CGM };

	protected override Type ExpectedMessagingProviderType => null;

	protected override Type ExpectedFeatureProviderType => typeof(CGMFeatureProvider);

	protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<CGMAsycudaBill, CGMAsycudaPack, AsycudaPackedItem>);

	protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<CGMAsycudaManifestHeader>);
}
