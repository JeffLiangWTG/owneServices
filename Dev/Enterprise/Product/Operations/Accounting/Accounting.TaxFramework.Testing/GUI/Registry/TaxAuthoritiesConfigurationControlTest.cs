using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.GUI.Testing
{
	[TestedType(typeof(TaxAuthoritiesConfigurationControl))]
	class TaxAuthoritiesConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TaxAuthoritiesConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ZGrid)control.Controls.Find("TaxAuthoritiesGrid", true)[0]).ReadOnly;
		}
	}
}
