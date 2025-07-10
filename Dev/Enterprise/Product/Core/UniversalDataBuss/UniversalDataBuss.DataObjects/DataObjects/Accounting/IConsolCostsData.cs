using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public interface IConsolCostsData
	{
		IDataContextDataObject DataContext { get; }
		ConsolCosts ConsolCosts { get; set; }
	}
}
