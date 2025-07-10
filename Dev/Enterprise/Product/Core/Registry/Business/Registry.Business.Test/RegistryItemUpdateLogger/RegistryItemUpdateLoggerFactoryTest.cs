using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RegistryItemUpdateLoggerFactory))]
	public class RegistryItemUpdateLoggerFactoryTest : TestCaseWithFactory
	{
		public void TestBindLogger()
		{
			// Test Binding
			var testBoolean = new BooleanRegistryItem("testBoolean", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, false);
			AssertForBinding(testBoolean);

			var testCodePair = new CodePairRegistryItem("testCodePair", null, null, null, null, RegistryStorageFlags.All, RegistryOptions.Default);
			AssertForBinding(testCodePair);

			var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair("ABC", "XYZ");
				return lookUpList;
			});
			var testCodePairWithAdditionalEvent = new CodePairWithAdditionalEventRegistryItem("testCodePairWithAdditionalEvent", null, null, null, lookUpListProvider, false, false, new ComboBoxRegistryEditorInfo(lookUpListProvider), null, RegistryStorageFlags.System, RegistryOptions.Default, "ABC", false);
			AssertForBinding(testCodePairWithAdditionalEvent);

			var testCurrencyDecimalSeparator = new CurrencyDecimalSeparatorRegistryItem("testCurrencyDecimalSeparator", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
			AssertForBinding(testCurrencyDecimalSeparator);

			var testCurrencyGroupSeparator = new CurrencyGroupSeparatorRegistryItem("testCurrencyGroupSeparator", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
			AssertForBinding(testCurrencyGroupSeparator);

			var testCurrencyGroupSizesStringList = new CurrencyGroupSizesStringListRegistryItem("testCurrencyGroupSizesStringList", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
			AssertForBinding(testCurrencyGroupSizesStringList);

			var testDateTime = new DateTimeRegistryItem("testDateTime", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, DateTime.Now);
			AssertForBinding(testDateTime);

			var testDecimal = new DecimalRegistryItem("testDecimal", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 0);
			AssertForBinding(testDecimal);

			var testDeleteExpiredRates = new DeleteExpiredRatesRegistryItem("testDeleteExpiredRates", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, null);
			AssertForBinding(testDeleteExpiredRates);

			var testGmailOAuth2JsonFile = new GmailOAuth2JsonFileRegistryItem("testGmailOAuth2JsonFile", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
			AssertForBinding(testGmailOAuth2JsonFile);

			var testGuidArray = new GuidArrayRegistryItem("testGuidArray", null, null, null, RegistryStorageFlags.All);
			AssertForBinding(testGuidArray);

			var testGuid = new GuidRegistryItem("testGuid", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, Guid.Empty);
			AssertForBinding(testGuid);

			var testImage = new ImageRegistryItem("testImage", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, null);
			AssertForBinding(testImage);

			var testInt = new IntRegistryItem("testInt", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 0);
			AssertForBinding(testInt);

			var testJsonStringArray = new JsonStringArrayRegistryItem("testJsonStringArray", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default);
			AssertForBinding(testJsonStringArray);

			var testMultilingualString = new MultilingualStringRegistryItem("testMultilingualString", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default);
			AssertForBinding(testMultilingualString);

			var testNumberDecimalSeparator = new NumberDecimalSeparatorRegistryItem("testNumberDecimalSeparator", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
			AssertForBinding(testNumberDecimalSeparator);

			var testNumberGroupSeparator = new NumberGroupSeparatorRegistryItem("testNumberGroupSeparator", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
			AssertForBinding(testNumberGroupSeparator);

			var testNumberGroupSizesStringList = new NumberGroupSizesStringListRegistryItem("testNumberGroupSizesStringList", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
			AssertForBinding(testNumberGroupSizesStringList);

			var testQuoteValidity = new QuoteValidityRegistryItem("testQuoteValidity", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, 0);
			AssertForBinding(testQuoteValidity);

			var testRatesServiceUrl = new RatesServiceUrlRegistryItem("testRatesServiceUrl", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, "");
			AssertForBinding(testRatesServiceUrl);

			var testServiceUrl = new ServiceUrlRegistryItem("testServiceUrl", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, "");
			AssertForBinding(testServiceUrl);

			var testStringArray = new StringArrayRegistryItem("testStringArray", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default);
			AssertForBinding(testStringArray);

			var testString = new StringRegistryItem("testString", null, null, null, RegistryStorageFlags.All);
			AssertForBinding(testString);

			var testWebPrintNudge = new WebPrintNudgeRegistryItem("testWebPrintNudge", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, null);
			AssertForBinding(testWebPrintNudge);

			// Test Logging
			var booleanArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testBoolean, false, true);
			AssertForLogging(testBoolean, booleanArgs, "Boolean|OLD=False|NEW=True");

			var codePairArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testCodePair, "ABC", "XYZ");
			AssertForLogging(testCodePair, codePairArgs, "CodePair|OLD=ABC|NEW=XYZ");

			var codePairWithAdditionalEventArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testCodePairWithAdditionalEvent, "JKL", "OPQ");
			AssertForLogging(testCodePairWithAdditionalEvent, codePairWithAdditionalEventArgs, "CodePair|OLD=JKL|NEW=OPQ");

			var currencyDecimalSeparatorArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testCurrencyDecimalSeparator, ".", ",");
			AssertForLogging(testCurrencyDecimalSeparator, currencyDecimalSeparatorArgs, "Separator|OLD=.|NEW=,");

			var currencyGroupSeparatorArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testCurrencyGroupSeparator, "@", "#");
			AssertForLogging(testCurrencyGroupSeparator, currencyGroupSeparatorArgs, "Separator|OLD=@|NEW=#");

			var currencyGroupSizesStringListArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testCurrencyGroupSizesStringList, 3, 5);
			AssertForLogging(testCurrencyGroupSizesStringList, currencyGroupSizesStringListArgs, "Currency Group Size|OLD=3|NEW=5");

			var dateTimeArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testDateTime, new DateTime(2025, 03, 20), new DateTime(2025, 03, 21));
			AssertForLogging(testDateTime, dateTimeArgs, "DateTime|OLD=03/20/2025 00:00:00|NEW=03/21/2025 00:00:00");

			var decimalArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testDecimal, 12.34F, 78.9F);
			AssertForLogging(testDecimal, decimalArgs, "Decimal|OLD=12.34|NEW=78.9");

			var deleteExpiredRatesArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testDeleteExpiredRates,
				new DeleteExpiredRates { BatchSize = 20, ExpiredRatesPeriodInYears = 1 },
				new DeleteExpiredRates { BatchSize = 50, ExpiredRatesPeriodInYears = 3 });
			AssertForLogging(testDeleteExpiredRates, deleteExpiredRatesArgs, "Delete Expired Rates|OLD=[Years:1, Size:20]|NEW=[Years:3, Size:50]");

			var gmailOAuth2JsonFileArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testGmailOAuth2JsonFile,
				new GmailOAuth2JsonFile { JsonText = "", FileName = "abc.json" },
				new GmailOAuth2JsonFile { JsonText = "", FileName = "xyz.json" });
			AssertForLogging(testGmailOAuth2JsonFile, gmailOAuth2JsonFileArgs, "Json File|OLD=[abc.json]|NEW=[xyz.json]");

			var guidArrayArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testGuidArray, new[] { Guid.Empty }, new[] { Guid.Parse("fa6a9e9b-2a4f-4a6e-826b-69da2ccd6f17") });
			AssertForLogging(testGuidArray, guidArrayArgs, "GuidArray|OLD=[Count: 1, 00000000-0000-0000-0000-000000000000]|NEW=[Count: 1, fa6a9e9b-2a4f-4a6e-826b-69da2ccd6f17]");

			var guidArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testGuid, Guid.Empty, Guid.Parse("624ee0d0-6261-4ea8-b679-964a99c1b2ab"));
			AssertForLogging(testGuid, guidArgs, "Guid|OLD=00000000-0000-0000-0000-000000000000|NEW=624ee0d0-6261-4ea8-b679-964a99c1b2ab");

			var imageArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testImage, new Bitmap(100, 80), new Bitmap(200, 160));
			AssertForLogging(testImage, imageArgs, "Image|OLD=[Width: 100, Height: 80, Format: Format32bppArgb]|NEW=[Width: 200, Height: 160, Format: Format32bppArgb]");

			var intArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testInt, 9, 11);
			AssertForLogging(testInt, intArgs, "Int|OLD=9|NEW=11");

			var jsonStringArrayArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testJsonStringArray, new[] { "abc" }, null);
			AssertForLogging(testJsonStringArray, jsonStringArrayArgs, "String Array|OLD=[Count: 1, abc]|NEW=");

			var multilingualStringArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testMultilingualString, "User Flag 01", "User Flag 07");
			AssertForLogging(testMultilingualString, multilingualStringArgs, "Multilingual|OLD=User Flag 01|NEW=User Flag 07");

			var numberDecimalSeparatorArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testNumberDecimalSeparator, ".", ";");
			AssertForLogging(testNumberDecimalSeparator, numberDecimalSeparatorArgs, "Separator|OLD=.|NEW=;");

			var numberGroupSeparatorArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testNumberGroupSeparator, "*", "^");
			AssertForLogging(testNumberGroupSeparator, numberGroupSeparatorArgs, "Separator|OLD=*|NEW=^");

			var numberGroupSizesStringListArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testNumberGroupSizesStringList, 15, 0);
			AssertForLogging(testNumberGroupSizesStringList, numberGroupSizesStringListArgs, "Number Group Size|OLD=15|NEW=0");

			var quoteValidityArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testQuoteValidity, 0, 1);
			AssertForLogging(testQuoteValidity, quoteValidityArgs, "Quote|OLD=0|NEW=1");

			var ratesServiceUrlArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testRatesServiceUrl, "", "http://www.test.com");
			AssertForLogging(testRatesServiceUrl, ratesServiceUrlArgs, "Url|OLD=|NEW=http://www.test.com");

			var serviceUrlArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testServiceUrl, "", "http://www.cdz.com");
			AssertForLogging(testServiceUrl, serviceUrlArgs, "Url|OLD=|NEW=http://www.cdz.com");

			var stringArrayArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testStringArray, new[] { "abc", "xyz" }, null);
			AssertForLogging(testStringArray, stringArrayArgs, "String Array|OLD=[Count: 2, abc, xyz]|NEW=");

			var stringArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testString, "", "CDZ");
			AssertForLogging(testString, stringArgs, "String|OLD=|NEW=CDZ");

			var webPrintNudgeArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testWebPrintNudge,
				new WebPrintNudge { SwtichBackToIPAddressIntervalInHours = 12, EnableIPAddress = false },
				new WebPrintNudge { SwtichBackToIPAddressIntervalInHours = 24, EnableIPAddress = true });
			AssertForLogging(testWebPrintNudge, webPrintNudgeArgs, "Print Nudge|OLD=[IpEnabled: False, Hours: 12]|NEW=[IpEnabled: True, Hours: 24]");
		}

		public void TestNotBindLogger_ForPasswordItems()
		{
			var ms365OAuth2TenantId = RawDataRegistry.Instance.Ms365OAuth2TenantId;
			AssertForNotBinding(ms365OAuth2TenantId);

			var eAdaptorInboundAuthentications = eAdaptorRegistry.Instance.eAdaptorInboundAuthentications;
			AssertForNotBinding(eAdaptorInboundAuthentications);
		}

		public void TestLoadFromGenericType()
		{
			var testFilterLayoutCodePair = new FilterLayoutCodePairRegistryItem("testFilterLayoutCodePair", null, null, null, false, false, null, RegistryStorageFlags.System, RegistryOptions.Default, "", false);
			AssertForBinding(testFilterLayoutCodePair);

			var filterLayoutCodePairArgs = new RegistryItemWrapper.BuildLogReferenceArgs(testFilterLayoutCodePair, "CDZ", "TEST");
			AssertForLogging(testFilterLayoutCodePair, filterLayoutCodePairArgs, "String|OLD=CDZ|NEW=TEST");
		}

		void AssertForBinding(RegistryItemWrapper wrapper)
		{
			RegistryItemUpdateLoggerFactory.BindLogger(wrapper);
			AssertNotNull(wrapper.OnBuildLogReference);
		}

		void AssertForNotBinding(RegistryItemWrapper wrapper)
		{
			RegistryItemUpdateLoggerFactory.BindLogger(wrapper);
			AssertNull(wrapper.OnBuildLogReference);
		}

		void AssertForLogging(RegistryItemWrapper wrapper, RegistryItemWrapper.BuildLogReferenceArgs args,string expectedLog)
		{
			AssertEquals(expectedLog, wrapper.OnBuildLogReference.Invoke(args));
		}
	}
}
