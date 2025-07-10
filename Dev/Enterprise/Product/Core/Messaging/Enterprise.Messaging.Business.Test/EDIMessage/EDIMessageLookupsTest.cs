using CargoWise.EntityFramework.Testing;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class EDIMessageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageTypeList()
		{
			AssertNotNull("lookups.MessageTypeList", lookups.MessageTypeList);
		}

		public void TestStatusList()
		{
			AssertNotNull("lookups.StatusList", lookups.StatusList);
		}

		public void TestApplicationList()
		{
			AssertNotNull("lookups.ApplicationList", lookups.ApplicationList);
		}

		public void TestCompaniesList()
		{
			AssertNotNull("lookups.Companies", lookups.Companies);
		}

		#region Implementation

		EDIMessageLookups lookups
		{
			get { return EDIMessageTestFactory.New(Factory).Lookups; }
		}

		#endregion
	}
}
