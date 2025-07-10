using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.DialogDefault;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Test
{
	sealed class StmDialogDefaultTests : TestCaseWithFactory
	{
		ISecurityCheckpoint CanCreateGlobalDialogDefaults
		{
			get { return EnvProxy.Instance.Security.FindCheckPoint("CanCreateAndModifyGlobalDialogDefaults"); }
		}

		public void TestUserWithoutPermissionCantChangeTheirLevel_NoPermission()
		{
			CanCreateGlobalDialogDefaults.IsAllowed = false;

			var parent = Factory.NewWithValidTestData<StmDialogDefault>();

			Assert("A user without permission should not be able to change their defaults to company", !parent.Lookups.Level.ContainsCode(DialogDefaultLevel.Codes.Company));
			Assert("A user without permission should not be able to change their defaults to global", !parent.Lookups.Level.ContainsCode(DialogDefaultLevel.Codes.Global));
			Assert("A user should be able to modify his own defaults", parent.Lookups.Level.ContainsCode(DialogDefaultLevel.Codes.User));
		}

		public void TestUserWithoutPermissionCantChangeTheirLevel_Permission()
		{
			CanCreateGlobalDialogDefaults.IsAllowed = true;

			var parent = Factory.NewWithValidTestData<StmDialogDefault>();

			var lookupLevels = parent.Lookups.Level;
			var allLevels = new DialogDefaultLevel();

			AssertEquals("Should have all the same elements", allLevels.Count, lookupLevels.Count);
			AssertContainsExactElementsInAnyOrder("Should have all the same elements", allLevels, lookupLevels);
		}

		public void TestOwnerCatersToAllLevelTypes()
		{
			foreach (CodeDescriptionPair pair in new DialogDefaultLevel())
			{
				var defaults = Factory.NewWithValidTestData<StmDialogDefault>();
				defaults.SDD_Level = pair.Code;

				ICollection owners = null;
				AssertNoExceptionThrown(() => owners = new StmDialogDefaultLookups(defaults).Owners);
				if (pair.Code != DialogDefaultLevel.Codes.Global)
				{
					AssertNotNull(owners);
				}
			}
		}
	}
}

namespace Enterprise.Core.DialogDefault.Testing
{
	[TestedType(typeof(StmDialogDefault))]
	sealed class StmDialogDefaultTests : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableOwnerNameWhenOwnerDoesNotExist()
		{
			var bizo = Factory.NewWithValidTestData<StmDialogDefault>();
			bizo.SDD_Level = DialogDefaultLevel.Codes.User;
			bizo.SDD_Owner = ZGuid.NewZGuid();
			bizo.SDD_Caption = "Some stuff";

			AssertEquals("<Unknown owner>: Some stuff", bizo.HumanReadableName);
		}

		public void TestHumanReadableName_USR()
		{
			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			user.GS_Code = "RGR";

			var bizo = Factory.NewWithValidTestData<StmDialogDefault>();
			bizo.SDD_Caption = "Some stuff";
			bizo.SDD_Level = DialogDefaultLevel.Codes.User;
			bizo.SDD_Owner = user.PK;

			AssertEquals("Staff (RGR): Some stuff", bizo.HumanReadableName);
		}

		public void TestHumanReadableName_CMP()
		{
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbCompany>());

			var bizo = Factory.NewWithValidTestData<StmDialogDefault>();
			bizo.SDD_Caption = "Some stuff";
			bizo.SDD_Level = DialogDefaultLevel.Codes.Company;
			bizo.SDD_Owner = company.PK;

			AssertEquals("Company: Some stuff", bizo.HumanReadableName);
		}

		public void TestHumanReadableNameWithUnknownLevel()
		{
			var bizo = Factory.NewWithValidTestData<StmDialogDefault>();
			bizo.SDD_Caption = "Some stuff";
			bizo.SDD_Level = "UNK";

			AssertEquals("<Unknown owner>: Some stuff", bizo.HumanReadableName);

			Assert(ErrorReporter.TotalErrorCount == 1 && ErrorReporter.LastMessageReported.StartsWith("Unknown SDD_Level value", StringComparison.Ordinal));
			ErrorReporter.Clear();
		}

		public void TestHumanReadableName_GLB()
		{
			var bizo = Factory.NewWithValidTestData<StmDialogDefault>();
			bizo.SDD_Caption = "Some stuff";
			bizo.SDD_Level = DialogDefaultLevel.Codes.Global;

			AssertEquals("Global: Some stuff", bizo.HumanReadableName);
		}

		public void TestUserCantApplyHisDefaultsForOthers()
		{
			var canCreateGlobalDefaults = EnvProxy.Instance.Security.FindCheckPoint("CanCreateAndModifyGlobalDialogDefaults");
			canCreateGlobalDefaults.IsAllowed = false;

			var defs = Factory.NewWithValidTestData<StmDialogDefault>();
			defs.SDD_Level = DialogDefaultLevel.Codes.User;

			Assert("User should not be able to apply his defaults to others without the security checkpoint", defs.SDD_Owner_ReadOnly);
		}

		public void TestModifyXmlSecurityCheckpoint()
		{
			var canModifyXml = EnvProxy.Instance.Security.FindCheckPoint("CanModifyDefaultsXML");

			var defs = Factory.New<StmDialogDefault>();

			canModifyXml.IsAllowed = false;
			Assert(defs.SDD_SerializedDefaults_ReadOnly);

			canModifyXml.IsAllowed = true;
			Assert(!defs.SDD_SerializedDefaults_ReadOnly);
		}

		public void TestChangingLevelsRemembersPreviousLevelsValues()
		{
			var defaults = Factory.NewWithValidTestData<StmDialogDefault>();

			//Group some guids with some codes
			var codesWithGuids = new Dictionary<ZGuid, string>();
			foreach (CodeDescriptionPair pair in (new DialogDefaultLevel()))
			{
				var guid = ZGuid.NewZGuid();

				codesWithGuids[guid] = pair.Code;
				defaults.SDD_Level = pair.Code;
				defaults.SDD_Owner = guid;
			}

			foreach (var pair in codesWithGuids)
			{
				defaults.SDD_Level = pair.Value;

				AssertEquals("The owner should be at the value it was last", pair.Value, codesWithGuids[defaults.SDD_Owner]);
			}
		}

		public void TestHasContext()
		{
			const string reasonWhyContextIsAlwaysReadOnly = @"
A context is always read only.

Under some cases there may be a default thats xml depends on some component of it's context. 
For that reason you cannot allow arbitrary defaults to have a mutable context.
";

			var defaults = Factory.NewWithValidTestData<StmDialogDefault>();

			defaults.SDD_Context = ZBlob.Empty;
			Assert("An empty context blob indicates there is no context", defaults.AppliesToSimilarDialogs);
			Assert(reasonWhyContextIsAlwaysReadOnly, defaults.AppliesToSimilarDialogsInfo.ReadOnly);

			defaults.SDD_Context = new ZBlob(new byte[] { 1, 2, 3, 4 });
			Assert("Non empty blob indicates a context exists", !defaults.AppliesToSimilarDialogs);
			Assert(reasonWhyContextIsAlwaysReadOnly, defaults.AppliesToSimilarDialogsInfo.ReadOnly);
		}

		public void TestModifyingOwnerWorks()
		{
			var defaults = Factory.NewWithValidTestData<StmDialogDefault>();

			defaults.SDD_Level = DialogDefaultLevel.Codes.User;
			defaults.SDD_Owner = ZGuid.NewZGuid();

			var newOwner = ZGuid.NewZGuid();
			defaults.SDD_Owner = newOwner;

			AssertEquals("Should be able to change guids", newOwner, defaults.SDD_Owner);

			defaults.SDD_Level = DialogDefaultLevel.Codes.Company;
			defaults.SDD_Level = DialogDefaultLevel.Codes.User;

			AssertEquals("Should have the newest guid set", newOwner, defaults.SDD_Owner);
		}

		public void TestGettingOwnerWithNewLevelGetsEmptyGuid()
		{
			var defaults = Factory.NewWithValidTestData<StmDialogDefault>();

			defaults.SDD_Level = DialogDefaultLevel.Codes.User;
			defaults.SDD_Owner = ZGuid.NewZGuid();

			defaults.SDD_Level = DialogDefaultLevel.Codes.Company;
			AssertEquals("Should be blank as a company hasn't been set", ZGuid.Empty, defaults.SDD_Owner);
		}

		public void TestSort()
		{
			//Specific user, specific context
			var mostSpecific = Factory.New<StmDialogDefault>();
			mostSpecific.SDD_Level = DialogDefaultLevel.Codes.User;
			mostSpecific.SDD_Context = new ZBlob(new byte[] { 1, 2, 3 });

			//Specific user, general context
			var prettySpecific = Factory.New<StmDialogDefault>();
			prettySpecific.SDD_Level = DialogDefaultLevel.Codes.User;
			prettySpecific.SDD_Context = ZBlob.Empty;

			//General user, specific context
			var notSpecific = Factory.New<StmDialogDefault>();
			notSpecific.SDD_Level = DialogDefaultLevel.Codes.Company;
			notSpecific.SDD_Context = mostSpecific.SDD_Context;

			//Higher == 'Less specific'
			Assert("Null context should be higher then non null", mostSpecific.CompareTo(prettySpecific) < 0);
			Assert("User owner should be lower than company", prettySpecific.CompareTo(notSpecific) < 0);
			Assert("Company should rank higher than user", notSpecific.CompareTo(mostSpecific) > 0);

			var desiredOrder = new[] { mostSpecific, prettySpecific, notSpecific };
			var actualOrder = (new[] { prettySpecific, notSpecific, mostSpecific }).OrderBy(o => o).ToArray();
			Assert("Sort did not work", actualOrder.Zip(desiredOrder, (def1, def2) => def1.Equals(def2)).All(b => b));
		}

		public void TestLongCaptionName()
		{
			var caption = new string('c', 2 * StmDialogDefault.Schema.SDD_CaptionMaxLength);
			var dialog = Factory.New<StmDialogDefault>();

			dialog.SDD_Caption = caption;

			AssertEquals("Should trunc the caption", StmDialogDefault.Schema.SDD_CaptionMaxLength, dialog.SDD_Caption.Length);
		}
	}
}
