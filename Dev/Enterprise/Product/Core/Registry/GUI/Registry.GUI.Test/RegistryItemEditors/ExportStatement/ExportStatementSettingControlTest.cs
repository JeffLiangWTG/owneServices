using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ExportStatementSettingControl))]
	sealed class ExportStatementSettingControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CountryExportStatementSettingCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			ExportStatementSettingControl exportStatementSettingControl = (ExportStatementSettingControl)control;
			return exportStatementSettingControl.CountryExportStatementSettingGridInternal.ReadOnly &&
				exportStatementSettingControl.ExportStatementSettingGridInternal.ReadOnly &&
				!exportStatementSettingControl.AirGroupBoxInternal.Enabled &&
				!exportStatementSettingControl.SeaGroupBoxInternal.Enabled;
		}
	}
}
