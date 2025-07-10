using Enterprise.Accounting.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceTotalRoundingRegistryDataType))]
	public class InvoiceTotalRoundingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InvoiceTotalRoundingRegistryDataType>
	{
		protected override InvoiceTotalRoundingRegistryDataType GetNewDataType() => new InvoiceTotalRoundingRegistryDataType();

		protected override string ExpectedEditorName => "InvoiceTotalRoundingRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new InvoiceTotalRoundingCollection();
			var rule = collection.AddNew();
			rule.Currency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			rule.RoundingOption = AccountingConstants.RoundingOptionsCodes.AlwaysRoundDown;
			rule.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.OneMajorUnit;

			var collection1 = new InvoiceTotalRoundingCollection();
			var rule1 = collection1.AddNew();
			rule1.Currency = Enterprise.Core.Constants.CurrencyCodes.China;
			rule1.RoundingOption = AccountingConstants.RoundingOptionsCodes.AlwaysRoundUp;
			rule1.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.FiftyMinorUnits;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new InvoiceTotalRoundingRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection1, new InvoiceTotalRoundingRegistryDataType().Serialise(collection1))
			};
		}
	}
}
