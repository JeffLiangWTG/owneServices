using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ChargeCodeRegistryItem))]
	sealed class ChargeCodeRegistryItemTest : ChargeCodeRegistryItemWrapperTestCase<Guid>
	{
		protected override Type ChargeCodeRegistryItemWrapperType
		{
			get { return typeof(ChargeCodeRegistryItem); }
		}

		protected override bool IsEqual(Guid actualChargeCodePK, object obtainedDefaultValue)
		{
			return (actualChargeCodePK == (Guid)obtainedDefaultValue);
		}
	}
}
