using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
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
				Destination.DeclarationValueSetStrategy.UpdateAddressesDependingOnDeclarantType(Destination.JE_DeclarantType, forceUpdateAddress: false);
				Destination.DeclarationValueSetStrategy.UpdateFinalDestinationPortFromImporter();
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

		new JobDeclaration Destination => (JobDeclaration)base.Destination;
	}
}
