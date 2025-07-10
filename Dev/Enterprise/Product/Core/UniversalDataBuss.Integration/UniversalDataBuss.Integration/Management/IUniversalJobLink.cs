using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalJobLink
	{
		ZString EnterpriseCode { get; }
		ZString ServerCode { get; }
		ZString CompanyCode { get; }
		DataContextType Context { get; }
		ZString Key { get; }
	}
}
