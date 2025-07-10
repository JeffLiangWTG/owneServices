using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class DutiesandTaxesProvider : IDutiesAndTaxes
	{
		public DutiesandTaxesProvider(int sequenceNumber, CusEntryLineFee cusEntryLineFee)
		{
			this.cusEntryLineFee = Argument.NotNull(cusEntryLineFee, nameof(cusEntryLineFee));
			SequenceNumber = sequenceNumber.ToString();
		}

		readonly CusEntryLineFee cusEntryLineFee;

		public string SequenceNumber { get; }

		public string TaxType => cusEntryLineFee.CF_ChargeType;

		public string CcQualifier => null;

		public decimal PayableTaxAmount => cusEntryLineFee.CF_ChargeAmount;

		public string MethodOfPayment => cusEntryLineFee.CF_MethodOfPayment;

		public IReadOnlyCollection<ITaxBase> TaxBase => taxBase ?? (taxBase = GetTaxBase().ToArray());
		IReadOnlyCollection<ITaxBase> taxBase;

		IEnumerable<ITaxBase> GetTaxBase()
		{
			yield return new TaxBaseProvider(1, cusEntryLineFee);
		}
	}
}
