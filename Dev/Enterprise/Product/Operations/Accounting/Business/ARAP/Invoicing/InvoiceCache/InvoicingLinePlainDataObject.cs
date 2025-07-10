using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.Invoicing
{
	public sealed class InvoicingLinePlainDataObject
	{
		public InvoicingLinePlainDataObject(ZGuid pk, ZGuid chargeCodePK, ZGuid branchPK, ZGuid departmentPK, ZGuid jobPK, ZGuid relatedJobPK, ZString currrency, ZDecimal exchangeRate)
		{
			PK = pk;
			ChargeCodePK = chargeCodePK;
			BranchPK = branchPK;
			DepartmentPK = departmentPK;
			JobPK = jobPK;
			RelatedJobPK = relatedJobPK;
			Currrency = currrency;
			ExchangeRate = exchangeRate;
		}

		public static InvoicingLinePlainDataObject Create(InvoicingLineBase invoiceLine)
		{
			return new InvoicingLinePlainDataObject(invoiceLine.PK,
				invoiceLine.ChargeCode?.PK ?? ZGuid.Empty,
				invoiceLine.Branch.PK,
				invoiceLine.Department.PK,
				invoiceLine.Job?.PK ?? ZGuid.Empty,
				invoiceLine.AL_Calc_RelatedJobPK,
				invoiceLine.AL_RX_NKTransactionCurrency,
				invoiceLine.AL_ExchangeRate);
		}

		public ZGuid PK { get; private set; }
		public ZGuid ChargeCodePK { get; private set; }
		public ZGuid BranchPK { get; private set; }
		public ZGuid DepartmentPK { get; private set; }
		public ZGuid JobPK { get; private set; }
		public ZGuid RelatedJobPK { get; private set; }
		public ZString Currrency { get; private set; }
		public ZDecimal ExchangeRate { get; private set; }
		public bool IsHaveDifferentCurrencyOrExchangeRate { get; set; }
	}
}
