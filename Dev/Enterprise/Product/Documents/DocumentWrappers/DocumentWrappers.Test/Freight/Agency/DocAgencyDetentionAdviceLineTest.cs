using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	internal abstract class DocAgencyDetentionAdviceLineTest : GenericWrapperTest
	{
		#region Implementation

		protected JobSailing Sailing
		{
			get
			{
				if (sailing == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
					voyage.GenerateSailings();
					sailing = voyage.Sailings[0];
				}
				return sailing;
			}
		}
		JobSailing sailing;

		protected JobVoyage Voyage
		{
			get { return voyage ?? (voyage = Sailing.Voyage); }
		}
		JobVoyage voyage;

		protected RefContainerStock Stock
		{
			get { return stock ?? (stock = Factory.New<RefContainerStock>()); }
		}
		RefContainerStock stock;

		protected ContainerMovement Movement
		{
			get { return movement ?? (movement = Stock.Movements.AddNew()); }
		}
		ContainerMovement movement;

		#endregion
	}
}
