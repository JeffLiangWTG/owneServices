using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.GUI.CashBook.BankReconciliation;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class BankReconcilliationController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BankReconcilationForm((BankReconciliation)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BankReconcilliation; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BankReconciliation); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new BankReconciliation(Factory);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.BankReconciliation; }
		}
	}
}
