using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class AggregationDiscrepanciesCalculator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AggregationDiscrepanciesCalculator(BusinessObjectFactory factory, ZString checkMethod)
			: base(factory)
		{
			PeriodCalculator = new AccountingPeriodCalculator(factory);
			CheckMethod = checkMethod;
		}

		#region Company

		[List("Companies")]
		public ZGuid Company
		{
			get { return fCompany; }
			set
			{
				fCompany = value;
				CompanyInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					CompanyInfo.ClearAllNotifications();
					MandatoryValidation.CheckEntered(CompanyInfo);
					ListValidation.ErrorIfInvalidPK(CompanyInfo, Companies);
				}
			}
		}
		ZGuid fCompany = GlbCompany.CurrentCompany.PK;
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

		#region NumberOfPeriods

		ZInt fNumberOfPeriods;
		public ZInt NumberOfPeriods
		{
			get
			{
				return fNumberOfPeriods;
			}
			set
			{
				fNumberOfPeriods = value;
				NumberOfPeriodsInfo.RefreshBinding();
				ValidateNumberOfPeriods();
			}
		}

		public ZPropertyInfo NumberOfPeriodsInfo
		{
			get { return GetZPropertyInfo(nameof(NumberOfPeriods)); }
		}

		public void ValidateNumberOfPeriods()
		{
			if (!IsValidationSuspended)
			{
				NumberOfPeriodsInfo.ClearAllNotifications();
				if (NumberOfPeriods < 0 || NumberOfPeriods > 11)
				{
					NumberOfPeriodsInfo.AddError(Res.GetString("e491170b-004e-472a-be40-fd349f4c4a27", "Number of periods should be between 0 and 11"));
				}
			}
		}

		#endregion

		#region Lines

		public AggregationDiscrepanciesCalculatorLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new AggregationDiscrepanciesCalculatorLineCollection();
				}
				return fLines;
			}
		}

		AggregationDiscrepanciesCalculatorLineCollection fLines;

		#endregion

		#region CalculateAggregationDiscrepancies

		readonly ZString CheckMethod;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void CalculateAggregationDiscrepancies()
		{
			if (!HasErrors)
			{
				Lines.RemoveAndDeleteAll();
				DbCommand cmd = Db.Connection.Command(SQL);
				cmd.CommandTimeout = 3600;
				cmd.AddParameter("@GC_PK", SqlDbType.UniqueIdentifier, Company.ToGuid());
				cmd.AddParameter("@AM_PeriodTo", SqlDbType.Int, (int)PeriodToReaggregate);
				cmd.AddParameter("@NumberOfPeriods", SqlDbType.Int, (int)NumberOfPeriods);
				using (var reader = cmd.ExecuteReader()) // Custom call to DB necessary since aggregator must issue command to DB directly
				{
					while (reader.Read())
					{
						AggregationDiscrepanciesCalculatorLine line = Lines.AddNew();
						line.LineType = reader.IsDBNull(0) ? "" : reader.GetString(0);
						line.Period1 = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
						line.Period2 = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
						line.Period3 = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
						line.Period4 = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
						line.Period5 = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
						line.Period6 = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
						line.Period7 = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
						line.Period8 = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
						line.Period9 = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
						line.Period10 = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
						line.Period11 = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
						line.Period12 = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);
					}
				}
				CalculateDifferences();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void CalculateDifferences()
		{
			int aCRExpected = -1;
			int wIPExpected = -1;
			int aCRAggregated = -1;
			int wIPAggregated = -1;
			int aRExpected = -1;
			int aPExpected = -1;
			int aRAggregated = -1;
			int aPAggregated = -1;

			for (int i = 0; i < Lines.Count; i++)
			{
				switch (Lines[i].LineType)
				{
					case "ACR Expected":
						{
							aCRExpected = i;
						} break;
					case "WIP Expected":
						{
							wIPExpected = i;
						} break;
					case "ACR Aggregated":
						{
							aCRAggregated = i;
						} break;
					case "WIP Aggregated":
						{
							wIPAggregated = i;
						} break;
					case "AR Expected":
						{
							aRExpected = i;
						} break;
					case "AP Expected":
						{
							aPExpected = i;
						} break;
					case "AR Aggregated":
						{
							aRAggregated = i;
						} break;
					case "AP Aggregated":
						{
							aPAggregated = i;
						} break;
				}
			}
			if (aCRExpected != -1 && aCRAggregated != -1)
			{
				CalculateDifference(aCRAggregated, aCRExpected, "ACR");
			}
			if (wIPExpected != -1 && wIPAggregated != -1)
			{
				CalculateDifference(wIPAggregated, wIPExpected, "WIP");
			}
			if (aRExpected != -1 && aRAggregated != -1)
			{
				CalculateDifference(aRAggregated, aRExpected, "AR");
			}
			if (aPExpected != -1 && aPAggregated != -1)
			{
				CalculateDifference(aPAggregated, aPExpected, "AP");
			}

			Lines.Sort("LineType", System.ComponentModel.ListSortDirection.Ascending);
		}

		void CalculateDifference(int actualRowNumber, int expectedRowNumber, string ledger)
		{
			AggregationDiscrepanciesCalculatorLine line = Lines.AddNew();
			line.LineType = ledger + (NoResString)" Inequality";
			line.Period1 = Lines[actualRowNumber].Period1 - Lines[expectedRowNumber].Period1;
			line.Period2 = Lines[actualRowNumber].Period2 - Lines[expectedRowNumber].Period2;
			line.Period3 = Lines[actualRowNumber].Period3 - Lines[expectedRowNumber].Period3;
			line.Period4 = Lines[actualRowNumber].Period4 - Lines[expectedRowNumber].Period4;
			line.Period5 = Lines[actualRowNumber].Period5 - Lines[expectedRowNumber].Period5;
			line.Period6 = Lines[actualRowNumber].Period6 - Lines[expectedRowNumber].Period6;
			line.Period7 = Lines[actualRowNumber].Period7 - Lines[expectedRowNumber].Period7;
			line.Period8 = Lines[actualRowNumber].Period8 - Lines[expectedRowNumber].Period8;
			line.Period9 = Lines[actualRowNumber].Period9 - Lines[expectedRowNumber].Period9;
			line.Period10 = Lines[actualRowNumber].Period10 - Lines[expectedRowNumber].Period10;
			line.Period11 = Lines[actualRowNumber].Period11 - Lines[expectedRowNumber].Period11;
			line.Period12 = Lines[actualRowNumber].Period12 - Lines[expectedRowNumber].Period12;
		}

		#endregion

		#region SQL

		string SQL
		{
			get
			{
				string result = "";
				switch (CheckMethod)
				{
					case "WIPACR":
						{
							result = WIPACRSQL;
						} break;
					case "DebtorCreditor":
						{
							result = DebtorCreditorSQL;
						} break;
				}
				return result;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1140:ColumnNameCaseAnalyzer", Justification = "A separate work item will address this")]
		const string WIPACRSQL = @"
DECLARE
	@AM_PeriodFrom INT

SELECT @AM_PeriodFrom = [Value] FROM dbo.AddPeriod(@AM_PeriodTo, -@NumberOfPeriods, @GC_PK)

SELECT
	AL_Linetype
	,[1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12]
FROM
	(
		SELECT
			PeriodRank
			,AL_LineType
			,AL_LineAmount
		FROM
		(
			SELECT
				AM_Period
				,CASE WHEN AL_LineType = 'ACR' THEN 'ACR Expected' ELSE 'WIP Expected' END as AL_LineType
				,-AL_LineAmount as AL_LineAmount
			FROM 
				dbo.AccTransactionLines  
				INNER JOIN dbo.GlbBranch 
					ON 
					AL_GB = GB_PK
					AND GB_GC = @GC_PK 
				INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
				JOIN dbo.AccPeriodManagement ON AL_PostDate BETWEEN AM_StartDate AND AM_EndDate AND AM_GC_Company = @GC_PK
			WHERE 
				AL_LineType IN ('WIP', 'ACR') 
				and AL_PostToGL = 'Y'
				AND AC_ChargeType !='CMT'
				AND AM_Period between @AM_PeriodFrom and @AM_PeriodTo

			UNION all

			SELECT 
				AM_Period
				,CASE WHEN AL_LineType = 'ACR' THEN 'ACR Expected' ELSE 'WIP Expected' END as AL_LineType
				,AL_LineAmount
			FROM 
				dbo.AccTransactionLines
				INNER JOIN dbo.GlbBranch 
					ON 
					AL_GB = GB_PK
					AND GB_GC = @GC_PK 
				INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
				JOIN dbo.AccPeriodManagement ON AL_ReverseDate BETWEEN AM_StartDate AND AM_EndDate AND AM_GC_Company = @GC_PK
			WHERE
				AL_Reversedate is not null			
				AND AL_LineType IN ('WIP', 'ACR') 
				and AL_ReverseToGL = 'Y'
				AND AC_ChargeType !='CMT'
				AND AM_Period between @AM_PeriodFrom and @AM_PeriodTo
			
			UNION ALL
				
			SELECT
				AA_Period
				,'ACR Aggregated'	
				,AA_Amount
			FROM
				dbo.AccGLAggregate
				INNER JOIN dbo.GlbBranch on AA_GB = GB_PK
				INNER JOIN dbo.StmData 
					on 
					SD_GuidValue = AA_AG
					AND SD_Name = 'GL_ACCRUED_COST_ACCOUNT'
			WHERE
				AA_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
				AND GB_GC = @GC_PK
			
			UNION ALL
				
			SELECT
				AA_Period
				,'WIP Aggregated'	
				,AA_Amount
			FROM
				dbo.AccGLAggregate
				INNER JOIN dbo.GlbBranch on AA_GB = GB_PK
				INNER JOIN dbo.StmData 
					on 
					SD_GuidValue = AA_AG
					AND SD_Name = 'GL_ACCRUED_REVENUE_ACCOUNT'
			WHERE
				AA_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
				AND GB_GC = @GC_PK
				
		) WIPSACRS
		INNER JOIN 
		(
			SELECT
				Rank() over (order by  AM_Period) as PeriodRank
				,AM_Period
			FROM
				dbo.AccPeriodManagement
			WHERE	
				AM_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
				AND AM_GC_Company = @GC_PK
		) Periods
		ON
		Periods.AM_Period = WIPSACRS.AM_Period
	) data
	PIVOT 
	(
		SUM(AL_LineAmount) 
		FOR PeriodRank IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])  
	) p";

		const string DebtorCreditorSQL = @"
DECLARE
	@AM_PeriodFrom INT

SELECT @AM_PeriodFrom = [Value] FROM dbo.AddPeriod(@AM_PeriodTo, -@NumberOfPeriods, @GC_PK)

SELECT
	AH_Ledger
	,[1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12]
FROM
	(
		SELECT
			PeriodRank
			,CASE 
				WHEN AH_Ledger = 'AR' THEN	
					'AR Expected'
				WHEN AH_Ledger = 'AP' THEN 
					'AP Expected'
				ELSE	
					AH_Ledger 
			END as AH_Ledger
			,AL_LineAmount
		FROM
		(
			SELECT
				CASE AL_LineType
				WHEN 'CST' THEN
					'AP'
				WHEN 'REV' THEN
					'AR'
				END AS AH_Ledger,
				AM_Period,
				AL_GSTVAT+AL_LineAmount AL_LineAmount
			FROM
				dbo.AccTransactionheader
				JOIN dbo.AccTransactionLines ON AL_AH = AccTransactionHeader.AH_PK
				JOIN dbo.AccPeriodManagement 
					ON	 
					AL_PostDate BETWEEN AM_StartDate AND AM_EndDate 
					AND AM_GC_Company = @GC_PK
			where
				AH_Ledger IN ('AR', 'AP') 
				AND	AH_TransactionType IN ('INV', 'CRD', 'ADJ') 
			   	AND AH_PostToGL = 'Y' 
				AND	AH_GC = @GC_PK
				AND AM_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo

			UNION ALL
					
			SELECT
				AH_Ledger
				,AM_Period,
				AH_InvoiceAmount
			FROM
				dbo.AccTransactionheader 
				JOIN dbo.AccPeriodManagement ON AH_PostDate BETWEEN AM_StartDate AND AM_EndDate AND AM_GC_Company = @GC_PK
			WHERE 
			AH_Ledger IN ('AR', 'AP') 
			AND	AH_TransactionType IN ('PAY', 'REC') 
			AND AH_PostToGL = 'Y' 
			AND	AH_GC = @GC_PK
			AND AM_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
			
			UNION ALL
			
			SELECT
				AH_Ledger
				,AM_Period,
				AH_InvoiceAmount
			FROM
				dbo.AccTransactionheader 
				JOIN dbo.AccPeriodManagement ON AH_PostDate BETWEEN AM_StartDate AND AM_EndDate AND AM_GC_Company = @GC_PK
			WHERE 
				AH_Ledger IN ('AR', 'AP') AND
				AH_TransactionType = 'CTR' AND 
				AH_PostToGL = 'Y' AND
				AH_GC = @GC_PK
				and AM_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
				
			UNION ALL
			
			SELECT
				AH_Ledger
				,AM_Period,
				AH_InvoiceAmount
			FROM
				dbo.AccTransactionheader 
				JOIN dbo.AccPeriodManagement ON AH_PostDate BETWEEN AM_StartDate AND AM_EndDate AND AM_GC_Company = @GC_PK 
			WHERE 
				AH_Ledger IN ('AR', 'AP') AND
				AH_TransactionType IN ('JNL') AND 
				AH_PostToGL = 'Y' AND
				AH_GC = @GC_PK
				AND AM_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo

			UNION ALL
			
			-- AR/AP Control Account Posting
			SELECT
				AH_Ledger
				,AM_Period,
				(AH_InvoiceAmount)
			FROM
				dbo.AccTransactionheader 
				JOIN dbo.AccPeriodManagement ON AH_PostDate BETWEEN AM_StartDate AND AM_EndDate AND AM_GC_Company = @GC_PK
			WHERE 
				AH_Ledger IN ('AR', 'AP') AND
				AH_TransactionType IN ('OVP', 'DSC', 'EXX') AND 
				AH_PostToGL = 'Y' AND
				AH_GC = @GC_PK
				and AM_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
			
			UNION ALL
				
			SELECT
				'AR Aggregated'	
				,AA_Period
				,AA_Amount
			FROM
				dbo.AccGLAggregate
				INNER JOIN dbo.GlbBranch on AA_GB = GB_PK
				INNER JOIN dbo.StmData 
					on 
					SD_GuidValue = AA_AG
					AND SD_Name = 'GL_AR_CONTROL_ACCOUNT'
			WHERE
				AA_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
			   AND GB_GC = @GC_PK
			
			UNION ALL
				
			SELECT
				'AP Aggregated'	
				,AA_Period
				,AA_Amount
			FROM
				dbo.AccGLAggregate
				INNER JOIN dbo.GlbBranch on AA_GB = GB_PK
				INNER JOIN dbo.StmData 
					on 
					SD_GuidValue = AA_AG
					AND SD_Name = 'GL_AP_CONTROL_ACCOUNT'
			WHERE
				AA_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
				AND GB_GC = @GC_PK	
				
				
			) TXNs
		INNER JOIN 
		(
			SELECT
				Rank() over (order by  AM_Period) as PeriodRank
				,AM_Period
			FROM
				dbo.AccPeriodManagement
			WHERE	
				AM_Period BETWEEN @AM_PeriodFrom AND @AM_PeriodTo
				AND AM_GC_Company = @GC_PK
		) Periods
		ON
		Periods.AM_Period = TXNs.AM_Period
	)
	data
	PIVOT 
	(
		SUM(AL_LineAmount) 
		FOR PeriodRank IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])  
	) p	
Order By
	AH_Ledger";

		#endregion
	}
}
