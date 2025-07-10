using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract partial class FilterGridModuleWithMultipleReversing : ZFilterGridModule
	{
		protected override void DeleteMultiple(BusinessObject[] selectedBusinessObjects)
		{
			var multipleReversingProvider = IsWIPAccrualsModule ? new MultipleReversingProviderForLine() : (MultipleReversingProviderBase)new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.AddRange(selectedBusinessObjects);
			BusinessObject[] multipleReversingProviderInArray = new BusinessObject[] { multipleReversingProvider };
			if (!IsWIPAccrualsModule)
			{
				PerformLevelAuthorizationForReversal(multipleReversingProvider as MultipleReversingProviderForHeader);
			}

			if (IsUserAllowedForMultipleReversing())
			{
				foreach (BusinessObject bizo in multipleReversingProvider)
				{
					ZController controller = GetNewController(bizo);
					controller.SetFormsModalTo(ParentModalForm ?? LocateMainForm());
					controller.DeleteMultiple(multipleReversingProviderInArray);

#if DEBUG
					LastFormShownForTest = controller.LastShownForm;
#endif
				}
			}
		}

		protected virtual void PerformLevelAuthorizationForReversal(MultipleReversingProviderForHeader multipleReversingProvider)
		{
		}

		bool IsWIPAccrualsModule => ID == ModuleIDs.WIPAccruals;

		protected virtual bool IsUserAllowedForMultipleReversing() => true;

#if DEBUG
		protected IZForm LastFormShownForTest;
		#endif

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("Accounting|ReverseText", "&Reverse", "Creates a new reversed item(s) to offset the currently selected item(s) (shortcut Del)");
		}

		protected override bool CanBeCopied()
		{
			return true;
		}

		protected void HandleRegenerateJournalEntries(object sender, EventArgs e)
		{
			RegenerateJournalEntriesHelper.RegenerateJournalEntries(SelectedBusinessObjects);
		}
	}
}
