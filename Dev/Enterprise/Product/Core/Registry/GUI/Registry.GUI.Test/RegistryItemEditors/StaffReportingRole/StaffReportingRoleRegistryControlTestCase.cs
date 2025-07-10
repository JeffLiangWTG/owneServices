using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StaffReportingRoleRegistryControl))]
	sealed class StaffReportingRoleRegistryControlTestCase : RegistryZUserControlTestCase
	{
		protected override CargoWise.EntityFramework.IBusiness GetNewBusinessEntity()
		{
			return new StaffReportingRoleCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((StaffReportingRoleRegistryControl)control).StaffReportingRoleGrid.ReadOnly;
		}
	}
}
