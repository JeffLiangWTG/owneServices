using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.GUI.Testing
{
	[TestedType(typeof(TaxSystemsConfigurationControl))]
	class TaxSystemsConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TaxSystemsConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ZGrid)control.Controls.Find("TaxSystemsGrid", true)[0]).ReadOnly;
		}
	}
}
