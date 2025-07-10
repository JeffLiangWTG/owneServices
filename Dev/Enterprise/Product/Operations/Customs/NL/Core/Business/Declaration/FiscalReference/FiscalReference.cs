using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.Business.Declaration;

public class FiscalReference : ImportExportAwareSupportingInfo, Integration.Customs.NL.IFiscalReference
{
	public FiscalReference(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : AutoCusSupportingInfo.Schema
	{
		public new const int CSI_CodeMaxLength = 3;
		public const string EntryInstructionID = "EntryInstructionID";
		public new const int CSI_ReferenceNumberMaxLength = 17;
	}

	[ResourceStringData("NLFiscalReference|G1_ReferenceNumber", Caption = "Holder EORI")]
	[List(nameof(Lookups) + "." + nameof(FiscalReferenceLookups.HolderIdentificationList))]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value.Left(Schema.CSI_ReferenceNumberMaxLength); }

	public override ZString KeyToDeterimeUniqueness => CSI_Code + CSI_ReferenceNumber;

	public new FiscalReferenceValidation Validation => (FiscalReferenceValidation)base.Validation;
	protected override CusSupportingInfoValidation GetNewValidation()
	{
		return new FiscalReferenceValidation(this);
	}

	public new FiscalReferenceLookups Lookups => (FiscalReferenceLookups)base.Lookups;
	protected override CusSupportingInfoLookups GetNewLookups()
	{
		return new FiscalReferenceLookups(this);
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.FiscalReference;
	}
	protected override ZString HumanReadableNameCore => Res.GetString("A442FD81-9BD8-4F69-9B78-4246633B090B", "Fiscal Reference");

	#region EntryInstruction
	[ReadOnlyMember(nameof(IsEntryInstructionReadOnly))]
	[List(nameof(Lookups) + "." + nameof(FiscalReferenceLookups.EntryInstructions))]
	[RelatedBusinessObject("EntryInstruction")]
	public ZGuid EntryInstructionID
	{
		get
		{
			if (entryInstructionPivot == null || entryInstructionPivot.IsDeleted)
			{
				entryInstructionPivot = MakeOrFindInstructionPivot();
			}
			return entryInstructionPivot.XX_Relation2ID;
		}
		set
		{
			if (value.IsEmpty && entryInstructionPivot != null)
			{
				entryInstructionPivot.Delete();
				entryInstructionPivot = null;
			}
			else
			{
				if (entryInstructionPivot == null || entryInstructionPivot.IsDeleted)
				{
					entryInstructionPivot = MakeOrFindInstructionPivot();
				}
				entryInstructionPivot.XX_Relation2ID = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryInstructionID();
				}
			}
			EntryInstructionIDInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo EntryInstructionIDInfo => GetZPropertyInfo(Schema.EntryInstructionID);

	GenPivot MakeOrFindInstructionPivot()
	{
		var query = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.FiscalReferenceRelatedEntryInstructionPivot);
		query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusSupportingInfoSchema.Constants.Prefix);
		query.AddToFilter(GenPivotSchema.XX_Relation1ID, PK);
		query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusEntryInstructionSchema.Constants.Prefix);
		var pivot = Factory.LoadTop1<GenPivot>(query);

		if (pivot == null)
		{
			pivot = Factory.New<GenPivot>();
			pivot.XX_RelationType = GenPivotTypeDecider.Types.FiscalReferenceRelatedEntryInstructionPivot;
			pivot.XX_Relation1ID = PK;
			pivot.XX_Relation1TableCode = CusSupportingInfoSchema.Constants.Prefix;
			pivot.XX_Relation2TableCode = CusEntryInstructionSchema.Constants.Prefix;
		}
		return pivot;
	}
	GenPivot entryInstructionPivot;

	public CusEntryInstruction EntryInstruction => Factory.Load<CusEntryInstruction>(EntryInstructionID);
	public ZBool IsRelatedToEntryInstruction(CusEntryInstruction entryInstruction)
	{
		return EntryInstruction == null || EntryInstruction == entryInstruction;
	}

	protected bool IsEntryInstructionReadOnly
	{
		get
		{
			return (Parent == null || !(Parent is JobDeclaration));
		}
	}

	#endregion
}
