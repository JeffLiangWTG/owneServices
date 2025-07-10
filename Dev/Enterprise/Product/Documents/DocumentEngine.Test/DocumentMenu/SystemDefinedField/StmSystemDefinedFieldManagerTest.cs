using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldManager))]
	sealed class StmSystemDefinedFieldManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFields()
		{
			StmSystemDefinedField field1 = Factory.New<StmSystemDefinedField>();
			StmSystemDefinedField field2 = Factory.New<StmSystemDefinedField>();

			field1.S1_BusinessContext = "Test";
			field2.S1_BusinessContext = "Consol";

			Factory.Save();

			AssertEquals("Fields.Count", 1, Manager.Fields.Count);
			AssertEquals("Fields.Contains(Field1)", true, Manager.Fields.Contains(field1));
			AssertEquals("Fields.ReadOnly", true, Manager.Fields.ReadOnly);

			AssertEquals("Fields.SortInformation.PropertyName", StmSystemDefinedFieldSchema.Constants.S1_Order, Manager.Fields.SortInformation.PropertyName);
			AssertEquals("Fields.SortInformation.Direction", ListSortDirection.Ascending, Manager.Fields.SortInformation.Direction);
		}

		public void TestCheckoutSuccessful()
		{
			var mockManager = GetNewMockManager();
			var controller = new DummySetupController();
			var manager = mockManager.Object;

			mockManager.Protected().Setup<ISetupController>("GetNewController").Returns(controller);

			AddTestFields(manager);

			string errorMessage;
			AssertEquals("IsCheckedOutByMe", false, controller.IsCheckedOutByMe);
			bool checkoutResult = manager.Checkout(out errorMessage);
			AssertEquals("IsCheckedOutByMe", true, controller.IsCheckedOutByMe);

			AssertEquals("ErrorMessage", "", errorMessage);
			AssertEquals("Manager.Checkout()", true, checkoutResult);

			AssertFieldsAreReset(manager, false);
		}

		public void TestCheckoutUnsuccessful()
		{
			var manager = MockManager.Object;

			string errorMessage;
			MockController.Setup(m => m.FullCheckOut()).Throws(new Exception("!!!"));
			bool checkoutResult = manager.Checkout(out errorMessage);
			MockController.VerifyAll();

			AssertEquals("ErrorMessage", "!!!", errorMessage);
			AssertEquals("Manager.Checkout()", false, checkoutResult);
			AssertEquals("Fields.ReadOnly", true, manager.Fields.ReadOnly);
		}

		public void TestSaveForCheckIn()
		{
			var manager = MockManager.Object;
			manager.Fields.SetReadOnlyIncludingChildren(false);

			MockController.Setup(m => m.FullSave());

			manager.SaveForCheckIn();
			MockController.VerifyAll();
			AssertEquals("Fields.ReadOnly", true, manager.Fields.ReadOnly);
		}

		public void TestUndoCheckout()
		{
			var mockManager = GetNewMockManager();
			var controller = new DummySetupController();
			var manager = mockManager.Object;
			mockManager.Protected().Setup<ISetupController>("GetNewController").Returns(controller);

			AddTestFields(manager);
			manager.Fields.SetReadOnlyIncludingChildren(false);
			controller.IsCheckedOutByMe = true;

			manager.UndoCheckout();
			AssertEquals("IsCheckedOutByMe", false, controller.IsCheckedOutByMe);
			AssertFieldsAreReset(manager, true);
		}

		public void TestEditWithoutCheckout()
		{
			var manager = MockManager.Object;

			StmSystemDefinedField field1 = manager.Fields.AddNew();
			StmSystemDefinedField field2 = manager.Fields.AddNew();

			field1.S1_Type = "GRD";
			field2.S1_Type = "TXT";

			MockController.Verify(m => m.FullCheckOut(), Times.Never);
			manager.EditWithoutCheckout();
			MockController.VerifyAll();

			AssertEquals("Fields.ReadOnly", false, manager.Fields.ReadOnly);
			AssertEquals("Field1.FieldColumns.ReadOnly", false, field1.FieldColumns.ReadOnly);
			AssertEquals("Field2.FieldColumns.ReadOnly", true, field2.FieldColumns.ReadOnly);
		}

		public void TestSaveWithoutCheckIn()
		{
			var manager = MockManager.Object;
			StmSystemDefinedField field = manager.Fields.AddNew();

			MockController.Verify(m => m.FullSave(), Times.Never);
			manager.SaveWithoutCheckIn();
			MockController.VerifyAll();

			AssertEquals("Fields.ReadOnly", true, manager.Fields.ReadOnly);
			AssertNotNull("Field should have been saved.", new BusinessObjectFactory().Load(typeof(StmSystemDefinedField), field.PK));
		}

		public void TestFactoryForSaving()
		{
			AssertEquals("FactoryForSaving", Manager.Fields.Factory, Manager.FactoryForSaving);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StmSystemDefinedFieldManager(new MockDocSupportBizO());
		}

		Mock<StmSystemDefinedFieldManager> GetNewMockManager()
		{
			var result = new Mock<StmSystemDefinedFieldManager>(new object[] { new MockDocSupportBizO() }) { CallBase = true };
			return result;
		}

		void AddTestFields(StmSystemDefinedFieldManager manager)
		{
			StmSystemDefinedField field1 = manager.Fields.AddNew();
			StmSystemDefinedField field2 = manager.Fields.AddNew();
			StmSystemDefinedField field3 = manager.Fields.AddNew();

			field1.S1_Name = "Chocolate";
			field2.S1_Name = "Cake";
			field3.S1_Name = "Apple";
			field3.S1_Type = "DAT";
		}

		void AssertFieldsAreReset(StmSystemDefinedFieldManager manager, bool readOnly)
		{
			AssertEquals(2, manager.Fields.Count);

			StmSystemDefinedField apple = (StmSystemDefinedField)manager.Fields.Find(new ZQuery(StmSystemDefinedFieldSchema.S1_Name, "Apple"))[0];
			StmSystemDefinedField orange = (StmSystemDefinedField)manager.Fields.Find(new ZQuery(StmSystemDefinedFieldSchema.S1_Name, "Orange"))[0];

			AssertEquals("Apple.S1_Type", "GRD", apple.S1_Type);
			AssertEquals("Orange.S1_Type", "TXT", orange.S1_Type);

			AssertEquals("Apple.FieldColumns.ReadOnly", readOnly, apple.FieldColumns.ReadOnly);
			AssertEquals("Orange.FieldColumns.ReadOnly", true, orange.FieldColumns.ReadOnly);

			AssertEquals("Fields.ReadOnly", readOnly, manager.Fields.ReadOnly);
		}

		Mock<StmSystemDefinedFieldManager> MockManager
		{
			get
			{
				if (fMockManager == null)
				{
					fMockManager = GetNewMockManager();
					fMockManager.Protected().Setup<ISetupController>("GetNewController").Returns(MockController.Object);
				}
				return fMockManager;
			}
		}

		Mock<ISetupController> MockController
		{
			get
			{
				if (fMockController == null)
				{
					fMockController = new Mock<ISetupController>();
				}
				return fMockController;
			}
		}

		StmSystemDefinedFieldManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = (StmSystemDefinedFieldManager)GetNewBusinessObject();
				}
				return fManager;
			}
		}

		Mock<StmSystemDefinedFieldManager> fMockManager;
		Mock<ISetupController> fMockController;
		StmSystemDefinedFieldManager fManager;

		#region class DummySetupController

		class DummySetupController : ISetupController
		{
			public DummySetupController()
			{
			}

			void ReloadData()
			{
				TestCaseHelper.ClearTable(StmSystemDefinedFieldSchema.Constants.TableName);
				BusinessObjectFactory factory = new BusinessObjectFactory();

				StmSystemDefinedField field1 = factory.New<StmSystemDefinedField>();
				StmSystemDefinedField field2 = factory.New<StmSystemDefinedField>();

				field1.S1_BusinessContext = "Test";
				field2.S1_BusinessContext = "Test";

				field1.S1_Name = "Apple";
				field2.S1_Name = "Orange";

				field1.S1_Type = "GRD";
				field2.S1_Type = "TXT";

				factory.Save();
			}

			public bool IsCheckedOutByMe;

			#region ISetupController Members

			void ISetupController.FullCheckOut()
			{
				ReloadData();
				IsCheckedOutByMe = true;
			}

			void ISetupController.FullUndoCheckOut()
			{
				ReloadData();
				IsCheckedOutByMe = false;
			}

			void ISetupController.FullSave()
			{
			}

			bool ISetupController.IsCheckedOutByMe
			{
				get { return IsCheckedOutByMe; }
			}

			#endregion
		}

		#endregion

		#endregion
	}
}
