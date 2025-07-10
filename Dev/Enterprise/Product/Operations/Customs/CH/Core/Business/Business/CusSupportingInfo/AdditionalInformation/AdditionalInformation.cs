using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class AdditionalInformation : CusSupportingInfo, IHugeSequenceNumberLine
{
	public new class Schema : CusSupportingInfo.Schema
	{
		public new const int CSI_CodeMaxLength = 3;
		public const int CSI_CodeMaxLength_Export = 5;
		public new const int CSI_DescriptionMaxLength = 512;
		public new const int CSI_ReferenceNumberMaxLength = 50;
		public const string CSI_DescriptionFieldType = "CSI_DescriptionFieldType";
	}

	public AdditionalInformation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new ICusSupportingInfoParent Parent => (ICusSupportingInfoParent)base.Parent;

	public new AdditionalInformationLookups Lookups => (AdditionalInformationLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalInformationLookups(this);

	public new AdditionalInformationValidation Validation => (AdditionalInformationValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInformationValidation(this);

	protected override ZString HumanReadableNameCore => Res.GetString("68941ee9-0812-4daf-81a6-9702e75171c1", "Additional Information");

	public override bool SupportsNotes => false;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.AdditionalInformation;
		CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
	}

	#region LineNo

	[ResourceStringData("Enterprise.Customs.CH.Business.AdditionalInformation|CSI_LineNo", Caption = "Detail#")]
	public override ZInt CSI_LineNo
	{
		get { return base.CSI_LineNo; }
		set
		{
			if (value > 0)
			{
				var oldValue = CSI_LineNo;
				base.CSI_LineNo = value;
				if (oldValue != value && !IsCopying && Parent != null)
				{
					Parent?.AdditionalInformationLineNumberGenerator?.RecalculateWhenRenumbered(this, oldValue);
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
			if (oldParent?.PK != value && !IsCopying)
			{
				SetLineNoOnSettingParent(oldParent);
			}
		}
	}

	void SetLineNoOnSettingParent(ICusSupportingInfoParent oldParent)
	{
		oldParent?.AdditionalInformationLineNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		Parent?.AdditionalInformationLineNumberGenerator?.RecalculateWhenAdded(this);
	}

	public override void Delete()
	{
		if (Parent != null)
		{
			Parent?.AdditionalInformationLineNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}
		base.Delete();
	}

	#endregion

	[ResourceStringData("Enterprise.Customs.CH.Business.AdditionalInformation|CSI_Code", Caption = "Code")]
	[MaxLength(nameof(CSI_CodeMaxLength))]
	[List(nameof(Lookups) + "." + nameof(AdditionalInformationLookups.AdditionalInformationCodeList))]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			ResetProductSubgroupDescriptionOnTabacco();
			base.CSI_Code = value;
			if (!IsValidationSuspended)
			{
				Parent.ValidateNonTradingGoods();
			}
		}
	}

	int CSI_CodeMaxLength => Parent.JobDeclaration.IsExportOrExportDeclarationActivation ? Schema.CSI_CodeMaxLength_Export : Schema.CSI_CodeMaxLength;

	[ResourceStringData("Enterprise.Customs.CH.Business.AdditionalInformation|CSI_ReferenceNumber", Caption = "Value")]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	[List(nameof(Lookups) + "." + nameof(AdditionalInformationLookups.ReferenceNumberCodeList))]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set
		{
			base.CSI_ReferenceNumber = value;
			if (!IsValidationSuspended)
			{
				Parent.ValidateNonTradingGoods();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.AdditionalInformation|CSI_Description", Caption = "Text")]
	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	[List(nameof(Lookups) + "." + nameof(AdditionalInformationLookups.DescriptionCodeList))]
	public override ZString CSI_Description
	{
		get => base.CSI_Description;
		set
		{
			ResetProductSubgroupDescriptionOnTabacco();
			base.CSI_Description = value;
		}
	}

	public string CSI_DescriptionFieldType
	{
		get
		{
			if (Parent is JobComInvoiceLine invoiceLine && invoiceLine.IsTobaccoRefundType && IsProductMainGroupOrSubgroup)
			{
				return nameof(FieldType.TextDropEdit);
			}

			return nameof(FieldType.TextMultiLine);
		}
	}

	void ResetProductSubgroupDescriptionOnTabacco()
	{
		if (Parent is JobComInvoiceLine invoiceLine && invoiceLine.IsTobaccoRefundType && CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup)
		{
			foreach (var additionalInformation in invoiceLine.AdditionalInformations)
			{
				if (additionalInformation.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup)
				{
					additionalInformation.CSI_Description = ZString.Empty;
				}
			}
		}
	}

	public bool IsSamnaunFreeZoneTraffic => CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic && CSI_ReferenceNumber == UniversalReferenceConstants.FreeZoneTradeCode.Samnaun;

	public bool IsProductMainGroupOrSubgroup => CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup || CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
}
