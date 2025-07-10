using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Licensing.Billing.Business.Testing
{
	public class WritableFeatureControlStorageProviderTest : TransactionedTestCase
	{
		public void TestRegistry()
		{
			var storageProvider = new WritableFeatureControlStorageProvider();

			var sampleData = @"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>0001-01-01T00:00:00</TimestampUtc>
</FeatureControl>";

			AssertExceptionThrown(typeof(RegistryValidationException), "Invalid Feature Control String Setting", () => storageProvider.SetDataAsync(sampleData, default).Wait());

			var data = storageProvider.GetDataAsync(default).Result;

			AssertEquals(string.Empty, data);
			AssertEquals(string.Empty, WebDataRegistry.Instance.FeatureControlRuleContent.Value);

			var utcNowDate = ZDateTime.UtcNow.Date;
			var featureControl = new FeatureControl();
			var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
			featureControl.TimestampUtc = utcNowDate.ToDateTime();
			var ruleList = new List<FeatureControlRule>();

			ruleList.Add(FeatureControlTestHelper.AddRule("CR5RESWIZ", isGlobal: true, utcNowDate.AddDays(-3), utcNowDate.AddDays(10), "F001 - Global - day -3 ~ day 10"));
			featureControl.Rules = ruleList.ToArray();

			var compressedBase64StringData = FeatureControlTestHelper.GetCompressedBase64String(featureControl);
			AssertNoExceptionThrown(() => storageProvider.SetDataAsync(compressedBase64StringData, default).Wait());

			data = storageProvider.GetDataAsync(default).Result;
			AssertEquals(compressedBase64StringData, data);
			AssertEquals(compressedBase64StringData, WebDataRegistry.Instance.FeatureControlRuleContent.Value);
		}
	}
}
