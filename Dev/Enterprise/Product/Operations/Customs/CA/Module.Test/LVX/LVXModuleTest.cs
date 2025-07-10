using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(LVXModule))]
	sealed class LVXModuleTest : ZModuleBasherTest
	{
		public void TestConsolidateMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 4, 1);
			declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			using (var form = new ZForm())
			using (var module = new LVXModule())
			{
				module.SetFormsModalTo(form);
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Bulk Consolidate");
				AssertNotNull("Consolidate is present", menuItem);
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object formOrDialog) =>
				{
					var bo = (LVXSelectionCriteriaBO)((ZForm)formOrDialog).BusinessEntity;
					bo.PeriodYear = 2015;
					bo.PeriodMonth = 3;
				});
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menuItem.PerformClick();
				AssertEquals("DialogShown", typeof(LVXSelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				Assert(ZFormModaliser.LastFormShownDialogForTest.IsDisposed);
				AssertEquals("There are no Courier LVS Declaration Jobs that match the selected criteria.", UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object formOrDialog) =>
				{
					var bo = (LVXSelectionCriteriaBO)((ZForm)formOrDialog).BusinessEntity;
					if (bo != null)
					{
						bo.PeriodYear = 2015;
						bo.PeriodMonth = 4;
					}
				});

				UnitTestUserNotification.Instance.AddOKAnswer();
				menuItem.PerformClick();
				AssertEquals("DialogShown", typeof(BulkConsolidationProgressForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				Assert(ZFormModaliser.LastFormShownDialogForTest.IsDisposed);
				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertCollectionContains("There are 1 Courier LVS Declaration Job(s) that match the selected criteria.\r\nDo you wish to continue?", UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text));
			}
		}

		public void TestConsolidationShowsCorrectPathForSecurityCheckpoint()
		{
			var supporter = new LVXOperationalActionSupporter();
			var runSecurityCheckpoint = Env.Security.FindOrCreateOperationalActionsRunSpecificCheckpoint((NoResString)"Consolidate Selected", supporter.RunSecurityCheckpoint);
			runSecurityCheckpoint.IsAllowed = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 4, 1);
			declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			using (var form = new ZForm())
			using (var module = new LVXModule())
			{
				module.SetFormsModalTo(form);
				var actionMenu = module.FormActionMenu.FindByText("&Actions");
				actionMenu.OnPopup(EventArgs.Empty);
				var menuItem = actionMenu.MenuItems.FindByText("Consolidate Selected");
				AssertNotNull("Consolidate is present", menuItem);
				menuItem.PerformClick();
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
			}

			runSecurityCheckpoint.IsAllowed = false;

			using (var form = new ZForm())
			using (var module = new LVXModule())
			{
				module.SetFormsModalTo(form);
				var actionMenu = module.FormActionMenu.FindByText("&Actions");
				actionMenu.OnPopup(EventArgs.Empty);
				var menuItem = actionMenu.MenuItems.FindByText("Consolidate Selected");
				AssertNotNull("Consolidate is present", menuItem);
				menuItem.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs -> Courier LVS Declarations -> Operational Actions -> Run Actions -> Consolidate Selected", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CALVXJobs;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = factory.NewWithValidTestData<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			result.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			result.JE_MergeBy = B3MergeByList.Codes.NotMerge;
			return result;
		}
	}
}
