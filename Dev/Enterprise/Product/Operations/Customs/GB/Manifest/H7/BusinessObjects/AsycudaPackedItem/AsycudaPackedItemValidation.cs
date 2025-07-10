using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.H7.Business;

public class AsycudaPackedItemValidation : EU.H7.Business.AsycudaPackedItemValidation
{
	public AsycudaPackedItemValidation(ASYCUDA.Business.AsycudaPackedItem parent) : base(parent)
	{
	}

	protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

	protected override void CheckAPI_GrossWeight()
	{
		base.CheckAPI_GrossWeight();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GrossWeightInfo);
	}

	protected override void CheckAPI_NetWeight()
	{
		base.CheckAPI_NetWeight();
		var zNetWeight = new ZWeight(Parent.API_NetWeight, Parent.API_NetWeightUQ);
		var zGrossWeight = new ZWeight(Parent.API_GrossWeight, Parent.API_GrossWeightUQ);
		if (zGrossWeight.IsValid && zNetWeight.IsValid && zGrossWeight < zNetWeight)
		{
			Parent.API_NetWeightInfo.AddMessageError(Res.GetString("e7744aa9-57dd-4a4d-8a05-53d5e68f4050", "Net mass should be less than gross mass."));
		}
	}

	protected override void CheckAPI_Tariff()
	{
		base.CheckAPI_Tariff();
		ListValidation.MessageErrorIfInvalidCode(Parent.API_TariffInfo);
	}

	protected override bool CheckTotalIntrinsicValueOfGoodsIsExceed(out string messageOfValueExceed)
	{
		var bill = Parent.Bill;
		if (bill.AdditionalProcedureCodes.Cast<EU.Business.AdditionalProcedureCode>().Any(x => x.CY_Code.Contains("4000C07")) && bill.SumOfGoodsValue > 135m)
		{
			messageOfValueExceed = Res.GetString("252d2dfb-fb89-44fd-b207-e8799d02cf08", "Sum of Intrinsic Value (Items) must not exceed GBP 135 when Add. Procedure(s) contains 4000C07.");
			return true;
		}
		else if (bill.AdditionalProcedureCodes.Count == 1 && bill.AdditionalProcedureCodes.ContainsCode("4000C08") && bill.SumOfGoodsValue > 39m)
		{
			messageOfValueExceed = Res.GetString("dc0c3655-dfd5-4006-9a11-41b021b2e4aa", "Sum of Intrinsic Value (Items) must not exceed GBP 39 when Add. Procedure(s) is 4000C08.");
			return true;
		}

		messageOfValueExceed = string.Empty;
		return false;
	}
}
