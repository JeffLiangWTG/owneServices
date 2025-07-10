using System.Collections.Immutable;

namespace CargoWise.Bi.Development.Common.SQL;

#pragma warning disable CW1161 // Res.GetString Analyzer

public static class SQLConstants
{
	public static readonly short MaxColumnLength = 4096;

	public static readonly ImmutableArray<string> ExcludedTables = ImmutableArray.Create(
		"AccCashBasisVATQueue",
		"AccOrgBalance",
		"AccOrgBalanceChanges",
		"EDIMessageAttach",
		"JobToCloseQueue",
		"MailDBAttachments",
		"MailDBItems",
		"MailDBRecipients",
		"MENTAgedScoreMetric",
		"StmALogQueue",
		"StmALogQueueWTE",
		"StmChangeLog",
		"StmEntityScreeningLog",
		"StmJobQueue",
		"StmLoginFailureLog",
		"StmPrintQueue",
		"StmProcessQueue",
		"StmQueueState",
		"StmUsageQueue"
	);

	public static readonly ImmutableArray<string> ExcludedTypes = ImmutableArray.Create(
		"sysname",
		"binary",
		"image",
		"text",
		"ntext",
		"cursor",
		"hierarchyid",
		"sqlvariant",
		"timestamp"
	);
}

#pragma warning restore CW1161 // Res.GetString Analyzer
