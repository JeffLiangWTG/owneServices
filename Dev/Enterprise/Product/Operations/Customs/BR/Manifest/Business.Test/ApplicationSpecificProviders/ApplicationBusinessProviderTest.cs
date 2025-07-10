using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.Business.Test
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public void TestManifestTypes_MUCR()
		{
			using (BRCustomsDataRegistry.Instance.EnableUCRManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var header = CreateNewManifest();
				header.AMA_ManifestType = "MUCR";
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new BRManifestTypes().MUCR }, header.ApplicationBusinessProvider.ManifestTypes);
			}
		}

		public void TestManifestTypes_MER()
		{
			using (BRCustomsDataRegistry.Instance.EnableUCRManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var header = CreateNewManifest();
				header.AMA_ManifestType = "MER";
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new BRManifestTypes().MER }, header.ApplicationBusinessProvider.ManifestTypes);
			}
		}

		public void TestManifestTypes_All()
		{
			using (BRCustomsDataRegistry.Instance.EnableUCRManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var header = CreateNewManifest();
				AssertContainsExactElementsInAnyOrder(new ManifestType.EqualityComparer(), new[] { new BRManifestTypes().MUCR, new BRManifestTypes().MER }, header.ApplicationBusinessProvider.ManifestTypes);
			}
		}

		public void TestManifestTypes_None()
		{
			using (BRCustomsDataRegistry.Instance.EnableUCRManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (BRCustomsDataRegistry.Instance.EnableMercanteSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var header = CreateNewManifest();
				AssertEquals(false, header.ApplicationBusinessProvider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty).Any(code => code == Core.Constants.CountryCodes.Brazil));
			}
		}

		public void TestManifestNature()
		{
			var oBRManifestType = new BRManifestTypes().All;

			var oMURC = oBRManifestType.FirstOrDefault(x => x.Code == BRManifestTypes.Codes.MUCR);
			AssertEquals(ShipmentTypeList.Codes.Export22, oMURC.ManifestNatures.CodesAsString);

			var oMER = oBRManifestType.FirstOrDefault(x => x.Code == BRManifestTypes.Codes.MER);
			AssertEquals(ShipmentTypeList.Codes.Import23, oMER.ManifestNatures.CodesAsString);
		}

		public override void TestAsycudaManifestHeaderType()
		{
			var header = CreateNewManifest();
			var provider = header.ApplicationBusinessProvider;
			AssertEquals(typeof(AsycudaManifestHeader), provider.AsycudaManifestHeaderType);
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new[] { new BRManifestTypes().MUCR };

		protected override Type ExpectedMessagingProviderType => null;

		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);

		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.Brazil;
			result.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			result.AMA_ManifestType = BRManifestTypes.Codes.MUCR;
			return result;
		}
	}
}
