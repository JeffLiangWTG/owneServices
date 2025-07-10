using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class Report : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T208";
		public ZString ReportNumber { get; set; }
		public ZString ReportName { get; set; }
		public ZString ReportDate { get; set; }
		public ZString ReportPeriod { get; set; }
		public ZString ReportCompanyName { get { return LocalCompanyName.GetCurrentCompanyLocalName(); } }

		public ZString CurrencyUnit { get { return (NoResString)"元"; } }
	}

	public class ReportCollection : NonPersistentBusinessObjectCollection<Report>	{
		public ReportCollection(BusinessObjectFactory factory, ZInt period)
			: base(factory)
		{
			AddElements(period);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Report();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be a some code abbreviature.")]
		void AddElements(ZInt period)
		{
			Report bSH = AddNew();
			bSH.ReportNumber = "1";
			bSH.ReportName = (NoResString)"资产负债表";
			bSH.ReportDate = ZDateTime.Today.ToString("yyyyMMdd");
			bSH.ReportPeriod = period.ToString();

			Report pNL = AddNew();
			pNL.ReportNumber = "201";
			pNL.ReportName = "损益表";
			pNL.ReportDate = ZDateTime.Today.ToString("yyyyMMdd");
			pNL.ReportPeriod = period.ToString();

			Report cF = AddNew();
			cF.ReportNumber = "3";
			cF.ReportName = "现金流量表";
			cF.ReportDate = ZDateTime.Today.ToString("yyyyMMdd");
			cF.ReportPeriod = period.ToString();

			Report cOE = AddNew();
			cOE.ReportNumber = "7";
			cOE.ReportName = "所有者权益变动表";
			cOE.ReportDate = ZDateTime.Today.ToString("yyyyMMdd");
			cOE.ReportPeriod = period.ToString();
		}
	}
}

