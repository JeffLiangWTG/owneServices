using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocTallyContainer : DocPackUnpackContainerRego
	{
		DocTallyContainer(TallyContainer tallyContainer, BusinessObjectFactory factoryToWrap)
			: base(tallyContainer, factoryToWrap)
		{
		}

		public static DocTallyContainer New(TallyContainer tallyContainer, BusinessObjectFactory factoryToWrap)
		{
			if (tallyContainer == null)
			{
				return null;
			}
			else
			{
				return new DocTallyContainer(tallyContainer, factoryToWrap);
			}
		}

		#region Properties

		public ZInt TotalShipments
		{
			get { return TallyContainer.TotalShipments; }
		}

		#endregion

		#region Related Objects

		public new DocShipmentCollection ContainerShipments
		{
			get { return base.ContainerShipments; }
		}

		TallyContainer TallyContainer
		{
			get { return (TallyContainer)WrappedObject; }
		}

		#endregion
	}
}
