using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.WorkflowDataRegistry.EnableLimitsOnWorkflowTemplates;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BooleanWithCheck))]
	sealed class BooleanWithCheckTest : RegistryDataTypeTestCase<BooleanWithCheck>
	{
		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		protected override void AssertValuesEqualForCheckingDefault(string message, object lhs, object rhs)
		{
			var left = (bool)lhs;
			var right = (bool)rhs;

			AssertEquals(left, right);
		}
		protected override BooleanWithCheck GetNewDataType()
		{
			return new BooleanWithCheck();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = true;
			var result2 = false;

			return new ValidSampleAndBinaryValueInDB[]
			{
					new ValidSampleAndBinaryValueInDB(result, new BooleanRegistryDataType().Serialise(result)),
					new ValidSampleAndBinaryValueInDB(result2, new BooleanRegistryDataType().Serialise(result2))
			};
		}
	}
}
