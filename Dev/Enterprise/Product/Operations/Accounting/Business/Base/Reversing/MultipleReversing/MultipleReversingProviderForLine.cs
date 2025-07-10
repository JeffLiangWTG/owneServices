using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class MultipleReversingProviderForLine : MultipleReversingProviderBase
	{
		public MultipleReversingProviderForLine() : base()
		{
		}

		public override BusinessObjectCollection BizObjectsForReversing
		{
			get { return transactionLinesForReversing ?? (transactionLinesForReversing = new TransactionLinesCollection(Factory)); }
		}
		TransactionLinesCollection transactionLinesForReversing;

		public TransactionLinesForReversingCollection TransactionLinesAlreadyReversed
		{
			get
			{
				if (transactionLinesAlreadyReversed == null)
				{
					transactionLinesAlreadyReversed = new TransactionLinesForReversingCollection(Factory);
					RegisterEditableChildObject(transactionLinesAlreadyReversed);
				}
				return transactionLinesAlreadyReversed;
			}
		}
		TransactionLinesForReversingCollection transactionLinesAlreadyReversed;

		public override BusinessObjectCollection BizObjectsAlreadyReversed => TransactionLinesAlreadyReversed;

		public override BusinessObject GetWrappedBusinessEntity(BusinessObject bizObjectAlreadyReversed) => ((TransactionLineForReversing)bizObjectAlreadyReversed).WrappedBusinessEntity;

		Dictionary<ZGuid, ZString> SelectedAccrualAndMissingAccrualDetailedMessage
		{
			get
			{
				if (selectedAccrualAndMissingAccrualDetailedMessage == null)
				{
					selectedAccrualAndMissingAccrualDetailedMessage = CheckMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccruals();
				}
				return selectedAccrualAndMissingAccrualDetailedMessage;
			}
		}

		Dictionary<ZGuid, ZString> selectedAccrualAndMissingAccrualDetailedMessage;

		public ZString GetMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccrualsErrorMessage(ZGuid selectedAccrualPK)
		{
			SelectedAccrualAndMissingAccrualDetailedMessage.TryGetValue(selectedAccrualPK, out ZString errorMessage);
			return errorMessage;
		}

		Dictionary<ZGuid, ZString> CheckMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccruals()
		{
			var baseWIPAccrualPKs = new HashSet<ZGuid>();
			foreach (BaseWIPAccrual baseWIPAccrual in BizObjectsForReversing)
			{
				baseWIPAccrualPKs.Add(baseWIPAccrual.PK);
			}

			var factoryForWIPAccrualReversal = new BusinessObjectFactory();

			var reloadedWIPAccruals = factoryForWIPAccrualReversal.Load<BaseWIPAccrual>(new ZQuery(AccTransactionLinesSchema.PK, baseWIPAccrualPKs));

			var reverserHelper = new WIPAccrualReverserHelper(reloadedWIPAccruals);

			return reverserHelper.PopulateErrorMessagesForMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccruals();
		}
	}
}
