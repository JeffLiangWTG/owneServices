using Enterprise.ZArchitecture.Business.Testing;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DocumentSupporterWithNullBOAdditionalBODataSourceTest : DocumentSupporterWithConcreteAdditionalBODataSourceTest
	{
		public override void TestGetBODocDataProvidersForBusinessObjectDataContext()
		{
			var results = DocSupporter.GetBODocDataProviders(new DataContextValue(".DummyTradeObject"), null);
			AssertNull("GetBODocDataProviders for .DummyTradeObject should be null.", results);
		}

		public override void TestGetBODocDataProvidersForDocumentWrapperDataContext()
		{
			var results = DocSupporter.GetBODocDataProviders(new DataContextValueForTesting(DataContext.Dummy), null);
			AssertNull("GetBODocDataProviders for DataContext.Dummy", results);
		}

		#region Implementation

		protected override DocumentSupporterForTesting GetNewDocSupporter(DummyEnterpriseBusinessObject parentBusinessObject)
		{
			return new DocumentSupporterWithNullBOAdditionalBODataSource(parentBusinessObject);
		}

		protected class DocumentSupporterWithNullBOAdditionalBODataSource : DocumentSupporterWithConcreteAdditionalBODataSource
		{
			public DocumentSupporterWithNullBOAdditionalBODataSource(DummyEnterpriseBusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
			}

			protected override DummyTradeObject AdditionalDocumentSupporterParent
			{
				get { return Factory.GetNull<DummyTradeObject>(); }
			}
		}

		#endregion
	}
}
