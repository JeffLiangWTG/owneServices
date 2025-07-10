using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLPackLinesDisplayOrderRegistryDataType))]
	sealed class HBLPackLinesDisplayOrderRegistryDataTypeTest : RegistryDataTypeTestCase<HBLPackLinesDisplayOrderRegistryDataType>
	{
		public void TestDedaultEditor()
		{
			var mockPairListProvider = new Mock<ICodeDescriptionPairListProvider>();

			var list1 = new CodeDescriptionPairList();
			list1.AddPair("SM1", "SomeValue 1");
			list1.AddPair("AN1", "Another Value 1");

			var list2 = new CodeDescriptionPairList();
			list2.AddPair("SM2", "SomeValue 2");
			list2.AddPair("AN2", "Another Value 2");

			var dataType = new HBLPackLinesDisplayOrderRegistryDataType(mockPairListProvider.Object);
			var editorInfo = (HBLPackLinesDisplayOrderRegistryEditorInfo)dataType.DefaultEditorInfo;
			mockPairListProvider.Setup(m => m.CodeDescriptionPairList).Returns(list1);

			AssertEquals(list1.ElementsAsString, editorInfo.LookUpList.ElementsAsString);

			mockPairListProvider.Setup(m => m.CodeDescriptionPairList).Returns(list2);

			AssertEquals(list2.ElementsAsString, editorInfo.LookUpList.ElementsAsString);
			mockPairListProvider.VerifyAll();
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override HBLPackLinesDisplayOrderRegistryDataType GetNewDataType()
		{
			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(Core.Constants.HBLPackLinesDisplayOrders.ContainerAndPackingOrder, "Container + Packing Order");
				list.AddPair(Core.Constants.HBLPackLinesDisplayOrders.ShowDGCargoFirst, "Show DG Cargo First");
				return list;
			});

			return new HBLPackLinesDisplayOrderRegistryDataType(listProvider);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(Core.Constants.HBLPackLinesDisplayOrders.ContainerAndPackingOrder, Encoding.Unicode.GetBytes(Core.Constants.HBLPackLinesDisplayOrders.ContainerAndPackingOrder)),
				new ValidSampleAndBinaryValueInDB(Core.Constants.HBLPackLinesDisplayOrders.ShowDGCargoFirst, Encoding.Unicode.GetBytes(Core.Constants.HBLPackLinesDisplayOrders.ShowDGCargoFirst))
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
			var dataType = new HBLPackLinesDisplayOrderRegistryDataType(listProvider);

			Assert("LookUpList should not be loaded yet", !listIsCreated);

			var lookUpList = dataType.LookUpList;
			Assert("CodeDescriptionPairList is now loaded when it is called for", listIsCreated);
		}
	}
}
