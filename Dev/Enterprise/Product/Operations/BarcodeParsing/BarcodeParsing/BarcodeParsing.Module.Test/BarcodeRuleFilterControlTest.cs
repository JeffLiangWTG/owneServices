using CargoWise.EntityFramework.Testing;
using Enterprise.BarcodeParsing.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Module.Testing
{
	class BarcodeRuleFilterControlTest : TestCaseWithFactory
	{
		#region TestGetNewFilterStripControl

		[ExpectNoExceptions]
		public void TestGetNewFilterStripControl()
		{
			using (var form = new ZForm())
			{
				var transports = new BarcodeRuleCollection(Factory);
				var filterBO = new BarcodeRuleFilterBusinessObject();
				var filterControl = new BarcodeRuleFilterControl(transports, filterBO);

				form.Controls.Add(filterControl);
				form.Show();

				filterControl.AddNewFilterStrip();
			}
		}

		#endregion
	}
}
