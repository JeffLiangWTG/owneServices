using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeCurrencyCollection))]
	public class ElectronicProcessingChargeCurrencyCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ElectronicProcessingChargeCurrencyCollection>
	{
		protected override ElectronicProcessingChargeCurrencyCollection GetCollectionToTest()
		{
			return new ElectronicProcessingChargeCurrencyCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ElectronicProcessingChargeCurrency();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;
	}
}
