using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5UAHeader : IMessageDataProvider
	{
		ZString ImportDeclarationNumber { get; }
		ZInt SequenceNo { get; }
		ZString PenaltyType { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IOrganization Declarant { get; }
		ZString ExemptionProcessCode { get; }
		ZDate AmendmentDeclarationDate { get; }
		ZInt AmendmentVersionNo { get; }
		ZString PenaltyExemptionReasonsCode { get; }
		ZString PenaltyExemptionReason { get; }
		ZDecimal PenaltyExemptionAmount { get; }
	}
}
