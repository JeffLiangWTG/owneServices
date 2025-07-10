using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(TaxTransactionsLinkedToJobChargeForDisplay))]
	public class TaxTransactionsLinkedToJobChargeForDisplayTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadTaxTransactionsLinkedToJobChargeCollection()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			var taxLinePivot = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();

			var taxTransactionsLinkedToJobChargeForDisplay = new TaxTransactionsLinkedToJobChargeForDisplay(Factory, ZGuid.Empty, ZGuid.Empty);
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			taxRecordLoaderMock.Setup(x => x.LoadTaxRecordsAndPivotsLinkedToTaxableLines(Factory, ZGuid.Empty, ZGuid.Empty))
							.Returns(new System.Collections.Generic.List<(AccTaxTransaction accTaxTransaction, AccTaxRecordTransactionLinePivot accTaxRecordTransactionLinePivot)>() { (taxRecord, taxLinePivot) });
			taxTransactionsLinkedToJobChargeForDisplay.SubstituteTaxRecordLoader_ForTestOnly(taxRecordLoaderMock.Object);
			var collection = taxTransactionsLinkedToJobChargeForDisplay.TaxTransactionsLinkedToJobCharge;
			AssertEquals(1, collection.Count);

			taxRecordLoaderMock.Verify(x => x.LoadTaxRecordsAndPivotsLinkedToTaxableLines(Factory, ZGuid.Empty, ZGuid.Empty), Times.Once);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TaxTransactionsLinkedToJobChargeForDisplay(Factory, ZGuid.Empty, ZGuid.Empty);
		}
	}
}
