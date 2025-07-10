using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(DbHealthWarningListControl))]
	class DbHealthWarningListControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DbHealthWarningRegistryCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(Enterprise.Registry.GUI.RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DbHealthWarningListControl)control).warningZGrid.ReadOnly;
		}
	}
}
