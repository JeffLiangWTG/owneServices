using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZFilterStripAuditDetailsTest : TestCaseWithDummy
	{
		public void TestGetDisplayDateTimeKind()
		{
			AssertEquals(DateTimeKind.Utc, FilterStripAuditDetails.GetDisplayDateTimeKind(typeof(DummyBusinessObject_WithUtcValueAndUtcDisplay)));
			AssertEquals(DateTimeKind.Local, FilterStripAuditDetails.GetDisplayDateTimeKind(typeof(DummyBusinessObject_WithUtcValueAndLocalDisplay)));

			AssertNotNull(Factory.LoadTop1(typeof(DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay), new ZQuery()));
			AssertEquals(DateTimeKind.Local, FilterStripAuditDetails.GetDisplayDateTimeKind(typeof(DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay)));

			var objects = Factory.Load(typeof(DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay), new ZQuery());
			Array.ForEach(objects, obj => obj.Delete());
			AssertNull(Factory.LoadTop1(typeof(DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay), new ZQuery()));
			AssertEquals(DateTimeKind.Local, FilterStripAuditDetails.GetDisplayDateTimeKind(typeof(DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay)));
		}

		public void TestGetDisplayDateTimeKind_ShouldNotUseFactory()
		{
			Factory.NewWithValidTestData<DummyBusinessObject>();
			using (AssertDbHitsForAllFactories(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 0 } }, ignoreUnspecified: true, thresholdForUnspecified: 30, includeFactoryPredicate: f => f.ThreadSentry.IsOwner))
			{
				var kind = FilterStripAuditDetails.GetDisplayDateTimeKind(typeof(DummyBusinessObject_WithUtcValueAndUtcDisplay));
				AssertEquals(DateTimeKind.Utc, kind);

				kind = FilterStripAuditDetails.GetDisplayDateTimeKind(typeof(DummyBusinessObject_WithUtcValueAndLocalDisplay));
				AssertEquals(DateTimeKind.Local, kind);
			}
		}

		public void TestAddAuditDetailsColumns_WithUtcValueAndUtcDisplay()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";

				FilterStripAuditDetails.AddAuditDetailsColumns(grid, EDIMessageSchema.Constants.TableName, typeof(DummyBusinessObject_WithUtcValueAndUtcDisplay));

				form.Controls.Add(grid);
				form.Show();

				AssertColumnAdded(grid, EDIMessageSchema.Constants.EM_SystemCreateUser);
				AssertDateColumnAdded(grid, EDIMessageSchema.Constants.EM_SystemCreateTimeUtc, "Created Time (UTC)", null);
				AssertColumnAdded(grid, EDIMessageSchema.Constants.EM_SystemLastEditUser);
				AssertDateColumnAdded(grid, EDIMessageSchema.Constants.EM_SystemLastEditTimeUtc, "Last Edited Time (UTC)", null);
			}
		}

		public void TestAddAuditDetailsColumns_WithUtcValueAndLocalDisplay()
		{
			using (var form = new ZForm(Dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";

				FilterStripAuditDetails.AddAuditDetailsColumns(grid, OrgColdCallRegisterSchema.Constants.TableName, typeof(DummyBusinessObject_WithUtcValueAndLocalDisplay));

				form.Controls.Add(grid);
				form.Show();

				AssertColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemCreateUser);
				AssertColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemCreateBranch);
				AssertColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemCreateDepartment);
				AssertDateColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemCreateTimeUtc, "Created Time", typeof(LocalAuditTimePropertyDescriptor));
				AssertColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemLastEditUser);
				AssertDateColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemLastEditTimeUtc, "Last Edited Time", typeof(LocalAuditTimePropertyDescriptor));
			}
		}

		public void TestAddAuditDetailsColumns_WithUtcValueAndLocalDisplayForAbstractType()
		{
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			Dummy.Factory.RefreshEnabled = false;
			var dummyInOtherFactory = factory2.NewWithValidTestData<DummyConcreteBusinessObject_WithUtcValueAndLocalDisplay>();
			factory2.Save();

			var factory3 = new BusinessObjectFactory(); //don't create or load any dummy bizOs in this factory, so the local cache is empty
			var randomBizO = factory3.NewWithValidTestData<StmNote_WithDummyBizOCollection>();

			using (var form = new ZForm(randomBizO))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";

				FilterStripAuditDetails.AddAuditDetailsColumns(grid, OrgColdCallRegisterSchema.Constants.TableName, typeof(DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay));

				form.Controls.Add(grid);
				form.Show();

				AssertColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemCreateUser);
				AssertColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemCreateBranch);
				AssertColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemCreateDepartment);
				AssertDateColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemCreateTimeUtc, "Created Time", typeof(LocalAuditTimePropertyDescriptor));
				AssertColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemLastEditUser);
				AssertDateColumnAdded(grid, OrgColdCallRegisterSchema.Constants.O1_SystemLastEditTimeUtc, "Last Edited Time", typeof(LocalAuditTimePropertyDescriptor));

				//we had to have the form use a non-dummy business object so that the factory's local cache lacked dummy bizOs, so we have to get and clear this message
				Assert(ErrorReporter.LastMessageReported.Contains("ZGrid .ZForm bound to CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection has the following non-Z columns:"));
				ErrorReporter.Clear();
			}
		}

		public void TestAddAuditDetailsColumns_WithNoArguments()
		{
			using (var grid = new ZGrid())
			{
				AssertExceptionThrown("InvalidOperationException exception was thrown on null grid", typeof(InvalidOperationException), () =>
				{
					FilterStripAuditDetails.AddAuditDetailsColumns(null, JobShipmentSchema.Constants.TableName, typeof(DummyEnterpriseBusinessObject));
				});

				AssertExceptionThrown("InvalidOperationException exception was thrown on empty table name", typeof(InvalidOperationException), () =>
				{
					FilterStripAuditDetails.AddAuditDetailsColumns(grid, "", typeof(DummyEnterpriseBusinessObject));
				});

				AssertExceptionThrown("InvalidOperationException exception was thrown on empty typeOfElements", typeof(InvalidOperationException), () =>
				{
					FilterStripAuditDetails.AddAuditDetailsColumns(grid, JobShipmentSchema.Constants.TableName, null);
				});
			}
		}

		#region Implementation

		void AssertColumnAdded(ZGrid grid, string columnName)
		{
			var column = grid.Columns[columnName];
			AssertNotNull("Column " + columnName + " added", column);
			AssertEquals("Column is readonly", true, column.ColumnStyle.ReadOnly);
			AssertEquals("Column is invisible by default", false, column.IsVisible);
		}

		void AssertDateColumnAdded(ZGrid grid, string columnName, string caption, Type propertyDescriptorType)
		{
			var column = grid.Columns[columnName];
			AssertNotNull("Column " + columnName + " added", column);
			AssertEquals("Column is readonly", true, column.ColumnStyle.ReadOnly);
			AssertEquals(caption, column.ColumnStyle.HeaderText);

			var propertyDescriptor = ((IOverridablePropertyDescriptor)((ZTextBoxColumnStyle)column.ColumnStyle).ColumnInfo).PropertyDescriptor;
			AssertEquals(propertyDescriptorType, propertyDescriptor != null ? propertyDescriptor.GetType() : null);
		}

		#region DummyBusinessObject Classes

		[ShouldDisplayInUtcTimeForEditAndCreateLogFields]
		class DummyBusinessObject_WithUtcValueAndUtcDisplay : DummyEnterpriseBusinessObject
		{
			public DummyBusinessObject_WithUtcValueAndUtcDisplay(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class DummyBusinessObject_WithUtcValueAndLocalDisplay : DummyEnterpriseBusinessObject
		{
			public DummyBusinessObject_WithUtcValueAndLocalDisplay(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		abstract class DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay : DummyEnterpriseBusinessObject
		{
			public new static readonly DummyTestTypeDecider TypeDecider = new DummyTestTypeDecider();

			public DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class DummyConcreteBusinessObject_WithUtcValueAndLocalDisplay : DummyAbstractBusinessObject_WithUtcValueAndLocalDisplay
		{
			public DummyConcreteBusinessObject_WithUtcValueAndLocalDisplay(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class DummyTestTypeDecider : TypeDecider
		{
			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return typeof(DummyConcreteBusinessObject_WithUtcValueAndLocalDisplay);
			}

			public override Type GetTypeForBinding()
			{
				return null;
			}

			public override Type GetTypeForNew()
			{
				return null;
			}
		}

		class StmNote_WithDummyBizOCollection : StmNote
		{
			public StmNote_WithDummyBizOCollection(BusinessObjectFactory factory, DataRow row)
					: base(factory, row)
			{
			}

			public DummyChildBusinessObjectCollection Collection
			{
				get
				{
					if (collection == null)
					{
						collection = NewCollection();
						//collection.Dummy = this;
					}
					return collection;
				}
				set { collection = value; }
			}

			DummyChildBusinessObjectCollection collection;

			protected virtual DummyChildBusinessObjectCollection NewCollection()
			{
				return new DummyChildBusinessObjectCollection(Factory);
			}
		}

		#endregion

		#endregion
	}
}
