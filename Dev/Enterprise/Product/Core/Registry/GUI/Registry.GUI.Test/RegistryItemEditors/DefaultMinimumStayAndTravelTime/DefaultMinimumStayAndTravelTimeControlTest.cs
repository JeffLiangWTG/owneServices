using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DefaultMinimumStayAndTravelTimeControl))]
	sealed class DefaultMinimumStayAndTravelTimeControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DefaultMinimumStayAndTravelTimeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DefaultMinimumStayAndTravelTimeControl)control).DefaultMinimumStayAndTravelTimeGrid.ReadOnly;
		}
	}
}
