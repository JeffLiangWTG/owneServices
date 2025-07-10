using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MailManager.Module
{
	public class MailItemTemplateController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public MailItemTemplateController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.MailItemTemplate; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.MailItemTemplate; }
		}

		public override System.Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MailItemTemplate); }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return new MailItemTemplateForm((MailItemTemplate)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.EmailTemplates; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.EmailTemplates; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EmailTemplates; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.EmailTemplates; }
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			var template = bizObject as MailItemTemplate;
			if (template != null && !IsEmailTemplateBelongingToCurrentLoginCompanyBranchDepartment(template))
			{
				return Env.Security.EditAllEmailTemplates;
			}
			else
			{
				return base.GetCheckPointForEdit(bizObject);
			}
		}

		bool IsEmailTemplateBelongingToCurrentLoginCompanyBranchDepartment(MailItemTemplate template)
		{
			return (template.MIT_GC_Company.IsEmpty || template.MIT_GC_Company == GlbCompany.CurrentCompany.PK) &&
				   (template.MIT_GB_Branch.IsEmpty || template.MIT_GB_Branch == GlbBranch.CurrentBranch.PK) &&
				   (template.MIT_GE_Department.IsEmpty || template.MIT_GE_Department == GlbDepartment.CurrentDepartment.PK);
		}
	}
}
