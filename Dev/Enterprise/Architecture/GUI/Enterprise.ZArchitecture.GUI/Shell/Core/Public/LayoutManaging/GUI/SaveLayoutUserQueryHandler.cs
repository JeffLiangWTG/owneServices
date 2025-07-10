using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class SaveLayoutUserQueryHandler
	{
		public SaveLayoutBizO QueryUser(IModifyModuleAndGridLayout layoutManageable, bool shouldShowSaveColumnCheckBox, bool shouldShowSaveAsUserDefinedFilter, SaveLayoutBizO defaultValues = null)
		{
			SaveLayoutBizO result = null;

			var preConditionMessage = layoutManageable.ValidateAndGetErrorsForSavingLayout();

			if (!string.IsNullOrEmpty(preConditionMessage))
			{
				Globals.Message.ShowError(preConditionMessage);
			}
			else
			{
				var saveLayoutBizObj = defaultValues ?? GetSaveLayoutBizObj(layoutManageable);
				var isOKToSave = false;

				using (var form = GetSaveLayoutForm(saveLayoutBizObj, shouldShowSaveColumnCheckBox, shouldShowSaveAsUserDefinedFilter))
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
					isOKToSave = form.IsOkToSave;
				}

				if (isOKToSave)
				{
					result = saveLayoutBizObj;
				}
			}

			return result;
		}

		protected virtual SaveLayoutBizO GetSaveLayoutBizObj(IModifyModuleAndGridLayout layoutManageable)
		{
			return new SaveLayoutBizO(layoutManageable);
		}

		protected virtual SaveLayoutForm GetSaveLayoutForm(SaveLayoutBizO saveLayoutBizObj, bool shouldShowSaveColumnCheckBox, bool shouldShowSaveAsUserDefinedFilter)
		{
			return new SaveLayoutForm(saveLayoutBizObj, shouldShowSaveColumnCheckBox, shouldShowSaveAsUserDefinedFilter);
		}
	}
}
