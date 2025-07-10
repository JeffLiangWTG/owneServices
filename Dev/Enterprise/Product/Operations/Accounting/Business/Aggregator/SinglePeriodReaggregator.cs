using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class SinglePeriodReaggregator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SinglePeriodReaggregator(BusinessObjectFactory factory)
			: base(factory)
		{
			PeriodCalculator = new AccountingPeriodCalculator(factory);
		}

		#region Company

		[ReadOnly(true)]
		[List("Companies")]
		[ResourceStringData("ReaggregateSinglePeriod|Company", Caption = "Company to Reaggregate", ShortCaption = "Company")]
		public ZGuid Company
		{
			get { return GlbCompany.CurrentCompany.PK; }
		}

		public ZPropertyInfo CompanyInfo
		{
			get { return GetZPropertyInfo(nameof(Company)); }
		}

		GlbCompanyCollection companies;
		public GlbCompanyCollection Companies
		{
			get
			{
				if (companies == null)
				{
					companies = new GlbCompanyCollection(Factory);
				}
				return companies;
			}
		}

		#endregion

		#region PeriodToReaggregate

		ZInt periodToReaggregate;
		[ResourceStringData("ReaggregateSinglePeriod|PeriodToReAggregate", Caption = "Period to Reaggregate", ShortCaption = "Period")]
		public ZInt PeriodToReaggregate
		{
			get
			{
				return periodToReaggregate;
			}
			set
			{
				periodToReaggregate = value;
				PeriodToReaggregateInfo.RefreshBinding();
				ValidatePeriodToReaggregate();
			}
		}

		public ZPropertyInfo PeriodToReaggregateInfo
		{
			get { return GetZPropertyInfo(nameof(PeriodToReaggregate)); }
		}

		public void ValidatePeriodToReaggregate()
		{
			if (!IsValidationSuspended)
			{
				PeriodToReaggregateInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(PeriodToReaggregateInfo);
				if (!PeriodCalculator.IsPeriodValid(PeriodToReaggregate))
				{
					PeriodToReaggregateInfo.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(PeriodToReaggregate));
				}
			}
		}

		readonly AccountingPeriodCalculator PeriodCalculator;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		virtual public void ReAggregate()
		{
			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				DbCommand cmd = Db.Connection.Command(SQL);
				cmd.CommandTimeout = 3600;
				cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, Company.ToGuid());
				cmd.AddParameter("@Period", SqlDbType.Int, (int)PeriodToReaggregate);
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();

				manager.CommitTransaction();
			}
		}

		#region SQL

		string SQL
		{
			get
			{
				return @"	delete from dbo.accglaggregate where 
							aa_period = @Period and 
							AA_GB IN (SELECT GB_PK FROM dbo.GlbBRanch WHERE GB_GC = @CompanyPK)

							DECLARE @AM_StartDate SMALLDATETIME
							DECLARE @AM_EndDate SMALLDATETIME

							SELECT TOP 1 @AM_StartDate = AM_StartDate, @AM_EndDate = AM_EndDate 
							FROM dbo.AccPeriodManagement WHERE AM_GC_Company = @CompanyPK AND AM_Period = @Period;

							INSERT INTO dbo.AccCashBasisVATQueue (YCC_YC)
							SELECT YC_PK FROM dbo.AccCashBasisVAT
							LEFT JOIN dbo.AccCashBasisVATQueue ON YCC_YC = YC_PK
							WHERE YC_GC = @CompanyPK AND YCC_YC IS NULL
							AND YC_PostDate BETWEEN @AM_StartDate AND @AM_EndDate;

							INSERT INTO dbo.AccTaxGLMovementQueue (ATQ_ATM)
							SELECT ATM_PK
							FROM dbo.AccTaxGLMovement
							INNER JOIN dbo.AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK
							LEFT JOIN dbo.AccTaxGLMovementQueue ON ATM_PK = ATQ_ATM
							WHERE ATQ_ATM IS NULL AND ATT_GC = @CompanyPK
							AND ATM_Period = @Period

							--GJL Journal Transactions
							insert into dbo.accglaggregate (aa_pk, aa_amount, aa_period, aa_ag, aa_gb, aa_gc, aa_ge, AA_TransactionCategory)
							select newid(), sum(al_lineamount), AM_Period, al_ag, al_gb, al_gc, al_ge, AH_TransactionCategory 
							FROM dbo.AccTransactionLines JOIN
							dbo.AccTransactionHeader on AL_AH = AH_PK JOIN
							dbo.AccPeriodManagement ON 
											AM_GC_Company = AH_GC AND 
											AM_Period = @Period
							WHERE AH_TransactionType = 'GJL' and 
							AH_GC = @CompanyPK AND 
							dbo.GetPeriodFromDate(AH_PostDate, AH_GC) = @Period
							GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory

							--NJL Journal Transactions
							insert into dbo.accglaggregate (aa_pk, aa_amount, aa_period, aa_ag, aa_gb, aa_gc, aa_ge, AA_TransactionCategory)
							select newid(), sum(al_lineamount), AM_Period, al_ag, al_gb, al_gc, al_ge, AH_TransactionCategory
							FROM dbo.AccTransactionLines JOIN
							dbo.AccTransactionHeader on AL_AH = AH_PK JOIN
							dbo.AccPeriodManagement ON 
											AM_GC_Company = AH_GC AND 
											AM_Period = @Period
							WHERE AH_TransactionType = 'NJL' and 
							AH_GC = @CompanyPK AND 
							dbo.GetPeriodFromDate(AH_PostDate, AH_GC) = @Period
							GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory

							--RJL Journal Transactions
							insert into dbo.accglaggregate (aa_pk, aa_amount, aa_period, aa_ag, aa_gb, aa_gc, aa_ge, AA_TransactionCategory)
							select newid(), sum(al_lineamount), AM_Period, al_ag, al_gb, al_gc, al_ge, AH_TransactionCategory
							FROM dbo.AccTransactionLines JOIN
							dbo.AccTransactionHeader on AL_AH = AH_PK JOIN
							dbo.AccPeriodManagement ON 
											AM_GC_Company = AH_GC AND 
											AM_Period = @Period
							WHERE AH_TransactionType = 'RJL' and 
							AH_GC = @CompanyPK AND 
							dbo.GetPeriodFromDate(AH_PostDate, AH_GC) = @Period
							GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory

							insert into dbo.accglaggregate (aa_pk, aa_amount, aa_period, aa_ag, aa_gb, aa_gc, aa_ge, AA_TransactionCategory)
							select newid(), -sum(al_lineamount), AM_Period, al_ag, al_gb, al_gc, al_ge, AH_TransactionCategory
							FROM dbo.AccTransactionLines JOIN
							dbo.AccTransactionHeader on AL_AH = AH_PK JOIN
							dbo.AccPeriodManagement ON 
											AM_GC_Company = AH_GC AND 
											AM_Period = @Period
							WHERE AH_TransactionType = 'RJL' and 
							AH_GC = @CompanyPK AND 
							dbo.GetPeriodFromDate(AH_DueDate, AH_GC) = @Period
							GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory

							--AJL Journal Transactions
							insert into dbo.accglaggregate (aa_pk, aa_amount, aa_period, aa_ag, aa_gb, aa_gc, aa_ge, AA_TransactionCategory)
							select newid(), sum(al_lineamount), AM_Period, al_ag, al_gb, al_gc, al_ge, AH_TransactionCategory
							FROM dbo.AccTransactionLines JOIN
							dbo.AccTransactionHeader on AL_AH = AH_PK JOIN
							dbo.AccPeriodManagement ON AM_GC_Company = AH_GC AND AM_Period = @Period
							WHERE AH_TransactionType = 'AJL' and 
							AH_GC = @CompanyPK AND 
								(dbo.GetPeriodFromDate(AH_PostDate, AH_GC) <= @Period AND 
									dbo.GetPeriodFromDate(AH_DueDate, AH_GC) >= @Period)
							GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory

							update dbo.acctransactionheader set ah_posttogl = 'N', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = @SystemLastEditUser
							FROM dbo.AccTransactionHeader join 
							dbo.accperiodmanagement on am_gc_company = AH_GC and (ah_postdate >= AM_StartDate AND AH_PostDate <= AM_EndDate)
							WHERE AH_GC = @CompanyPK AND AM_Period = @Period

							update dbo.acctransactionlines set al_posttogl = 'N', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = @SystemLastEditUser
							FROM dbo.AccTransactionLines JOIN 
							dbo.glbbranch on al_gb = gb_pk JOIN
							dbo.accperiodmanagement on am_gc_company = GB_GC and (al_postdate >= AM_StartDate AND AL_PostDate <= AM_EndDate)
							WHERE AM_Period = @Period AND GB_GC = @CompanyPK AND AL_AG IS NOT NULL

							update dbo.acctransactionlines set al_reversetogl = 'N', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = @SystemLastEditUser
							FROM dbo.AccTransactionLines JOIN 
							dbo.glbbranch on al_gb = gb_pk JOIN
							dbo.accperiodmanagement on am_gc_company = GB_GC and 
							(al_reversedate >= AM_StartDate AND al_reversedate <= AM_EndDate)
							WHERE AM_Period = @Period AND GB_GC = @CompanyPK AND AL_AG IS NOT NULL";
			}
		}

		#endregion
	}
}
