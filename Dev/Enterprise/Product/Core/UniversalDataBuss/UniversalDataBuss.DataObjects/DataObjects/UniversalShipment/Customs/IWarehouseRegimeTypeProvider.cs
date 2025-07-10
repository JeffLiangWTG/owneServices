namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("To be used by Warehouse and Customs for Imports into or outof InwardsProcessing")]
	public interface IWarehouseRegimeTypeProvider
	{
		CustomsRegime GetCustomsRegime();
	}
}
