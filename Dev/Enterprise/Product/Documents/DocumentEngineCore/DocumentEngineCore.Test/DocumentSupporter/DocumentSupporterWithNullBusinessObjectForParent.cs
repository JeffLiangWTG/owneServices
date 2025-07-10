using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DocumentSupporterWithNullBusinessObjectForParent : DocumentSupporterBaseTest
	{
		public override void TestGetBODocDataProvidersForDocumentWrapperDataContext()
		{
			IBODocDataProvider[] results = DocSupporter.GetBODocDataProviders(new DataContextValueForTesting(DataContext.UnitTest), null);
			AssertNull("GetBODocDataProviders for DataContext.Declaration", results);

			results = DocSupporter.GetBODocDataProviders(new DataContextValueForTesting(DataContext.Declaration), null);
			AssertNull("GetBODocDataProviders for DataContext.Declaration", results);
		}

		public override void TestGetBODocDataProvidersForBusinessObjectDataContext()
		{
			IBODocDataProvider[] results = DocSupporter.GetBODocDataProviders(new DataContextValue(".DummyBusinessObject"), null);
			AssertNull("GetBODocDataProviders for .JobDeclaration should be null.", results);

			results = DocSupporter.GetBODocDataProviders(new DataContextValue(".JobDeclaration"), null);
			AssertNull("GetBODocDataProviders for .JobDeclaration should be null.", results);
		}

		public void TestIsNonPersistent()
		{
			AssertEquals(true, DocSupporter.IsNonPersistent);
		}

		#region Implementation

		protected override DummyEnterpriseBusinessObject GetNewBusinessObject()
		{
			return Factory.GetNull<DummyEnterpriseBusinessObject>();
		}

		#endregion
	}
}
