using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.TaxFramework.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTaxTransactionLinePivotCollection))]
	sealed class DocTaxTransactionLinePivotCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocTaxTransactionLinePivotCollection>
	{
		protected override DocTaxTransactionLinePivotCollection GetCollectionToTest()
		{
			return DocTaxTransactionLinePivotCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> DocTaxTransactionLinePivot.New(Factory.New<AccTaxRecordTransactionLinePivot>(), Factory);
	}
}
