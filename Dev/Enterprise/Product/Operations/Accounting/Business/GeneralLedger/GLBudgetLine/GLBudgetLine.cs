using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public partial class GLBudgetLine : AutoAccGLBudgetLines
	{
		public GLBudgetLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			DebitCredit = new DebitCreditDataEntry(() => AD_Amount, x => AD_Amount = x);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ResetLastYearValues();
		}

		public void ResetLastYearValues()
		{
			fLastYearActual = 0;
			fLastYearBudget = 0;
			fLastYearActualWithoutDebitCredit = 0;
			fLastYearActualDebitCredit = "";
			fLastYearBudgetDebitCredit = "";
		}

		public int LocalDecimals => Parent?.Branch != null ? Parent.Branch.Company.GetLocalDecimals() : GlbCompany.CurrentCompany.GetLocalDecimals();

		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;
		#region New Properties

		#region LastYearActual

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LastYearActual
		{
			get
			{
				if (fLastYearActual.IsEmpty)
				{
					fLastYearActual = LastYearActualWithoutDebitCredit >= 0m ? LastYearActualWithoutDebitCredit : (ZDecimal)(-LastYearActualWithoutDebitCredit);
				}
				return fLastYearActual;
			}
		}
		ZDecimal fLastYearActual;

		public ZPropertyInfo LastYearActualInfo
		{
			get { return GetZPropertyInfo(nameof(LastYearActual)); }
		}

		ZDecimal fLastYearActualWithoutDebitCredit;
		ZDecimal LastYearActualWithoutDebitCredit
		{
			get
			{
				if (fLastYearActualWithoutDebitCredit.IsEmpty)
				{
					fLastYearActualWithoutDebitCredit = GetLastYearActual(AD_Period);
				}
				return fLastYearActualWithoutDebitCredit;
			}
		}

		#endregion

		#region LastYearActualDebitCredit

		ZString fLastYearActualDebitCredit;
		public ZString LastYearActualDebitCredit
		{
			get
			{
				if (fLastYearActualDebitCredit.IsEmpty)
				{
					if (LastYearActualWithoutDebitCredit > 0)
					{
						fLastYearActualDebitCredit = DebitCreditTypeList.Codes.debit;
					}
					else if (LastYearActualWithoutDebitCredit == 0)
					{
						if (Parent != null && Parent.GLHeader != null)
						{
							fLastYearActualDebitCredit = Parent.GLHeader.AG_DebitCredit;
						}
					}
					else
					{
						fLastYearActualDebitCredit = DebitCreditTypeList.Codes.credit;
					}
				}
				return fLastYearActualDebitCredit;
			}
		}

		public ZPropertyInfo LastYearActualDebitCreditInfo
		{
			get { return GetZPropertyInfo(nameof(LastYearActualDebitCredit)); }
		}

		public int LastYearActualDebitCredit_MaxLength
		{
			get { return DebitCreditTypes.MaxCodeLength; }
		}

		#endregion

		#region LastYearBudget

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LastYearBudget
		{
			get
			{
				if (fLastYearBudget.IsEmpty)
				{
					if (LastYearBudgetLine != null)
					{
						fLastYearBudget = LastYearBudgetLine.UnsignedAmount;
					}
				}
				return fLastYearBudget;
			}
		}
		ZDecimal fLastYearBudget;

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LastYearBudget_SignedAmount
		{
			get
			{
				if (fLastYearBudget_SignedAmount.IsEmpty)
				{
					if (LastYearBudgetLine != null)
					{
						fLastYearBudget_SignedAmount = LastYearBudgetLine.AD_Amount;
					}
				}
				return fLastYearBudget_SignedAmount;
			}
		}
		ZDecimal fLastYearBudget_SignedAmount;

		public ZPropertyInfo LastYearBudgetInfo
		{
			get { return GetZPropertyInfo(nameof(LastYearBudget)); }
		}

		#endregion

		#region LastYearBudgetDebitCredit

		ZString fLastYearBudgetDebitCredit;
		public ZString LastYearBudgetDebitCredit
		{
			get
			{
				if (fLastYearBudgetDebitCredit.IsEmpty)
				{
					if (LastYearBudget_SignedAmount > 0)
					{
						fLastYearBudgetDebitCredit = DebitCreditTypeList.Codes.debit;
					}
					else if (LastYearBudget_SignedAmount == 0)
					{
						if (Parent != null && Parent.GLHeader != null)
						{
							fLastYearBudgetDebitCredit = Parent.GLHeader.AG_DebitCredit;
						}
					}
					else
					{
						fLastYearBudgetDebitCredit = DebitCreditTypeList.Codes.credit;
					}
				}
				return fLastYearBudgetDebitCredit;
			}
		}

		public ZPropertyInfo LastYearBudgetDebitCreditInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(LastYearBudgetDebitCredit));
			}
		}

		public int LastYearBudgetDebitCredit_MaxLength
		{
			get { return DebitCreditTypes.MaxCodeLength; }
		}

		#endregion

		#region DebitCreditTypes

		DebitCreditTypeList fDebitCreditTypes;
		public DebitCreditTypeList DebitCreditTypes
		{
			get
			{
				if (fDebitCreditTypes == null)
				{
					fDebitCreditTypes = new DebitCreditTypeList();
				}
				return fDebitCreditTypes;
			}
		}

		#endregion

		#region UnsignedAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal UnsignedAmount
		{
			get { return DebitCredit.UnsignedAmount; }
			set
			{
				DebitCredit.UnsignedAmount = value;
				BudgetLineValidation.ValidateUnsignedLineAmount();
				UnsignedAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnsignedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(UnsignedAmount)); }
		}

		protected bool UnsignedAmount_ReadOnly
		{
			get { return Parent == null || Parent.AU_AllocationType == AllocationTypeList.Codes.percentage; }
		}

		#endregion

		#region DebitCreditSign

		[List("DebitCreditTypes")]
		public ZString DebitCreditSign
		{
			get { return DebitCredit.DebitCreditSign; }
			set
			{
				if (DebitCredit.DebitCreditSign != value)
				{
					CheckMaximumLength(DebitCreditSignInfo, value);
					DebitCredit.DebitCreditSign = value;
					BudgetLineValidation.ValidateDebitCreditSign();
				}
				DebitCreditSignInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DebitCreditSignInfo
		{
			get { return GetZPropertyInfo(nameof(DebitCreditSign)); }
		}

		protected bool DebitCreditSign_ReadOnly
		{
			get { return Parent == null || Parent.AU_AllocationType == AllocationTypeList.Codes.percentage; }
		}

		public int DebitCreditSign_MaxLength
		{
			get { return DebitCreditTypes.MaxCodeLength; }
		}

		#endregion

		#endregion

		#region Property Override

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal AD_Amount
		{
			get { return base.AD_Amount; }
			set
			{
				base.AD_Amount = value;
				if (Parent != null)
				{
					if (value == 0m && Parent.GLHeader != null && Parent.AU_AllocationType != AllocationTypeList.Codes.percentage)
					{
						DebitCredit.SetDebitCreditWithoutRecalculation(Parent.GLHeader.AG_DebitCredit);
					}
					Parent.TotalAmountInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal AD_Percent
		{
			get { return base.AD_Percent; }
			set
			{
				base.AD_Percent = value;
				value = SetPercentageAmount(value);
			}
		}

		protected bool AD_Percent_ReadOnly
		{
			get { return Parent == null || Parent.AU_AllocationType != AllocationTypeList.Codes.percentage; }
		}

		public ZDecimal SetPercentageAmount(ZDecimal value)
		{
			if (Parent != null)
			{
				UnsignedAmount = Parent.AU_AllocationValue * (value / 100m);
			}
			return value;
		}

		#endregion

		#region Implementation

		public void SwapDebitCredit()
		{
			if (DebitCreditSign == DebitCreditTypeList.Codes.credit)
			{
				DebitCreditSign = DebitCreditTypeList.Codes.debit;
			}
			else if (DebitCreditSign == DebitCreditTypeList.Codes.debit)
			{
				DebitCreditSign = DebitCreditTypeList.Codes.credit;
			}
		}

		ZShort GetYearFromPeriod(ZInt period)
		{
			return (ZShort)(period / 100);
		}

		ZInt GetLastYearPeriod(ZInt period)
		{
			ZShort currentYear = GetYearFromPeriod(period);
			// Currently this method is purely mathematical. We do not care about actual Period Setup.
			ZShort lastYear = currentYear - 1;
			ZInt periodPotion = period - (currentYear * 100);

			return ((lastYear * 100) + periodPotion);
		}

		ZDecimal GetLastYearActual(ZInt period)
		{
			ZDecimal result = 0m;
			ZInt lastYearPeriod = GetLastYearPeriod(period);
			if (Parent != null)
			{
				foreach (DynamicBusinessObject lastYearActual in Parent.LastYearActualAmountCollection)
				{
					if ((ZInt)lastYearActual["Period"] == lastYearPeriod)
					{
						result = (ZDecimal)lastYearActual[nameof(LastYearActual)];
						break;
					}
				}
			}
			return result;
		}

		GLBudgetLine GetLastYearBudget(ZInt period)
		{
			if (Parent != null)
			{
				GLBudgetLine[] lines = (GLBudgetLine[])Parent.PreviousCollection.Find(new ZQuery(AccGLBudgetLinesSchema.AD_Period, GetLastYearPeriod(AD_Period)));
				if (lines.Length > 0)
				{
					return lines[0];
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		protected readonly DebitCreditDataEntry DebitCredit;

		#region Other Lazy-getted Properites

		GLBudgetLine fLastYearBudgetLine;
		GLBudgetLine LastYearBudgetLine
		{
			get
			{
				if (fLastYearBudgetLine == null)
				{
					fLastYearBudgetLine = GetLastYearBudget(AD_Period);
				}
				return fLastYearBudgetLine;
			}
		}

		GLBudget fParent;
		public GLBudget Parent
		{
			get
			{
				if (fParent == null && AD_AU.IsValid)
				{
					fParent = Factory.Load<GLBudget>(AD_AU);
				}
				return fParent;
			}
		}

		#endregion

		#endregion

		#region Various Providers

		GLBudgetLinesValidation BudgetLineValidation
		{
			get { return this.Validation as GLBudgetLinesValidation; }
		}

		#endregion

		#region Base Method Override

		protected override AccGLBudgetLinesValidation GetNewValidation()
		{
			return new GLBudgetLinesValidation(this);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			DebitCredit.OnLoaded();
		}

		#endregion
	}
}

