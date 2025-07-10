using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.AccStatement
{
	public class StatementCollection : DependentBusinessObjectCollection<Statement, BusinessObject>
	{
		public StatementCollection(BankStatement bank, BusinessObjectFactory factory)
			: base(bank, factory)
		{
		}

		public StatementCollection(BankStatement bank, ZQuery filter)
			: base(bank, filter)
		{
		}

		#region Overrides

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			Statement removingStatement = (Statement)bizO;
			fRemovedStatementType = removingStatement.AS_DebitCredit;
			fRemovedStatementAmount = removingStatement.AS_Amount;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (fRemovedStatementAmount != ZDecimal.Zero)
			{
				((BankStatement)Master).CalculateTotalsAfterStatementDeleted(fRemovedStatementType, fRemovedStatementAmount);
				fRemovedStatementAmount = ZDecimal.Zero;
				fRemovedStatementType = ZString.Empty;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((Statement)child).AS_PageNumber = GetPreviousPageNumberForNewChild();
			((Statement)child).AS_StatementDate = ((BankStatement)Master).AB_LastStatementDate;
		}

		#endregion

		#region Implementation

		protected ZShort GetPreviousPageNumberForNewChild()
		{
			ZShort previoudPageNo = (ZShort)1;

			if (Count > 0)
			{
				previoudPageNo = this[Count - 1].AS_PageNumber;
			}

			return previoudPageNo;
		}

		ZDecimal fRemovedStatementAmount;
		ZString fRemovedStatementType;

		#endregion
	}
}
