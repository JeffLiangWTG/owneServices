using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureSetDatabaseCollection))]
	public class FeatureSetDatabaseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var featureSet = Factory.NewWithValidTestData<FeatureControlSet>();
			return featureSet.Databases;
		}
	}
}
