using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Registry.GUI;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(UPEGlbGroupsControl))]
	class UPEGlbGroupsControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new UPEGlbGroupsRegistryObjectCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((UPEGlbGroupsControl)control).UPEGlbGroupsGridForTest.ReadOnly;
		}
	}
}
