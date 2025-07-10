using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutomaticContainerCreationControl))]
	sealed class AutomaticContainerCreationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new AutomaticContainerCreation();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;

		[RequiresSTA]
		public void TestSetControlOrBusinessEntityReadOnly()
		{
			using (var form = new ZForm())
			using (var automaticContainerCreationControl = new AutomaticContainerCreationControlForTest())
			{
				form.Controls.Add(automaticContainerCreationControl);
				form.Show();

				automaticContainerCreationControl.SetControlOrBusinessEntityReadOnly(true);

				AssertEquals(false, automaticContainerCreationControl.alwaysCreateRadioButton.ReadOnly);
				AssertEquals(false, automaticContainerCreationControl.createUpToATDOrShippingInstructionRadioButton.ReadOnly);
				AssertEquals(false, automaticContainerCreationControl.neverCreateRadioButton.ReadOnly);
			}
		}
	}
}
