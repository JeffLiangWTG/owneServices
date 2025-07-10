using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5GWEntry : IExtendedOfficeHoursEntry
	{
		ZString ReferenceNumberType { get; }
		ZString HSDescription { get; }
		ZString BondedAreaCode { get; }
		ZString PayerCompanyName { get; }
	}
}
