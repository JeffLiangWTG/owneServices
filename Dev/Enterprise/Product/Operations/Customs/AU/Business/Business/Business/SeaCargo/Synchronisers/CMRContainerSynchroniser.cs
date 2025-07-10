using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRContainerSynchroniser : ContainerSynchroniser
	{
		public CMRContainerSynchroniser(CusSCAContainer destination, CommonContainer source, CommonConsol parentConsol)
			: base(destination, source, parentConsol)
		{
			this.SCAContainer = destination;
			this.JobContainer = source;
		}

		public readonly CusSCAContainer SCAContainer;
		public readonly CommonContainer JobContainer;

		#region Implementation

		protected override void ContainerModeSynchroniser_Format(object sender, Customs.Business.FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZString)
			{
				ZString containerMode = (ZString)e.Value;
				switch (containerMode)
				{
					case Enterprise.Core.Constants.ContainerModes.FCL:
						e.Value = new ZString(Enterprise.Core.Constants.ContainerModes.FCL);
						break;
					case Enterprise.Core.Constants.ContainerModes.BuyersConsol:
						e.Value = new ZString(Enterprise.Core.Constants.ContainerModes.FCLMixedShipper);
						break;
					case Enterprise.Core.Constants.ContainerModes.LCL:
					case Enterprise.Core.Constants.ContainerModes.FCLMixedShipper:
						if (JobContainer.Consol != null && JobContainer.Consol.JK_ConsolMode == Enterprise.Core.Constants.ContainerModes.BuyersConsol)
						{
							e.Value = new ZString(Enterprise.Core.Constants.ContainerModes.FCLMixedShipper);
						}
						break;
					default:
						e.Value = new ZString(Enterprise.Core.Constants.ContainerModes.LCL);
						break;
				}
			}
		}

		#endregion
	}
}
