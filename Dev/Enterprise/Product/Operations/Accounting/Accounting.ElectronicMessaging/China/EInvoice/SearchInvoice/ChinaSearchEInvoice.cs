using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	[DataContract]
	public class ChinaSearchEInvoice
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "reqType", Order = 1)]
		public string ReqType { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "data", Order = 2)]
		public SearchInvoiceData Data { get; set; } = new SearchInvoiceData();
	}

	[DataContract]
	public class SearchInvoiceData
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "serialNumber", Order = 1)]
		public string SerialNumber { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "extend", Order = 2)]
		public string Extend { get; set; }
	}
}
