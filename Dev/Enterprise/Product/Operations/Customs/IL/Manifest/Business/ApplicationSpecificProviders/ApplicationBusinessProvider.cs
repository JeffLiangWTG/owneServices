using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		public override ZString PackedItemTariffType => Universal.Constants.TariffTypes.Import;

		public override ZString PackedItemTariffDataGrouping => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Israel) ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override IReadOnlyList<ZString> CreateCountryCodes() => new ZString[] { Core.Constants.CountryCodes.Israel };

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
			=> ILCustomsDataRegistry.Instance.ILEnableILManifest.Value switch
			{
				ILManifestRegistryOptions.Codes.IMPORT => new IManifestType[] { new ILManifestTypes().ImportManifest },
				ILManifestRegistryOptions.Codes.EXPORT => new IManifestType[] { new ILManifestTypes().ExportManifest },
				ILManifestRegistryOptions.Codes.ALL => new IManifestType[] { new ILManifestTypes().AllManifest },
				_ => new IManifestType[] { new ILManifestTypes().AllManifest },
			};

		protected override IReadOnlyList<ZString> ApplicableCountryCodesCore(Directions direction, ZString transportMode, string manifestStyle = null)
		{
			var manifestTypes = new ILManifestTypes();
			if ((direction == Directions.Import || direction == Directions.Unknown)
				&& (ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.IMPORT || ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.ALL)
				&& (transportMode.IsEmpty || manifestTypes.ImportManifest.ApplicableTransportModes.Any(s => s == transportMode)))
			{
				return base.ApplicableCountryCodesCore(direction, manifestStyle);
			}

			if ((direction == Directions.Export || direction == Directions.Unknown)
				&& (ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.EXPORT || ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.ALL)
				&& (transportMode.IsEmpty || manifestTypes.ExportManifest.ApplicableTransportModes.Any(s => s == transportMode)))
			{
				return base.ApplicableCountryCodesCore(direction, manifestStyle);
			}

			return Enumerable.Empty<ZString>().ToList();
		}

		public override IEnumerable<string> GetAcceptableTransportModesFromAsycudaManifestHeader(BusinessObjectFactory factory, string countryCode, string applicationCode, Directions direction)
		{
			var manifestTypes = new ILManifestTypes();
			if (
				(direction == Directions.Import || direction == Directions.Unknown) &&
				(ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.IMPORT || ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.ALL) &&
				manifestTypes.ImportManifest.ApplicableManifestStyles.Contains(applicationCode))
			{
				return manifestTypes.ImportManifest.ApplicableTransportModes;
			}

			if (
				(direction == Directions.Export || direction == Directions.Unknown) &&
				(ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.EXPORT || ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.ALL) &&
				manifestTypes.ExportManifest.ApplicableManifestStyles.Contains(applicationCode))
			{
				return manifestTypes.ExportManifest.ApplicableTransportModes;
			}

			return Enumerable.Empty<string>();
		}

		public override IEnumerable<(ZString CountryCode, ZString Description)> GetManifestDescriptions(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, Func<IManifestType, bool> filter)
		{
			yield return (Core.Constants.CountryCodes.Israel, ResString.GetMultilingualString("98DD802B-5177-4903-9FAE-B9133F980E75", "Israel"));
		}

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager)
			=> new AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>(manager);

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(UniversalDataBuss.Integration.IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			=> new AsycudaForCustomsDeclarationDataObjectWriter<ASYCUDA.Business.AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);
	}
}
