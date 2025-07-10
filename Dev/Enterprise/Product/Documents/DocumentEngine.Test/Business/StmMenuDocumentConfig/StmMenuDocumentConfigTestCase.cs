using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.DocumentEngine.Business.Testing
{
	abstract class StmMenuDocumentConfigTestCase<T> : EnterpriseBusinessObjectTestCase where T : StmMenuDocumentConfig
	{
		protected T DocConfig
		{
			get { return fDocConfig ?? (fDocConfig = (T)GetNewBusinessObject()); }
		}
		T fDocConfig;

		public void TestDelete()
		{
			StmMenuDocumentConfigItem configItem1 = DocConfig.ConfigItems.AddNew();
			StmMenuDocumentConfigItem configItem2 = DocConfig.ConfigItems.AddNew();
			DocConfig.Delete();
			AssertEquals("configItem1.IsDeleted", true, configItem1.IsDeleted);
			AssertEquals("configItem2.IsDeleted", true, configItem2.IsDeleted);
		}

		public abstract void TestGetRelatedDocConfigs();
	}
}
