using System.Text;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.Registry.JobCosting
{
	[TestedType(typeof(JCDServiceTaskControllerRegistryItemDataType))]
	class JCDServiceTaskControllerRegistryItemDataTypeTest : RegistryDataTypeTestCase<JCDServiceTaskControllerRegistryItemDataType>
	{
		protected override JCDServiceTaskControllerRegistryItemDataType GetNewDataType()
		{
			return new JCDServiceTaskControllerRegistryItemDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("NON", Encoding.Unicode.GetBytes("NON")),
				new ValidSampleAndBinaryValueInDB("STR", Encoding.Unicode.GetBytes("STR"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "ZZZ" };
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue
		{
			get { return true; }
		}
	}
}
