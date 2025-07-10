using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(CustomsIncoTermOverrideControl))]
	sealed class CustomsIncoTermOverrideControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CustomsIncoTermOverrideCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			CustomsIncoTermOverrideControl customsIncoTermOverrideControl = (CustomsIncoTermOverrideControl)control;
			return customsIncoTermOverrideControl.IncoTermsGridForTest.ReadOnly;
		}
	}
}
