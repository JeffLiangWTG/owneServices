using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(DecimalEffectiveDateControl))]
	sealed class DecimalEffectiveDateControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DecimalEffectiveDate();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			bool result = false;
			DecimalEffectiveDate decimalEffectiveDate = control.CurrentDataItem as DecimalEffectiveDate;
			if (decimalEffectiveDate != null)
			{
				result = decimalEffectiveDate.ReadOnly;
			}
			return result;
		}
	}
}
