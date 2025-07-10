using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public sealed class TransactionImportAdditionalInfoProvider : IService
	{
		public TransactionImportAdditionalInfoProvider()
		{
			TransactionLineMatchingCriteriaCollectionMapper = new Dictionary<ZGuid, IEnumerable<MatchingCriteria>>();
			transactionLineApportionedChargeMapper = new Dictionary<ZGuid, ZGuid>();
			transactionLineConsolCostMapper = new Dictionary<ZGuid, ZGuid>();
			transactionLineApportionedChargeDisplaySequenceMapper = new Dictionary<ZGuid, ZShort>();
			TransactionLineMappingInfoCollection = new HashSet<(ZGuid LinePK, ZGuid TargetPK, JobChargeMappingInfoType InfoType)>();
		}

		#region MatchingCriteriaCollection

		public IEnumerable<MatchingCriteria> GetMatchingCriteriaCollection(ZGuid linePK)
		{
			return TransactionLineMatchingCriteriaCollectionMapper.TryGetValue(linePK, out var matchingCriteriaCollection) ? matchingCriteriaCollection : null;
		}

		public void MapTransactionLineWithMatchingCriteriaCollection(ZGuid linePK, IEnumerable<MatchingCriteria> matchingCriteriaCollection)
		{
			TransactionLineMatchingCriteriaCollectionMapper[linePK] = matchingCriteriaCollection;
		}

		#endregion

		#region JobCharge

		public ZGuid? GetApportionedChargePK(ZGuid linePK)
		{
			return transactionLineApportionedChargeMapper.TryGetValue(linePK, out var apportionedChargePK) ? apportionedChargePK : null;
		}

		public void MapTransactionLineWithApportionedCharge(ZGuid linePK, ZGuid apportionedChargePK)
		{
			transactionLineApportionedChargeMapper[linePK] = apportionedChargePK;
		}

		#endregion

		#region ConsolCost

		public ZGuid? GetConsolCostPK(ZGuid linePK)
		{
			return transactionLineConsolCostMapper.TryGetValue(linePK, out var consolCostPK) ? consolCostPK : null;
		}

		public void MapTransactionLineWithConsolCost(ZGuid linePK, ZGuid consolCostPK)
		{
			transactionLineConsolCostMapper[linePK] = consolCostPK;
		}

		#endregion

		#region DisplaySequence

		public ZShort? GetApportionedChargeDisplaySequence(ZGuid linePK)
		{
			return transactionLineApportionedChargeDisplaySequenceMapper.TryGetValue(linePK, out var displaySequence) ? displaySequence : null;
		}

		public void MapTransactionLineWithApportionedChargeDisplaySequence(ZGuid linePK, ZShort displaySequence)
		{
			transactionLineApportionedChargeDisplaySequenceMapper[linePK] = displaySequence;
		}

		#endregion

		public ISet<(ZGuid LinePK, ZGuid TargetPK, JobChargeMappingInfoType InfoType)> TransactionLineMappingInfoCollection { get; private set; }
		public IDictionary<ZGuid, IEnumerable<MatchingCriteria>> TransactionLineMatchingCriteriaCollectionMapper { get; private set; }

		readonly Dictionary<ZGuid, ZGuid> transactionLineApportionedChargeMapper;
		readonly Dictionary<ZGuid, ZGuid> transactionLineConsolCostMapper;
		readonly Dictionary<ZGuid, ZShort> transactionLineApportionedChargeDisplaySequenceMapper;
	}
}
