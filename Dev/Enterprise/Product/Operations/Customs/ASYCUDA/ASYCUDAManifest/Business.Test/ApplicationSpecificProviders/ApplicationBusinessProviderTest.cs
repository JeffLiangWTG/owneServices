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
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDAManifest.Business.Testing
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

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.CookIslands);
			var ck = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.CookIslands, Core.Constants.CountryCodes.CookIslands, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(ck.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");

			Factory.Save();
			var provider = new ApplicationBusinessProvider();
			provider.Initialise(Factory);
			Assert("Contain manifest countries", provider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC).Contains(Core.Constants.CountryCodes.Eritrea));
			Assert("Not contains Fiji", !provider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC).Contains(Core.Constants.CountryCodes.Fiji));
			Assert("Not contains CookIslands", !provider.ApplicableCountryCodes(Directions.Unknown, ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC).Contains(Core.Constants.CountryCodes.CookIslands));
		}

		public void TestCountryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var er = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(er.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Fiji);
			var fj = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, Core.Constants.CountryCodes.Fiji, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(fj.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.CookIslands);
			var ck = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.CookIslands, Core.Constants.CountryCodes.CookIslands, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(ck.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");

			Factory.Save();
			var provider = new ApplicationBusinessProvider();
			provider.Initialise(Factory);
			Assert("Contain manifest countries", provider.CountryCodes.Contains(Core.Constants.CountryCodes.Eritrea));
			Assert("Not contains Fiji", !provider.CountryCodes.Contains(Core.Constants.CountryCodes.Fiji));
			Assert("Not contains CookIslands", !provider.CountryCodes.Contains(Core.Constants.CountryCodes.CookIslands));
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => new IManifestType[]
		{
			new ManifestType(
				"ASY",
				"Asycuda Manifest",
				new[] { "AIR", "SEA", "MAI", "ROA" },
				new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
				MessageLevel.Manifest
			)
		};

		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);

		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriter);

		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);

		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(AsycudaManifestHeaderDataObjectWriterHelper);

		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(UniversalDataTransfer.AsycudaManifestDataObjectReaderHelper);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory,
				new ModeAndCountry(Core.Constants.TransportModes.Air, Core.Constants.CountryCodes.Eritrea), new ModeAndCountry(Core.Constants.TransportModes.Road, Core.Constants.CountryCodes.Eritrea));
			var header = base.CreateNewManifest();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			return header;
		}
	}
}
