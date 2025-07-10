using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class AccontingHelperClassTest : TestCaseWithFactory
	{
		public void TestDescriptionInDocumentsForTaxAmountsRule()
		{
			AssertEquals(Enterprise.Accounting.Business.AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code, AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule);
		}

		public void TestZeroAmountTaxTypesDescriptionValue()
		{
			var collection = AccountingConfigurationRegistry.Instance.ZeroAmountTaxTypesDescription.Value;
			AssertEquals(8, collection.Count);
			AssertEquals("Exempt", AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt));

			var taxTypesDescriptions = collection.Cast<ZeroAmountTaxTypesDescriptions>().FirstOrDefault(x => x.TaxType == Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt);
			taxTypesDescriptions.OverrideValue = (ZArchitecture.Core.NoResString)"Exempt Override";
			AccountingConfigurationRegistry.Instance.ZeroAmountTaxTypesDescription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("Exempt Override", AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(Enterprise.MasterFiles.Business.AccTaxRate.Types.Exempt));
		}
	}
}
