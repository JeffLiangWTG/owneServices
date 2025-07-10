using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Client.EDI.DocManager.Business
{
	public class EDIARInvoiceDocManagerInfo : ARInvoiceDocManagerInfo
	{
		protected EDIARInvoiceDocManagerInfo(BusinessObject parent, string docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		public static new ARInvoiceDocManagerInfo New(BusinessObject parent, string docManagerCode)
		{
			return new EDIARInvoiceDocManagerInfo(parent, docManagerCode);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNew);
		}

		static ARInvoiceDocManagerInfo OverriddenNew(BusinessObject parent, string docManagerCode)
		{
			return New(parent, docManagerCode);
		}

		/// <summary>
		/// Never readonly for accounts - for adding timecard sheets to invoices
		/// </summary>
		public override bool ReadOnly
		{
			get { return false; }
		}
	}
}
