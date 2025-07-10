using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationsModeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Messaging.EDICommunicationsMode; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Messaging.EDICommunicationsMode; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDICommunicationsMode); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			SetInitialTabPageNameToSelectWhenAFormIsShown($"{OrganisationTabPages.Details.Name}+Config+EDI Communications");
			return new ZOrganisationsForm((EDICommunicationsMode)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrganisationView; }
		}
	}
}
