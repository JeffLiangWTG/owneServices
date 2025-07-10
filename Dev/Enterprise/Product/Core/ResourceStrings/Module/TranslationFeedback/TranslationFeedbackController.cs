using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ResourceStrings.Module
{
	public class TranslationFeedbackController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.TranslationFeedback; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmTranslationFeedback); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var feedbackCollection = new TopLevelTranslationFeedbackCollection(businessEntity.Factory);
			feedbackCollection.Add((StmTranslationFeedback)businessEntity);
			return new TranslationFeedbackViewForm(feedbackCollection);
		}

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Length == 0)
			{
				return;
			}
			var topLevelCollection = new TopLevelTranslationFeedbackCollection(selectedBusinessObjects[0].Factory);
			topLevelCollection.AddRange(selectedBusinessObjects);
			foreach (StmTranslationFeedback feedback in topLevelCollection)
			{
				feedback.XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			}
			selectedBusinessObjects[0].Factory.Save();
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			DeleteMultiple(new BusinessObject[] { sourceEntity });
			return null;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TranslationFeedback; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TranslationFeedback; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TranslationFeedback; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TranslationFeedback; }
		}

		#endregion
	}
}
