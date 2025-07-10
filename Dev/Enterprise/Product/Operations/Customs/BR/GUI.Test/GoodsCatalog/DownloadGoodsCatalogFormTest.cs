using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(DownloadGoodsCatalogForm))]
	public class DownloadGoodsCatalogFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DownloadGoodsCatalogForm(new GoodsCatalogDownloadObject(Factory));
		}

		public void TestDownload()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERDOWNLOADCATALOG";
			staff.GS_Code = "DOW";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "XVBQP68S";
			orgHeader.OH_IsConsignor = true;
			orgHeader.PrimaryRegistrationNumber.Number = "58500398";
			orgHeader.OH_FullName = "TEST COMPANY";
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "27094734", Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			var downloadObject = new GoodsCatalogDownloadObject(Factory) { OwnerCode = "", DownloadDeactivated = true, BrokerCode = "" };

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			using (var form = new DownloadGoodsCatalogForm(downloadObject))
			{
				form.Show();
				form.DownloadButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				downloadObject.OwnerCode = orgHeader.OH_Code;
				form.DownloadButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				downloadObject.BrokerCode = "DOW";
				form.DownloadButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				downloadObject.DownloadCatalog = true;
				downloadObject.DownloadForeignOperator = true;
				form.DownloadButton.PerformClick();
				AssertEquals("The selected messages will be sent to a test environment!", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.DownloadButton.PerformClick();
				AssertEquals("3 Download message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.CAT));

				AssertEquals(3, messages.Length);
				AssertContainsExactElementsInAnyOrder(new[] { EDIMessageSubTypeList.Codes.CatalogZipFile, EDIMessageSubTypeList.Codes.ManufacturerZipFile, EDIMessageSubTypeList.Codes.OperatorZipFile },
					messages.Select(x => x.EM_MessageSubType));
			}
		}

		public void TestCheckIsOKToSendXtProduction()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERDOWNLOADCATALOG";
			staff.GS_Code = "DOW";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "XVBQP68S";
			orgHeader.OH_FullName = "TEST COMPANY";
			orgHeader.OH_IsConsignor = true;
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "58.500.398/-04", Core.Constants.CountryCodes.Brazil);
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "58500398", Core.Constants.CountryCodes.Brazil);

			var downloadObject = new GoodsCatalogDownloadObject(Factory);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			using (var form = new DownloadGoodsCatalogForm(downloadObject))
			{
				form.Show();
				downloadObject.OwnerCode = orgHeader.OH_Code;
				downloadObject.BrokerCode = staff.GS_Code;
				downloadObject.DownloadCatalog = true;
				form.DownloadButton.PerformClick();

				Assert(!UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Contains("The selected messages will be sent to a test environment!"));
			}
		}

		public void TestDownload_CanSendMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERDOWNLOADCATALOG";
			staff.GS_Code = "DOW";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "XVBQP68S";
			orgHeader.OH_FullName = "TEST COMPANY";
			orgHeader.OH_IsConsignor = true;
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "58.500.398/-04", Core.Constants.CountryCodes.Brazil);
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "58500398", Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			{
				ShowDownloadGoodsCatalogFormAndClickDownload();
				AssertEquals("3 Download message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				ShowDownloadGoodsCatalogFormAndClickDownload();
				AssertEquals("A second download request is not allowed for this Owner when the first one was not replied.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			void ShowDownloadGoodsCatalogFormAndClickDownload()
			{
				var downloadObject = new GoodsCatalogDownloadObject(new BusinessObjectFactory());
				using var form = new DownloadGoodsCatalogForm(downloadObject);
				form.Show();
				downloadObject.OwnerCode = orgHeader.OH_Code;
				downloadObject.BrokerCode = staff.GS_Code;
				downloadObject.DownloadCatalog = true;
				downloadObject.DownloadForeignOperator = true;

				form.DownloadButton.PerformClick();
			}
		}
	}
}
