using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(TestRigRegistryControl))]
	public class TestRigRegistryControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TestRigRegistryHeader();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var testRigControl = (TestRigRegistryControl)control;
			return testRigControl.Grid.ReadOnly;
		}
	}
}
