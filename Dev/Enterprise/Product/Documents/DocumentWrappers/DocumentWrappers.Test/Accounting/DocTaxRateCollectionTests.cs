using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTaxRateCollection))]
	public class DocTaxRateCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocTaxRateCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var accTaxRate = Factory.New<AccTaxRate>();
			return DocTaxRate.New(accTaxRate, Factory);
		}

		protected override DocTaxRateCollection GetCollectionToTest()
		{
			return new DocTaxRateCollection(Factory);
		}
	}
}
