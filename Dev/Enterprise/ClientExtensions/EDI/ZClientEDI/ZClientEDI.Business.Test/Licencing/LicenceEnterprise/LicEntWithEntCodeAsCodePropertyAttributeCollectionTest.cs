using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceEnterpriseCollectionForEntCodeFilter))]
	public class LicEntWithEntCodeAsCodePropertyAttributeCollectionTest : ActiveBusinessObjectCollectionTestCase<LicenceEnterpriseCollectionForEntCodeFilter>
	{
		public void TestCodeProperty()
		{
			var collection = new LicenceEnterpriseCollectionForEntCodeFilter(Factory);
			var ent = collection.AddNew();
			AssertEquals("LE_EnterpriseCode", CodePropertyAttribute.CodePropertyNameFromType(ent.GetType()));
		}
	}
}
