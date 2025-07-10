using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ICSPermitCollection : CusCodeDataCollection<ICSPermit>
	{
		public ICSPermitCollection(CusClassPartPivot pivot)
			: base(pivot, CusCodeDataTypeList.Codes.ICSPermit)
		{
		}

		public ICSPermitCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.ICSPermit)
		{
		}

		public ZString GetSortedPermitNumbers() => string.Join(", ", Where(x => !x.CY_Data.IsEmpty).Select(x => x.CY_Data).OrderBy(x => x));
	}
}
