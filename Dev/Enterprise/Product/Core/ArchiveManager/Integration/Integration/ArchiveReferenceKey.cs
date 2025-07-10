using System;
using System.ComponentModel;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Integration
{
	[ImmutableObject(true)]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ReferenceKeyType
	{
		public ReferenceKeyType(string code, MultilingualString description)
		{
			Code = code;
			Description = description;
		}

		public readonly string Code;
		public readonly MultilingualString Description;
	}

	public class ArchiveReferenceKey
	{
		public static class CommonTypes
		{
			public static ReferenceKeyType JobNo
				=> jobNo ?? (jobNo = new ReferenceKeyType("JOB", ResString.GetMultilingualString("30b7a1e1-8fa5-4808-b40a-1b4f47be5e4d", "Job #")));

			[ThreadStatic]
			static ReferenceKeyType jobNo;

			public static ReferenceKeyType ShipmentNo
				=> shipmentNo ?? (shipmentNo = new ReferenceKeyType("SHP", ResString.GetMultilingualString("350d7743-f3d3-44ce-9720-99bdad7a2fa6", "Shipment #")));

			[ThreadStatic]
			static ReferenceKeyType shipmentNo;

			public static ReferenceKeyType DeclarationNo
				=> declarationNo ?? (declarationNo = new ReferenceKeyType("DEC", ResString.GetMultilingualString("d2499f57-6ecb-49b0-8e04-aa0fab248665", "Declaration #")));

			[ThreadStatic]
			static ReferenceKeyType declarationNo;

			public static ReferenceKeyType Housebill
				=> housebill ?? (housebill = new ReferenceKeyType("HBL", ResString.GetMultilingualString("53601a56-0003-47e1-b730-24c788300e01", "House Bill #")));

			[ThreadStatic]
			static ReferenceKeyType housebill;

			public static ReferenceKeyType Masterbill
				=> masterbill ?? (masterbill = new ReferenceKeyType("MBL", ResString.GetMultilingualString("87feff84-e92d-4972-8073-c2e25ca23eb7", "Master Bill #")));

			[ThreadStatic]
			static ReferenceKeyType masterbill;

			public static ReferenceKeyType Consignee
				=> consignee ?? (consignee = new ReferenceKeyType("CNE", ResString.GetMultilingualString("188de59f-4391-4eb7-8e60-f54b8f9ee92c", "Consignee/Importer Code")));

			[ThreadStatic]
			static ReferenceKeyType consignee;

			public static ReferenceKeyType Consignor
				=> consignor ?? (consignor = new ReferenceKeyType("CNR", ResString.GetMultilingualString("faca62ce-da65-4cc8-bf3e-f98a51f7d004", "Consignor/Supplier Code")));

			[ThreadStatic]
			static ReferenceKeyType consignor;

			public static ReferenceKeyType OwnerReference
				=> ownerReference ?? (ownerReference = new ReferenceKeyType("OWN", ResString.GetMultilingualString("033CC86B-BAC9-47f7-A62E-F5432FCF2D5F", "Owner Reference #")));

			[ThreadStatic]
			static ReferenceKeyType ownerReference;

			public static ReferenceKeyType InvoiceNo
				=> invoiceNo ?? (invoiceNo = new ReferenceKeyType("INV", ResString.GetMultilingualString("886a413f-bd2e-4794-8f2a-f1df76170ea3", "Invoice #")));

			[ThreadStatic]
			static ReferenceKeyType invoiceNo;

			public static ReferenceKeyType FlightAndDate
				=> flightAndDate ?? (flightAndDate = new ReferenceKeyType("FLT", ResString.GetMultilingualString("627834bd-56f1-4d00-bf68-9aa1847b7981", "Flight/Date(YYYMMDD)")));

			[ThreadStatic]
			static ReferenceKeyType flightAndDate;

			public static ReferenceKeyType RegoAndDate
				=> regoAndDate ?? (regoAndDate = new ReferenceKeyType("REG", ResString.GetMultilingualString("506298a4-a8d1-442d-9023-ae28f42aaf1b", "Rego./Date(YYYYMMDD)")));

			[ThreadStatic]
			static ReferenceKeyType regoAndDate;

			public static ReferenceKeyType VesselVoyage
				=> vesselVoyage ?? (vesselVoyage = new ReferenceKeyType("VVY", ResString.GetMultilingualString("5f75c4bb-8b7d-4d90-a80d-dc74da6e3981", "Vessel/Voyage")));

			[ThreadStatic]
			static ReferenceKeyType vesselVoyage;

			public static ReferenceKeyType ContainerNo
				=> containerNo ?? (containerNo = new ReferenceKeyType("CNO", ResString.GetMultilingualString("9d11e6d4-17ea-4028-98f2-e0f3f0a9a5c0", "Container #")));

			[ThreadStatic]
			static ReferenceKeyType containerNo;

			public static ReferenceKeyType PartNo
				=> partNo ?? (partNo = new ReferenceKeyType("PTN", ResString.GetMultilingualString("9f1c5504-f387-48a0-8e23-b120618e4370", "Part #")));

			[ThreadStatic]
			static ReferenceKeyType partNo;
		}

		public ArchiveReferenceKey(ReferenceKeyType keyType, string value)
		{
			_ = Argument.NotNull(keyType, "keyType");
			if (value.Length > StorageReferenceSchema.SR_Reference.MaxLength)
			{
				ErrorReporter.ReportOnce("MaxLengthExceededInArchiveReferenceKey",
					string.Format("ArchiveReferenceKey value is too long. Max length is {0}, keyType = {1}/{2}, value = {3}", StorageReferenceSchema.SR_Reference.MaxLength, keyType.Code, keyType.Description, value));
			}

			KeyType = keyType;
			Value = value;
		}

		public ReferenceKeyType KeyType { get; private set; }

		public string Value { get; private set; }
	}
}
