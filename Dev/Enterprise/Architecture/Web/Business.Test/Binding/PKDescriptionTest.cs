using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[TestedType(typeof(PKDescription))]
	sealed class PKDescriptionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PKDescription();
		}

		public void TestProperties()
		{
			ZGuid expectedPK = ZGuid.NewZGuid();
			ZString expectedDescription = "aaaa";

			PKDescription bizO = new PKDescription(expectedPK, expectedDescription);

			AssertEquals("PK", expectedPK, bizO.PK);
			AssertEquals("Description", expectedDescription, bizO.Description);
			Assert("HyperLink will be created by default", !bizO.DoNotCreateHyperLink);
		}
	}
}
