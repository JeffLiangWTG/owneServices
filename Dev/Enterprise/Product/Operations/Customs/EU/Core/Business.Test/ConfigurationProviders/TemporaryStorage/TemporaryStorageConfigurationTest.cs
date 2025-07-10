using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(TemporaryStorageConfiguration))]
	public abstract class TemporaryStorageConfigurationAbstractTest<T> : TestCaseWithFactory where T : TemporaryStorageConfiguration, new()
	{
		public abstract void TestPreviousDocumentConfiguration();

		public abstract void TestSupportingDocumentConfiguration();

		public abstract void TestTemporaryStorageHeaderValidationDecider();

		public abstract void TestBillConfiguration();

		public abstract void TestPackedItemConfiguration();

		[ExpectNoExceptions]
		public void TestSupportLRNGeneration()
		{
			NUnit.Framework.Assert.That(configuration.SupportLRNGeneration, NUnit.Framework.Is.EqualTo(ExpectedSupportLRNGeneration), "SupportLRNGeneration");
		}

		protected abstract bool ExpectedSupportLRNGeneration { get; }

		[ExpectNoExceptions]
		public void TestSupportAgentDefaulting()
		{
			NUnit.Framework.Assert.That(configuration.SupportAgentDefaulting, NUnit.Framework.Is.EqualTo(ExpectedSupportAgentDefaulting), "SupportAgentDefaulting");
		}

		protected abstract bool ExpectedSupportAgentDefaulting { get; }

		[ExpectNoExceptions]
		public void TestTemporaryStorageHeaderDocumentWrapperClass()
		{
			NUnit.Framework.Assert.That(configuration.TemporaryStorageHeaderDocumentWrapperClass, NUnit.Framework.Is.EqualTo(TemporaryStorageHeaderDocumentWrapperClass), "TemporaryStorageHeaderDocumentWrapperClass");
		}

		protected abstract string TemporaryStorageHeaderDocumentWrapperClass { get; }

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new T();
		}

		protected T configuration;
	}

	[TestedType(typeof(TemporaryStorageConfiguration))]
	sealed class TemporaryStorageConfigurationTest : TemporaryStorageConfigurationAbstractTest<TemporaryStorageConfiguration>
	{
		[ExpectNoExceptions]
		public void TestGetConfiguration()
		{
			CombineAssertions(() =>
			{
				AssertConfigurationClassFullNameForCountry(countryCode: "", "Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageConfiguration");
				AssertConfigurationClassFullNameForCountry(Core.Constants.CountryCodes.EuropeanUnion, "Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageConfiguration");
				AssertConfigurationClassFullNameForCountry(Core.Constants.CountryCodes.France, "Enterprise.Customs.FR.Business.TemporaryStorageConfiguration");
				AssertConfigurationClassFullNameForCountry(Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageConfiguration");
				AssertConfigurationClassFullNameForCountry(Core.Constants.CountryCodes.Italy, "Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageConfiguration");
			});

			void AssertConfigurationClassFullNameForCountry(string countryCode, string expectedConfigurationClassFullName)
			{
				var configuration = TemporaryStorageConfiguration.GetConfiguration(Factory, countryCode);
				NUnit.Framework.Assert.That(configuration.GetType().FullName, NUnit.Framework.Is.EqualTo(expectedConfigurationClassFullName), $"Configuration class FullName for Country: '{countryCode}'");
			}
		}

		[ExpectNoExceptions]
		public override void TestPreviousDocumentConfiguration()
		{
			NUnit.Framework.Assert.That(configuration.PreviousDocumentConfiguration, NUnit.Framework.Is.TypeOf<TemporaryStoragePreviousDocumentConfiguration>());
		}

		[ExpectNoExceptions]
		public override void TestSupportingDocumentConfiguration()
		{
			NUnit.Framework.Assert.That(configuration.SupportingDocumentConfiguration, NUnit.Framework.Is.TypeOf<TemporaryStorageSupportingDocumentConfiguration>());
		}

		[ExpectNoExceptions]
		public override void TestTemporaryStorageHeaderValidationDecider()
		{
			NUnit.Framework.Assert.That(configuration.GetValidationDecider(), NUnit.Framework.Is.TypeOf<TemporaryStorageHeaderValidationDecider>());
		}

		[ExpectNoExceptions]
		public override void TestBillConfiguration()
		{
			NUnit.Framework.Assert.That(configuration.BillConfiguration, NUnit.Framework.Is.TypeOf<TemporaryStorageBillConfiguration>());
		}

		public override void TestPackedItemConfiguration()
		{
			AssertType<TemporaryStoragePackedItemConfiguration>(configuration.PackedItemConfiguration);
		}

		protected override bool ExpectedSupportLRNGeneration => true;

		protected override bool ExpectedSupportAgentDefaulting => true;

		protected override string TemporaryStorageHeaderDocumentWrapperClass => "Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageHeaderWrapper";
	}
}
