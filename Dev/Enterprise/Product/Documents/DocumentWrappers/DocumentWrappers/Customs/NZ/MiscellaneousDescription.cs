using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class MiscellaneousDescription : DocBaseWrapper
	{
		public MiscellaneousDescription(BusinessObjectFactory factoryToWrap, ZString description)
			: base(null, factoryToWrap)
		{
			fDescription = description;
		}

		public ZString Description
		{
			get { return fDescription; }
		}

		readonly ZString fDescription;
	}
}
