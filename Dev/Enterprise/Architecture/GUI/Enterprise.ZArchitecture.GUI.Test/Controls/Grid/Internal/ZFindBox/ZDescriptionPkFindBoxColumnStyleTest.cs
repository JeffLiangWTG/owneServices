using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZDescriptionPkFindBoxColumnStyleTest : TestCaseWithDummy
	{
		#region TestZDescriptionPkFindBoxColumnStyle

		public void TestZDescriptionPkFindBoxColumnStyle()
		{
			var propertyDescriptor = TypeDescriptor.GetProperties(Dummy)[DummyBizoSchema.Z0_Guid.Name];
			var columnStyleInfo =
				new ZDescriptionPkFindBoxColumnStyleInfo(propertyDescriptor)
				{
					DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
				};
			using (var columnStyle = new ZDescriptionPkFindBoxColumnStyle(columnStyleInfo))
			{
				AssertEquals(propertyDescriptor, columnStyle.PropertyDescriptor);
				AssertEquals(DummyBizoSchema.Z0_Description.Name, columnStyle.DescriptionColumnName);
				AssertEquals(typeof(ZDescriptionGridFindBox), columnStyle.FindBox.GetType());
			}
		}

		#endregion

		#region TestGetColumnValueAtRow

		public void TestGetColumnValueAtRow()
		{
			PrepareTestData();
			var collection = new DummiesWithCodes(Factory);
			var dummy = collection.AddNew();

			using (var form = new ZForm(collection))
			{
				var grid = new ZGrid();
				grid.ColumnStyles.Add(
					new ZDescriptionPkFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Key"])
					{
						ColumnName = "Key" + ZDescriptionPkFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionPkFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				dummy.Code = "A1";
				dummy.Key = dummy.PK;

				columnStyle.Initialised4Test = false;
				AssertEquals("", columnStyle.GetColumnValueAtRowExposed(grid.ListManager, 0));

				columnStyle.Initialised4Test = true;
				AssertEquals(dummy.PK, columnStyle.GetColumnValueAtRowExposed(grid.ListManager, 0));
			}
		}

		#endregion

		#region TestColumnTextAtRow

		public void TestColumnTextAtRow()
		{
			PrepareTestData();
			var collection = new DummiesWithCodes(Factory);
			var dummy = collection.AddNew();

			using (var form = new ZForm(collection))
			{
				var grid = new ZGrid();
				grid.ColumnStyles.Add(
					new ZDescriptionPkFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Key"])
					{
						ColumnName = "Key" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionPkFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				dummy.Key = Dummy.PK;
				AssertEquals("Xxx Yyy Zzz", columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));

				columnStyle.Initialised4Test = false;
				AssertEquals("Xxx Yyy Zzz", columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));

				dummy.Key = ZGuid.Invalid;
				AssertEquals(Constants.FindBoxMessages.InvalidSelection, columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));

				dummy.Key = ZGuid.Empty;
				AssertEquals("", columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));
			}
		}

		#endregion

		#region TestEditValue

		public void TestEditValue()
		{
			PrepareTestData();
			var collection = new DummiesWithCodes(Factory);
			var dummy = collection.AddNew();

			using (var form = new ZForm(collection))
			{
				var grid = new ZGrid();
				grid.ColumnStyles.Add(
					new ZDescriptionPkFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Key"])
					{
						ColumnName = "Key" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionPkFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				((ZDescriptionGridFindBox)columnStyle.FindBoxExposed).PullList(dummy, "Key");

				columnStyle.FindBoxExposed.Code = "Xxx Yyy Zzz";
				AssertEquals(Dummy.PK, columnStyle.EditValueExposed);

				columnStyle.FindBoxExposed.Code = "Xxx Xxx";
				AssertEquals(ZGuid.Empty, columnStyle.EditValueExposed);
			}
		}

		#endregion

		#region TestSetColumnValueAtRow

		public void TestSetColumnValueAtRow()
		{
			PrepareTestData();
			var collection = new DummiesWithCodes(Factory);
			var dummy = collection.AddNew();

			using (var form = new ZForm(collection))
			{
				var grid = new ZGrid();
				grid.ColumnStyles.Add(
					new ZDescriptionPkFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Key"])
					{
						ColumnName = "Key" + ZDescriptionPkFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionPkFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				((ZDescriptionGridFindBox)columnStyle.FindBoxExposed).PullList(dummy, "Key");

				SetEditValue(grid.ListManager, 0, columnStyle, "Xxx Yyy Zzz");
				AssertEquals(Dummy.PK, dummy.Key);
				Assert(string.IsNullOrEmpty(FieldInvalidTextMemory.GetInvalidText(dummy, "Key")));

				columnStyle.SetColumnValueAtRowExposed(grid.ListManager, 0, ZGuid.Invalid);
				AssertEquals(ZGuid.Invalid, dummy.Key);
			}
		}

		#endregion

		#region Implementation

		void PrepareTestData()
		{
			Dummy.Z0_Code = "XYZ";
			Dummy.Z0_Description = "Xxx Yyy Zzz";

			CreateDummy("A1", "Aaa Aaa");
			CreateDummy("B2", "Bbb Bbb");
			CreateDummy("C3", "Ccc Ccc");
		}

		void CreateDummy(string code, string description)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = code;
			dummy.Z0_Description = description;
		}

		void SetEditValue(CurrencyManager source, int rowNum, ZDescriptionPkFindBoxColumnStyle4Test columnStyle, string value)
		{
			columnStyle.IsEditingExposed = true;
			columnStyle.FindBoxExposed.Code = value;
			columnStyle.CommitExposed(source, rowNum);
		}

		#endregion

		#region Test classes

		class DummyWithCodes : DummyBusinessObject
		{
			public DummyWithCodes(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[List("Codes")]
			public ZString Code { get; set; }

			public ZPropertyInfo CodeInfo
			{
				get { return GetZPropertyInfo(nameof(Code)); }
			}

			public ReferenceableCollection Codes { get { return new ReferenceableCollection(Factory); } }

			[List("Keys")]
			public ZGuid Key { get; set; }

			public ZPropertyInfo KeyInfo
			{
				get
				{
					return GetZPropertyInfo(nameof(Key));
				}
			}

			public ReferenceableCollection Keys
			{
				get
				{
					return new ReferenceableCollection(Factory);
				}
			}
		}

		class DummiesWithCodes : BusinessObjectCollection<DummyWithCodes>
		{
			public DummiesWithCodes(BusinessObjectFactory factory) : base(factory) { }
		}

		[CodeProperty("Z0_Code"), DescriptionProperty("Z0_Description", CanBeReferencedBy = true)]
		class ReferenceableDummy : DummyBusinessObject
		{
			public ReferenceableDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class ReferenceableCollection : BusinessObjectCollection<ReferenceableDummy>
		{
			public ReferenceableCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		class ZDescriptionPkFindBoxColumnStyleInfo4Test : ZDescriptionPkFindBoxColumnStyleInfo
		{
			public ZDescriptionPkFindBoxColumnStyleInfo4Test(PropertyDescriptor propertyDescriptor)
				: base(propertyDescriptor)
			{
				((IOverridablePropertyDescriptor)this).PropertyDescriptor = propertyDescriptor;
			}

			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public override Type ColumnStyleType
			{
				get { return typeof(ZDescriptionPkFindBoxColumnStyle4Test); }
			}
		}

		class ZDescriptionPkFindBoxColumnStyle4Test : ZDescriptionPkFindBoxColumnStyle
		{
			public ZDescriptionPkFindBoxColumnStyle4Test(ZDescriptionPkFindBoxColumnStyleInfo4Test columnInfo) : base(columnInfo) { }

			public IFindBox FindBoxExposed
			{
				get { return FindBox; }
			}

			public object EditValueExposed
			{
				get { return EditValue; }
			}

			public new object GetColumnValueAtRowExposed(CurrencyManager source, int rowNum)
			{
				return GetColumnValueAtRow(source, rowNum);
			}

			public string ColumnTextAtRowExposed(CurrencyManager source, int rowNum)
			{
				return ColumnTextAtRow(source, rowNum);
			}

			public void SetColumnValueAtRowExposed(CurrencyManager source, int rowNum, object value)
			{
				SetColumnValueAtRow(source, rowNum, value);
			}

			public bool IsEditingExposed
			{
				set { IsEditing = value; }
			}

			public void CommitExposed(CurrencyManager source, int rowNum)
			{
				Commit(source, rowNum);
			}
		}

		#endregion
	}
}
