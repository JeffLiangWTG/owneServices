using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EnforceZeroBalanceDisbursementsConfiguration))]
	public class EnforceZeroBalanceDisbursementsConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		FallbackLevel CurrentFallbackLevel
		{
			get
			{
				if (fallbackLevel == null)
				{
					fallbackLevel = NewFallbackLevel();
				}
				return fallbackLevel;
			}
		}
		FallbackLevel fallbackLevel;

		public void TestIsMaximumVarianceApplicable()
		{
			var config = new EnforceZeroBalanceDisbursementsConfiguration(CurrentFallbackLevel, Factory);
			Assert("Maximum Variance should not be applicable", !config.IsMaximumVarianceApplicable);
			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.OSAmount.Code;
			Assert("Maximum Variance should not be applicable", !config.IsMaximumVarianceApplicable);
			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.LocalAmount.Code;
			Assert("Maximum Variance should be applicable", config.IsMaximumVarianceApplicable);
			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.Either.Code;
			Assert("Maximum Variance should be applicable", config.IsMaximumVarianceApplicable);
			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.Both.Code;
			Assert("Maximum Variance should be applicable", config.IsMaximumVarianceApplicable);
		}

		public void TestMaximumVarianceForNonZeroValueAsDefault()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var company = Env.CurrentCompany as GlbCompany;
			company.GC_RX_NKLocalCurrency = creator.AUD.Code;
			Factory.Save();
			var config = new EnforceZeroBalanceDisbursementsConfiguration(CurrentFallbackLevel, Factory);

			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.Default.Code;
			AssertEquals("Should not yet set default value for maximum variance as it is not applicable for the selected EnforceZeroBalanceDisbursementsOption", new ZDecimal(0), config.MaximumVariance);
			AssertEquals("Should not yet set default value for unit decimals as it is not applicable for the selected EnforceZeroBalanceDisbursementsOption", new ZInt(0), config.UnitDecimals);

			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.OSAmount.Code;
			AssertEquals("Should not yet set default value for maximum variance as it is not applicable for the selected EnforceZeroBalanceDisbursementsOption", new ZDecimal(0), config.MaximumVariance);
			AssertEquals("Should not yet set default value for unit decimals as it is not applicable for the selected EnforceZeroBalanceDisbursementsOption", new ZInt(0), config.UnitDecimals);

			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.Both.Code;
			AssertEquals("Should get default Maximum Variance according to the fallback company's local currency", new ZDecimal(0.1), config.MaximumVariance);
			AssertEquals("Should get default UnitDecimals according to the fallback company's local currency", new ZInt(2), config.UnitDecimals);

			config.MaximumVariance = 0.23;
			AssertEquals("Maximum Variance should be set to new value", new ZDecimal(0.23), config.MaximumVariance);
			AssertEquals("Unit Decimals value should be unchanged", new ZInt(2), config.UnitDecimals);

			config.MaximumVariance = -0.2;
			AssertEquals("Maximum Variance should be set to new value but with error", new ZDecimal(-0.2), config.MaximumVariance);
			AssertHasError("Maximum Variance should be set to new value but with error", config.MaximumVarianceInfo, "Enter a numeric value greater than or equal to zero for Maximum Variance.");
			AssertEquals("Unit Decimals value should be unchanged", new ZInt(2), config.UnitDecimals);

			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.Default.Code;
			AssertEquals("Should reset maximum variance as it is not applicable for the selected EnforceZeroBalanceDisbursementsOption", new ZDecimal(0), config.MaximumVariance);
			AssertEquals("Should reset unit decimals as it is not applicable for the selected EnforceZeroBalanceDisbursementsOption", new ZInt(0), config.UnitDecimals);

			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.LocalAmount.Code;
			AssertEquals("Should get default Maximum Variance according to the fallback company's local currency", new ZDecimal(0.1), config.MaximumVariance);
			AssertEquals("Should get default UnitDecimals according to the fallback company's local currency", new ZInt(2), config.UnitDecimals);

			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.OSAmount.Code;
			AssertEquals("Should reset maximum variance as it is not applicable for the selected EnforceZeroBalanceDisbursementsOption", new ZDecimal(0), config.MaximumVariance);
			AssertEquals("Should reset unit decimals as it is not applicable for the selected EnforceZeroBalanceDisbursementsOption", new ZInt(0), config.UnitDecimals);

			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.Either.Code;
			AssertEquals("Should get default Maximum Variance according to the fallback company's local currency", new ZDecimal(0.1), config.MaximumVariance);
			AssertEquals("Should get default UnitDecimals according to the fallback company's local currency", new ZInt(2), config.UnitDecimals);
		}

		public void TestMaximumVarianceForZeroValueAsDefault()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var company = Env.CurrentCompany as GlbCompany;
			company.SetCurrency(null);
			Factory.Save();
			var config = new EnforceZeroBalanceDisbursementsConfiguration(CurrentFallbackLevel, Factory);
			config.EnforceZeroBalanceDisbursementsValidationType = EnforceZeroBalanceDisbursementsOption.Both.Code;
			AssertEquals("Should get default Maximum Variance as zero due to null currency", new ZDecimal(0), config.MaximumVariance);
			AssertEquals("Should get default Unit Decimals as zero due to null currency", new ZInt(0), config.UnitDecimals);
		}

		public void TestEnforceZeroBalanceDisbursementsValidationTypes()
		{
			var config = new EnforceZeroBalanceDisbursementsConfiguration(CurrentFallbackLevel, Factory);
			AssertEquals("Should return correct default validation type", EnforceZeroBalanceDisbursementsOption.Default.Code, config.EnforceZeroBalanceDisbursementsValidationType);
			AssertEquals("Should return all validation types", EnforceZeroBalanceDisbursementsOption.CodeList, config.EnforceZeroBalanceDisbursementsValidationTypes);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var fallbackLevel = (BizObj?.CurrentFallbackLevel) ?? NewFallbackLevel();
			return new EnforceZeroBalanceDisbursementsConfiguration(fallbackLevel, Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (EnforceZeroBalanceDisbursementsConfiguration)GetNewBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return (EnforceZeroBalanceDisbursementsConfiguration)GetNewBusinessObject();
		}

		protected new EnforceZeroBalanceDisbursementsConfiguration BizObj
		{
			get { return (EnforceZeroBalanceDisbursementsConfiguration)base.BizObj; }
		}

		#endregion
	}
}
