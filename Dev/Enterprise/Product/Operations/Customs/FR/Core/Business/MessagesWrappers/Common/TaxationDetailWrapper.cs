using CargoWise.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class TaxationDetailWrapper : ITaxationDetail
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public TaxationDetailWrapper(CusEntryLineFee entryLineFee)
		{
			this.entryLineFee = Argument.NotNull(entryLineFee, "Entry line fee cannot be null");
		}

		public TaxationDetailWrapper(CusEntryHeaderCharges entryHeaderCharge)
		{
			this.entryHeaderCharge = Argument.NotNull(entryHeaderCharge, "Entry header charge cannot be null");
		}

		public ITax Tax => GetTax();

		public ISupplementaryUnit SuppUnit => GetSuppUnits();

		public ITax GetTax()
		{
			if (entryLineFee != null)
			{
				return new EntryLineFeeWrapper(entryLineFee);
			}
			else if (entryHeaderCharge != null)
			{
				return new EntryHeaderChargeWrapper(entryHeaderCharge);
			}

			return null;
		}

		public ISupplementaryUnit GetSuppUnits()
		{
			if (entryLineFee != null && Tax.TaxType == "0" && entryLineFee.CF_MethodOfCalculation != "%")
			{
				return new SupplementaryUnitWrapper((CusEntryLine)entryLineFee?.EntryLine);
			}
			else
			{
				return new SupplementaryUnitWrapper();
			}
		}

		readonly CusEntryLineFee entryLineFee;
		readonly CusEntryHeaderCharges entryHeaderCharge;
	}
}
