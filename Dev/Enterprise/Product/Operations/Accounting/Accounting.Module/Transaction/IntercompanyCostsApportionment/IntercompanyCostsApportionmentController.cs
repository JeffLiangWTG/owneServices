using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class IntercompanyCostsApportionmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.APIntercompanyCostsApportionment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(IntercompanyCostsApportionmentInvoice); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new IntercompanyCostsApportionmentForm((IntercompanyCostsApportionmentInvoice)businessEntity);
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesOverheadCostsApportionment; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new IntercompanyCostsApportionmentInvoice(Factory);
		}
	}
}
