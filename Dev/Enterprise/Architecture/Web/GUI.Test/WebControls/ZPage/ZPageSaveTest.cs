using System.Data;
using System.Linq;
using System.Web.UI;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZPageSaveTest : WebControlTest
	{
		#region TestSaveDataSourceValidation

		public void TestSaveDataSourceValidation()
		{
			TestPage.LoadOrCreateDataSource();

			DummyBusinessObject testBizO = TestPage.DataSource as DummyBusinessObject;

			testBizO.Z0_Guid = ZGuid.Invalid;
			testBizO.RunPreSaveValidation();
			AssertEquals("TestBizO should have error", true, testBizO.HasErrors);
			bool result = TestPage.SaveDataSourceFactory();
			AssertEquals("SaveDataSourceFactory should have failed", false, result);
			AssertEquals("DataSource should not be in database", false, testBizO.IsInDatabase);

			testBizO.Z0_Guid = ZGuid.NewZGuid();
			testBizO.Z0_Date = ZDateTime.Now.AddHours(-1);
			testBizO.RunPreSaveValidation();
			AssertEquals("TestBizO should not have an error", false, testBizO.HasErrors);
			AssertEquals("TestBizO should have message error", true, testBizO.HasMessageErrors);
			result = TestPage.SaveDataSourceFactory();
			AssertEquals("SaveDataSourceFactory should have failed", false, result);
			AssertEquals("DataSource should not be in database", false, testBizO.IsInDatabase);

			TestPage.NotificationFlags.DisplayMessageErrors = false;
			AssertEquals("TestBizO should have message error", true, testBizO.HasMessageErrors);
			result = TestPage.SaveDataSourceFactory();
			AssertEquals("SaveDataSourceFactory should have succeeded", true, result);
			AssertEquals("DataSource should be in database now", true, testBizO.IsInDatabase);
		}
		#endregion

		#region TestSaveConcurrencyException

		public void TestSaveConcurrencyException()
		{
			TestPage.LoadOrCreateDataSource();

			var dummy = TestPage.DataSource as DummyBusinessObject;
			var result = TestPage.SaveDataSourceFactory();
			AssertEquals("SaveDataSourceFactory should have succeeded", true, result);
			AssertEquals("DataSource should be in database", true, dummy.IsInDatabase);

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummyInOtherFactory = otherFactory.Load<DummyBusinessObject>(dummy.PK);
			dummyInOtherFactory.Z0_Code = "XXX";
			otherFactory.Save();

			dummy.Z0_Code = "YYY";
			result = TestPage.SaveDataSourceFactory();
			AssertEquals("SaveDataSourceFactory should have failed", false, result);
			AssertEquals("BizO should have notifications", 2, dummy.Notifications.Count());
			AssertContains("Error message contains concurrency exception text", "While you have been working with this form, another user has made changes.", dummy.Notifications.GetFirst().Message);
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			TestPage.LoadOrCreateDataSource();
			DummyBusinessObjectMessageErrorTesting testBizO = TestPage.DataSource as DummyBusinessObjectMessageErrorTesting;

			testBizO.Z0_Date = ZDateTime.Now.AddHours(-1);
			AssertEquals("PreCondition: No validation should have been run", false, testBizO.ValidationCalled);
			TestPage.Validate("");
			AssertEquals("Validation should have been run on datasource", true, testBizO.ValidationCalled);
		}

		#endregion
		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZSaveTestPage(Factory);
		}

		ZPage TestPage
		{
			get { return (ZPage)Control; }
		}
		#endregion Implementation

		#region DummyBusinessObjectMessageErrorTesting

		public class DummyBusinessObjectMessageErrorTesting : DummyBusinessObject
		{
			protected override void RunPreSaveValidationCore()
			{
				ValidationCalled = true;
				base.RunPreSaveValidationCore();
			}
			public bool ValidationCalled;

			public DummyBusinessObjectMessageErrorTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override DummyBizoValidation GetNewValidation()
			{
				return new DummyBusinessObjectMessageErrorValidation(this);
			}

			protected bool Z0_Date_ReadOnly
			{
				get { return Z0_VarCharMax == "READONLY"; }
			}
		}

		public class DummyBusinessObjectMessageErrorValidation : DummyBizoValidation
		{
			public DummyBusinessObjectMessageErrorValidation(AutoDummyBizo parent) : base(parent)
			{
			}

			protected override void CheckZ0_Date()
			{
				if (!Parent.Z0_Date.IsEmpty && Parent.Z0_Date < ZDateTime.Now)
				{
					Parent.Z0_DateInfo.AddMessageError("Z0_Date should be in the future");
				}
			}
		}

		public class ZSaveTestPage : ZTestPage
		{
			public ZSaveTestPage(BusinessObjectFactory factoryForTesting) : base()
			{
				this.FactoryForTesting = factoryForTesting;
			}
			readonly BusinessObjectFactory FactoryForTesting;

			protected override BusinessObject GetNewDataSource()
			{
				return Factory.New<DummyBusinessObjectMessageErrorTesting>();
			}

			protected override BusinessObjectFactory GetNewFactory()
			{
				return FactoryForTesting;
			}
		}

		#endregion
	}
}
