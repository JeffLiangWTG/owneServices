using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class PortMessagingBillingSystem : TransactionBillingSystem
	{
		public PortMessagingBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.PortMessaging; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new PortMessagingBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new PortMessagingUsage("", factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			return BuildOdplRawUsageFromDataReader(reader, context, UsageCaptions);
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			// STL usage handled elsewhere by registry config StlRawUsageReportRefCaption
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			// STL usage handled elsewhere by registry config StlRawUsageReportRefCaption
			BuilderRawUsageInCsv(context, isStlBilling, action, UsageCaptions);
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			BuildRawUsageInCsv(context, isStlBilling, writer, UsageCaptions);
		}

		public static List<StlRawUsageReportRefCaption> GetUsageCaptions()
		{
			var usageCaptions = EDIDataRegistry.Instance.StlRawUsageReportRefCaption.Value
				.Cast<StlRawUsageReportRefCaption>()
				.Where(x => x.Category == "PMG")
				.OrderBy(x => x.UsageDescription)
				.ToList();

			if (usageCaptions.Count == 0)
			{
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "PMG",
					UsageCode = "PM1",
					UsageDescription = "Port Order with HDS",
					Ref1Caption = "Consol",
					Ref2Caption = "Shipment",
					Ref4Caption = "Message Id",
					Ref5Caption = "Service Provider"
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "PMG",
					UsageCode = "PM2",
					UsageDescription = "Other Message",
					Ref1Caption = "Consol",
					Ref2Caption = "Shipment",
					Ref4Caption = "Message Id",
					Ref5Caption = "Service Provider"
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "PMG",
					UsageCode = "PM3",
					UsageDescription = "Status Message",
					Ref1Caption = "Job Number",
					Ref2Caption = "Z or B-Number",
					Ref4Caption = "Message Id",
					Ref5Caption = "Sender"
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "PMG",
					UsageCode = "PMN",
					UsageDescription = "Data Record / Original Entry",
					Ref1Caption = "Consol",
					Ref2Caption = "MRN",
					Ref3Caption = "Service Provider/Recipient",
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "PMG",
					UsageCode = "POZ",
					UsageDescription = "Data Record / Original Entry",
					Ref1Caption = "Consol",
					Ref2Caption = "Message Type",
					Ref3Caption = "Submission Type",
					Ref4Caption = "Message ID"
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "PMG",
					UsageCode = "PSN",
					UsageDescription = "Port Status",
					Ref1Caption = "Consol",
					Ref2Caption = "MRN",
					Ref3Caption = "Service Provider/Sender",
					Ref4Caption = "Status Code"
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "PMG",
					UsageCode = "PSZ",
					UsageDescription = "Port Status",
					Ref1Caption = "Consol",
					Ref2Caption = "Event Type",
					Ref3Caption = "Message Type",
					Ref4Caption = "Order",
					Ref5Caption = "Message ID"
				});
			}

			return usageCaptions;
		}

		List<StlRawUsageReportRefCaption> UsageCaptions => usageCaptions ?? (usageCaptions = GetUsageCaptions());
		List<StlRawUsageReportRefCaption> usageCaptions;

		protected override string Query_Raw_Usage
		{
			get
			{
				var priceCodes = UsageCaptions.Select(x => (string)x.UsageCode).ToList();
				string priceCodeSql = string.Join("', '", priceCodes);
				return string.Format(CultureInfo.InvariantCulture, sql_Raw_Usage, priceCodeSql);
			}
		}

		const string sql_Raw_Usage = @"
SELECT
	TX_PriceItemCode,
	TX_ClientId,
	CompanyCode = ISNULL(LCC_Code, ''),
	TX_Reference1,
	TX_Reference2 = ISNULL(TX_Reference2, ''),
	TX_Reference3 = ISNULL(TX_Reference3, ''),
	TX_Reference4 = ISNULL(TX_Reference4, ''),
	TX_Reference5 = ISNULL(TX_Reference5, ''),
	TX_ServiceOccuredUTC,
	ISNULL(TX_Branch, '') TX_Branch,
	TX_BillableCount,
	TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
FROM
	dbo.BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_Category = 'PMG'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
	AND TX_PriceItemCode IN ('{0}')
	AND (TX_PriceItemCode = @PriceItemCode or @PriceItemCode = '')
ORDER BY TX_PriceItemCode, TX_ServiceOccuredUTC
";

		#endregion

	}
}

