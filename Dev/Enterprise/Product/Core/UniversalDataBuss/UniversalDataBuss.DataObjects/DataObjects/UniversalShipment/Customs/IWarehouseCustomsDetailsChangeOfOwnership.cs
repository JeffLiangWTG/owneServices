namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("To be used by Warehouse and Customs")]
	public interface IWarehouseCustomsDetailsChangeOfOwnership
	{
		OrganizationAddress OldOwner { get; }
		OrganizationAddress NewOwner { get; }
		OrganizationAddress NewWarehouse { get; }
	}
}
