using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class TaxIdAndTaxMessageMappingValidatorTest : TestCaseWithFactory
	{
		public void TestIsValidateMapping_LineTypes()
		{
			PrepareTaxInfo();

			foreach (var lineType in ValidLineTypeList)
			{
				PrepareMappingValidator(
					(lineType, taxRate1, taxMsg11),
					(lineType, taxRate1, taxMsg12),
					(lineType, taxRate2, taxMsg2)
				);

				AssertLineTypes(new string[] { lineType });
			}
		}

		protected abstract void AssertLineTypes(string[] lineTypes);

		public void TestIsValidateMapping_MixLineTypes()
		{
			var lineTypes = new[] { TransactionLineTypes.Cost, TransactionLineTypes.Revenue };

			PrepareTaxInfo();
			PrepareMappingValidator(
				(TransactionLineTypes.Cost, taxRate1, taxMsg11),
				(TransactionLineTypes.Revenue, taxRate1, taxMsg11),
				(TransactionLineTypes.Cost, taxRate1, taxMsg12)
			);

			AssertMixLineTypes(lineTypes);
		}

		protected abstract void AssertMixLineTypes(string[] lineTypes);

		public void TestIsValidateMapping_OtherLineTypes()
		{
			var otherLineTypes = new[] { "ACR", "AJL", "GJL", "NJL", "RJL", "UCT", "WIP" };

			PrepareTaxInfo();
			PrepareMappingValidator(
				(TransactionLineTypes.Cost, taxRate1, taxMsg11),
				(TransactionLineTypes.Revenue, taxRate2, taxMsg2)
			);

			AssertOtherLineTypes(otherLineTypes);
		}

		protected abstract void AssertOtherLineTypes(string[] otherLineTypes);

		public void TestIsValidateMapping_EmptyValues()
		{
			PrepareTaxInfo();
			PrepareMappingValidator((TransactionLineTypes.Cost, taxRate1, taxMsg11));
			AssertEmptyValues();
		}

		protected abstract void AssertEmptyValues();

		#region Implementation

		protected void PrepareMappingValidator(params (string LineType, AccTaxRate TaxRate, AccInvMsg TaxMessage)[] settings)
		{
			var rules = TestObjectCreator
				.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(settings)
				.TaxIdAndTaxMessageCombinationRulesCollection;
			MappingValidator = CreateMappingValidator(rules);
		}

		protected abstract TaxIdAndTaxMessageMappingValidator CreateMappingValidator(TaxIdAndTaxMessageCombinationRulesCollection rules);

		protected void PrepareTaxInfo()
		{
			taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			taxRate1.AT_Code = "TaxRate01";
			taxMsg11 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg11.A9_Code = "TaxMsg11";
			taxMsg12 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg12.A9_Code = "TaxMsg12";

			taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = AccTaxRate.Types.IntegratedGST;
			taxRate2.AT_Code = "TaxRate02";
			taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";

			taxRate3 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate3.AT_Type = AccTaxRate.Types.Rated;
			taxRate3.AT_Code = "TaxRate03";

			Factory.Save();
		}

		protected AccTaxRate taxRate1, taxRate2, taxRate3;
		protected AccInvMsg taxMsg11, taxMsg12, taxMsg2;

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected TaxIdAndTaxMessageMappingValidator MappingValidator { get; private set; }

		protected IEnumerable<string> ValidLineTypeList = new List<string>() { TransactionLineTypes.Cost, TransactionLineTypes.Revenue, TransactionTypes.DirectPayment, TransactionTypes.DirectReceipt };

		#endregion
	}
}
