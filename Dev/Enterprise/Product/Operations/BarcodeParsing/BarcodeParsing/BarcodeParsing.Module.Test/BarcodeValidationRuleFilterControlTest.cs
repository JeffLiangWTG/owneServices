using CargoWise.EntityFramework.Testing;
using Enterprise.BarcodeParsing.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Module.Testing
{
	class BarcodeValidationRuleFilterControlTest : TestCaseWithFactory
	{
		#region TestGetNewFilterStripControl

		[ExpectNoExceptions]
		public void TestGetNewFilterStripControl()
		{
			using (var form = new ZForm())
			{
				var ruleCollection = new BarcodeValidationRuleCollection(Factory);
				var filterBO = new BarcodeValidationRuleFilterBusinessObject();
				var filterControl = new BarcodeValidationRuleFilterControl(ruleCollection, filterBO);

				form.Controls.Add(filterControl);
				form.Show();

				filterControl.AddNewFilterStrip();
			}
		}

		#endregion
	}
}
