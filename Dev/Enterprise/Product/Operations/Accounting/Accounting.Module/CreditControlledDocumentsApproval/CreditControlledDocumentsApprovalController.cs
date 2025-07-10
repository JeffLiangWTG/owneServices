using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CreditControlledDocumentsApprovalController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public CreditControlledDocumentsApprovalController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CreditControlledDocumentsApproval; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CreditControlledDocumentsApproval; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CreditControlledDocumentsApproval); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CreditControlledDocumentsApprovalForm(new CreditControlledDocumentsApprovalBulk(businessEntity.Factory, (CreditControlledDocumentsApproval)businessEntity), CreditControlledDocumentsApprovalFormModes.View);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CreditControlledDocumentsApproval; }
		}
	}
}
