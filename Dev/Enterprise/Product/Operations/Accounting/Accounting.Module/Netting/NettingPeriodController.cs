using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	class NettingPeriodController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public NettingPeriodController()
		{
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null; // Not Applicable for this module.
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			Globals.Message.Show(Res.GetString("b13d5c4c-7aab-47eb-aa18-0e92673ad954", "Please view relevant netting period in Glow desktop version."));
			return null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			Globals.Message.Show(Res.GetString("da03158a-0f92-4240-adf4-7c531c693142", "Please edit relevant netting period in Glow desktop version."));
			return null;
		}

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("de885338-705a-463a-b75d-63132b6160fe", "New netting period can be created from Glow desktop version."));
			return null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			Globals.Message.Show(Res.GetString("3a2c5699-f0a8-4329-983c-f4ba04c9d244", "Netting periods can not be deleted."));
			return null; // Not Applicable for this module.
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.NettingPeriod; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.NettingPeriod; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NettingSystemPeriod); }
		}
	}
}
