using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZGuidFindBoxColumnStyleTest : TestCaseWithDummy
	{
		public void TestValueIsCommittedOnPopupSelection_WhenDifferentPkButSameCode()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "ADL";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "ADL";

			Factory.Save();

			var dummy = SuperDummy.Collection.AddNew();
			dummy.Z0_Guid = dummy1.PK;
			using (TestForm = CreateTestForm(SuperDummy, true))
			{
				TestForm.Show();

				var guidFindBox = ((ZGuidFindBoxColumnStyle)Grid.Columns.First(column => column.ColumnStyle is ZGuidFindBoxColumnStyle).ColumnStyle).GuidFindBox;
				guidFindBox.ModuleID = DummyModuleIDs.Dummy;

				Grid.CurrentCell = new DataGridCell(0, 0);
				new PopupModuleDecisionProvider(guidFindBox).HandleFindBoxOKButton(new[] { dummy2 });
				Grid.CurrentCell = new DataGridCell(1, 0);

				AssertEquals(dummy2.PK, dummy.Z0_Guid);
			}
		}

		[RequiresSTA]
		public virtual void TestGetValueAsStringCore()
		{
			using (TestForm = CreateTestForm(SuperDummy, true))
			{
				SuperDummy.RegisterEditableChildObject(SuperDummy.Collection);
				SuperDummy.SetReadOnlyIncludingChildren(true);
				var dummy = SuperDummy.Collection.AddNew();
				dummy.Z0_Guid = Child1.PK;

				TestForm.Show();

				var columnStyle = Grid.Columns.Select(gridColumn => gridColumn.ColumnStyle).OfType<ZGuidFindBoxColumnStyle>().FirstOrDefault();
				AssertNotNull("Should find column style info", columnStyle);

				columnStyle.ResetInitializedForTest();
				AssertEquals("Should get correct Code value", "CH1", columnStyle.GetValueAsString(Grid.ListManager, 0));
			}
		}

		public void TestDoesNotShowGuidWhenReadOnlyAndChangedWhileFocused()
		{
			using (TestForm = CreateTestForm(SuperDummy, true))
			{
				SuperDummy.RegisterEditableChildObject(SuperDummy.Collection);
				SuperDummy.SetReadOnlyIncludingChildren(true);
				var dummy = SuperDummy.Collection.AddNew();
				dummy.Z0_Guid = Child1.PK;

				TestForm.Show();
				AssertEquals("CH1", Grid.LastFocusedColumn.TextBox.Text);

				dummy.Z0_Money = 5; // seting another property calls UpdateUI, which previously would cause the guid to appear.
				AssertEquals("CH1", Grid.LastFocusedColumn.TextBox.Text);
			}
		}

		public void TestInvalid()
		{
			using (TestForm = CreateTestForm(SuperDummy, false))
			{
				TestForm.Show();
				AssertPreconditions();

				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.Z);
				SendKeyToEditControl(Keys.Z);
				SendKeyToEditControl(Keys.Z);
				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.Shift | Keys.Tab);

				AssertEquals("ZGuid.Invalid set in Dummy", ZGuid.Invalid, SuperDummy.Collection[0].Z0_Guid);
				AssertEquals("ZZZ Stored in FieldInvalidTextMemory", "ZZZ", FieldInvalidTextMemory.GetInvalidText(SuperDummy.Collection[0], "Z0_Guid"));
				AssertEquals("ZZZ should be shown in editor", "ZZZ", Grid.LastFocusedColumn.EditControl.Text);

				SendKeyToEditControl(Keys.Y);
				SendKeyToEditControl(Keys.Y);
				SendKeyToEditControl(Keys.Y);
				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.Shift | Keys.Tab);

				AssertEquals("ZGuid.Invalid set in Dummy", ZGuid.Invalid, SuperDummy.Collection[0].Z0_Guid);
				AssertEquals("YYY Stored in FieldInvalidTextMemory", "YYY", FieldInvalidTextMemory.GetInvalidText(SuperDummy.Collection[0], "Z0_Guid"));
				AssertEquals("YYY should be shown in editor", "YYY", Grid.LastFocusedColumn.EditControl.Text);

				SuperDummy.Collection[0].Z0_Guid = ZGuid.Empty;
				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.Shift | Keys.Tab);

				AssertEquals("Empty value should be shown in editor", "", Grid.LastFocusedColumn.EditControl.Text);
			}
		}

		public void TestEmpty()
		{
			using (TestForm = CreateTestForm(SuperDummy, false))
			{
				TestForm.Show();
				AssertPreconditions();

				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.D1);
				SendKeyToEditControl(Keys.Back);
				SendKeyToEditControl(Keys.Tab);

				AssertEquals("ZGuid.Empty set in Dummy", ZGuid.Empty, SuperDummy.Collection[0].Z0_Guid);
			}
		}

		[RequiresSTA]
		public void TestValid()
		{
			using (TestForm = CreateTestForm(SuperDummy, false))
			{
				TestForm.Show();
				AssertPreconditions();

				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.C);
				SendKeyToEditControl(Keys.H);
				SendKeyToEditControl(Keys.D1);
				SendKeyToEditControl(Keys.Tab);

				AssertEquals("CH1 PK set in Dummy", Child1.PK, SuperDummy.Collection[0].Z0_Guid);
			}
		}

		public void TestReadOnly()
		{
			using (TestForm = CreateTestForm(SuperDummy, false))
			{
				SuperDummy.Collection.AddNew();
				SuperDummy.Collection[0].Z0_Guid = Child1.PK;
				SuperDummy.Collection[0].Z0_Guid_ReadOnly = true;

				TestForm.Show();
				AssertPreconditions();

				SendKeyToEditControl(Keys.Right);
				AssertEquals("Navigated Right", new DataGridCell(0, 1), Grid.CurrentCell);

				AssertEquals("Text while ReadOnly", "CH1", Grid.LastFocusedColumn.TextBox.Text);
				AssertEquals("All Text should be selected when entering readonly column", "CH1", Grid.LastFocusedColumn.TextBox.SelectedText);
			}
		}

		#region Test Classes

		public class TestBusinessObject : BusinessObject, IObsoleteValidation
		{
			#region Property Constants

			public abstract class Schema
			{
				public const string TableName = "DUMMYBIZO";
				public const string SS_Name = "Z0_Description";
			}

			#endregion

			public TestBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region Business Object Overrides

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				DummyEnterpriseBusinessObject.SetDataRowDefaultValues(((INeedRow)this).Row);
			}

			#endregion

			#region PK

			public override SchemaGuidColumn PKSchemaColumn
			{
				get { return DummyBizoSchema.PK; }
			}

			#endregion

			#region Dummy Collection

			public DummyEnterpriseBusinessObjectCollection Collection
			{
				get
				{
					if (fCollection == null)
					{
						fCollection = new DummyEnterpriseBusinessObjectCollection(Factory);
					}
					return fCollection;
				}
			}
			DummyEnterpriseBusinessObjectCollection fCollection;

			#endregion
		}

		public class DummyEnterpriseBusinessObjectCollection : DummyBusinessObjectCollection
		{
			public DummyEnterpriseBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new DummyEnterpriseBusinessObject this[int i]
			{
				get { return (DummyEnterpriseBusinessObject)base[i]; }
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Child1 = Factory.New<DummyChildBusinessObject>();
			Child1.Z0_Code = "CH1";

			Child2 = Factory.New<DummyChildBusinessObject>();
			Child2.Z0_Code = "CH2";

			Child3 = Factory.New<DummyChildBusinessObject>();
			Child3.Z0_Code = "CH3";

			SuperDummy = Factory.New<TestBusinessObject>();
		}

		public TestBusinessObject SuperDummy;
		public DummyChildBusinessObject Child1;
		public DummyChildBusinessObject Child2;
		public DummyChildBusinessObject Child3;

		protected virtual void SendKeyToEditControl(Keys key)
		{
			var findBox = Grid.LastFocusedColumn.EditControl as ZGridFindBox;
			KeySender.PostKeyDown(findBox.CodeBox, findBox.CodeBox.Handle, key);
			Application.DoEvents();
		}

		public ZForm TestForm;

		protected virtual ZGrid Grid
		{
			get { return ((ZGuidFindBoxColumnStyleTestForm)TestForm).zGrid1; }
		}

		public void AssertPreconditions()
		{
			AssertNotNull(TestForm);
			AssertNotNull(Grid);
			AssertNotNull(Grid.LastFocusedColumn);
			AssertNotNull(Grid.LastFocusedColumn.EditControl);
			AssertEquals("Default Cell Position", new DataGridCell(0, 0), Grid.CurrentCell);
		}

		public ZForm CreateTestForm(BusinessObject superDummy, bool addGuidColumnFirst)
		{
			ZForm result = null;
			ZGuidFindBoxColumnStyleTestForm.AddGuidColumnFirst = addGuidColumnFirst;
			try
			{
				result = CreateTestFormCore(SuperDummy);
			}
			finally
			{
				ZGuidFindBoxColumnStyleTestForm.AddGuidColumnFirst = false;
			}

			return result;
		}

		protected virtual ZForm CreateTestFormCore(BusinessObject superDummy)
		{
			return new ZGuidFindBoxColumnStyleTestForm(SuperDummy);
		}

		#endregion
	}

	sealed class GuidFindBoxTest : TestCaseWithDummy
	{
		public void TestGetMaxLength()
		{
			var columnInfo = new ZGuidFindBoxColumnStyleInfo();
			columnInfo.BindToList = "Collection";
			columnInfo.Caption = "GuidFindBox";
			columnInfo.ColumnName = "Z0_Guid";

			using (var columnStyle = new ZGuidFindBoxColumnStyle(columnInfo))
			{
				columnStyle.InitialiseEditControlForNewPosition(Dummy, false);
				AssertEquals(5, columnStyle.GetMaxLength(Dummy));

				Dummy.Collection = new TestDummyChildBusinessObjectCollection(Factory);
				columnStyle.InitialiseEditControlForNewPosition(Dummy, false);
				AssertEquals(99, columnStyle.GetMaxLength(Dummy));
			}
		}
#if !WINZOR
		public void TestGetPreferredSize()
		{
			var superDummy = Factory.New<SuperDummyBusinessObject>();
			var dummy = superDummy.Collection.AddNew();

			using (var form = new ZGuidFindBoxColumnStyleTestForm(superDummy))
			{
				form.Show();
				var g = form.CreateGraphics();

				var columnStyle = (ZGuidFindBoxColumnStyle)form.zGrid1.Columns[1].ColumnStyle;
				var size = columnStyle.GetPreferredSizeExposed(g, ZGuid.NewZGuid());
				var expectedWidth = (int)Math.Ceiling(g.MeasureString("BBBBB", form.zGrid1.Font).Width);
				var message = string.Format("Expected Width: {0}, Actual Width: {1}, Delta: 10", expectedWidth, size.Width);
				Assert(message, size.Width > expectedWidth && size.Width < expectedWidth + 10);
			}
		}
#endif
		public void TestColumnTextAtRow_WhenNoCurrentPosition()
		{
			var superDummy = Factory.New<SuperDummyBusinessObject>();

			using (var form = new ZGuidFindBoxColumnStyleTestForm(superDummy))
			{
				form.Show();
				var columnStyle = (ZGuidFindBoxColumnStyle)form.zGrid1.Columns[1].ColumnStyle;
				form.zGrid1.ListManager.Position = -1;
				var columnText = columnStyle.ColumnTextAtRow(form.zGrid1.ListManager, -1);
				AssertEquals("ColumnTextAtRow value when no current position", "", columnText);
			}
		}

		class TestDummyChildBusinessObjectCollection : DummyChildBusinessObjectCollection
		{
			public TestDummyChildBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override IFindBoxListProvider FindBoxListProvider
			{
				get { return new TestListProvider(this); }
			}

			TestDummyChildBusinessObject element;
			public TestDummyChildBusinessObject Element
			{
				get { return element ?? (element = Factory.New<TestDummyChildBusinessObject>()); }
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(TestDummyChildBusinessObject);
			}

			public class TestListProvider : FindBoxListProvider
			{
				public TestListProvider(TestDummyChildBusinessObjectCollection collection)
					: base(collection)
				{
					this.collection = collection;
				}

				readonly TestDummyChildBusinessObjectCollection collection;

				protected override IEnumerable<BusinessObject> GetBusinessObjectsFromCodeCore(string code)
				{
					return new[] { collection.Element };
				}
			}
		}

		[CodeProperty("Code")]
		class TestDummyChildBusinessObject : DummyChildBusinessObject
		{
			public TestDummyChildBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[CargoWise.ComponentModel.MaxLength(99)]
			public ZString Code
			{
				get { return null; }
			}
		}
	}
}
