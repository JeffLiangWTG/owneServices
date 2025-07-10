using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobComInvoiceHeaderDocAddress : JobDocAddress
{
	public JobComInvoiceHeaderDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}
}
