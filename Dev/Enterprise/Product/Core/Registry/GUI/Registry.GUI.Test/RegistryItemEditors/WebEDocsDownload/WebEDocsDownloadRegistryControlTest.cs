using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebEDocsDownloadRegistryControl))]
	sealed class WebEDocsDownloadRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((WebEDocsDownloadRegistryControl)control).Grid.ReadOnly;
		}
	}
}
