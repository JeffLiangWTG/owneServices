using System;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACarrierCombinedCollectionProvider))]
	sealed class CACarrierCombinedCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ZZRefCarrierCombinedCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.Universal.ZZRefCarrier;

		protected override int ExpectedMaxLength => AutoCAAddInfo.Schema.CA_CarrierCodeMaxLength;

		protected override void SetUp()
		{
			base.SetUp();
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "$123";
			carrier.ZZ4_Description = "DUMMY CARRIER";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsAir = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
		}
	}
}
