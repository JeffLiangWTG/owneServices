using System;

namespace Enterprise.Accounting.Module.Testing
{
	public class JobConsolCostingControllerForTest : JobConsolCostingController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(DummyBusinessObjectWithNavigationProvider);
	}
}
