using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.CA.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class K84AmountsWrapper : NonPersistentBusinessObject
	{
		internal K84AmountsWrapper(MOASegmentMessageSection moaSection)
		{
			this.moaSection = moaSection;
		}

		[ColumnName(1)]
		public ZDecimal CustomsDuty
		{
			get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.StandardDuty); }
		}

		[ColumnName(2)]
		public ZDecimal SIMAAssessment
		{
			get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.OtherValuationChargesCustoms); }
		}

		[ColumnName(3)]
		public ZDecimal ExciseTax
		{
			get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.AdditionalRoyaltiesCustoms); }
		}

		[ColumnName(4)]
		public ZDecimal GST
		{
			get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.Vat1stValue); }
		}

		[ColumnName(5)]
		public ZDecimal TotalDutiesAndTaxes
		{
			get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount); }
		}

		internal virtual object[] ToArray()
		{
			return new object[] { CustomsDuty, SIMAAssessment, ExciseTax, GST, TotalDutiesAndTaxes, };
		}

		protected MOASegmentMessageSection moaSection;
	}
}
