using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class StlTransactionForDisplay : NonPersistentBusinessObject
	{
		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|Branch", Caption = "Branch")]
		public ZString Branch { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|ClientStaffCode", Caption = "Client Staff Code")]
		public ZString ClientStaffCode { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|Company", Caption = "Company")]
		public ZString Company { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|PriceItemCode", Caption = "Code")]
		public ZString PriceItemCode { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|Reference1", Caption = "Reference 1")]
		public ZString Reference1 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|Reference2", Caption = "Reference 2")]
		public ZString Reference2 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|Reference3", Caption = "Reference 3")]
		public ZString Reference3 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|Reference4", Caption = "Reference 4")]
		public ZString Reference4 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|Reference5", Caption = "Reference 5")]
		public ZString Reference5 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|BillableCount", Caption = "Billing Count")]
		public ZInt BillableCount { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|ServiceOccuredUTC", Caption = "Service Occurred UTC")]
		public ZDateTime ServiceOccuredUTC { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|AdditionalRefs", Caption = "Additional References")]
		public ZString AdditionalRefs { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is string representation of the billing transaction, used in error reports")]
		public override string ToString()
		{
			return $"BillableCount: {BillableCount}, Branch: {Branch}, ClientStaffCode: {ClientStaffCode}, PriceItemCode: {PriceItemCode}, Reference1: {Reference1}, Reference2: {Reference2}, Reference3: {Reference3}, Reference4: {Reference4}, Reference5: {Reference5}, ServiceOccuredUTC: {ServiceOccuredUTC}, AdditionalRefs: {AdditionalRefs}, Company: {Company}";
		}
	}
}
