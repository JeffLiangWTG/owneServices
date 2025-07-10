using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Layout.Testing
{
	sealed class RowLayoutStyleCollectionTest : TestCase
	{
		public void TestGetVisibleRowNumber()
		{
			TestGetVisibleRowNumber(false);
		}

		public void TestGetVisibleRowNumber_AtDesignTime()
		{
			TestGetVisibleRowNumber(true);
		}

		void TestGetVisibleRowNumber(bool designTime)
		{
			Rows.Designing = designTime;

			TextBox control0 = new TextBox();
			TextBox control1 = new TextBox();
			TextBox control2 = new TextBox();
			Rows[0].AddControl(control0);
			Rows[1].AddControl(control1);
			Rows[2].AddControl(control2);
			AssertEquals(0, Rows.GetVisibleRowNumber(-1));

			AssertEquals(0, Rows.GetVisibleRowNumber(0));
			AssertEquals(1, Rows.GetVisibleRowNumber(1));
			AssertEquals(2, Rows.GetVisibleRowNumber(2));

			control1.Visible = false;
			AssertEquals(0, Rows.GetVisibleRowNumber(0));
			AssertEquals(designTime ? 1 : -1, Rows.GetVisibleRowNumber(1));
			AssertEquals(designTime ? 2 : 1, Rows.GetVisibleRowNumber(2));

			control1.Visible = true;
			AssertEquals(0, Rows.GetVisibleRowNumber(0));
			AssertEquals(1, Rows.GetVisibleRowNumber(1));
			AssertEquals(2, Rows.GetVisibleRowNumber(2));
		}

		public void TestGetRowNumber()
		{
			TestGetRowNumber(false);
		}

		public void TestGetRowNumber_AtDesignTime()
		{
			TestGetRowNumber(true);
		}

		void TestGetRowNumber(bool designTime)
		{
			Rows.Designing = designTime;

			TextBox control0 = new TextBox();
			TextBox control1 = new TextBox();
			TextBox control2 = new TextBox();
			Rows[0].AddControl(control0);
			Rows[1].AddControl(control1);
			Rows[2].AddControl(control2);

			AssertEquals(0, Rows.GetRowNumber(0));
			control1.Visible = false;
			AssertEquals(designTime ? 1 : 2, Rows.GetRowNumber(1));
			control1.Visible = true;
			AssertEquals(1, Rows.GetRowNumber(1));
		}

		public void TestGetRowNumberFromControl()
		{
			TextBox control0 = new TextBox();
			TextBox control1 = new TextBox();

			Rows[0].AddControl(control0);
			Rows[1].AddControl(control1);

			AssertEquals(0, Rows.GetRowNumberFromControl(control0));
			control1.Visible = false;
			AssertEquals(1, Rows.GetRowNumberFromControl(control1));

			control1.Dispose();
			AssertEquals(-1, Rows.GetRowNumberFromControl(control1));
		}

		#region Test Classes

		class TestRowLayoutRowCollection : RowLayoutRowCollection
		{
			public bool Designing { get; set; }

			protected override bool IsDesigning
			{
				get { return base.IsDesigning || Designing; }
			}
		}

		#endregion

		#region Implementation

		TestRowLayoutRowCollection Rows
		{
			get { return rows ?? (rows = new TestRowLayoutRowCollection()); }
		}
		TestRowLayoutRowCollection rows;

		#endregion
	}
}
