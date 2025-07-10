using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BE.Business;

public class Operator : ITOperator
{
	public Operator(IDocAddress address)
	{
		this.address = address;
		OperatorAddress = new Address(address);
	}

	protected IDocAddress address;

	public ZString Country => (address.Organisation as OrgHeader)?.GetEuIdentificationNumberComponents().CountryCode ?? ZString.Empty;
	public ZString Identifier => "005";
	public ZString OperatorIdentity => (address.Organisation as OrgHeader)?.GetEuIdentificationNumberComponents().RegistrationNumber ?? ZString.Empty;
	public virtual ITContactPerson ContactPerson { get; }
	public virtual ITAddress OperatorAddress { get; set; }
	public virtual ZString OperatorName { get => address.Organisation.OH_FullName; }
}
