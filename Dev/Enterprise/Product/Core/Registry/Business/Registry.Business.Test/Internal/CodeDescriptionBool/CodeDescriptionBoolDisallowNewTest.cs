using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNew))]
	class CodeDescriptionBoolDisallowNewTest : CodeDescriptionBoolTest
	{
		public override void TestReadOnlyStates()
		{
			var result = (CodeDescriptionBoolDisallowNew)GetNewBusinessObject();

			result.SystemDefined = false;
			AssertEquals("CodeInfo.ReadOnly", false, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", false, result.BoolInfo.ReadOnly);

			result.SystemDefined = true;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", true, result.BoolInfo.ReadOnly);

			result.ReadOnly = true;
			result.ReadOnly = false;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("BoolInfo.ReadOnly", true, result.BoolInfo.ReadOnly);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			CodeDescriptionBoolDisallowNew clone = clone1 as CodeDescriptionBoolDisallowNew;
			AssertEquals("Code", "TSCOD", clone.Code);
			AssertEquals("Description", "DescriptionA", clone.Description);
			AssertEquals("CodeMaxLength", 5, clone.CodeMaxLength);
			Assert("SystemDefined", clone.SystemDefined);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CodeDescriptionBoolDisallowNew result = (CodeDescriptionBoolDisallowNew)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			AssertEquals("SystemDefined", ((CodeDescriptionBoolDisallowNew)originalBusinessObject).SystemDefined, ((CodeDescriptionBoolDisallowNew)newBusinessObject).SystemDefined);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
