using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GLBudgetController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GLBudgetController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GLBudget; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GLBudget; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GLBudget); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GLBudgetForm((GLBudget)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get	{ return Env.Security.DeleteBudget; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EditBudget; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get	{ return Env.Security.NewBudget; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewBudget; }
		}
	}
}
