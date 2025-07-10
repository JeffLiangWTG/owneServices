using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAgencyContainerCollection))]
	sealed class DocAgencyContainerCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocAgencyContainerCollection>
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipment>().BookedContainers.AddNew();
			return DocAgencyContainer.New(container, Factory);
		}

		protected override DocAgencyContainerCollection GetCollectionToTest()
		{
			return new DocAgencyContainerCollection(Factory);
		}

		#endregion
	}
}
