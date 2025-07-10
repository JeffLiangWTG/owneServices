using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.OIA.Business.Testing
{
	[TestedType(typeof(OIAGLTransactionBusinessObject))]
	public class OOIAGLTransactionBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			BizObj.RunPreSaveValidation();
			AssertEquals("Empty Directory value", "HDR", BizObj.DescriptionDisplay);
			Assert("Empty From Date value", BizObj.FromDate.IsEmpty);
			Assert("Empty To Date value", BizObj.ToDate.IsEmpty);
			Assert("Empty Start GL Account PK", BizObj.StartGLAccountPK.IsEmpty);
			Assert("Empty End GL Account PK", BizObj.EndGLAccountPK.IsEmpty);
			Assert("Bizo has errors", BizObj.HasErrors);
			TestHelper.SetValidRegistryAll();
			bizObj = null;
			AssertEquals("Default value of Directory", SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.Value, BizObj.ExportDirectory);
			AssertEquals("Default value of export existing batch", true, BizObj.ExportExistingBatch);
		}

		OIAGLTransactionBusinessObject BizObj
		{
			get
			{
				if (bizObj == null)
				{
					bizObj = new OIAGLTransactionBusinessObject(Factory);
				}

				return bizObj;
			}
		}

		OIAGLTransactionBusinessObject bizObj;
		OIATestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new OIATestHelper(Factory));
			}
		}

		OIATestHelper testHelper;
	}
}
