#if DEBUG
using System;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DbUpgrader.Data;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Business
{
	public class GlbReleaseNoteManagerForSourceSafe : GlbReleaseNoteManager
	{
		public GlbReleaseNoteManagerForSourceSafe(BusinessObjectFactory factory)
			: base(factory, true, "")
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			showReadNotes = true;
		}
		internal void InternalSetDefaultValues() => SetDefaultValues();

		protected override bool ShouldFilterByModule
		{
			get { return false; }
		}
		internal bool InternalShouldFilterByModule => ShouldFilterByModule;

		#endregion

		protected override string[] AdditionalReleaseNoteSectionsToShow
		{
			get { return new[] { NewsSectionTypeList.Codes.WiseNews, NewsSectionTypeList.Codes.WiseLearningUpdates, NewsSectionTypeList.Codes.TechnicalAdvisoryNotes, NewsSectionTypeList.Codes.BorderWise, NewsSectionTypeList.Codes.WiseTechAcademy }; }
		}

		#region Source Control

		public void CheckIn()
		{
			CheckInCore();
		}

		public bool Checkout(out string errorMessage)
		{
			return CheckoutCore(out errorMessage);
		}

		public void UndoCheckout()
		{
			UndoCheckoutCore();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		protected virtual void CheckInCore()
		{
			Controller.FullSave();

			var now = DateTime.Now;
			var submissionName = string.Format("Update Notes {0:0000}{1:00}{2:00} {3:00}{4:00}", now.Year, now.Month, now.Day, now.Hour, now.Minute);
			using (var sourceControl = SourceControlFactory.Instance.GetSourceControl(Controller.DataVersionFile))
			{
				sourceControl.SubmitChanges(Env.CurrentUser.Initials, submissionName, "UN", new string[] { Controller.DataVersionFile });
			}
			ReleaseNotes.SetReadOnlyIncludingChildren(true);
		}

		protected virtual bool CheckoutCore(out string errorMessage)
		{
			bool result = true;
			errorMessage = "";

			try
			{
				Controller.FullCheckOut();
				ReleaseNotes.SetReadOnlyIncludingChildren(false);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				result = false;
				errorMessage = e.Message;
			}

			return result;
		}

		protected virtual void UndoCheckoutCore()
		{
			Controller.FullUndoCheckOut();
			ReleaseNotes.SetReadOnlyIncludingChildren(true);
		}

		#endregion

		#region Data Upgrade

		internal DataUpgradeSetupController Controller
		{
			get
			{
				if (fController == null)
				{
					UpgradeTask[] tasks = Array.Empty<UpgradeTask>();
					fController = GetNewDataUpgradeSetupController(tasks);
				}

				return fController;
			}
		}

		protected virtual DataUpgradeSetupController GetNewDataUpgradeSetupController(UpgradeTask[] tasks)
		{
			return new DataUpgradeSetupController(tasks);
		}

		DataUpgradeSetupController fController;

#endregion
	}
}
#endif
