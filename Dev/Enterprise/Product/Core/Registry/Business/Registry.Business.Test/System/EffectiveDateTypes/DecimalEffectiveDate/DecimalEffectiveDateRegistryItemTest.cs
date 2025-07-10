using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DecimalEffectiveDateRegistryItem))]
	sealed class DecimalEffectiveDateRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<DecimalEffectiveDate>
	{
		protected override StronglyTypedRegistryItem<DecimalEffectiveDate, DecimalEffectiveDate> GetNewRegistryItem()
		{
			return new DecimalEffectiveDateRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override DecimalEffectiveDate ValidValue
		{
			get
			{
				DecimalEffectiveDate decimalEffectiveDate = new DecimalEffectiveDate();
				decimalEffectiveDate.PreviousValue = ZDecimal.Zero;
				decimalEffectiveDate.NewValue = ZDecimal.Zero;
				decimalEffectiveDate.EffectiveDate = ZDateTime.Today;
				return decimalEffectiveDate;
			}
		}
	}
}
