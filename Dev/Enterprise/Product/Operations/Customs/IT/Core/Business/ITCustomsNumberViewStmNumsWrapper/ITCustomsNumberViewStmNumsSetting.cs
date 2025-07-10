using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class ITCustomsNumberViewStmNumsSetting : CustomsNumberViewStmNumsSetting
{
	public ITCustomsNumberViewStmNumsSetting(GlbCompany company, ZString rangeType)
		: base(company, rangeType)
	{
	}

	protected override int RequiredDigitCore()
	{
		var result = 6;
		switch (RangeType)
		{
			case NumberRangeTypeList.Codes.EntrySummaryDeclaration:
			case NumberRangeTypeList.Codes.EntrySummaryDeclarationAmendment:
			case NumberRangeTypeList.Codes.EntrySummaryDeclarationDiversion:
			case NumberRangeTypeList.Codes.ExitSummaryDeclaration:
			case NumberRangeTypeList.Codes.ExitSummaryDeclarationAmendment:
				result = 8;
				break;
		}
		return result;
	}

	protected override ZLong? DefaultTypeRangeMaxCore()
	{
		ZLong result = 999999L;
		switch (RangeType)
		{
			case NumberRangeTypeList.Codes.EntrySummaryDeclaration:
			case NumberRangeTypeList.Codes.EntrySummaryDeclarationAmendment:
			case NumberRangeTypeList.Codes.EntrySummaryDeclarationDiversion:
			case NumberRangeTypeList.Codes.ExitSummaryDeclaration:
			case NumberRangeTypeList.Codes.ExitSummaryDeclarationAmendment:
				result = 99999999L;
				break;
		}
		return result;
	}

	protected override ZLong GetThresholdRunOutWarningCore()
	{
		return 100L;
	}
}
