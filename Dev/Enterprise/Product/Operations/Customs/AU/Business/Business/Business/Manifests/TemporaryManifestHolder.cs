using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TemporaryManifestHolder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TemporaryManifestHolder(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TemporaryManifestsCollection Manifests
		{
			get
			{
				if (manifests == null)
				{
					manifests = new TemporaryManifestsCollection(Factory);
				}
				return manifests;
			}
		}
		TemporaryManifestsCollection manifests;

		public ZString VesselName { get; set; }

		public ZString VoyageNumber { get; set; }
	}
}
