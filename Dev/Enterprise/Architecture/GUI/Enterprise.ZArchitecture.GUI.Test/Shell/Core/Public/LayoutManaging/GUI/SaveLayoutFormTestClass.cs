using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class SaveLayoutFormTestClass : SaveLayoutForm
	{
		public SaveLayoutFormTestClass(SaveLayoutBizO bizObj, bool shouldSaveColumnBeVisible, bool shouldShowSaveAsUserDefinedFilter)
			: base(bizObj, shouldSaveColumnBeVisible, shouldShowSaveAsUserDefinedFilter)
		{
		}

		public ZCheckBox IsUserDefinedFilterCheckBoxExposed
		{
			get { return IsUserDefinedFilterCheckBox; }
		}

		public ZCheckBox SaveColumnsCheckBoxExposed
		{
			get { return SaveColumnsCheckBox; }
		}

		public ZButton SaveButtonExposed
		{
			get { return this.SaveFilterButton; }
		}

		public ZButton CancelButtonExposed
		{
			get
			{
				return this.CancelSaveFilterButton;
			}
		}
	}
}
