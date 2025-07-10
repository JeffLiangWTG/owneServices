using CargoWise.Types;

namespace Enterprise.Customs.CA.Business;

public class PGAEmptyContactDetails : IPGAContactDetails
{
	public ZString EmailAddress => ZString.Empty;
	public ZString PhoneNumber => ZString.Empty;
	public ZString ContactName => ZString.Empty;
}
