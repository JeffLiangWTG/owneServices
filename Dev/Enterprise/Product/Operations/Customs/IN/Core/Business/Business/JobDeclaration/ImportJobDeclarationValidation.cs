using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class ImportJobDeclarationValidation : JobDeclarationValidation
{
	public ImportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckIECCode()
	{
		base.CheckIECCode();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.IECCodeInfo, "IEC for Selected Importer");
	}

	protected override void CheckBranchSerialNumber()
	{
		base.CheckBranchSerialNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.BranchSerialNumberInfo ,"BSN – Branch Serial Number for Selected Importer");
	}
}
