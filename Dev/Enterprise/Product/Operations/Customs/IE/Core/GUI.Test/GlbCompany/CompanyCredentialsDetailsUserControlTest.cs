using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using GlbCompanyWrapper = Enterprise.Customs.IE.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsDetailsUserControl))]
	sealed class CompanyCredentialsDetailsUserControlTest : BasherTest
	{
		public void TestToggleCertificateLoaderUserControlStatus()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				var companyWrapper = (GlbCompanyWrapper)((ZForm)form).CurrentDataItem;
				Assert(userControl.CertificateLoaderUserControl.Enabled);
				Assert(userControl.EMCSCertificateLoaderUserControl.Enabled);
			}
		}

		public void TestCaptions()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				AssertEquals("Revenue Online Service Credentials – AES/AIS/NCTS", userControl.ROSCredentialsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Revenue Online Service Credentials – EMCS", userControl.EMCSROSCredentialsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestEMCSROSCredentialsGroupBoxVisibility()
		{
			var company1 = Company;
			company1.GC_Name = "TEST IE COMP 1";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "ZA2";
			company2.GC_Name = "TEST IE COMP 2";
			company2.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			using (EmcsCustomsDataRegistry.Instance.EnableEmcsFunctions.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (EmcsCustomsDataRegistry.Instance.EnableEmcsFunctions.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = (ZForm)GetFormToBash())
			{
				form.Show();
				form.SetDataBinding(Provider.GetWrapper(company1), "");
				AssertEquals("Should hide EMCSROSCredentialsGroupBox when the value of EnableEmcsFunctions is false.", false, userControl.EMCSROSCredentialsGroupBox.Visible);
				form.SetDataBinding(Provider.GetWrapper(company2), "");
				AssertEquals("Should show EMCSROSCredentialsGroupBox when the value of EnableEmcsFunctions is true.", true, userControl.EMCSROSCredentialsGroupBox.Visible);
			}
		}

		public void TestMailboxRequest()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupMailboxCollectURL();
			var company = Company;
			company.GC_Name = "TEST IE COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "B!2";
			branch1.GB_BranchName = $"TEST IE1 BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch1.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			branch1.GB_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "B!1";
			branch2.GB_BranchName = $"TEST IE2 BRANCH";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch2.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			branch2.GB_SystemCreateTimeUtc = branch1.GB_SystemCreateTimeUtc.AddSeconds(-1);
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			Factory.Save();
			using (var form = GetFormToBash())
			{
				form.Show();
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, companyCredential.PK);
				CombineAssertions("Login company same", () =>
				{
					using (Environment.DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						userControl.MailboxRequestButton.PerformClick();
						var newFactory = new BusinessObjectFactory();
						var interchange = newFactory.Load<EDIInterchange>(query).Single();
						AssertEquals("Should use login branch", branch1.PK, interchange.EI_GB);
						AssertEquals("Message error", $"One Mailbox Request (Interchange Num = '{interchange.EI_InterchangeNum}') created.", UnitTestUserNotification.Instance.LastMessage.Text);
						interchange.Delete();
						newFactory.Save();
					}
				});
				CombineAssertions("Login company different", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					userControl.MailboxRequestButton.PerformClick();
					var interchange = new BusinessObjectFactory().Load<EDIInterchange>(query).Single();
					AssertEquals("Should use firt active branch", branch2.PK, interchange.EI_GB);
					AssertEquals("Message error", $"One Mailbox Request (Interchange Num = '{interchange.EI_InterchangeNum}') created.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestMailboxRequestForInvalidCredential()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupMailboxCollectURL();
			var company = Company;
			company.GC_Name = "TEST IE COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B!1";
			branch.GB_BranchName = $"TEST IE1 BRANCH";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			Factory.Save();
			CombineAssertions(() =>
			{
				using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					using (var form = GetFormToBash())
					{
						form.Show();
						companyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						userControl.MailboxRequestButton.PerformClick();
						var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
						query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
						query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
						query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
						query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
						query.AddToFilter(EDIInterchangeSchema.EI_GP, companyCredential.PK);
						query.AddToFilter(EDIInterchangeSchema.EI_GB, branch.PK);
						AssertEquals("Message error", "Cannot create a Mailbox Request for invalid credential.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("No request was created", 0, Factory.Load<EDIInterchange>(query).Length);

						companyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
						companyCredential.GP_ExpiryDate = ZDateTime.Today.AddMonths(-1);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						userControl.MailboxRequestButton.PerformClick();
						AssertEquals("Message error", "Cannot create a Mailbox Request for invalid credential.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("No request was created", 0, Factory.Load<EDIInterchange>(query).Length);
					}
				}
			});
		}

		public void TestEMCSMailboxRequest()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupEMCSMailboxCollectURL();
			var company = Company;
			company.GC_Name = "TEST IE COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "B!2";
			branch1.GB_BranchName = $"TEST IE1 BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch1.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			branch1.GB_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "B!1";
			branch2.GB_BranchName = $"TEST IE2 BRANCH";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch2.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			branch2.GB_SystemCreateTimeUtc = branch1.GB_SystemCreateTimeUtc.AddSeconds(-1);
			var companyWrapper = GlbCompanyWrapper.Get(company);
			var emcsCredential = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			Factory.Save();
			using (EmcsCustomsDataRegistry.Instance.EnableEmcsFunctions.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (var form = GetFormToBash())
				{
					form.Show();
					userControl.EMCSExternalPasswordCertificateGrid.Select(0);
					var contextMenuItem = userControl.EMCSExternalPasswordCertificateGrid.ContextMenu.MenuItems.FindByText("Mailbox Request");
					var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS);
					query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
					query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
					query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
					query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
					query.AddToFilter(EDIInterchangeSchema.EI_GP, emcsCredential.PK);
					CombineAssertions("Login company same", () =>
					{
						using (Environment.DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
						{
							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							contextMenuItem.PerformClick();
							var newFactory = new BusinessObjectFactory();
							var interchange = newFactory.Load<EDIInterchange>(query).Single();
							AssertEquals("Should use login branch", branch1.PK, interchange.EI_GB);
							AssertEquals("Message error", $"One Mailbox Request (Interchange Num = '{interchange.EI_InterchangeNum}') created.", UnitTestUserNotification.Instance.LastMessage.Text);
							interchange.Delete();
							newFactory.Save();
						}
					});
					CombineAssertions("Login company different", () =>
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						contextMenuItem.PerformClick();
						var interchange = new BusinessObjectFactory().Load<EDIInterchange>(query).Single();
						AssertEquals("Should use firt active branch", branch2.PK, interchange.EI_GB);
						AssertEquals("Message error", $"One Mailbox Request (Interchange Num = '{interchange.EI_InterchangeNum}') created.", UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		public void TestEMCSMailboxRequestForInvalidCredential()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupEMCSMailboxCollectURL();
			var company = Company;
			company.GC_Name = "TEST IE COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B!1";
			branch.GB_BranchName = $"TEST IE1 BRANCH";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var companyWrapper = GlbCompanyWrapper.Get(company);
			var emcsCredential = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			emcsCredential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			Factory.Save();
			CombineAssertions(() =>
			{
				using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				using (EmcsCustomsDataRegistry.Instance.EnableEmcsFunctions.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					using (var form = GetFormToBash())
					{
						form.Show();
						userControl.EMCSExternalPasswordCertificateGrid.Select(0);
						var contextMenuItem = userControl.EMCSExternalPasswordCertificateGrid.ContextMenu.MenuItems.FindByText("Mailbox Request");
						emcsCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						contextMenuItem.PerformClick();
						var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS);
						query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
						query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
						query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
						query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
						query.AddToFilter(EDIInterchangeSchema.EI_GP, emcsCredential.PK);
						query.AddToFilter(EDIInterchangeSchema.EI_GB, branch.PK);
						AssertEquals("Message error", "Cannot create a Mailbox Request for invalid credential.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("No request was created", 0, Factory.Load<EDIInterchange>(query).Length);

						emcsCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
						emcsCredential.GP_ExpiryDate = ZDateTime.Today.AddMonths(-1);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						contextMenuItem.PerformClick();
						AssertEquals("Message error", "Cannot create a Mailbox Request for invalid credential.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("No request was created", 0, Factory.Load<EDIInterchange>(query).Length);
					}
				}
			});
		}

		public void TestEMCSExternalPasswordCertificateGrid()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				AssertSequencesEqual("Columns",
				new[] { "GP_MailBoxID", "CurrentDecryptedCertificatePassphrase", "GP_ExpiryDate", "PasswordStatus" },
				userControl.EMCSExternalPasswordCertificateGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public override Form GetFormToBash()
		{
			var form = new ZForm();
			form.CaptionRenderingEnabled = true;

			userControl = new CompanyCredentialsDetailsUserControl();
			userControl.Dock = DockStyle.Fill;

			form.Controls.Add(userControl);
			form.SetDataBinding(Provider.GetWrapper(Company), "");

			return form;
		}

		GlbCompany Company
		{
			get
			{
				if (glbCompany == null)
				{
					glbCompany = Factory.New<GlbCompany>();
					glbCompany.GC_Code = "ZAC";
				}
				return glbCompany;
			}
		}
		GlbCompany glbCompany;

		GlbCompanyWrapperProvider Provider => provider ?? (provider = (GlbCompanyWrapperProvider)GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Ireland));
		GlbCompanyWrapperProvider provider;

		CompanyCredentialsDetailsUserControl userControl;
	}
}
