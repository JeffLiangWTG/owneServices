using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ReportTableProviders
{
	public class GLAccountMultiLingualDataProvider : GLAccountDocumentDataProvider
	{
		public GLAccountMultiLingualDataProvider()
		{
		}

		#region Document Provider Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Parameter Name Does Not Need To Be Localised")]
		protected override Parameter[] ExpectedParameters()
		{
			return new Parameter[]
			{
				new Parameter("Start Period", typeof(ZInt)),
				new Parameter("End Period", typeof(ZInt)),
				new Parameter("Start Account", typeof(Guid)),
				new Parameter("End Account", typeof(Guid)),
				new Parameter("CurrentCompany", typeof(Guid)),
				new Parameter("BranchPK", typeof(Guid)),
				new Parameter("DepartmentPK", typeof(Guid)),
				new Parameter("Language", typeof(object)),
				new Parameter("Incl all Accounts", typeof(object))
			};
		}

		protected override void SetupParameterValues(object[] values)
		{
			StartPeriod = (ZInt)values[0];
			EndPeriod = (ZInt)values[1];

			if (StartPeriod > EndPeriod)
			{
				EndPeriod = StartPeriod;
				StartPeriod = (ZInt)values[1];
			}

			LocalAccountStart = (Guid)values[2];
			LocalAccounEnd = (Guid)values[3];
			StartAccount = new LocalAccount(LocalAccountStart);
			EndAccount = new LocalAccount(LocalAccounEnd);
			Company = (Guid)values[4];
			BranchPK = (Guid)values[5];
			DepartmentPK = (Guid)values[6];
			Language = ((values.Length > 7 && values[7] != DBNull.Value) ? (ZString)values[7] : ZString.Empty);
			IncludeAccountsWithoutTransactions = values[8].ToString() == "Y";
			SetPeriodRange(StartPeriod, EndPeriod);
			SetAccountRange();
		}

		protected bool IncludeAccountsWithoutTransactions;

		#endregion

		#region SQL

		protected override string GetSQL()
		{
			string sQL = "";

			sQL += SetGLFromHeader();
			sQL += SetGetGLFromHeaderBank();
			if (fGLTransactionContainsControlAccount)
			{
				sQL += SetGetGLFromControlAccount();
			}
			sQL += SetDirectReceiptPaymentLine();
			sQL += SetDirectReceiptPaymentBank();
			sQL += SetInvCrdAdj();
			sQL += SetWIPSJournal();
			sQL += SetWIPSJournalReverse();
			sQL += SetCFX();
			sQL += SetGLJournal();
			sQL += SetGLAutoJournal();
			sQL += SetGLReverseJournal();

			return sQL;
		}

		protected override void SetAccountRange()
		{
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.GreaterThanOrEqualTo, StartAccount.AccountNo);
			filter.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.LessThanOrEqualTo, EndAccount.AccountNo);
			filter.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_Language, SQLComparisonOperator.Equal, Language);
			AccountList = Factory.Load(typeof(AccGLAccountDescriptor), filter) as AccGLAccountDescriptor[];
		}

		protected override void GetControlAccount()
		{
			fARControl = new LocalAccount(AccountingConfigurationRegistry.Instance.ARControlAccount.Value, Language);
			fAPControl = new LocalAccount(AccountingConfigurationRegistry.Instance.APControlAccount.Value, Language);
			fExchangeDifference = new LocalAccount(AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value, Language);
			fDiscount = new LocalAccount(AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value, Language);
			fOverpayment = new LocalAccount(AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value, Language);
			fGSTInput = new LocalAccount(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, Language);
			fGSTOutput = new LocalAccount(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value, Language);
			fWIPAccount = new LocalAccount(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value, Language);
			fACRAccount = new LocalAccount(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value, Language);
			fPendingGSTInput = new LocalAccount(AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value, Language);
			fPendingGSTOutput = new LocalAccount(AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value, Language);
			fJobRevenueJournalControl = new LocalAccount(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value, Language);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		protected override void SetWhereClause()
		{
			if (!fGLTransactionContainsControlAccount)
			{
				WhereClause = " AND " + AccGLAccountDescriptorSchema.AJ_LocalAccountNumber.Name + " >= '" + StartAccount.AccountNo + "' AND " +
					AccGLAccountDescriptorSchema.AJ_LocalAccountNumber.Name + " <= '" + EndAccount.AccountNo + "' AND " +
					AccGLAccountDescriptorSchema.AJ_Language.Name + " = '" + Language + "'" +
					@" AND " + AccGLAccountDescriptorSchema.AJ_ReportType.Name + " = '" + AccGLAccountDescriptor.ReportTypeCOA + "'" +
					@" AND " + AccGLAccountDescriptorSchema.AJ_ReportCategory.Name + " IN ('BSH', 'P&L')";
			}
		}

		protected override bool SetGLTransactionContainsControlAccount()
		{
			foreach (AccGLAccountDescriptor header in AccountList)
			{
				if (fControlAccountList.Contains(header.PK.ToGuid()))
				{
					return true;
				}
			}
			return false;
		}

		protected override void SetOpeningBalance(GLAccountListDocumentDataSet dataSetToProcess)
		{
			SetLocalAccountOpeningBalance(dataSetToProcess);
		}

		void SetLocalAccountOpeningBalance(GLAccountListDocumentDataSet dataSetToProcess)
		{
			string filter = (NoResString)" AND (" + Business.AccountingUtils.GenerateCompanyFilter(AccGLAggregateSchema.AA_GB, new BusinessObjectFactory()).LiteralTextSqlFormatted + (NoResString)")";

			foreach (int period in PeriodRange)
			{
				foreach (AccGLAccountDescriptor account in AccountList)
				{
					string innerFilter = " Period = " + period + " AND GLAccount = '" + account.AJ_LocalAccountNumber + "'";
					decimal openingBalance = GetOpeningBalance(account.AJ_LocalAccountNumber, period, filter);
					DataRow[] rows = dataSetToProcess.GLAccountListDataSet.Select(innerFilter);
					if (rows.Length == 0 && IncludeAccountsWithoutTransactions)
					{
						InsertEmptyLocalAccountRow(dataSetToProcess, period, account, openingBalance);
					}
					else
					{
						SetOpeningBalance(rows, openingBalance);
					}
				}
			}
		}

		void InsertEmptyLocalAccountRow(GLAccountListDocumentDataSet dataSetToProcess, int period, AccGLAccountDescriptor account, decimal openingBalance)
		{
			GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd = dataSetToProcess.GLAccountListDataSet.NewGLAccountListDataSetRow();

			rowToAdd.Period = period;
			rowToAdd.GLAccount = account.AJ_LocalAccountNumber;
			rowToAdd.GLAccountDesc = account.AJ_AccountDescription;
			SetOpeningBalance(rowToAdd, openingBalance);

			dataSetToProcess.GLAccountListDataSet.AddGLAccountListDataSetRow(rowToAdd);
		}

		protected override decimal GetOpeningBalance(string accountNum, int period, string companyFilter)
		{
			return GetLocalAccountOpeningBalance(accountNum, period, companyFilter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		decimal GetLocalAccountOpeningBalance(string accountNum, int period, string companyFilter)
		{
			int startingPeriod = 0;
			string accountType = GetLocalAccountType(accountNum);
			string additionalFilter = "";
			if (accountType == "P&L")
			{
				startingPeriod = GetStartPeriodOfFinancialYear(period);
			}

			if (BranchPK != Guid.Empty)
			{
				additionalFilter += " AND AA_GB = '" + BranchPK + "'";
			}

			if (DepartmentPK != Guid.Empty)
			{
				additionalFilter += " AND AA_GE = '" + DepartmentPK + "'";
			}

			string sQL = @"Select sum(AA_Amount)
							From dbo.accglaggregate 
							inner join dbo.accglheader on AG_PK = AA_AG 
							INNER JOIN dbo.AccGLDescriptorPivot on YJ_AG = AA_AG
							inner join dbo.accglaccountdescriptor ON YJ_AJ = AJ_PK 
							Where 
							(AA_Period >= @StartingPeriod 
							AND AA_Period < @Period) 
							AND AJ_LocalAccountNumber = @AccountNum
							AND AJ_Language = @Language "
				+ companyFilter + additionalFilter;

			DbCommand command = ((IDbConnected)Factory).Connection.Command(sQL);
			command.AddParameter("@Period", SqlDbType.Int, period);
			command.AddParameter("@StartingPeriod", SqlDbType.Int, startingPeriod);
			command.AddParameter("@AccountNum", SqlDbType.VarChar, accountNum);
			command.AddParameter("@Language", SqlDbType.Char, 3, Language);

			return Utilities.ConvertToDecimal(command.ExecuteScalar());
		}

		string GetLocalAccountType(string accountNum)
		{
			string accountType = "";
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.Equal, accountNum);
			filter.MaximumRows = 1;
			AccGLAccountDescriptor gLAccountDescriptor = (AccGLAccountDescriptor)(Factory.Load(typeof(AccGLAccountDescriptor), filter)[0]);

			if (gLAccountDescriptor != null)
			{
				accountType = gLAccountDescriptor.AJ_ReportCategory;
			}

			return accountType;
		}

		protected override string SetGLFromHeader()
		{
			return @" SELECT			AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate,
										AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AH_InvoiceAmount as Amount,
										AH_GSTAmount as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK
						
						FROM			dbo.AccTransactionHeader 
						INNER JOIN		dbo.AccGLHeader ON AH_AG = AG_PK
						INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
						INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
						INNER JOIN		dbo.GlbBranch ON AH_GB = GB_PK
						INNER JOIN		dbo.GlbDepartment ON AH_GE = GE_PK
						LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
						LEFT OUTER JOIN	dbo.OrgHeader ON AH_OH = OH_PK
					
						WHERE 			AH_Ledger in ('AR', 'AP') 
										AND AH_TransactionType = 'JNL'
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained from AH_AB
		/// Header Only
		/// AR & AP Payment and Receipt, CB Transfer and CB Exchange Differences
		/// </summary>
		/// <returns></returns>
		protected override string SetGetGLFromHeaderBank()
		{
			return @"	SELECT			AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate,
										AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AH_InvoiceAmount as Amount,
										AH_GSTAmount as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK

						FROM			dbo.AccTransactionHeader 
						INNER JOIN		dbo.AccBankAccount ON AH_AB = AB_PK
						INNER JOIN		dbo.AccGLHeader ON AB_AG = AG_PK
						INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
						INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
						INNER JOIN		dbo.GlbBranch ON AH_GB = GB_PK
						INNER JOIN		dbo.GlbDepartment ON AH_GE = GE_PK
						LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
						LEFT OUTER JOIN	dbo.OrgHeader ON AH_OH = OH_PK

						WHERE 	((AH_Ledger IN ('AR', 'AP') AND AH_TransactionType IN ('PAY', 'REC')) OR
								(AH_Ledger = 'CB' AND AH_TransactionType in ('TRF', 'EXX')))
								AND AH_PostToGL = 'Y'
								AND GB_GC = @CurrentCompany
								AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generate Transactions which GL is obtained from their respected Control Account 
		/// Header Only
		/// Overpayment, Discount, AR & AP Exchange Diff, Contra and Transfer
		/// </summary>
		/// <returns></returns>
		protected override string SetGetGLFromControlAccount()
		{
			return @" SELECT			'' as GLAccount,
										'' as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate,
										AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AH_InvoiceAmount as Amount,
										AH_GSTAmount as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK

						FROM 			dbo.AccTransactionHeader 
						INNER JOIN		dbo.GlbBranch ON AH_GB = GB_PK
						INNER JOIN		dbo.GlbDepartment ON AH_GE = GE_PK
						LEFT OUTER JOIN	dbo.OrgHeader ON AH_OH = OH_PK
						LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK

						WHERE	(AH_TransactionType in ('OVP', 'DSC') OR
								(AH_TransactionType in ('EXX', 'CTR', 'TRF') AND AH_Ledger in ('AR', 'AP')))
								AND AH_PostToGL = 'Y'
								AND GB_GC = @CurrentCompany
								AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained From Transaction Line
		/// Header and Lines
		/// Direct Receipt & Payment
		/// </summary>
		/// <returns></returns>
		protected override string SetDirectReceiptPaymentLine()
		{
			return @"	SELECT			AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate,
										AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										'LINE' AS ExtraInfo
						
						FROM 			dbo.AccTransactionHeader 
						INNER JOIN		dbo.AccTransactionLines ON AH_PK = AL_AH
						INNER JOIN		dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN		dbo.GlbDepartment ON AL_GE = GE_PK
						INNER JOIN		dbo.AccGLHeader ON AL_AG = AG_PK 	
						INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
						INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK	
						LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
						LEFT OUTER JOIN	dbo.OrgHeader ON AL_OH = OH_PK
						LEFT OUTER JOIN dbo.AccChargeCode ON AL_AC = AC_PK					

						WHERE 	AH_TransactionType in ('DRC', 'DPY')
								AND AH_Ledger = 'CB'
								AND AH_PostToGL = 'Y'
								AND GB_GC = @CurrentCompany
								AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained from the Header's Bank
		/// Header Only
		/// Direct Receipt & Payment
		/// </summary>
		/// <returns></returns>
		protected override string SetDirectReceiptPaymentBank()
		{
			return @"	SELECT			AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AH_InvoiceAmount as Amount,
										AH_GSTAmount as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										'BANK' AS ExtraInfo

						FROM 			dbo.AccTransactionHeader 
						INNER JOIN		dbo.AccBankAccount ON AH_AB = AB_PK 
						INNER JOIN		dbo.AccGLHeader ON AB_AG = AG_PK 
						INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
						INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
						INNER JOIN		dbo.GlbBranch ON AH_GB = GB_PK
						INNER JOIN		dbo.GlbDepartment ON AH_GE = GE_PK
						LEFT OUTER JOIN	dbo.JobHeader ON AH_JH = JH_PK
						LEFT OUTER JOIN	dbo.OrgHeader ON AH_OH = OH_PK
						
						WHERE 			AH_TransactionType in ('DRC', 'DPY')
										AND AH_Ledger = 'CB'
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained either directly from the Lines or Line Charge
		/// Header and Lines
		/// Invoice, Credit Notes and Adjustment
		/// </summary>
		/// <returns></returns>
		protected override string SetInvCrdAdj()
		{
			return @"	SELECT			AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo

						FROM 			dbo.AccTransactionHeader 
						INNER JOIN		dbo.AccTransactionLines ON AH_PK = AL_AH 
						LEFT OUTER JOIN	dbo.AccChargeCode ON AL_AC = AC_PK 
						INNER JOIN		dbo.AccGLHeader ON CASE WHEN AL_AC is null THEN AL_AG ELSE 
														(CASE	WHEN AL_LineType = 'REV' THEN AC_AG_RevenueAccount
																WHEN AL_LineType = 'CST' THEN AC_AG_CostAccount
																ELSE AL_AG END) END = AG_PK
						INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
						INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
						INNER JOIN		dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN		dbo.GlbDepartment ON AL_GE = GE_PK
						LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
						LEFT OUTER JOIN dbo.OrgHeader ON AH_OH = OH_PK
						
						WHERE 			AH_Ledger IN ('AP', 'AR') 
										AND AH_TransactionType IN ('INV', 'CRD', 'ADJ')
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained from the Line Charges
		/// Header and Lines
		/// WIPS and Accruals
		/// </summary>
		/// <returns></returns>
		protected override string SetWIPSJournal()
		{
			return @"	SELECT			AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AL_PostDate as InvoiceDate, AL_PostDate as PostDate,
										dbo.GetPeriodFromDate (AL_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										'GL' as Ledger,
										AL_LineType as Type,
										'' as TransactionNumber,
										" + GetAL_DescTruncated() + @" as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AL_ReverseDate, @CurrentCompany) as ReversePeriod
						
						FROM 			dbo.AccTransactionLines
						INNER JOIN 		dbo.AccChargeCode ON AL_AC = AC_PK 
						INNER JOIN 		dbo.AccGLHeader ON CASE WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount ELSE AC_AG_AccrualAccount END = AG_PK
						INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
						INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
						INNER JOIN 		dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN 		dbo.GlbDepartment ON AL_GE = GE_PK
						LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
						LEFT OUTER JOIN dbo.OrgHeader ON AL_OH = OH_PK
						
						WHERE 			AL_PostToGL = 'Y'
										AND AL_LineType in ('WIP', 'ACR')
										AND GB_GC = @CurrentCompany
										AND AL_PostDate Between @StartDate AND @EndDate "
				+ WhereClause;
		}

		protected override string SetWIPSJournalReverse()
		{
			return @"	SELECT			AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AL_PostDate as InvoiceDate, AL_PostDate as PostDate,
										dbo.GetPeriodFromDate (AL_ReverseDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										'GL' as Ledger,
										AL_LineType as Type,
										'' as TransactionNumber,
										" + GetAL_DescTruncated() + @" as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										-AL_LineAmount as Amount,
										-AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AL_ReverseDate, @CurrentCompany) as ReversePeriod
						
						FROM 			dbo.AccTransactionLines
						INNER JOIN 		dbo.AccChargeCode ON AL_AC = AC_PK 
						INNER JOIN 		dbo.AccGLHeader ON CASE WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount ELSE AC_AG_AccrualAccount END = AG_PK
						INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
						INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
						INNER JOIN 		dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN 		dbo.GlbDepartment ON AL_GE = GE_PK
						LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
						LEFT OUTER JOIN dbo.OrgHeader ON AL_OH = OH_PK
						
						WHERE 			AL_ReverseToGL = 'Y'
										AND AL_LineType in ('WIP', 'ACR')
										AND GB_GC = @CurrentCompany
										AND AL_ReverseDate Between @StartDate AND @EndDate  "
				+ WhereClause;
		}

		/// <summary>
		/// Generates Transactions where GL is obtained both Charges and Lines
		/// Headers and Lines
		/// CFX (JobCosting Ledger Type)
		/// </summary>
		/// <returns></returns>
		protected override string SetCFX()
		{
			return @"SELECT				AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,										
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										AC_Code as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										'CHARGE' as ExtraInfo
						
						FROM 			dbo.AccTransactionHeader 
										INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH 
										INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK 
										INNER JOIN dbo.AccGLHeader ON CASE 	WHEN AL_LineType = 'REV' THEN AC_AG_RevenueAccount 
														WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount
														WHEN AL_LineType = 'CST' THEN AC_AG_CostAccount
														ELSE AC_AG_AccrualAccount END = AG_PK
										INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
										INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
										LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
										LEFT OUTER JOIN dbo.OrgHeader ON AH_OH = OH_PK
						
						WHERE 			AH_Ledger IN ('JC') 
										AND AH_TransactionType IN ('JNL')
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause +
				@" SELECT				AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										JH_JobNum as Job,
										'' as ChargeCode,
										OH_Code as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										'LINE' as ExtraInfo
						
						FROM 			dbo.AccTransactionHeader 
										INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH 
										INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
										INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
										INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
										LEFT OUTER JOIN	dbo.JobHeader ON AL_JH = JH_PK
										LEFT OUTER JOIN dbo.OrgHeader ON AH_OH = OH_PK
						
						WHERE 			AH_Ledger IN ('JC') 
										AND AH_TransactionType IN ('JNL')
										AND AH_PostToGL = 'Y'
										AND GB_GC = @CurrentCompany
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		protected override string SetGLJournal()
		{
			return @"SELECT				AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										'' as Job,
										'' as ChargeCode,
										'' as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AH_DueDate, @CurrentCompany) as ReversePeriod
					
					FROM				dbo.AccTransactionHeader
										INNER JOIN dbo.AccTransactionLines on AH_PK = AL_AH
										INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
										INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
										INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
					
					WHERE				AH_Ledger  = 'GL' 
										AND AH_TransactionType in ('GJL')
										AND GB_GC = @CurrentCompany 
										AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) "
				+ WhereClause;
		}

		protected override string SetGLAutoJournal()
		{
			return @"SELECT				AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_InvoiceDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										'' as Job,
										'' as ChargeCode,
										'' as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AH_DueDate, @CurrentCompany) as ReversePeriod
					
					FROM				dbo.AccTransactionHeader
										INNER JOIN dbo.AccTransactionLines on AH_PK = AL_AH
										INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
										INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
										INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
					
					WHERE				AH_Ledger  = 'GL' 
										AND AH_TransactionType in ('AJL')
										AND GB_GC = @CurrentCompany 
										AND NOT
											(AH_DueDate < @StartDate
											OR AH_PostDate >= @EndDate) "
				+ WhereClause;
		}

		protected override string SetGLReverseJournal()
		{
			return @"SELECT				AJ_LocalAccountNumber as GLAccount,
										AJ_AccountDescription as GLAccountDesc,
										AH_DueDate as InvoiceDate, AH_PostDate as PostDate,
										dbo.GetPeriodFromDate (AH_PostDate, @CurrentCompany) as Period,
										GB_Code as Branch,
										GE_Code as Department,
										AH_Ledger as Ledger,
										AH_TransactionType as Type,
										AH_TransactionNum as TransactionNumber,
										AH_Desc as TransactionDesc,
										'' as Job,
										'' as ChargeCode,
										'' as Account,
										AL_LineAmount as Amount,
										AL_GSTVAT as GST,
										GB_PK as BranchPK,
										GE_PK as DepartmentPK,
										AL_LineType as ExtraInfo,
										dbo.GetPeriodFromDate (AH_DueDate, @CurrentCompany) as ReversePeriod
					
					FROM				dbo.AccTransactionHeader
										INNER JOIN dbo.AccTransactionLines on AH_PK = AL_AH
										INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
										INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = AG_PK
										INNER JOIN dbo.AccGLAccountDescriptor ON YJ_AJ = AJ_PK
										INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
										INNER JOIN dbo.GlbDepartment ON AL_GE = GE_PK
					
					WHERE				AH_Ledger  = 'GL' 
										AND AH_TransactionType in ('RJL')
										AND GB_GC = @CurrentCompany 
										AND ((AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate) 
										OR (AH_DueDate >= @StartDate AND AH_DueDate <= @EndDate)) "
				+ WhereClause;
		}

		#endregion
	}
}
