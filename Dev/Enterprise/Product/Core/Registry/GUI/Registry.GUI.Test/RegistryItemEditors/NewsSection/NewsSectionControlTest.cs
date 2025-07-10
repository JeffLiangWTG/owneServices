using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(NewsSectionControl))]
	sealed class NewsSectionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new NewsSectionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var allControlsReadonly = true;
			foreach (ZGrid control in control1.Controls.OfType<ZGrid>())
			{
				if (!control.ReadOnly)
				{
					allControlsReadonly = false;
					break;
				}
			}

			return ((NewsSectionControl)control1).ReadOnly && allControlsReadonly;
		}
	}
}
