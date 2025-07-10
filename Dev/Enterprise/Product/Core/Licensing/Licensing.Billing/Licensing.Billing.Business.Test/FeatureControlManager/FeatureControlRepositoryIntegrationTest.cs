using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Licensing.Billing.Business.Testing
{
	public class FeatureControlRepositoryIntegrationTest : TransactionedTestCase
	{
		public void TestGetFeatureControlTimestampUtc()
		{
			FeatureControlTestHelper.SetRegistryValue(null);
			var repository = ObjectFactory.Get<IFeatureControlRuleRepository>();
			var timestamp = repository.GetFeatureControlTimestampUtc();
			AssertEquals(DateTime.MinValue, timestamp);
			AssertEquals(DateTimeKind.Utc, timestamp.Kind);

			FeatureControlTestHelper.SetRegistryValue("");
			timestamp = repository.GetFeatureControlTimestampUtc();
			AssertEquals(DateTime.MinValue, timestamp);
			AssertEquals(DateTimeKind.Utc, timestamp.Kind);

			AssertExceptionThrown(typeof(RegistryValidationException), "Invalid Feature Control String Setting", () => FeatureControlTestHelper.SetRegistryValue("something wrong!"));

			var featureControl = new FeatureControl();
			featureControl.TimestampUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			FeatureControlTestHelper.SetRegistryValue(FeatureControlTestHelper.GetCompressedBase64String(featureControl));
			timestamp = repository.GetFeatureControlTimestampUtc();
			AssertEquals(featureControl.TimestampUtc, timestamp);
			AssertEquals(DateTimeKind.Utc, timestamp.Kind);

			featureControl = new FeatureControl();
			featureControl.TimestampUtc = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
			FeatureControlTestHelper.SetRegistryValue(FeatureControlTestHelper.GetCompressedBase64String(featureControl));
			timestamp = repository.GetFeatureControlTimestampUtc();
			AssertEquals(featureControl.TimestampUtc, timestamp);
			AssertEquals(DateTimeKind.Utc, timestamp.Kind);
		}

		public void TestSaveFeatureControlRuleContent()
		{
			var featureControl = new FeatureControl();
			featureControl.TimestampUtc = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
			var base64String = FeatureControlTestHelper.GetCompressedBase64String(featureControl);

			var repository = ObjectFactory.Get<IFeatureControlRuleRepository>();
			AssertEquals(true, repository.SaveFeatureControlRuleContent(Convert.FromBase64String(base64String)));
			AssertRegistryValue(base64String);

			AssertEquals(true, repository.SaveFeatureControlRuleContent(null));
			AssertRegistryValue("");

			AssertEquals(true, repository.SaveFeatureControlRuleContent(Array.Empty<byte>()));
			AssertRegistryValue("");

			AssertEquals(true, repository.SaveFeatureControlRuleContent(Convert.FromBase64String(base64String)));
			AssertRegistryValue(base64String);

			AssertEquals(false, repository.SaveFeatureControlRuleContent([1, 2, 3]));
			AssertRegistryValue(base64String);
			AssertEquals(nameof(FeatureControlRuleRepository), ErrorReporter.LastKeyReported);
			AssertEquals("Found invalid data while decoding.", ErrorReporter.LastExceptionReported.Message);
			AssertEquals("FeatureControlDeserializer.GetFeatureControlCore() rule=AQID", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		static void AssertRegistryValue(string valueExpected) =>
			AssertEquals(valueExpected, WebDataRegistry.Instance.FeatureControlRuleContent.Value);
	}
}
