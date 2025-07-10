using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Internal;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DecimalArrayControl))]
	sealed class DecimalArrayControlTest : Testing.RegistryZUserControlTestCase
	{
		public void TestGridDoesNotAllowSorting()
		{
			using (DecimalArrayControl control = new DecimalArrayControl())
			{
				AssertEquals("AllowSorting", false, control.DecimalGrid.AllowSorting);
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DecimalLineCollection(new DecimalArrayRegistryDataType());
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DecimalArrayControl)control).DecimalGrid.ReadOnly;
		}
	}
}
