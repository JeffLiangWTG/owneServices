using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLineExportPermitCollection : Customs.Business.CusCodeDataCollection<InvoiceLineExportPermit>
	{
		public InvoiceLineExportPermitCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.Permit)
		{
		}

		public new InvoiceLineExportPermit AddNew(ZString permitNumber)
		{
			return AddNew(CusCodeDataTypeList.Codes.Permit, permitNumber);
		}

		public void Add(ZString permitNumsAsAString)
		{
			if (!permitNumsAsAString.IsEmpty)
			{
				var permitNums = permitNumsAsAString.Split(';', ',');
				foreach (var num in permitNums)
				{
					if (!ContainsNumber(num))
					{
						AddNew(num);
					}
				}
			}
		}

		public bool ContainsNumber(ZString number)
		{
			return this.Cast<InvoiceLineExportPermit>().Any(x => x.CY_Data == number);
		}
	}
}
