using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class PartyContactPersonGlbStaffProvider : IAESPartyContactPerson
	{
		readonly GlbStaff staff;

		public static PartyContactPersonGlbStaffProvider NewOrNull(GlbStaff staff) => staff == null ? null : new PartyContactPersonGlbStaffProvider(staff);

		PartyContactPersonGlbStaffProvider(GlbStaff staff)
		{
			this.staff = Argument.NotNull(staff, nameof(staff));
		}

		public string Position => staff.GS_Title;

		public string PersonName => staff.GS_FullName;

		public string PhoneNumber => staff.GS_WorkPhone;

		public string FacsimileNumber => staff.GS_FaxNum;

		public string MailAddress => staff.GS_EmailAddress;
	}
}
