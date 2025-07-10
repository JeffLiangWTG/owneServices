using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	[DataContract]
	public class TaxCoreEInvoiceResponse
	{
		//v3 Properties.

		[JsonProperty(PropertyName = "sdcDateTime")]
		public string SdcDateTime { get; set; }

		[JsonProperty(PropertyName = "invoiceCounter")]
		public string InvoiceCounter { get; set; } = null;

		[JsonProperty(PropertyName = "invoiceNumber")]
		public string InvoiceNumber { get; set; } = null;

		[JsonProperty(PropertyName = "encryptedInternalData")]
		public string InternalData { get; set; } = null;

		[JsonProperty(PropertyName = "signature")]
		public string Signature { get; set; } = null;

		// Legacy support properties.

		[DataMember(Name = "DT")]
		public string DT { get => SdcDateTime; set => SdcDateTime = value; }

		[DataMember(Name = "IC")]
		public string IC { get => InvoiceCounter; set => InvoiceCounter = value; }

		[DataMember(Name = "IN")]
		public string IN { get => InvoiceNumber; set => InvoiceNumber = value; }

		[DataMember(Name = "ID")]
		public string ID { get => InternalData; set => InternalData = value; }

		[DataMember(Name = "S")]
		public string S { get => Signature; set => Signature = value; }

		// Common properties.

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "RequestedBy", Order = 1)]
		public string RequestedBy { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "SignedBy", Order = 2)]
		public string SignedBy { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "InvoiceCounterExtension", Order = 5)]
		public string InvoiceCounterExtension { get; set; } = null;

		[SuppressMessage("Microsoft.Design", "CA1056: URI properties should not be strings")]
		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "VerificationUrl", Order = 7)]
		public string VerificationUrl { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "VerificationQRCode", Order = 8)]
		public string VerificationQRCode { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "Journal", Order = 9)]
		public string Journal { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "Messages", Order = 10)]
		public string Messages { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "TotalCounter", Order = 11)]
		public int TotalCounter { get; set; } = 0;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "TransactionTypeCounter", Order = 12)]
		public int TransactionTypeCounter { get; set; } = 0;

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "TotalAmount", Order = 13)]
		public decimal TotalAmount { get; set; } = 0m;

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "TaxItems", Order = 16)]
		public List<TaxItem> TaxItems => taxItems ?? (taxItems = new List<TaxItem>());
		List<TaxItem> taxItems;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "Hash", Order = 17)]
		public string Hash { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "BusinessName", Order = 18)]
		public string BusinessName { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "LocationName", Order = 18)]
		public string LocationName { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "Address", Order = 19)]
		public string Address { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "TIN", Order = 20)]
		public string TIN { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "District", Order = 21)]
		public string District { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "MRC", Order = 22)]
		public string MRC { get; set; } = null;

		[DataContract]
		public class TaxItem
		{
			[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "Label", Order = 1)]
			public string Label { get; set; } = null;

			[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "CategoryName", Order = 2)]
			public string CategoryName { get; set; } = null;

			[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "Rate", Order = 3)]
			public decimal Rate { get; set; } = 0m;

			[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "Amount", Order = 4)]
			public decimal Amount { get; set; } = 0m;
		}
	}
}
