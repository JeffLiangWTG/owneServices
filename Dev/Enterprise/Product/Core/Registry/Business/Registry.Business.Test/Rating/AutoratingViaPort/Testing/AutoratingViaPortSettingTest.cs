using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutoratingViaPortSetting))]
	sealed class AutoratingViaPortSettingTest : RegistryBusinessObjectTemplateTestCase<AutoratingViaPortSetting>
	{
		public void TestDirectionList()
		{
			(string code, string description)[] values =
			{
				("ALL", "All Directions"),
				("EXP", "Export"),
				("IMP", "Import"),
			};

			AssertCodeDescriptionPairList("", values, x => x.DirectionList);
		}

		public void TestOriginSourceOptionList_Shipment()
		{
			(string code, string description)[] values =
			{
				("1L", "1st Load"),
				("N1L", "≠ 1st Load"),
			};

			AssertCodeDescriptionPairList("SHP", values, x => x.OriginSourceOptionList);
		}

		public void TestOriginSourceOptionList_ForwardingConsol()
		{
			(string code, string description)[] values =
			{
				("VL", "Voyage Load"),
				("NVL", "≠ Voyage Load"),
			};

			AssertCodeDescriptionPairList("FCN", values, x => x.OriginSourceOptionList);
		}

		public void TestOriginSourceOptionList_QuotedBooking()
		{
			(string code, string description)[] values =
			{
				("VL", "Voyage Load"),
				("NVL", "≠ Voyage Load"),
			};

			AssertCodeDescriptionPairList("QSH", values, x => x.OriginSourceOptionList);
		}

		public void TestDestinationSourceOptionList_Shipment()
		{
			(string code, string description)[] values =
			{
				("LD", "Last Discharge"),
				("NLD", "≠ Last Discharge"),
			};

			AssertCodeDescriptionPairList("SHP", values, x => x.DestinationSourceOptionList);
		}

		public void TestDestinationSourceOptionList_ForwardingConsol()
		{
			(string code, string description)[] values =
			{
				("VD", "Voyage Discharge"),
				("NVD", "≠ Voyage Discharge"),
			};

			AssertCodeDescriptionPairList("FCN", values, x => x.DestinationSourceOptionList);
		}

		public void TestDestinationSourceOptionList_QuotedBooking()
		{
			(string code, string description)[] values =
			{
				("VD", "Voyage Discharge"),
				("NVD", "≠ Voyage Discharge"),
			};

			AssertCodeDescriptionPairList("QSH", values, x => x.DestinationSourceOptionList);
		}

		public void TestViaSourceOptionList_Shipment()
		{
			(string code, string description)[] values =
			{
				("1L", "1st Load"),
				("LD", "Last Discharge"),
				("LRD", "Last Mode & Route Set Discharge"),
			};

			AssertCodeDescriptionPairList("SHP", values, x => x.ViaSourceOptionList);
		}

		public void TestViaSourceOptionList_ForwardingConsol()
		{
			(string code, string description)[] values =
			{
				("VL", "Voyage Load"),
				("VD", "Voyage Discharge"),
				("LRD", "Last Mode & Route Set Discharge"),
			};

			AssertCodeDescriptionPairList("FCN", values, x => x.ViaSourceOptionList);
		}

		public void TestViaSourceOptionList_QuotedBooking()
		{
			(string code, string description)[] values =
			{
				("VL", "Voyage Load"),
				("VD", "Voyage Discharge"),
			};

			AssertCodeDescriptionPairList("QSH", values, x => x.ViaSourceOptionList);
		}

		public void AssertCodeDescriptionPairList(
			string jobType,
			(string code, string description)[] expectedList,
			Func<AutoratingViaPortSetting, CodeDescriptionPairList> actualListGetter)
		{
			var setting = GetNewBusinessObject(jobType);

			var actual = actualListGetter(setting);
			AssertEquals(expectedList.Length, actual.Count);

			for (var i = 0; i < expectedList.Length; ++i)
			{
				AssertEquals(expectedList[i].code, actual[i].Code);
				AssertEquals(expectedList[i].description, actual[i].Description);
			}
		}

		public void TestRunPreSaveValidation()
		{
			string originalValue;
			var setting = (AutoratingViaPortSetting)GetNewBusinessObject();
			setting.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			// Direction Validation
			originalValue = setting.Direction;
			setting.Direction = "XXX";
			setting.ClearAllNotifications();
			AssertNoErrors("Precondition: Direction should not have errors", setting);
			setting.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated Direction", setting.DirectionInfo);
			setting.Direction = originalValue;

			// OriginSourceOption Validation
			originalValue = setting.OriginSourceOption;
			setting.OriginSourceOption = "XXX";
			setting.ClearAllNotifications();
			AssertNoErrors("Precondition: OriginSourceOption should not have errors", setting);
			setting.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated OriginSourceOption", setting.OriginSourceOptionInfo);
			setting.OriginSourceOption = originalValue;

			// DestinationSourceOption Validation
			originalValue = setting.DestinationSourceOption;
			setting.DestinationSourceOption = "XXX";
			setting.ClearAllNotifications();
			AssertNoErrors("Precondition: DestinationSourceOption should not have errors", setting);
			setting.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated DestinationSourceOption", setting.DestinationSourceOptionInfo);
			setting.DestinationSourceOption = originalValue;

			// ViaSourceOption Validation
			originalValue = setting.ViaSourceOption;
			setting.ViaSourceOption = "XXX";
			setting.ClearAllNotifications();
			AssertNoErrors("Precondition: ViaSourceOption should not have errors", setting);
			setting.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated ViaSourceOption", setting.ViaSourceOptionInfo);
			setting.ViaSourceOption = originalValue;

			// CompositeKey Validation
			AssertNoRowErrors("Precondition: Setting should not have row errors", setting);

			var parentCollection = setting.ParentConfiguration.Settings;
			AssertEquals(1, parentCollection.Count);

			var newSetting = parentCollection.AddNew();
			newSetting.Direction = setting.Direction;
			newSetting.OriginSourceOption = setting.OriginSourceOption;
			newSetting.DestinationSourceOption = setting.DestinationSourceOption;
			newSetting.ViaSourceOption = setting.ViaSourceOption;

			setting.RunPreSaveValidation();
			AssertHasRowError("Precondition: Setting should have row error", newSetting, AutoratingViaPortSetting.IdenticalSettingExists);

			newSetting.OriginSourceOption = "";
			setting.RunPreSaveValidation();
			AssertNoRowErrors("Precondition: Setting should not have row errors", setting);

			parentCollection.RemoveAndDelete(newSetting);
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
			=> GetNewBusinessObject("");

		AutoratingViaPortSetting GetNewBusinessObject(string jobType)
		{
			var items = AutoratingViaPortConfigurationCollection.Defaults
				.Where(x => string.IsNullOrEmpty(jobType) || x.jobType == jobType)
				.ToList();

			var index = new Random().Next(items.Count);

			var currentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var parentConfiguration = new AutoratingViaPortConfiguration(currentFallbackLevel, Factory);
			parentConfiguration.JobType = items[index].jobType;
			parentConfiguration.TransportMode = items[index].transportMode;

			var setting = parentConfiguration.Settings.AddNew();
			setting.Direction = items[index].direction;
			setting.OriginSourceOption = items[index].origin;
			setting.DestinationSourceOption = items[index].destination;
			setting.ViaSourceOption = items[index].via;

			return setting;
		}

		protected override AutoratingViaPortSetting GetBusinessObjectToClone()
			=> (AutoratingViaPortSetting)GetNewBusinessObject();

		protected override AutoratingViaPortSetting GetBusinessObjectToSerialise()
			=> (AutoratingViaPortSetting)GetNewBusinessObject();

		#endregion
	}
}
