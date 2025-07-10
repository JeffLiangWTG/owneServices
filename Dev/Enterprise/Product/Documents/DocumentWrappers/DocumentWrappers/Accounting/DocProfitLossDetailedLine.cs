using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.DocumentWrappers
{
	public class DocProfitLossDetailedLine : DocBaseWrapper
	{
		protected DocProfitLossDetailedLine(ProfitLossDetailView line, BusinessObjectFactory factory)
			: base(line, factory)
		{
		}

		public static DocProfitLossDetailedLine New(ProfitLossDetailView line, BusinessObjectFactory factory)
		{
			return new DocProfitLossDetailedLine(line, factory);
		}

		public ZString Branch
		{
			get { return (Line.Branch != null) ? Line.Branch.GB_Code : ZString.Empty; }
		}

		public ZString Department
		{
			get { return (Line.Department != null) ? Line.Department.GE_Code : ZString.Empty; }
		}

		public ZString Organisation
		{
			get { return (Line.Organisation != null) ? Line.Organisation.OH_Code : ZString.Empty; }
		}

		public ZDecimal LineAmount
		{
			get { return new ZDecimal(Line.ZY_Calc_LineAmount); }
		}

		public ZString LineType
		{
			get { return Line.ZY_Calc_LineType; }
		}

		public ZDecimal Income => Line.ZY_Calc_ARLine.IsValid ? LineAmount : 0;

		public ZDecimal Expense => Line.ZY_Calc_APLine.IsValid ? -LineAmount : 0;

		public ZDecimal Profit
		{
			get { return Income - Expense; }
		}

		public DocChargeCode ChargeCode
		{
			get { return DocChargeCode.New(Line.ChargeCode, Factory); }
		}

		public DocJobInvoicingJob Job
		{
			get { return DocJobInvoicingJob.New(Factory, Line.Job.PK); }
		}

		ProfitLossDetailView Line
		{
			get { return (ProfitLossDetailView)WrappedObject; }
		}
	}
}
