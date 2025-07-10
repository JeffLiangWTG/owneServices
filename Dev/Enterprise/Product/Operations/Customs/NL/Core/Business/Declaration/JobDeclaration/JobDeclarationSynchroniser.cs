using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class JobDeclarationSynchroniser : EU.Business.Declaration.JobDeclarationSynchroniser
{
	public JobDeclarationSynchroniser(BaseJobDeclaration destination) : base(destination)
	{
	}

	protected override void OnSynchronised()
	{
		base.OnSynchronised();
		if (IsEnabled)
		{
			Destination.SetDeclarantTypeFromSupplier();
			Destination.DeclarationValueSetStrategy.UpdateAddressesDependingOnDeclarantType(Destination.JE_DeclarantType, forceUpdateAddress: false);
		}
	}

	protected override IZType GetBrokerOrgAddressPk()
	{
		if (!Destination.IsExport)
		{
			return base.GetBrokerOrgAddressPk();
		}
		else
		{
			return Destination.JE_OA_DeclarantAddress;
		}
	}

	new JobDeclaration Destination => base.Destination as JobDeclaration;
}
