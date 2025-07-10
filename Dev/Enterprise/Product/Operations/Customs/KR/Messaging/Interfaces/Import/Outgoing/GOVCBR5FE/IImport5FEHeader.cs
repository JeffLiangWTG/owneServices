using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5FEHeader : IMessageDataProvider, IImport5UASessionDetails
	{
		ZString ImportDeclarationNumber { get; }
		ZDate DeclarationDate { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		ZString DeclarantType { get; }
		IOrganization Declarant { get; }
		IOrganization Payer { get; }
		ZInt TotalAmendedItemCount { get; }
		ZInt TotalAmendedTaxCount { get; }
		ZString DomesticTaxPenaltyType { get; }
		ZString DutyPenaltyReducedYN { get; }
		ZString RefundRequestNumber { get; }
		IEnumerable<IImport5FEItem> AmendedItems { get; }
		IEnumerable<IImport5FETaxItem> TaxItems { get; }
		ZDecimal BeforeTotalDutyTaxAmount { get; }
		ZDecimal AfterTotalDutyTaxAmount { get; }
		ZDecimal DutyTaxDifference { get; }
		ZDecimal BeforeCustomsValue { get; }
		ZDecimal AfterCustomsValue { get; }
		ZDecimal CustomsValueDifference { get; }
		ZString DutyPenaltyType { get; }
	}
}
