using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(SingleMessageSendingForm))]
	public class SingleMessageSendingFormTest : ZFormBasherTest
	{
		public void TestControls()
		{
			using (var form = (SingleMessageSendingForm)GetFormToBashCore())
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertType<ZGroupBox>("CredentialGroupBox type should be", form.CredentialGroupBox);
					AssertType<ZCodeFindBox>("BrokerCodeFindBox type should be", form.BrokerCodeFindBox);
				});
			}
		}

		public void TestSendActivationCheckIsOKToSendXtTest()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERPERMIT";
			staff.GS_Code = "PRT";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			Factory.Save();

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var goodsCatalogSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			using (var form = new SingleMessageSendingForm(goodsCatalogSendingObject))
			{
				form.Show();

				var sendingMessage = form.BusinessEntity as GoodsCatalogMessageSendingObject;
				sendingMessage.BrokerCode = staff.GS_Code;
				var sendButton = form.FindSingle<ZButton>("SendButton");

				sendButton.PerformClick();

				AssertEquals("The selected messages will be sent to a test environment!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendActivationCheckIsOKToSendXtProduction()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERPERMIT";
			staff.GS_Code = "PRT";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			Factory.Save();

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var goodsCatalogSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			using (var form = new SingleMessageSendingForm(goodsCatalogSendingObject))
			{
				form.Show();

				var sendingMessage = form.BusinessEntity as GoodsCatalogMessageSendingObject;
				sendingMessage.BrokerCode = staff.GS_Code;
				var sendButton = form.FindSingle<ZButton>("SendButton");

				sendButton.PerformClick();

				Assert(!UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Contains("The selected messages will be sent to a test environment!"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var sendingObjectParent = new GoodsCatalogMessageSendingObject(goodsCatalog);
			return new SingleMessageSendingForm(sendingObjectParent);
		}
	}
}

