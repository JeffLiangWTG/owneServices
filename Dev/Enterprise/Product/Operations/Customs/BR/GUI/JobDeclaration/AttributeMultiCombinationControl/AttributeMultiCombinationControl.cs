using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.Customs.BR.GUI
{
	public class AttributeMultiCombinationControl : ZMultiCombinationControl
	{
		protected override IGridControl CreateGridControl(FieldType typeToCreate)
		{
			if (typeToCreate == FieldType.TextCodeFindBox)
			{
				var findBox = new MultiCodesFindBox();
				findBox.BindToList = BindToList;
				return findBox;
			}
			return base.CreateGridControl(typeToCreate);
		}
	}
}
