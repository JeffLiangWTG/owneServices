using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolDefaultReadonly))]
	sealed class CodeDescriptionBoolReadonlyTest : CodeDescriptionBoolTest
	{
		#region TestDefaultColumnReadOnly

		public void TestDefaultColumnReadOnly()
		{
			var result = (CodeDescriptionBoolDefaultReadonly)GetNewBusinessObject();
			AssertEquals(false, result.DefaultColumnReadOnly);

			result.DefaultColumnReadOnly = true;
			AssertEquals(true, result.DefaultColumnReadOnly);
		}

		#endregion

		#region TestBool_ReadOnly

		public void TestBool_ReadOnly()
		{
			var result = (CodeDescriptionBoolDefaultReadonly)GetNewBusinessObject();
			AssertEquals(false, result.BoolInfo.ReadOnly);

			result.DefaultColumnReadOnly = true;
			AssertEquals(true, result.BoolInfo.ReadOnly);
		}

		#endregion

		#region Overrides

		protected override void AssertCloneValues(RegistryBusinessObject clonedBizO)
		{
			var clone = clonedBizO as CodeDescriptionBoolDefaultReadonly;
			AssertEquals("IsBoolColumnReadOnly", true, clone.DefaultColumnReadOnly);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CodeDescriptionBoolDefaultReadonly)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			result.DefaultColumnReadOnly = true;

			return result;
		}

		#endregion
	}
}
