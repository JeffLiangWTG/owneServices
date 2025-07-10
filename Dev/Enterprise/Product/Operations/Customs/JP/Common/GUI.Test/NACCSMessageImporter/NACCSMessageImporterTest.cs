using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Shared.GUI.Testing;

[CountrySpecificTest(Core.Constants.CountryCodes.Japan)]
[TestedType(typeof(NACCSMessageImporter))]
sealed class NACCSMessageImporterTest : TestCaseWithFactory
{
	public void TestImportFromFile()
	{
		var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.JP.IJobDeclaration>();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_DataModel = Core.Constants.CountryCodes.Japan;
		entryHeader.CH_BGMReference = "InputRef01";

		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

		var proxyOrg = Factory.NewWithValidTestData<OrgHeader>();
		company.GC_OH_OrgProxy = proxyOrg.PK;

		var customsCode = proxyOrg.CustomsCodes.AddNew();
		customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
		customsCode.OK_CustomsRegNo = "XXXXX";

		var branch = company.Branches.AddNew();
		branch.FillWithValidTestData();
		declaration.JE_GB = branch.PK;

		var password = Factory.New<GlbExternalPasswordNMC>();
		password.GP_MailBoxID = "XXX00301";
		password.CurrentDecryptedPassword = "123";
		password.GP_GC = company.PK;
		password.ShouldReceive = true;
		password.GP_PasswordType = JPPasswordType.Codes.NMC;

		Factory.Save();

		using var context = branch.SetAsTemporaryContext();
		var form = new BaseJobDeclarationForm(declaration);
		using var file = TempFile.NewWithExtension("txt");
		form.ControllerID = ControllerIDs.Customs.JobDeclaration;

		var naccsMessageImporter = new NACCSMessageImporter(form, declaration as INACCSMessageImportSupporter);
		File.WriteAllText(file.Filename, "        *XXX   ", Encoding.GetEncoding("shift_jis"));
		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
		UnitTestUserNotification.Instance.ClearMessages();

		try
		{
			KForm.FormCreated -= OnFormCreated;
			KForm.FormCreated += OnFormCreated;

			naccsMessageImporter.ImportFromFile();
			declaration = form.Declaration;
			entryHeader = form.Declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			naccsMessageImporter = new NACCSMessageImporter(form, declaration as INACCSMessageImportSupporter);
			AssertEquals("No message is created for invalid message", 0, entryHeader.Messages.Count);
			Assert("Error for invalid message", UnitTestUserNotification.Instance.LastMessage.WasError);

			var messageText = @"MSPIDC  SAT0471202306241106  XXXXX                 XXX00301@MAIL.TEST.NACCS6                                       JPtest78901                                                                                   2439973568IDA00000000000800000000083001EP   InputRef00                                                                                                    C2           G3              008565\r\n地方消費税";
			File.WriteAllText(file.Filename, messageText, Encoding.GetEncoding("shift_jis"));
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
			UnitTestUserNotification.Instance.ClearMessages();

			naccsMessageImporter.ImportFromFile();
			declaration = form.Declaration;
			entryHeader = form.Declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			naccsMessageImporter = new NACCSMessageImporter(form, declaration as INACCSMessageImportSupporter);
			AssertEquals("No message is created when not related to this header.", 0, entryHeader.Messages.Count);
			Assert("Error for wrong message", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(
				"This message cannot be imported. It does not match this job by message reference, input reference nor subject.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			messageText = messageText.Replace("InputRef00", "InputRef01");
			File.WriteAllText(file.Filename, messageText, Encoding.GetEncoding("shift_jis"));
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			naccsMessageImporter.ImportFromFile();
			declaration = form.Declaration;
			entryHeader = form.Declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			naccsMessageImporter = new NACCSMessageImporter(form, declaration as INACCSMessageImportSupporter);
			var message = entryHeader.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Receive);
			AssertContains("Do you want to reload this form now to reflect the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Message is successfully import because input reference is matched.", 1, entryHeader.Messages.Count);
			AssertEquals("Message's branch is updated", entryHeader.Branch.PK, message.EM_GB);
			AssertEquals("Message is updated as processed", EDIMessage.Status.ProcessedOK, message.EM_Status);
			Assert("Message's encoding should have been converted", message.EM_FormattedMessageText.EndsWith("地方消費税"));
		}
		finally
		{
			form?.Close();
			form?.Dispose();
			KForm.FormCreated -= OnFormCreated;
		}

		void OnFormCreated(object sender, EventArgs e)
		{
			form?.Dispose();

			if (sender is BaseJobDeclarationForm declarationForm)
			{
				form = declarationForm;
			}
		}
	}

	public void TestImportFromFile_AdditionalWarning()
	{
		var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.JPManifest.IAsycudaManifestHeader>();
		var exportedMessage = header.Factory.New<EDIMessage>();
		exportedMessage.EM_LinkedObject = header;
		exportedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
		exportedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		exportedMessage.EM_ApplicationReference = EDIMessage.FlatFile;
		exportedMessage.EM_GB = GlbBranch.CurrentBranch.PK;
		exportedMessage.EM_MessageNum = "HCH01000000000800000000084";
		Factory.Save();

		var form = new ManifestForm(header);
		form.Show();
		using (JPRegistry.Instance.EnableHCHForwarderManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			using var file = TempFile.NewWithExtension("txt");
			form.ControllerID = ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest;

			var naccsMessageImporter = new NACCSMessageImporter(form, header as INACCSMessageImportSupporter);
			var messageText = @"   HCH01SAS0721202306241106  XXXXX                 XXX00301@MAIL.PROD.NACCS6                                       12345678901                                                                                   2439973568HCH01000000000800000000084001EP   InputRef01                                                                                                    C2           G3              008565";
			File.WriteAllText(file.Filename, messageText, Encoding.GetEncoding("shift_jis"));
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			try
			{
				KForm.FormCreated -= OnFormCreated;
				KForm.FormCreated += OnFormCreated;
				naccsMessageImporter.ImportFromFile();

				Assert("Warning for messages without any matched bill", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("The imported message contains no bills."));
				AssertEquals("Message should be allowed to be imported even without any bill matched.", 2, form.BusinessEntity.Messages.Count);
			}
			finally
			{
				form?.Close();
				form?.Dispose();
				KForm.FormCreated -= OnFormCreated;
			}
		}

		void OnFormCreated(object sender, EventArgs e)
		{
			form?.Dispose();

			if (sender is ManifestForm manifestForm)
			{
				form = manifestForm;
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var configType = helper.CreateOrGetExistingRefSysConfigType("NACCSMailT", "NACCS Mail Test", "NACCS Mail Test");
		helper.CreateOrUpdateExistingRefSysConfig(configType.ZRT_ConfigCode, "NACCS@Mail.Test.NACCS6", ZDateTime.Now.AddYears(-1), ZDateTime.Now.AddYears(1));
	}
}
