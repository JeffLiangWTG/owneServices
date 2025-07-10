using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ChargeForBulkChargeImportCollection : ChargeCollection
	{
		public ChargeForBulkChargeImportCollection(InvoicingBaseBulkChargeImporterDependentJob parentJob)
			: base(parentJob)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public ZBool IsChargeSelected(Charge charge) => SelectedChargePKs.Contains(charge.PK);

		public void SetChargeSelected(Charge charge, ZBool selected)
		{
			var currentStatus = IsChargeSelected(charge);
			if (selected)
			{
				SelectedChargePKs.Add(charge.PK);
			}
			else
			{
				SelectedChargePKs.Remove(charge.PK);
			}

			if (selected != currentStatus && IsSelectedChanged != null)
			{
				IsSelectedChanged(charge, EventArgs.Empty);
			}
		}

		readonly HashSet<ZGuid> SelectedChargePKs = new HashSet<ZGuid>();

		void ChargeForBulkChargeImportCollection_IsSelectedChanged(object sender, EventArgs e)
		{
			if (IsSelectedChanged != null)
			{
				IsSelectedChanged(sender, e);
			}
		}

		public event EventHandler IsSelectedChanged;
	}
}
