using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing
{
	class AmendmentObjectTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var amendmentObject = new AmendmentObject(null, null);
			AssertNullOrEmpty("Property type", amendmentObject.AmendmentType);
			AssertNull("Property pointers", amendmentObject.Pointers);

			var type = "Type";
			var pointers = new ZString[] { "pointer1", "pointer2" };
			amendmentObject = new AmendmentObject(type, pointers);
			AssertEquals("Property type", type, amendmentObject.AmendmentType);
			AssertEquals("Property pointers", pointers, amendmentObject.Pointers);
		}
	}
}
