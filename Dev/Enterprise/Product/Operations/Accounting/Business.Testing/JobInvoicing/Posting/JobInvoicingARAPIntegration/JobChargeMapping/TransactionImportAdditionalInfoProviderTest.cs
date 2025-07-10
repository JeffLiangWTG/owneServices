using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public sealed class TransactionImportAdditionalInfoProviderTest : TestCaseWithFactory
	{
		public void TestAddAndGetMatchingCriteriaCollection()
		{
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();

			var linePK1 = ZGuid.NewZGuid();
			AssertEquals(null, additionalInfoProvider.GetMatchingCriteriaCollection(linePK1));

			IEnumerable<MatchingCriteria> matchingCriteriaCollection1 = new[] { JobChargeMappingTestHelper.CreateMatchingCriteria("PrimaryKey") };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK1, matchingCriteriaCollection1);
			AssertEquals(matchingCriteriaCollection1, additionalInfoProvider.GetMatchingCriteriaCollection(linePK1));

			IEnumerable<MatchingCriteria> matchingCriteriaCollection2 = new[] { JobChargeMappingTestHelper.CreateMatchingCriteria("DisplaySequence") };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK1, matchingCriteriaCollection2);
			AssertEquals(matchingCriteriaCollection2, additionalInfoProvider.GetMatchingCriteriaCollection(linePK1));

			var linePK2 = ZGuid.NewZGuid();
			IEnumerable<MatchingCriteria> matchingCriteriaCollection3 = new[] { JobChargeMappingTestHelper.CreateMatchingCriteria("ChargeCode") };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK2, matchingCriteriaCollection3);
			AssertEquals(matchingCriteriaCollection3, additionalInfoProvider.GetMatchingCriteriaCollection(linePK2));
			AssertEquals(matchingCriteriaCollection2, additionalInfoProvider.GetMatchingCriteriaCollection(linePK1));
		}

		public void TestAddAndGetApportionedChargePK()
		{
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			var linePK1 = ZGuid.NewZGuid();
			AssertEquals(null, additionalInfoProvider.GetApportionedChargePK(linePK1));

			var chargePK1 = ZGuid.NewZGuid();
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(linePK1, chargePK1);
			AssertEquals(chargePK1, additionalInfoProvider.GetApportionedChargePK(linePK1));

			var chargePK2 = ZGuid.NewZGuid();
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(linePK1, chargePK2);
			AssertEquals(chargePK2, additionalInfoProvider.GetApportionedChargePK(linePK1));

			var linePK2 = ZGuid.NewZGuid();
			var chargePK3 = ZGuid.NewZGuid();
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(linePK2, chargePK3);
			AssertEquals(chargePK3, additionalInfoProvider.GetApportionedChargePK(linePK2));
			AssertEquals(chargePK2, additionalInfoProvider.GetApportionedChargePK(linePK1));
		}

		public void TestAddAndGetConsolCostPK()
		{
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			var linePK1 = ZGuid.NewZGuid();
			AssertEquals(null, additionalInfoProvider.GetConsolCostPK(linePK1));

			var chargePK1 = ZGuid.NewZGuid();
			additionalInfoProvider.MapTransactionLineWithConsolCost(linePK1, chargePK1);
			AssertEquals(chargePK1, additionalInfoProvider.GetConsolCostPK(linePK1));

			var chargePK2 = ZGuid.NewZGuid();
			additionalInfoProvider.MapTransactionLineWithConsolCost(linePK1, chargePK2);
			AssertEquals(chargePK2, additionalInfoProvider.GetConsolCostPK(linePK1));

			var linePK2 = ZGuid.NewZGuid();
			var chargePK3 = ZGuid.NewZGuid();
			additionalInfoProvider.MapTransactionLineWithConsolCost(linePK2, chargePK3);
			AssertEquals(chargePK3, additionalInfoProvider.GetConsolCostPK(linePK2));
			AssertEquals(chargePK2, additionalInfoProvider.GetConsolCostPK(linePK1));
		}

		public void TestAddAndGetDisplaySequence()
		{
			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			var linePK1 = ZGuid.NewZGuid();
			AssertEquals(null, additionalInfoProvider.GetApportionedChargeDisplaySequence(linePK1));

			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(linePK1, new ZShort(1));
			AssertEquals(new ZShort(1), additionalInfoProvider.GetApportionedChargeDisplaySequence(linePK1));
		}
	}
}
