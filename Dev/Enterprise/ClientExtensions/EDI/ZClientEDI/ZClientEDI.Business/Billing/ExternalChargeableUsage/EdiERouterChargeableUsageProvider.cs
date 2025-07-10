using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Integration;
using ZClientEDI.Business.Billing;

namespace Enterprise.Client.EDI.Billing.ERouter
{
	public class EdiERouterChargeableUsageProvider : ExternalChargeableUsageProvider
	{
		public EdiERouterChargeableUsageProvider(string applicationCode)
		{
			ApplicationCode = applicationCode;
		}

		readonly string ApplicationCode;

		protected override DataTable GetChargeableUsages(DbConnection externalConnection, BillingPeriod billingPeriod, ILogger logger) => throw new NotImplementedException();
		public override void BulkCopyUsages(BillingPeriod billingPeriod, ILogger logger) => throw new NotImplementedException();

		protected override DbConnection GetNewConnection()
		{
			return Db.NewExtraConnection(EDIDataRegistry.Instance.EDIERouterUsageDBServerName.Value,
				EDIDataRegistry.Instance.EDIERouterUsageDBName.Value,
				EDIDataRegistry.Instance.EDIERouterUsageDBLogin.Value.UserName,
				EDIDataRegistry.Instance.EDIERouterUsageDBLogin.Value.Password);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "raw usages in remote database")]
		protected override DbCommand GetRawUsageCommand(DbConnection connection, BillingLoadRawUsageContext context)
		{
			var sql = @"
DECLARE @LCC_XmlData XML
SET @LCC_XmlData = @LCC_XmlDataInput;

SELECT
	U2_Type,
	U2_Sender,
	U2_Reference,
	U2_Date,
	CompanyCode = ISNULL(LCC_Code, '')
FROM dbo.eRouter_ChargeableMessage_20240411
JOIN
(
	SELECT
	R.value('(PK)[1]', 'UNIQUEIDENTIFIER') AS LCC_PK,
	R.value('(Code)[1]', 'CHAR(3)') AS LCC_Code
	FROM @LCC_XmlData.nodes('/LCC') AS LCC(R)
) LCC ON U2_LCC = LCC_PK
WHERE
	U2_ApplicationCode = @ApplicationCode
	AND U2_Date >= @DateFrom
	AND U2_Date < @DateTo
ORDER BY U2_Date
";
			var lccXmlDataInput = "";
			var lccQuery =
@"
SELECT
(
	SELECT
	LCC_PK AS 'LCC/PK',
	LCC_Code AS 'LCC/Code'
    FROM dbo.ClientCompany LCC
    WHERE LCC_PK = @ClientCompanyPk OR LCC_LD = @DatabasePk
    FOR XML PATH('')
) AS ResultXmlString
";

			using (var lccCmd = Db.Connection.Command(lccQuery))
			{
				lccCmd.AddParameter("@ClientCompanyPk", SqlDbType.UniqueIdentifier, context.ClientCompanyPK.IsEmpty ? DBNull.Value : context.ClientCompanyPK.ToGuid());
				lccCmd.AddParameter("@DatabasePk", SqlDbType.UniqueIdentifier, context.DatabasePK.IsEmpty ? DBNull.Value : context.DatabasePK.ToGuid());
				lccXmlDataInput = lccCmd.ExecuteScalar()?.ToString() ?? "";
			}

			var cmd = connection.Command(sql);
			cmd.AddParameter("@DateFrom", SqlDbType.SmallDateTime, context.PeriodStartTimeUtc.ToDateTime());
			cmd.AddParameter("@DateTo", SqlDbType.SmallDateTime, context.PeriodEndTimeUtc.ToDateTime());
			cmd.AddParameter("@LCC_XmlDataInput", SqlDbType.Xml, lccXmlDataInput);
			cmd.AddParameter("@ApplicationCode", SqlDbType.VarChar, ApplicationCode);
			return cmd;
		}
	}
}
