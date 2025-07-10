using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Integration;
using ZClientEDI.Business.Billing;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class EdiRemoteDevicesChargeableUsageProvider : ExternalChargeableUsageProvider
	{
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
SELECT DISTINCT 
	PrinterName = CASE 
						WHEN @PriceItemCode = '#HS' THEN '' 
						ELSE 
							CASE 
								WHEN SQ_DisplayName != '' THEN SQ_DisplayName
								ELSE SQ_QueueName
							END
					END,
	PrintServerName = CASE 
							WHEN @PriceItemCode = '#HR' THEN ''
							ELSE SQ_ServerName 
						END,
	DateCaptured
FROM dbo.WiseGridPrinters_202206
WHERE 
	Period = @Period AND DBName = @DBName
ORDER BY
	PrintServerName
";
			var cmd = connection.Command(sql);
			cmd.AddParameter("@Period", SqlDbType.Int, context.PeriodAsInt);
			cmd.AddParameter("@PriceItemCode", SqlDbType.VarChar, context.PriceItemCode.ToString());
			cmd.AddParameter("@DBName", SqlDbType.VarChar, context.HostDatabaseName.ToString());

			return cmd;
		}
	}
}
