using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TaxBaseProvider : ITaxBase
	{
		public TaxBaseProvider(int sequenceNumber, CusEntryLineFee cusEntryLineFee)
		{
			this.cusEntryLineFee = Argument.NotNull(cusEntryLineFee, nameof(cusEntryLineFee));
			SequenceNumber = sequenceNumber.ToString();
		}

		readonly CusEntryLineFee cusEntryLineFee;

		public string SequenceNumber { get; }

		public decimal TaxRate => cusEntryLineFee.CF_Rate;

		public string MeasurementUnitAndQualifier => cusEntryLineFee.CF_MethodOfCalculation;

		public decimal Amount => cusEntryLineFee.CF_MethodOfCalculation.IsEmpty || cusEntryLineFee.CF_MethodOfCalculation == Mathematics.Percentage ? cusEntryLineFee.CF_BaseValue : 0;

		public decimal Quantity => !cusEntryLineFee.CF_MethodOfCalculation.IsEmpty && cusEntryLineFee.CF_MethodOfCalculation != Mathematics.Percentage ? cusEntryLineFee.CF_BaseValue : 0;

		public decimal TaxAmount => cusEntryLineFee.CF_ChargeAmount;
	}
}
