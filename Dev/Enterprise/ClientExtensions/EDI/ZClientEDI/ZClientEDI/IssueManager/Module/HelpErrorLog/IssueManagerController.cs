using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.IssueManager.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	public class IssueManagerController : ZController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.IssueManager; }
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.IssueManager; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EdiHelpErrorLog); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ErrorLogForm((EdiHelpErrorLog)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return EDISecurityCheckpoints.IssueManager; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.IssueManager; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.IssueManager; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return EDISecurityCheckpoints.IssueManager; }
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

#endregion
	}
}
