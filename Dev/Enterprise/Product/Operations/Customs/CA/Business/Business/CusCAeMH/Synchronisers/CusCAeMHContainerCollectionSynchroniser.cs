using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHContainerCollectionSynchroniser : GenericCollectionSynchroniser<CusCAeMHContainerSynchroniser>
	{
		public CusCAeMHContainerCollectionSynchroniser(ForwardingConsol consol, CusCAeMHMaster destination)
			: base(consol, destination, consol.Containers, destination.Containers)
		{
		}

		protected new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		#region Implementation

		protected override bool CompareBizosEqual(BusinessObject source, BusinessObject destination)
		{
			return ((ForwardingContainer)source).JC_ContainerNum == ((CusCAeMHContainer)destination).BQ_ContainerNumber;
		}

		protected override CusCAeMHContainerSynchroniser CreateNewSynchroniser(BusinessObject destination, BusinessObject source)
		{
			return new CusCAeMHContainerSynchroniser((CusCAeMHContainer)destination, (ForwardingContainer)source);
		}

		protected override void DeleteBizo(BusinessObject bizo)
		{
			var destinationContainer = (CusCAeMHContainer)bizo;
			if (!destinationContainer.IsNonContainerized)
			{
				destinationContainer.Delete();
			}
			else
			{
				bool consolContainsContainer = Source?.Containers.Any() ?? ZBool.False;
				if (consolContainsContainer)
				{
					destinationContainer.Delete();
				}
			}
		}

		#endregion
	}
}
