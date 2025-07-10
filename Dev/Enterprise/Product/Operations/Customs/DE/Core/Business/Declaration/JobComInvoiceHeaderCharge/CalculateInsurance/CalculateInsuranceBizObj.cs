using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration;

public class CalculateInsuranceBizObj : EU.Business.Declaration.CalculateInsuranceBizObj
{
	public CalculateInsuranceBizObj(EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory, EU.Business.Declaration.JobComInvoiceHeader invoice) : base(euIncoTermAndChargeFactory, invoice)
	{
	}

	public override bool IsDutiablePercentEnabled => false;

	[BusinessObjectTestExclude]
	public override ZDecimal DutiablePercent
	{
		get => base.DutiablePercent;
		set => base.DutiablePercent = value;
	}
}
