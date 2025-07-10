using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceHeaderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public LicenceHeaderController()
		{
		}

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("Can't create a new LicenceHeader");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Can't view a new LicenceHeader");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Can't edit a new LicenceHeader");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Can't delete a new LicenceHeader");
		}

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.LicenceHeader; }
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.LicenceHeader; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Business.LicenceHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrganisationView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return EDISecurityCheckpoints.OrgLicenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.OrgLicenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceModify; }
		}
	}
}
