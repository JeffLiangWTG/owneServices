using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants.Hosting;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class HostingStorageBillingSystem : BillingSystemWithDatabase
	{
		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.HostingStorage; }
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			var bufferPercentage = EDIDataRegistry.Instance.HostingStorageBufferPercentage.Value;
			var bufferPeriodStart = GetBufferPeriodStart();

			var result = new List<HostingUsage>();
			foreach (var chargeableUsagesByDatabaseByPeriod in chargeableUsages.GroupBy(x => new { x.U1_LD, x.U1_PeriodStart }))
			{
				var usagesByDatabase = new List<HostingUsage>();

				foreach (var chargeableUsage in chargeableUsagesByDatabaseByPeriod)
				{
					var sizeMB = chargeableUsage.U1_UnitCountAsInt;
					sizeMB = SizeWithBuffer(sizeMB, chargeableUsage.U1_PeriodStart, bufferPercentage, bufferPeriodStart);
					var usage = new HostingUsage(Context.Factory, new UsingParty(chargeableUsage), chargeableUsage.U1_PeriodStart, chargeableUsage.U1_Code, chargeableUsage.U1_SubCode, sizeMB);
					if (Context.IsPreviewOnly)
					{
						usage.SetPreviewOnly(Context.PreviewPriceHeader);
					}

					usage.ChargeableUsagePKs.Add(chargeableUsage.PK);
					usagesByDatabase.Add(usage);
				}

				result.AddRange(FilterOutUnusedUsages(usagesByDatabase));
			}

			return result.ToArray();
		}

		IEnumerable<HostingUsage> FilterOutUnusedUsages(IEnumerable<HostingUsage> usages)
		{
			var result = usages.Where(x => x.UnitCount != 0);

			if (result.Any())
			{
				var priceHeader = usages.FirstOrDefault(x => x.PriceHeader != null)?.PriceHeader;

				if (priceHeader != null)
				{
					var items = priceHeader.LocalOrStandardItems;
					var hasDataStorageCode = items.FindByCode(DataStorageCode) != null;
					var hasEDocsStorageCode = items.FindByCode(eDocsStorageCode) != null;
					var hasUltraFastStorageCode = items.FindByCode(UltraFastStorageCode) != null;

					result = result.Where(x => !(x.SubCode == DataStorageCode && !hasDataStorageCode && hasUltraFastStorageCode)
											&& !(x.SubCode == eDocsStorageCode && !hasEDocsStorageCode && hasUltraFastStorageCode)
											&& !(x.SubCode == UltraFastStorageCode && !hasUltraFastStorageCode && (hasDataStorageCode || hasEDocsStorageCode)));
				}
				else
				{
					if (result.First().PeriodStart < Context.PeriodStart)
					{
						result = Enumerable.Empty<HostingUsage>();
					}
				}
			}

			return result;
		}

		internal static ZDateTime GetBufferPeriodStart()
		{
			return new ZDateTime(2015, 5, 1);
		}

		internal static int SizeWithBuffer(int sizeMB, ZDateTime periodStart, int bufferPercentage, ZDateTime bufferPeriodStart)
		{
			int result;

			if (bufferPercentage != 0 && periodStart >= bufferPeriodStart)
			{
				result = (int)(((long)sizeMB) * (100 + bufferPercentage) / 100);
			}
			else
			{
				result = sizeMB;
			}

			return result;
		}

		protected override SystemBill CreateSystemBill()
		{
			return new HostingBill(Context.Factory);
		}

		#region Load Raw Usage

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			SystemCodeRawUsage rawUsage = new SystemCodeRawUsage(context, SystemCode);
			rawUsage.SummaryHeaderDescription = "WiseCloud Premium - Data Storage";

			rawUsage.Summary.Header.Column1 = "Type";
			rawUsage.Summary.Header.Column2 = "GB";

			int bufferPercentage = EDIDataRegistry.Instance.HostingStorageBufferPercentage.Value;
			ZDateTime bufferPeriodStart = GetBufferPeriodStart();
			ClientChargeableUsage[] chargeableUsages = LoadRawChargeableUsages(context);
			var usagesForReporting = UsagesFilteredByContext(chargeableUsages, context, false);

			foreach (var usage in usagesForReporting)
			{
				SummaryLine line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = StorageDescription(usage);
				int sizeMB = SizeWithBuffer(usage.U1_UnitCountAsInt, usage.U1_PeriodStart, bufferPercentage, bufferPeriodStart);
				line.Column2 = (sizeMB / (decimal)BillingConstants.Hosting.MBperGB).ToString("#,##0.000", CultureInfo.InvariantCulture);
			}

			return rawUsage;
		}

		static string StorageDescription(ClientChargeableUsage usage)
		{
			return StorageCodeDescription(usage.U1_SubCode) + " [" + usage.Database.LD_ServerCode + "]";
		}

		static string StorageCodeDescription(string code)
		{
			switch (code)
			{
				case BillingConstants.Hosting.DataStorageCode: return "High Speed";
				case BillingConstants.Hosting.eDocsStorageCode: return "Image";
				case BillingConstants.Hosting.UltraFastStorageCode: return "Ultra Fast";
				case BillingConstants.Hosting.NonProductionStorageCode: return "Non-Production";
			}

			return "";
		}

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context, SystemCode);
			rawUsage.SummaryHeaderDescription = "WiseCloud Premium - Data Storage";

			rawUsage.Summary.Header.Column1 = "Company Code";
			rawUsage.Summary.Header.Column2 = "Type";
			rawUsage.Summary.Header.Column3 = "GB";

			int bufferPercentage = EDIDataRegistry.Instance.HostingStorageBufferPercentage.Value;
			ZDateTime bufferPeriodStart = GetBufferPeriodStart();
			ClientChargeableUsage[] chargeableUsages = LoadRawChargeableUsages(context);
			var usagesForReporting = UsagesFilteredByContext(chargeableUsages, context, true);

			foreach (var usage in usagesForReporting)
			{
				SummaryLine line = rawUsage.Summary.Lines.AddNew();
				line.Column1 = usage.CompanyCode;
				line.Column2 = StorageDescription(usage);
				int sizeMB = SizeWithBuffer(usage.U1_UnitCountAsInt, usage.U1_PeriodStart, bufferPercentage, bufferPeriodStart);
				line.Column3 = (sizeMB / (decimal)BillingConstants.Hosting.MBperGB).ToString("#,##0.000", CultureInfo.InvariantCulture);
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			string[] headerColumns;
			if (isStlBilling)
			{
				headerColumns = new string[] { "Company Code", "Type", "GB" };
			}
			else
			{
				headerColumns = new string[] { "Type", "GB" };
			}

			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			int bufferPercentage = EDIDataRegistry.Instance.HostingStorageBufferPercentage.Value;
			var bufferPeriodStart = GetBufferPeriodStart();
			var chargeableUsages = LoadRawChargeableUsages(context);
			var usagesForReporting = UsagesFilteredByContext(chargeableUsages, context, isStlBilling);

			foreach (var usage in usagesForReporting)
			{
				string storageType = StorageDescription(usage);
				int sizeMB = SizeWithBuffer(usage.U1_UnitCountAsInt, usage.U1_PeriodStart, bufferPercentage, bufferPeriodStart);
				string sizeGBText = (sizeMB / (decimal)BillingConstants.Hosting.MBperGB).ToString("#,##0.000", CultureInfo.InvariantCulture);

				string[] dataValues;
				if (isStlBilling)
				{
					dataValues = new string[] { usage.CompanyCode, storageType, sizeGBText };
				}
				else
				{
					dataValues = new string[] { storageType, sizeGBText };
				}

				var dataCsvLine = new OCsvLine(dataValues);
				action(dataCsvLine.ToString());
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			int bufferPercentage = EDIDataRegistry.Instance.HostingStorageBufferPercentage.Value;
			var bufferPeriodStart = GetBufferPeriodStart();
			var chargeableUsages = LoadRawChargeableUsages(context);
			var usagesForReporting = UsagesFilteredByContext(chargeableUsages, context, isStlBilling);

			foreach (var usage in usagesForReporting)
			{
				string storageType = StorageDescription(usage);
				int sizeMB = SizeWithBuffer(usage.U1_UnitCountAsInt, usage.U1_PeriodStart, bufferPercentage, bufferPeriodStart);
				string sizeGBText = (sizeMB / (decimal)BillingConstants.Hosting.MBperGB).ToString("#,##0.000", CultureInfo.InvariantCulture);
				string companyCode = "";
				var feeType = context.PriceItemFeeType;
				if (feeType.IsEmpty)
				{
					feeType = BillingConstants.FeeType.Per10GBPerMonthMin1GB;
				}
				int unitCount = HostingUsage.ConvertToFeeTypeUnits(feeType, sizeMB);
				writer.WriteCsvUsageReport(usage.U1_SystemCreateTimeUtc, companyCode, "", "", string.Concat(storageType, ' ', sizeGBText).Trim(), context.PriceItemCode, context.PriceItemDescription, unitCount);
			}
		}

		static IEnumerable<ClientChargeableUsage> UsagesFilteredByContext(IEnumerable<ClientChargeableUsage> usages, BillingLoadRawUsageContext context, bool isStlBilling)
		{
			var unsorted = isStlBilling
				? usages.Where(x => x.U1_LD == context.DatabasePK)
				: usages.Where(x => x.OrganisationPK == context.OrganisationPK && (context.DatabasePK.IsEmpty || context.DatabasePK == x.U1_LD));

			if (!context.PriceItemCode.IsEmpty)
			{
				unsorted = unsorted.Where(x => x.U1_SubCode == context.PriceItemCode);
			}
			else if (context.LicenceCompanyPK.IsValid)
			{
				var priceHeader = context.Factory.Load<LicenceCompany>(context.LicenceCompanyPK)
					?.PriceHeaderForDate(context.Period, isStlBilling ? BillingConstants.BillingSystem.STL : BillingConstants.BillingSystem.ODM);

				if (priceHeader != null)
				{
					if (priceHeader.L6_IsStandard)
					{
						priceHeader = priceHeader.StandardPrices;
					}

					var priceItemCodes = new HashSet<ZString>(priceHeader.Items.Select(x => x.L7_Code).Where(x => !x.IsEmpty).Distinct());
					if (priceItemCodes.Any())
					{
						unsorted = unsorted.Where(x => priceItemCodes.Contains(x.U1_SubCode));
					}
				}
			}

			return unsorted.OrderBy(x => x.U1_SubCode);
		}

		protected override string Query_Raw_Usage
		{
			get { throw new InvalidOperationException(); }
		}

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			throw new InvalidOperationException();
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(System.Data.IDataReader reader, BillingLoadRawUsageContext context)
		{
			throw new InvalidOperationException();
		}

		#endregion

		#region Accumulate Unbilled Months

		protected override ZDateTime EarliestUsageToAccumulate
		{
			get { return Context.PeriodStart.AddMonths(-1); }
		}

		#endregion
	}
}

