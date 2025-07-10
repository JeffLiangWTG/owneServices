namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// This will be part of ZGrid.Key for grid layout user settings matching against StmData.SD_Name.
	/// 
	/// ie. Customs.GUI.BaseCustomsBrokerageUserControl returns a current company's country code to have separate settins persisted for each country
	/// </summary>
	public interface IDataGridLayoutIdentifierRoot
	{
		string ID { get; }
	}
}
