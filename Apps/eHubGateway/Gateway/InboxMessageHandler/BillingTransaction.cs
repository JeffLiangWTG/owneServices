using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CargoWise.eHub.Gateway
{
	[Serializable]
	public class BillingTransaction
	{
		public int BillableCount { get; set; }
		public string Branch { get; set; }
		public string Category { get; set; }
		public string ClientID { get; set; }
		public string ClientNumber { get; set; }
		public string ClientStaffCode { get; set; }
		public string PriceItemCode { get; set; }
		public string Reference1 { get; set; }
		public string Reference2 { get; set; }
		public string Reference3 { get; set; }
		public string Reference4 { get; set; }
		public string Reference5 { get; set; }
		public string ReportingSource { get; set; }
		public DateTime ServiceOccuredUTC { get; set; }
		public int Version { get; set; }
		public string MessageTrackingID { get; set; }
		public string AdditionalRefs { get; set; }

		public override string ToString()
        {
            return string.Format("Version: {0}, Category: {1}, PriceItemCode: {2}, BillableCount: {3}, ReportingSource: {4}, ServiceOccuredUTC: {5}, ClientID: {6}, ClientNumber: {7}, ClientStaffCode: {8}, Branch: {9}, Reference1: {10}, Reference2: {11}, Reference3: {12}, Reference4: {13}, Reference5: {14}, MessageTrackingID: {15}", Version, Category, PriceItemCode, BillableCount, ReportingSource, ServiceOccuredUTC.ToString("s"), ClientID, ClientNumber, ClientStaffCode, Branch, Reference1, Reference2, Reference3, Reference4, Reference5, MessageTrackingID);
        }
	}
}