using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture
{
	sealed class CalcEditTest : TestCaseWithDummy
	{
#if !WINZOR

		public void TestPadRightIfRightAligned()
		{
			using (var form = new TestForm(Dummy))
			{
				form.Show();

				var style = (ZCalcEditColumnStyle)form.Grid.TableStyles[0].GridColumnStyles[0];

				using (style.UsePadRightForTesting())
				{
					style.Alignment = HorizontalAlignment.Right;
					Assert(style.HeaderText.EndsWith("" + (char)32 + (char)31));

					style.Alignment = HorizontalAlignment.Center;
					Assert(!style.HeaderText.EndsWith("" + (char)32 + (char)31));
				}
			}
		}

#endif

		public void TestGetDecimalPlacesForCurrentWithZInt()
		{
			CheckGetDecimalPlacesForCurrent(DummyBizoSchema.Z0_Number, new ZInt(4));
		}

		public void TestGetDecimalPlacesForCurrentWithZByte()
		{
			CheckGetDecimalPlacesForCurrent(DummyBizoSchema.Z0_Byte, new ZByte(4));
		}

		public void TestGetDecimalPlacesForCurrentWithZShort()
		{
			CheckGetDecimalPlacesForCurrent(DummyBizoSchema.Z0_Short, new ZShort((short)4));
		}

		public void TestGetDecimalPlacesForCurrentWithZDecimal()
		{
			CheckGetDecimalPlacesForCurrent(DummyBizoSchema.Z0_Decimal, new ZDecimal(4));
		}

		void CheckGetDecimalPlacesForCurrent(SchemaColumn bindToDecimalPlacesColumn, INumericZType numberOfDPForTest)
		{
			var info = new ZCalcEditColumnStyleInfo();
			using (var columnStyle = new ZCalcEditColumnStyle(info))
			{
				columnStyle.Decimals = 1;

				AssertEquals("Fallback is Decimals property value", 1, columnStyle.GetDecimalPlacesForCurrent(null));
				AssertEquals("Fallback is Decimals property value", 1, columnStyle.GetDecimalPlacesForCurrent(Dummy));

				Dummy[bindToDecimalPlacesColumn.Name] = numberOfDPForTest;
				info.BindToDecimalPlaces = bindToDecimalPlacesColumn.Name;
				AssertEquals("Got correct value from Dummy", 4, columnStyle.GetDecimalPlacesForCurrent(Dummy));
			}
		}

		public void TestBindToDecimalPlacesDisplay()
		{
			using (var form = new TestForm(Dummy))
			{
				form.Show();
				UserIdleWorker.Flush();
				var calcEditStyle = (ZCalcEditColumnStyle)form.Grid.Columns[0].ColumnStyle;

				var dummyChild1 = Dummy.Collection.AddNew();
				var dummyChild2 = Dummy.Collection.AddNew();

				dummyChild1.Z0_AnotherDecimal = 42.3925m;
				dummyChild1.Z0_Number = 3;

				dummyChild2.Z0_AnotherDecimal = 1.3m;
				dummyChild2.Z0_Number = 0;

				form.Grid.Focus();
				form.Grid.CurrentCell = new DataGridCell(0, 0);
				var cm = form.Grid.ListManager;
				AssertEquals("42.393", form.Grid.LastFocusedColumn.EditControl.Text);
				AssertEquals("42.393", calcEditStyle.FormatValueObject(cm.Current, calcEditStyle.GetColumnValueAtRowExposed(cm, 0))); // emulates what gets painted when no edit control

				form.Grid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals("1", form.Grid.LastFocusedColumn.EditControl.Text);
				AssertEquals("1", calcEditStyle.FormatValueObject(cm.Current, calcEditStyle.GetColumnValueAtRowExposed(cm, 1))); // emulates what gets painted when no edit control
			}
		}

		public void TestGetCellTextAsStringWhenValueIsNull()
		{
			using (var form = new TestForm(Dummy))
			{
				form.Show();
				var calcEditStyle = (ZCalcEditColumnStyle)form.Grid.Columns[0].ColumnStyle;

				var dummyChild1 = Dummy.Collection.AddNew();
				var dummyChild2 = Dummy.Collection.AddNew();
				dummyChild1.Z0_Number = 3;
				dummyChild2.Z0_Number = 0;

				AssertEquals("0.000", calcEditStyle.FormatValueObject(dummyChild1, null));
				AssertEquals("0", calcEditStyle.FormatValueObject(dummyChild2, null));
			}
		}

		class TestForm : GUI.Testing.ZTestForm
		{
			public TestForm(DummyBusinessObject dummy) : base(dummy) { }

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Grid.ColumnStyles.Clear();
				var columnStyleInfo =
					new ZCalcEditColumnStyleInfo
					{
						ColumnName = DummyBizoSchema.Z0_AnotherDecimal.Name,
						BindToDecimalPlaces = DummyBizoSchema.Z0_Number.Name,
					};
				Grid.ColumnStyles.Add(columnStyleInfo);
				Grid.RefreshTableStyles();
			}
		}
	}
}
