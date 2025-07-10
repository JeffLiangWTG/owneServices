using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZGuidFindBoxTest : BaseZGuidFindBoxTest
	{
		public void TestGuid()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob";

			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummyChild2 = dummy2.Collection.AddNew();
			dummyChild2.Z0_Code = "Bob";

			Factory.Save();

			using (var findBox = new ZGuidFindBox())
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;

				findBox.CodeBox.Text = string.Empty;
				AssertEquals(ZGuid.Empty, findBox.Guid);

				var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild2 });
				AssertEquals(dummyChild2.PK, findBox.Guid);

				findBox.List = dummy1.Collection;
				findBox.CodeBox.Text = "XXX";
				AssertEquals(ZGuid.Invalid, findBox.Guid);

				findBox.CodeBox.Text = "Bob";
				AssertEquals(dummyChild1.PK, findBox.Guid);
			}
		}

		public void TestPopupSelectedPrimaryKeyFromCodeRequired()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob1";

			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummyChild2 = dummy2.Collection.AddNew();
			dummyChild2.Z0_Code = "Bob2";
			Factory.Save();

			using (var form = new ZForm())
			using (var findBox = new ZGuidFindBox())
			{
				form.Controls.Add(findBox);
				form.Show();
				findBox.IsPrimaryKeyFromCodeRequired = true;
				findBox.ModuleID = DummyModuleIDs.Dummy;
				findBox.List = new DummyBusinessObjectCollectionWithRelationShipFilter(Factory);
				findBox.CodeBox.Text = string.Empty;
				AssertEquals(ZGuid.Empty, findBox.Guid);

				//Set to Bob of dummy2 from the popup
				var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild2 });
				AssertEquals("BOB2", findBox.CodeBox.Text);

				UserIdleWorker.Flush();
				AssertEquals("Description after Selection", "{Invalid Selection}", findBox.DescriptionBox.Text);
			}
		}

		class DummyBusinessObjectCollectionWithRelationShipFilter : BusinessObjectCollection<DummyBusinessObject>
		{
			public DummyBusinessObjectCollectionWithRelationShipFilter(BusinessObjectFactory factory) : base(factory)
			{
			}
			protected override ZQuery CreateRelationshipFilter()
			{
				return new ZQuery().AddToFilter(DummyBizoSchema.PK, SQLComparisonOperator.Equal, Guid.NewGuid());
			}
		}

		public void TestDescriptionFromTheSameCode()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "Bob";
			dummy1.Z0_Description = "First Bob";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "Bob";
			dummy2.Z0_Description = "Second Bob";

			Factory.Save();

			using (var form = new ZForm())
			using (var findBox = new ZGuidFindBox())
			{
				form.Controls.Add(findBox);
				form.Show();
				findBox.ModuleID = DummyModuleIDs.Dummy;
				findBox.List = new DummyBusinessObjectCollection(Factory);

				findBox.CodeBox.Text = string.Empty;
				AssertEquals(ZGuid.Empty, findBox.Guid);

				findBox.CodeBox.Text = "Bob";
				AssertEquals(dummy1.PK, findBox.Guid);
				UserIdleWorker.Flush();
				AssertEquals(dummy1.Z0_Description, findBox.DescriptionBox.Text);

				//Set to Bob of dummy2 from the popup
				findBox.CodeBox.Text = "Bob";
				var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummy2 });

				AssertEquals(dummy2.PK, findBox.Guid);
				UserIdleWorker.Flush();
				AssertEquals(dummy2.Z0_Description, findBox.DescriptionBox.Text);
			}
		}

		class DummyBusinessObjectWithReference : DummyBusinessObject
		{
			public DummyBusinessObjectWithReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[List("Collection")]
			public ZGuid OtherBizOPK
			{
				get
				{
					return otherBizOPK;
				}
				set
				{
					otherBizOPK = value;
				}
			}
			ZGuid otherBizOPK;
		}

		public void TestGuidWithSameCodeStillRefreshesBinding()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "BOB";

			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummyChild2 = dummy2.Collection.AddNew();
			dummyChild2.Z0_Code = "BOB";

			Factory.Save();

			using (var testForm = new TestForm())
			using (var findBox = new ZGuidFindBox())
			using (var otherControl = new ZGuidFindBox())
			{
				var formBusinessObject = Factory.New<DummyBusinessObjectWithReference>();
				testForm.DataSourceType = typeof(DummyBusinessObjectWithReference);
				testForm.DataSource = formBusinessObject;
				findBox.ModuleID = DummyModuleIDs.Dummy;
				testForm.SetDataBinding(formBusinessObject, "");
				findBox.SetDataBinding(formBusinessObject, "OtherBizOPK");
				testForm.Controls.Add(findBox);
				testForm.Controls.Add(otherControl);

				testForm.Show();
				Application.DoEvents();

				findBox.CodeBox.Text = string.Empty;
				findBox.CodeBox.Focus();
				otherControl.CodeBox.Focus();
				AssertEquals(ZGuid.Empty, findBox.Guid);
				AssertEquals(ZGuid.Empty, formBusinessObject.OtherBizOPK);

				findBox.List = dummy1.Collection;
				findBox.CodeBox.Text = "XXX";
				findBox.CodeBox.Focus();
				otherControl.CodeBox.Focus();
				AssertEquals(ZGuid.Invalid, findBox.Guid);

				findBox.CodeBox.Text = "BOB";
				findBox.CodeBox.Focus();
				otherControl.CodeBox.Focus();
				AssertEquals(dummyChild1.PK, findBox.Guid);
				AssertEquals(dummyChild1.PK, formBusinessObject.OtherBizOPK);

				var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);

				AssertEquals("BOB", findBox.CodeBox.Text);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild2 });
				AssertEquals("BOB", findBox.CodeBox.Text);
				findBox.CodeBox.Focus();
				otherControl.CodeBox.Focus();
				AssertEquals(dummyChild2.PK, findBox.Guid);
				AssertEquals(dummyChild2.PK, formBusinessObject.OtherBizOPK);

				AssertEquals("BOB", findBox.CodeBox.Text);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild1 });
				AssertEquals("BOB", findBox.CodeBox.Text);
				findBox.CodeBox.Focus();
				otherControl.CodeBox.Focus();
				AssertEquals(dummyChild1.PK, findBox.Guid);
				AssertEquals(dummyChild1.PK, formBusinessObject.OtherBizOPK);

				AssertEquals("BOB", findBox.CodeBox.Text);
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild2 });
				AssertEquals("BOB", findBox.CodeBox.Text);
				findBox.CodeBox.Focus();
				otherControl.CodeBox.Focus();
				AssertEquals(dummyChild2.PK, findBox.Guid);
				AssertEquals(dummyChild2.PK, formBusinessObject.OtherBizOPK);

				findBox.List = dummy1.Collection;
				findBox.CodeBox.Text = "XXX";
				findBox.CodeBox.Focus();
				otherControl.CodeBox.Focus();
				AssertEquals(ZGuid.Invalid, findBox.Guid);

				findBox.CodeBox.Text = "BOB";
				findBox.CodeBox.Focus();
				otherControl.CodeBox.Focus();
				AssertEquals(dummyChild1.PK, findBox.Guid);
				AssertEquals(dummyChild1.PK, formBusinessObject.OtherBizOPK);
			}
		}

		public void TestGetMaxLengthFromListProvider()
		{
			// null provider
			AssertEquals(-1, ZGuidFindBox.GetMaxLengthFromListProvider(null));

			// null list
			var listProviderMock = new Mock<IFindBoxListProvider>(MockBehavior.Strict);
			listProviderMock.SetupGet(o => o.List).Returns((IBusinessObjectCollection)null).Verifiable();
			AssertEquals(-1, ZGuidFindBox.GetMaxLengthFromListProvider(listProviderMock.Object));
			listProviderMock.Verify();

			// normal list with schema column
			var collection = new DummyBusinessObjectCollection(Factory);
			listProviderMock.SetupGet(o => o.List).Returns(collection).Verifiable();
			AssertEquals(5, ZGuidFindBox.GetMaxLengthFromListProvider(listProviderMock.Object));
			listProviderMock.Verify();

			// list with no schema column and no elements
			var bizObj = Factory.New<TestBusinessObject>();
			var listMock = new Mock<BusinessObjectCollection>(Factory);
			listMock.Setup(o => o.GetTypeOfElementsFromPK(It.IsAny<ZGuid>())).Returns(typeof(TestBusinessObject));
			listProviderMock.SetupGet(o => o.List).Returns(listMock.Object);
			listProviderMock.Setup(o => o.NearestMatch(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(("XXX", true));
			listProviderMock.Setup(o => o.GetBusinessObjectFromCode("XXX")).Returns(bizObj);
			AssertEquals(99, ZGuidFindBox.GetMaxLengthFromListProvider(listProviderMock.Object));
			listProviderMock.Verify();
			listMock.Verify();

			// list with no schema column but with elements
			listProviderMock = new Mock<IFindBoxListProvider>(MockBehavior.Strict);
			listProviderMock.SetupGet(o => o.List).Returns(listMock.Object);
			listProviderMock.Setup(o => o.NearestMatch(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(("XXX", true));
			listProviderMock.Setup(o => o.GetBusinessObjectFromCode("XXX")).Returns(bizObj);
			listMock.Object.Add(bizObj);
			listMock.Setup(o => o.GetTypeOfElementsFromPK(It.IsAny<ZGuid>())).Returns(typeof(TestBusinessObject));
			AssertEquals(99, ZGuidFindBox.GetMaxLengthFromListProvider(listProviderMock.Object));
			listProviderMock.Verify();
			listMock.Verify();
		}

		public void TestGetMaxLengthFromActiveBOListProvider()
		{
			var prov = new MyFindBoxListProvider();
			AssertNoExceptionThrown(delegate
			{ ZGuidFindBox.GetMaxLengthFromListProvider(prov); });
			AssertEquals("Should get correct max length", 99, ZGuidFindBox.GetMaxLengthFromListProvider(prov));
			Assert("Collection shouldn't be loaded.", !prov.List.IsLoaded);
		}

		#region Test classes

		[CodeProperty("Code")]
		class TestBusinessObject : DummyBusinessObject
		{
			public TestBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[MaxLength(99)]
			public ZString Code
			{
				get { return null; }
			}
		}

		[CodeProperty("Code")]
		class TestDependantBusinessObject : DummyDependantBusinessObject
		{
			public TestDependantBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[MaxLength(99)]
			public ZString Code
			{
				get { return null; }
			}
		}

		class MyFindBoxListProvider : IFindBoxListProvider
		{
			#region IFindBoxListProvider Members

			public bool AutoCompleteOnCommit
			{
				get { throw new NotImplementedException(); }
			}

			public string CodeFromPrimaryKey(ZGuid pK)
			{
				throw new NotImplementedException();
			}

			public string DescriptionFromCode(string code)
			{
				throw new NotImplementedException();
			}

			public string DescriptionFromPrimaryKey(ZGuid pK)
			{
				throw new NotImplementedException();
			}

			public ICodeDescription GetCustomCodeDescription(BusinessObject bizo)
				=> bizo;

			public IEnumerable<BusinessObject> GetBusinessObjectsFromCode(string code)
			{
				return new BusinessObject[] { factory.New<TestDependantBusinessObject>() };
			}

			public IEnumerable<BusinessObject> GetBusinessObjectsFromCodeWithoutFilter(string code)
			{
				return new BusinessObject[] { factory.New<TestDependantBusinessObject>() };
			}

			public BusinessObject GetBusinessObjectFromCode(string code)
			{
				return factory.New<TestDependantBusinessObject>();
			}

			public BusinessObject GetBusinessObjectFromCodeWithoutFilter(string code)
			{
				return factory.New<TestDependantBusinessObject>();
			}

			readonly BusinessObjectFactory factory = new BusinessObjectFactory();

			TestBusinessObject master;
			public TestBusinessObject Master
			{
				get
				{
					if (master == null)
					{
						master = factory.New<TestBusinessObject>();
					}
					return master;
				}
			}

			ActiveBusinessObjectCollection<TestDependantBusinessObject> list;
			public IBusinessObjectCollection List
			{
				get
				{
					if (list == null)
					{
						list = new ActiveBusinessObjectCollection<TestDependantBusinessObject>(Master);
					}
					return list;
				}
			}

			public int NearestMatchCount;
			public (string, bool) NearestMatch(string code, bool explicitAutoComplete, int cursor)
			{
				NearestMatchCount++;
				return (string.Empty, false);
			}

			public (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				NearestMatchCount++;
				return (string.Empty, false);
			}

			public ZGuid PrimaryKeyFromCode(string code)
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		#endregion

		public void TestInvalidCodeAndDescription()
		{
			DeleteAllDummies();
			CreateDummies();
			Dummy = Factory.New<FindBoxDummyBusinessObject>();

			using (var testForm = new ZChildForm())
			{
				IsBindingToDetail = true;
				CreateControls(testForm, "");

				testForm.Show();
				ZGuidFindBox.CodeBox.Focus();

				AssertEquals("List after Show", Dummy.Dummies, ZGuidFindBox.List);
				AssertEquals("Code after Show", "", ZGuidFindBox.CodeBox.Text);
				UserIdleWorker.Flush();
				AssertEquals("Description after Show", "{None Selected}", ZGuidFindBox.DescriptionBox.Text);
				AssertEquals("ReadOnly on empty collection", true, ZGuidFindBox.ReadOnly);

				var newDummy = Dummy.Collection.AddNew();
				AssertEquals("Not ReadOnly on not-empty collection", false, ZGuidFindBox.ReadOnly);

				newDummy.Z0_Guid = Dummy2.PK;
				AssertEquals("Code", Dummy2.Z0_Code, ZGuidFindBox.CodeBox.Text);
				UserIdleWorker.Flush();
				AssertEquals("Description", Dummy2.Z0_Description, ZGuidFindBox.DescriptionBox.Text);

				ZGuidFindBox.CodeBox.Text = "INVLD";
				ChangeFocusToInvokeBinding();

				AssertEquals("Bound Value", ZGuid.Invalid, newDummy.Z0_Guid);
				AssertEquals("Invalid Code", "INVLD", FieldInvalidTextMemory.GetInvalidText(newDummy, "Z0_Guid"));
				UserIdleWorker.Flush();
				AssertEquals("Description after New", "{Invalid Selection}", ZGuidFindBox.DescriptionBox.Text);

				var newDummy2 = Dummy.Collection.AddNew();
				ZGuidFindBox.DataBindings["ReadOnlyForBinding"].BindingManagerBase.Position++;

				AssertEquals("Code after Position++", "", ZGuidFindBox.CodeBox.Text);
				UserIdleWorker.Flush();
				AssertEquals("Description after Position++", "{None Selected}", ZGuidFindBox.DescriptionBox.Text);

				ZGuidFindBox.DataBindings["ReadOnlyForBinding"].BindingManagerBase.Position--;
				AssertEquals("Code after Position--", "INVLD", ZGuidFindBox.CodeBox.Text);
				UserIdleWorker.Flush();
				AssertEquals("Description after Position--", "{Invalid Selection}", ZGuidFindBox.DescriptionBox.Text);
			}
		}

		public void TestInvalidGuidFromBeforeBinding()
		{
			CreateDummies();

			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "");

				Dummy.SS_DummyGuid = ZGuid.Invalid;

				testForm.Show();
				ZGuidFindBox.CodeBox.Focus();

				AssertEquals("Code", "", ZGuidFindBox.CodeBox.Text);
			}
		}

		public void TestMaxLength()
		{
			var testDummy = Factory.New<MaxLengthDummyBizO>();
			Dummy = testDummy;

			testDummy.BoundCollectionToUse = testDummy.Dummies;

			using (var testForm = new ZChildForm())
			{
				IsBindingForMaxLengthTest = true;
				CreateControls(testForm, "");

				testForm.Show();
				AssertEquals("Maxlength of codebox", DummyBizoSchema.Z0_Code.MaxLength, ZGuidFindBox.CodeBox.MaxLength);

				//TestDummy.BoundCollectionToUse = new GlbStaffCollection(Factory);
				testDummy.BoundCollectionToUse = new DummyBusinessObjectCollection(Factory);
				testDummy.SS_DummyGuidInfo.RefreshBinding();
				//AssertEquals("Maxlength of codebox", GlbStaffSchema.GS_Code.MaxLength, ZGuidFindBox.CodeBox.MaxLength);
				AssertEquals("Maxlength of codebox", DummyBizoSchema.Z0_Code.MaxLength, ZGuidFindBox.CodeBox.MaxLength);

				AssertEquals("No developer errors should be reported yet", 0, ExceptionReporterTestListener.Instance.Count);

				testDummy.BoundCollectionToUse = new BizOWithNoCodePropertyCollection(Factory);
				testDummy.SS_DummyGuidInfo.RefreshBinding();
				AssertEquals("Maxlength of codebox", 130, ZGuidFindBox.CodeBox.MaxLength);

				AssertEquals("Developer error should be reported for missing code property", 1, ExceptionReporterTestListener.Instance.Count);
				Assert("Developer error should be reported for missing code property", ErrorReporter.LastMessageReported.IndexOf("Cannot find code property for " + typeof(BizOWithNoCodeProperty).FullName) != -1);
				ErrorReporter.Clear();
			}
		}

		#region IDataBoundControl

		public void TestDataSourceType()
		{
			using (var findBox = new ZGuidFindBox())
			{
				AssertEquals(typeof(ZGuid), findBox.DataSourceType);
			}
		}

		#endregion

		#region Implementation

		bool IsBindingForMaxLengthTest;

		protected override void SetBindTo(ZFindBoxUserControl findBox)
		{
			if (IsBindingForMaxLengthTest)
			{
				findBox.BindTo = "SS_DummyGuid";
				findBox.BindToList = "BoundCollection";
			}
			else
			{
				base.SetBindTo(findBox);
			}
		}

		class MaxLengthDummyBizO : FindBoxDummyBusinessObject
		{
			public MaxLengthDummyBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public BusinessObjectCollection BoundCollectionToUse;
			public BusinessObjectCollection BoundCollection
			{
				get { return BoundCollectionToUse; }
			}
		}

		class BizOWithNoCodeProperty : BusinessObject
		{
			public abstract class Schema
			{
				public const string TableName = "BizOWithNoCodeProperty";
			}

			public override SchemaGuidColumn PKSchemaColumn
			{
				get { return CargoWise.Schema.Schema.GenericPkColumn; }
			}

			public BizOWithNoCodeProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class BizOWithNoCodePropertyCollection : BusinessObjectCollection
		{
			public BizOWithNoCodePropertyCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public BizOWithNoCodeProperty this[int index]
			{
				get { return (BizOWithNoCodeProperty)Elements[index]; }
			}
		}

		#endregion
	}
}
