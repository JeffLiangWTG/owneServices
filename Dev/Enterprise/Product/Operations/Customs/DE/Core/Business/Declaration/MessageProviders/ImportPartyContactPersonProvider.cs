using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class ImportPartyContactPersonProvider : IImportPartyContactPerson
	{
		public static ImportPartyContactPersonProvider NewOrNull(GlbStaff staff) => staff == null ? null : new ImportPartyContactPersonProvider(staff);

		ImportPartyContactPersonProvider(GlbStaff staff)
		{
			this.staff = staff;
		}
		readonly GlbStaff staff;

		public string Position => staff.GS_Title.LeftOrNull(35);

		public string PersonName => staff.GS_FullName;

		public string PhoneNumber => staff.GS_WorkPhone;

		public string MailAddress => staff.GS_EmailAddress;
	}
}
