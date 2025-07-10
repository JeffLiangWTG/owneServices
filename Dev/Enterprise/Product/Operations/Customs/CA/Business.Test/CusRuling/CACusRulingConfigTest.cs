using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using ConfigTypes = Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACusRulingConfig))]
	sealed class CACusRulingConfigTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZZY_Rate()
		{
			var cusRuling = Factory.New<CACusRuling>();
			var config = cusRuling.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DTY, ConfigTypes.Specific, 5, CustomsUnitOfMeasureList.Codes.Centilitre);
			config.ZZY_Type = ConfigTypes.AcceptAmount;
			AssertEquals(0.000M, config.ZZY_Rate);
		}

		public void TestZZY_Value()
		{
			var cusRuling = Factory.New<CACusRuling>();
			var config = cusRuling.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DTY, ConfigTypes.Specific, 5, CustomsUnitOfMeasureList.Codes.Centilitre);
			config.ZZY_Type = ConfigTypes.AcceptRate;
			AssertEquals("", config.ZZY_Value);
		}

		public void TestValidationForMaxAndMin()
		{
			var cusRuling = Factory.New<CACusRuling>();
			var config = cusRuling.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DTY, ConfigTypes.Maximum, 0, ZString.Empty);
			config.Validation.ValidateAll();
			AssertHasErrors(config.ZZY_ValueInfo);
			config.ZZY_Value = "1.00";
			AssertNoErrors(config.ZZY_RateInfo);

			var config2 = cusRuling.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DTY, ConfigTypes.Minimum, 0, ZString.Empty);
			config2.Validation.ValidateAll();
			AssertHasErrors(config2.ZZY_ValueInfo);
			config2.ZZY_Rate = 2.00m;
			AssertNoErrors(config2.ZZY_ValueInfo);
		}

		public void TestRateReadonly()
		{
			AssertConfigRateReadonly(RefCusRulingConfigCategories.Codes.GST);
			AssertConfigRateReadonly(RefCusRulingConfigCategories.Codes.EXC);
			AssertConfigRateReadonly(RefCusRulingConfigCategories.Codes.DTY);
			AssertConfigRateReadonly(RefCusRulingConfigCategories.Codes.EXD);
			AssertConfigRateReadonly(RefCusRulingConfigCategories.Codes.SIM);

			var cusRuling = Factory.New<CACusRuling>();
			var config = cusRuling.Configurations.AddNew();
			config.ZZY_Category = RefCusRulingConfigCategories.Codes.DTY;
			config.ZZY_Type = ConfigTypes.TreatmentCode;
			Assert("Readonly DTY TreatmentCode", config.ZZY_RateInfo.ReadOnly);

			config.ZZY_Category = RefCusRulingConfigCategories.Codes.EXD;
			config.ZZY_Type = ConfigTypes.TreatmentCode;
			Assert("Readonly EXD TreatmentCode", config.ZZY_RateInfo.ReadOnly);

			config.ZZY_Category = RefCusRulingConfigCategories.Codes.GST;
			config.ZZY_Type = ConfigTypes.ExemptCode;
			Assert("Readonly GST ExemptCode", config.ZZY_RateInfo.ReadOnly);

			config.ZZY_Category = RefCusRulingConfigCategories.Codes.SIM;
			config.ZZY_Type = ConfigTypes.ExemptCode;
			Assert("Readonly SIM ExemptCode", config.ZZY_RateInfo.ReadOnly);

			config.ZZY_Category = RefCusRulingConfigCategories.Codes.EXC;
			config.ZZY_Type = ConfigTypes.ExemptCode;
			Assert("Readonly EXC ExemptCode", config.ZZY_RateInfo.ReadOnly);

			config.ZZY_Category = RefCusRulingConfigCategories.Codes.DAT;
			config.ZZY_Type = ConfigTypes.REL;
			Assert("Readonly DAT RELCode", config.ZZY_RateInfo.ReadOnly);

			config.ZZY_Category = RefCusRulingConfigCategories.Codes.DAT;
			config.ZZY_Type = ConfigTypes.DSD;
			Assert("Readonly DAT DSDCode", config.ZZY_RateInfo.ReadOnly);
		}

		void AssertConfigRateReadonly(ZString category)
		{
			CombineAssertions(category, () =>
			{
				var cusRuling = Factory.New<CACusRuling>();
				cusRuling.ZZX_RulingType = RefCusRulingConfigValuesForAcceptType.Codes.T;
				var config = cusRuling.Configurations.AddNew();
				config.ZZY_Category = category;
				config.ZZY_Type = ConfigTypes.NoneFree;
				Assert("Readonly NoneFree", config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.AcceptAmount;
				Assert("Readonly AcceptAmount", config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.AcceptRate;
				Assert("Readonly AcceptRate", config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.Maximum;
				Assert("Not Readonly Maximum", !config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Value = "XXX";
				Assert("Not Readonly Maximum invalid value", !config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Value = "0";
				Assert("Not Readonly Maximum value 0", !config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Value = "1";
				Assert("Readonly Maximum when value is not 0", config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Value = ZString.Empty;
				Assert("Not Readonly Maximum when value is empty", !config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.Minimum;
				Assert("Not Readonly Minimum value 0", !config.ZZY_RateInfo.ReadOnly);
				config.ZZY_Value = "1";
				Assert("Readonly Minimum value not 0", config.ZZY_RateInfo.ReadOnly);
			});
		}

		public void TestValueReadonly()
		{
			AssertConfigValueReadonly(RefCusRulingConfigCategories.Codes.GST);
			AssertConfigValueReadonly(RefCusRulingConfigCategories.Codes.EXC);
			AssertConfigValueReadonly(RefCusRulingConfigCategories.Codes.DTY);
			AssertConfigValueReadonly(RefCusRulingConfigCategories.Codes.SIM);

			var cusRuling = Factory.New<CACusRuling>();
			var config = cusRuling.Configurations.AddNew();
			config.ZZY_Category = RefCusRulingConfigCategories.Codes.DAT;
			config.ZZY_Type = ConfigTypes.REL;
			Assert("Readonly DAT RELCode", config.ZZY_ValueInfo.ReadOnly);

			config.ZZY_Category = RefCusRulingConfigCategories.Codes.DAT;
			config.ZZY_Type = ConfigTypes.DSD;
			Assert("Readonly DAT DSDCode", config.ZZY_ValueInfo.ReadOnly);
		}

		void AssertConfigValueReadonly(ZString category)
		{
			CombineAssertions(category, () =>
			{
				var cusRuling = Factory.New<CACusRuling>();
				cusRuling.ZZX_RulingType = RefCusRulingConfigValuesForAcceptType.Codes.T;
				var config = cusRuling.Configurations.AddNew();
				config.ZZY_Category = category;
				config.ZZY_Type = ConfigTypes.NoneFree;
				Assert("Readonly NoneFree", config.ZZY_ValueInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.AcceptRate;
				Assert("Readonly AcceptRate", config.ZZY_ValueInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.AdValorem;
				Assert("Readonly AdValorem", config.ZZY_ValueInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.Specific;
				Assert("Readonly Specific", config.ZZY_ValueInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.Maximum;
				config.ZZY_Rate = 10.0m;
				Assert("Readonly Maximum when rate not zero", config.ZZY_ValueInfo.ReadOnly);
				config.ZZY_Rate = 0m;
				Assert("Not Readonly Maximum when rate zero", !config.ZZY_ValueInfo.ReadOnly);
				config.ZZY_Type = ConfigTypes.Minimum;
				config.ZZY_Rate = 10.0m;
				Assert("Readonly Minimum when rate not zero", config.ZZY_ValueInfo.ReadOnly);
				config.ZZY_Rate = 0m;
				Assert("Not Readonly Maximum when rate zero", !config.ZZY_ValueInfo.ReadOnly);
			});
		}

		public void TestGetValueFieldType()
		{
			AssertConfigValueFieldType(RefCusRulingConfigCategories.Codes.GST);
			AssertConfigValueFieldType(RefCusRulingConfigCategories.Codes.EXC);
			AssertConfigValueFieldType(RefCusRulingConfigCategories.Codes.DTY);
			AssertConfigValueFieldType(RefCusRulingConfigCategories.Codes.SIM);
			AssertConfigValueFieldType(RefCusRulingConfigCategories.Codes.EXD);

			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var config = cusRuling.Configurations.AddNew();
			config.ZZY_Category = RefCusRulingConfigCategories.Codes.DTY;
			config.ZZY_Type = ConfigTypes.TreatmentCode;
			AssertEquals("DTY TreatmentCode", nameof(FieldType.TextDropEdit), config.ValueFieldType);
			config.ZZY_Category = RefCusRulingConfigCategories.Codes.EXD;
			config.ZZY_Type = ConfigTypes.TreatmentCode;
			AssertEquals("EXD TreatmentCode", nameof(FieldType.TextDropEdit), config.ValueFieldType);
			config.ZZY_Category = RefCusRulingConfigCategories.Codes.EXC;
			config.ZZY_Type = ConfigTypes.ExemptCode;
			AssertEquals("EXC ExemptCode", nameof(FieldType.TextDropEdit), config.ValueFieldType);
			config.ZZY_Category = RefCusRulingConfigCategories.Codes.GST;
			config.ZZY_Type = ConfigTypes.ExemptCode;
			AssertEquals("GST ExemptCode", nameof(FieldType.TextDropEdit), config.ValueFieldType);
			config.ZZY_Category = RefCusRulingConfigCategories.Codes.SIM;
			config.ZZY_Type = ConfigTypes.ExemptCode;
			AssertEquals("SIM ExemptCode", nameof(FieldType.TextDropEdit), config.ValueFieldType);
		}

		void AssertConfigValueFieldType(ZString category)
		{
			CombineAssertions(category, () =>
			{
				var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
				var config = cusRuling.Configurations.AddNew();
				AssertEquals("Empty Category", nameof(FieldType.Text), config.ValueFieldType);
				config.ZZY_Category = "XXX";
				AssertEquals("Invalid Category", nameof(FieldType.Text), config.ValueFieldType);
				config.ZZY_Category = category;
				config.ZZY_Type = ConfigTypes.AcceptAmount;
				AssertEquals("Decimal AcceptAmount", nameof(FieldType.Decimal), config.ValueFieldType);
				config.ZZY_Type = ConfigTypes.Maximum;
				AssertEquals("Decimal Maximum", nameof(FieldType.Decimal), config.ValueFieldType);
				config.ZZY_Type = ConfigTypes.Minimum;
				AssertEquals("Decimal Minimum", nameof(FieldType.Decimal), config.ValueFieldType);
			});
		}

		public void TestLookups()
		{
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var config = cusRuling.Configurations.AddNew();
			AssertType<CACusRulingConfigLookups>(config.Lookups);
		}

		public void TestValidation()
		{
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var config = cusRuling.Configurations.AddNew();
			AssertType<CACusRulingConfigValidation>(config.Validation);
		}

		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return config;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			TestSaveAndDelete();
		}

		[ExpectNoExceptions]
		public void TestSaveAndDelete()
		{
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var config = cusRuling.Configurations.AddNew();
			config.ZZY_Category = RefCusRulingConfigCategories.Codes.DTY;
			config.ZZY_Type = ConfigTypes.AcceptAmount;
			Factory.Save();
			config.Delete();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return config;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cusRuling = Factory.New<CACusRuling>();
			cusRuling.ZZX_Description = "11111 Des";
			cusRuling.ZZX_RulingNumber = "11111";
			cusRuling.ZZX_RulingType = RefCusRulingConfigValuesForAcceptType.Codes.X;
			cusRuling.ZZX_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			config = cusRuling.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DTY, ConfigTypes.AcceptAmount, 0, RefCusRulingConfigValuesForAcceptType.Codes.X);
		}
		CACusRulingConfig config;
		#endregion
	}
}
