using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public class AuthorizationProvider : IAuthorization
{
	public AuthorizationProvider(EU.Business.CusAuthorizationUsage authorisationUsage, int sequenceNumber)
	{
		this.authorisationUsage = Argument.NotNull(authorisationUsage, nameof(authorisationUsage));
		this.sequenceNumber = sequenceNumber;
	}

	readonly EU.Business.CusAuthorizationUsage authorisationUsage;
	readonly int sequenceNumber;

	public string SequenceNumber => sequenceNumber.ToString();

	public string Type => authorisationUsage.AGC_Code;

	public string ReferenceNumber => authorisationUsage.AGC_Number;

	public string HolderOfAuthorisation => authorisationUsage.Owner.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
}
