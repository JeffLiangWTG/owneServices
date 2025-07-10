using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerForPlugInTest : DummyController, IPluginControllerBusinessObjectProvider
	{
		public override ControllerID ID => DummyControllerIDs.DummyForPlugIn;

		public IBusiness LoadBusinessEntityForPlugIn(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return factory.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Guid, sourceEntityPK));
		}
	}
}
