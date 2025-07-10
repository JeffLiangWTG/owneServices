using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Statements;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class StatementController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Statement; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Statement); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new StatementPrintForm(Statement.New(GlbBranch.CurrentBranch));
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Statement.New(GlbBranch.CurrentBranch);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ReceivablesCollectionDocuments; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}

		#endregion
	}
}
