using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AWBLabelCustomisationRegistryControl))]
	sealed class AWBLabelCustomisationRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AWBLabelCustomisation();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			bool allControlsReadonly = true;
			foreach (Control control in control1.Controls)
			{
				if (control.Enabled)
				{
					allControlsReadonly = false;
					break;
				}
			}
			return ((AWBLabelCustomisationRegistryControl)control1).ReadOnly && allControlsReadonly;
		}
	}
}
