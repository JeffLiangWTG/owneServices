using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.DocumentWrappers
{
	public class DocProfitLossSummaryLine : DocBaseWrapper
	{
		protected DocProfitLossSummaryLine(ProfitLossSummaryDetailView line, BusinessObjectFactory factory)
			: base(line, factory)
		{
		}

		public static DocProfitLossSummaryLine New(ProfitLossSummaryDetailView line, BusinessObjectFactory factory)
		{
			return new DocProfitLossSummaryLine(line, factory);
		}

		public ZString Branch
		{
			get { return (Line.Branch != null) ? Line.Branch.GB_Code : ZString.Empty; }
		}

		public ZString Department
		{
			get { return (Line.Department != null) ? Line.Department.GE_Code : ZString.Empty; }
		}

		public ZDecimal Revenue
		{
			get { return new ZDecimal(Line.ZZ_Calc_Revenue); }
		}

		public ZDecimal WIP
		{
			get { return new ZDecimal(Line.ZZ_Calc_WIP); }
		}

		public ZDecimal Cost
		{
			get
			{
				return new ZDecimal(Line.ZZ_Calc_Cost * -1);
			}
		}

		public ZDecimal Accrual
		{
			get { return new ZDecimal(Line.ZZ_Calc_Accrual * -1); }
		}

		public ZDecimal Income
		{
			get { return Revenue + WIP; }
		}

		public ZDecimal Expense
		{
			get { return -(Cost + Accrual); }
		}

		public ZDecimal Profit
		{
			get { return Income + Expense; }
		}

		public DocChargeCode ChargeCode
		{
			get { return DocChargeCode.New(Line.ChargeCode, Factory); }
		}

		public DocJobInvoicingJob Job
		{
			get { return DocJobInvoicingJob.New(Factory, Line.Job.PK); }
		}

		ProfitLossSummaryDetailView Line
		{
			get { return (ProfitLossSummaryDetailView)WrappedObject; }
		}
	}
}
