using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		protected override IReadOnlyList<ZString> ApplicableCountryCodesCore(Directions direction, ZString transportMode, string manifestStyle = null)
			=> BaseAsycudaCountries.Where(x => x.HasAttribute(manifestStyle)).Select(x => x.ZZD_Code).ToList();

		List<ZZRefCusCodeListCombined> BaseAsycudaCountries
		{
			get
			{
				return factory.GetCachedValue("BaseAsycudaCountries", delegate
				{
					var countries = CountryHelper.SupportedAsycudaCountryCodesList(factory).ToList();
					countries.RemoveAll(x => x.ZZD_Code == Core.Constants.CountryCodes.Fiji);
					countries.RemoveAll(x => x.ZZD_Code == Core.Constants.CountryCodes.CookIslands);
					return countries;
				});
			}
		}

		protected override IReadOnlyList<ZString> CreateCountryCodes() => BaseAsycudaCountries.Select(x => x.ZZD_Code).ToList();

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			return new[]
			{
				new ManifestType(
					ASYCUDAManifestTypes.Codes.ASY,
					ASYCUDAManifestTypes.Descriptions.ASY,
					GetAllPossibleTransportModes().GetAllCodes(),
					new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
					MessageLevel.Manifest
				)
			};
		}

		static CodeDescriptionPairList GetAllPossibleTransportModes()
		{
			return new CodeDescriptionPairList
			{
				new CodeDescriptionPair(Customs.Business.TransportTypeList.Codes.Air, Customs.Business.TransportTypeList.Descriptions.Air),
				new CodeDescriptionPair(Customs.Business.TransportTypeList.Codes.Sea, Customs.Business.TransportTypeList.Descriptions.Sea),
				new CodeDescriptionPair(Customs.Business.TransportTypeList.Codes.Mail, Customs.Business.TransportTypeList.Descriptions.Mail),
				new CodeDescriptionPair(Customs.Business.TransportTypeList.Codes.Road, Customs.Business.TransportTypeList.Descriptions.Road)
			};
		}

		public override IEnumerable<string> GetAcceptableTransportModesFromAsycudaManifestHeader(BusinessObjectFactory factory, string countryCode, string applicationCode, Directions direction)
		{
			var allPossibleModes = GetAllPossibleTransportModes();
			var result = new HashSet<string>();
			var asycudaManifestCountryList = ZZDatabaseValidationHelper.GetAsycudaManifestCountries(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC).ToList();
			var zzd = asycudaManifestCountryList.FirstOrDefault(x => x.ZZD_Code == countryCode);
			if (zzd != null)
			{
				var transportModes = zzd.TransportModes;
				var codesForThisCountry = transportModes.Any() ? transportModes.ToArray() : allPossibleModes.GetAllCodesZString();  // When zzd exists but there are no zze records, assume enabled for all modes
				result.UnionWith(codesForThisCountry.Select(x => (string)x));
			}

			return result;
		}

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager) => new UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriter(manager);

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) => new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode) => new UniversalDataTransfer.AsycudaManifestDataObjectReaderHelper(countryCode, factory);
	}
}
