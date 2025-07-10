using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ZeroAmountTaxTypesDescriptionsCollection))]
	public class ZeroAmountTaxTypesDescriptionsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ZeroAmountTaxTypesDescriptionsCollection>
	{
		#region AllowNew

		public void TestAllowNew()
		{
			AssertEquals("Must not allow new rows", false, Collection.AllowNew);
		}

		#endregion

		#region Overrides

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ZeroAmountTaxTypesDescriptionsCollection GetCollectionToTest()
		{
			return new ZeroAmountTaxTypesDescriptionsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ZeroAmountTaxTypesDescriptions();
		}

		#endregion

		#region TestGetDefault

		public void TestGetDefault()
		{
			var defaultValue = ZeroAmountTaxTypesDescriptionsCollection.GetDefault();
			AssertEquals(8, defaultValue.Count);

			AssertDefaultZeroAmountTaxTypesDescriptions(
				defaultValue[0],
				AccTaxRate.Types.Rated,
				ZeroAmountTaxTypesDescriptionsCollection.ZeroAmountTaxTypeDescription.ZeroRate,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.Rated));
			AssertDefaultZeroAmountTaxTypesDescriptions(
				defaultValue[1],
				AccTaxRate.Types.Exempt,
				ZeroAmountTaxTypesDescriptionsCollection.ZeroAmountTaxTypeDescription.Exempt,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.Exempt));
			AssertDefaultZeroAmountTaxTypesDescriptions(
				defaultValue[2],
				AccTaxRate.Types.NotReportable,
				ZeroAmountTaxTypesDescriptionsCollection.ZeroAmountTaxTypeDescription.NotReportable,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.NotReportable));
			AssertDefaultZeroAmountTaxTypesDescriptions(
				defaultValue[3],
				AccTaxRate.Types.ReverseRated,
				ZeroAmountTaxTypesDescriptionsCollection.ZeroAmountTaxTypeDescription.Reverse,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.ReverseRated));
			AssertDefaultZeroAmountTaxTypesDescriptions(
				defaultValue[4],
				AccTaxRate.Types.Suspended,
				ZeroAmountTaxTypesDescriptionsCollection.ZeroAmountTaxTypeDescription.Suspended,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.Suspended));
			AssertDefaultZeroAmountTaxTypesDescriptions(
				defaultValue[5],
				AccTaxRate.Types.ReportableUnderBusinessTax,
				ZeroAmountTaxTypesDescriptionsCollection.ZeroAmountTaxTypeDescription.ReportableUnderBusinessTax,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.ReportableUnderBusinessTax));
			AssertDefaultZeroAmountTaxTypesDescriptions(
				defaultValue[6],
				AccTaxRate.Types.ExcludedFromTheTaxBase,
				ZeroAmountTaxTypesDescriptionsCollection.ZeroAmountTaxTypeDescription.ExcludedFromTheTaxBase,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.ExcludedFromTheTaxBase));
			AssertDefaultZeroAmountTaxTypesDescriptions(
				defaultValue[7],
				AccTaxRate.Types.CapitalRated,
				ZeroAmountTaxTypesDescriptionsCollection.ZeroAmountTaxTypeDescription.ZeroRatedCapital,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.CapitalRated));
		}

		void AssertDefaultZeroAmountTaxTypesDescriptions(ZeroAmountTaxTypesDescriptions zeroAmountTaxTypesDescriptions, string taxType, string description, MultilingualString defaultValue)
		{
			AssertEquals(taxType, zeroAmountTaxTypesDescriptions.TaxType);
			AssertEquals(description, zeroAmountTaxTypesDescriptions.Description);
			AssertEquals(defaultValue, zeroAmountTaxTypesDescriptions.DefaultValue);
		}

		#endregion
	}
}
