using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APCreditNoteLineCollection : InvoicingLineBaseCollection
	{
		public APCreditNoteLineCollection(BusinessObject parent)
			: base(parent)
		{
		}

		public new APCreditNoteLine this[int i]
		{
			get { return (APCreditNoteLine)Elements[i]; }
		}

		public new APCreditNoteLine AddNew()
		{
			return (APCreditNoteLine)base.AddNew();
		}

		protected override void SetDefaultsForNewChildCore(BusinessObject child)
		{
			APCreditNoteLine newLine = child as APCreditNoteLine;
			if (newLine != null && AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.Value)
			{
				newLine.ShowJobChargesForImportEvent += OnShowJobChargesForImportEvent;
			}
			base.SetDefaultsForNewChildCore(child);
		}
	}
}
