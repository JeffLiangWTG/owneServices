using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CH;
using CusEntryHeader = Enterprise.Customs.CH.Business.CusEntryHeader;

namespace Enterprise.Customs.CH.NCTS.Business;

public class RelatedExportEntryHeaderGenPivot : CustomsGenPivot, INctsRelatedExportEntryHeaderGenPivot, IHugeSequenceNumberLine
{
	public RelatedExportEntryHeaderGenPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new RelatedExportEntryHeaderGenPivotValidation Validation => (RelatedExportEntryHeaderGenPivotValidation)base.Validation;

	protected override GenPivotValidation GetNewValidation() => new RelatedExportEntryHeaderGenPivotValidation(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		XX_RelationType = GenPivotTypeDecider.Types.NctsRelatedExportGenPivot;
		XX_Relation1TableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
		XX_Relation2TableCode = CusEntryHeaderSchema.Constants.Prefix;
	}

	public override void Delete()
	{
		RecalculateSequenceOnDelete();
		base.Delete();
	}

	public NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)Relation1Object;

	public CusEntryHeader EntryHeader => (CusEntryHeader)Relation2Object;

	public override ZGuid XX_Relation1ID
	{
		get => base.XX_Relation1ID;
		set
		{
			var oldParent = Parent;
			base.XX_Relation1ID = value;
			if (oldParent != Parent && !IsCopying)
			{
				RecalculateSequenceOnParentChange(oldParent);
			}
		}
	}

	[ReadOnly(true)]
	[ResourceStringData("CH.RelatedExportEntryHeaderGenPivot.XX_Sequence", Caption = "Sequence")]
	public override ZInt XX_Sequence { get => base.XX_Sequence; set => base.XX_Sequence = value; }

	[ResourceStringData("CH.RelatedExportEntryHeaderGenPivot.ShipmentType", Caption = "Shipment Type")]
	public ZString ShipmentType => EntryHeader?.Declaration?.JE_MessageType ?? ZString.Empty;

	public ZPropertyInfo ShipmentTypeInfo => GetZPropertyInfo(nameof(ShipmentType));

	[ResourceStringData("CH.RelatedExportEntryHeaderGenPivot.EntryNumber", Caption = "Entry Number")]
	public ZString EntryNumber => EntryHeader?.EntryNumber ?? ZString.Empty;

	[ResourceStringData("CH.RelatedExportEntryHeaderGenPivot.JobNumber", Caption = "Job Number")]
	public ZString JobNumber => EntryHeader?.Declaration.JE_DeclarationReference ?? ZString.Empty;

	[ResourceStringData("CH.RelatedExportEntryHeaderGenPivot.ReferenceNumber", Caption = "Reference Number")]
	public ZString ReferenceNumber => EntryHeader?.CH_BGMReference ?? ZString.Empty;

	[ResourceStringData("CH.RelatedExportEntryHeaderGenPivot.EntryStatus", Caption = "Entry Status")]
	public ZString EntryStatus => EntryHeader?.CH_EntryStatus ?? ZString.Empty;

	[ResourceStringData("CH.RelatedExportEntryHeaderGenPivot.EntryStatusDescription", Caption = "Entry Status Description")]
	public ZString EntryStatusDescription => EntryHeader?.EntryHeaderStatusDescription ?? ZString.Empty;

	#region Sequence Number calculation

	ZInt ISequenceNumberLine<ZInt>.SequenceNumber { get => XX_Sequence; set => XX_Sequence = value; }

	ZGuid ISequenceNumberLine.FKToHeader => Relation1ID;

	void RecalculateSequenceOnParentChange(NctsDepartureMovementHeader oldParent)
	{
		oldParent?.RelatedExportEntryHeadersLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		Parent?.RelatedExportEntryHeadersLineNumberGenerator.RecalculateWhenAdded(this);
	}

	void RecalculateSequenceOnDelete()
	{
		Parent?.RelatedExportEntryHeadersLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
	}

	#endregion

	protected override ZString HumanReadableNameCore => Res.GetString("9DCF8197-AA31-462B-8091-772C1F73DD7B", "Export Declaration");
}
