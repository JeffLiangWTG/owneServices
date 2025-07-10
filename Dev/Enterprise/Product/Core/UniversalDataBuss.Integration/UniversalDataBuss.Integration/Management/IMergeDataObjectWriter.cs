namespace Enterprise.UniversalDataBuss.Integration
{
	using CargoWise.EntityFramework;

	public interface IMergeDataObjectWriter : ITopLevelDataObjectWriter
	{
		void MergeData(IDataObject dataObject, BusinessObject mergingBO);
	}
}
