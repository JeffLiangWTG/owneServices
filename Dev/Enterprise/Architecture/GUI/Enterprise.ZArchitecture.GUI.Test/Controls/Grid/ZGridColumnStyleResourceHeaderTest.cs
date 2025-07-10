using System.Drawing;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridColumnStyleResourceHeaderTest : TestCase
	{
		TestGridColumn column;
		TestZGridColumnStyleResourceHeader header;

		protected override void SetUp()
		{
			ContainerControl.Name = "ContainerControl";
			var grid = new ZGrid();
			ContainerControl.Controls.Add(grid);
			column = new TestGridColumn(grid);
			column.CaptionResourceString = new ResourceStringData("", "1", "134", "12345", "");
			header = new TestZGridColumnStyleResourceHeader(column);
		}

		TestContainerControl ContainerControl
		{
			get { return containerControl ?? (containerControl = new TestContainerControl()); }
		}
		TestContainerControl containerControl;

		protected override void TearDown()
		{
			column.Dispose();
			if (containerControl != null)
			{
				containerControl.Dispose();
			}
		}

		public void TestReturnsBaseTextIfItIsNotEmptyAndWasNotDefaulted()
		{
			AssertEquals("test", header.GetHeaderText("test", false));
		}

		public void TestReturnsBaseTextIfItIsNotEmptyAndWasDefaultedButNoResourceDataCanBeFound()
		{
			using (var emptyResourceStringCacheInitialized = Res.UseMockData())
			{
				AssertEquals("test", header.GetHeaderText("test", true));
			}
		}

		public void TestReturnsEmptyStringWhenNoResourceDataIsFound()
		{
			using (var emptyResourceStringCacheInitialized = Res.UseMockData())
			{
				AssertNull(header.GetHeaderText("", false));
			}
		}

		public void TestReturnsBestFitStringWhenResourceDataIsFound()
		{
			header.BestToFitResult = "123456";
			AssertEquals("123456", header.GetHeaderText("1234", true));

			AssertEquals(column.Width - 8, header.Width);
			AssertEquals(column.HeaderFont, header.Font);
			AssertArrayEqualsByElements(column.CaptionResourceString.GetCaptions(), header.Captions);
		}

		public void TestReturnsCaptionResourceString()
		{
			using (var grid = new ZGrid())
			using (var columnStyle = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo() { CaptionResourceString = new ResourceStringData("T", "Test") }))
			{
				columnStyle.HeaderFont = grid.HeaderFont;
				AssertEquals("Test", columnStyle.ResourceHeader.GetHeaderText(string.Empty, false));
			}
		}

		#region Test Classes

		class TestZGridColumnStyleResourceHeader : ZGridColumnStyleResourceHeader
		{
			public string[] Captions;
			public int Width;
			public Font Font;

			public string BestToFitResult;

			public TestZGridColumnStyleResourceHeader(ZTextBoxColumnStyle column)
				: base(column)
			{
			}

			protected override string BestToFit(string[] captions, int width, Font font)
			{
				Captions = captions;
				Width = width;
				Font = font;
				return BestToFitResult;
			}
		}

		class TestContainerControl : ZUserControl
		{
		}

		class TestGridColumn : ZTextBoxColumnStyle
		{
			public TestGridColumn(DataGrid grid)
				: base(new ZTextBoxColumnStyleInfo())
			{
				base.SetDataGridInColumn(grid);
			}
		}

		#endregion
	}
}
