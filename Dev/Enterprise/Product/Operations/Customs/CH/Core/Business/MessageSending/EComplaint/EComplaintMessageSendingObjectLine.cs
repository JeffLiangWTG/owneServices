using CargoWise.ComponentModel;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EComplaintMessageSendingObjectLine : AutoEComplaintMessageSendingObjectLine
{
	public EComplaintMessageSendingObjectLine(CusEntryHeader entryHeader) : base(entryHeader.Factory)
	{
		EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	internal CusEntryHeader EntryHeader { get; }

	[List(nameof(Lookups) + "." + nameof(EComplaintMessageSendingObjectLineLookups.LocationList))]
	public override ZString Location
	{
		get => base.Location;
		set
		{
			base.Location = value;
			if (!IsCopying && EntryLinePK_ReadOnly)
			{
				EntryLinePK = ZGuid.Empty;
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateEntryLinePK();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(EComplaintMessageSendingObjectLineLookups.FieldNameList))]
	public override ZString FieldName { get => base.FieldName; set => base.FieldName = value; }

	[List(nameof(Lookups) + "." + nameof(EComplaintMessageSendingObjectLineLookups.EntryLineList))]
	public override ZGuid EntryLinePK { get => base.EntryLinePK; set => base.EntryLinePK = value; }

	protected override bool EntryLinePK_ReadOnly => Location != EComplaintLocationList.Codes.Line;

	public string EntryLineNumber => Location == EComplaintLocationList.Codes.Line && EntryLinePK.IsValid ? Factory.Load<CusEntryLine>(EntryLinePK)?.CL_LineNumber.ToString() : null;

	public EComplaintMessageSendingObjectLineLookups Lookups => lookups ?? (lookups = new EComplaintMessageSendingObjectLineLookups(this));
	EComplaintMessageSendingObjectLineLookups lookups;
}
