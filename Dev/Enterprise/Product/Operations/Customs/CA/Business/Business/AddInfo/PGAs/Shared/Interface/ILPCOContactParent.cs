using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface ILPCOContactParent
	{
		IPGAContactDetails GetContactDetails(ZString contactType);
	}

	public interface IPGAContactDetails
	{
		ZString ContactName { get; }
		ZString EmailAddress { get; }
		ZString PhoneNumber { get; }
	}
}
