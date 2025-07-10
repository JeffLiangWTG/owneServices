using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public void TestGetPackedItemTariffDataGrouping()
		{
			var header = CreateNewManifest();
			AssertEquals("ES", header.ApplicationBusinessProvider.PackedItemTariffDataGrouping);
		}

		public void TestGetPackedItemTariffType()
		{
			var header = CreateNewManifest();
			AssertEquals("EXP", header.ApplicationBusinessProvider.PackedItemTariffType);
		}

		public void TestGetSelectNomenclatureModes()
		{
			var expectedList = new List<SelectionStyle> { SelectionStyle.Heading, SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff };
			var header = CreateNewManifest();
			AssertEquals(expectedList.ToString(), header.ApplicationBusinessProvider.SelectNomenclatureModes.ToString());
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new ESManifestTypes().All;

		protected override Type ExpectedMessagingProviderType => null;

		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);

		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
			result.AMA_ManifestType = ESManifestTypes.Codes.ICS;
			result.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			return result;
		}
	}
}
