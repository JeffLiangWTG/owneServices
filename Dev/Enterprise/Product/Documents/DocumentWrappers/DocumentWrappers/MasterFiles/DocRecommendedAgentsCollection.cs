using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocRecommendedAgentsCollection : DocOrganisationCollection
	{
		public DocRecommendedAgentsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocRecommendedAgentsCollection(BusinessObjectFactory factory, DocUNLOCO loco, ZString transportMode, ZString direction)
			: base(factory)
		{
			this.Loco = loco;
			this.TransportMode = transportMode;
			this.Direction = direction;
		}

		#region Load

		public override void Load()
		{
			this.RemoveAll();

			if (Loco != null)
			{
				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.Rail:
					case Core.Constants.TransportModes.Road:
					case Core.Constants.TransportModes.Sea:
						AddPublishedAgent(TransportMode);
						break;

					case Core.Constants.TransportModes.All:
						AddPublishedAgent(Core.Constants.TransportModes.Air);
						AddPublishedAgent(Core.Constants.TransportModes.Rail);
						AddPublishedAgent(Core.Constants.TransportModes.Road);
						AddPublishedAgent(Core.Constants.TransportModes.Sea);
						break;
				}
			}
		}

		#endregion

		#region Implementation

		void AddPublishedAgent(ZString transportMode)
		{
			OrgAddress agent = Loco.RefUNLOCO.GetPublishedAgent(transportMode, Direction);
			if (agent != null)
			{
				this.Add(DocOrganisation.New(agent, Factory));
			}
		}

		readonly DocUNLOCO Loco;
		readonly ZString TransportMode;
		readonly ZString Direction;

		#endregion
	}
}
