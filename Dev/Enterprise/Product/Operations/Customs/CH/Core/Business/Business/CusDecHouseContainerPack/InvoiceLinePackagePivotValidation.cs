using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

internal class InvoiceLinePackagePivotValidation : CusHouseContPackInvoiceLinePivotValidation
{
	public InvoiceLinePackagePivotValidation(AutoCusHouseContPackInvoiceLinePivot parent) : base(parent)
	{
	}

	new InvoiceLinePackagePivot Parent => (InvoiceLinePackagePivot)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(Parent.Factory.Load<JobComInvoiceLine>(Parent.CHC_JI)));
	PlausiValidation plausiValidation;

	protected override void CheckCHC_NumberOfPacks()
	{
		PlausiValidation.CheckNP70020(Parent.CHC_NumberOfPacksInfo, Parent);
		PlausiValidation.CheckNP70178(Parent.CHC_NumberOfPacksInfo, Parent);
		PlausiValidation.CheckNS30021R132_NumberOfPacks(Parent.CHC_NumberOfPacksInfo, Parent);
		PlausiValidation.CheckNS30181(Parent.CHC_NumberOfPacksInfo, Parent);
	}
}
