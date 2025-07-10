using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public class StaffPerson : ITContactPerson
{
	public StaffPerson(GlbStaff staff)
	{
		Argument.NotNull(staff, GlbStaff.Schema.TableName);
		this.staff = staff;
	}

	readonly GlbStaff staff;

	public ZString ContactPersonCommunicationNumber { get => staff.GS_WorkPhone; }
	public ZString ContactPersonEmail { get => staff.GS_EmailAddress; }
	public ZString ContactPersonFaxNumber { get => staff.GS_FaxNum_Formatted; }
	public ZString ContactPersonName { get => staff.GS_FullName; }
}
