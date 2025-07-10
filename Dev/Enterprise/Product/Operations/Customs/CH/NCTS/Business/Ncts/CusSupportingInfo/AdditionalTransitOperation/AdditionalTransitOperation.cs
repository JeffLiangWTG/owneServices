using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification nides desired base class")]
public sealed class AdditionalTransitOperation : CusSupportingInfo, IHugeSequenceNumberLine
{
	public new class Schema : CusSupportingInfo.Schema
	{
		public const int CSI_PackQtyMaxLength = 8;
		public new const int CSI_IssuerTypeMaxLength = 2;
		public new const int CSI_UnitOfQuantityMaxLength = 2;
		public new const int CSI_ReferenceNumberMaxLength = 70;
	}

	public AdditionalTransitOperation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ResourceStringData("CH.NCTS.AdditionalTransitOperation.CSI_LineNo", Caption = "Sequence Number", ShortCaption = "Seq. No.")]
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
					Parent.AdditionalTransitOperationLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
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
		oldParent?.AdditionalTransitOperationLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		Parent?.AdditionalTransitOperationLineNumberGenerator.RecalculateWhenAdded(this);
	}

	public override void Delete()
	{
		Parent?.AdditionalTransitOperationLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		base.Delete();
	}

	[ResourceStringData("CH.NCTS.AdditionalTransitOperation.CSI_IssuerType", Caption = "Add. Transit Code", ShortCaption = "Add. Transit")]
	[MaxLength(Schema.CSI_IssuerTypeMaxLength)]
	public override ZString CSI_IssuerType { get => base.CSI_IssuerType; set => base.CSI_IssuerType = value; }

	[ResourceStringData("CH.NCTS.AdditionalTransitOperation.CSI_ReferenceNumber", Caption = "Reference Number", ShortCaption = "Reference")]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("CH.NCTS.AdditionalTransitOperation.CSI_Description", Caption = "Description")]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[ResourceStringData("CH.NCTS.AdditionalTransitOperation.CSI_Quantity", Caption = "Gross Mass")]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[ReadOnly(true)]
	[MaxLength(Schema.CSI_UnitOfQuantityMaxLength)]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	[ResourceStringData("CH.NCTS.AdditionalTransitOperation.CSI_PackQty", Caption = "Number of Packages", ShortCaption = "No. Packages")]
	[MaxLength(Schema.CSI_PackQtyMaxLength)]
	public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

	[ResourceStringData("CH.NCTS.AdditionalTransitOperation.CSI_PackType", Caption = "Type of Package")]
	public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

	[List(nameof(Lookups) + "." + nameof(AdditionalTransitOperationLookups.StateOfSealsValidList))]
	[ResourceStringData("CH.NCTS.AdditionalTransitOperation.CSI_Status", Caption = "State of Seals Valid")]
	public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.AdditionalTransitOperation;
		CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
		CSI_PackType = PackTypeCodes.PK;
	}

	public override bool ReadOnly
	{
		get => base.ReadOnly || (Parent?.IsAdditionalGoodsInformationLocked ?? false);
	}

	public new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

	public new AdditionalTransitOperationLookups Lookups => (AdditionalTransitOperationLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalTransitOperationLookups(this);

	protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalTransitOperationValidation(this);
}
