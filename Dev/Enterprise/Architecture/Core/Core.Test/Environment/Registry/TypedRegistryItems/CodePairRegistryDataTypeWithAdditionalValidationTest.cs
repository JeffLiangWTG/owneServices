using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CodePairRegistryDataTypeWithAdditionalValidation))]
	sealed class CodePairRegistryDataTypeWithAdditionalValidationTest : RegistryDataTypeTestCase<CodePairRegistryDataTypeWithAdditionalValidation>
	{
		public void TestAdditionalValidation()
		{
			var registryDataType = GetNewDataType();

			ValidationResult = null;
			AssertNoExceptionThrown(() => registryDataType.Validate(null, "SMV", Guid.Empty, Guid.Empty, Guid.Empty));

			ValidationResult = string.Empty;
			AssertNoExceptionThrown(() => registryDataType.Validate(null, "SMV", Guid.Empty, Guid.Empty, Guid.Empty));

			ValidationResult = "Error!";
			AssertExceptionThrown<RegistryValidationException>("Error!", () => registryDataType.Validate(null, "SMV", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		string ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return ValidationResult;
		}

		string ValidationResult;

		Mock<ICodeDescriptionPairListProvider> MoqPairListProvider
		{
			get
			{
				if (moqPairListProvider == null)
				{
					var list = new CodeDescriptionPairList();
					list.AddPair("SMV", "Some Value");
					list.AddPair("ANV", "Another Value");

					moqPairListProvider = new Mock<ICodeDescriptionPairListProvider>();
					moqPairListProvider.Setup(m => m.CodeDescriptionPairList)
						.Returns(list);
				}

				return moqPairListProvider;
			}
		}
		Mock<ICodeDescriptionPairListProvider> moqPairListProvider;

		protected override CodePairRegistryDataTypeWithAdditionalValidation GetNewDataType()
		{
			return new CodePairRegistryDataTypeWithAdditionalValidation(MoqPairListProvider.Object, true, true, ValidateCore);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes("")),
				new ValidSampleAndBinaryValueInDB("SMV", Encoding.Unicode.GetBytes("SMV")),
				new ValidSampleAndBinaryValueInDB("ANV", Encoding.Unicode.GetBytes("ANV"))
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
	}
}
