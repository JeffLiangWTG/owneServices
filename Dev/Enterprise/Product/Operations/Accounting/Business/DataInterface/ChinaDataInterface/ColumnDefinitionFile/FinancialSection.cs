using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class FinancialSection : Section
	{
		public FinancialSection(BusinessObjectFactory factory)
			: base(factory)
		{
			fSectionHeader = "[帐套]";
		}

		public ZDateTime Date
		{
			get { return fDate; }
			set { fDate = value; }
		}
		ZDateTime fDate;

		protected override void SetDefault()
		{
		}

		protected override void Define()
		{
			FinancialSystemSetLine = new ColumnDefinitionLine("帐套");
			SetNameLine = new ColumnDefinitionLine("帐套名称");
			CompanyNameLine = new ColumnDefinitionLine("单位名称");
			AccountingYearLine = new ColumnDefinitionLine("会计年度");
			IndustryLine = new ColumnDefinitionLine("行业");
			CompanyRegistrationNumLine = new ColumnDefinitionLine("单位组织机构代码");
		}

		protected override void SetValue()
		{
			fCurrentAccountingYear = GetAccountingYear(fDate).ToString();
			FinancialSystemSetLine.Value = "1,1";
			SetNameLine.Value = BrandingFactory.Instance.ProductName;
			CompanyNameLine.Value = GlbCompany.CurrentCompany.GC_Name;
			AccountingYearLine.Value = fCurrentAccountingYear;
			IndustryLine.Value = "运输代理服务";
			CompanyRegistrationNumLine.Value = GlbCompany.CurrentCompany.GC_BusinessRegNo;
		}

		protected override void AddLines()
		{
			Lines.Add(FinancialSystemSetLine);
			Lines.Add(SetNameLine);
			Lines.Add(CompanyNameLine);
			Lines.Add(AccountingYearLine);
			Lines.Add(IndustryLine);
			Lines.Add(CompanyRegistrationNumLine);
		}

		ColumnDefinitionLine FinancialSystemSetLine;
		ColumnDefinitionLine SetNameLine;
		ColumnDefinitionLine CompanyNameLine;
		ColumnDefinitionLine CompanyRegistrationNumLine;
		ColumnDefinitionLine AccountingYearLine;
		ColumnDefinitionLine IndustryLine;

		internal string fCurrentAccountingYear;
	}
}
