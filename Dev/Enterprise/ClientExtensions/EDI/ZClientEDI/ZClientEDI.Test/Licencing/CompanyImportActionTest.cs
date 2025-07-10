using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.Module.Testing
{
	public class CompanyImportActionTest : TestCaseWithFactory
	{
		public void TestSend_Decline()
		{
			var organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			organisation.OH_FullName = "ZEBRA";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			var licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = organisation.PK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			using (var tempDir = new TempDirectory())
			{
				var companyAction = new GenerateCompanyNativeXmlEmailActionWithBrowserPathSetForTest(tempDir.DirectoryName);
				companyAction.ExportAndEmail(organisation);
				AssertEquals(null, companyAction.LastFormShownForTest);
			}
		}

		public void TestSend()
		{
			AssertSend();
		}

		public void TestSendWithGlobalChargeCodeSetup()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);
			AssertSend();
		}

		public void AssertSend()
		{
			var organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			organisation.OH_FullName = "ZEBRA";
			var licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = organisation.PK;
			licenceCompany.LC_IsGSTRegistered = true;
			var currency = Factory.NewWithValidTestData<RefCurrency>();
			licenceCompany.LC_RX_NKCurrency = currency.RX_Code;
			var db = licenceCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
			unloco.RL_Code = unloco.RL_RN_NKCountryCode + "XXX";
			Assert(Array.Exists(Country.LicenceKeyBuilderSupportedCountryCodes, x => x == unloco.RL_RN_NKCountryCode));
			organisation.OH_RL_NKClosestPort = unloco.RL_Code;
			AssertEquals(Core.Constants.CountryCodes.HongKong, organisation.CountryCode);
			licenceCompany.LicEnterprise.LE_EnterpriseCode = "ZEC";
			licenceCompany.LC_CompanyCode = "ZCC";
			GlbStaff.CurrentUser.GS_FullName = "Gandalf Grey";
			GlbStaff.CurrentUser.GS_EmailAddress = "gandalf@cargowise.com";
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var tempDir = new TempDirectory())
			{
				var companyAction = new GenerateCompanyNativeXmlEmailActionWithBrowserPathSetForTest(tempDir.DirectoryName);
				companyAction.ExportAndEmail(organisation);
				AssertEquals(typeof(EmailContactForm), companyAction.LastFormShownForTest.GetType());
				var emailContactForm = (EmailContactForm)companyAction.LastFormShownForTest;
				var emailToContactBusinessObject = emailContactForm.BusinessEntity;
				AssertEquals(true, emailToContactBusinessObject.AttachmentList[0].Code.StartsWith("ZEC"));
				AssertEquals(true, emailToContactBusinessObject.AttachmentList[0].Code.EndsWith("xml"));
				AssertEquals("Company Native XML File for ZEBRA", emailToContactBusinessObject.Subject);
				AssertEquals("Gandalf Grey", emailToContactBusinessObject.FromDisplayName);
				AssertEquals("gandalf@cargowise.com", emailToContactBusinessObject.FromEmailAddress);
				Assert(!companyAction.ExportContent.Contains("GeoLocation"));
				Assert(companyAction.ExportContent.Contains($"<Code>{organisation.MainAddress.OA_RN_NKCountryCode}</Code>"));
				companyAction.LastFormShownForTest.Dispose();
			}
		}

		public class GenerateCompanyNativeXmlEmailActionWithBrowserPathSetForTest : GenerateCompanyNativeXmlEmailAction
		{
			string content;
			readonly string path;
			public GenerateCompanyNativeXmlEmailActionWithBrowserPathSetForTest(string path)
			{
				this.path = path;
			}

			protected override DialogResult ShowDialog(ZFolderBrowserDialog browser)
			{
				return DialogResult.OK;
			}

			protected override string GetPath(ZFolderBrowserDialog browser)
			{
				return path;
			}

			public ZForm LastFormShownForTest;
			protected override void ShowForm(ZForm form)
			{
				LastFormShownForTest = form;
			}

			protected override string SendOneEmailCompany(string xmlContent, EDIOrgHeader organisation, string fullPath)
			{
				content = xmlContent;
				return base.SendOneEmailCompany(xmlContent, organisation, fullPath);
			}

			public string ExportContent
			{
				get
				{
					return content;
				}
			}
		}
	}
}
