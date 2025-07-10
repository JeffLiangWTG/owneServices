using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ShareSequentialNumbersControl))]
	public class ShareSequentialNumbersControlRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ShareSequentialNumbersControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new ShareSequentialNumbersControl();
		}
	}
}
