using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class StaffContactPersonDataProvider : IContactPerson
{
	public static StaffContactPersonDataProvider New(GlbStaff user) => user != null ? new StaffContactPersonDataProvider(user) : null;

	StaffContactPersonDataProvider(GlbStaff user)
	{
		this.user = user;
	}
	readonly GlbStaff user;

	public string Name => user.GS_FullName;

	public string PhoneNumber => user.GS_WorkPhone.ReturnNullIfEmpty();

	public string EmailAddress => user.GS_EmailAddress;
}
