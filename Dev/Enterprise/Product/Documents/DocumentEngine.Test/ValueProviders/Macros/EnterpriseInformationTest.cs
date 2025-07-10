using System.Globalization;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(EnterpriseInformation))]
	sealed class EnterpriseInformationTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <EnterpriseInformation>", !ValueProviderToTest.IsResponsibleForReplacing("<EnterpriseInformation>", Passes.FirstPass));
			Assert("should match <EnterpriseInformation.SomeProperty>", ValueProviderToTest.IsResponsibleForReplacing("<EnterpriseInformation.SomeProperty>", Passes.FirstPass));
			Assert("should match <enterpriseinformation.sOMEpROPERTY>", ValueProviderToTest.IsResponsibleForReplacing("<enterpriseinformation.sOMEpROPERTY>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var retriever = new EnterpriseInformationRetriever();
			AssertEquals(retriever.VersionNumber, ValueProviderToTest.GetReplacement("<EnterpriseInformation.VersionNumber>", Report));
			AssertEquals(retriever.DBServerName, ValueProviderToTest.GetReplacement("<EnterpriseInformation.DBServerName>", Report));
			AssertEquals("Invalid property name: <EnterpriseInformation.Whatever>", ValueProviderToTest.GetReplacement("<EnterpriseInformation.Whatever>", Report));
		}

		public void TestReleaseInfoDate()
		{
			string versionDate;
			var retriever = new EnterpriseInformationRetriever();

			using (Culture.SetTemporarily(Culture.Invariant))
			{
				versionDate = retriever.VersionDate;
			}

			using (Culture.SetTemporarily(new CultureInfo("zh-CN")))
			{
				AssertEquals(versionDate, ValueProviderToTest.GetReplacement("<EnterpriseInformation.VersionDate>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new EnterpriseInformation();
		}
	}
}
