using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoDepotTally))]
	sealed class SeaCargoDepotTallyTest : SeaCargoDepotNonPersistantBusineesObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			SeaCargoDepotTally result = SeaCargoDepotTally.Load(container);
			return result;
		}

		CFSContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<CFSContainer>();
			container.JC_OH_CFSClient = SomeForwarder().PK;
		}
	}
}
