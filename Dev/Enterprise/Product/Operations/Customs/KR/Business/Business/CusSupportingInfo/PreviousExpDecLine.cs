using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class PreviousExpDecLine : CusSupportingInfo
	{
		public PreviousExpDecLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int PEX_ReferenceNumberMaxLength = 15;
			public const int PEX_ReferenceNumber2MaxLength = 3;
			public const int PEX_ItemNumberMaxLength = 2;
		}

		[ReadOnly(true)]
		[ResourceStringData("39D27F9D-ABC7-4C32-B020-9C23CE303CB1", Caption = "Seq #")]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set => base.CSI_LineNo = value;
		}

		[MaxLength(Schema.PEX_ReferenceNumberMaxLength)]
		[ResourceStringData("685C64D4-BF3F-4B7B-9ADA-97B1AABE787D", Caption = "Export Entry Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[MaxLength(Schema.PEX_ReferenceNumber2MaxLength)]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		[ResourceStringData("598AE98E-2DC2-4D96-8A29-55844967AB75", Caption = "Entry Line No.")]
		public ZInt EntryLineNumber
		{
			get
			{
				return ZInt.ParseSafe(CSI_ReferenceNumber2, 0);
			}
			set
			{
				CSI_ReferenceNumber2 = value.ToString();
			}
		}

		[ResourceStringData("FCBEF4C0-D6CE-49E9-B177-AF22A02A7E87", Caption = "Invoice Line No.")]
		[MaxLength(Schema.PEX_ItemNumberMaxLength)]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set => base.CSI_ItemNumber = value;
		}

		[ResourceStringData("6C1B812B-A5EC-4895-84F9-C109BE9DF411", Caption = "Used Qty")]
		[DecimalPlaces(DecimalPlacesConstants.UsedQtyPlaces)]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set => base.CSI_Quantity = value;
		}

		[List(nameof(Lookups) + "." + nameof(NonGADetailLookups.UnitOfQuantityList))]
		[ResourceStringData("F8D08626-5259-4F38-B300-90E553652BC1", Caption = "UQ")]
		public override ZString CSI_UnitOfQuantity
		{
			get => base.CSI_UnitOfQuantity;
			set => base.CSI_UnitOfQuantity = value;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new PreviousExpDecLineValidation(this);

		public new ILineOrProduct Parent => base.Parent as ILineOrProduct;
		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return Parent?.IsValidationEnabled ?? true;
		}

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new PreviousExpDecLineLookups(this);
		}

		public IEnumerable<IZType> KeyFields => new List<IZType>() { CSI_ReferenceNumber, CSI_ReferenceNumber2, CSI_ItemNumber };

		public bool HasSameKey(PreviousExpDecLine other)
		{
			return KeyFields.SequenceEqual(other.KeyFields);
		}
	}
}
