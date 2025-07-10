using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class BindingContextExtensionsTest : TestCaseWithFactory
	{
		public void TestBindingContextIndexer_ForPropertyManager()
		{
			var propertyManager = Form.BindingContext[Factory.New<DummyWithRelated>()];
			Assert("PropertyManager returned", propertyManager is CurrencyManager);
			var property = propertyManager.GetItemProperties()["Z0_Code"];
			Assert("PropertyDescriptors are of the correct type", property is KPropertyDescriptor);
			Assert(typeof(DummyBusinessObject).IsAssignableFrom(property.ComponentType));
		}

		public void TestBindingContextIndexer_ForRelatedPropertyManager()
		{
			var relatedPropertyManager = Form.BindingContext[Factory.New<DummyWithRelated>(), "RelatedObject"];
			Assert(relatedPropertyManager is CurrencyManager);
			var property = relatedPropertyManager.GetItemProperties()["Z0_ChildOnly"];
			Assert(property is KPropertyDescriptor);
			Assert(typeof(DummyChildBusinessObject).IsAssignableFrom(property.ComponentType));
		}

		public void TestBindingContextIndexer_ForRelatedCurrencyManager()
		{
			var relatedCurrencyManager = Form.BindingContext[Factory.New<DummyWithRelated>(), "DetailObjects"];
			var property = relatedCurrencyManager.GetItemProperties()["DetailDetailObjects"];
			AssertEquals("property.ComponentType", true, typeof(DummyDetail).IsAssignableFrom(property.ComponentType));
			Assert(property is KPropertyDescriptor);
		}

		public void TestGetBindingManager()
		{
			var bc = new BindingContext();
			BusinessObject entity = Factory.New<DummyWithRelated>();

			var bm = bc.EnsureListManager(entity, "DetailObjects.DetailProperty");
			var metaBM = BindingContextExtensions.GetBindingManager(bc, entity, "DetailObjects.DetailProperty", MetaDataTypes.ReadOnly);
			AssertNotNull("Should find the meta-data BindingManagerBase", metaBM);
			Assert("Shouldn't return the original BindingManagerBase which would be dumb", bm != metaBM);
		}

		public void TestSetBindingManagerCurrent()
		{
			var masterEntity = Factory.New<DummyWithRelated>();
			var detailEntity = masterEntity.DetailObjects.AddNew();
			var detailDetailEntity1 = detailEntity.DetailDetailObjects.AddNew();
			var detailDetailEntity2 = detailEntity.DetailDetailObjects.AddNew();

			var bc = new BindingContext();
			var cm = (CurrencyManager)bc[masterEntity, "DetailObjects.DetailDetailObjects"];
			using (var control = new TestUserControl())
			{
				control.SetDataSourceType(typeof(DummyWithRelated));
				control.SetDataBinding(masterEntity, "DetailObjects");

				BindingContextExtensions.SetBindingManagerCurrent(bc, control, "DetailDetailObjects", detailDetailEntity2);
				AssertEquals(1, cm.Position);
				BindingContextExtensions.SetBindingManagerCurrent(bc, control, "DetailDetailObjects", detailDetailEntity1);
				AssertEquals(0, cm.Position);
			}
		}

		public void TestBindingMemberNotFoundExceptionMessage()
		{
			var msg = string.Empty;
			try
			{
				_ = new BindingContext()[new object(), "BindingMember"];
			}
			catch (ArgumentException ex)
			{
				msg = ex.Message;
			}
			Assert(msg, msg.Contains("BindingMember"));
		}

		#region Implementation

		Form Form
		{
			get { return form ?? (form = new Form()); } // Used for design time only
		}
		Form form;

		#region Test classes

		class DummyWithRelated : DummyBusinessObject
		{
			public DummyWithRelated(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyChildBusinessObject RelatedObject
			{
				get;
				set;
			}

			public DummyDetailColection DetailObjects
			{
				get
				{
					return detailObjects ?? (detailObjects = new DummyDetailColection(Factory));
				}
			}
			public DummyDetailColection detailObjects;
		}

		class DummyDetail : DummyBusinessObject
		{
			public DummyDetail(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString DetailProperty { get; set; }

			public DummyDetailDetailColection DetailDetailObjects
			{
				get
				{
					return detailDetailObjects ?? (detailDetailObjects = new DummyDetailDetailColection(Factory));
				}
			}
			public DummyDetailDetailColection detailDetailObjects;
		}

		class DummyDetailDetail : DummyBusinessObject
		{
			public DummyDetailDetail(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString DetailDetailProperty { get; set; }
		}

		class DummyDetailColection : BusinessObjectCollection<DummyDetail>
		{
			public DummyDetailColection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		class DummyDetailDetailColection : BusinessObjectCollection<DummyDetailDetail>
		{
			public DummyDetailDetailColection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		class TestUserControl : KUserControl // for testing only
		{
			public void SetDataSourceType(Type type)
			{ BindingSource.DataSourceType = type; }
		}
		#endregion

		#endregion
	}
}
