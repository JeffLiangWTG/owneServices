using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	/// <summary>
	/// Used for matching via the DataSource or DataTarget Keys and their other content.
	/// </summary>
	public interface IDataContextMatchingKey
	{
		ZString Key { get; }
		ZString OwnerOrganisationCode { get; }
		ZString CompanyCode { get; }
		ITopLevelDataObject DataObject { get; }  //For Freight testing only can be removed after
	}
}
