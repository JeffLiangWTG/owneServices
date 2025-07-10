using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationPartyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Messaging.EDICommunicationParty; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Messaging.EDICommunicationParty; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDICommunicationParty); }
		}

		// TODO: Base form is incorrect, to be updated
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDICommunicationPartyForm((EDICommunicationParty)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.EDICommunicationPartyDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EDICommunicationPartyEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.EDICommunicationPartyNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.EDICommunicationPartyView; }
		}
	}
}
