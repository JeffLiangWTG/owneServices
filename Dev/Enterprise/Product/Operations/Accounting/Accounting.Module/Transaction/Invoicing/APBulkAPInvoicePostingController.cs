using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APBulkInvoicePostingController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BulkCostApportionmentFormHollywood((APBulkInvoicePoster)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesPayment; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.APBulkInvoicePosting; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APBulkInvoicePoster); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new APBulkInvoicePoster(Factory);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			factory.SetContext(BusinessContext.APBulkInvoicePoster);
			return factory;
		}
	}
}
