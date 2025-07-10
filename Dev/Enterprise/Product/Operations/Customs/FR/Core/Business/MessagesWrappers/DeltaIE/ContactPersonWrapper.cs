using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class ContactPersonWrapper : IContactPerson
	{
		ContactPersonWrapper(GlbStaff glbStaff, GlbBranch glbBranch)
		{
			this.glbStaff = Argument.NotNull(glbStaff, nameof(glbStaff));
			this.glbBranch = Argument.NotNull(glbBranch, nameof(glbBranch));
		}

		readonly GlbStaff glbStaff;
		readonly GlbBranch glbBranch;

		public string EMailAddress => IsStaffEmailOrPhoneNumberEmpty ? glbBranch.GB_Email : staffEmail;

		public string Name => glbStaff.GS_FullName;

		public string PhoneNumber => IsStaffEmailOrPhoneNumberEmpty ? glbBranch.GB_Phone_Wrapper.FormattedForBinding : glbStaff.GS_WorkPhone;

		bool IsStaffEmailOrPhoneNumberEmpty => glbStaff.GS_WorkPhone.IsEmpty || staffEmail.IsEmpty;

		ZString staffEmail => glbStaff.EmailAddresses.FirstOrDefault(e => !e.GSE_EmailAddress.IsEmpty)?.GSE_EmailAddress ?? ZString.Empty;

		public static ContactPersonWrapper New(GlbStaff glbStaff, GlbBranch glbBranch) => (glbStaff is null || glbBranch is null) ? null : new ContactPersonWrapper(glbStaff, glbBranch);
	}
}
