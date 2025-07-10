using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Chief.Messaging.DLU;
using Enterprise.Customs.GB.GUI.DLU;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module.DLU
{
	public class DLUMessageController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.GB.DLUMessage; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.DLUController; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DLUMessage); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DLUMessageForm((DLUMessage)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsFiles; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsFiles; }  // not really applicable
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsFiles; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsFiles; }
		}
	}
}
