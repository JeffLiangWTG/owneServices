namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IActivityDataContextManager : IEventDataContextManager
	{
		bool ManagesActivities { get; }
		bool DoesManageDataContextType(DataContextType type);
		ITopLevelDataObjectWriter GetActivityDataObjectWriter(IDataWritingManager writeManager, bool shouldIncludeRelatedItems);
		ITopLevelDataObjectReader GetActivityDataObjectReader(ITopLevelDataObject activity, IXmlImportLogger logger, IUniversalObjectFactory factory);
	}
}
