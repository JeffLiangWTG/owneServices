using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZActiveGridCustomColumnsInitializerTest : TestCaseWithFactory
	{
		public void TestHookCollection()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			using (var grid = new RefreshTableGrid())
			using (var activeGridCustomColumnsInitializer = new ZActiveGridCustomColumnsInitializerHitCount(grid, collection, new ResourceStringData("", "Test Properties")))
			{
				activeGridCustomColumnsInitializer.HookCollection();

				AssertEquals(0, grid.ColumnStyles.Count);
				AssertEquals(0, grid.RefreshTableStylesCoreHitCount);
				AssertEquals(0, activeGridCustomColumnsInitializer.HookBusinessObjectCoreHitCount);

				var dummy = Factory.New<DummyWithPropertyContainer>();
				dummy.CustomPropertyContainer.AddCustomProperty("_abc", "Abc", typeof(ZString));
				dummy.CustomPropertyContainer.AddCustomProperty("_xyz", "Xyz", typeof(ZInt));

				AssertEquals(0, grid.ColumnStyles.Count);
				AssertEquals(0, grid.RefreshTableStylesCoreHitCount);

				collection.Add(dummy);
				AssertEquals("Added a new element, so should be hooked.", 1, activeGridCustomColumnsInitializer.HookBusinessObjectCoreHitCount);
				AssertEquals(1, grid.RefreshTableStylesCoreHitCount);

				AssertEquals(2, grid.ColumnStyles.Count);
				AssertEquals("_abc", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("_xyz", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals(true, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[0]).IsSubmissive);
				AssertEquals(true, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[1]).IsSubmissive);

				dummy.CustomPropertyContainer.AddCustomProperty("_ttt", "Ttt", typeof(ZString));
				AssertEquals("called to reset Custom property hooks.", 2, activeGridCustomColumnsInitializer.HookBusinessObjectCoreHitCount);
				AssertEquals(3, grid.ColumnStyles.Count);
				AssertEquals("_ttt", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals(true, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[0]).IsSubmissive);
				AssertEquals(2, grid.RefreshTableStylesCoreHitCount);

				((IBusinessObjectCollectionInternals)collection).FireListResetEvent();
				AssertEquals("Same objects in the collection, so no objects to rehook.", 2, activeGridCustomColumnsInitializer.HookBusinessObjectCoreHitCount);
				AssertEquals(2, grid.RefreshTableStylesCoreHitCount);

				collection.Remove(dummy);
				AssertEquals("Removing business object should not remove columns from grid.", 3, grid.ColumnStyles.Count);
				AssertEquals("Is removing so no hooking done here.", 2, activeGridCustomColumnsInitializer.HookBusinessObjectCoreHitCount);
				AssertEquals(2, grid.RefreshTableStylesCoreHitCount);
			}
		}

		public void TestHookCollectionDoesntRefreshTablesMoreThanOnce()
		{
			var dummy = Factory.New<DummyWithPropertyContainer>();
			dummy.CustomPropertyContainer.AddCustomProperty("_abc", "Abc", typeof(ZString));
			dummy.CustomPropertyContainer.AddCustomProperty("_xyz", "Xyz", typeof(ZInt));

			var dummy2 = Factory.New<DummyWithPropertyContainer>();
			dummy2.CustomPropertyContainer.AddCustomProperty("_abc", "Abc", typeof(ZString));
			dummy2.CustomPropertyContainer.AddCustomProperty("_xyz", "Xyz", typeof(ZInt));

			var activeCollection = new ActiveBusinessObjectCollection<DummyWithPropertyContainer>(Factory);
			activeCollection.AdditionalFilter = ZQuery.NoResultQuery;

			using (var grid = new RefreshTableGrid())
			using (var activeGridCustomColumnsInitializer = new ZActiveGridCustomColumnsInitializerHitCount(grid, activeCollection, new ResourceStringData("", "Test Properties")))
			{
				activeGridCustomColumnsInitializer.HookCollection();

				AssertEquals(0, grid.ColumnStyles.Count);
				AssertEquals(0, grid.RefreshTableStylesCoreHitCount);
				AssertEquals(0, activeGridCustomColumnsInitializer.HookBusinessObjectCoreHitCount);

				activeCollection.AdditionalFilter = new ZQuery();
				AssertEquals("Added a new element, so should be hooked.", 2, activeGridCustomColumnsInitializer.HookBusinessObjectCoreHitCount);
				AssertEquals("Only hit once, even though there are 2 objects to hook", 1, grid.RefreshTableStylesCoreHitCount);
			}
		}

		#region Test Classes

		public class DummyWithPropertyContainer : DummyBusinessObject, IActiveCustomPropertyContainer
		{
			public DummyWithPropertyContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			#region ICustomPropertyContainer Members

			IEnumerable<ICustomProperty> ICustomPropertyContainer.CustomProperties
			{
				get { return CustomPropertyContainer.CustomProperties; }
			}

			public event EventHandler PropertySetChanged;

			public void AddCustomProperty(ICustomProperty property)
			{
				CustomPropertyContainer.AddCustomProperty(property);
			}

			void OnPropertySetChanged(object sender, EventArgs eventArgs)
			{
				if (PropertySetChanged != null)
				{
					PropertySetChanged(this, eventArgs);
				}
			}

			public CustomPropertyContainer CustomPropertyContainer
			{
				get
				{
					if (customPropertyContainer == null)
					{
						customPropertyContainer = new CustomPropertyContainer();
						customPropertyContainer.PropertySetChanged += OnPropertySetChanged;
					}
					return customPropertyContainer;
				}
			}
			CustomPropertyContainer customPropertyContainer;

			#endregion
		}

		public class ZActiveGridCustomColumnsInitializerHitCount : ZActiveGridCustomColumnsInitializer
		{
			public ZActiveGridCustomColumnsInitializerHitCount(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, bool isVisible = false, bool isReadonly = true)
				: base(grid, collection, groupName, isVisible, isReadonly)
			{ }

			protected override void HookBusinessObjectCore(IEnumerable<BusinessObject> businessObjects)
			{
				HookBusinessObjectCoreHitCount += businessObjects.Count();

				base.HookBusinessObjectCore(businessObjects);
			}

			public int HookBusinessObjectCoreHitCount;
		}

		class RefreshTableGrid : ZGrid
		{
			protected override void RefreshTableStylesCore()
			{
				RefreshTableStylesCoreHitCount++;
				base.RefreshTableStylesCore();
			}

			public int RefreshTableStylesCoreHitCount;
		}

		#endregion
	}
}
