using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification nides desired base class")]
public sealed class MovementReferenceNumberSupportingInfo : CusSupportingInfo, IHugeSequenceNumberLine
{
	public new class Schema : CusSupportingInfo.Schema
	{
		public new const int CSI_DescriptionMaxLength = CusInBondMoveHeader.Schema.BM_AdditionalTextMaxLength;
	}

	public MovementReferenceNumberSupportingInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

	public new MovementReferenceNumberSupportingInfoLookups Lookups => (MovementReferenceNumberSupportingInfoLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new MovementReferenceNumberSupportingInfoLookups(this);

	public new MovementReferenceNumberSupportingInfoValidation Validation => (MovementReferenceNumberSupportingInfoValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new MovementReferenceNumberSupportingInfoValidation(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = CusSupportingInfoTypeList.Codes.MovementReferenceNumber;
	}

	protected override ZString HumanReadableNameCore => Res.GetString("bb0544dd-16f0-4b8a-8b5b-899e4919e65b", "Movement Reference Number");

	[ReadOnly(true)]
	[ResourceStringData("CH.NCTS.MovementReferenceNumberSupportingInfo.CSI_LineNo", Caption = "Sequence Number", ShortCaption = "Seq. No.")]
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
					Parent.MovementReferenceNumberLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
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
		oldParent?.MovementReferenceNumberLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		Parent?.MovementReferenceNumberLineNumberGenerator.RecalculateWhenAdded(this);
	}

	public override void Delete()
	{
		Parent?.MovementReferenceNumberLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		base.Delete();
	}

	[ResourceStringData("CH.NCTS.MovementReferenceNumberSupportingInfo.CSI_ReferenceNumber", Caption = "MRN")]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("CH.NCTS.MovementReferenceNumberSupportingInfo.CSI_Status", Caption = "Seals State Valid")]
	[List(nameof(Lookups) + "." + nameof(MovementReferenceNumberSupportingInfoLookups.YesNoList))]
	[MaxLength(1)]
	public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

	[ResourceStringData("CH.NCTS.MovementReferenceNumberSupportingInfo.CSI_Description", Caption = "Additional Text")]
	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	public override bool ReadOnly
	{
		get => base.ReadOnly || (Parent?.IsArrivalNotificationDisabled ?? false);
		set => base.ReadOnly = value;
	}

	public new class Loader : BusinessObject.Loader
	{
		public Loader(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override Type GetTypeOfBusinessObjectToLoad() => typeof(MovementReferenceNumberSupportingInfo);

		public MovementReferenceNumberSupportingInfo FindByMovementReferenceNumber(string mrn)
		{
			var referenceNumberQuery = new ZDBOnlyQuery(typeof(MovementReferenceNumberSupportingInfo));
			referenceNumberQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusInBondMoveHeaderSchema.Constants.Prefix);
			referenceNumberQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.MovementReferenceNumber);
			referenceNumberQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, mrn);

			var movementQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusSupportingInfoSchema.CSI_ParentID);

				var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondMoveHeaderSchema.BM_BH);
				headerQuery.AddToFilter(NctsHeader.Loader.GetNctsHeaderQuery(Common.EU.NctsMoveHeaderType.Codes.Arrival));

			movementQuery.AddSubQuery(headerQuery, JoinCondition.And);
			referenceNumberQuery.AddSubQuery(movementQuery, JoinCondition.And);

			return Factory.LoadTop1<MovementReferenceNumberSupportingInfo>(referenceNumberQuery);
		}

		public MovementReferenceNumberSupportingInfo[] LoadMovementReferenceNumbers(params ZString[] entryNumbers)
		{
			return Factory.Load<MovementReferenceNumberSupportingInfo>(GetLoadMovementReferenceNumbersQuery(entryNumbers));
		}

		public static ZQuery GetLoadMovementReferenceNumbersQuery(params ZString[] entryNumbers)
		{
			if (entryNumbers.Length == 0)
			{
				return ZQuery.NoResultQuery;
			}

			var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			headerQuery.AddToFilter(NctsHeader.Loader.GetNctsHeaderQuery(NctsMovementType.Codes.Arrival));

				var arrivalQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusSupportingInfoSchema.CSI_ParentID);
				arrivalQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Common.EU.NctsMoveHeaderType.Codes.Arrival);
				arrivalQuery.AddSubQuery(CusInBondMoveHeaderSchema.BM_BH, headerQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.MovementReferenceNumber);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, CusInBondMoveHeaderSchema.Constants.Prefix);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, entryNumbers);
			query.AddSubQuery(arrivalQuery, JoinCondition.And);

			return query;
		}
	}
}
