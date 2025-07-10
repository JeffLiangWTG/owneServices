using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(OtherTaxesLinkedToTransactionLinesCollection))]
	public class OtherTaxesLinkedToTransactionLinesCollectionTest : ActiveBusinessObjectCollectionTestCase<OtherTaxesLinkedToTransactionLinesCollection>
	{
		public override void TestDelete()
		{
			var element = GetNewElementToAddToTheCollection() as AccTaxTransaction;
			var collection = GetCollectionToTest();
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			var inputParams = new object[2];
			taxProcessorMock.Setup(t => t.DeleteTaxRecordNotInDB(It.IsAny<ITaxRecordParent>(), It.IsAny<AccTaxTransaction>())).Callback<ITaxRecordParent, AccTaxTransaction>((t1, t2) => { inputParams[0] = t1; inputParams[1] = t2; });
			collection.Delete(element);
			AssertEquals(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(line.InvoiceBase), inputParams[0]);
			AssertEquals(element, inputParams[1]);
		}

		protected override OtherTaxesLinkedToTransactionLinesCollection GetCollectionToTest()
		{
			return new OtherTaxesLinkedToTransactionLinesCollection(line, TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(line.InvoiceBase));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var accTaxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			var pivot = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot.ATP_ATT = accTaxTransaction.PK;
			pivot.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			return accTaxTransaction;
		}

		public void TestOtherTaxesLinkedToTransactionLinesCollection()
		{
			var accTaxTransaction1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			var accTaxTransaction2 = Factory.NewWithValidTestData<AccTaxTransaction>();

			var pivot1 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			var pivot2 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			var pivot3 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();

			var line1 = Factory.NewWithValidTestData<APInvoiceLine>();
			var line2 = Factory.NewWithValidTestData<APInvoiceLine>();

			pivot1.ATP_ATT = accTaxTransaction1.PK;
			pivot1.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line1));
			pivot2.ATP_ATT = accTaxTransaction1.PK;
			pivot2.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line2));
			pivot3.ATP_ATT = accTaxTransaction2.PK;
			pivot3.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line2));

			var collection = new OtherTaxesLinkedToTransactionLinesCollection(line1, TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(line1.InvoiceBase));
			AssertContainsExactElementsInAnyOrder(new[] { accTaxTransaction1.PK }, collection.Select(t => t.PK));

			collection = new OtherTaxesLinkedToTransactionLinesCollection(line2, TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(line2.InvoiceBase));
			AssertContainsExactElementsInAnyOrder(new[] { accTaxTransaction1.PK, accTaxTransaction2.PK }, collection.Select(t => t.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var creator = new TestObjectCreator(Factory);
			var invoice = creator.CreateInvoice(typeof(APInvoice));
			line = creator.CreateInvoiceLine(invoice, 100m);
		}

		InvoicingLineBase line;
	}
}

