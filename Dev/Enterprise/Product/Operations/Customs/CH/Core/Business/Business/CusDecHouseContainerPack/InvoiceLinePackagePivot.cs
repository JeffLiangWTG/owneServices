using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class InvoiceLinePackagePivot : Customs.Business.InvoiceLinePackagePivot
{
	public InvoiceLinePackagePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override ZPropertyInfo CHC_NumberOfPacksInfo
	{
		get
		{
			var numberOfPacksInfo = base.CHC_NumberOfPacksInfo;
			numberOfPacksInfo.HumanReadableName = Res.GetString("1A0A0DB3-354E-435C-8189-6FC72F144E29", "Invoice Line Pack Quantity");
			return numberOfPacksInfo;
		}
	}

	protected override CusHouseContPackInvoiceLinePivotValidation GetNewValidation()
	{
		return new InvoiceLinePackagePivotValidation(this);
	}

	public new Package Package => (Package)base.Package;
}
