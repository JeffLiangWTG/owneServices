using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDisallowNewWithDefaultDisabled))]
	sealed class CodeDescriptionBoolDisallowNewWithDefaultDisabledTest : CodeDescriptionBoolTest
	{
		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			CodeDescriptionBoolDisallowNewWithDefaultDisabled clone = clone1 as CodeDescriptionBoolDisallowNewWithDefaultDisabled;
			AssertEquals("Code", "TSCOD", clone.Code);
			AssertEquals("Description", "DescriptionA", clone.Description);
			AssertEquals("CodeMaxLength", 5, clone.CodeMaxLength);
			Assert("SystemDefined", clone.SystemDefined);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CodeDescriptionBoolDisallowNewWithDefaultDisabled result = (CodeDescriptionBoolDisallowNewWithDefaultDisabled)GetNewBusinessObject();

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
