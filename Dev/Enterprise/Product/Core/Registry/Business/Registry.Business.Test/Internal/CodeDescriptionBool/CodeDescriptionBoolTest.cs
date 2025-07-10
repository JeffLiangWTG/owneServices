using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBool))]
	public class CodeDescriptionBoolTest : RegistryBusinessObjectTest
	{
		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 256, BizObj.MaxDescriptionLengthInternal);
		}

		public virtual void TestCanDelete()
		{
			CodeDescriptionBool result = (CodeDescriptionBool)GetNewBusinessObject();
			ICanDelete canDelete = result;
			AssertEquals("ReasonForNotAbleToDelete", "This is a system defined value and cannot be deleted.", canDelete.ReasonForNotAbleToDelete);
			result.SystemDefined = false;
			AssertEquals("CanDelete", true, canDelete.CanDelete);
			result.SystemDefined = true;
			AssertEquals("CanDelete", false, canDelete.CanDelete);
		}

		public virtual void TestReadOnlyStates()
		{
			CodeDescriptionBool result = (CodeDescriptionBool)GetNewBusinessObject();

			result.SystemDefined = false;
			AssertEquals("CodeInfo.ReadOnly", false, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, result.BoolInfo.ReadOnly);

			result.SystemDefined = true;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, result.BoolInfo.ReadOnly);

			result.ReadOnly = true;
			result.ReadOnly = false;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, result.BoolInfo.ReadOnly);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			CodeDescriptionBool cloneObject = clone as CodeDescriptionBool;
			AssertEquals("Code", "TSCOD", cloneObject.Code);
			AssertEquals("Description", "DescriptionA", cloneObject.Description);
			AssertEquals("CodeMaxLength", 5, cloneObject.CodeMaxLength);
			Assert("SystemDefined", cloneObject.SystemDefined);
			AssertNotNull("CodeList", cloneObject.CodeList);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CodeDescriptionBool result = (CodeDescriptionBool)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
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

		new CodeDescriptionBool BizObj
		{
			get { return (CodeDescriptionBool)base.BizObj; }
		}
	}
}
