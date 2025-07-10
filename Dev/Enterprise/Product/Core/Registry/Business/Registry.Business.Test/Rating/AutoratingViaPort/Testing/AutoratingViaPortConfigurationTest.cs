using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutoratingViaPortConfiguration))]
	sealed class AutoratingViaPortConfigurationTest : RegistryBusinessObjectTemplateTestCase<AutoratingViaPortConfiguration>
	{
		public void TestSettings()
		{
			var setting = Configuration.Settings.AddNew();

			AssertEquals("Settings.ParentLocationsChargesGroup", BizObj, BizObj.Settings.ParentConfiguration);
			AssertEquals("Settings.Factory", BizObj.Factory, BizObj.Settings.Factory);
			AssertEquals("Settings.CurrentFallbackLevel", BizObj.CurrentFallbackLevel, setting.CurrentFallbackLevel);
		}

		public void TestJobTypeList()
		{
			(string code, string description)[] values =
{
				("FCN", "Consol"),
				("SHP", "Shipment"),
				("QSH", "Quick Booking"),
			};

			var configuration = (AutoratingViaPortConfiguration)GetNewBusinessObject();

			AssertEquals(values.Length, configuration.JobTypeList.Count);

			for (var i = 0; i < values.Length; ++i)
			{
				AssertEquals(values[i].code, configuration.JobTypeList[i].Code);
				AssertEquals(values[i].description, configuration.JobTypeList[i].Description);
			}
		}

		public void TestTransportModeList()
		{
			(string code, string description)[] values =
{
				("ALL", "All"),
				("AIR", "Air Freight"),
				("SEA", "Sea Freight"),
			};

			var configuration = (AutoratingViaPortConfiguration)GetNewBusinessObject();

			AssertEquals(values.Length, configuration.TransportModeList.Count);

			for (var i = 0; i < values.Length; ++i)
			{
				AssertEquals(values[i].code, configuration.TransportModeList[i].Code);
				AssertEquals(values[i].description, configuration.TransportModeList[i].Description);
			}
		}

		public void TestRunPreSaveValidation()
		{
			string originalValue;
			var configuration = (AutoratingViaPortConfiguration)GetNewBusinessObject();

			// JobType Validation
			originalValue = configuration.JobType;
			configuration.JobType = "XXX";
			configuration.ClearAllNotifications();
			AssertNoErrors("Precondition: JobType should not have errors", configuration);
			configuration.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated JobType", configuration.JobTypeInfo);
			configuration.JobType = originalValue;

			// TransportMode Validation
			originalValue = configuration.TransportMode;
			configuration.TransportMode = "XXX";
			configuration.ClearAllNotifications();
			AssertNoErrors("Precondition: TransportMode should not have errors", configuration);
			configuration.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated TransportMode", configuration.TransportModeInfo);
			configuration.TransportMode = originalValue;
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var index = new Random().Next(AutoratingViaPortConfigurationCollection.Defaults.Count);

			var currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var configuration = new AutoratingViaPortConfiguration(currentFallbackLevel, Factory);
			configuration.JobType = AutoratingViaPortConfigurationCollection.Defaults[index].jobType;
			configuration.TransportMode = AutoratingViaPortConfigurationCollection.Defaults[index].transportMode;

			return configuration;
		}

		protected override AutoratingViaPortConfiguration GetBusinessObjectToClone()
			=> (AutoratingViaPortConfiguration)GetNewBusinessObject();

		protected override AutoratingViaPortConfiguration GetBusinessObjectToSerialise()
			=> (AutoratingViaPortConfiguration)GetNewBusinessObject();

		#endregion

		AutoratingViaPortConfiguration Configuration => BizObj;
	}
}
