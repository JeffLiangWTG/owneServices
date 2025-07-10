using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.AutoDeploy.Business.Test
{
	[TestedType(typeof(UpgradesToClientCollection))]
	class UpgradesToClientCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<UpgradesToClient>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new UpgradesToClientCollection(Factory);
		}
	}
}
