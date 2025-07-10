using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccApportionmentTemplateController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.AccApportionmentTemplate; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccApportionmentTemplate; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccApportionmentTemplate); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ApportionmentTemplateForm((AccApportionmentTemplate)businessEntity);
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ApportionmentTemplateDelete; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ApportionmentTemplateEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ApportionmentTemplateNew; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ApportionmentTemplateView; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<AccApportionmentTemplate>();
		}
	}
}
