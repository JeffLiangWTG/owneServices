using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaContainerLookups : ASYCUDA.Business.AsycudaContainerLookups
	{
		public AsycudaContainerLookups(AsycudaContainer parent) : base(parent)
		{
		}

		public ICollection VanningLocationCodeCollection => JPRefCusCodeListTypes.GetJapanBondedAreaCodes(Factory, Parent.Header?.AMA_TransportMode ?? ZString.Empty);
	}
}
