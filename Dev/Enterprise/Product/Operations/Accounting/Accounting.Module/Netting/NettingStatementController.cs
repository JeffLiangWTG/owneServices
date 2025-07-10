using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.Netting;
using Enterprise.Accounting.Netting;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class NettingStatementController : ZSingletonController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ParticipantStatementForm((NettingDocumentPrinter)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.NettingStatement; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NettingDocumentPrinter); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return NettingDocumentPrinter.New();
		}
	}
}
