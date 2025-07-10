using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class BookKeepingSection : Section
	{
		public BookKeepingSection(ZDateTime startDate, BusinessObjectFactory factory)
			: base(factory)
		{
			fStartDate = startDate;
			fFactory = factory;
			fSectionHeader = "[帐务]";
		}

		readonly ZDateTime fStartDate;
		readonly BusinessObjectFactory fFactory;

		protected override void Define()
		{
			SoftwareCompanyLine = new ColumnDefinitionLine("软件公司");
			SoftwareVersionLine = new ColumnDefinitionLine("软件版本");
			CommencementDateLine = new ColumnDefinitionLine("启用日期");

			NoOfPeriodInAYear = new ColumnDefinitionLine("会计期数");
			CurrentAccountingYearStartDateLine = new ColumnDefinitionLine("启用会计期");
			CurrentPeriodStartDateLine = new ColumnDefinitionLine("当前会计期");
		}

#if DEBUG
		public void Define_ForTestOnly()
		{
			Define();
		}

		public void SetValue_ForTestOnly()
		{
			SetValue();
		}
#endif

		protected override void SetDefault()
		{
		}

		protected override void SetValue()
		{
			SoftwareCompanyLine.Value = "Eagle Datamation International Pty. Ltd.";
			SoftwareVersionLine.Value = "1.1";
			NoOfPeriodInAYear.Value = GetNoOfPeriodInAYear(fFactory);
			CommencementDateLine.Value = GetCommencementDate();
			CurrentAccountingYearStartDateLine.Value = GetAccountingYearFromDate(fFactory).ToString(DataInterfaceConstant.DateFormat);
			CurrentPeriodStartDateLine.Value = fStartDate.ToString(DataInterfaceConstant.DateFormat);
		}

		protected override void AddLines()
		{
			Lines.Add(SoftwareCompanyLine);
			Lines.Add(SoftwareVersionLine);
			Lines.Add(NoOfPeriodInAYear);
			Lines.Add(CommencementDateLine);
			Lines.Add(CurrentAccountingYearStartDateLine);
			Lines.Add(CurrentPeriodStartDateLine);
		}

		string GetCommencementDate()
		{
			ZString commencementDate = "";
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			filter.OrderBy = AccTransactionHeaderSchema.Constants.AH_PostDate;

			AccTransactionHeader firstTransaction = fFactory.LoadTop1(typeof(AccTransactionHeader), filter) as AccTransactionHeader;
			if (firstTransaction != null)
			{
				commencementDate = firstTransaction.AH_PostDate.ToString(DataInterfaceConstant.DateFormat);
			}
			return commencementDate;
		}

		ZString GetNoOfPeriodInAYear(BusinessObjectFactory factory)
		{
			ZString periodCount = "";

			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(factory);
			AccPeriodManagement currentPeriod = periodCalc.GetPeriodManagementFromDate(fStartDate);
			ZInt firstPeriod = periodCalc.GetFirstPeriodForYear(currentPeriod.AM_Year);
			ZInt lastPeriod = periodCalc.GetLastPeriodForYear(currentPeriod.AM_Year);

			periodCount = periodCalc.GetPeriodCount(firstPeriod, lastPeriod).ToString();
			return periodCount;
		}

		ZDateTime GetAccountingYearFromDate(BusinessObjectFactory factory)
		{
			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(factory);
			AccPeriodManagement currentPeriod = periodCalc.GetPeriodManagementFromDate(fStartDate);
			if (currentPeriod != null)
			{
				return periodCalc.GetFirstDayForPeriod(periodCalc.GetFirstPeriodForYear(currentPeriod.AM_Year));
			}
			else
			{
				return ZDateTime.Now;
			}
		}

		ColumnDefinitionLine SoftwareCompanyLine;
		ColumnDefinitionLine SoftwareVersionLine;
		internal ColumnDefinitionLine CommencementDateLine;
		ColumnDefinitionLine NoOfPeriodInAYear;
		ColumnDefinitionLine CurrentAccountingYearStartDateLine;
		ColumnDefinitionLine CurrentPeriodStartDateLine;
	}
}
