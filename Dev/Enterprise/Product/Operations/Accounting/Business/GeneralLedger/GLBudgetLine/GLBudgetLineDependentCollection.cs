using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public class GLBudgetLineDependentCollection : DependentBusinessObjectCollection<GLBudgetLine, GLBudget>
	{
		static readonly int MaxLengthOfPeriodIndex = 2;

		public GLBudgetLineDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GLBudgetLineDependentCollection(GLBudget parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			CalculationSuspend = false;
		}

		#region New Property

		ZDecimal fLineSum;
		public ZDecimal LineSum
		{
			get
			{
				if (!CalculationSuspend)
				{
					fLineSum = GetTheLineSum();
				}
				return fLineSum;
			}
		}

		ZDecimal fPercentageSum;
		public ZDecimal PercentageSum
		{
			get
			{
				if (!CalculationSuspend)
				{
					fPercentageSum = GetThePercentageSum();
				}
				return fPercentageSum;
			}
		}

		bool CalculationSuspend;

		public void SuspendTotalCalculation()
		{
			CalculationSuspend = true;
		}

		public void ResumeTotalCalculation()
		{
			CalculationSuspend = false;
		}

		#endregion

		#region Property Overrides

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			GLBudgetLine newLine = (GLBudgetLine)child;
			newLine.AD_Period = new ZInt(Convert.ToInt32(Master.AU_Year.ToString() + (Count + 1).ToString().PadLeft(MaxLengthOfPeriodIndex, '0')));
		}

		#endregion

		#region Allocation Methods

		public void SetDebitCredit(ZString debitCredit)
		{
			foreach (GLBudgetLine line in this)
			{
				line.DebitCreditSign = debitCredit;
			}
		}

		public void RecalculateAmount()
		{
			if (this.Count > 0)
			{
				SuspendTotalCalculation();
				if (Master.AU_AllocationType == AllocationTypeList.Codes.individual)
				{
					SetPercentageToZero();
					CalculateIndividualAllocation();
				}
				else if (Master.AU_AllocationType == AllocationTypeList.Codes.byPeriod)
				{
					SetPercentageToZero();
					CalculatePeriodAllocation();
				}
				else if (Master.AU_AllocationType == AllocationTypeList.Codes.none)
				{
					SetPercentageToZero();
					SetValuesToZero();
				}
				else if (Master.AU_AllocationType == AllocationTypeList.Codes.percentage)
				{
					SetValuesToZero();
					SetPercentage();
				}
				ResumeTotalCalculation();
				Master.TotalAmountInfo.RefreshBinding();
			}
		}

		public void ResetLineValues()
		{
			foreach (GLBudgetLine line in this)
			{
				line.ResetLastYearValues();
			}
		}

		void SetPercentage()
		{
			foreach (GLBudgetLine line in this)
			{
				line.SetPercentageAmount(line.AD_Percent);
			}
			SetDebitCreditBasedOnParentAndAmountSign();
		}

		public void SetDebitCreditBasedOnParentAndAmountSign()
		{
			if (Master != null && Master.GLHeader != null)
			{
				if (Master.GLHeader.AG_DebitCredit == DebitCreditTypeList.Codes.debit)
				{
					SetDebitCredit(Master.AU_AllocationValue >= 0 ? DebitCreditTypeList.Codes.debit : DebitCreditTypeList.Codes.credit);
				}
				else
				{
					SetDebitCredit(Master.AU_AllocationValue >= 0 ? DebitCreditTypeList.Codes.credit : DebitCreditTypeList.Codes.debit);
				}
			}
		}

		void SetPercentageToZero()
		{
			foreach (GLBudgetLine line in this)
			{
				line.AD_Percent = 0m;
			}
		}

		void SetValuesToZero()
		{
			foreach (GLBudgetLine line in this)
			{
				line.AD_Amount = 0m;
			}
		}

		void CalculateIndividualAllocation()
		{
			this[0].UnsignedAmount = Math.Abs(Master.AU_AllocationValue);
			for (int i = 1; i < this.Count; ++i)
			{
				this[i].UnsignedAmount = this[i - 1].UnsignedAmount * ((100 + Master.AU_AllocationIncrement) / 100);
			}

			SwapLineDebitCreditSigns();
		}

		void SwapLineDebitCreditSigns()
		{
			if (Master.AU_AllocationValue < 0)
			{
				foreach (GLBudgetLine line in this)
				{
					line.SwapDebitCredit();
				}
			}
		}

		void CalculatePeriodAllocation()
		{
			double initialValue = (double)Math.Abs(Master.AU_AllocationValue);

			double allocationIncrement = (double)(Master.AU_AllocationIncrement / 100) + 1;
			double allocationValue = initialValue / GetGeometricProgressionDivisor(this.Count, allocationIncrement);

			for (int i = 0; i < this.Count; ++i)
			{
				double valueToSet = allocationValue * Math.Pow(allocationIncrement, i);
				this[i].UnsignedAmount = ZArchitecture.Core.Utilities.Round((decimal)valueToSet, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
			SwapLineDebitCreditSigns();
			PadTheLastPeriod();
		}

		void PadTheLastPeriod()
		{
			ZDecimal lineSum = GetTheLineSumByParentSource();
			if (lineSum != Master.AU_AllocationValue)
			{
				if (Master != null && Master.GLHeader != null && Master.GLHeader.AG_DebitCredit == DebitCreditTypeList.Codes.credit)
				{
					this[Count - 1].AD_Amount -= (Master.AU_AllocationValue - lineSum);
				}
				else
				{
					this[Count - 1].AD_Amount += (Master.AU_AllocationValue - lineSum);
				}
			}
		}

		#endregion

		#region Utility Methods

		//BusinessObjectCollection has RefreshBinding() and RefreshBindingIncludingChildren()
		public new void RefreshBinding()
		{
			foreach (GLBudgetLine line in Elements)
			{
				line.RefreshBinding();
			}
		}

		ZDecimal GetTheLineSumByParentSource()
		{
			ZDecimal result = GetTheLineSum();
			if (Master != null && Master.GLHeader != null && Master.GLHeader.AG_DebitCredit == DebitCreditTypeList.Codes.credit)
			{
				result = result * -1;
			}
			return result;
		}

		ZDecimal GetTheLineSum()
		{
			ZDecimal result = 0m;
			foreach (GLBudgetLine line in Elements)
			{
				result += ZArchitecture.Core.Utilities.Round(line.AD_Amount, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
			return result;
		}

		ZDecimal GetThePercentageSum()
		{
			ZDecimal result = 0m;
			foreach (GLBudgetLine line in Elements)
			{
				result += line.AD_Percent;
			}
			return result;
		}

		double GetGeometricProgressionDivisor(ZInt endPeriod, double percentage)
		{
			double result = 1;
			for (int period = 1; period < endPeriod; period++)
			{
				result = result + Math.Pow(percentage, period);
			}
			return result;
		}

		#endregion
	}
}

