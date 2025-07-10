using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.GUI.ARAP.HotCheque;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccHotChequeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccHotChequeController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccHotCheque; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccHotCheque; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccHotCheque); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccHotChequeForm((AccHotCheque)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CancelHotCheque; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EditHotCheque; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewHotCheque; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewHotCheque; }
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			AccHotCheque cheque = sourceEntity as AccHotCheque;
			if (cheque.AQ_Cancelled || cheque.AQ_AH.IsValid)
			{
				return base.ShowViewForm(sourceEntity);
			}
			else
			{
				return base.ShowEditForm(sourceEntity);
			}
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			AccHotCheque cheque = sourceEntity as AccHotCheque;

			if (cheque.AQ_Cancelled)
			{
				Globals.Message.ShowError(Res.GetString("cde46e9c-7331-40e9-8333-d4a2657763f2", "This cheque is already canceled."));
				return null;
			}
			else if (cheque.AQ_AH.IsValid)
			{
				Globals.Message.ShowError(Res.GetString("88577653-c326-4fbd-93e1-9bd8930b21a8", "This cheque is posted and cannot be canceled."));
				return null;
			}
			else
			{
				return base.ShowDeleteForm(cheque);
			}
		}
	}
}
