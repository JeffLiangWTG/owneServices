using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class CusEngine : Customs.Business.CusEngine
{
	public CusEngine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusEngine.Schema
	{
		public const int CEG_CapacityCCPrecision = 6;
		public const int CEG_CapacityCCScale = 2;
	}

	protected override Customs.Business.CusEngineValidation GetNewValidation() => new CusEngineValidation(this);

	[ResourceStringData("Enterprise.Customs.AE.Business.CusEngine|CEG_EngineNumber", Caption = "Engine Number", ShortCaption = "Engine No.")]
	public override ZString CEG_EngineNumber { get => base.CEG_EngineNumber; set => base.CEG_EngineNumber = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.CusEngine|CEG_CapacityCC", Caption = "Engine Capacity (L)", MediumCaption = "Engine Capacity", ShortCaption = "Engine Cap.")]
	public override ZDecimal CEG_CapacityCC { get => base.CEG_CapacityCC; set => base.CEG_CapacityCC = value; }
}
