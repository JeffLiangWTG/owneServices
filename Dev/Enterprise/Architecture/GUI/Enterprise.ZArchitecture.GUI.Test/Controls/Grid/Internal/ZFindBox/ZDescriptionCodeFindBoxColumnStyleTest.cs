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
	sealed class ZDescriptionCodeFindBoxColumnStyleTest : TestCaseWithDummy
	{
		#region TestZDescriptionCodeFindBoxColumnStyle

		public void TestZDescriptionCodeFindBoxColumnStyle()
		{
			var propertyDescriptor = TypeDescriptor.GetProperties(Dummy)[DummyBizoSchema.Z0_Code.Name];
			var columnStyleInfo =
				new ZDescriptionCodeFindBoxColumnStyleInfo(propertyDescriptor)
				{
					DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
				};
			using (var columnStyle = new ZDescriptionCodeFindBoxColumnStyle(columnStyleInfo))
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
					new ZDescriptionCodeFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Code"])
					{
						ColumnName = "Code" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionCodeFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				dummy.Code = "A1";

				columnStyle.Initialised4Test = false;
				AssertEquals("", columnStyle.GetColumnValueAtRowExposed(grid.ListManager, 0));

				columnStyle.Initialised4Test = true;
				AssertEquals("A1", columnStyle.GetColumnValueAtRowExposed(grid.ListManager, 0));
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
					new ZDescriptionCodeFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Code"])
					{
						ColumnName = "Code" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionCodeFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				dummy.Code = "A1";
				AssertEquals("Aaa Aaa", columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));

				dummy.Code = "B2";
				AssertEquals("Bbb Bbb", columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));

				columnStyle.Initialised4Test = false;
				AssertEquals("Bbb Bbb", columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));

				dummy.Code = ZDescriptionCodeFindBoxColumnStyle.InvalidCode;
				AssertEquals(Constants.FindBoxMessages.InvalidSelection, columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));

				FieldInvalidTextMemory.SetInvalidText(dummy, "Code", "Xxx Xxx");
				AssertEquals("Xxx Xxx", columnStyle.ColumnTextAtRowExposed(grid.ListManager, 0));
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
					new ZDescriptionCodeFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Code"])
					{
						ColumnName = "Code" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionCodeFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				((ZDescriptionGridFindBox)columnStyle.FindBoxExposed).PullList(dummy, "Code");

				columnStyle.FindBoxExposed.Code = "Aaa Aaa";
				AssertEquals("A1", columnStyle.EditValueExposed);

				columnStyle.FindBoxExposed.Code = "Bbb Bbb";
				AssertEquals("B2", columnStyle.EditValueExposed);

				columnStyle.FindBoxExposed.Code = "Xxx Xxx";
				AssertEquals(ZDescriptionCodeFindBoxColumnStyle.InvalidCode, columnStyle.EditValueExposed);
			}
		}

		public void TestEditValueWithDuplicates()
		{
			CreateDummy("A1", "Aaa Aaa");
			CreateDummy("A2", "Aaa Aaa");
			CreateDummy("B1", "Bbb Bbb");

			var collection = new DummiesWithCodes(Factory);
			var dummy = collection.AddNew();

			using (var form = new ZForm(collection))
			{
				var grid = new ZGrid();
				grid.ColumnStyles.Add(
					new ZDescriptionCodeFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Code"])
					{
						ColumnName = "Code" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionCodeFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				((ZDescriptionGridFindBox)columnStyle.FindBoxExposed).PullList(dummy, "Code");

				((IFindBoxPopupWithAdvancedCodeStore)columnStyle.FindBoxExposed).CodeFromPopup = "A1";

				columnStyle.FindBoxExposed.Code = "Aaa Aaa";
				AssertEquals("A1", columnStyle.EditValueExposed);

				columnStyle.FindBoxExposed.Code = "Bbb Bbb";
				AssertEquals("B1", columnStyle.EditValueExposed);

				columnStyle.FindBoxExposed.Code = "Xxx Xxx";
				AssertEquals(ZDescriptionCodeFindBoxColumnStyle.InvalidCode, columnStyle.EditValueExposed);

				((IFindBoxPopupWithAdvancedCodeStore)columnStyle.FindBoxExposed).CodeFromPopup = "A2";

				columnStyle.FindBoxExposed.Code = "Aaa Aaa";
				AssertEquals("A2", columnStyle.EditValueExposed);

				columnStyle.FindBoxExposed.Code = "Bbb Bbb";
				AssertEquals("B1", columnStyle.EditValueExposed);

				columnStyle.FindBoxExposed.Code = "Xxx Xxx";
				AssertEquals(ZDescriptionCodeFindBoxColumnStyle.InvalidCode, columnStyle.EditValueExposed);

				((IFindBoxPopupWithAdvancedCodeStore)columnStyle.FindBoxExposed).CodeFromPopup = "";

				columnStyle.FindBoxExposed.Code = "Aaa Aaa";
				Assert("A1 or A2 code should be selected", columnStyle.EditValueExposed.ToString().StartsWith("A"));

				columnStyle.FindBoxExposed.Code = "Bbb Bbb";
				AssertEquals("B1", columnStyle.EditValueExposed);

				columnStyle.FindBoxExposed.Code = "Xxx Xxx";
				AssertEquals(ZDescriptionCodeFindBoxColumnStyle.InvalidCode, columnStyle.EditValueExposed);
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
					new ZDescriptionCodeFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy)["Code"])
					{
						ColumnName = "Code" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Z0_Description.Name
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionCodeFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				((ZDescriptionGridFindBox)columnStyle.FindBoxExposed).PullList(dummy, "Code");

				SetEditValue(grid.ListManager, 0, columnStyle, "Aaa Aaa");
				AssertEquals("A1", dummy.Code);
				Assert(string.IsNullOrEmpty(FieldInvalidTextMemory.GetInvalidText(dummy, "Code")));

				columnStyle.SetColumnValueAtRowExposed(grid.ListManager, 0, new ZString("B1"));
				AssertEquals("B1", dummy.Code);
				Assert(string.IsNullOrEmpty(FieldInvalidTextMemory.GetInvalidText(dummy, "Code")));

				columnStyle.SetColumnValueAtRowExposed(grid.ListManager, 0, new ZString(ZDescriptionCodeFindBoxColumnStyle.InvalidCode));
				AssertEquals(ZDescriptionCodeFindBoxColumnStyle.InvalidCode, dummy.Code);
				AssertEquals("Aaa Aaa", FieldInvalidTextMemory.GetInvalidText(dummy, "Code"));
			}
		}

		#endregion

		#region TestIsCellReadOnlyCore

		public void TestIsCellReadOnlyCore()
		{
			var collection = new DummiesWithCodes(Factory);

			var dummy1 = Factory.New<DummyWithReadonlyCode>();
			dummy1.Code = "AAA";
			dummy1.Z0_Description = "Aaa Aaa";
			collection.Add(dummy1);
			AssertEquals(false, dummy1.CodeInfo.ReadOnly);
			AssertEquals(false, dummy1.Z0_DescriptionInfo.ReadOnly);

			var dummy2 = Factory.New<DummyWithReadonlyCode>();
			dummy2.Code = "XXX";
			dummy2.Z0_Description = "Xxx Xxx";
			collection.Add(dummy2);
			AssertEquals("Should be readonly for XXX", true, dummy2.CodeInfo.ReadOnly);
			AssertEquals(false, dummy2.Z0_DescriptionInfo.ReadOnly);

			AssertEquals(2, collection.Count);
			AssertEquals("AAA", collection[0].Code);
			AssertEquals("XXX", collection[1].Code);

			using (var form = new ZForm(collection))
			{
				var grid = new ZGrid();
				grid.ColumnStyles.Add(
					new ZDescriptionCodeFindBoxColumnStyleInfo4Test(TypeDescriptor.GetProperties(dummy1)["Code"])
					{
						ColumnName = "Code" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix,
						DescriptionColumnName = DummyBizoSchema.Constants.Z0_Description
					});
				form.Controls.Add(grid);

				form.Show();
				grid.SetDataBinding(collection, "");
				Application.DoEvents();

				var columnStyle = grid.Columns[0].ColumnStyle as ZDescriptionCodeFindBoxColumnStyle4Test;
				AssertNotNull(columnStyle);

				AssertEquals(false, columnStyle.IsCellReadOnly(grid.ListManager, 0));
				AssertEquals("Should be readonly for XXX", true, columnStyle.IsCellReadOnly(grid.ListManager, 1));
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

		void SetEditValue(CurrencyManager source, int rowNum, ZDescriptionCodeFindBoxColumnStyle4Test columnStyle, string value)
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

		class DummyWithReadonlyCode : DummyWithCodes
		{
			public DummyWithReadonlyCode(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected bool Code_ReadOnly { get { return Code == "XXX"; } }
		}

		class ZDescriptionCodeFindBoxColumnStyleInfo4Test : ZDescriptionCodeFindBoxColumnStyleInfo
		{
			public ZDescriptionCodeFindBoxColumnStyleInfo4Test(PropertyDescriptor propertyDescriptor)
				: base(propertyDescriptor)
			{
				((IOverridablePropertyDescriptor)this).PropertyDescriptor = propertyDescriptor;
			}

			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public override Type ColumnStyleType
			{
				get { return typeof(ZDescriptionCodeFindBoxColumnStyle4Test); }
			}
		}

		class ZDescriptionCodeFindBoxColumnStyle4Test : ZDescriptionCodeFindBoxColumnStyle
		{
			public ZDescriptionCodeFindBoxColumnStyle4Test(ZDescriptionCodeFindBoxColumnStyleInfo4Test columnInfo) : base(columnInfo) { }

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
