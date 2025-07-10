using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Posting.JobInvoicingARAPIntegration.JobChargeMapping;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class TransactionLineMappingStrategyFactoryTest : TestCaseWithFactory
	{
		public void TestCreateStrategies_InvalidParameters()
		{
			var strategyFactory = new TransactionLineMappingStrategyFactory();

			var linePK = ZGuid.NewZGuid();
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			var universalLine = new PostingJournal();
			var primaryKeyMatchingCriteria = JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey");

			var mapTransactionStrategies = strategyFactory.CreateStrategies(linePK, additionalInfoProvider);
			AssertEquals("Should have no available strategies.", 0, mapTransactionStrategies.Count());

			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK, Array.Empty<MatchingCriteria>());
			mapTransactionStrategies = strategyFactory.CreateStrategies(linePK, additionalInfoProvider);
			AssertEquals("Should have no available strategies.", 0, mapTransactionStrategies.Count());

			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK, new[] { primaryKeyMatchingCriteria });
			mapTransactionStrategies = strategyFactory.CreateStrategies(linePK, null);
			AssertEquals("Should have no available strategies.", 0, mapTransactionStrategies.Count());
		}
		public void TestCreateStrategies()
		{
			var strategyFactory = new TransactionLineMappingStrategyFactory();

			var linePK = ZGuid.NewZGuid();
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			var universalLine = new PostingJournal();
			var primaryKeyMatchingCriteria = JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey");
			var displaySequenceMatchingCriteria = JobChargeMappingTestHelper.CreateMatchingCriteria("DisplaySequence");

			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK, new[] { primaryKeyMatchingCriteria });
			var mapTransactionStrategies = strategyFactory.CreateStrategies(linePK, additionalInfoProvider);
			AssertEquals("Should have 1 available strategy.", 1, mapTransactionStrategies.Count());
			AssertType<PrimaryKeyMappingStrategy>("Should have PrimaryKeyMappingStrategy", mapTransactionStrategies.First());

			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK, new[] { displaySequenceMatchingCriteria });
			mapTransactionStrategies = strategyFactory.CreateStrategies(linePK, additionalInfoProvider);
			AssertEquals("Should have 1 available strategy.", 1, mapTransactionStrategies.Count());
			AssertType<DisplaySequenceMappingStrategy>("Should have DisplaySequenceMappingStrategy", mapTransactionStrategies.First());

			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK, new[] { primaryKeyMatchingCriteria, displaySequenceMatchingCriteria });
			mapTransactionStrategies = strategyFactory.CreateStrategies(linePK, additionalInfoProvider);
			AssertEquals("Should have 1 available strategy because PrimaryKey is the critical criteria.", 1, mapTransactionStrategies.Count());
			AssertType<PrimaryKeyMappingStrategy>("Should have PrimaryKeyMappingStrategy", mapTransactionStrategies.First());
		}
	}
}
