using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI
{
	sealed class PageSelectorControlTest : TestCase
	{
		Mock<PageSelectorControl> MockPSC;

		#region Setup

		protected override void SetUp()
		{
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			MockPSC = GetMockPSC();
		}

		static Mock<PageSelectorControl> GetMockPSC()
		{
			var mockPSC = new Mock<PageSelectorControl>
			{
				CallBase = true
			};
			AssertNotNull(mockPSC.Object); // this is required to 'initialise' the mocked object once since lazy initialisation seems to be used
			mockPSC.Invocations.Clear();
			return mockPSC;
		}

		protected override void TearDown() => MockPSC.Object.Dispose();

		#endregion

		public void TestNumericEventHandlerSet()
		{
			// arrange
			var mockPSC = MockPSC.Object;
			mockPSC.PageNumericUpDown.Minimum = 1;
			mockPSC.PageNumericUpDown.Maximum = 5;
			MockPSC.Invocations.Clear();

			// act
			mockPSC.PageNumericUpDown.Value = 2;

			// assert
			MockPSC.Verify(x => x.ChangeCurrentPage(), Times.Once);
			Assert(true);
		}

		public void TestChangeCurrentPageCallsEventHandler()
		{
			using (var psc = new PageSelectorControl())
			{
				// arrange
				var wasCalled = false;
				psc.CurrentPageChanged += (a, b) => wasCalled = true;
				Assert("CurrentPageChanged handler should not be called yet", !wasCalled);

				// act - assert
				psc.Enabled = false;
				psc.ChangeCurrentPage();
				Assert("CurrentPageChanged handler should not be called but it was", !wasCalled);

				// act - assert
				psc.Enabled = true;
				psc.ChangeCurrentPage();
				Assert("CurrentPageChanged handler should be called but it wasn't", wasCalled);
			}
		}
	}
}
