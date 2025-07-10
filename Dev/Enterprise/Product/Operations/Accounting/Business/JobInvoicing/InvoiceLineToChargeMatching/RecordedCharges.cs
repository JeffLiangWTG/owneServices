using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public class RecordedCharges
	{
		public ZGuid JR_PK { get; set; }
		public ZGuid E6_PK { get; set; }
		public ZGuid JR_JH { get; set; }
		public ZString JH_JobNum { get; set; }
		public ZString JK_UniqueConsignRef { get; set; }
		public ZGuid OH_PK { get; set; }
		public ZString OH_Code { get; set; }
		public OrgType OrgType { get; set; }
		public ZString AC_Code { get; set; }
		public ZString Currency { get; set; }
		public ZString JH_GS_NKRepOps { get; set; }
		public ZString AL_SystemCreateUser { get; set; }
		public ZDecimal TotalCostAmount { get; set; }
		public ZDecimal ConsolCostAmount { get; set; }
		public ZBool IsSuggestionSelected { get; set; }
	}
}
