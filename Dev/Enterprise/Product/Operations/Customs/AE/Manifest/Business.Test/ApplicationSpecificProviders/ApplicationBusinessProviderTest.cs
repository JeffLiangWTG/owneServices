using System;
using System.Collections.Generic;
using Enterprise.Customs.AE.Registry;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(ApplicationBusinessProvider))]
sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
{
	[ExpectNoExceptions]
	public void TestGetPackedItemTariffDataGrouping()
	{
		var header = CreateNewManifest();
		NUnit.Framework.Assert.That(header.ApplicationBusinessProvider.PackedItemTariffDataGrouping, Is.EqualTo("GCC").Using(CustomComparers.TypeComparison));
	}

	public override void TestManifestTypes()
	{
		expectedManifestTypes = new[] { new AEManifestTypes().ACI };
		base.TestManifestTypes();

		using (AECustomsRegistry.Instance.EnableUAESeaExportManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		{
			expectedManifestTypes = Array.Empty<IManifestType>();
			base.TestManifestTypes();
		}
	}

	protected override IEnumerable<IManifestType> ExpectedManifestTypes => expectedManifestTypes;
	IEnumerable<IManifestType> expectedManifestTypes;

	protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);

	protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

	protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);

	protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);

	protected override AsycudaManifestHeader CreateNewManifest()
	{
		var result = base.CreateNewManifest();
		result.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		result.AMA_ManifestType = AEManifestTypes.Codes.ACI;
		return result;
	}
}
