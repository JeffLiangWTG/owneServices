using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public abstract partial class DependentTransactionLineCollection : DependentBusinessObjectCollection<DependentTransactionLine, BusinessObject>
	{
		public DependentTransactionLineCollection(BusinessObject parent, ZQuery query)
			: base(parent, query)
		{
		}

		public DependentTransactionLineCollection(BusinessObject parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public TransactionHeaderWithLines TransactionHeader
		{
			get { return (TransactionHeaderWithLines)Master; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			if (ParentTransactionHeader != null)
			{
				return ParentTransactionHeader.DependentTransactionLineType;
			}
			else
			{
				return base.GetTypeOfElementsFromPK(pK);
			}
		}

		public TransactionHeaderWithLines ParentTransactionHeader
		{
			get { return Master as TransactionHeaderWithLines; }
		}

		public void AddRange(DependentTransactionLine[] lines)
		{
			using (SuspendListChanged())
			{
				foreach (DependentTransactionLine transactionLine in lines)
				{
					base.Add(transactionLine);
				}
			}
			UpdateHeaderAmounts();
		}

		bool OnAddingNew;

		protected override BusinessObject AddNewCore()
		{
			BusinessObject newElement;

			try
			{
				OnAddingNew = true;

				newElement = base.AddNewCore();
			}
			finally
			{
				OnAddingNew = false;
			}

			return newElement;
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			BusinessObject newElement;

			try
			{
				OnAddingNew = true;

				newElement = base.AddNewCore(bizoType);
			}
			finally
			{
				OnAddingNew = false;
			}

			return newElement;
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			PAToAPTransactionLineMonitor.GetInstance(ParentTransactionHeader)?.RecordLineItemChange(e);

			bool isNonCommittedTransaction = IsNonCommittedCollectionElement(e.BizObject);
			base.OnCountChanged(e);
			if (!IsLoading && !OnAddingNew)
			{
				using (isNonCommittedTransaction ? ParentTransactionHeader.GetValidationSuspender() : null)
				{
					UpdateHeaderAmounts();
				}
			}
		}

		public Action<AccGLHeaderCollection, List<AccGLHeader>> ShowGLAccountsForImportAction;

		protected sealed override void SetDefaultsForNewChild(BusinessObject child)
		{
			bool suspendValidation = IsNonCommittedCollectionElement(child) || Master.IsValidationSuspended || IsValidationSuspended;
			using (suspendValidation ? child.GetValidationSuspender() : null)
			{
				var line = (DependentTransactionLine)child;
				line.AL_GB = TransactionHeader.AH_GB;
				line.AL_GE = TransactionHeader.AH_GE;
				line.AL_GB_TaxBranch = TransactionHeader.AH_GB_TaxBranch;
				SetDefaultsForNewChildCore(child);
				line.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
			}
		}

		protected virtual void SetDefaultsForNewChildCore(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var line = (DependentTransactionLine)child;
			if (line.AL_RX_NKTransactionCurrency != TransactionHeader.AH_RX_NKTransactionCurrency)
			{
				line.AL_RX_NKTransactionCurrency = TransactionHeader.AH_RX_NKTransactionCurrency;
			}
			SetDefaultExchangeRate(line);
			if (line.AL_LineType == TransactionLineTypes.Cost)
			{
				line.AL_IsFinalCharge = AccountingConfigurationRegistry.Instance.PayableFinalFlag.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			}

			if (((IBusinessObjectCollectionInternals)this).IsListChangedSuspended)
			{
				using (line.GetValidationSuspender())
				{
					line.AL_Sequence = defaultSequenceWhenListChangesSuspended;
				}
			}
			else
			{
				line.AL_Sequence = GetValidNextSequence();
			}

			line.AL_PlaceOfSupply = TransactionHeader.AH_PlaceOfSupply;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(AccTransactionLinesSchema.AL_AH, SQLComparisonOperator.NotEqual, null);
			return query;
		}

		#region GetValidNextSequence

		short GetValidNextSequence()
		{
			short result = 1;

			if (Count > 0)
			{
				unchecked
				{
					result = (short)((maxSequenceForAddRange ?? GetMaxPositiveLineSequence()) + 1);
				}

				if (result > 0 && maxSequenceForAddRange != null)
				{
					maxSequenceForAddRange = result;
				}

				if (result < 0)
				{
					result = 1;

					unchecked
					{
						foreach (var number in GetPositiveLineSequences().OrderBy(x => x))
						{
							if (result < number)
							{
								break;
							}

							if (result == number)
							{
								result++;
							}
						}
					}

					if (result < 0)
					{
						result = short.MaxValue;
					}

					if (maxSequenceForAddRange != null)
					{
						maxSequenceForAddRange = result == short.MaxValue ? valueToIndicateSequenceIsFull : short.MaxValue;
					}
				}
			}

			return result;
		}

		short GetMaxPositiveLineSequence()
		{
			var lineSequences = GetPositiveLineSequences().ToArray();

			return lineSequences.Any() ? (short)lineSequences.Max() : (short)0;
		}

		IEnumerable<ZShort> GetPositiveLineSequences()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				GetPositiveLineSequencesCalls_ForTestOnly++;
			}
#endif
			return this.Cast<DependentTransactionLine>().Where(x => x.AL_Sequence > 0).Select(x => x.AL_Sequence);
		}

#if DEBUG
		public int GetPositiveLineSequencesCalls_ForTestOnly;
#endif

		void RebuildSequenceAfterSuspendListChanged()
		{
			try
			{
				foreach (DependentTransactionLine lineWithoutSequence in this)
				{
					if (lineWithoutSequence.AL_Sequence == defaultSequenceWhenListChangesSuspended)
					{
						if (maxSequenceForAddRange == null)
						{
							maxSequenceForAddRange = GetMaxPositiveLineSequence();
						}

						lineWithoutSequence.AL_Sequence = maxSequenceForAddRange == valueToIndicateSequenceIsFull ? short.MaxValue : GetValidNextSequence();
					}
				}
			}
			finally
			{
				maxSequenceForAddRange = null;
			}
		}

		short? maxSequenceForAddRange;
		readonly short defaultSequenceWhenListChangesSuspended = short.MinValue;
		readonly short valueToIndicateSequenceIsFull = short.MinValue + 111;

		#endregion

		protected virtual void SetDefaultExchangeRate(DependentTransactionLine line)
		{
			line.AL_ExchangeRate = TransactionHeader.AH_ExchangeRate;
		}

		#region Update Amounts

		bool UpdateHeaderAmountsIsRunningNow;
		internal void UpdateHeaderAmounts()
		{
			if (TransactionHeader.IsDeleted || UpdateHeaderAmountsIsRunningNow)
			{
				return;
			}
			UpdateHeaderAmountsIsRunningNow = true;
			try
			{
				using (TransactionHeader.ValidateAH_OSTotalAmountSuspender.GetSuspender())
				{
					TransactionHeader.UpdateAH_OSExTaxAmount();
					TransactionHeader.UpdateAH_OSTaxAmount();
					TransactionHeader.UpdateAH_OSWHTAmount();
					TransactionHeader.UpdateAH_OSTotalAmount();
					TransactionHeader.UpdateAH_OSExtraTaxAmount();

					TransactionHeader.UpdateAH_LocalExTaxAmount();
					TransactionHeader.UpdateAH_LocalTaxAmount();
					TransactionHeader.UpdateAH_LocalWHTAmount();
					TransactionHeader.UpdateAH_LocalExtraTaxAmount();
				}
			}
			finally
			{
				UpdateHeaderAmountsIsRunningNow = false;
			}
		}

		#endregion

		#region SuspendListChanged

		protected override DisposableList GetAdditionalListChangedSuspenders()
		{
			var disposableList = new List<IDisposable>();
			if (TransactionHeader != null)
			{
				disposableList.Add(TransactionHeader.GetHeaderAmountsUpdateSuspender());
			}

			disposableList.Add(new DisposableAction(() => RebuildSequenceAfterSuspendListChanged()));

			return new DisposableList(disposableList);
		}

		#endregion
	}
}
