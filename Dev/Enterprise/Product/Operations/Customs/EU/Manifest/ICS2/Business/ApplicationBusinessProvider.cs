using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ApplicationBusinessProvider : ASYCUDA.Business.ApplicationBusinessProvider
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);

		public override IEnumerable<(ZString CountryCode, ZString Description)> GetManifestDescriptions(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, Func<IManifestType, bool> filter)
		{
			return new[] { (new ZString(Core.Constants.CountryCodes.EuropeanUnion), new ZString(Res.GetString("EUICS2ManifestMenuItemDescription", "ICS2 (Europe)"))) };
		}

		public override ASYCUDA.Business.MessagingProvider MessagingProvider => new MessagingProvider();

		public override ASYCUDA.Business.FeatureProvider FeatureProvider => new FeatureProvider();

		protected override IReadOnlyList<ZString> CreateCountryCodes()
		{
			var countryCodeList = new HashSet<ZString>();
			foreach (var country in factory.GetEuropeanUnionForCustomsMembers())
			{
				countryCodeList.Add(country);
			}
			countryCodeList.Add(Core.Constants.CountryCodes.Iceland);
			countryCodeList.Add(Core.Constants.CountryCodes.Norway);
			countryCodeList.Add(Core.Constants.CountryCodes.Switzerland);
			countryCodeList.Add(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes);
			countryCodeList.Add(Core.Constants.CountryCodes.Guadeloupe);
			countryCodeList.Add(Core.Constants.CountryCodes.Reunion);
			countryCodeList.Add(Core.Constants.CountryCodes.Martinique);
			countryCodeList.Add(Core.Constants.CountryCodes.FrenchGuyana);
			countryCodeList.Add(Core.Constants.CountryCodes.Mayotte);
			countryCodeList.Add(Core.Constants.CountryCodes.SaintMartin);

			countryCodeList.Add(Core.Constants.CountryCodes.EuropeanUnion);

			return countryCodeList.ToArray();
		}

		protected override IReadOnlyList<IManifestType> CreateManifestTypes()
		{
			return new EUICS2ManifestTypes().All;
		}

		protected override IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager)
		{
			return new ICS2AsycudaManifestHeaderDataObjectWriter(manager);
		}

		protected override IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
		{
			return new AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>(manager, helper);
		}

		protected override AsycudaManifestHeaderDataObjectWriterHelper GetAsycudaManifestHeaderDataObjectWriterHelperCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new ICS2AsycudaManifestHeaderDataObjectWriterHelper((AsycudaManifestHeader)header);
		}

		protected override AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode)
		{
			return new ICS2AsycudaManifestDataObjectReaderHelper(countryCode, factory);
		}

		public override ZString PackedItemTariffDataGrouping => Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		public override ZString PackedItemTariffType => Universal.Constants.TariffTypes.Export;

		public override List<SelectionStyle> SelectNomenclatureModes => [SelectionStyle.Heading, SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff];
	}
}
