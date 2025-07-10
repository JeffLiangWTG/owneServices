using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(NLCustomsRegistry))]
sealed class NLCustomsRegistryTest : RegistryItemSetTestCaseWithFactory<NLCustomsRegistry>
{
	public void TestSenderId()
	{
		TestGenericRegistryItem(ItemSet.SenderIDs,
			"NLSenderIDs",
			NLCustomsRegistry.Categories.Customs_Netherlands,
			"HTG Sender ID",
			"A unique sender ID for the message header of declarations.",
			RegistryStorageFlags.Company,
			RegistryOptions.Default);
	}

	public void TestCustomsMessageVersion()
	{
		TestGenericRegistryItem(ItemSet.CustomsMessageVersion,
			"NLCustomsMessageVersion",
			NLCustomsRegistry.Categories.Customs_Netherlands,
			"Customs Message Recipient IDs",
			"Current version of customs messages the company is configured at customs to submit.",
			RegistryStorageFlags.Company,
			RegistryOptions.Default);
		AssertContainsExactElementsInAnyOrder(MessageVersionRegistryCollection.DefaultCollection.Cast<MessageVersionRegistry>().Select(x => (x.DomainCode, x.TargetSystemName)),
			ItemSet.CustomsMessageVersion.DefaultValue.Cast<MessageVersionRegistry>().Select(x => (x.DomainCode, x.TargetSystemName)));
	}

	public void TestNLTestingSystem()
	{
		TestGenericRegistryItem(ItemSet.IsNLTestingSystem,
			"IsNLTestingSystem",
			RawDataRegistry.Categories.Customs_Netherlands,
			"Is NL Testing System?",
			"NL Messages be sent to the Test rather than Production System?",
			RegistryStorageFlags.Company,
			RegistryOptions.IsOnlyForSupport);
	}

	public void TestEntryNumberCustomisation()
	{
		TestGenericRegistryItem(ItemSet.EntryNumberCustomisation,
			"EntryNumberCustomisation",
			NLCustomsRegistry.Categories.Customs_Netherlands_DMS,
			"Entry Number Customization",
			"Override this value to customize how Entry Number values are formatted",
			RegistryStorageFlags.Company,
			RegistryOptions.Default
			);
	}

	public void TestNLNctsFallbackEntryNumberCustomisation()
	{
		TestGenericRegistryItem(ItemSet.NLNctsFallbackEntryNumberCustomisation,
			"NLNctsFallbackEntryNumberCustomisation",
			NLCustomsRegistry.Categories.Customs_Netherlands_DVA,
			"DVA Departure - Emergency procedure sequence numbers",
			"The sequence preferences for DVA Departure emergency procedure documents",
			RegistryStorageFlags.Company,
			RegistryOptions.Default
			);
	}

	public void TestCalCalculationMethod()
	{
		TestGenericRegistryItem(ItemSet.CalCalculationMethod,
			"NLCalCalculationMethod",
			NLCustomsRegistry.Categories.Customs_Netherlands_DVA,
			"CAL Calculation Method",
			"The default CAL calculation method for DVA declarations",
			RegistryStorageFlags.Company,
			RegistryOptions.Default);
		AssertContainsExactElementsInAnyOrder(CalCalculationMethodRegistryCollection.DefaultCollection.Cast<CalCalculationMethodRegistry>().Select(x => (x.CalculationMethodName, x.CalculationMethodValue, x.CalculationMethodDefault)),
			ItemSet.CalCalculationMethod.DefaultValue.Cast<CalCalculationMethodRegistry>().Select(x => (x.CalculationMethodName, x.CalculationMethodValue, x.CalculationMethodDefault)));
	}

	public void TestFallbackTimerInMinutes_DMSExport()
	{
		TestGenericRegistryItem(ItemSet.FallbackTimerInMinutes_DMSExport,
			"NLFallbackTimerInMinutesDMSExport",
			NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
			"DMS Export",
			"Fallback timer in minutes for DMS Export",
			RegistryStorageFlags.System | RegistryStorageFlags.Company);
	}

	public void TestFallbackTimerInMinutes_DMSImport()
	{
		TestGenericRegistryItem(ItemSet.FallbackTimerInMinutes_DMSImport,
			"NLFallbackTimerInMinutesDMSImport",
			NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
			"DMS Import",
			"Fallback timer in minutes for DMS Import",
			RegistryStorageFlags.System | RegistryStorageFlags.Company);
	}

	public void TestFallbackTimerInMinutes_DVA()
	{
		TestGenericRegistryItem(ItemSet.FallbackTimerInMinutes_DVA,
			"NLFallbackTimerInMinutesDVA",
			NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
			"DVA",
			"Fallback timer in minutes for DVA",
			RegistryStorageFlags.System | RegistryStorageFlags.Company);
	}

	public void TestFallbackTimerInMinutes_ECS()
	{
		TestGenericRegistryItem(ItemSet.FallbackTimerInMinutes_ECS,
			"NLFallbackTimerInMinutesECS",
			NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
			"ECS",
			"Fallback timer in minutes for ECS",
			RegistryStorageFlags.System | RegistryStorageFlags.Company);
	}

	public void TestFallbackTimerInMinutes_Portbase()
	{
		TestGenericRegistryItem(ItemSet.FallbackTimerInMinutes_Portbase,
			"NLFallbackTimerInMinutesPortbase",
			NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
			"Portbase",
			"Fallback timer in minutes for Portbase",
			RegistryStorageFlags.System | RegistryStorageFlags.Company);
	}

	public void TestFallbackConfiguration_DMSExport()
	{
		CombineAssertions(() =>
		{
			AssertType<FallbackConfigurationRegistryItem>("Type should be FallbackConfigurationRegistryItem", ItemSet.FallbackConfiguration_DMSExport);
			TestGenericRegistryItem(
				ItemSet.FallbackConfiguration_DMSExport,
				"NLFallbackConfigurationDMSExport",
				NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
				"DMS Export",
				"Fallback configuration for DMS Export",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		});
	}

	public void TestFallbackConfiguration_DMSImport()
	{
		CombineAssertions(() =>
		{
			AssertType<FallbackConfigurationRegistryItem>("Type should be FallbackConfigurationRegistryItem", ItemSet.FallbackConfiguration_DMSImport);
			TestGenericRegistryItem(
				ItemSet.FallbackConfiguration_DMSImport,
				"NLFallbackConfigurationDMSImport",
				NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
				"DMS Import",
				"Fallback configuration for DMS Import",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		});
	}

	public void TestFallbackConfiguration_DVA()
	{
		CombineAssertions(() =>
		{
			AssertType<FallbackConfigurationRegistryItem>("Type should be FallbackConfigurationRegistryItem", ItemSet.FallbackConfiguration_DVA);
			TestGenericRegistryItem(
				ItemSet.FallbackConfiguration_DVA,
				"NLFallbackConfigurationDVA",
				NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
				"DVA",
				"Fallback configuration for DVA",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		});
	}

	public void TestFallbackConfiguration_ECS()
	{
		CombineAssertions(() =>
		{
			AssertType<FallbackConfigurationRegistryItem>("Type should be FallbackConfigurationRegistryItem", ItemSet.FallbackConfiguration_ECS);
			TestGenericRegistryItem(
				ItemSet.FallbackConfiguration_ECS,
				"NLFallbackConfigurationECS",
				NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
				"ECS",
				"Fallback configuration for ECS",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		});
	}

	public void TestFallbackConfiguration_Portbase()
	{
		CombineAssertions(() =>
		{
			AssertType<FallbackConfigurationRegistryItem>("Type should be FallbackConfigurationRegistryItem", ItemSet.FallbackConfiguration_Portbase);
			TestGenericRegistryItem(
				ItemSet.FallbackConfiguration_Portbase,
				"NLFallbackConfigurationPortbase",
				NLCustomsRegistry.Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
				"Portbase",
				"Fallback configuration for Portbase",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		});
	}

	public void TestRegularizationTimerInMinutes()
	{
		TestGenericRegistryItem(ItemSet.FallbackRegularizationTimerInMinutes,
			"NLFallbackRegularizationTimerInMinutes",
			NLCustomsRegistry.Categories.Customs_Netherlands_Fallback,
			"Regularization timer in minutes",
			"Regularization timer in minutes",
			RegistryStorageFlags.System | RegistryStorageFlags.Company,
			5);
	}

	public void TestFallbackEmail()
	{
		TestRegistryItem(
			ItemSet.FallbackEmail,
			"FallbackEmail",
			NLCustomsRegistry.Categories.Customs_Netherlands_Fallback,
			"Send email to Customs during fallback",
			"The field below defines how the emails to Customs are sent to Customs during Fallback procedure",
			RegistryStorageFlags.Company,
			new EmailFallbackList(),
			EmailFallbackList.Codes.MNL);
	}

	public void TestEmailFallbackList()
	{
		var emailFallbackList = new EmailFallbackList();
		AssertEquals("Code as string", "AUT, MNL", emailFallbackList.CodesAsString);
	}

	public void TestAllRegistryItemsHaveNLCountryFilter()
	{
		CombineAssertions(() =>
		{
			foreach (IRegistryItem registryItem in AllItems)
			{
				Assert(registryItem.Name + ".CountryFilterPK", registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Netherlands));
			}
		});
	}
}
