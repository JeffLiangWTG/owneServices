using CargoWise.EntityFramework;
using Enterprise.Client.TGE.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TGE.GUI.Testing
{
	[TestedType(typeof(TGEEventsRegistryItemControl))]
	public class TGEEventsRegistryItemControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			TGEEventsRegistryItemControl control = (TGEEventsRegistryItemControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new TGEEventRegistryBusinessObjectCollection();
		}
	}
}
