using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(UPEBranchIDsControl))]
	class UPEBranchIDsControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new UPEBranchIDsRegistryObjectCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((UPEBranchIDsControl)control).UPEBranchIDsGridForTest.ReadOnly;
		}
	}
}
