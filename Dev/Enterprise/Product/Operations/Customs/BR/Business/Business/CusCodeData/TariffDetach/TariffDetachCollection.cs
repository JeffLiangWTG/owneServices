using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class TariffDetachCollection : CusCodeDataCollection<TariffDetach>
	{
		public TariffDetachCollection(JobComInvoiceLine parent)
			: base(parent, CusCodeDataTypeList.Codes.TariffDetach)
		{
		}

		public TariffDetachCollection(CusClassPartPivot parent)
			: base(parent, CusCodeDataTypeList.Codes.TariffDetach)
		{
		}

		public override bool ReadOnly => base.ReadOnly || (Master is JobComInvoiceLine parent && parent.HasLinkedInvoiceLine);

		public ZString ConcatenatedCodes => string.Join(",", Where(x => !x.CY_Code.IsEmpty).Select(x => x.CY_Code).OrderBy(x => x));
	}
}
