using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class CusEntryHeaderValidation : Customs.Business.CusEntryHeaderValidation
{
	public CusEntryHeaderValidation(CusEntryHeader parent)
		: base(parent)
	{
	}

	new CusEntryHeader Parent => base.Parent as CusEntryHeader;

	protected override void CheckCH_Status()
	{
		base.CheckCH_Status();
		if (Parent.EntryInstruction?.StatusOverride ?? false)
		{
			ListValidation.ErrorIfInvalidCode(Parent.CH_StatusInfo);
		}
	}

	protected override void CheckCH_EntryStatus()
	{
		base.CheckCH_EntryStatus();
		if (Parent.EntryInstruction?.StatusOverride ?? false)
		{
			ListValidation.ErrorIfInvalidCode(Parent.CH_EntryStatusInfo);
		}
	}
}
