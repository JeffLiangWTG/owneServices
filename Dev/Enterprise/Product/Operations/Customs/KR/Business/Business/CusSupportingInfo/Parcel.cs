using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class Parcel : CusSupportingInfo
	{
		public Parcel(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int MAI_ReferenceNumberMaxLength = 14;
			public const int MAI_ReferenceNumber2MaxLength = 13;
			public const int MAI_CodeMaxLength = 1;
		}

		[MaxLength(Schema.MAI_ReferenceNumberMaxLength)]

		[ResourceStringData("285C9EA0-5D4E-4727-9A68-84FB57AD5318", Caption = "Customs Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[MaxLength(Schema.MAI_ReferenceNumber2MaxLength)]
		[ResourceStringData("C8500EB5-ACD0-4726-AC4B-0850BF6C3301", Caption = "Parcel Number")]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		[MaxLength(Schema.MAI_CodeMaxLength)]
		[ResourceStringData("8BE67B19-BAE2-46DF-9076-399000DBE013", Caption = "Delivery Type")]
		[List(nameof(Lookups) + "." + nameof(ParcelLookups.DeliveryTypeCodeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new ParcelValidation(this);
		public new ParcelValidation Validation => (ParcelValidation)base.Validation;

		protected override CusSupportingInfoLookups GetNewLookups() => new ParcelLookups(this);
		public new ParcelLookups Lookups => (ParcelLookups)base.Lookups;
		public new JobComInvoiceHeader Parent => base.Parent as JobComInvoiceHeader;
	}
}
