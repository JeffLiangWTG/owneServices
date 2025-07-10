using CargoWise.Types;
using Enterprise.Customs.Business;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobDeclarationSynchroniser : EU.Business.Declaration.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration destination) : base(destination)
		{
		}

		protected override void PackingModeSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			base.PackingModeSynchroniser_Format(sender, e);

			if (e.DesiredType == typeof(ZString))
			{
				var shipmentContainerMode = Source.JS_PackingMode;
				var consolidationContainerMode = Destination.RelevantConsol?.JK_ConsolMode ?? ZString.Empty;
				if ((consolidationContainerMode == ContainerModes.BuyersConsol || consolidationContainerMode == ContainerModes.FCL || consolidationContainerMode == ContainerModes.Groupage) && (shipmentContainerMode == ContainerModes.LCL || shipmentContainerMode == ContainerModes.BuyersConsol))
				{
					e.Value = (ZString)ContainerModes.Containerised;
				}
			}
		}
	}
}
