using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWithPlugInAndNavigationProvider : DummyController, IPluginControllerBusinessObjectProvider, INavigationControllerIDProvider
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerWithPlugInAndNavigationProvider;

		public IBusiness LoadBusinessEntityForPlugIn(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return factory.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Guid, sourceEntityPK));
		}

		public bool ShouldLoadBusinessObject { get => true; }

		public ControllerID GetValidControllerID(object dataSource)
		{
			return DummyControllerIDs.Dummy;
		}
	}
}
