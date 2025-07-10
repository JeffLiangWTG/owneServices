using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Serialization;

namespace Enterprise.Billing.Integration.OldBillingTransactions.NoVersion
{
	[Serializable]
	[XmlType(AnonymousType = true)]
	[XmlRoot(Namespace = "", IsNullable = false)]
	[XmlSerializerAssembly("Enterprise.Billing.Integration.XmlSerializers")]
	public class BillingTransaction : IOldBillingTransaction
	{
		public int BillableCount { get; set; }
		public string ClientID { get; set; }
		public string ClientNumber { get; set; }
		public string ClientStaffCode { get; set; }
		public string PriceItemCode { get; set; }
		public string Reference1 { get; set; }
		public string Reference2 { get; set; }
		public string Reference3 { get; set; }
		public string Reference4 { get; set; }
		public string ReportingSource { get; set; }
		public DateTime ServiceOccuredUTC { get; set; }

		public Integration.BillingTransaction ToLatest()
		{
			return new Integration.BillingTransaction
			{
				BillableCount = BillableCount,
				Category = GetCategory(),
				ClientID = ClientID,
				ClientNumber = ClientNumber,
				ClientStaffCode = ClientStaffCode,
				PriceItemCode = PriceItemCode,
				Reference1 = Reference1,
				Reference2 = Reference2,
				Reference3 = Reference3,
				Reference4 = Reference4,
				ReportingSource = ReportingSource,
				ServiceOccuredUTC = ServiceOccuredUTC,
			};
		}

		string GetCategory()
		{
			if (string.IsNullOrEmpty(PriceItemCode))
			{
				return null;
			}

			if (PriceItemCode.StartsWith("IC", StringComparison.OrdinalIgnoreCase) ||
				PriceItemCode.StartsWith("IU", StringComparison.OrdinalIgnoreCase) ||
				(PriceItemCode == "EAO" && Reference2 == null))
			{
				return "EAD";
			}
			if (PriceItemCode.StartsWith("CC", StringComparison.OrdinalIgnoreCase) ||
				PriceItemCode.StartsWith("CU", StringComparison.OrdinalIgnoreCase))
			{
				return "ICN";
			}
			if (StlPriceItemCodes.Contains(PriceItemCode))
			{
				return "STL";
			}
			return "UNK";
		}

		static readonly ImmutableHashSet<string> StlPriceItemCodes = new HashSet<string> { "ACA", "ACM", "ACN", "ACO", "ACS", "ACX", "ADP", "AHK", "AI3", "ANZ", "BKA", "BKG", "BKU", "BOL", "CB2", "CDM", "CDT", "CFC", "CFN", "CFP", "CFT", "CME", "CMR", "COO", "CTE", "CTI", "CTT", "CTW", "DRW", "EAD", "EAO", "ECE", "ECM", "EFC", "FGB", "FTZ", "GFC", "GFT", "GTW", "HKC", "IFB", "IFG", "INB", "INT", "JPC", "K45", "LCH", "LDG", "LTC", "LTK", "LTL", "LTM", "LVS", "MSC", "OPM", "ORM", "PTC", "PTT", "RFP", "SCE", "SCI", "SHP", "SLG", "SPK", "SRE", "SRI", "TNP", "UNB", "URC", "USP", "USR", "W3T", "W4P", "WAD", "WBM", "WIN", "WOD", "WOL", "WTD", "WTH", "WTP", "WTR" }.ToImmutableHashSet();
	}
}