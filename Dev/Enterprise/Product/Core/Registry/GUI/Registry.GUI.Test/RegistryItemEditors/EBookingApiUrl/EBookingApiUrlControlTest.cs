using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EBookingApiUrlControl))]
	sealed class EBookingApiUrlControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new EBookingApiUrls();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((EBookingApiUrlControl)control).ReadOnly && ((EBookingApiUrlControl)control).PresetEBookingApiUrlDropEditInternal.ReadOnly;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;
	}
}
