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
	public class GlobalChargeCodeOrganizationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GlobalChargeCodeOrganizationController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlobalChargeCodeOrganization; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlobalChargeCodeOrganization; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlobalChargeCodeMapOrganization); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlobalChargeCodeOrganizationForm((GlobalChargeCodeMapOrganization)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GlobalChargeCodeOrganization; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GlobalChargeCodeOrganization; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GlobalChargeCodeOrganization; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GlobalChargeCodeOrganization; }
		}
	}
}
