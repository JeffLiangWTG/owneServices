using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EdiGlbStaffForm : GlbStaffForm
	{
		public EdiGlbStaffForm() { }

		public EdiGlbStaffForm(EDIGlbStaff staff)
			: base(staff)
		{
			if (staff.IsCurrentUser || EDISecurityCheckpoints.SendGitHubInviteForOthers.IsAllowed)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("874C858C-223B-43C4-B5AF-B39EB1BA0E25", "Send GitHub Invite"), OnSendGitHubInvite);
			}
		}

		void OnSendGitHubInvite(object sender, EventArgs e)
		{
			((EDIGlbStaff)DataSource)?.SendGitHubInvite();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var oldStaff = (EDIGlbStaff)DataSource;
			if (oldStaff != null)
			{
				oldStaff.GS_FullNameInfo.ValueChanged -= new EventHandler(GS_FullName_ValueChanged);
			}
			base.SetDataBinding(dataSource, dataMember);
			var newStaff = (EDIGlbStaff)DataSource;
			if (newStaff != null)
			{
				newStaff.GS_FullNameInfo.ValueChanged += new EventHandler(GS_FullName_ValueChanged);
			}
		}

		void GS_FullName_ValueChanged(object sender, EventArgs e)
		{
			var staff = (EDIGlbStaff)DataSource;
			if (staff != null)
			{
				var staffEx = staff.StaffEx;
				if (staffEx.GS9_FirstName.IsEmpty &&
					staffEx.GS9_MiddleName.IsEmpty &&
					staffEx.GS9_LastName.IsEmpty &&
					staffEx.GS9_DomesticName.IsEmpty)
				{
					staffEx.PopulateNamesFromStaffFullName();
				}
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			var nameGroup = EmployeeDetailsGroupBox;

			//remove the "Friendly Name"
			nameGroup.Controls.Remove(GS_FriendlyNameBoundText);

			ControlDpiScalingHelper.SetHeight(nameGroup, nameGroup.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(heightOfFriendlyNameFields), false);

			// move all fields in name group after "Full Name" up
			var fullNameControl = GS_FullNameBoundTextBox;
			int freeSpaceTop = nameGroup.Height;
			foreach (Control control in nameGroup.Controls)
			{
				if (control.Top > fullNameControl.Bottom)
				{
					if (freeSpaceTop > control.Top)
					{
						freeSpaceTop = control.Top;
					}
					ControlDpiScalingHelper.SetTop(control, control.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(heightOfFriendlyNameFields), false);
					control.TabIndex += 5;
				}
			}

			// move home branch group box up
			ControlDpiScalingHelper.SetTop(PrivacyGroupBox, PrivacyGroupBox.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(heightOfFriendlyNameFields), false);
			ControlDpiScalingHelper.SetTop(HomeBranchDepartmentGroupBox, HomeBranchDepartmentGroupBox.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(heightOfFriendlyNameFields), false);

			//ReadOnly by default
			GS_TitleTextBox.Enabled = false;
			HomeDepartmentFindBox.Enabled = false;
			HomeBranchGuidFindBox.Enabled = false;
		}

		protected override bool ShouldHidePasswordControls
		{
			get
			{
				if (!IsOIDCEnabled)
				{
					return false;
				}

				var dataSource = (EDIGlbStaff)DataSource;
				if (dataSource != null && dataSource.GS_IsController)
				{
					return true;
				}

				var groupPkGuid = EDIDataRegistry.Instance.PasswordAndSignatureAlwaysVisibleToGroup.Value;
				if (groupPkGuid != Guid.Empty)
				{
					var staff = GlbStaff.CurrentUser;
					return staff == null || staff.Groups.Cast<GlbGroup>().All(x => x.PK != groupPkGuid);
				}

				return true;
			}
		}

		const int heightOfFriendlyNameFields = 23;
	}
}
