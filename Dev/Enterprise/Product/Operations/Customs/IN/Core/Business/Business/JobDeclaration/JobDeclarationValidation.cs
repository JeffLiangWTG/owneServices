using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class JobDeclarationValidation : AutoINJobDeclarationValidation
{
	public JobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	protected override void CheckJE_ContainerMode_Mandatory()
	{
		if (Parent.IsSea)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ContainerModeInfo, Res.GetString("d1f1a924-19d1-4684-b26a-cb9dd1d9414d", "Container Mode / Nature Of Cargo."));
		}
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public void ValidateIECCode()
	{
		ValidateCalculatedProperty(Parent.IECCodeInfo);
	}

	public void ValidateBranchSerialNumber()
	{
		ValidateCalculatedProperty(Parent.BranchSerialNumberInfo);
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateIECCode();
		ValidateBranchSerialNumber();
	}

	protected virtual void CheckIECCode()
	{
	}

	protected virtual void CheckBranchSerialNumber()
	{
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsOfficeInfo);
	}
}
