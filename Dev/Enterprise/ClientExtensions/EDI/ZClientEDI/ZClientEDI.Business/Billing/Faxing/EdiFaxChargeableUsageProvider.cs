using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Integration;
using ZClientEDI.Business.Billing;

namespace Enterprise.Client.EDI.Billing.Fax
{
	public class EdiFaxChargeableUsageProvider : ExternalChargeableUsageProvider
	{
		public override void BulkCopyUsages(BillingPeriod billingPeriod, ILogger logger)
		{
			Db.Connection.ExecuteNonQuery($"DELETE dbo.EdiExternalChargeableUsage WHERE EXU_Period = {billingPeriod.Period} AND EXU_Code = 'FAX';");
			base.BulkCopyUsages(billingPeriod, logger);
		}

		protected override DbConnection GetNewConnection()
		{
			return Db.NewExtraConnection(EDIDataRegistry.Instance.EDIFaxUsageDBServerName.Value,
				EDIDataRegistry.Instance.EDIFaxUsageDBName.Value,
				EDIDataRegistry.Instance.EDIFaxUsageDBLogin.Value.UserName,
				EDIDataRegistry.Instance.EDIFaxUsageDBLogin.Value.Password);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "raw usages in remote database")]
		protected override DataTable GetChargeableUsages(DbConnection externalConnection, BillingPeriod billingPeriod, ILogger logger)
		{
			var sql = @"
	SELECT
		EXU_PK = NEWID(),
		EXU_Period = @Period,
		EXU_Code = 'FAX',
		EXU_EnterpriseCode = EnterpriseCode, 
		EXU_CompanyCode = CompanyCode,
		EXU_ServerCode = ServerCode,
		EXU_UnitCount = sum (case 
			when AckSuccess = 1 THEN PageCount 
			when Status = 0 AND Price > 0 THEN Pages 
			ELSE 0 END)
	FROM dbo.FaxJobs
		INNER JOIN dbo.FaxRecipients ON FaxJobs.FaxJobId = FaxRecipients.FaxJobId
		LEFT JOIN dbo.FaxBilling ON FaxRecipients.FaxRecipientID = FaxBilling.ChargeCode
		WHERE ReceivedDateTime >= @PeriodStartTimeUtc and ReceivedDateTime < @PeriodEndTimeUtc
		AND IsAcknowledged = 1
		AND (AckSuccess = 1 OR Price > 0)
	GROUP BY EnterpriseCode, CompanyCode, ServerCode;
";
			using (var cmd = externalConnection.Command(sql))
			{
				cmd.AddParameter("@Period", SqlDbType.Int, (int)billingPeriod.Period);
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, billingPeriod.StartTimeUtc.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, billingPeriod.EndTimeUtc.ToDateTime());
				var faxUsages = DataUtils.GetDataTableFromCommand(cmd);
				faxUsages.TableName = "EdiExternalChargeableUsage";
				return faxUsages;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "raw usages in remote database")]
		protected override DbCommand GetRawUsageCommand(DbConnection connection, BillingLoadRawUsageContext context)
		{
			var sql = @"
DECLARE @LCC_XmlData XML
SET @LCC_XmlData = @LCC_XmlDataInput;

SELECT 
	pagecount as [NumberOfPages],
	FaxRecipients.faxnumber as [FaxNumber],
	receiveddatetime as [ReceivedDateTime],
	CompanyCode
FROM dbo.FaxJobs
INNER JOIN dbo.FaxRecipients on FaxJobs.FaxJobId = FaxRecipients.FaxJobId
LEFT JOIN dbo.FaxBilling on FaxRecipients.FaxRecipientID = FaxBilling.ChargeCode
OUTER APPLY
(
	SELECT TOP 1 LCC_PK FROM
	(
		SELECT
		R.value('(PK)[1]', 'UNIQUEIDENTIFIER') AS LCC_PK,
		R.value('(Code)[1]', 'CHAR(3)') AS LCC_Code,
		R.value('(ValidFromUtc)[1]', 'SMALLDATETIME') AS LCC_CodeValidFromUtc
		FROM @LCC_XmlData.nodes('/LCC') AS LCC(R)
	) LCC
	WHERE @ClientCompanyPk IS NOT NULL
		AND LCC_Code = CompanyCode
	ORDER BY (CASE WHEN LCC_CodeValidFromUtc <= DATEADD(DAY, 15, @DateFrom) THEN 0 ELSE 1 END), LCC_CodeValidFromUtc DESC
) a
WHERE 
	ReceivedDateTime >= @DateFrom and ReceivedDateTime < @DateTo
	AND (acksuccess = 1 or (acksuccess = 0 and Price > 0))
	AND EnterpriseCode = @EnterpriseCode
	AND ServerCode = @DatabaseCode
	AND (@ClientCompanyPk is null or @ClientCompanyPk = LCC_PK)
ORDER BY sentdatetime
";
			var lccXmlDataInput = "";
			if (!context.ClientCompanyPK.IsEmpty)
			{
				var companyCodeHistoryQuery =
@"
SELECT
(
	SELECT
	LCC_PK AS 'LCC/PK',
	LCC_Code AS 'LCC/Code',
	LCC_CodeValidFromUtc AS 'LCC/ValidFromUtc'
    FROM dbo.EdiViewClientCompanyCodeHistory LCC
    WHERE LCC_LD = @LD
    FOR XML PATH('')
) AS ResultXmlString
";

				using (var companyCodeHistoryCmd = Db.Connection.Command(companyCodeHistoryQuery))
				{
					companyCodeHistoryCmd.AddParameter("@LD", SqlDbType.UniqueIdentifier, context.DatabasePK.ToGuid());
					lccXmlDataInput = companyCodeHistoryCmd.ExecuteScalar()?.ToString() ?? "";
				}
			}

			var cmd = connection.Command(sql);
			cmd.AddParameter("@EnterpriseCode", SqlDbType.VarChar, context.EnterpriseCode.ToString());
			cmd.AddParameter("@DatabaseCode", SqlDbType.VarChar, context.ServerCode.ToString());
			cmd.AddParameter("@DateFrom", SqlDbType.SmallDateTime, context.PeriodStartTimeUtc.ToDateTime());
			cmd.AddParameter("@DateTo", SqlDbType.SmallDateTime, context.PeriodEndTimeUtc.ToDateTime());
			cmd.AddParameter("@ClientCompanyPk", SqlDbType.UniqueIdentifier, context.ClientCompanyPK.IsEmpty ? DBNull.Value : context.ClientCompanyPK.ToGuid());
			cmd.AddParameter("@LCC_XmlDataInput", SqlDbType.Xml, lccXmlDataInput);

			return cmd;
		}
	}
}
