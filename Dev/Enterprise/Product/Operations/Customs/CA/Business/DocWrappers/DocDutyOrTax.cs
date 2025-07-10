namespace Enterprise.Customs.CA.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Common.CA;
	using Enterprise.DocumentWrappers;

	public class DocDutyOrTax : DocBaseWrapper
	{
		DocDutyOrTax(object objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		/// <summary>
		/// WARNING: This method for test only
		/// </summary>
		/// <returns>Empty wrapper</returns>
		public static DocDutyOrTax New(JobComInvoiceLine line, BusinessObjectFactory factoryForWrapper)
		{
			return New(factoryForWrapper, line, string.Empty);
		}

		public static DocDutyOrTax New(BusinessObjectFactory factoryForWrapper, JobComInvoiceLine line, string dutyOrTaxType, int? index = null)
		{
			return new DocDutyOrTax(line == null ? null : new DutyOrTaxDocumentWrapper(line, dutyOrTaxType, index), factoryForWrapper);
		}

		public ZString ExemptCodeOrRateFormatted
		{
			get { return DutyOrTax == null || !DutyOrTax.IsTax ? ZString.Empty : !DutyOrTax.ExemptCode.IsEmpty ? DutyOrTax.ExemptCode : RateFormatted; }
		}

		public ZString ExemptCode
		{
			get { return DutyOrTax != null ? DutyOrTax.ExemptCode : ZString.Empty; }
		}

		public ZDecimal Rate
		{
			get { return DutyOrTax != null ? DutyOrTax.Rate : ZDecimal.Zero; }
		}

		public ZString RateFormatted
		{
			get { return DutyOrTax != null && (!DutyOrTax.IsTax || DutyOrTax.Rate > 0) ? DutyOrTax.Rate.ToString(RateFormat) : string.Empty; }
		}

		public ZString RateType
		{
			get { return DutyOrTax != null ? DutyOrTax.RateType : ZString.Empty; }
		}

		public ZString UnitOfMeasure
		{
			get { return DutyOrTax != null ? DutyOrTax.UnitOfMeasure : ZString.Empty; }
		}

		public ZDecimal Amount
		{
			get { return DutyOrTax != null ? DutyOrTax.Amount : ZDecimal.Zero; }
		}

		DutyOrTaxDocumentWrapper DutyOrTax
		{
			get { return WrappedObject as DutyOrTaxDocumentWrapper; }
		}

		ZString RateFormat
		{
			get { return RateType == RateTypes.Codes.AdValorem ? "0.0####" : "0.00###"; }
		}
	}
}
