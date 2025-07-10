using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.PAVE.MENT.Module
{
	public class MENTAgedScoreExtractionController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ControllerID ID
		{
			get { return ControllerIDs.MENTAgedScoreExtraction; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MENTAgedScoreExtraction); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.MENTAgedScoreExtraction; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return MENTAgedScoreExtractionFormFactory.ShowForm(businessEntity);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			base.ShowEditForm(sourceEntity);

			MENTAgedScoreExtractionFormFactory.NavigateToExtractionItem(LastShownForm, (MENTAgedScoreExtraction)sourceEntity);

			return LastShownForm;
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			base.ShowViewForm(sourceEntity);

			MENTAgedScoreExtractionFormFactory.NavigateToExtractionItem(LastShownForm, (MENTAgedScoreExtraction)sourceEntity);

			return LastShownForm;
		}

		public override IZForm ShowNewForm()
		{
			return null;
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.MENTAgedScoreQueryView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.MENTAgedScoreQueryEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MENTAgedScoreExtractionNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MENTAgedScoreExtractionDelete; }
		}

		#endregion
	}
}
