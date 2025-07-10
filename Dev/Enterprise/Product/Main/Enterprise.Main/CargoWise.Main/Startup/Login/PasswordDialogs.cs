using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;

namespace CargoWise.Main.Startup.Login;

interface IPasswordDialogs
{
	bool ShowChangePasswordDialog(GlbStaff staff);
	bool ShowResetPasswordDialog(GlbStaff staff);
}

class CWNextPasswordDialogs : IPasswordDialogs
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used for Winzor")]
	readonly IPasswordDialogs fallback;

	internal CWNextPasswordDialogs(IPasswordDialogs fallback)
	{
		this.fallback = fallback;
	}

	public bool ShowChangePasswordDialog(GlbStaff staff)
	{
#if !WINZOR
		var owner = Enterprise.Startup.StartupOpenMainFormTask.MainFormInstance.Handle;
		var window = Enterprise.MasterFiles.GUI.ChangePassword.ChangeDialog(staff, owner);
		return window.ShowDialog() ?? false;
#else
		return fallback.ShowChangePasswordDialog(staff);
#endif
	}

	public bool ShowResetPasswordDialog(GlbStaff staff)
	{
#if !WINZOR
		var owner = Enterprise.Startup.StartupOpenMainFormTask.MainFormInstance.Handle;
		var window = Enterprise.MasterFiles.GUI.ChangePassword.ResetDialog(staff, owner);
		return window.ShowDialog() ?? false;
#else
		return fallback.ShowResetPasswordDialog(staff);
#endif
	}
}

class PasswordDialogs : IPasswordDialogs
{
	public static IPasswordDialogs Instance
	{
		get
		{
			if (CWNextFeatureHelper.IsCWNextEnabled())
			{
				return new CWNextPasswordDialogs(new PasswordDialogs());
			}
			return new PasswordDialogs();
		}
	}

	public bool ShowChangePasswordDialog(GlbStaff staff)
	{
		return ChangePasswordDialog.ChangePassword(staff) == DialogResult.OK;
	}

	public bool ShowResetPasswordDialog(GlbStaff staff)
	{
		return ChangePasswordDialog.ResetPassword(staff) == DialogResult.OK;
	}
}
