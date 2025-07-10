using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeCurrencyRegistryDataType))]
	public class ElectronicProcessingChargeCurrencyRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ElectronicProcessingChargeCurrencyRegistryDataType>
	{
		protected override ElectronicProcessingChargeCurrencyRegistryDataType GetNewDataType()
		{
			return new ElectronicProcessingChargeCurrencyRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);

			var collection = new ElectronicProcessingChargeCurrencyCollection
			{
				new ElectronicProcessingChargeCurrency { CurrencyPK = testObjectCreator.CNY.PK, ValidFromDate = DateTime.Today },
			};

			var collection1 = new ElectronicProcessingChargeCurrencyCollection
			{
				new ElectronicProcessingChargeCurrency { CurrencyPK = testObjectCreator.USD.PK, ValidFromDate = DateTime.Today.AddDays(1) },
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new ElectronicProcessingChargeCurrencyRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection1, new ElectronicProcessingChargeCurrencyRegistryDataType().Serialise(collection1))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "ElectronicProcessingChargeCurrencyRegistryItemEditor"; }
		}
	}
}
