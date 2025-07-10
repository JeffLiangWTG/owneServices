using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing
{
	public class OceanCarrierMessagingBillingSystem : TransactionBillingSystem
	{
		public OceanCarrierMessagingBillingSystem() : base()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.OceanCarrierMessaging; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new OceanCarrierMessagingBill(Context.Factory);
		}

		protected override PriceItemUsage CreateTransactionalSystemUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
		{
			return new OceanCarrierMessagingUsage("", factory, user, periodStart);
		}

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			return BuildOdplRawUsageFromDataReader(reader, context, UsageCaptions);
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			// STL usage handled elsewhere by registry config StlRawUsageReportRefCaption
			return null;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			// STL usage handled elsewhere by registry config StlRawUsageReportRefCaption
			BuilderRawUsageInCsv(context, isStlBilling, action, UsageCaptions);
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			BuildRawUsageInCsv(context, isStlBilling, writer, UsageCaptions);
		}

		public static CodeDescriptionPairList GetCachedOceanCarrierMessageTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("OceanCarrierMessageTypes", () =>
			{
				var result = new CodeDescriptionPairList();
				foreach (var caption in GetUsageCaptions())
				{
					result.AddPairIfNotExist(caption.UsageCode, caption.UsageDescription);
				}
				return result;
			});
		}

		public static List<StlRawUsageReportRefCaption> GetUsageCaptions()
		{
			var usageCaptions = EDIDataRegistry.Instance.StlRawUsageReportRefCaption.Value
				.Cast<StlRawUsageReportRefCaption>()
				.Where(x => x.Category == "SHI")
				.ToList();

			if (usageCaptions.Count == 0)
			{
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "SHI",
					UsageCode = "SHI",
					UsageDescription = "Shipping Instruction Sent",
					Ref1Caption = "Message ID",
					Ref2Caption = "Provider",
					Ref3Caption = "Consol",
					Ref4Caption = "Carrier Code",
					Ref5Caption = null
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "SHI",
					UsageCode = "BRT",
					UsageDescription = "Booking Request Sent",
					Ref1Caption = "Message ID",
					Ref2Caption = "Provider",
					Ref3Caption = "Consol",
					Ref4Caption = "Carrier Code",
					Ref5Caption = null
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "SHI",
					UsageCode = "VGM",
					UsageDescription = "VGM Sent",
					Ref1Caption = "Message ID",
					Ref2Caption = "Provider",
					Ref3Caption = "Consol",
					Ref4Caption = "Carrier Code",
					Ref5Caption = null
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "SHI",
					UsageCode = "SHR",
					UsageDescription = "Shipping Instruction Received",
					Ref1Caption = "Message ID",
					Ref2Caption = "Provider",
					Ref3Caption = "Job Number",
					Ref4Caption = "Event Type",
					Ref5Caption = "Reference Number"
				});
				usageCaptions.Add(new StlRawUsageReportRefCaption()
				{
					Category = "SHI",
					UsageCode = "CMV",
					UsageDescription = "VGM Received",
					Ref1Caption = "Container",
					Ref2Caption = "Booking Number",
					Ref3Caption = "Bill Number",
					Ref4Caption = "Sender",
					Ref5Caption = null
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

				var builder = new ZStringBuilder();
				builder.AppendLine("	CASE TX_PriceItemCode");
				for (int i = 0; i < priceCodes.Count; ++i)
				{
					builder.Append("	WHEN '");
					builder.Append(priceCodes[i]);
					builder.Append("' THEN ");
					builder.AppendLine((i + 1).ToString(CultureInfo.InvariantCulture));
				}
				builder.AppendLine("	END");

				string priceCodeSql = string.Join("', '", priceCodes);
				string orderBySql = builder.ToString();
				return string.Format(CultureInfo.InvariantCulture, sql_Raw_Usage, priceCodeSql, orderBySql);
			}
		}

		const string sql_Raw_Usage =
@"SELECT 
	TX_PriceItemCode,
	TX_ClientID,
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
	BillingViewChargeable
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
WHERE
	TX_PriceItemCode IN ('{0}')
	AND (TX_PriceItemCode = @PriceItemCode OR @PriceItemCode = '')
	AND TX_Category = 'SHI'
	AND TX_SystemId = @DatabaseId
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	AND TX_Period = @Period
	AND TX_ClientID != TX_Reference2
ORDER BY {1}	, TX_ServiceOccuredUTC
";

		#endregion
	}
}

