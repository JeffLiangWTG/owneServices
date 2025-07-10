namespace Enterprise.ComplianceRisk.Integration
{
	public enum CommodityRiskCalculateFactor
	{
		Import = 1,
		Export = 1 << 1,
		All = Import | Export
	}
}
