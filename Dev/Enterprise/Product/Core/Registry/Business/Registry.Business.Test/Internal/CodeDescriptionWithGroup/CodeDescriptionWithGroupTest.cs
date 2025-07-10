using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithGroup))]
	public class CodeDescriptionWithGroupTest : RegistryBusinessObjectTest
	{
		public void TestCanDelete()
		{
			var result = (CodeDescriptionWithGroup)GetNewBusinessObject();
			ICanDelete canDelete = result;
			AssertEquals("ReasonForNotAbleToDelete", "This is a system defined value and cannot be deleted.", result.ReasonForNotAbleToDelete);
			AssertEquals("ReasonForNotAbleToDelete", "This is a system defined value and cannot be deleted.", canDelete.ReasonForNotAbleToDelete);
			result.SystemDefined = false;
			AssertEquals("CanDelete", true, result.CanDelete);
			AssertEquals("CanDelete", true, canDelete.CanDelete);
			result.SystemDefined = true;
			AssertEquals("CanDelete", false, result.CanDelete);
			AssertEquals("CanDelete", false, canDelete.CanDelete);
		}

		public void TestReadOnlyStates()
		{
			var result = (CodeDescriptionWithGroup)GetNewBusinessObject();

			result.SystemDefined = false;
			AssertEquals("CodeInfo.ReadOnly", false, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("GroupInfo.ReadOnly", false, result.GroupInfo.ReadOnly);

			result.SystemDefined = true;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("GroupInfo.ReadOnly", false, result.GroupInfo.ReadOnly);

			result.ReadOnly = true;
			result.ReadOnly = false;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("GroupInfo.ReadOnly", false, result.GroupInfo.ReadOnly);
		}

		public void TestGroupLookup()
		{
			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("ABC", "Group 1");
			groupLookup.AddPair("CDE", "Group 2");

			var collection = new CodeDescriptionWithGroupCollection(groupLookup, "ABC");
			var element = (CodeDescriptionWithGroup)GetNewBusinessObject();
			AssertNotNull("GroupLookup", element.GroupLookup);
			AssertEquals(0, element.GroupLookup.Count);

			collection.Add(element);
			AssertNotNull("GroupLookup", element.GroupLookup);
			AssertEquals(2, element.GroupLookup.Count);
			AssertEquals("The same List we passed to Collection", groupLookup, element.GroupLookup);
		}

		public void TestValidateGroup()
		{
			AssertValidationAction((x) => x.ValidateGroup());
		}

		public void TestRunPreSaveValidation()
		{
			AssertValidationAction((x) => x.RunPreSaveValidation());
		}

		void AssertValidationAction(Action<CodeDescriptionWithGroup> validationAction)
		{
			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("ABC", "Group 1");
			groupLookup.AddPair("CDE", "Group 2");

			var collection = new CodeDescriptionWithGroupCollection(groupLookup, "ABC");
			var element = (CodeDescriptionWithGroup)GetNewBusinessObject();
			element.Group = "ABC";
			AssertEquals(0, element.GroupLookup.Count);

			validationAction(element);
			AssertHasErrors(element.GroupInfo);

			collection.Add(element);
			AssertNotNull("GroupLookup", element.GroupLookup);
			AssertEquals(2, element.GroupLookup.Count);

			validationAction(element);
			AssertNoErrors(element.GroupInfo);

			element.Group = "QQQ";
			validationAction(element);
			AssertHasErrors(element.GroupInfo);

			element.Group = "CDE";
			validationAction(element);
			AssertNoErrors(element.GroupInfo);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 256, BizObj.MaxDescriptionLengthInternal);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var cloneObject = clone as CodeDescriptionWithGroup;
			AssertEquals("Code", "TSCOD", cloneObject.Code);
			AssertEquals("Description", "DescriptionA", cloneObject.Description);
			AssertEquals("Group", "GRP", cloneObject.Group);
			AssertEquals("CodeMaxLength", 5, cloneObject.CodeMaxLength);
			Assert("SystemDefined", cloneObject.SystemDefined);
			AssertNotNull("CodeList", cloneObject.CodeList);
			AssertNotNull("CodeList", cloneObject.GroupLookup);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CodeDescriptionWithGroup)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.Group = "GRP";
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

		new CodeDescriptionWithGroup BizObj
		{
			get { return (CodeDescriptionWithGroup)base.BizObj; }
		}
	}
}
