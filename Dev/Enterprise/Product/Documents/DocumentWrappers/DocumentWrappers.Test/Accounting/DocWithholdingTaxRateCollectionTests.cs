using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocWithholdingTaxRateCollection))]
	public class DocWithholdingTaxRateCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocWithholdingTaxRateCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var withholding = Factory.New<AccWithholding>();
			return DocWithholdingTaxRate.New(withholding, Factory);
		}

		protected override DocWithholdingTaxRateCollection GetCollectionToTest()
		{
			return new DocWithholdingTaxRateCollection(Factory);
		}
	}
}
