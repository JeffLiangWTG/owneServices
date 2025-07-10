using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StringEffectiveDateRegistryItem))]
	sealed class StringEffectiveDateRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<StringEffectiveDate>
	{
		protected override StronglyTypedRegistryItem<StringEffectiveDate, StringEffectiveDate> GetNewRegistryItem()
		{
			return new StringEffectiveDateRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override StringEffectiveDate ValidValue
		{
			get
			{
				StringEffectiveDate stringEffectiveDate = new StringEffectiveDate();
				stringEffectiveDate.PreviousValue = ZString.Empty;
				stringEffectiveDate.NewValue = ZString.Empty;
				stringEffectiveDate.EffectiveDate = ZDateTime.Today;
				return stringEffectiveDate;
			}
		}
	}
}
