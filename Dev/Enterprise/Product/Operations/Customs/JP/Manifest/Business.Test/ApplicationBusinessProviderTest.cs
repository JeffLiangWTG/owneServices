using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using AsycudaPack = Enterprise.Customs.ASYCUDA.Business.AsycudaPack;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public void TestManifestTypes_Disable()
		{
			using (JPRegistry.Instance.EnableHCHForwarderManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (JPRegistry.Instance.EnableHDFForwarderManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					using (JPRegistry.Instance.EnableNVCForwarderManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
					{
						var manifest = CreateNewManifest();
						var applicationBusinessProvider = manifest.ApplicationBusinessProvider as ApplicationBusinessProvider;
						AssertNull(applicationBusinessProvider);
					}
				}
			}
		}

		public void TestGetApplicableManifestTypes()
		{
			var header = CreateNewManifest();
			var provider = header.ApplicationBusinessProvider;
			var jpManifestTypes = new JPManifestTypes();
			var comparer = new ManifestType.EqualityComparer();

			AssertContainsExactElementsInAnyOrder(comparer, [jpManifestTypes.HCH], provider.GetApplicableManifestTypes(Directions.Import, TransportTypeList.Codes.Air));
			AssertContainsExactElementsInAnyOrder(comparer, [jpManifestTypes.HDF], provider.GetApplicableManifestTypes(Directions.Export, TransportTypeList.Codes.Air));
			AssertContainsExactElementsInAnyOrder(comparer, [jpManifestTypes.NVC], provider.GetApplicableManifestTypes(Directions.Import, TransportTypeList.Codes.Sea));
			AssertContainsExactElementsInAnyOrder(comparer, [jpManifestTypes.VAN], provider.GetApplicableManifestTypes(Directions.Export, TransportTypeList.Codes.Sea));
		}

		public override void TestAsycudaManifestHeaderType()
		{
			var header = CreateNewManifest();
			var provider = header.ApplicationBusinessProvider;
			AssertEquals(typeof(AsycudaManifestHeader), provider.AsycudaManifestHeaderType);
		}

		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);

		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);

		protected override IEnumerable<IManifestType> ExpectedManifestTypes
		{
			get
			{
				var jpManifestTypes = new JPManifestTypes();
				return [jpManifestTypes.HCH, jpManifestTypes.HDF, jpManifestTypes.NVC, jpManifestTypes.VAN];
			}
		}

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var asycudaBill = header.Bills.AddNew();
			asycudaBill.ABL_BolType = "BOL";
			var asycudaPack = asycudaBill.Packs.AddNew();
			asycudaPack.APA_GoodsDescription = "HELLO";
			asycudaPack.PackedItemForTesting();

			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Japan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			JPRegistry.Instance.EnableHCHForwarderManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
