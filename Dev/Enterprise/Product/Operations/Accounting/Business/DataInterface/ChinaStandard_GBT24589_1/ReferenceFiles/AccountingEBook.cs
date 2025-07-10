using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Fixed or constant values")]
	public sealed class AccountingEBook : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T101";
		public const string BookNumber = "1";
		public ZString BookName { get { return ZString.Format("{0}{1}", LocalCompanyName.GetCurrentCompanyLocalName(), "账套"); } }
		public ZString CompanyName { get { return LocalCompanyName.GetCurrentCompanyLocalName(); } }
		public ZString CompanyRegistrationCode { get { return GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo; } }
		public const string CompanyType = "企业单位";
		public const string Industry = "装卸搬运和运输代理业";
		public const string DevelopmentCompany = "慧咨 (上海) 信息技术有限公司";
		public const string Version = "CW1";
		public ZString BaseCurrency { get { return GlbCompany.CurrentCompany.LocalCurrency.RX_DescMultilingual.ToString(Constants.Languages.ChineseSimplified); } }
		public ZString FinancialYear { get; set; }
		public const string ChinaStandardVersion = "GB/T 24589.1-2010";
	}
}

