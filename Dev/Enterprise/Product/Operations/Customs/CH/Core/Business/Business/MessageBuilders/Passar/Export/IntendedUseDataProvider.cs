using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class IntendedUseDataProvider : IIntendedUse
{
	public static IntendedUseDataProvider New(CusEntryInstruction entryInstruction) => entryInstruction == null ? null : new IntendedUseDataProvider(entryInstruction);

	public IntendedUseDataProvider(CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = entryInstruction;
	}
	protected readonly CusEntryInstruction entryInstruction;

	public string GoodsProvision => entryInstruction.CEI_Procedure;
	public string InputControl => entryInstruction.CEI_Style;

	public string ReasonTaxExemption => null;
}
