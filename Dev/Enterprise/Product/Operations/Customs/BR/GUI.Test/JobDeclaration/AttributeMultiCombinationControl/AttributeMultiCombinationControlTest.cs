using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class AttributeMultiCombinationControlTest : ZMultiCombinationControlTest
	{
		public override ZMultiCombinationControl GetNewMultiCombinationControl()
		{
			return new AttributeMultiCombinationControl();
		}

		public new void TestShowingCodeFindBox()
		{
			RunBindToListTest(FieldType.TextCodeFindBox, typeof(MultiCodesFindBox), true);
		}
	}
}
