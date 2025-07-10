using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification nides desired base class")]
public sealed class SupernumeraryGoods : CusSupportingInfo, IHugeSequenceNumberLine
{
	public new class Schema : CusSupportingInfo.Schema
	{
		public new const int CSI_TariffMaxLength = 6;
		public const int CSI_PackQtyMaxLength = 8;
	}

	public SupernumeraryGoods(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ResourceStringData("CH.NCTS.SupernumeraryGoods.CSI_LineNo", Caption = "Sequence Number", ShortCaption = "Seq. No.")]
	public override ZInt CSI_LineNo
	{
		get => base.CSI_LineNo;
		set
		{
			if (value > 0)
			{
				var oldValue = CSI_LineNo;
				base.CSI_LineNo = value;
				if (oldValue != value && !IsCopying && Parent != null)
				{
					Parent.SupernumeraryGoodsLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}
	}

	ZGuid ISequenceNumberLine.FKToHeader => CSI_ParentID;

	ZInt ISequenceNumberLine<ZInt>.SequenceNumber
	{
		get => CSI_LineNo;
		set => CSI_LineNo = value;
	}

	public override ZGuid CSI_ParentID
	{
		get => base.CSI_ParentID;
		set
		{
			var oldParent = Parent;
			base.CSI_ParentID = value;
			if (oldParent != value && !IsCopying)
			{
				SetLineNoOnSettingParent(oldParent);
			}
		}
	}

	public override ZString CSI_ParentTableCode
	{
		get => base.CSI_ParentTableCode;
		set
		{
			var oldParent = Parent;
			base.CSI_ParentTableCode = value;
			if (oldParent != value && !IsCopying)
			{
				SetLineNoOnSettingParent(oldParent);
			}
		}
	}

	void SetLineNoOnSettingParent(NctsArrivalMovementHeader oldParent)
	{
		oldParent?.SupernumeraryGoodsLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		Parent?.SupernumeraryGoodsLineNumberGenerator.RecalculateWhenAdded(this);
	}

	public override void Delete()
	{
		Parent?.SupernumeraryGoodsLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		base.Delete();
	}

	[ResourceStringData("CH.NCTS.SupernumeraryGoods.CSI_Quantity", Caption = "Gross Mass")]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[ReadOnly(true)]
	[MaxLength(2)]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	[ResourceStringData("CH.NCTS.SupernumeraryGoods.CSI_Tariff", Caption = "Tariff Code", ShortCaption = "HS-Code")]
	[MaxLength(Schema.CSI_TariffMaxLength)]
	[List(nameof(Lookups) + "." + nameof(SupernumeraryGoodsLookups.TariffList))]
	public override ZString CSI_Tariff { get => base.CSI_Tariff; set => base.CSI_Tariff = value; }

	public const string TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;

	public const string TariffDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;

	[ResourceStringData("CH.NCTS.SupernumeraryGoods.CSI_PackQty", Caption = "Number of Packages", ShortCaption = "No. Packages")]
	[MaxLength(Schema.CSI_PackQtyMaxLength)]
	public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

	[ResourceStringData("CH.NCTS.SupernumeraryGoods.CSI_PackType", Caption = "Type of Package")]
	public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.SupernumeraryGoods;
		CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
	}

	public override bool ReadOnly
	{
		get => base.ReadOnly || (Parent?.IsAdditionalGoodsInformationLocked ?? false);
	}

	public new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

	public new SupernumeraryGoodsLookups Lookups => (SupernumeraryGoodsLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new SupernumeraryGoodsLookups(this);

	public new SupernumeraryGoodsValidation Validation => (SupernumeraryGoodsValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new SupernumeraryGoodsValidation(this);
}
