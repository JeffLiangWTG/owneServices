using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	[TestedType(typeof(EdiUserAgreementLookupsTest))]
	public class EdiUserAgreementLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVariantsList()
		{
			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement1.ERA_VariantCode = "VA1";
			agreement1.ERA_VariantDescription = "Variant 1";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var agreement2 = Factory.New<EdiUserAgreement>();
			agreement2.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement2.ERA_VariantCode = "VA2";
			agreement2.ERA_VariantDescription = "Variant 2";
			agreement2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var agreement3 = Factory.New<EdiUserAgreement>();
			agreement3.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement3.ERA_VariantCode = "VA3";
			agreement3.ERA_VariantDescription = "Variant 3";
			agreement3.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var agreement4 = Factory.New<EdiUserAgreement>();
			agreement4.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			agreement4.ERA_VariantCode = "MV1";
			agreement4.ERA_VariantDescription = "MYA 4";
			agreement4.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);

			Factory.Save();

			var agreement = Factory.New<EdiUserAgreement>();
			var agreementLookups = new EdiUserAgreementLookups(agreement);

			AssertNullOrEmpty(agreement.ERA_Type);
			AssertEquals(0, agreementLookups.Variants.Count);

			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			AssertEquals(3, agreementLookups.Variants.Count);
			AssertEquals("VA1", agreementLookups.Variants[0].Code);
			AssertEquals("Variant 1", agreementLookups.Variants[0].Description);
			AssertEquals("VA2", agreementLookups.Variants[1].Code);
			AssertEquals("Variant 2", agreementLookups.Variants[1].Description);
			AssertEquals("VA3", agreementLookups.Variants[2].Code);
			AssertEquals("Variant 3", agreementLookups.Variants[2].Description);

			agreement.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			AssertNotEquals(agreement.Level, EdiUserAgreementLevelList.Codes.Corporate);
			AssertEquals("Only COP agreement should have drop list", 0, agreementLookups.Variants.Count);
		}

		public void TestTypes()
		{
			CombineAssertions(() =>
			{
				var types = lookups.Types;
				AssertType<EdiUserAgreementTypes>("Correct lookups type", lookups.Types);
				AssertSame("Factory Cached", lookups.Types, types);
			});
		}

		public void TestLevels()
		{
			AssertType<EdiUserAgreementLevelList>(lookups.Levels);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var ediUserAgreement = Factory.New<EdiUserAgreement>();
			ediUserAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(10);
			lookups = new EdiUserAgreementLookups(ediUserAgreement);
		}

		EdiUserAgreementLookups lookups;
	}
}
