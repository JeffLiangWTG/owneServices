using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.GUI.Testing
{
	internal static class TestExtensions
	{
		public static IDisposable SetTemporaryCurrentUser(this BusinessObjectFactory factory, string title = "", string fullName = "", string workPhone = "", string code = "")
		{
			var staff = factory.New<GlbStaff>();
			staff.GS_Title = title;
			staff.GS_FullName = fullName;
			staff.GS_WorkPhone = workPhone;
			staff.GS_Code = code;
			return Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
		}
	}
}
