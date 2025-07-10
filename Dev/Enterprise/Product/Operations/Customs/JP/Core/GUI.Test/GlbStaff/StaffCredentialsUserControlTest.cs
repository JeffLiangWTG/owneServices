using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(StaffCredentialsUserControl))]
	sealed class StaffCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
	{
		public void TestPasswordCollectionGrid()
		{
			using (var form = new ZForm(Common.GlbStaffWrapper.Get(Factory.New<GlbStaff>())))
			{
				var credentialsControl = new StaffCredentialsUserControl();
				form.Controls.Add(credentialsControl);
				form.Show();

				CombineAssertions(() =>
				{
					var grid = credentialsControl.FindSingle<ZGrid>();
					AssertEndsWith("Binding", nameof(Common.GlbStaffWrapper.PasswordCollection), grid.BindTo);
					AssertType<ZDropEditColumnStyleInfo>(Common.GlbExternalPasswordCUS.Schema.GP_PasswordType, grid.GetColumnStyle(Common.GlbExternalPasswordCUS.Schema.GP_PasswordType));
					AssertType<ZDropEditColumnStyleInfo>(Common.GlbExternalPasswordCUS.Schema.GP_Transport, grid.GetColumnStyle(Common.GlbExternalPasswordCUS.Schema.GP_Transport));
					AssertType<ZTextBoxColumnStyleInfo>(Common.GlbExternalPasswordCUS.Schema.GP_MailBoxID, grid.GetColumnStyle(Common.GlbExternalPasswordCUS.Schema.GP_MailBoxID));
					AssertType<ZTextBoxColumnStyleInfo>(Common.GlbExternalPasswordCUS.Schema.GP_UserID, grid.GetColumnStyle(Common.GlbExternalPasswordCUS.Schema.GP_UserID));
					AssertType<ZTextBoxColumnStyleInfo>(Common.GlbExternalPasswordCUS.Schema.CurrentDecryptedPassword, grid.GetColumnStyle(Common.GlbExternalPasswordCUS.Schema.CurrentDecryptedPassword));

					Assert(grid.GetColumnStyle(Common.GlbExternalPasswordCUS.Schema.GP_PasswordType).IsReadOnly);
				});
			}
		}
	}
}
