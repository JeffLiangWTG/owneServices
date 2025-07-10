using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class EnquiryController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.Enquiry; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Enquiry); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ClientModuleRegistration.Enquiry; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EnquiryForm(businessEntity as Enquiry);
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
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
	}
}
