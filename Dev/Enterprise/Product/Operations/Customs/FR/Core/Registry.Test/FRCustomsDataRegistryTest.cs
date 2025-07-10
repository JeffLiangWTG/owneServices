using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(FRCustomsDataRegistry))]
	public class FRCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<FRCustomsDataRegistry>
	{
		public void TestAllRegistryItemsHaveDECountryFilter()
		{
			CombineAssertions(() =>
			{
				foreach (IRegistryItem registryItem in AllItems)
				{
					AssertContainsExactElementsInAnyOrder(registryItem.Name + ".CountryFilterPK", RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments, registryItem.CountryFilterPKs);
				}
			});
		}

		#region Various Entries

		[TestDate(2020, 01, 01, 12, 0, 0)]
		public void TestDeltaGFallbackInvocationDate()
		{
			var fallbackSetting = new FallbackSettings();

			Assert(!FRCustomsDataRegistry.DeltaGFallbackIsActive);

			fallbackSetting.End = ZDateTime.Today.AddDays(-1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
			AssertEquals("31/12/2019 00:00", FRCustomsDataRegistry.DeltaGFallbackInvocationDate.ToString("dd/MM/yyy HH:mm"));
		}

		public void TestRecipientIDRegistry()
		{
			TestGenericRegistryItem(
				ItemSet.RecipientID,
				"FRRecipientID",
				FRCustomsDataRegistry.Categories.Customs_France,
				"Recipient ID",
				"Recipient ID for Routing via eHub",
				RegistryStorageFlags.System
			);
		}

		public void TestCINSenderIDRegistry()
		{
			TestGenericRegistryItem(
				ItemSet.CINSenderID,
				"FRCINSenderID",
				FRCustomsDataRegistry.Categories.Customs_France,
				"CIN Sender ID",
				"CIN Sender ID",
				RegistryStorageFlags.Company
			);
		}

		public void TestCorrelationIDCustomisation()
		{
			TestGenericRegistryItem(
				ItemSet.CorrelationIDCustomisation,
				"CorrelationIDCustomisation",
				FRCustomsDataRegistry.Categories.Customs_France_CorrelationID,
				"Correlation ID Customization",
				"Override this value to customize how Correlation ID are formatted",
				RegistryStorageFlags.All,
				RegistryOptions.IsOnlyForSupport
				);

			AssertType<CorrelationIDCustomisationRegistryDataType>(ItemSet.CorrelationIDCustomisation.DataType);
		}

		public void TestFRStatementNumberCustomisation()
		{
			TestGenericRegistryItem(
				ItemSet.FRStatementNumberCustomisation,
				"FRStatementNumberCustomisation",
				FRCustomsDataRegistry.Categories.Customs_France_Liquidation,
				"Statement Number Customization",
				"Override this value to customize how Statement Job Numbers are formatted",
				RegistryStorageFlags.Company);

			AssertType<StatementNumberCustomisationRegistryDataType>(ItemSet.FRStatementNumberCustomisation.DataType);
		}

		#endregion

		#region CreditCheck

		public void TestCheckWhenSendingAnArrivedValideeRegistry()
		{
			TestGenericRegistryItem(
				ItemSet.CheckWhenSendingAnArrivedValidee,
				"CheckWhenSendingAnArrivedValidee",
				FRCustomsDataRegistry.Categories.Customs_France,
				"Check when sending an arrived validation",
				"Perform the credit check on declarations that do not have an entry number when sending a 'validate' entry.",
				RegistryStorageFlags.Company
			);
		}

		public void TestCheckWhenChangingAnAnticipateToAValideeRegistry()
		{
			TestGenericRegistryItem(
				ItemSet.CheckWhenChangingAnAnticipateToAValidee,
				"CheckWhenChangingAnAnticipateToAValidee",
				FRCustomsDataRegistry.Categories.Customs_France,
				"Check when changing an anticipate to a validation",
				"Perform the credit check on declarations that are 'anticipated' when sending a 'validate' entry.",
				RegistryStorageFlags.Company
			);
		}

		public void TestCheckAtEverySubmissionRegistry()
		{
			TestGenericRegistryItem(
				ItemSet.CheckAtEverySubmission,
				"CheckAtEverySubmission",
				FRCustomsDataRegistry.Categories.Customs_France,
				"Always check at every submission",
				"Always check the credit standing before sending a message. Overrides all other settings in this category.",
				RegistryStorageFlags.Company
			);
		}

		public void TestIncrementalThresholdForCheckingRegistry()
		{
			TestGenericRegistryItem(
				ItemSet.IncrementalThresholdForChecking,
				"IncrementalThresholdForChecking",
				FRCustomsDataRegistry.Categories.Customs_France,
				"Incremental threshold for checking",
				"Perform the credit check on declarations that have an entry number and whose estimated duty/VAT at the time of submission is greater than the already recorded fees by this percentage. Set to a negative number to suppress the check.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				10
			);
		}

		#endregion

		#region DeltaD
		public void TestNbDaysWaitingForSendBAECRegistry()
		{
			TestGenericRegistryItem(
				ItemSet.NbDaysWaitBeforeSendingDeltaDStep2,
				"FRNbDaysWaitBeforeSendingDeltaDStep2",
				FRCustomsDataRegistry.Categories.Customs_France,
				"Days to wait after BAE before automatically sending Delta G2 Second Step message",
				"Days to wait after BAE before automatically sending Delta G2 Second Step message",
				RegistryStorageFlags.Company
			);
		}
		#endregion

		#region Fallback procedure
		public void TestFallbackSettingRegistryItem()
		{
			TestGenericRegistryItem(
				ItemSet.FallbackTimerInMinutes,
				"FRFallbackTimerInMinutes",
				FRCustomsDataRegistry.Categories.Customs_France_Fallback,
				"Fallback timer in minutes",
				"Fallback timer in minutes",
				RegistryStorageFlags.System | RegistryStorageFlags.Company
			);

			TestGenericRegistryItem(
				ItemSet.RegularisationTimerInMinutes,
				"FRRegularisationTimerInMinutes",
				FRCustomsDataRegistry.Categories.Customs_France_Fallback,
				"Regularization timer in minutes",
				"Regularization timer in minutes",
				RegistryStorageFlags.System | RegistryStorageFlags.Company
				);

			TestGenericRegistryItem(
				ItemSet.CustomsFallbackConfiguration,
				"CustomsFallbackConfiguration",
				FRCustomsDataRegistry.Categories.Customs_France_Fallback,
				"Fallback Configuration (All)",
				"Fallback Configuration (All)",
				RegistryStorageFlags.System | RegistryStorageFlags.Company
			);
		}

		public void TestFallbackModeSettingRegistryItem()
		{
			FallbackSettings defaultFallbackSetting = null;
			FallbackSettings deltaGFallbackSetting = null;
			FallbackSettings deltaResetFallbackSetting = null;

			PrepareFallbackSettingsForDeltaModeTest(ref defaultFallbackSetting, ref deltaGFallbackSetting, ref deltaResetFallbackSetting);
			AssertNotEquals("Pre-req", null, defaultFallbackSetting);

			FRCustomsDataRegistry.Instance.CustomsFallbackConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultFallbackSetting);
			AssertEquals("Pre-req", defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.CustomsFallbackConfiguration.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);

			FRCustomsDataRegistry.Instance.CustomsFallbackConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultFallbackSetting);
			AssertEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.CustomsFallbackConfiguration.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
			AssertEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.DeltaGMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
			AssertEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.DeltaIMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
			AssertEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.DeltaTMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
			AssertEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.GammaMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
			AssertEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.DeltaXMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
			AssertEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.ICSMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
			AssertEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.ECSMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);

			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, deltaGFallbackSetting);
			AssertNotEquals(defaultFallbackSetting.Start, (FRCustomsDataRegistry.Instance.DeltaGMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);

			AssertEquals(deltaGFallbackSetting.Start, (FRCustomsDataRegistry.Instance.DeltaGMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);

			FRCustomsDataRegistry.Instance.CustomsFallbackConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, deltaResetFallbackSetting);

			AssertEquals(deltaGFallbackSetting.Start, (FRCustomsDataRegistry.Instance.DeltaGMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
			AssertEquals(deltaResetFallbackSetting.Start, (FRCustomsDataRegistry.Instance.DeltaTMode.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty) as FallbackSettings).Start);
		}

		void PrepareFallbackSettingsForDeltaModeTest(ref FallbackSettings defaultFallbackSetting, ref FallbackSettings deltaGFallbackSetting, ref FallbackSettings deltaResetFallbackSetting)
		{
			defaultFallbackSetting = new FallbackSettings();
			defaultFallbackSetting.Start = new ZDateTime(2020, 03, 24);
			defaultFallbackSetting.InvocationReason = "Invocation default";
			defaultFallbackSetting.RegularisationPeriod = 10;

			deltaGFallbackSetting = new FallbackSettings();
			deltaGFallbackSetting.Start = new ZDateTime(2020, 04, 24);
			deltaGFallbackSetting.InvocationReason = "Invocation delta G";
			deltaGFallbackSetting.RegularisationPeriod = 60;

			deltaResetFallbackSetting = new FallbackSettings();
			deltaResetFallbackSetting.Start = new ZDateTime(2020, 03, 24);
			deltaResetFallbackSetting.InvocationReason = "Invocation reset";
			deltaResetFallbackSetting.RegularisationPeriod = 60;
		}

		public void AssertFallbackActive(Action<FallbackSettings> setFallback, Func<bool> isFallbackActive)
		{
			var fallbackSetting = new FallbackSettings();

			AssertEquals("Fallback should be inactive by default.", false, isFallbackActive());

			fallbackSetting.End = ZDateTime.Today.AddDays(-1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			setFallback(fallbackSetting);
			AssertEquals("Fallback should be inactive when both start and end dates are in the past.", false, isFallbackActive());

			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(1);
			setFallback(fallbackSetting);
			AssertEquals("Fallback should be inactive when both start and end dates are in the future.", false, isFallbackActive());

			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			setFallback(fallbackSetting);
			AssertEquals("Fallback should be active when start is in the past and end is in the future.", true, isFallbackActive());

			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			setFallback(fallbackSetting);
			AssertEquals("Fallback should be active when end is empty and start is in the past.", true, isFallbackActive());

			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(1);
			setFallback(fallbackSetting);
			AssertEquals("Fallback should be inactive when end is empty and start is in the future.", false, isFallbackActive());

			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Empty;
			setFallback(fallbackSetting);
			AssertEquals("Fallback should be inactive when start is empty and end is in the future.", false, isFallbackActive());
		}

		public void TestDeltaGFallbackIsActive()
		{
			AssertFallbackActive(fs => FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fs), () => FRCustomsDataRegistry.DeltaGFallbackIsActive);
		}

		public void TestDeltaIFallbackIsActive()
		{
			AssertFallbackActive(fs => FRCustomsDataRegistry.Instance.DeltaIMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fs), () => FRCustomsDataRegistry.DeltaIFallbackIsActive);
		}

		public void TestDeltaTFallbackIsActive()
		{
			AssertFallbackActive(fs => FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fs), () => FRCustomsDataRegistry.DeltaTFallbackIsActive);
		}

		public static void SetDeltaGFallbackIsActive(bool isActive)
		{
			var fallbackSetting = new FallbackSettings();

			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(isActive ? -1 : 1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);
		}
		#endregion

		#region Notifications

		public void TestFallbackRegularisationReportGroup()
		{
			TestGenericRegistryItem(
				ItemSet.FallbackRegularisationReportNotificationGroup,
				"FallbackRegularisationReportNotificationGroup",
				FRCustomsDataRegistry.Categories.Customs_France_Notifications,
				"Fallback Regularization Report Notification group",
				"Fallback regularization report notification settings.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory,
				Guid.Empty
			);
		}

		public void TestBondedWarehouseNotificationGroup()
		{
			TestGenericRegistryItem(
				ItemSet.BondedWarehouseNotificationGroup,
				"BondedWarehouseNotificationGroup",
				FRCustomsDataRegistry.Categories.Customs_France_Notifications,
				"Bonded Warehouse Notification group",
				"Bonded Warehouse movements notification settings.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory,
				Guid.Empty
			);
		}

		public void TestDCGResponseNotificationGroup()
		{
			TestGroupNotificationRegistryItem(ItemSet.DCGResponseNotificationGroup,
				"DCGResponseNotificationGroup",
				FRCustomsDataRegistry.Categories.Customs_France_Notifications,
				"Liquidation (DCG) Responses",
				"Liquidation (DCG) response notification settings.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory,
				ManifestGroupNotification.Default);
		}

		public void TestNctsResponseNotificationGroup()
		{
			TestGroupNotificationRegistryItem(ItemSet.DeltaTResponseNotificationGroup,
				"DeltaTResponseNotificationGroup",
				FRCustomsDataRegistry.Categories.Customs_France_Notifications,
				"Transit (NCTS) Response",
				"Transit (NCTS) Response notification settings.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory,
				ManifestGroupNotification.Default);
		}

		public void TestDeltaGResponseNotificationGroup()
		{
			TestGroupNotificationRegistryItem(ItemSet.DeltaGResponseNotificationGroup,
				"DeltaGResponseNotificationGroup",
				FRCustomsDataRegistry.Categories.Customs_France_Notifications,
				"Delta G Response",
				"Delta G Response notification settings.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory,
				ManifestGroupNotification.Default);
		}

		void TestGroupNotificationRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
			RegistryStorageFlags expectedStorage, RegistryOptions options, GroupNotification expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
			AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
			AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
		}

		#endregion

		#region FR VAT Report

		public void TestFRVATReportLastRunTime()
		{
			TestGenericRegistryItem(
				ItemSet.FRVATReportLastRunTime,
				"FRVATReportLastRunTime",
				FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
				"FR VAT report last run time",
				"Indicate the last time the FR VAT Report has been running.",
				RegistryStorageFlags.Company
			);
		}

		public void TestFRVATReportStaggeringFactor()
		{
			TestRegistryItem(
				ItemSet.FRVATReportStaggeringFactor,
				"FRVATReportStaggeringFactor",
				FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
				"FR VAT report staggering factor",
				"Staggering factor for France VAT report.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				1,
				0,
				int.MaxValue
			);
		}

		public void TestFRVATReportDeadlineDayOfTheMonth()
		{
			TestRegistryItem(
				ItemSet.FRVATReportDeadlineDayOfTheMonth,
				"FRVATReportDeadlineDayOfTheMonth",
				FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
				"FR VAT report deadline day of the month",
				"Deadline day of the month to perform the VAT Report for France.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				23, 3, 31
			);
		}

		public void TestRunFRVATReportInUAT()
		{
			TestGenericRegistryItem(
				ItemSet.RunFRVATReportInUAT,
				"RunFRVATReportInUAT",
				FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
				"Should the service task FRV be allowed to run in UAT?",
				"Should the service task FRV be allowed to run in UAT?",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				false
			);
		}

		public void TestFRVATReportConfiguration()
		{
			TestRegistryItem(ItemSet.FRVATReportConfiguration,
				"FRVATReportConfiguration",
				FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
				"FR VAT Report configuration name",
				"Configuration that applies when executing the automatic FR VAT report.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsValueMandatory,
				VATReportConfigurationLookups.ConfigurationLookups,
				null
			);
		}

		#endregion

		#region Delta I/E

		public void TestEnableDeltaIEForImports()
		{
			TestRegistryItem(ItemSet.EnableDeltaIEForImports,
				"EnableDeltaIEForImports",
				FRCustomsDataRegistry.Categories.Customs_France,
				"Enable Delta I/E for Imports",
				"Enable Delta I/E for Imports?",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestEnableDeltaIEForExports()
		{
			TestRegistryItem(ItemSet.EnableDeltaIEForExports,
				"EnableDeltaIEForExports",
				FRCustomsDataRegistry.Categories.Customs_France,
				"Enable Delta I/E for Exports",
				"Enable Delta I/E for Exports?",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				false);
		}

		#endregion

		#region Auto Send Messages

		public void TestTriggerPointsConfiguration()
		{
			TestGenericRegistryItem(
				ItemSet.TriggerPointsConfiguration,
				"FRTriggerPointsConfiguration",
				FRCustomsDataRegistry.Categories.Customs_France_AutoSendMessages,
				"Trigger points for automated validation",
				"Set here the events expected to trigger the auto validation of messages to Customs",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch
			);

			AssertEquals("Registry TriggerPointsConfiguration  Export value should default to NUL.", TriggerPointsCodeList.Codes.NUL, FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.DefaultValue.ExportTriggerPoint);
			AssertEquals("Registry TriggerPointsConfiguration  Import value should default to NUL.", TriggerPointsCodeList.Codes.NUL, FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.DefaultValue.ImportTriggerPoint);
		}

		public void TestAutomatedModification()
		{
			TestGenericRegistryItem(
				ItemSet.FRAutomatedModification,
				"AutomatedModification",
				FRCustomsDataRegistry.Categories.Customs_France_AutoSendMessages,
				"Date for Duty automated modification.",
				"Choose here to enable automated modification of the time at which to advance the date for duty of declaration 24h later.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch
			);

			AssertEquals("Registry AutomatedModification DateByDefault value should default to empty.", ZDateTime.Empty, FRCustomsDataRegistry.Instance.FRAutomatedModification.DefaultValue.TimeByDefault);

			AssertEquals("Registry AutomatedModification EnableAutomatedModification value should default to false.", false, FRCustomsDataRegistry.Instance.FRAutomatedModification.DefaultValue.EnableAutomatedModification);
		}

		#endregion

		#region Exit Control System

		public void TestDocumentTypeAsAlternateProofOfExit()
		{
			TestGenericRegistryItem(
				ItemSet.DocumentTypeAsAlternateProofOfExit,
				"FRDocumentTypeAsAlternateProofOfExit",
				FRCustomsDataRegistry.Categories.Customs_France_ExitControlSystem,
				"Document Type as Alternate Proof of Exit",
				"Select a document type as Alternate Proof of Exit.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				string.Empty
			);
		}

		#endregion

		#region SAD

		public void TestSADGenerationOnConfirmedExitEnabled()
		{
			TestGenericRegistryItem(ItemSet.SADGenerationOnConfirmedExitEnabled,
				"SADGenerationOnConfirmedExitEnabled",
				FRCustomsDataRegistry.Categories.Customs_France,
				"SAD Generation on Confirmed Exit Enabled",
				"Set to YES to enable SAD Generation on Confirmed Exit.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		#endregion SAD
	}
}
