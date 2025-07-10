using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using ZClientEDI.Business.Registry;

namespace ZClientEDI.Business.Test.Registry.FallbackWorkItemCriteria
{
	[TestedType(typeof(WorkItemCriteriaRegistryDataType))]
	sealed class WorkItemCriteriaRegistryDataTypeTest : RegistryDataTypeTestCase<WorkItemCriteriaRegistryDataType>
	{
		protected override WorkItemCriteriaRegistryDataType GetNewDataType()
		{
			return new WorkItemCriteriaRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return
			[
				new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes("")),
				new ValidSampleAndBinaryValueInDB("ABC/DE/H2J", Encoding.Unicode.GetBytes("ABC/DE/H2J")),
				new ValidSampleAndBinaryValueInDB("ABC/DEF/HIJ", Encoding.Unicode.GetBytes("ABC/DEF/HIJ")),
			];
		}

		protected override object[] GetInvalidSamples()
		{
			return ["ABCDEF", "   ", "/", "//", "1/xY1/ZZZZ"];
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;
	}
}
