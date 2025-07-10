using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WHTAmountLoaderTest : TestCaseWithFactory
	{
		public void TestWHTAmountLoading_CalculatedFromTaxTransaction()
		{
			var factory = new BusinessObjectFactory();

			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) amnts = (Guid.NewGuid(), 20M, 45M);
			ZGuid[] passedTransactionPKs = null;

			var taxTransactionBasedWHTAmountCalculatorMock = new Mock<IWHTAmountCalculator>();
			var apJournalBasedWHTAmountCalculatorMock = new Mock<IWithholdingJournalCreationManager>();
			TaxFrameworkObjectFactory.SubstituteAPJournalBasedWHTAmountCalculator_ForTestOnly(factory, apJournalBasedWHTAmountCalculatorMock.Object);

			using (ObjectFactory.Substitute(taxTransactionBasedWHTAmountCalculatorMock.Object))
			{
				SetupMockWHTAmountCalculator(taxTransactionBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(amnts), (x) => passedTransactionPKs = x);
				SetupMockAPJournalBasedWHTAmountCalculator(apJournalBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(amnts).First(), x => passedTransactionPKs = new[] { x });
				var loader = new WHTAmountLoader(factory) { IsWHTRealizationInProgress = false };
				AssertWHTAmount(loader.GetNotionalWHT(amnts.TransactionPK), loader.GetRealizedWHT(amnts.TransactionPK), 20M, 45M);
				AssertContainsExactElementsInAnyOrder(new[] { amnts.TransactionPK }, passedTransactionPKs);
				taxTransactionBasedWHTAmountCalculatorMock.Verify(x => x.Calculate(It.IsAny<ZGuid[]>()), Times.Once);
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateNotionalWHT(It.IsAny<ZGuid>()), Times.Never);
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateRealizedWHT(It.IsAny<ZGuid>()), Times.Never);
			}
		}

		public void TestWHTAmountLoading_CalculatedFromAPJournal()
		{
			var factory = new BusinessObjectFactory();

			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) amnts = (Guid.NewGuid(), 20M, 45M);
			ZGuid[] passedTransactionPKs = null;

			var taxTransactionBasedWHTAmountCalculatorMock = new Mock<IWHTAmountCalculator>();
			var apJournalBasedWHTAmountCalculatorMock = new Mock<IWithholdingJournalCreationManager>();
			TaxFrameworkObjectFactory.SubstituteAPJournalBasedWHTAmountCalculator_ForTestOnly(factory, apJournalBasedWHTAmountCalculatorMock.Object);

			using (ObjectFactory.Substitute(taxTransactionBasedWHTAmountCalculatorMock.Object))
			{
				SetupMockWHTAmountCalculator(taxTransactionBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(amnts), (x) => passedTransactionPKs = x);
				SetupMockAPJournalBasedWHTAmountCalculator(apJournalBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(amnts).First(), x => passedTransactionPKs = new[] { x });

				var loader = new WHTAmountLoader(factory) { IsWHTRealizationInProgress = true };
				AssertWHTAmount(loader.GetNotionalWHT(amnts.TransactionPK), loader.GetRealizedWHT(amnts.TransactionPK), 20M, 45M);
				AssertContainsExactElementsInAnyOrder(new[] { amnts.TransactionPK }, passedTransactionPKs);
				taxTransactionBasedWHTAmountCalculatorMock.Verify(x => x.Calculate(It.IsAny<ZGuid[]>()), Times.Never);
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateNotionalWHT(It.IsAny<ZGuid>()), Times.Exactly(1));
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateRealizedWHT(It.IsAny<ZGuid>()), Times.Exactly(1));
			}
		}

		public void TestWHTAmountLoading_ValuesAreCached()
		{
			var factory = new BusinessObjectFactory();

			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) amnts = (Guid.NewGuid(), 20M, 45M);
			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) modifiedAmnts = (Guid.NewGuid(), 10M, 35M);
			ZGuid[] passedTransactionPKs = null;

			var taxTransactionBasedWHTAmountCalculatorMock = new Mock<IWHTAmountCalculator>();

			using (ObjectFactory.Substitute(taxTransactionBasedWHTAmountCalculatorMock.Object))
			{
				SetupMockWHTAmountCalculator(taxTransactionBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(amnts), x => passedTransactionPKs = x);
				var loader = new WHTAmountLoader(factory) { IsWHTRealizationInProgress = false };
				AssertAmounts();
				taxTransactionBasedWHTAmountCalculatorMock.Verify(x => x.Calculate(It.IsAny<ZGuid[]>()), Times.Once);

				//Values in DB has changed.
				SetupMockWHTAmountCalculator(taxTransactionBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(modifiedAmnts), x => passedTransactionPKs = x);
				//loader will return value from cache
				AssertAmounts();
				taxTransactionBasedWHTAmountCalculatorMock.Verify(x => x.Calculate(It.IsAny<ZGuid[]>()), Times.Once);

				void AssertAmounts()
				{
					AssertWHTAmount(loader.GetNotionalWHT(amnts.TransactionPK), loader.GetRealizedWHT(amnts.TransactionPK), 20M, 45M);
					AssertContainsExactElementsInAnyOrder(new[] { amnts.TransactionPK }, passedTransactionPKs);
				}
			}
		}

		public void TestWHTAmountLoading_ValuesAreNotCachedWhenCalculatedFromAPJournal()
		{
			var factory = new BusinessObjectFactory();

			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) amnts = (Guid.NewGuid(), 20M, 45M);
			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) modifiedAmnts = (Guid.NewGuid(), 10M, 35M);

			ZGuid[] passedTransactionPKs = null;

			var apJournalBasedWHTAmountCalculatorMock = new Mock<IWithholdingJournalCreationManager>();
			TaxFrameworkObjectFactory.SubstituteAPJournalBasedWHTAmountCalculator_ForTestOnly(factory, apJournalBasedWHTAmountCalculatorMock.Object);

			using (ObjectFactory.Substitute(apJournalBasedWHTAmountCalculatorMock.Object))
			{
				var loader = new WHTAmountLoader(factory) { IsWHTRealizationInProgress = true };

				SetupMockAPJournalBasedWHTAmountCalculator(apJournalBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(amnts).First(), x => passedTransactionPKs = new[] { x });
				AssertWHTAmount(loader.GetNotionalWHT(amnts.TransactionPK), loader.GetRealizedWHT(amnts.TransactionPK), 20M, 45M);
				AssertContainsExactElementsInAnyOrder(new[] { amnts.TransactionPK }, passedTransactionPKs);
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateNotionalWHT(It.IsAny<ZGuid>()), Times.Exactly(1));
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateRealizedWHT(It.IsAny<ZGuid>()), Times.Exactly(1));

				//APJournal has changed.
				SetupMockAPJournalBasedWHTAmountCalculator(apJournalBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(modifiedAmnts).First(), x => passedTransactionPKs = new[] { x });
				//loader will ask for recalculated value from APJournal
				AssertWHTAmount(loader.GetNotionalWHT(amnts.TransactionPK), loader.GetRealizedWHT(amnts.TransactionPK), 10M, 35M);
				AssertContainsExactElementsInAnyOrder(new[] { amnts.TransactionPK }, passedTransactionPKs);
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateNotionalWHT(It.IsAny<ZGuid>()), Times.Exactly(2));
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateRealizedWHT(It.IsAny<ZGuid>()), Times.Exactly(2));
			}
		}

		public void TestWHTAmountLoading_FallBackToTaxTransactionBasedCalculation()
		{
			var factory = new BusinessObjectFactory();

			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) amnts = (Guid.NewGuid(), 20M, 45M);
			ZGuid[] passedTransactionPKs = null;

			var taxTransactionBasedWHTAmountCalculatorMock = new Mock<IWHTAmountCalculator>();
			var apJournalBasedWHTAmountCalculatorMock = new Mock<IWithholdingJournalCreationManager>();
			TaxFrameworkObjectFactory.SubstituteAPJournalBasedWHTAmountCalculator_ForTestOnly(factory, apJournalBasedWHTAmountCalculatorMock.Object);

			using (ObjectFactory.Substitute(taxTransactionBasedWHTAmountCalculatorMock.Object))
			{
				var loader = new WHTAmountLoader(factory) { IsWHTRealizationInProgress = true };

				SetupMockWHTAmountCalculator(taxTransactionBasedWHTAmountCalculatorMock, MockWithheldTaxAmounts(amnts), x => passedTransactionPKs = x);
				//No APJournal has been created. Hence we are passing Null
				SetupMockAPJournalBasedWHTAmountCalculator(apJournalBasedWHTAmountCalculatorMock, null, x => passedTransactionPKs = new[] { x });

				AssertWHTAmount(loader.GetNotionalWHT(amnts.TransactionPK), loader.GetRealizedWHT(amnts.TransactionPK), 20M, 45M);
				AssertContainsExactElementsInAnyOrder(new[] { amnts.TransactionPK }, passedTransactionPKs);
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateNotionalWHT(It.IsAny<ZGuid>()), Times.Exactly(1));
				apJournalBasedWHTAmountCalculatorMock.Verify(x => x.CalculateRealizedWHT(It.IsAny<ZGuid>()), Times.Exactly(1));
			}
		}

		public void TestWHTAmountLoading_RegisterForLoadingWHTAmounts()
		{
			var factory = new BusinessObjectFactory();

			var amnts = new (ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt)[]
			{
				(Guid.NewGuid(), 20M, 45M),
				(Guid.NewGuid(), 25M, 55M),
				(Guid.NewGuid(), 30M, 65M)
			};

			ZGuid[] passedTransactionPKs = null;
			var set1 = MockWithheldTaxAmounts(amnts[0], amnts[1]);
			var set2 = MockWithheldTaxAmounts(amnts[2]);
			var whtAmountCalculatorMock = new Mock<IWHTAmountCalculator>();

			using (ObjectFactory.Substitute(whtAmountCalculatorMock.Object))
			{
				var loader = new WHTAmountLoader(factory) { IsWHTRealizationInProgress = false };
				loader.RegisterForLoadingWHTAmounts(amnts[0].TransactionPK, amnts[1].TransactionPK);

				SetupMockWHTAmountCalculator(whtAmountCalculatorMock, set1, (x) => passedTransactionPKs = x);
				AssertWHTAmount(loader.GetNotionalWHT(amnts[0].TransactionPK), loader.GetRealizedWHT(amnts[0].TransactionPK), 20M, 45M);
				AssertWHTAmount(loader.GetNotionalWHT(amnts[1].TransactionPK), loader.GetRealizedWHT(amnts[1].TransactionPK), 25M, 55M);
				AssertContainsExactElementsInAnyOrder(new[] { amnts[0].TransactionPK, amnts[1].TransactionPK }, passedTransactionPKs);
				whtAmountCalculatorMock.Verify(x => x.Calculate(It.IsAny<ZGuid[]>()), Times.Once);

				SetupMockWHTAmountCalculator(whtAmountCalculatorMock, set2, (x) => passedTransactionPKs = x);
				AssertWHTAmount(loader.GetNotionalWHT(amnts[2].TransactionPK), loader.GetRealizedWHT(amnts[2].TransactionPK), 30M, 65M);
				AssertContainsExactElementsInAnyOrder(new[] { amnts[2].TransactionPK }, passedTransactionPKs);
				whtAmountCalculatorMock.Verify(x => x.Calculate(It.IsAny<ZGuid[]>()), Times.Exactly(2));
			}
		}

		IEnumerable<IWithheldTaxAmounts> MockWithheldTaxAmounts(params (ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt)[] whtAmounts)
		{
			var mockedAmounts = new List<IWithheldTaxAmounts>();
			foreach (var whtAmnt in whtAmounts)
			{
				var withheldAmountsMock = new Mock<IWithheldTaxAmounts>();
				withheldAmountsMock.SetupGet(x => x.TransactionPK).Returns(whtAmnt.TransactionPK);
				withheldAmountsMock.SetupGet(x => x.NotionalAmount).Returns(whtAmnt.NotionalAmnt);
				withheldAmountsMock.SetupGet(x => x.RealizedAmount).Returns(whtAmnt.RealizedAmnt);
				mockedAmounts.Add(withheldAmountsMock.Object);
			}
			return mockedAmounts;
		}

		void AssertWHTAmount(ZDecimal notionalAmount, ZDecimal realizedAmountt, ZDecimal expectedNotionalAmount, ZDecimal expectedRealizedAmount)
		{
			AssertEquals("Notional Amount", expectedNotionalAmount, notionalAmount);
			AssertEquals("Realized Amount", expectedRealizedAmount, realizedAmountt);
		}

		void SetupMockWHTAmountCalculator(Mock<IWHTAmountCalculator> whtAmountCalculatorMock, IEnumerable<IWithheldTaxAmounts> amounts, Action<ZGuid[]> callBackAction)
		{
			whtAmountCalculatorMock
				.Setup(x => x.Calculate(It.IsAny<ZGuid[]>()))
				.Returns(amounts)
				.Callback(callBackAction);
		}

		void SetupMockAPJournalBasedWHTAmountCalculator(Mock<IWithholdingJournalCreationManager> whtAmountCalculatorMock, IWithheldTaxAmounts amounts, Action<ZGuid> callBackAction)
		{
			whtAmountCalculatorMock
				.Setup(x => x.CalculateRealizedWHT(It.IsAny<ZGuid>()))
				.Returns(amounts != null ? amounts.RealizedAmount : null)
				.Callback(callBackAction);

			whtAmountCalculatorMock
				.Setup(x => x.CalculateNotionalWHT(It.IsAny<ZGuid>()))
				.Returns(amounts != null ? amounts.NotionalAmount : null)
				.Callback(callBackAction);
		}
	}
}
