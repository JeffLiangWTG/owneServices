using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5ULHeader : IMessageDataProvider
	{
		ZString RefundDeclarationNumber { get; }
		ZString RefundType { get; }
		ZString RefundCauseCode { get; }
		ZString RefundReasonCode { get; }
		ZBool Are5FE_5ULToBeSentTogether { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		ZString BankAccountNumber { get; }
		ZString BankCode { get; }
		IOrganization Payer { get; }
		ZDecimal TotalRefundAmount { get; }
		ZString TaxOfficeCode { get; }
		IEnumerable<IImport5ULEntryLine> EntryLines { get; }
	}
}
