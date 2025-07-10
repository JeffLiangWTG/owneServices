using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(AccTaxRateRegistryItem))]
	sealed class AccTaxRateCodeRegistryItemTest : AccTaxRateRegistryItemWrapperTestCase<Guid>
	{
		protected override Type AccTaxRateRegistryItemWrapperType
		{
			get { return typeof(AccTaxRateRegistryItem); }
		}

		protected override bool IsEqual(Guid actualTaxRatePK, object obtainedDefaultValue)
		{
			return (actualTaxRatePK == (Guid)obtainedDefaultValue);
		}

		protected override StronglyTypedRegistryItem<Guid, Guid> GetNewRegistryItem()
		{
			return new AccTaxRateRegistryItem("", null, null, null, "");
		}
	}
}
