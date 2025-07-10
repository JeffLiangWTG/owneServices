using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5WNMessageData
	{
		ZDate DeclarationDate { get; }
		ZString ImportDeclarationNumber { get; }
		ZDate AmendmentDeclarationDate { get; }
		ZInt AmendmentVersionNo { get; }
		ZDate NoticeDate { get; }
		ZString PayerCompanyName { get; }
		ZString PayerRepresentativeName { get; }
		ZString PayerAddressLine { get; }
		ZString CustomsDepartment { get; }
		ZString CustomsPersonName { get; }
		ZString CustomsPersonPhoneNumber { get; }
		ZString TaxAdjustmentReason { get; }
		ZString CustomsOffice { get; }
		ZString NoticeNumber { get; }
		ZDecimal DutyTaxDifference { get; }
		IReadOnlyDictionary<ZString, ZDecimal> DutyTax { get; }
		ZDate ProcessedDate { get; }
	}

	public class GOVCBR5WNMessageData : NonPersistentBusinessObject, IGOVCBR5WNMessageData
	{
		public GOVCBR5WNMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZDate DeclarationDate { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZDate AmendmentDeclarationDate { get; set; }
		public ZInt AmendmentVersionNo { get; set; }
		public ZString AmendmentSequenceNo { get; set; }
		public ZDate NoticeDate { get; set; }
		public ZString PayerCompanyName { get; set; }
		public ZString PayerRepresentativeName { get; set; }
		public ZString PayerAddressLine { get; set; }
		public ZString CustomsDepartment { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString CustomsPersonPhoneNumber { get; set; }
		public ZString CustomsPrimaryOfficial { get; set; }
		public ZString TaxAdjustmentReason { get; set; }
		public ZString CustomsOffice { get; set; }
		public ZString NoticeNumber { get; set; }
		public ZDecimal DutyTaxDifference { get; set; }
		public IReadOnlyDictionary<ZString, ZDecimal> DutyTax { get; set; }
		public ZDate ProcessedDate { get; set; }

		public ZDecimal DutyAmountDifference { get; set; }
		public ZDecimal SpecialConsumptionTaxDifference { get; set; }
		public ZDecimal TransportationTaxDifference { get; set; }
		public ZDecimal LiquorTaxDifference { get; set; }
		public ZDecimal EducationTaxDifference { get; set; }
		public ZDecimal AgriculturalTaxDifference { get; set; }
		public ZDecimal VATDifference { get; set; }
		public ZDecimal PenaltyOnDutyForUnderDeclarationDifference { get; set; }
		public ZDecimal PenaltyOnDomesticTaxForUnderDeclarationDifference { get; set; }
		public ZDecimal PenaltyOnDutyForLatePaymentDifference { get; set; }
		public ZDecimal PenaltyOnDomesticTaxForLatePaymentDifference { get; set; }
		public ZDecimal PenaltyOnMissedDeclarationDifference { get; set; }
		public ZDecimal PenaltyOnLateDeclarationDifference { get; set; }
		public ZDecimal PenaltyOnNonCompliantDeclarationDifference { get; set; }
		public ZDecimal PenaltyOnBreachOfReExportationDifference { get; set; }
		public ZDecimal PenaltyOnMissedDeclarationForPersonalItemsDifference { get; set; }
		public ZDecimal PenaltyOnOverdrawbackDifference { get; set; }
		public ZDecimal TotalDifferenceAmount { get; set; }

		public ZString FormattedNoticeNumber => MessageFunctions.GetFormattedNumber(NoticeNumber, new int[] { 0, 3, 5, 7 });

		public GOVCBR5WNLineMessageDataCollection Lines => lines ?? (lines = new GOVCBR5WNLineMessageDataCollection(Factory));
		GOVCBR5WNLineMessageDataCollection lines;
	}
}
