using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class CalloutTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
		}

		#region Property Overrides

		public void TestTypeOfProcessQueue()
		{
			AssertEquals(typeof(UPECalloutQueue), TestHelper.TestCallout.CurrentQueue.GetType());
		}

		public void TestReadOnlys()
		{
			AssertEquals("Should be read-only", true, TestHelper.TestCallout.ShipmentTypeInfo.ReadOnly);
			AssertEquals("Should be read-only", true, TestHelper.TestCallout.DutyTypeInfo.ReadOnly);
			AssertEquals("Should be read-only", true, TestHelper.TestCallout.BillingTermsInfo.ReadOnly);
		}

		#endregion

		#region Read Only
		public void TestSetHAWBDetailsReadOnlyOnLoaded()
		{
			AssertNotNull("Must access this wrapped property for test", TestHelper.TestCallout.ZPropertyInfoHash["CurrentQueue+" + ProcessQueueSchema.Constants.P4_SubStatus]);
			AssertEquals("Pre-condition, should not be read-only before OnLoaded is called", false, TestHelper.TestCallout.CS_GoodsDescriptionInfo.ReadOnly);

			TestHelper.TestCallout.OnLoaded();
			AssertEquals("Properties on the ProcessQueue should not be made read-only", false, TestHelper.TestCallout.CurrentQueue.P4_SubStatusInfo.ReadOnly);
			foreach (ZPropertyInfo propertyInfo in TestHelper.TestCallout.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				if (propertyInfo != TestHelper.TestCallout.IsHoldForCollectionInfo &&
					propertyInfo != TestHelper.TestCallout.IsRedirectedInfo &&
					propertyInfo != TestHelper.TestCallout.CS_ConsigneeContactNameInfo &&
					propertyInfo != TestHelper.TestCallout.CS_ConsigneePhoneInfo &&
					propertyInfo != TestHelper.TestCallout.IsExcludedFromBISIWarningInfo)
				{
					AssertEquals("Property should be read only; " + propertyInfo.Name, true, propertyInfo.ReadOnly);
				}
				else
				{
					AssertEquals("Property should not be read only; " + propertyInfo.Name, false, propertyInfo.ReadOnly);
				}
				AssertEquals("CS_ShipmentTypeForBinding should be readonly", true, TestHelper.TestCallout.CS_ShipmentTypeForBindingInfo.ReadOnly);
				AssertEquals("CS_FreightPrepaidCollectForBinding should be readonly", true, TestHelper.TestCallout.CS_FreightPrepaidCollectForBindingInfo.ReadOnly);
			}
		}

		public void TestHoldForCollectionNotReadOnly()
		{
			Assert("IsHoldForCollection must not be read-only", !TestHelper.TestCallout.IsHoldForCollectionInfo.ReadOnly);
		}
		#endregion

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper(Factory)); }
		}
		UPETestHelper testHelper;
	}
}
