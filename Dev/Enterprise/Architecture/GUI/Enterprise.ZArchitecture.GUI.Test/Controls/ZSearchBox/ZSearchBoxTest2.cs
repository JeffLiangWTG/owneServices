using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.SearchBox;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZSearchBoxTestMocked : TestCase
	{
		Mock<ZSearchBox> MockSearchBox;

		#region Setup

		protected override void SetUp()
		{
			MockSearchBox = GetFormMock();
		}

		static Mock<ZSearchBox> GetFormMock()
		{
			//var mockForm = new Mock<ZSearchBox>(MockBehavior.Strict); // Strict means the test will assert if any method not expected is called
			var mockForm = new Mock<ZSearchBox>(MockBehavior.Loose); // Strict means the test will assert if any method not expected is called
			mockForm.CallBase = true;
			AssertNotNull(mockForm.Object); // this is required to 'initialise' the mocked object once since lazy initialisation seems to be used
			mockForm.Invocations.Clear();
			return mockForm;
		}

		protected override void TearDown()
		{
			MockSearchBox.VerifyAll();
			MockSearchBox.Object.Dispose();
		}

		#endregion

		public void TestPerformSearchEmptyString()
		{
			// arrange
			MockSearchBox.Object.tbSearch.Text = string.Empty;

			// act
			ZSearchBoxForTest.WaitForSearch(MockSearchBox.Object);

			// assert
			MockSearchBox.Verify(x => x.ShowResults(), Times.Once);
			Assert(true); // keeps nunit happy
		}

		public void TestPerformSearchEqualToLast()
		{
			// arrange
			MockSearchBox.Object.tbSearch.Text = "TestPerformSearchEmptyString";
			MockSearchBox.Object.lastSearch = "TestPerformSearchEmptyString";

			// act
			ZSearchBoxForTest.WaitForSearch(MockSearchBox.Object);

			// assert
			MockSearchBox.Verify(x => x.ShowResults(), Times.Once);
			Assert(true); // keeps nunit happy
		}

		public void TestbtnSearch_Click()
		{
			// arrange
			MockSearchBox.Object.SearchResultsDisplay.Visible = true;

			// act
			MockSearchBox.Object.btnSearch.PerformClick();

			// assert
			MockSearchBox.Verify(x => x.HideResults(), Times.Once);

			// act
			MockSearchBox.Object.SearchResultsDisplay.Visible = false;

			MockSearchBox.Object.btnSearch.PerformClick();
			ZSearchBoxForTest.WaitForSearch(MockSearchBox.Object, false);

			// assert
			MockSearchBox.Verify(x => x.PerformSearch(), Times.Once);
			Assert(true); // keeps nunit happy
		}

		public void TesttbSearch_TextChanged_WithNormalSearch()
		{
			// arrange
			MockSearchBox.Object.AutoSearch = false;
			MockSearchBox.Object.tbSearch.Text = "completely different";

			// assert
			MockSearchBox.Verify(x => x.PerformSearch(), Times.Never);
			Assert(true); // keeps nunit happy
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		public void TesttbSearch_TextChanged_WithAutoSearch()
		{
			// arrange
			MockSearchBox.CallBase = false; // don't call PerformSearch real implementation
			MockSearchBox.Object.AutoSearch = true;
			MockSearchBox.Object.tbSearch.Text = "something different";

			// act
			var count = 0;
			const int MaxRetries = 5;
			while (MockSearchBox.Object.AutoSearchTimer.Enabled && count++ < MaxRetries)
			{
				Thread.Sleep(MockSearchBox.Object.AutoSearchDelay);
				Application.DoEvents();
			}

			// assert
			Assert("Test timed out waiting for search to return", count <= MaxRetries);
			Assert("Search timer should be disabled after it fires", !MockSearchBox.Object.AutoSearchTimer.Enabled);

			MockSearchBox.Verify(x => x.PerformSearch(), Times.Once);
			MockSearchBox.VerifyAll();

			MockSearchBox.CallBase = true; // set it back to true so the real Dispose can be called
			Assert(true); // keeps nunit happy
		}
	}
}
