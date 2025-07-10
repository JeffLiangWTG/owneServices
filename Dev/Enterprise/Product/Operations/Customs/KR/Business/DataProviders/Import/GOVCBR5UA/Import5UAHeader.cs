using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5UAHeader : IImport5UAHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public int SequenceNo { get; set; }
		public string PenaltyType { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public Organisation Declarant { get; set; }
		public string ExemptionProcessCode { get; set; }
		public DateTime AmendmentDeclarationDate { get; set; }
		public int AmendmentVersionNo { get; set; }
		public string PenaltyExemptionReasonsCode { get; set; }
		public string PenaltyExemptionReason { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public decimal PenaltyExemptionAmount { get; set; }

		ZString IImport5UAHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZInt IImport5UAHeader.SequenceNo => SequenceNo;
		ZString IImport5UAHeader.PenaltyType => PenaltyType;
		ZString IImport5UAHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport5UAHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IOrganization IImport5UAHeader.Declarant => Declarant;
		ZString IImport5UAHeader.ExemptionProcessCode => ExemptionProcessCode;
		ZDate IImport5UAHeader.AmendmentDeclarationDate => new ZDate(AmendmentDeclarationDate);
		ZInt IImport5UAHeader.AmendmentVersionNo => AmendmentVersionNo;
		ZString IImport5UAHeader.PenaltyExemptionReasonsCode => PenaltyExemptionReasonsCode;
		ZString IImport5UAHeader.PenaltyExemptionReason => PenaltyExemptionReason;
		ZDecimal IImport5UAHeader.PenaltyExemptionAmount => PenaltyExemptionAmount;
	}
}
