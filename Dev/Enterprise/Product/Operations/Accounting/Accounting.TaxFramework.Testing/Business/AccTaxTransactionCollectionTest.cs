using System.ComponentModel;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using IntegrationAccounting = Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(AccTaxTransactionCollection))]
	public class AccTaxTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<AccTaxTransactionCollection>
	{
		public override void TestDelete()
		{
			var element = GetNewElementToAddToTheCollection() as AccTaxTransaction;
			var collection = GetCollectionToTest();
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			var inputParams = new object[2];
			taxProcessorMock.Setup(t => t.DeleteTaxRecordNotInDB(It.IsAny<ITaxRecordParent>(), It.IsAny<AccTaxTransaction>())).Callback<ITaxRecordParent, AccTaxTransaction>((t1, t2) => { inputParams[0] = t1; inputParams[1] = t2; Assert(Factory.HasContext(IntegrationAccounting.BusinessContext.MakingChangesToOtherTaxes)); });
			Assert(!Factory.HasContext(IntegrationAccounting.BusinessContext.MakingChangesToOtherTaxes));
			collection.Delete(element);
			Assert(!Factory.HasContext(IntegrationAccounting.BusinessContext.MakingChangesToOtherTaxes));
			AssertEquals(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(transaction), inputParams[0]);
			AssertEquals(element, inputParams[1]);
		}

		public void TestAllowNew()
		{
			Assert(!((IBindingList)GetCollectionToTest()).AllowNew);
		}

		protected override AccTaxTransactionCollection GetCollectionToTest()
		{
			return new AccTaxTransactionCollection(Factory, transaction, TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(transaction));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = transaction.PK;
			return taxTransaction;
		}

		protected override void SetUp()
		{
			base.SetUp();
			transaction = Factory.NewWithValidTestData<APInvoice>();
		}

		InvoicingBase transaction;
	}
}
