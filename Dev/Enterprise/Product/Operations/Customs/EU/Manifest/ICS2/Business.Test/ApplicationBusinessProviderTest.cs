using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public void TestCreateCountryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			var tradeGroup = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Constants.CountryCodes.Italy, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var provider = new ApplicationBusinessProvider();
			provider.Initialise(Factory);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					Constants.CountryCodes.Italy, Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Constants.CountryCodes.Switzerland,
					Constants.CountryCodes.Norway, Constants.CountryCodes.Iceland, Constants.CountryCodes.Guadeloupe, Constants.CountryCodes.Reunion,
					Constants.CountryCodes.Martinique, Constants.CountryCodes.FrenchGuyana, Constants.CountryCodes.Mayotte, Constants.CountryCodes.SaintMartin,
					Constants.CountryCodes.EuropeanUnion
				}, provider.CountryCodes);
		}

		public void TestApplicableTransportModes()
		{
			var header = CreateNewManifest();
			var transportModes = header.Lookups.TransportModeList;

			CombineAssertions(() =>
			{
				AssertEquals("There is 3 codes in the transport mode list", 3, transportModes.Count);
				Assert(transportModes.ContainsCode(Core.Constants.TransportModes.Air));
				Assert(transportModes.ContainsCode(Core.Constants.TransportModes.Sea));
				Assert(transportModes.ContainsCode(Core.Constants.TransportModes.InlandWaterwayTransport));
			});
		}

		public void TestApplicableTransportModes_IE()
		{
			var header = CreateNewManifest();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;
			var transportModes = header.Lookups.TransportModeList;

			CombineAssertions(() =>
			{
				AssertEquals("There is 3 codes in the transport mode list", 3, transportModes.Count);
				Assert(transportModes.ContainsCode(Core.Constants.TransportModes.Air));
				Assert(transportModes.ContainsCode(Core.Constants.TransportModes.Sea));
				Assert(transportModes.ContainsCode(Core.Constants.TransportModes.InlandWaterwayTransport));
			});
		}

		public override void TestManifestTypes()
		{
			expectedManifestTypes = new EUICS2ManifestTypes().All;
			base.TestManifestTypes();
		}

		public void TestMenuItemName()
		{
			var (countryCode, description) = ApplicationBusinessProvider.GetManifestApplicationBusinessProvidersForActiveManifestTypes(Factory, ApplicationCodeTypeList.Codes.Consolidator)
					.SelectMany(x => x.GetManifestDescriptions(Factory, x.CountryCodes, type => true))
					.SingleOrDefault(c => c.CountryCode.EqualsIgnoringCase(Core.Constants.CountryCodes.EuropeanUnion));
			AssertEquals("ICS2 (Europe)", description);
		}

		public void TestGetPackedItemTariffDataGrouping()
		{
			var header = CreateNewManifest();
			AssertEquals("EUN", header.ApplicationBusinessProvider.PackedItemTariffDataGrouping);
		}

		public void TestGetPackedItemTariffType()
		{
			var header = CreateNewManifest();
			AssertEquals("EXP", header.ApplicationBusinessProvider.PackedItemTariffType);
		}

		public void TestGetSelectNomenclatureModes()
		{
			var header = CreateNewManifest();
			var expected = new SelectionStyle[] { SelectionStyle.Heading, SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff };
			AssertContainsExactElementsInAnyOrder(expected, header.ApplicationBusinessProvider.SelectNomenclatureModes);
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => expectedManifestTypes;
		IEnumerable<IManifestType> expectedManifestTypes;
		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);
		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(ICS2AsycudaManifestHeaderDataObjectWriter);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(ICS2AsycudaManifestHeaderDataObjectWriterHelper);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(ICS2AsycudaManifestDataObjectReaderHelper);
		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			return result;
		}
	}
}
