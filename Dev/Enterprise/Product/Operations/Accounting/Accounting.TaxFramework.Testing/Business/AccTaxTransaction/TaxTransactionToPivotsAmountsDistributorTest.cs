using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxTransactionToPivotsAmountsDistributorTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			var taxRecord = Factory.New<AccTaxTransaction>();
			AssertType<TaxTransactionToPivotsAmountsDistributor>(taxRecord.TaxTransactionToPivotsAmountsDistributor_ExposedForTestOnly);
		}

		public void TestAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount()
		{
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(10m, 10m, System.Array.Empty<ZDecimal>(), System.Array.Empty<ZDecimal>());
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(30m, 30m, new ZDecimal[] { 10m, 20m }, new ZDecimal[] { 10m, 20m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(10m, 10m, new ZDecimal[] { -10m, 20m }, new ZDecimal[] { -10m, 20m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-10m, -10m, new ZDecimal[] { 10m, -20m }, new ZDecimal[] { 10m, -20m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-30m, -30m, new ZDecimal[] { -10m, -20m }, new ZDecimal[] { -10m, -20m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(10m, 0m, new ZDecimal[] { 0m, 0m }, new ZDecimal[] { 10m, 0m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-10m, 0m, new ZDecimal[] { 0m, 0m }, new ZDecimal[] { 0m, -10m });

			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(25m, 25m, new ZDecimal[] { 10m, 20m }, new ZDecimal[] { 10m, 15m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(30m, 30m, new ZDecimal[] { 10m, 20m, 30m }, new ZDecimal[] { 10m, 20m, 0m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-10m, -10m, new ZDecimal[] { 10m, 20m }, new ZDecimal[] { 0m, -10m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(15m, 15m, new ZDecimal[] { 10m, 20m }, new ZDecimal[] { 10m, 5m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(20m, 20m, new ZDecimal[] { -10m, 80m }, new ZDecimal[] { -10m, 30m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(10m, 10m, new ZDecimal[] { 10m, -5m, 20m }, new ZDecimal[] { 10m, -5m, 5m });

			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-25m, -25m, new ZDecimal[] { -10m, -20m }, new ZDecimal[] { -10m, -15m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-10m, -10m, new ZDecimal[] { -10m, -20m, -30m }, new ZDecimal[] { -10m, 0m, 0m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(10m, 10m, new ZDecimal[] { -10m, -20m }, new ZDecimal[] { 0m, 10m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-5m, -5m, new ZDecimal[] { -10m, -20m }, new ZDecimal[] { -5m, 0m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(60m, 60m, new ZDecimal[] { 10m, 30m }, new ZDecimal[] { 30m, 30m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(80m, 80m, new ZDecimal[] { 10m, -5m, 30m }, new ZDecimal[] { 50m, 0m, 30m });

			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(0.03m, 10m, new ZDecimal[] { 4m, 3m }, new ZDecimal[] { 0.01m, 0.02m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(0.03m, 10m, new ZDecimal[] { 3m, 4m }, new ZDecimal[] { 0.02m, 0.01m });

			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-0.02m, -0.77m, new ZDecimal[] { -0.22m, -0.22m, -0.22m, -0.22m, 0.01m }, new ZDecimal[] { 0m, 0m, -0.01m, -0.01m, 0m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(0.06m, -0.77m, new ZDecimal[] { 0.11m, -0.22m, -0.22m, -0.22m, -0.22m }, new ZDecimal[] { -0.01m, 0.02m, 0.02m, 0.02m, 0.01m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-0.15m, -1.32m, new ZDecimal[] { -0.22m, -0.22m, -0.22m, -0.22m, -0.22m, -0.22m, -0.22m, -0.22m, 0.11m, 0.11m, 0.11m, 0.11m }, new ZDecimal[] { 0m, -0.01m, -0.03m, -0.03m, -0.03m, -0.03m, -0.03m, -0.03m, 0.01m, 0.01m, 0.01m, 0.01m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(0.04m, -0.79m, new ZDecimal[] { 0.11m, 0.02m, 0.02m, -0.02m, -0.02m, -0.02m, -0.22m, -0.22m, -0.22m, -0.22m }, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 0.01m, 0.01m, 0.01m, 0.01m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(0.2m, -1.32m, new ZDecimal[] { 0.11m, 0.11m, 0.11m, 0.11m, -0.22m, -0.22m, -0.22m, -0.22m, -0.22m, -0.22m, -0.22m, -0.22m }, new ZDecimal[] { 0m, 0m, -0.02m, -0.02m, 0.03m, 0.03m, 0.03m, 0.03m, 0.03m, 0.03m, 0.03m, 0.03m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-0.03m, -0.75m, new ZDecimal[] { -0.11m, -0.11m, -0.11m, -0.11m, -0.11m, -0.11m, -0.11m, -0.02m, -0.02m, -0.02m, -0.02m, -0.02m, -0.02m, 0.02m, 0.02m, 0.02m, 0.02m, 0.02m, 0.04m }, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, -0.03m });
			AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(-0.09m, -1.32m, new ZDecimal[] { -0.01m, -0.01m, -0.02m, -0.02m, -0.03m, -0.03m, -0.04m, -0.04m, -0.05m, -0.05m, -0.06m, -0.06m, -0.07m, -0.07m, -0.08m, -0.08m, -0.09m, -0.09m, -0.1m, -0.1m, -0.11m, -0.11m }, new ZDecimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, -0.01m, -0.02m, -0.01m, -0.01m, -0.01m, -0.01m, -0.01m, -0.01m });
		}

		void AssertAdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(ZDecimal localTaxAmount, ZDecimal osTaxBaseAmount, ZDecimal[] baseOSAmounts, ZDecimal[] expectedLocalTaxAmounts)
		{
			var taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
			var taxConfig = taxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);

			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_ETC = taxConfig.PK;
			taxRecord.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxRecord.ATT_LocalTaxAmount = localTaxAmount;
			taxRecord.ATT_OSTaxBaseAmount = osTaxBaseAmount;

			List<AccTaxRecordTransactionLinePivot> pivots = null;
			var objForTest = new TaxTransactionToPivotsAmountsDistributor() as ITaxTransactionToPivotsAmountsDistributor;

			SetupPivotsInFactory();
			objForTest.AdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(taxRecord);

			AssertPivotsLocalTaxAmountsInFactory();

			void SetupPivotsInFactory()
			{
				pivots = new List<AccTaxRecordTransactionLinePivot>();
				foreach (var baseOSAmount in baseOSAmounts)
				{
					var pivot = Factory.New<AccTaxRecordTransactionLinePivot>();
					pivot.ATP_ATT = taxRecord.PK;
					var taxableTransactionLine = new Mock<ITaxableTransactionLine>();
					taxableTransactionLine.Setup(x => x.PK).Returns(ZGuid.NewZGuid());
					taxableTransactionLine.Setup(x => x.BaseOSAmount).Returns(baseOSAmount);
					pivot.LinkLine(taxableTransactionLine.Object);
					pivots.Add(pivot);
				}
			}

			void AssertPivotsLocalTaxAmountsInFactory()
			{
				AssertArrayEqualsByElements(expectedLocalTaxAmounts, pivots.Select(p => p.ATP_LocalTaxAmount).ToArray());
			}
		}
	}
}
