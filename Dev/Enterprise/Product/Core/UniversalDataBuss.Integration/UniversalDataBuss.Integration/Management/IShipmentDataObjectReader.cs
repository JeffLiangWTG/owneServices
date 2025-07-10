namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IShipmentDataObjectReader : ITopLevelDataObjectReader
	{
		IShipmentDataObjectReader ParentReader { get; }
		ITopLevelDataObject DataObject { get; }
	}
}
