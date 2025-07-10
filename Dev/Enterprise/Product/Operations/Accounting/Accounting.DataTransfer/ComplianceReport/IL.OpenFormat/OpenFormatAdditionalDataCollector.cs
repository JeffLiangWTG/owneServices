using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.IL.OpenFormat
{
	internal class OpenFormatAdditionalDataCollector : ComplianceReportAdditionalDataCollectorBase<TransactionHeaderDetailsOFS>
	{
		public OpenFormatAdditionalDataCollector(AccComplianceReport report) : base(report, ComplianceReportDataCollectionMode.OpenFormatSimplified)
		{
			LineDetails = GetTransactionLineDetails(Report);
			CountryCodesOfEuropeanUnion = GetCountryCodesOfEuropeanUnion(Report);
			OrgHeaderDetails = GetOrgHeaderDetails(Report);
		}

		protected override bool CollectsTaxRegistrationNumbers => false;

		internal protected override Dictionary<ZGuid, TransactionHeaderDetailsOFS> GetTransactionHeaderDetails(AccComplianceReport report)
		{
			var result = new Dictionary<ZGuid, TransactionHeaderDetailsOFS>();
			var sql = FormattableString.Invariant($@"
		WITH
		HeaderSequences
		AS
		(
			SELECT DISTINCT AH_PK = AL_AH
			FROM dbo.AccComplianceReportTransactionPivot
				JOIN dbo.AccTransactionLines ON AL_PK = ACL_ParentID
			WHERE ACL_ACR_Report = @ReportPK AND ACL_ParentTableCode = 'AL' AND AL_AH IS NOT NULL
		)

		SELECT header.AH_PK, OrgVatNumber.OK_CustomsRegNo, AH_InvoiceDate, AH_PostDate, branch.GB_Code,
			AH_Ledger, AH_TransactionType, AH_TransactionCategory, AH_Desc, AH_IsCancelled, AH_DueDate,
			AH_OSTotal, AH_InvoiceAmount, AH_GSTAmount, AH_OutstandingAmount, AH_RX_NKTransactionCurrency,
			AH_SystemCreateUser CreateUserCode, createUser.GS_FullName CreateUserName,
			DATEADD(MINUTE, ISNULL(createTimeOffset.Offset, 0), header.AH_SystemCreateTimeUtc) AS CreateTime
		FROM HeaderSequences
			JOIN dbo.AccTransactionHeader header ON header.AH_PK = HeaderSequences.AH_PK
			LEFT JOIN dbo.GlbStaff createUser ON createUser.GS_Code = header.AH_SystemCreateUser
			LEFT JOIN dbo.GlbBranch branch ON branch.GB_PK = header.AH_GB
			OUTER APPLY dbo.GetTimeZoneOffsetInMinutes(branch.GB_RL_NKHomePort, header.AH_SystemCreateTimeUtc) createTimeOffset
			OUTER APPLY dbo.csfn_GetUniqueRegistrationNumberByCodeType(AH_OH, '{OrgCusCode.CodeTypes.VATCode}', '{CountryCodes.Israel}') OrgVatNumber
");

			var details = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			details.Load(sql, new ZSqlParameter[] { ZSqlParameter.New("@ReportPK", report.PK, AccComplianceReportSchema.PK) });

			foreach (var row in details)
			{
				result.Add((ZGuid)row["AH_PK"], new TransactionHeaderDetailsOFS()
				{
					Description = (ZString)row[AccTransactionHeaderSchema.Constants.AH_Desc],
					CreateUserCode = (ZString)row["CreateUserCode"],
					CreateUserName = (ZString)row["CreateUserName"],
					CreateTime = (ZDateTime)row["CreateTime"],
					Ledger = (ZString)row[AccTransactionHeaderSchema.Constants.AH_Ledger],
					TransactionType = (ZString)row[AccTransactionHeaderSchema.Constants.AH_TransactionType],
					TransactionCategory = (ZString)row[AccTransactionHeaderSchema.Constants.AH_TransactionCategory],
					TransactionCurrency = (ZString)row[AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency],
					IsCancelled = (ZBool)row[AccTransactionHeaderSchema.Constants.AH_IsCancelled],
					VatNumber = (ZString)row[OrgCusCodeSchema.Constants.OK_CustomsRegNo],
					DueDate = (ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_DueDate],
					OSTotal = (ZDecimal)row[AccTransactionHeaderSchema.Constants.AH_OSTotal],
					InvoiceAmount = (ZDecimal)row[AccTransactionHeaderSchema.Constants.AH_InvoiceAmount],
					GSTAmount = (ZDecimal)row[AccTransactionHeaderSchema.Constants.AH_GSTAmount],
					OutstandingAmount = (ZDecimal)row[AccTransactionHeaderSchema.Constants.AH_OutstandingAmount],
					InvoiceDate = ((ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_InvoiceDate]),
					PostDate = ((ZDateTime)row[AccTransactionHeaderSchema.Constants.AH_PostDate]),
					BranchCode = (ZString)row[GlbBranchSchema.Constants.GB_Code],
				});
			}

			return result;
		}

		Dictionary<ZInt, TransactionLineDetailsOFS> GetTransactionLineDetails(AccComplianceReport report)
		{
			var result = new Dictionary<ZInt, TransactionLineDetailsOFS>();

			if (ShouldPopulateAdditionalHeaderDetails(report))
			{
				var sql = @"SELECT DISTINCT ACL_ReportSequence,
	AL_Sequence,
	AL_Desc,
	AL_LineAmount,
	AL_TaxRateNumerator,
	AL_TaxExtraRateDenominator,
	AC_Code,
	GB_Code
FROM dbo.AccComplianceReportTransactionPivot
JOIN dbo.AccTransactionLines ON AL_PK = ACL_ParentID
LEFT JOIN dbo.AccChargeCode ON AC_PK = AL_AC
LEFT JOIN dbo.GlbBranch ON GB_PK = AL_GB
WHERE ACL_ACR_Report = @ReportPK";

				var details = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
				details.Load(sql, new ZSqlParameter[] { ZSqlParameter.New("@ReportPK", report.PK, AccComplianceReportSchema.PK) });

				var lineNumOffSet = report.LineNumOffset;
				foreach (var row in details)
				{
					var reportSequence = (ZInt)row["ACL_ReportSequence"] + lineNumOffSet;
					if (!result.ContainsKey(reportSequence))
					{
						result.Add(reportSequence,
							new TransactionLineDetailsOFS()
							{
								LineSequence = (ZShort)row[AccTransactionLinesSchema.Constants.AL_Sequence],
								BranchCode = (ZString)row[GlbBranchSchema.Constants.GB_Code],
								ChargeCode = (ZString)row[AccChargeCodeSchema.Constants.AC_Code],
								Description = (ZString)row[AccTransactionLinesSchema.Constants.AL_Desc],
								LineAmount = (ZDecimal)row[AccTransactionLinesSchema.Constants.AL_LineAmount],
								TaxRateNumerator = (ZInt)row[AccTransactionLinesSchema.Constants.AL_TaxRateNumerator],
								TaxExtraRateDenominator = (ZInt)row[AccTransactionLinesSchema.Constants.AL_TaxExtraRateDenominator]
							});
					}
				}
			}

			return result;
		}

		internal TransactionLineDetailsOFS GetTransactionLineData(ZInt sequence)
		{
			return LineDetails?.GetValueSafe(sequence);
		}

		internal Dictionary<ZInt, TransactionLineDetailsOFS> LineDetails;
	}

	internal class TransactionHeaderDetailsOFS : IComplianceReportTransactionHeaderDetails
	{
		public ZInt HeaderSequence { get; set; }
		public ZString Description { get; set; }
		public ZString CreateUserCode { get; set; }
		public ZString CreateUserName { get; set; }
		public ZDateTime CreateTime { get; set; }
		public ZString Ledger { get; set; }
		public ZString TransactionType { get; set; }
		public ZString TransactionCategory { get; set; }
		public ZString TransactionCurrency { get; set; }
		public ZBool IsCancelled { get; set; }
		public ZString VatNumber { get; set; }
		public ZDateTime DueDate { get; set; }
		public ZDecimal OSTotal { get; set; }
		public ZDecimal InvoiceAmount { get; set; }
		public ZDecimal GSTAmount { get; set; }
		public ZDecimal OutstandingAmount { get; set; }
		public ZDateTime InvoiceDate { get; set; }
		public ZDateTime PostDate { get; set; }
		public ZString BranchCode { get; set; }
	}

	internal class TransactionLineDetailsOFS
	{
		public ZShort LineSequence { get; set; }
		public ZString BranchCode { get; set; }
		public ZString ChargeCode { get; set; }
		public ZString Description { get; set; }
		public ZDecimal LineAmount { get; set; }
		public ZInt TaxRateNumerator { get; set; }
		public ZInt TaxExtraRateDenominator { get; set; }
	}
}
