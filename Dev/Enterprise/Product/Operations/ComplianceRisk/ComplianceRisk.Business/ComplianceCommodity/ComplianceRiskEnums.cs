namespace Enterprise.ComplianceRisk.Business
{
	public enum CommodityType
	{
		Unknown = 0,
		FetchDataEntry,
		UserDataEntry,
		RelatedJobLink
	}

	public enum ComplianceRiskTypes
	{
		Parties,
		Locations,
		Assessment,
		Commodities
	}
}
