using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class TemporaryStorageDutyAndTax : AsycudaTax
{
	public TemporaryStorageDutyAndTax(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ResourceStringData("Enterprise.Customs.EU.Business.TemporaryStorageDutyAndTax|AET_ChargeType", Caption = "Type", FullDescription = "Type of duty or tax.")]
	[ReadOnly(true)]
	public override ZString AET_ChargeType { get => base.AET_ChargeType; set => base.AET_ChargeType = value; }

	[ResourceStringData("Enterprise.Customs.EU.Business.TemporaryStorageDutyAndTax|AET_MethodOfCalculation", Caption = "Method", MediumCaption = "Calculation method", FullDescription = "Method of calculation.")]
	[ReadOnly(true)]
	public override ZString AET_MethodOfCalculation { get => base.AET_MethodOfCalculation; set => base.AET_MethodOfCalculation = value; }

	[ResourceStringData("Enterprise.Customs.EU.Business.TemporaryStorageDutyAndTax|AET_BaseValue", Caption = "Base", MediumCaption = "Base value", FullDescription = "Base value for calculation.")]
	[ReadOnly(true)]
	public override ZDecimal AET_BaseValue { get => base.AET_BaseValue; set => base.AET_BaseValue = value; }

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.EU.Business.TemporaryStorageDutyAndTax|AET_Rate", Caption = "Rate", FullDescription = "Rate for calculation.")]
	public override ZDecimal AET_Rate { get => base.AET_Rate; set => base.AET_Rate = value; }

	[ResourceStringData("Enterprise.Customs.EU.Business.TemporaryStorageDutyAndTax|AET_ChargeAmount", Caption = "Amount", FullDescription = "Calculated amount.")]
	[ReadOnly(true)]
	public override ZDecimal AET_ChargeAmount { get => base.AET_ChargeAmount; set => base.AET_ChargeAmount = value; }
}
