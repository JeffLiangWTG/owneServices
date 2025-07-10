using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	static class DEMonthlyClosingEntryLineSnapshotBuilderHelper
	{
		internal static decimal GetNetMassMeasure(this IMonthlyClosingEntryLineSnapshot snapshot) => MessageBuilderHelper.FormatDecimal(snapshot.NetMassMeasure, 1);

		internal static decimal GetForeignTradeStatisticsGrossMassMeasure(this IMonthlyClosingEntryLineSnapshot snapshot) => MessageBuilderHelper.FormatDecimal(snapshot.ForeignTradeStatisticsGrossMassMeasure, 1);

		internal static decimal GetAssessmentCustomsValue(this IMonthlyClosingEntryLineSnapshot snapshot) => MessageBuilderHelper.FormatDecimal(snapshot.AssessmentCustomsValue, 2);

		internal static decimal GetImportSpecificRateValue(this IImportSpecificRate importSpecificRate) => importSpecificRate.Value.RoundAndNormalize(2);

		internal static decimal GetContentInformationDegreePercentage(this IContentInformation contentInformation) => contentInformation.DegreePercentage.Round(2).Normalize();

		internal static decimal GetExciseDutyDegreePercentage(this IExciseDuty exciseDuty) => exciseDuty.DegreePercentage.RoundAndNormalize(2);

		internal static decimal GetExciseDutyValue(this IExciseDuty exciseDuty) => exciseDuty.Value.RoundAndNormalize(2);

		internal static DateTime? GetImportLineDocumentIssuingDate(this IImportLineDocument importLineDocument) => importLineDocument.IssuingDate;
	}
}
