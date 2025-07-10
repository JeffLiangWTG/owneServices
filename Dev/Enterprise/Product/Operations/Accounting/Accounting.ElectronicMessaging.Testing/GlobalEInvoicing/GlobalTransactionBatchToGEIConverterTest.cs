using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	public class GlobalTransactionBatchToGEIConverterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConvert()
		{
			Helper.SetupControlAccounts();
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, TestObjectCreator.NonCurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.NonCurrentCompany.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetTransactionBatchToPayloadWriter()).Returns(new Mock<ITransactionBatchToPayloadWriter>().Object);

			var converter = new GlobalTransactionBatchToGEIConverter(countryFactoryMock.Object);
			var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);

			countryFactoryMock.Verify(x => x.GetTransactionBatchToPayloadWriter(), Times.Once);
			countryFactoryMock.Verify(x => x.CountryCode);
		}

		TestObjectCreator TestObjectCreator
			=> testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		EInvoicingTestHelper Helper
			=> helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper helper;
	}
}
