using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(USCustomsDisbursementChargeCodesValueProvider))]
	sealed class USCustomsDisbursementChargeCodesValueProviderTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < USCustomsDisbursementChargeCodes >", ValueProviderToTest.IsResponsibleForReplacing("< USCustomsDisbursementChargeCodes >", Passes.FirstPass));
			Assert("should match <USCustomsDisbursementChargeCodes>", ValueProviderToTest.IsResponsibleForReplacing("<USCustomsDisbursementChargeCodes>", Passes.FirstPass));
			Assert("should match < USCustomsDisbursementChargeCodes>", ValueProviderToTest.IsResponsibleForReplacing("< USCustomsDisbursementChargeCodes>", Passes.FirstPass));
			Assert("should match <USCustomsDisbursementChargeCodes >", ValueProviderToTest.IsResponsibleForReplacing("<USCustomsDisbursementChargeCodes >", Passes.FirstPass));
			Assert("should match <uscustomsdisbursementchargecodes >", ValueProviderToTest.IsResponsibleForReplacing("<uscustomsdisbursementchargecodes >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var factory = new BusinessObjectFactory();
			var chargeCode1 = factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = factory.NewWithValidTestData<AccChargeCode>();
			factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode1.PK.ToGuid());

			var coll = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), factory);
			var entryChargeType1 = coll.AddNew();
			entryChargeType1.AC_ChargeCode = chargeCode2.PK;
			entryChargeType1.ChargeType = "DTY";

			var entryChargeType2 = coll.AddNew();
			entryChargeType2.AC_ChargeCode = chargeCode3.PK;
			entryChargeType2.ChargeType = "499";

			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, coll);

			var expected = chargeCode1.PK.ToString() + "|" + chargeCode2.PK.ToString() + "|" + chargeCode3.PK.ToString();
			AssertEquals(expected, ValueProviderToTest.GetReplacement("<USCustomsDisbursementChargeCodes>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new USCustomsDisbursementChargeCodesValueProvider();
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var registryDataType = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.DataType;
			var registryDataType2 = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.DataType;
			using (registryDataType.SuspendValidation())
			using (registryDataType2.SuspendValidation())
			{
				RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new Guid("C83A70E8-BD85-468E-9445-A59003CB4B71"));
				var coll = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
				var entryChargeType1 = coll.AddNew();
				entryChargeType1.AC_ChargeCode = new Guid("2FB5259F-BD20-4B40-8157-D7E7E7E66086");
				entryChargeType1.ChargeType = "DTY";
				var entryChargeType2 = coll.AddNew();
				entryChargeType2.AC_ChargeCode = new Guid("0A5A037C-5E9A-4BD5-B250-07921907797D");
				entryChargeType2.ChargeType = "499";
				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, coll);
				base.AssertExamplesAreReplacedAsExpected(example, expectedResult);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
		}
	}
}
