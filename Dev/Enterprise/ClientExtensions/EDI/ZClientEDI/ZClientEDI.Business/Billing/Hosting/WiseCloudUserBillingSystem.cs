using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class WiseCloudUserBillingSystem : BillingSystemWithDatabase
	{
		public override string SystemCode => BillingConstants.BillingSystem.WiseCloudUser;

		protected override SystemBill CreateSystemBill() => new HostingBill(Context.Factory);

		protected override bool AccumulateUnbilledMonths => false;

		protected override void AddCodeFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			chargeableUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.ODM);
			chargeableUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, BillingConstants.Hosting.WiseCloudUserFeeCode);
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			var result = new List<HostingUsage>();
			foreach (var chargeableUsage in chargeableUsages)
			{
				var usingParty = new UsingParty(chargeableUsage);
				var usage = new HostingUsage(Context.Factory,
					usingParty,
					chargeableUsage.U1_PeriodStart,
					BillingConstants.BillingSystem.WiseCloudUser,
					chargeableUsage.U1_SubCode,
					chargeableUsage.U1_UnitCountAsInt);

				if (Context.IsPreviewOnly)
				{
					usage.SetPreviewOnly(Context.PreviewPriceHeader);
				}

				if (usage.PriceItem != null)
				{
					usage.ChargeableUsagePKs.Add(chargeableUsage.PK);
					result.Add(usage);
				}
			}

			return result.ToArray();
		}

		protected override string Query_Raw_Usage => string.Empty;

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			// Raw Usage handled by ODPLBillingSystem
			return null;
		}

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			// Raw Usage handled by ODPLBillingSystem
			return null;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			// Raw Usage handled by ODPLBillingSystem
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			// Raw Usage handled by ODPLBillingSystem
		}

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			// Raw Usage handled by ODPLBillingSystem
			return null;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			// Raw Usage handled by ODPLBillingSystem
			return null;
		}
	}
}
