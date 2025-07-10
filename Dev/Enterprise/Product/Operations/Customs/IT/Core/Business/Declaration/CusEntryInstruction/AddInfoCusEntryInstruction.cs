using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
{
	public AddInfoCusEntryInstruction(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction.CEI_AddInfoInfo)
	{
	}

	public new AddInfoCusEntryInstructionLookups Lookups => (AddInfoCusEntryInstructionLookups)base.Lookups;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryInstructionLookups(this);

	public new AddInfoCusEntryInstructionValidation Validation => (AddInfoCusEntryInstructionValidation)base.Validation;

	protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusEntryInstructionValidation(this);

	[MaxLength(3)]
	[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.ParticipantTypeList))]
	public override ZString ZG_ParticipantType { get => base.ZG_ParticipantType; set => base.ZG_ParticipantType = value; }

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ZG_PreviousInvoiceAmount", Caption = "Amount")]
	public override ZDecimal ZG_PreviousInvoiceAmount { get => base.ZG_PreviousInvoiceAmount; set => base.ZG_PreviousInvoiceAmount = value; }

	[MaxLength(3)]
	[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.CurrencyList))]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction|ZG_PreviousInvoiceCurrency", Caption = "Currency")]
	public override ZString ZG_PreviousInvoiceCurrency { get => base.ZG_PreviousInvoiceCurrency; set => base.ZG_PreviousInvoiceCurrency = value; }
}
