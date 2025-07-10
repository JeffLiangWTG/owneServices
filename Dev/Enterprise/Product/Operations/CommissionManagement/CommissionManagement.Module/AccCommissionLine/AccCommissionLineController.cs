using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CommissionManagement.Module
{
	public class AccCommissionLineController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.CommissionLine; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ViewCommissionLine); }
		}

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException("Does not support New and Delete functionality");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			BusinessObject entityToShow;
			var controller = GetController(sourceEntity, out entityToShow);
			if (controller != null)
			{
				LastShownForm = controller.ShowViewForm(entityToShow);
			}
			else
			{
				LastShownForm = null;
			}

			return LastShownForm;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			BusinessObject entityToShow;
			var controller = GetController(sourceEntity, out entityToShow);
			if (controller != null)
			{
				LastShownForm = controller.ShowEditForm(entityToShow);
			}
			else
			{
				LastShownForm = null;
			}

			return LastShownForm;
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("Does not support New functionality");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("Does not support Delete functionality");
		}

		ZController GetController(BusinessObject sourceEntity, out BusinessObject entityToShow)
		{
			var commissionLine = sourceEntity as ViewCommissionLine;
			var commissionHeader = commissionLine != null ? commissionLine.CommissionHeader : null;
			if (commissionHeader != null)
			{
				entityToShow = commissionHeader.Source;
				return AccountingControllerCreator.GetNewController(commissionHeader.Source);
			}

			entityToShow = null;
			return null;
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CommissionManagerCancel; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CommissionManagerView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CommissionManagerNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CommissionManagerView; }
		}

		#endregion
	}
}
