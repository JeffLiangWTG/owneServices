using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Sailing
{
	[TestedType(typeof(DocVesselCollection))]
	sealed class DocVesselCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocVesselCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var refVessel = Factory.New<RefVessel>();
			return DocVessel.New(refVessel, Factory);
		}

		protected override DocVesselCollection GetCollectionToTest()
		{
			return new DocVesselCollection(Factory);
		}
	}
}
