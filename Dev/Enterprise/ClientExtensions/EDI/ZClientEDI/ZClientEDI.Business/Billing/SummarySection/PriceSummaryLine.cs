using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// For "STL Billing Summary.xls"
	/// </summary>
	public class PriceSummaryLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PriceSummaryLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString Currency { get; set; }

		public ZString UndiscountedAmount { get; set; }
		public ZString DiscountAmount { get; set; }

		public ZString VersionSurchargeDescription { get; set; }
		public ZString VersionSurchargeAmount { get; set; }

		public ZString InvoiceSurchargeDescription { get; set; }
		public ZString InvoiceSurchargeAmount { get; set; }

		public ZString FinalAmount { get; set; }

		public ZString GroupBy => Currency;
	}
}
