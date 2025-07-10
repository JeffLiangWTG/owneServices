using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class CusContainer : Customs.Business.BaseCusContainer
{
	public CusContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new CusContainer Clone() => (CusContainer)base.Clone();

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusContainerLookups Lookups => (CusContainerLookups)base.Lookups;

	public new CusContainerValidation Validation => (CusContainerValidation)base.Validation;

	protected override Customs.Business.CusContainerLookups GetNewLookups() => new CusContainerLookups(this);

	protected override Customs.Business.CusContainerValidation GetNewValidation() => new CusContainerValidation(this);

	protected override System.Type GetJobDeclarationType() => typeof(JobDeclaration);

	public override ZString CO_Seal
	{
		get => base.CO_Seal;
		set
		{
			base.CO_Seal = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateCO_SecondSeal();
			}
		}
	}

	public override ZString CO_SecondSeal
	{
		get => base.CO_SecondSeal;
		set
		{
			base.CO_SecondSeal = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateCO_Seal();
			}
		}
	}
}
