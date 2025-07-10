using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(FallbackSettings))]
	public class FallbackSettingsTest : RegistryBusinessObjectTemplateTestCase<FallbackSettings>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override FallbackSettings GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override FallbackSettings GetBusinessObjectToSerialise()
		{
			fFallbackSettings = new FallbackSettings();
			fFallbackSettings.Start = fStart;
			fFallbackSettings.End = fFallbackSettings.Start.AddHours(6);
			fFallbackSettings.Regularisation = fFallbackSettings.End.AddHours(1);
			fFallbackSettings.RegularisationPeriod = 60;
			fFallbackSettings.RegularisationCount = 1;
			fFallbackSettings.InvocationReason = "Blabla";
			fFallbackSettings.RevocationReason = "Blabla";
			fFallbackSettings.RegularisationBatchSize = 10;

			return fFallbackSettings;
		}

		public void TestReadElements()
		{
			FallbackSettings setting = GetBusinessObjectToSerialise();

			IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(FallbackSettings));
			ZBlob serialisedValue = dummyDataType.Serialise(setting);
			FallbackSettings deserialisedBusinessObject = (FallbackSettings)dummyDataType.Deserialise(serialisedValue);

			AssertEquals(fStart.ToString(FallbackSettings.DateFormat, CultureInfo.InvariantCulture), deserialisedBusinessObject.Start.ToString(FallbackSettings.DateFormat, CultureInfo.InvariantCulture));
		}

		[TestDate(2020, 03, 8, 17, 0, 0)]
		public void TestFallbackSettingsValidation()
		{
			FallbackSettings fallbackSettings = new FallbackSettings();

			#region Fallback invocation

			fallbackSettings.ValidateAll();
			AssertHasMessageErrorContaining(fallbackSettings.StartInfo, FormattableString.Invariant($"{FallbackSettings.StartDateNotEmpty}"));
			AssertHasMessageErrorContaining(fallbackSettings.InvocationReasonInfo, FormattableString.Invariant($"{FallbackSettings.InvocationReasonNotEmpty}"));

			var now = ZDateTime.Now;

			fallbackSettings.Start = now;
			fallbackSettings.ValidateAll();
			AssertNoMessageErrorContaining(fallbackSettings.StartInfo, FormattableString.Invariant($"{FallbackSettings.StartDateNotEmpty}"));

			fallbackSettings.InvocationReason = "Invocation reason description";
			fallbackSettings.ValidateAll();
			AssertNoMessageErrorContaining(fallbackSettings.InvocationReasonInfo, FormattableString.Invariant($"{FallbackSettings.InvocationReasonNotEmpty}"));

			#endregion

			#region Fallback revocation
			fallbackSettings.End = now.AddDays(1);
			fallbackSettings.RevocationReason = ZString.Empty;
			fallbackSettings.Regularisation = ZDateTime.Empty;
			fallbackSettings.RegularisationPeriod = ZInt.Zero;
			fallbackSettings.ValidateAll();
			AssertHasMessageErrorContaining(fallbackSettings.RevocationReasonInfo, FormattableString.Invariant($"{FallbackSettings.RevocationReasonNotEmpty}"));
			AssertHasMessageErrorContaining(fallbackSettings.RegularisationInfo, FormattableString.Invariant($"{FallbackSettings.RegularisationNotEmpty}"));
			AssertHasMessageErrorContaining(fallbackSettings.RegularisationPeriodInfo, FormattableString.Invariant($"{FallbackSettings.RegularisationPeriodNotEmpty}"));

			fallbackSettings.RevocationReason = "Revocation reason description";
			fallbackSettings.Regularisation = fallbackSettings.End.AddHours(5);
			fallbackSettings.RegularisationPeriod = 60;
			fallbackSettings.ValidateAll();

			AssertNoMessageErrorContaining(fallbackSettings.EndInfo, FormattableString.Invariant($"{FallbackSettings.EndDateNotEmpty}"));
			AssertNoMessageErrorContaining(fallbackSettings.RevocationReasonInfo, FormattableString.Invariant($"{FallbackSettings.RevocationReasonNotEmpty}"));
			AssertNoMessageErrorContaining(fallbackSettings.RegularisationInfo, FormattableString.Invariant($"{FallbackSettings.RegularisationNotEmpty}"));
			AssertNoMessageErrorContaining(fallbackSettings.RegularisationPeriodInfo, FormattableString.Invariant($"{FallbackSettings.RegularisationPeriodNotEmpty}"));
			AssertNoMessageErrorContaining(fallbackSettings.EndInfo, FormattableString.Invariant($"{FallbackSettings.EndDateNotEmpty}"));

			fallbackSettings.Regularisation = fallbackSettings.End.AddHours(-10);
			fallbackSettings.ValidateAll();

			AssertNoMessageErrorContaining(fallbackSettings.EndInfo, FormattableString.Invariant($"{FallbackSettings.EndDateNotEmpty}"));
			AssertHasMessageErrorContaining(fallbackSettings.RegularisationInfo, FormattableString.Invariant($"{FallbackSettings.RegularisationAfterEnd}"));

			AssertNoMessageErrorContaining(fallbackSettings.EndInfo, FormattableString.Invariant($"{FallbackSettings.EndAfterStart}"));
			fallbackSettings.End = now.AddDays(-1);

			AssertHasMessageErrorContaining(fallbackSettings.EndInfo, FormattableString.Invariant($"{FallbackSettings.EndAfterStart}"));

			fallbackSettings.Start = ZDateTime.Empty;
			fallbackSettings.End = now.AddDays(1);
			fallbackSettings.ValidateAll();
			AssertHasMessageErrorContaining(fallbackSettings.StartInfo, FormattableString.Invariant($"{FallbackSettings.StartDateNotEmpty}"));

			fallbackSettings.Start = now;
			fallbackSettings.End = ZDateTime.Empty;
			fallbackSettings.ValidateAll();
			AssertHasMessageErrorContaining(fallbackSettings.EndInfo, FormattableString.Invariant($"{FallbackSettings.EndDateNotEmpty}"));

			#endregion
		}

		readonly ZDateTime fStart = new ZDateTime("20/03/2020 10:00:00");
		FallbackSettings fFallbackSettings;
	}
}
