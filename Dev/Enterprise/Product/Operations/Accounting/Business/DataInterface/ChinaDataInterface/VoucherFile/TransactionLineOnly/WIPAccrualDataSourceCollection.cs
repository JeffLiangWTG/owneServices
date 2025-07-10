using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class WIPAccrualDataSourceCollection : DynamicBusinessObjectCollection<VoucherDataSource>
	{
		public WIPAccrualDataSourceCollection()
			: this(new BusinessObjectFactory())
		{
		}

		public WIPAccrualDataSourceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		// Set to virtual to allow Mocking
		public virtual int GetCount()
		{
			return Count;
		}

		// Set to virtual to allow Mocking
		public virtual VoucherDataSource GetVoucherData(int index)
		{
			return this[index];
		}

		public void LoadCollection(int period, string lineType, bool isGroupByBranchCode = false)
		{
			fPeriodToReport = period;
			startDate = (new AccountingPeriodCalculator(Factory)).GetFirstDayForPeriod(period);
			endDate = (new AccountingPeriodCalculator(Factory)).GetLastDayForPeriod(period).AddDays(1).Date;
			fLineType = lineType;
			fIsGroupByBranchCode = isGroupByBranchCode;
			Load(SQL, SQLParams);
		}

		public void LoadCollection(ZDateTime startDate, ZDateTime endDate, string lineType)
		{
			fLineType = lineType;
			fPeriodToReport = (new AccountingPeriodCalculator(Factory)).GetPeriodFromDate(startDate);
			this.startDate = startDate.Date;
			this.endDate = endDate.AddDays(1).Date;
			Load(SQL, SQLParams);
		}

		#region Implementation
		ZDateTime startDate;
		ZDateTime endDate;
		int fPeriodToReport;
		ZString fLineType;
		bool fIsGroupByBranchCode;

		ZSqlParameterCollection SQLParams
		{
			get
			{
				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC);
				@params.Add("@VoucherNumber", DataInterfaceUtils.GetVoucherNumberFowWIPAccrual(fLineType, fPeriodToReport), AccTransactionHeaderSchema.AH_TransactionNum);

				@params.Add("@LineType", fLineType, AccTransactionLinesSchema.AL_LineType);

				@params.Add("@StartDate", startDate, AccTransactionLinesSchema.AL_PostDate);
				@params.Add("@EndDate", endDate, AccTransactionLinesSchema.AL_PostDate);

				// Constant
				@params.Add("@Y", "Y", AccTransactionLinesSchema.AL_PostToGL);

				return @params;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query")]
		string SQL
		{
			get
			{
				var branchCode = fIsGroupByBranchCode ? "GB_CODE AS BranchCode" : "NULL AS BranchCode";
				var branchCodeForGroupByString = fIsGroupByBranchCode ? "Group By GLHeader, AL_LineType, GB_CODE" : "Group By GLHeader, AL_LineType";
				var branchCodeForResultColumn = fIsGroupByBranchCode ? ", GB_CODE" : "";
				return @$"SELECT 
								{branchCode}, 
								NULL AS DepartmentCode, 
								GLHeader, 
								SUM(AL_LineAmount) as Amount, 
								SUM(AL_OSAmount) as OSAmount, 
								AL_LineType as TransactionType, 
								@VoucherNumber as VoucherNumber
							FROM (
								SELECT	AL_AG as GLHeader, AL_LineAmount, AL_OSAmount, AL_LineType {branchCodeForResultColumn}
								FROM 			dbo.AccTransactionLines
								INNER JOIN 		dbo.GlbBranch ON AL_GB = GB_PK
								WHERE 			AL_LineType = @LineType
								AND GB_GC = @CurrentCompany
								AND AL_PostDate >= @StartDate AND AL_PostDate < @EndDate
							UNION ALL
								SELECT	AL_AG as GLHeader, -AL_LineAmount as AL_LineAmount, -AL_OSAmount AS AL_OSAmount, AL_LineType {branchCodeForResultColumn}
								FROM 			dbo.AccTransactionLines
								INNER JOIN 		dbo.GlbBranch ON AL_GB = GB_PK
								WHERE 			AL_ReverseDate is not NULL
								AND AL_LineType = @LineType
								AND GB_GC = @CurrentCompany
								AND AL_ReverseDate >= @StartDate AND AL_ReverseDate < @EndDate
							) as InnerSelection
							{branchCodeForGroupByString}";
			}
		}

		#endregion
	}
}
