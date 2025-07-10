using System;

namespace Enterprise.Integration.Billing
{
	public interface IStlTransaction
	{
		int BillableCount { get; set; }
		DateTime ServiceOccuredUTC { get; set; }
		string AdditionalRefs { get; set; }
		string ClientStaffCode { get; set; }
		string Reference1 { get; set; }
		string Reference2 { get; set; }
		string Reference3 { get; set; }
		string Reference4 { get; set; }
		string Reference5 { get; set; }
		string PriceItemCode { get; set; }
		string CompanyCode { get; set; }
		string Branch { get; set; }
	}
}
