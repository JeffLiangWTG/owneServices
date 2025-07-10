using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.Module
{
	public class EDIMessageController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Messaging.EDIMessage; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Messaging.EDIMessage; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIMessage); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIMessageForm((EDIMessage)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.EDIMessageModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EDIMessageModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.EDIMessageModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.EDIMessage; }
		}
	}
}
