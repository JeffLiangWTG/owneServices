using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WHTAmountLoaderHelperTest : TestCaseWithFactory
	{
		public void TestWHTAmountLoadingFromFactory_IsWHTRealizationInProgressTrue()
		{
			WHTAmountLoadingFromFactory(true);
		}

		public void TestWHTAmountLoadingFromFactory_IsWHTRealizationInProgressFalse()
		{
			WHTAmountLoadingFromFactory(false);
		}

		void WHTAmountLoadingFromFactory(bool isRealizationInProgress)
		{
			var factory = new BusinessObjectFactory();

			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) amnt = (Guid.NewGuid(), 20M, 45M);

			var loaderMock = new Mock<IWHTAmountLoader>();
			var inputNotionalPK = ZGuid.Empty;
			var inputRealizedPK = ZGuid.Empty;
			loaderMock.SetupProperty(x => x.IsWHTRealizationInProgress);
			loaderMock.Setup(x => x.RegisterForLoadingWHTAmounts(amnt.TransactionPK));
			loaderMock.Setup(x => x.GetNotionalWHT(It.IsAny<ZGuid>())).Returns(20M).Callback<ZGuid>(x => inputNotionalPK = x);
			loaderMock.Setup(x => x.GetRealizedWHT(It.IsAny<ZGuid>())).Returns(45M).Callback<ZGuid>(x => inputRealizedPK = x);

			TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(factory, loaderMock.Object);
			factory.RegisterTransactionsForLoadingWHTAmounts(isRealizationInProgress, amnt.TransactionPK);

			loaderMock.VerifySet(x => x.IsWHTRealizationInProgress = isRealizationInProgress);
			loaderMock.Verify(x => x.RegisterForLoadingWHTAmounts(amnt.TransactionPK), Times.Once);
			AssertEquals(20M, factory.LoadNotionalWHT(amnt.TransactionPK));
			AssertEquals(amnt.TransactionPK, inputNotionalPK);
			AssertEquals(45M, factory.LoadRealizedWHT(amnt.TransactionPK));
			AssertEquals(amnt.TransactionPK, inputRealizedPK);
		}
	}
}