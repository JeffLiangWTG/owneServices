using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ITopLevelDataObjectWriter
	{
		ITopLevelDataObject GetDataObject(BusinessObject sourceBO);
		DataContextType TopLevelDataContextType { get; }
		ZString EDIMessageSubType { get; }
		ZString RootElementName { get; }
	}
}
