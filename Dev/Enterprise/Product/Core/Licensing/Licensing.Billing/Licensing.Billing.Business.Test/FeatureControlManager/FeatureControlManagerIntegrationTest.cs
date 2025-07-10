using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Licensing.Billing.Business.Testing
{
	public class FeatureControlManagerIntegrationTest : TransactionedTestCase
	{
		public void TestGetFeatureData()
		{
			var utcNowDate = ZDateTime.UtcNow.Date;
			var featureControl = new FeatureControl();
			var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
			featureControl.TimestampUtc = utcNowDate.ToDateTime();
			var ruleList = new List<FeatureControlRule>();

			ruleList.Add(FeatureControlTestHelper.AddRule("CR5RESWIZ", isGlobal: true, utcNowDate.AddDays(-3), utcNowDate.AddDays(10), "F001 - Global - day -3 ~ day 10"));
			featureControl.Rules = ruleList.ToArray();

			FeatureControlTestHelper.SetRegistryValue(FeatureControlTestHelper.GetCompressedBase64String(featureControl));
			FeatureControlTestHelper.AssertGetFeatureData(utcNowDate, "CR5RESWIZ", "F001 - Global - day -3 ~ day 10");
		}

		public void TestGetFeatureDataShouldKeepOriginalWhenTryToSaveInvalidContent()
		{
			var utcNowDate = ZDateTime.UtcNow.Date;
			var featureControl = new FeatureControl();
			var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
			featureControl.TimestampUtc = utcNowDate.ToDateTime();
			var ruleList = new List<FeatureControlRule>();

			ruleList.Add(FeatureControlTestHelper.AddRule("CR5RESWIZ", isGlobal: true, utcNowDate.AddDays(-3), utcNowDate.AddDays(10), "F001 - Global - day -3 ~ day 10"));
			featureControl.Rules = ruleList.ToArray();

			FeatureControlTestHelper.SetRegistryValue(FeatureControlTestHelper.GetCompressedBase64String(featureControl));
			FeatureControlTestHelper.AssertGetFeatureData(utcNowDate, "CR5RESWIZ", "F001 - Global - day -3 ~ day 10");

			AssertExceptionThrown(typeof(RegistryValidationException), "Invalid Feature Control String Setting", () => FeatureControlTestHelper.SetRegistryValue("invalid rules"));

			AssertNotNull(featureControlManager.GetFeatureData("CR5RESWIZ"));
			FeatureControlTestHelper.AssertGetFeatureData(utcNowDate, "CR5RESWIZ", "F001 - Global - day -3 ~ day 10");
		}
	}
}
