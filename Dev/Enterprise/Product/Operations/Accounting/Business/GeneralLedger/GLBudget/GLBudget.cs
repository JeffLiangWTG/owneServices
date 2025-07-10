using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public class GLBudget : AutoAccGLBudget, IAuditParent
	{
		public GLBudget(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Base Methods Override

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			fPeriodCount = 0;
			AU_Year = new ZInt(ZDateTime.Now.Year);
			AU_AllocationType = AllocationTypeList.Codes.none;
			AU_GE = GlbDepartment.CurrentDepartment.PK;
			AU_GB = GlbBranch.CurrentBranch.PK;
		}

		public override void OnLoaded()
		{
			BudgetLines.SuspendTotalCalculation();
			base.OnLoaded();
			BudgetLines.ResumeTotalCalculation();
			TotalAmountInfo.RefreshBinding();
		}

		public override void Delete()
		{
			base.Delete();
			BudgetLines.RemoveAndDeleteAll();
		}

		protected override AccGLBudgetValidation GetNewValidation()
		{
			return new GLBudgetValidation(this);
		}

		#endregion

		#region Related Business Objects

		#region Budget Lines

		[ChildEditable(true)]
		public GLBudgetLineDependentCollection BudgetLines
		{
			get
			{
				if (fBudgetLines == null)
				{
					fBudgetLines = new GLBudgetLineDependentCollection(this, Factory);
					fBudgetLines.Load();
					RegisterEditableChildObject(fBudgetLines);
				}

				return fBudgetLines;
			}
		}
		GLBudgetLineDependentCollection fBudgetLines;

		#endregion

		#endregion

		#region Property Overrides

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal AU_AllocationIncrement
		{
			get { return base.AU_AllocationIncrement; }
			set
			{
				base.AU_AllocationIncrement = value;
				BudgetLines.RecalculateAmount();
			}
		}

		[List("AllocationTypes")]
		public override ZString AU_AllocationType
		{
			get { return base.AU_AllocationType; }
			set
			{
				base.AU_AllocationType = value;

				if (value == AllocationTypeList.Codes.none)
				{
					AU_AllocationValue = 0;
					AU_AllocationIncrement = 0;
				}
				else if (value == AllocationTypeList.Codes.percentage)
				{
					AU_AllocationIncrement = 0;
				}

				BudgetLines.RecalculateAmount();
				BudgetLines.RefreshBinding();
			}
		}

		[DecimalPlaces(nameof(Decimals))]
		public override ZDecimal AU_AllocationValue
		{
			get { return base.AU_AllocationValue; }
			set
			{
				base.AU_AllocationValue = value;
				BudgetLines.RecalculateAmount();
			}
		}

		[DecimalPlaces(nameof(Decimals))]
		public override ZDecimal AU_Opening
		{
			get { return base.AU_Opening; }
			set { base.AU_Opening = value; }
		}

		[DecimalPlaces(nameof(Decimals))]
		public override ZDecimal AU_Closing
		{
			get => base.AU_Closing;
			set => base.AU_Closing = value;
		}

		public int Decimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		#region AU_AG

		[List("Lookups.GLHeaders")]
		public override ZGuid AU_AG
		{
			get { return base.AU_AG; }
			set
			{
				base.AU_AG = value;
				fPreviousYearCollection = null;
				fLastYearActualAmountCollection = null;
				BudgetLines.ResetLineValues();
				if (GLHeader != null)
				{
					BudgetLines.SetDebitCreditBasedOnParentAndAmountSign();
				}
				BudgetLines.RefreshBinding();
			}
		}

		#endregion

		#region AU_GB

		[List("Lookups.Branches")]
		public override ZGuid AU_GB
		{
			get { return base.AU_GB; }
			set
			{
				base.AU_GB = value;
				fPreviousYearCollection = null;
				fLastYearActualAmountCollection = null;
				BudgetLines.ResetLineValues();
				BudgetLines.RefreshBinding();

				if (!IsValidationSuspended && GlbBranchCombinationValidation.ShouldValidateCombination(Branch))
				{
					Validation.ValidateAU_GE();
				}
			}
		}

		#endregion

		#region AU_GE

		[List("Lookups.Departments")]
		public override ZGuid AU_GE
		{
			get { return base.AU_GE; }
			set
			{
				base.AU_GE = value;
				fPreviousYearCollection = null;
				fLastYearActualAmountCollection = null;
				BudgetLines.ResetLineValues();
				BudgetLines.RefreshBinding();
			}
		}

		#endregion

		public override ZInt AU_Year
		{
			get { return base.AU_Year; }
			set
			{
				base.AU_Year = value;
				fPeriodCount = 0;
				fPreviousYearCollection = null;
				fLastYearActualAmountCollection = null;
				BudgetLines.RemoveAndDeleteAll();
				for (int i = 0; i < PeriodCount; ++i)
				{
					BudgetLines.AddNew(typeof(GLBudgetLine));
				}
			}
		}

		#region Property Info Override

		protected bool AU_Year_ReadOnly
		{
			get { return IsInDatabase; }
		}

		protected bool AU_AG_ReadOnly
		{
			get { return IsInDatabase; }
		}

		protected bool AU_GB_ReadOnly
		{
			get { return IsInDatabase; }
		}

		protected bool AU_GE_ReadOnly
		{
			get { return IsInDatabase; }
		}

		protected bool AU_Opening_ReadOnly
		{
			get
			{
				return IsInDatabase || GLHeader == null ||
					GLHeader != null && GLHeader.AG_AccountType != AccountTypesList.Codes.BalanceSheet;
			}
		}

		protected bool AU_AllocationIncrement_ReadOnly
		{
			get { return AU_AllocationType == AllocationTypeList.Codes.percentage || AU_AllocationType == AllocationTypeList.Codes.none; }
		}

		protected bool AU_AllocationValue_ReadOnly
		{
			get { return AU_AllocationType == AllocationTypeList.Codes.none; }
		}

		#endregion

		#endregion
		#region New Properties

		#region Period Count
		int fPeriodCount;
		/// <summary>
		/// Gets the number of periods in the year of this budget.
		/// </summary>
		public int PeriodCount
		{
			get
			{
				if (fPeriodCount == 0)
				{
					ZQuery periodFilter = new ZQuery(AccPeriodManagementSchema.AM_Year, (short)AU_Year);
					periodFilter.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
					AccPeriodManagement[] periodsForThisYear = Factory.Load<AccPeriodManagement>(periodFilter);
					fPeriodCount = periodsForThisYear.Length;
				}
				return fPeriodCount;
			}
		}

		#endregion

		#region AccountDescription

		public ZString AccountDescription
		{
			get { return GLHeader != null ? GLHeader.AG_DescriptionMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo AccountDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(AccountDescription)); }
		}

		#endregion

		#region AllocationTypes

		AllocationTypeList fAllocationTypes;
		public AllocationTypeList AllocationTypes
		{
			get
			{
				if (fAllocationTypes == null)
				{
					fAllocationTypes = new AllocationTypeList();
				}
				return fAllocationTypes;
			}
		}

		#endregion

		#region CreationDate

		public ZDateTime CreationDate
		{
			get { return base.AU_SystemCreateTimeUtc; }
		}

		public ZPropertyInfo CreationDateInfo
		{
			get { return GetZPropertyInfo(nameof(CreationDate)); }
		}

		#endregion

		#region CreatorName

		public ZString CreatorName
		{
			get { return base.AU_SystemCreateUser; }
		}

		public ZPropertyInfo CreatorNameInfo
		{
			get { return GetZPropertyInfo(nameof(CreatorName)); }
		}

		#endregion

		#region OpeningBalance

		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal OpeningBalance
		{
			get { return fOpeningBalance; }
			set { SetNonPersistentPropertyValue(OpeningBalanceInfo, ref fOpeningBalance, value); }
		}
		ZDecimal fOpeningBalance;

		public ZPropertyInfo OpeningBalanceInfo
		{
			get { return GetZPropertyInfo(nameof(OpeningBalance)); }
		}

		#endregion

		#region Total Amount

		[ReadOnly(true)]
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal TotalAmount
		{
			get { return BudgetLines.LineSum; }
		}

		public ZPropertyInfo TotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAmount)); }
		}

		#endregion

		#region Total Percentage

		[ReadOnly(true)]
		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal TotalPercentage
		{
			get
			{
				((GLBudgetValidation)Validation).ValidateTotalPercentage();
				return BudgetLines.PercentageSum;
			}
		}

		public ZPropertyInfo TotalPercentageInfo
		{
			get { return GetZPropertyInfo(nameof(TotalPercentage)); }
		}

		#endregion

		#endregion

		#region Optiization Collections

		DynamicBusinessObjectCollection fLastYearActualAmountCollection;
		public DynamicBusinessObjectCollection LastYearActualAmountCollection
		{
			get
			{
				if (fLastYearActualAmountCollection == null)
				{
					string sQL = @"SELECT AA_Period as Period, sum(AA_Amount) as LastYearActual
							FROM dbo.AccGLAggregate
							WHERE AA_Period >= @LastYearPeriodStart
							AND AA_Period <= @LastYearPeriodEnd
							AND AA_GB = @Branch
							AND AA_GE = @Department
							AND AA_AG = @GLAccount
							GROUP BY AA_Period";

					ZInt lastYearPeriodStart = PeriodCalculator.GetFirstPeriodForYear(AU_Year - 1);
					ZInt lastYearPeriodEnd = PeriodCalculator.GetLastPeriodForYear(AU_Year - 1);

					ZSqlParameterCollection parameters = new ZSqlParameterCollection();
					parameters.Add("@LastYearPeriodStart", lastYearPeriodStart, AccGLAggregateSchema.AA_Period);
					parameters.Add("@LastYearPeriodEnd", lastYearPeriodEnd, AccGLAggregateSchema.AA_Period);
					parameters.Add("@Branch", AU_GB, AccGLAggregateSchema.AA_GB);
					parameters.Add("@Department", AU_GE, AccGLAggregateSchema.AA_GE);
					parameters.Add("@GLAccount", AU_AG, AccGLAggregateSchema.AA_AG);
					fLastYearActualAmountCollection = new DynamicBusinessObjectCollection(Factory);
					fLastYearActualAmountCollection.Load(sQL, parameters);
				}
				return fLastYearActualAmountCollection;
			}
		}

		GLBudgetLineDependentCollection fPreviousYearCollection;
		public GLBudgetLineDependentCollection PreviousCollection
		{
			get
			{
				if (fPreviousYearCollection == null)
				{
					fPreviousYearCollection = GetLastYearBudgetCollection();
					fPreviousYearCollection.Load();
				}
				return fPreviousYearCollection;
			}
		}
		
		GLBudgetLineDependentCollection GetLastYearBudgetCollection()
		{
			GLBudgetLineDependentCollection collectionToReturn = new GLBudgetLineDependentCollection(Factory);
			ZInt lastYear = AU_Year - 1;

			ZQuery query = new ZQuery();
			query.AddToFilter(AccGLBudgetSchema.AU_AG, AU_AG);
			query.AddToFilter(AccGLBudgetSchema.AU_GB, AU_GB);
			query.AddToFilter(AccGLBudgetSchema.AU_GE, AU_GE);
			query.AddToFilter(AccGLBudgetSchema.AU_Year, lastYear);

			GLBudget budget = Factory.LoadTop1<GLBudget>(query);

			if (budget != null)
			{
				collectionToReturn = budget.BudgetLines;
			}
			return collectionToReturn;
		}

		AccountingPeriodCalculator fPeriodCalculator;
		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		#endregion

		#region IAuditParent Members

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(AccGLBudgetLinesSchema.AD_AU, null);
			}
		}

		#endregion
	}
}

