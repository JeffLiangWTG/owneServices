using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithThreeGroups))]
	sealed class CodeDescriptionWithThreeGroupsTest : RegistryBusinessObjectTest
	{
		public void TestCanDelete()
		{
			var result = (CodeDescriptionWithThreeGroups)GetNewBusinessObject();
			ICanDelete canDelete = result;
			AssertEquals("ReasonForNotAbleToDelete", "This is a system defined value and cannot be deleted.", canDelete.ReasonForNotAbleToDelete);
			result.SystemDefined = false;
			AssertEquals("CanDelete", true, canDelete.CanDelete);
			result.SystemDefined = true;
			AssertEquals("CanDelete", false, canDelete.CanDelete);
		}

		public void TestReadOnlyStates()
		{
			var result = (CodeDescriptionWithThreeGroups)GetNewBusinessObject();

			result.SystemDefined = false;
			AssertEquals("CodeInfo.ReadOnly", false, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("GroupInfo.ReadOnly", false, result.GroupInfo.ReadOnly);
			AssertEquals("Group2Info.ReadOnly", false, result.Group2Info.ReadOnly);
			AssertEquals("Group3Info.ReadOnly", false, result.Group3Info.ReadOnly);

			result.SystemDefined = true;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("GroupInfo.ReadOnly", false, result.GroupInfo.ReadOnly);
			AssertEquals("Group2Info.ReadOnly", false, result.Group2Info.ReadOnly);
			AssertEquals("Group3Info.ReadOnly", false, result.Group3Info.ReadOnly);

			result.ReadOnly = true;
			result.ReadOnly = false;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("GroupInfo.ReadOnly", false, result.GroupInfo.ReadOnly);
			AssertEquals("Group2Info.ReadOnly", false, result.Group2Info.ReadOnly);
			AssertEquals("Group3Info.ReadOnly", false, result.Group3Info.ReadOnly);
		}

		public void TestGroupLookup()
		{
			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("ABC", "Group 1");
			groupLookup.AddPair("CDE", "Group 2");
			groupLookup.DefaultCode = "ABC";
			var group2Lookup = new CodeDescriptionPairList();
			group2Lookup.AddPair("AB2", "Group 12");
			group2Lookup.AddPair("CD2", "Group 22");
			group2Lookup.AddPair("EF2", "Group 23");
			group2Lookup.AddPair("GH2", "Group 24");
			group2Lookup.DefaultCode = "AB2";
			var group3Lookup = new CodeDescriptionPairList();
			group3Lookup.AddPair("AB3", "Group 13");
			group3Lookup.AddPair("CD3", "Group 23");
			group3Lookup.AddPair("EF3", "Group 33");
			group3Lookup.DefaultCode = "AB3";

			var collection = new CodeDescriptionWithThreeGroupsCollection(groupLookup, group2Lookup, group3Lookup, 17);
			var element = (CodeDescriptionWithThreeGroups)GetNewBusinessObject();
			AssertNotNull("GroupLookup", element.GroupLookup);
			AssertNotNull("Group2Lookup", element.Group2Lookup);
			AssertNotNull("Group3Lookup", element.Group3Lookup);
			AssertEquals(0, element.GroupLookup.Count);
			AssertEquals(0, element.Group2Lookup.Count);
			AssertEquals(0, element.Group3Lookup.Count);

			collection.Add(element);
			AssertNotNull("GroupLookup", element.GroupLookup);
			AssertNotNull("Group2Lookup", element.Group2Lookup);
			AssertNotNull("Group3Lookup", element.Group3Lookup);
			AssertEquals(2, element.GroupLookup.Count);
			AssertEquals(4, element.Group2Lookup.Count);
			AssertEquals(3, element.Group3Lookup.Count);
			AssertEquals("The same List we passed to Collection", groupLookup, element.GroupLookup);
			AssertEquals("The same List2 we passed to Collection", group2Lookup, element.Group2Lookup);
			AssertEquals("The same List3 we passed to Collection", group3Lookup, element.Group3Lookup);
		}

		public void TestValidateGroup()
		{
			AssertValidationAction((x) => x.ValidateGroup());
		}

		public void TestRunPreSaveValidation()
		{
			AssertValidationAction((x) => x.RunPreSaveValidation());
		}

		void AssertValidationAction(Action<CodeDescriptionWithThreeGroups> validationAction)
		{
			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("ABC", "Group 1");
			groupLookup.AddPair("NA", "Group 2");
			groupLookup.DefaultCode = "NA";
			var group2Lookup = new CodeDescriptionPairList();
			group2Lookup.AddPair("AB2", "Group 12");
			group2Lookup.AddPair("CD2", "Group 22");
			group2Lookup.AddPair("EF2", "Group 23");
			group2Lookup.AddPair("NA", "Group 24");
			group2Lookup.DefaultCode = "NA";
			var group3Lookup = new CodeDescriptionPairList();
			group3Lookup.AddPair("AB3", "Group 13");
			group3Lookup.AddPair("CD3", "Group 23");
			group3Lookup.AddPair("NA", "Group 33");
			group3Lookup.DefaultCode = "NA";

			var collection = new CodeDescriptionWithThreeGroupsCollection(groupLookup, group2Lookup, group3Lookup, 17);
			var element = (CodeDescriptionWithThreeGroups)GetNewBusinessObject();
			element.Group = "NA";
			element.Group2 = "NA";
			element.Group3 = "NA";
			AssertEquals(0, element.GroupLookup.Count);
			AssertEquals(0, element.Group2Lookup.Count);
			AssertEquals(0, element.Group3Lookup.Count);

			validationAction(element);
			AssertHasError("Expected error for Group == 'NA'. Value not found in GroupLookup because GroupLookup is not set.", element.GroupInfo, "Enter a valid selection.");
			AssertHasError("Expected error for Group2 == 'NA'. Value not found in Group2Lookup because Group2Lookup is not set.", element.Group2Info, "Enter a valid selection.");
			AssertHasError("Expected error for Group3 == 'NA'. Value not found in Group3Lookup because Group3Lookup is not set.", element.Group3Info, "Enter a valid selection.");

			collection.Add(element);
			AssertNotNull("GroupLookup", element.GroupLookup);
			AssertEquals(2, element.GroupLookup.Count);
			AssertNotNull("Group2Lookup", element.Group2Lookup);
			AssertEquals(4, element.Group2Lookup.Count);
			AssertNotNull("Group3Lookup", element.Group3Lookup);
			AssertEquals(3, element.Group3Lookup.Count);

			validationAction(element);
			AssertNoErrors(element.GroupInfo);
			AssertNoErrors(element.Group2Info);
			AssertNoErrors(element.Group3Info);

			element.Group = "QQQ";
			element.Group2 = "PPP";
			element.Group3 = "RRR";
			validationAction(element);
			AssertHasError("Expected error for Group == 'QQQ'. Value not available in GroupLookup.", element.GroupInfo, "Enter a valid selection.");
			AssertEquals("", element.MainDescription);
			AssertHasError("Expected error for Group == 'PPP'. Value not available in Group2Lookup.", element.Group2Info, "Enter a valid selection.");
			AssertEquals("", element.MainDescription);
			AssertHasError("Expected error for Group == 'RRR'. Value not available in Group3Lookup.", element.Group3Info, "Enter a valid selection.");
			AssertEquals("", element.ExtraDescription);

			element.Group = "ABC";
			element.Group2 = "CD2";
			element.Group3 = "AB3";
			validationAction(element);
			AssertNoErrors(element.GroupInfo);
			AssertEquals("Group 1", element.MainDescription);
			AssertNoErrors(element.Group2Info);
			AssertEquals("Group 1", element.MainDescription);
			AssertNoErrors(element.Group3Info);
			AssertEquals("Group 13", element.ExtraDescription);

			element.Group = string.Empty;
			element.Group2 = "CD2";
			AssertNoErrors(element.Group2Info);
			AssertEquals("Group 22", element.MainDescription);
			element.Group = "ABC";
			AssertNoErrors(element.GroupInfo);
			AssertEquals("Group 1", element.MainDescription);
			element.Group3 = "AB3";
			validationAction(element);
			AssertNoErrors(element.Group3Info);
			AssertEquals("Group 13", element.ExtraDescription);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 256, BizObj.MaxDescriptionLengthInternal);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			var clone = clone1 as CodeDescriptionWithThreeGroups;
			AssertEquals("Code", "TSCOD", clone.Code);
			AssertEquals("Group", "GRP", clone.Group);
			AssertEquals("Group2", "GR2", clone.Group2);
			AssertEquals("Group3", "GR3", clone.Group3);
			AssertEquals("CodeMaxLength", 5, clone.CodeMaxLength);
			Assert("SystemDefined", clone.SystemDefined);
			AssertNotNull("CodeList", clone.CodeList);
			AssertNotNull("GroupLookup", clone.GroupLookup);
			AssertNotNull("Group2Lookup", clone.Group2Lookup);
			AssertNotNull("Group3Lookup", clone.Group3Lookup);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CodeDescriptionWithThreeGroups)GetNewBusinessObject();

			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("GRP", "MainDescriptionA");
			groupLookup.AddPair("NA", "Group 2");
			var group2Lookup = new CodeDescriptionPairList();
			group2Lookup.AddPair("GR2", "Group 12");
			group2Lookup.AddPair("CD2", "Group 22");
			group2Lookup.AddPair("EF2", "Group 23");
			group2Lookup.AddPair("NA", "Group 24");
			var group3Lookup = new CodeDescriptionPairList();
			group3Lookup.AddPair("GR3", "ExtraDescriptionA");
			group3Lookup.AddPair("CD3", "Group 23");
			group3Lookup.AddPair("NA", "Group 33");

			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Group = "GRP";
			result.Group2 = "GR2";
			result.Group3 = "GR3";
			result.SystemDefined = true;
			result.CodeList = new CodeDescriptionPairList();

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new CodeDescriptionWithThreeGroups BizObj
		{
			get { return (CodeDescriptionWithThreeGroups)base.BizObj; }
		}
	}
}
