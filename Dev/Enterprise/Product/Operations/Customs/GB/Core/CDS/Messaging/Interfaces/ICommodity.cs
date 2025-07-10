using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface ICommodity
	{
		ZString Description { get; }
		IEnumerable<IClassification> Classifications { get; }
		ZString UNDGID { get; }
		IEnumerable<IDutyTaxFee> DutyTaxFees { get; }
		IAmountAndCurrency InvoiceLineItemCharge { get; }
		ZDecimal NetWeight { get; }
		ZDecimal GrossWeight { get; }
		ZDecimal TariffQuantity { get; }
		IEnumerable<ZString> TransportEquipmentIDs { get; }

		bool NoAdditionalProcedureCodesAreE01orE02 { get; }
	}
}
