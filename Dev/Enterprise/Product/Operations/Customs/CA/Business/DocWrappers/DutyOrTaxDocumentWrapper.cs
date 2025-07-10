namespace Enterprise.Customs.CA.Business
{
	using CargoWise.Types;

	public class DutyOrTaxDocumentWrapper
	{
		public DutyOrTaxDocumentWrapper(JobComInvoiceLine line, string dutyOrTaxType, int? index = null)
		{
			manager = line.DutyAndTaxManager;
			this.dutyOrTaxType = dutyOrTaxType;
			this.index = index;
		}

		public bool IsTax
		{
			get { return dutyOrTaxType == DutyAndTaxTypes.Codes.ExciseTax || dutyOrTaxType == DutyAndTaxTypes.Codes.GST; }
		}

		public ZString ExemptCode
		{
			get { return manager.GetExemptCode(dutyOrTaxType, index); }
		}

		public ZDecimal Rate
		{
			get { return manager.GetRate(dutyOrTaxType, index); }
		}

		public ZString RateType
		{
			get { return manager.GetRateType(dutyOrTaxType, index); }
		}

		public ZString UnitOfMeasure
		{
			get { return manager.GetUnitOfMeasure(dutyOrTaxType, index); }
		}

		public ZDecimal Amount
		{
			get { return manager.GetAmount(dutyOrTaxType, index); }
		}

		readonly DutyAndTaxManager manager;
		readonly string dutyOrTaxType;
		readonly int? index;
	}
}

//Tested in Enterprise.Customs.CA.Business.DocDutyOrTax
