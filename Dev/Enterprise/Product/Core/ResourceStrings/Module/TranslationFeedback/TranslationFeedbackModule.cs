using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ResourceStrings.Module
{
	public class TranslationFeedbackModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.TranslationFeedback; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.TranslationFeedback);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new TranslationFeedbackFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmTranslationFeedbackCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new TranslationFeedbackFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			if (TranslationFeedbackConfiguration.IsMasterDatabase)
			{
				List<MenuItem> menuItems = new List<MenuItem>();
				menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("a17012d6-fea1-41b9-b708-a9b2b968e658", "Approve and Apply to All Contexts"), new EventHandler(ApproveAndApplyToAllContexts)));
				menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("c184e662-45eb-4654-a033-ca2a31a20d62", "Approve and Apply to User-Specified Contexts Only"), new EventHandler(ApproveAndApplyToUserSpecifiedContexts)));
				menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("e5144b92-d670-431c-8790-8c27d04d3a5d", "Reject"), new EventHandler(Reject)));
				menuItems.AddRange(base.GetNewActionMenuItems());
				return menuItems.ToArray();
			}
			else
			{
				return base.GetNewActionMenuItems();
			}
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("80a0ebe1-7880-46b7-afac-39e11b7a1fad", "Translation Search"), delegate { new TranslationFeedbackProvider().OpenTranslationSearchForm(); }));
			return menuItems.ToArray();
		}

		void ApproveAndApplyToAllContexts(object sender, EventArgs e)
		{
			Approve(true);
		}

		void ApproveAndApplyToUserSpecifiedContexts(object sender, EventArgs e)
		{
			Approve(false);
		}

		void Approve(bool applyToAllContexts)
		{
			if (this.SelectedBusinessObjects.Length == 0)
			{
				return;
			}
			var topLevelCollection = new TopLevelTranslationFeedbackCollection(this.SelectedBusinessObjects[0].Factory);
			topLevelCollection.AddRange(this.SelectedBusinessObjects);
			foreach (StmTranslationFeedback feedback in topLevelCollection)
			{
				feedback.XT_Status = TranslationFeedbackStatusList.Codes.Approved;
				if (applyToAllContexts)
				{
					foreach (StmTranslationFeedbackResource context in feedback.AllContexts)
					{
						context.Update = true;
					}
				}
			}
			this.SelectedBusinessObjects[0].Factory.Save();
		}

		void Reject(object sender, EventArgs e)
		{
			if (this.SelectedBusinessObjects.Length == 0)
			{
				return;
			}
			var topLevelCollection = new TopLevelTranslationFeedbackCollection(this.SelectedBusinessObjects[0].Factory);
			topLevelCollection.AddRange(this.SelectedBusinessObjects);
			foreach (StmTranslationFeedback feedback in topLevelCollection)
			{
				feedback.XT_Status = TranslationFeedbackStatusList.Codes.Rejected;
			}
			this.SelectedBusinessObjects[0].Factory.Save();
		}

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.TranslationFeedback; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TranslationFeedback; }
		}

		#endregion

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return !TranslationFeedbackConfiguration.IsMasterDatabase; }
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("855d63c3-febf-49f4-a629-a7b399407121", "Cancel", "Deletes the selected item after viewing its details read-only (shortcut Del)");
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override Control GetNewEmbeddedControl()
		{
			ShowModuleWarning();
			return base.GetNewEmbeddedControl();
		}

		void ShowModuleWarning()
		{
			if (Res.IsSystemDefinedEnglish(Res.CurrentLanguage) && Factory.LoadTop1<StmTranslationFeedback>(new ZQuery()) == null)
			{
				ZFormModaliser.ShowDialogAndDispose(new TranslationFeedbackModuleInfoForm());
			}
		}
	}
}
