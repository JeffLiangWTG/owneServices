using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Licensing.GUI.Test
{
	[TestedType(typeof(LicenseAgreementUserControl))]
	public class LicenseAgreementUserControlTest : TestCaseWithFactory
	{
		ZForm GetFormToBashCore()
		{
			var form = new ZForm();
			form.Controls.Add(new LicenseAgreementUserControl(new System.Threading.Tasks.TaskCompletionSource<bool>()));
			return form;
		}

		IDisposable WithIp(string ip)
		{
			var mock = new Mock<IWiseCloudSecurityClient>();
			mock.Setup(x => x.GetClientIPAddress(It.IsAny<string>(), It.IsAny<string>())).Returns(ip);
			return ObjectFactory.Substitute(mock.Object);
		}

		[TestDate(2024, 7, 4)]
		public void TestAcceptance_SetsFields()
		{
			var agreement = Factory.New<LicenseAgreement>();
			agreement.LAG_Title = "Who did it?";
			agreement.LAG_Type = LicenseAgreementTypeList.Codes.CargoWiseNext;
			agreement.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
			var ip = "10.0.0.1";
			using (WithIp(ip))
			using (var form = GetFormToBashCore())
			{
				form.SetDataBinding(agreement, "");
				form.Show();
				form.FindAll<ZCheckBox>().Single().Checked = true;
				form.FindAll<ZButton>().Single(n => n.Name.StartsWith("accept")).PerformClick();
				Application.DoEvents();

				AssertEquals(ZDateTime.UtcNow, agreement.LAG_AcceptedTimeUtc);
				AssertEquals(GlbStaff.CurrentUser.GS_FullName, agreement.LAG_AcceptedByName);
				AssertEquals(GlbStaff.CurrentUser.GS_EmailAddress, agreement.LAG_AcceptedByEmail);
				AssertEquals(ip, agreement.LAG_AcceptedIPAddress);
				AssertEquals(LicenseAgreementStatusList.Codes.Queued, agreement.LAG_Status);
			}
		}

		[TestDate(2024, 7, 4)]
		public void TestAcceptance_Unchecked_Prompt()
		{
			var agreement = Factory.New<LicenseAgreement>();
			agreement.LAG_Title = "I did it!";
			agreement.LAG_Type = LicenseAgreementTypeList.Codes.CargoWiseNext;
			agreement.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
			using (var form = GetFormToBashCore())
			{
				form.SetDataBinding(agreement, "");
				form.Show();
				form.FindAll<ZCheckBox>().Single().Checked = false;
				var messages = UnitTestUserNotification.Instance.PreviousMessages.Length;
				form.FindAll<ZButton>().Single(n => n.Name.StartsWith("accept")).PerformClick();
				Application.DoEvents();
				AssertEquals("Prompting.", messages + 1, UnitTestUserNotification.Instance.PreviousMessages.Length);
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, agreement.LAG_Status);
			}
		}
	}
}
