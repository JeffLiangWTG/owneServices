using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.CA.Business;

[TestExcludeBusinessObjectsAllHaveTestCases]
public class K84AmountsWithPenaltyWrapper : K84AmountsWrapper
{
	internal K84AmountsWithPenaltyWrapper(MOASegmentMessageSection moaSection)
		: base(moaSection)
	{
	}

	[ColumnName(6)]
	public ZDecimal LateFilingPenalty
	{
		get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.PenaltyAmount); }
	}

	[ColumnName(7, "Total")]
	public ZDecimal TotalAmount
	{
		get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.TotalAmount); }
	}

	internal override object[] ToArray()
	{
		return base.ToArray().Concat(new object[] { LateFilingPenalty, TotalAmount }).ToArray();
	}
}
