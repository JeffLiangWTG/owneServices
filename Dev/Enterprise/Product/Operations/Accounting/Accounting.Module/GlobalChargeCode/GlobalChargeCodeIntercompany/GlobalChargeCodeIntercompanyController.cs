using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GlobalChargeCodeIntercompanyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GlobalChargeCodeIntercompanyController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlobalChargeCodeIntercompany; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlobalChargeCodeIntercompany; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlobalChargeCodeMapIntercompany); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlobalChargeCodeIntercompanyForm((GlobalChargeCodeMapIntercompany)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GlobalChargeCodeIntercompany; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GlobalChargeCodeIntercompany; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GlobalChargeCodeIntercompany; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GlobalChargeCodeIntercompany; }
		}
	}
}
