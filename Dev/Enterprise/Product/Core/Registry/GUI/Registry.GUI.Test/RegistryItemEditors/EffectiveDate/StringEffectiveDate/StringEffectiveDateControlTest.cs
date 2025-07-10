using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(StringEffectiveDateControl))]
	sealed class StringEffectiveDateControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new StringEffectiveDate();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			bool result = false;
			StringEffectiveDate stringEffectiveDate = control.CurrentDataItem as StringEffectiveDate;
			if (stringEffectiveDate != null)
			{
				result = stringEffectiveDate.ReadOnly;
			}
			return result;
		}
	}
}
