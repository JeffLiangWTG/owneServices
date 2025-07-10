namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalScheduleImporter
	{
		MessageStatus ImportUniversalSchedule(ITopLevelDataObject dataObject);
	}
}
