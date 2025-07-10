using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIGlbStaffValidation : GlbStaffValidationReal
	{
		public EDIGlbStaffValidation(EDIGlbStaff parent)
			: base(parent)
		{
		}

		public new EDIGlbStaff Parent
		{
			get { return base.Parent as EDIGlbStaff; }
		}

		public override void ValidateAll()
		{
			ValidateCalendarEmailAddress();
			base.ValidateAll();
		}

		public void ValidateCalendarEmailAddress()
		{
			Parent.ClearRowNotifications();
			if (!Parent.CalendarEmailAddress.IsEmpty && !EmailAddressValidation.IsEmailAddressValid(Parent.CalendarEmailAddress))
			{
				Parent.AddRowError(string.Format(CultureInfo.CurrentCulture, "Staff calendar email {0} is not a valid email address. Please check on Notes tab.", Parent.CalendarEmailAddress));
			}
		}

		protected override void CheckGS_GE_HomeDepartment()
		{
		}

		protected override void CheckGS_GB_HomeBranch()
		{
		}
	}
}
