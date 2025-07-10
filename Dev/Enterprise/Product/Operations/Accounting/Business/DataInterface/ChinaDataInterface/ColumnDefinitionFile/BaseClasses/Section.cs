using System.Collections;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public abstract class Section : IFileOutput
	{
		public Section(BusinessObjectFactory factory)
		{
			Factory = factory;
			Lines = new ArrayList();
			SetDefault();
			Define();
		}

		protected readonly BusinessObjectFactory Factory;

		public void Write(StreamWriter writer)
		{
			SetValue();
			AddLines();
			WriteSectionHeader(writer);

			foreach (ColumnDefinitionLine line in Lines)
			{
				line.Write(writer);
			}
		}

		protected void WriteSectionHeader(StreamWriter writer)
		{
			writer.WriteLine(fSectionHeader);
		}

		protected string SectionHeader
		{
			get
			{
				return fSectionHeader;
			}
		}

		protected abstract void Define();
		protected abstract void SetDefault();
		protected abstract void SetValue();
		protected abstract void AddLines();

		internal protected ZString GetAccountingYear(ZDateTime date)
		{
			if (!date.IsEmpty)
			{
				return GetAccountingYearByPeriodBeginAndEnd(date);
			}

			return "0";
		}

		protected ZString GetAccountingYearByPeriodBeginAndEnd(ZDateTime date)
		{
			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement period = periodCalc.GetPeriodManagementFromDate(date);

			return period != null ? period.AM_StartDate.ToString(Format) : "0";
		}

		protected ArrayList Lines;
		protected string fSectionHeader;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Year format string")]
		protected string Format
		{
			get
			{
				return "yyyy";
			}
		}
	}
}
