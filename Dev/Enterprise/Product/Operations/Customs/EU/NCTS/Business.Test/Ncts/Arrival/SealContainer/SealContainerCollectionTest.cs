using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(SealContainerCollection))]
	public class SealContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestBC_ParentTableCode()
		{
			AssertEquals(enRouteSeal.TablePrefix, sealContainerCollection.AddNew().BC_ParentTableCode);
		}

		public void TestRelationShipFilter()
		{
			var sealContainer = enRouteSeal.SealContainers.AddNew();
			var nonSealContainer = Factory.New<SealContainer>();
			nonSealContainer.BC_ParentID = enRouteSeal.PK;
			nonSealContainer.BC_ParentTableCode = enRouteSeal.TablePrefix;
			nonSealContainer.BC_TypeOfService = ContainerTypeOfServiceList.Codes.Incident;

			CombineAssertions(() =>
			{
				AssertEquals("Matches Filter", true, sealContainer.MatchesFilter(sealContainerCollection.CompleteFilter));
				AssertEquals("No Filter Match", false, nonSealContainer.MatchesFilter(sealContainerCollection.CompleteFilter));
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => sealContainerCollection;

		protected override System.Type GetExpectedCollectionType() => typeof(SealContainerCollection);

		protected override void SetUp()
		{
			base.SetUp();
			enRouteSeal = Factory.New<EnRouteSeal>();
			sealContainerCollection = new SealContainerCollection(enRouteSeal);
		}
		EnRouteSeal enRouteSeal;
		SealContainerCollection sealContainerCollection;
	}
}
