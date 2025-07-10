using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(FallbackConfiguration))]
sealed class FallbackConfigurationTest : RegistryBusinessObjectTemplateTestCase<FallbackConfiguration>
{
	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override FallbackConfiguration GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

	protected override FallbackConfiguration GetBusinessObjectToSerialise() => FallbackConfigurationTestHelper.GetFallbackConfiguration(new ZDateTime("18/12/2024 10:00:00"));

	public void TestReadElements()
	{
		var config = GetBusinessObjectToSerialise();

		IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(FallbackConfiguration));
		ZBlob serialisedValue = dummyDataType.Serialise(config);
		var deserialisedBusinessObject = (FallbackConfiguration)dummyDataType.Deserialise(serialisedValue);

		AssertEquals(new ZDateTime("18/12/2024 10:00:00").ToString(FallbackConfiguration.DateFormat, CultureInfo.InvariantCulture), deserialisedBusinessObject.Start.ToString(FallbackConfiguration.DateFormat, CultureInfo.InvariantCulture));
	}

	[TestDate(2024, 12, 18, 00, 00, 00)]
	public void TestFallbackConfigurationValidation()
	{
		var config = new FallbackConfiguration();

		config.ValidateAll();
		AssertHasMessageErrorContaining(config.StartInfo, FormattableString.Invariant($"{FallbackConfiguration.StartDateNotEmpty}"));
		AssertHasMessageErrorContaining(config.InvocationReasonInfo, FormattableString.Invariant($"{FallbackConfiguration.InvocationReasonNotEmpty}"));

		config.Start = ZDateTime.Now;
		config.ValidateAll();
		AssertNoMessageErrorContaining(config.StartInfo, FormattableString.Invariant($"{FallbackConfiguration.StartDateNotEmpty}"));

		config.InvocationReason = "Invocation reason";
		config.ValidateAll();
		AssertNoMessageErrorContaining(config.InvocationReasonInfo, FormattableString.Invariant($"{FallbackConfiguration.InvocationReasonNotEmpty}"));

		config.End = ZDateTime.Now.AddDays(1);
		config.RevocationReason = ZString.Empty;
		config.Regularisation = ZDateTime.Empty;
		config.RegularisationPeriod = ZInt.Zero;
		config.ValidateAll();
		AssertHasMessageErrorContaining(config.RevocationReasonInfo, FormattableString.Invariant($"{FallbackConfiguration.RevocationReasonNotEmpty}"));
		AssertHasMessageErrorContaining(config.RegularisationInfo, FormattableString.Invariant($"{FallbackConfiguration.RegularisationNotEmpty}"));
		AssertHasMessageErrorContaining(config.RegularisationPeriodInfo, FormattableString.Invariant($"{FallbackConfiguration.RegularisationPeriodNotEmpty}"));

		config.RevocationReason = "Revocation reason description";
		config.Regularisation = config.End.AddHours(5);
		config.RegularisationPeriod = 60;
		config.ValidateAll();

		AssertNoMessageErrorContaining(config.EndInfo, FormattableString.Invariant($"{FallbackConfiguration.EndDateNotEmpty}"));
		AssertNoMessageErrorContaining(config.RevocationReasonInfo, FormattableString.Invariant($"{FallbackConfiguration.RevocationReasonNotEmpty}"));
		AssertNoMessageErrorContaining(config.RegularisationInfo, FormattableString.Invariant($"{FallbackConfiguration.RegularisationNotEmpty}"));
		AssertNoMessageErrorContaining(config.RegularisationPeriodInfo, FormattableString.Invariant($"{FallbackConfiguration.RegularisationPeriodNotEmpty}"));
		AssertNoMessageErrorContaining(config.EndInfo, FormattableString.Invariant($"{FallbackConfiguration.EndDateNotEmpty}"));

		config.Regularisation = config.End.AddHours(-10);
		config.ValidateAll();

		AssertNoMessageErrorContaining(config.EndInfo, FormattableString.Invariant($"{FallbackConfiguration.EndDateNotEmpty}"));
		AssertHasMessageErrorContaining(config.RegularisationInfo, FormattableString.Invariant($"{FallbackConfiguration.RegularisationAfterEnd}"));

		AssertNoMessageErrorContaining(config.EndInfo, FormattableString.Invariant($"{FallbackConfiguration.EndAfterStart}"));
		config.End = ZDateTime.Now.AddDays(-1);

		AssertHasMessageErrorContaining(config.EndInfo, FormattableString.Invariant($"{FallbackConfiguration.EndAfterStart}"));

		config.Start = ZDateTime.Empty;
		config.End = ZDateTime.Now.AddDays(1);
		config.ValidateAll();
		AssertHasMessageErrorContaining(config.StartInfo, FormattableString.Invariant($"{FallbackConfiguration.StartDateNotEmpty}"));

		config.Start = ZDateTime.Now;
		config.End = ZDateTime.Empty;
		config.ValidateAll();
		AssertHasMessageErrorContaining(config.EndInfo, FormattableString.Invariant($"{FallbackConfiguration.EndDateNotEmpty}"));
	}
}
