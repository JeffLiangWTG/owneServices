using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class PeriodicInvoiceBulkController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesBulkPeriodicInvoice; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new PeriodicInvoicingBulkForm((PeriodicInvoiceBulk)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.PeriodicInvoiceBulk; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(PeriodicInvoiceBulk); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new PeriodicInvoiceBulk(Factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
