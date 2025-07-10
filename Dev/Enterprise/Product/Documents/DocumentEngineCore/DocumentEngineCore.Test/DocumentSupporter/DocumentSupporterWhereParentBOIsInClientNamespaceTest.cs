using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DocumentSupporterWhereParentBOIsInClientNamespaceTest : DocumentSupporterBaseTest
	{
		public override void TestGetBODocDataProvidersForBusinessObjectDataContext()
		{
			IBODocDataProvider[] results = DocSupporter.GetBODocDataProviders(new DataContextValue(".DummyEnterpriseBusinessObject"), null);
			AssertNotNull("GetBODocDataProviders for .DummyEnterpriseBusinessObject should not be null.", results);
			AssertEquals("GetBODocDataProviders Count for .DummyEnterpriseBusinessObject", 1, results.Length);
			AssertEquals("GetBODocDataProviders for .DummyEnterpriseBusinessObject", typeof(Client.___.Testing.ClientDummyBusinessObject), BODocDataProvider.GetBusinessObject(results[0]).GetType());

			results = DocSupporter.GetBODocDataProviders(new DataContextValue(".JobDeclaration"), null);
			AssertNull("GetBODocDataProviders for .JobDeclaration should be null.", results);
		}

		#region Implementation
		protected override DummyEnterpriseBusinessObject GetNewBusinessObject()
		{
			return Factory.New<Client.___.Testing.ClientDummyBusinessObject>();
		}
		#endregion
	}
}
