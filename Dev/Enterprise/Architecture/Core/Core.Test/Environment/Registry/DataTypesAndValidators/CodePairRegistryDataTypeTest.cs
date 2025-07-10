using System.Text;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(CodePairRegistryDataType))]
	sealed class CodePairRegistryDataTypeTest : RegistryDataTypeTestCase<CodePairRegistryDataType>
	{
		public void TestDedaultEditor()
		{
			var mockPairListProvider = new Mock<ICodeDescriptionPairListProvider>();

			CodeDescriptionPairList list1 = new CodeDescriptionPairList();
			list1.AddPair("SM1", "SomeValue 1");
			list1.AddPair("AN1", "Another Value 1");

			CodeDescriptionPairList list2 = new CodeDescriptionPairList();
			list2.AddPair("SM2", "SomeValue 2");
			list2.AddPair("AN2", "Another Value 2");

			CodePairRegistryDataType codePairRegistryDataType = new CodePairRegistryDataType(mockPairListProvider.Object, false, false);
			ComboBoxRegistryEditorInfo editorInfo = (ComboBoxRegistryEditorInfo)codePairRegistryDataType.DefaultEditorInfo;
			mockPairListProvider.Setup(m => m.CodeDescriptionPairList)
				.Returns(list1);

			AssertEquals(list1.ElementsAsString, editorInfo.LookUpList.ElementsAsString);

			mockPairListProvider.Setup(m => m.CodeDescriptionPairList)
				.Returns(list2);

			AssertEquals(list2.ElementsAsString, editorInfo.LookUpList.ElementsAsString);
			mockPairListProvider.VerifyAll();
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override CodePairRegistryDataType GetNewDataType()
		{
			return new CodePairRegistryDataType(OLookUpEditType.PaymentType);
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

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "ZZZ" };
		}

		public void TestLookUpListIsLazyLoaded()
		{
			var listIsCreated = false;
			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				listIsCreated = true;
				return new CodeDescriptionPairList(OLookUpEditType.CustomType);
			});
			var codePairRegistryDataType = new CodePairRegistryDataType(listProvider, false, false);

			Assert("LookUpList should not be loaded yet", !listIsCreated);

			var lookUpList = codePairRegistryDataType.LookUpList;
			Assert("CodeDescriptionPairList is now loaded when it is called for", listIsCreated);
		}
	}
}
