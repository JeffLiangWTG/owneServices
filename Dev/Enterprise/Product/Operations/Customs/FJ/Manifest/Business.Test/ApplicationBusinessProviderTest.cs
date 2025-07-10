using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FJ.Manifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public void TestApplicableCountryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var er = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(er.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Fiji);
			var fj = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, Core.Constants.CountryCodes.Fiji, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(fj.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			Factory.Save();
			var provider = new ApplicationBusinessProvider();
			provider.Initialise(Factory);
			Assert("Does noot contain other manifest countries", !provider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC).Contains(Core.Constants.CountryCodes.Eritrea));
			Assert("Contains Fiji", provider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC).Contains(Core.Constants.CountryCodes.Fiji));
			Assert("Does not contain CookIslands", !provider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC).Contains(Core.Constants.CountryCodes.CookIslands));
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new IManifestType[] { new ManifestType("ASY", "Asycuda Manifest", new[] { "AIR", "SEA", "MAI", "ROA" }, new[] { ManifestBase.ApplicationCodeTypeList.Codes.Consolidator, ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine }, MessageLevel.Manifest) };
		protected override Type ExpectedMessagingProviderType => typeof(ASYCUDAManifest.Business.MessagingProvider);
		protected override Type ExpectedFeatureProviderType => typeof(ASYCUDAManifest.Business.FeatureProvider);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(AsycudaManifestHeaderDataObjectWriter<ASYCUDAManifest.Business.AsycudaManifestHeader>);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<ASYCUDAManifest.Business.AsycudaBill, ASYCUDAManifest.Business.AsycudaPack, AsycudaPackedItem>);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(AsycudaManifestDataObjectReaderHelper);
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry(Core.Constants.TransportModes.Air, Core.Constants.CountryCodes.Fiji), new ModeAndCountry(Core.Constants.TransportModes.Road, Core.Constants.CountryCodes.Fiji));
			var header = base.CreateNewManifest();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
			return header;
		}
	}
}
