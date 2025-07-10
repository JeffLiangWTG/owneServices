using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageMinimumFee))]
	public class UsageMinimumFeeTest : RegistryBusinessObjectTemplateTestCase<UsageMinimumFee>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override UsageMinimumFee GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override UsageMinimumFee GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		UsageMinimumFee NewPopulatedBusinessObject() => new UsageMinimumFee(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
	}
}
