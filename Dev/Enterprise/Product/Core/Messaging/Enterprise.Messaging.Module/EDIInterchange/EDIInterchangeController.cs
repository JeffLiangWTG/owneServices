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
	public class EDIInterchangeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Messaging.EDIInterchange; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Messaging.EDIInterchange; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIInterchange); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIInterchangeForm((EDIInterchange)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.EDIInterchangeModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EDIInterchangeModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.EDIInterchangeModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.EDIInterchange; }
		}
	}
}
