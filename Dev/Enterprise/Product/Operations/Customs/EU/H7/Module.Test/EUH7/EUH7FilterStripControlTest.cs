using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	class EUH7FilterStripControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCustomsStatusIsAddedToGrid()
		{
			using (var module = new EUH7Module())
			{ 
				var userControl = (EUH7FilterStripControl)module.EmbeddedControl;
				var grid = userControl.FilteredGrid;
				AssertNotNull(grid.GetColumnStyle("RegistrationStatus"));
			}
		}
	}
}
