using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionBoolWithExtraColumnControl : CodeDescriptionBoolControl
	{
		public CodeDescriptionBoolWithExtraColumnControl(bool isDescriptionColumnTranslatable) : base(isDescriptionColumnTranslatable)
		{
			var index = CodeDescriptionBoolGrid.ColumnStyles.Count - 1;
			index = index < 0 ? 0 : index;
			ExtraZTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ExtraZTextBoxColumnStyleInfo.ColumnName = "String1";
			ExtraZTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("6682AAA4-3776-4D8F-B8D7-693C5450B2B2", "Extra Column");
			CodeDescriptionBoolGrid.ColumnStyles.Insert(index, ExtraZTextBoxColumnStyleInfo);
		}
		public void SetupExtraColumn(ResourceStringData extraColumnCaption, bool extraColumnReadOnly)
		{
			ExtraZTextBoxColumnStyleInfo.CaptionResourceString = extraColumnCaption;
			ExtraZTextBoxColumnStyleInfo.IsReadOnly = extraColumnReadOnly;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var codeDescriptionBoolDataSource = dataSource as CodeDescriptionBoolDisallowNewCodeReadOnlyWithExtraStringCollection;
			base.SetDataBinding(codeDescriptionBoolDataSource, dataMember);
		}

		readonly ZTextBoxColumnStyleInfo ExtraZTextBoxColumnStyleInfo;
	}
}
