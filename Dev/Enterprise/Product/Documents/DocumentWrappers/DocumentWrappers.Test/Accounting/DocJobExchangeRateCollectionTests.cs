using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobExchangeRateCollection))]
	public class DocJobExchangeRateCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocJobExchangeRateCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var excRate = Factory.New<ExchangeRate>();
			return DocJobExchangeRate.New(excRate, Factory);
		}

		protected override DocJobExchangeRateCollection GetCollectionToTest()
		{
			return new DocJobExchangeRateCollection(Factory);
		}
	}
}
