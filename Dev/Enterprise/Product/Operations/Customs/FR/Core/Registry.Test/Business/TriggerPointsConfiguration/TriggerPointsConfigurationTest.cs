using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(TriggerPointsConfiguration))]
	public class TriggerPointsConfigurationTest : RegistryBusinessObjectTemplateTestCase<TriggerPointsConfiguration>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override TriggerPointsConfiguration GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override TriggerPointsConfiguration GetBusinessObjectToSerialise()
		{
			triggerPointsConfiguration = new TriggerPointsConfiguration();
			triggerPointsConfiguration.EnableAutomatedValidation = true;
			triggerPointsConfiguration.ImportTriggerPoint = TriggerPointsCodeList.Codes.PAB;
			triggerPointsConfiguration.ExportTriggerPoint = TriggerPointsCodeList.Codes.REC;
			return triggerPointsConfiguration;
		}

		public void TestSetEnableAutomatedValidation()
		{
			triggerPointsConfiguration = new TriggerPointsConfiguration();
			triggerPointsConfiguration.ImportTriggerPoint = TriggerPointsCodeList.Codes.PAB;
			triggerPointsConfiguration.ExportTriggerPoint = TriggerPointsCodeList.Codes.REC;
			triggerPointsConfiguration.EnableAutomatedValidation = true;
			CombineAssertions("Defaulting of trigger points when enabling automated validation", () =>
			{
				AssertEquals("Enabling automated validation should not default Import Trigger Point.", TriggerPointsCodeList.Codes.PAB, triggerPointsConfiguration.ImportTriggerPoint);
				AssertEquals("Enabling automated validation should not default Export Trigger Point.", TriggerPointsCodeList.Codes.REC, triggerPointsConfiguration.ExportTriggerPoint);
			});

			triggerPointsConfiguration.EnableAutomatedValidation = false;
			CombineAssertions("Defaulting of trigger points when disabling automated validation", () =>
			{
				AssertEquals("Disabling Automated Validation should default Import Trigger Point to NUL.", TriggerPointsCodeList.Codes.NUL, triggerPointsConfiguration.ImportTriggerPoint);
				AssertEquals("Disabling Automated Validation should default Export Trigger Point to NUL.", TriggerPointsCodeList.Codes.NUL, triggerPointsConfiguration.ExportTriggerPoint);
			});
		}

		public void TestReadOnly()
		{
			triggerPointsConfiguration = new TriggerPointsConfiguration();
			CombineAssertions("ReadOnly values when automated validation enabled", () =>
			{
				Assert(triggerPointsConfiguration.ImportTriggerPointInfo.ReadOnly);
				Assert(triggerPointsConfiguration.ExportTriggerPointInfo.ReadOnly);
			});

			triggerPointsConfiguration.EnableAutomatedValidation = true;
			CombineAssertions("ReadOnly values when automated validation disabled", () =>
			{
				Assert(!triggerPointsConfiguration.ImportTriggerPointInfo.ReadOnly);
				Assert(!triggerPointsConfiguration.ExportTriggerPointInfo.ReadOnly);
			});
		}

		#region Lists

		public void TestImportTriggerPointsCodeList()
		{
			triggerPointsConfiguration = new TriggerPointsConfiguration();
			AssertContainsExactElementsInAnyOrder(new string[] { TriggerPointsCodeList.Codes.PAB, TriggerPointsCodeList.Codes.VAQ, TriggerPointsCodeList.Codes.NUL }, triggerPointsConfiguration.ImportTriggerPointsCodeList.GetAllCodes());
		}

		public void TestExportTriggerPointsCodeList()
		{
			triggerPointsConfiguration = new TriggerPointsConfiguration();
			AssertContainsExactElementsInAnyOrder(new string[] { TriggerPointsCodeList.Codes.REC, TriggerPointsCodeList.Codes.NUL }, triggerPointsConfiguration.ExportTriggerPointsCodeList.GetAllCodes());
		}

		#endregion

		#region Validation

		public void TestImportTriggerPointValidation()
		{
			var triggerPointsConfiguration = new TriggerPointsConfiguration();
			AssertEquals("Empty value should be allowed.", 0, triggerPointsConfiguration.ImportTriggerPointInfo.Notifications.Count());

			triggerPointsConfiguration.ImportTriggerPoint = "AAA";
			AssertHasErrorContaining("Code not in list should return an error.", triggerPointsConfiguration.ImportTriggerPointInfo, ListValidation.InvalidCodeError);

			triggerPointsConfiguration.ImportTriggerPoint = TriggerPointsCodeList.Codes.PAB;
			AssertNoErrorContaining("Code in list should not return an error.", triggerPointsConfiguration.ImportTriggerPointInfo, ListValidation.InvalidCodeError);
		}

		public void TestExportTriggerPointValidation()
		{
			var triggerPointsConfiguration = new TriggerPointsConfiguration();
			AssertEquals("Empty value should be allowed.", 0, triggerPointsConfiguration.ExportTriggerPointInfo.Notifications.Count());

			triggerPointsConfiguration.ExportTriggerPoint = "AAA";
			AssertHasErrorContaining("Code not in list should return an error.", triggerPointsConfiguration.ExportTriggerPointInfo, ListValidation.InvalidCodeError);

			triggerPointsConfiguration.ExportTriggerPoint = TriggerPointsCodeList.Codes.REC;
			AssertNoErrorContaining("Code in list should not return an error.", triggerPointsConfiguration.ExportTriggerPointInfo, ListValidation.InvalidCodeError);
		}

		#endregion

		TriggerPointsConfiguration triggerPointsConfiguration;
	}
}
