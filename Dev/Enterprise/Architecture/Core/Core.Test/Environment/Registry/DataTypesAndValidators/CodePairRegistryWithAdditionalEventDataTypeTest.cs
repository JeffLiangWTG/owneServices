using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CodePairRegistryWithAdditionalEventDataType))]
	sealed class CodePairRegistryWithAdditionalEventDataTypeTest : RegistryDataTypeTestCase<CodePairRegistryWithAdditionalEventDataType>
	{
		public void TestAdditionalItemEvent()
		{
			var mockRepo = new MockRepository(MockBehavior.Strict);

			var mockRegistryItem = mockRepo.Create<IRegistryItem>(MockBehavior.Strict);
			var mockPairListProvider = mockRepo.Create<ICodeDescriptionPairListProvider>(MockBehavior.Strict);
			CodeDescriptionPairList list1 = new CodeDescriptionPairList();
			list1.AddPair("SM1", "SomeValue 1");
			list1.AddPair("AN1", "Another Value 1");

			var wasCalled = false;
			Func<IRegistryItem, bool> mockAdditionalItemEvent = (registryItem) =>
			{
				wasCalled = true;
				return wasCalled;
			};

			var codePairRegistryDataType = new Mock<CodePairRegistryWithAdditionalEventDataType>(mockPairListProvider.Object, false, true, mockAdditionalItemEvent) { CallBase = true };

			mockPairListProvider.Setup(m => m.CodeDescriptionPairList).Returns(list1);
			codePairRegistryDataType.Setup(m => m.LookUpList).Returns(list1);
			mockRegistryItem.Setup(m => m.EditorInfo).Returns<IRegistryItem>(null);

			CodeDescriptionPairList resultList = mockPairListProvider.Object.CodeDescriptionPairList;
			codePairRegistryDataType.Object.Validate(mockRegistryItem.Object, "AN1", Guid.Empty, Guid.Empty, Guid.Empty);
			Assert(wasCalled);
		}

		protected override CodePairRegistryWithAdditionalEventDataType GetNewDataType()
		{
			Func<IRegistryItem, bool> mockAdditionalItemEvent = (registryItem) => { return true; };
			return new CodePairRegistryWithAdditionalEventDataType(null, true, false, mockAdditionalItemEvent);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes("")),
				new ValidSampleAndBinaryValueInDB("CCX", Encoding.Unicode.GetBytes("CCX")),
				new ValidSampleAndBinaryValueInDB("PPD", Encoding.Unicode.GetBytes("PPD"))
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
