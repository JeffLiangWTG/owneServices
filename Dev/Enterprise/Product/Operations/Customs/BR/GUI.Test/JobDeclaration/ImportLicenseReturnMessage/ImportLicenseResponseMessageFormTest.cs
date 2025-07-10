using System.IO;
using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIInterchange;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ImportLicenseResponseMessageForm))]
	public class ImportLicenseResponseMessageFormTest : ZFormBasherTest
	{
		public void TestCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			IImportLicenseResponseObjectParent importLicenseParent = new ImportLicenseAcceptResponseObjectParent(declaration);
			using (var form = new ImportLicenseResponseMessageForm(importLicenseParent))
			{
				AssertEquals("Caption should be", "Load Response from Customs", form.FormCaption);
			}

			importLicenseParent = new ImportLicenseStatusResponseObjectParent(declaration);
			using (var form = new ImportLicenseResponseMessageForm(importLicenseParent))
			{
				AssertEquals("Caption should be", "Update Import License Status", form.FormCaption);
			}
		}

		public void TestEntryHeaderGrid()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "123456";
			entry.CH_BGMReference = "123";

			IImportLicenseResponseObjectParent importLicenseParent = new ImportLicenseAcceptResponseObjectParent(declaration);

			using (var form = new ImportLicenseResponseMessageForm(importLicenseParent))
			{
				form.Show();

				AssertEquals(4, form.ImportLicensesGrid.Columns.Count);
				AssertEquals(ImportLicenseResponseObject.Schema.ReferenceNumber, form.ImportLicensesGrid.Columns[0].ColumnName);
				AssertEquals(ImportLicenseResponseObject.Schema.EntryNumber, form.ImportLicensesGrid.Columns[1].ColumnName);
				AssertEquals(ImportLicenseResponseObject.Schema.RegistrationDate, form.ImportLicensesGrid.Columns[2].ColumnName);
				AssertEquals(ImportLicenseResponseObject.Schema.Diagnosis, form.ImportLicensesGrid.Columns[3].ColumnName);
				AssertEquals(false, form.ImportLicensesGrid.GetColumnStyle(ImportLicenseResponseObject.Schema.Diagnosis).IsUnavailable);
				AssertEquals(true, form.ImportLicensesGrid.GetColumnStyle(ImportLicenseResponseObject.Schema.Status).IsUnavailable);
			}

			importLicenseParent = new ImportLicenseStatusResponseObjectParent(declaration);

			using (var form = new ImportLicenseResponseMessageForm(importLicenseParent))
			{
				form.Show();

				AssertEquals(3, form.ImportLicensesGrid.Columns.Count);
				AssertEquals(true, form.ImportLicensesGrid.GetColumnStyle(ImportLicenseResponseObject.Schema.Diagnosis).IsUnavailable);
				AssertEquals(true, form.ImportLicensesGrid.GetColumnStyle(ImportLicenseResponseObject.Schema.ReferenceNumber).IsUnavailable);
				AssertEquals(false, form.ImportLicensesGrid.GetColumnStyle(ImportLicenseResponseObject.Schema.Status).IsUnavailable);
			}
		}

		public void TestValidateAcceptResponseWithWarning()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, xmlText);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
				using (var form = new ImportLicenseResponseMessageForm(new ImportLicenseAcceptResponseObjectParent(declaration)))
				{
					form.Show();

					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					form.BrowseButton.PerformClick();

					AssertNull("No error", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Log Count", 1, form.LogDetailsListBox.Items.Count);
					AssertEquals("Log", "The Batch Identifier contained in the XML file (000001) does not match any Entry of this Job.", form.LogDetailsListBox.Items[0]);
				}
			}
		}

		public void TestLoadCorrectFile_AcceptResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "58500398000105";
			declaration.JE_OH_Importer = importer.PK;

			var message = Factory.NewWithValidTestData<BREDIMessage>();
			message.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			message.EM_ApplicationReference = "000001";

			var message2 = Factory.NewWithValidTestData<BREDIMessage>();
			message2.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			message2.EM_ApplicationReference = "000001";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			Factory.Save();

			entry.ImportLicenseIdentifierNumber.CE_EntryNum = "BXI000010021";
			entry2.ImportLicenseIdentifierNumber.CE_EntryNum = "BXI000010022";

			message.EM_LinkedObject = entry;
			message2.EM_LinkedObject = entry2;

			entry.Messages.Add(message);
			entry2.Messages.Add(message2);
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, xmlText);

				using (var form = new ImportLicenseResponseMessageForm(new ImportLicenseAcceptResponseObjectParent(declaration)))
				{
					form.Show();

					form.ImportButton.PerformClick();
					AssertEquals("The message should be", "No message has been created. Please check the Log Details.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					form.BrowseButton.PerformClick();

					form.ImportButton.PerformClick();
					AssertEquals("The message should be", "The message(s) has been loaded.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestLoadIncorrectFile_AcceptResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();

			using (var tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, xmlIncorrect);

				using (var form = new ImportLicenseResponseMessageForm(new ImportLicenseAcceptResponseObjectParent(declaration)))
				{
					form.Show();

					form.ImportButton.PerformClick();
					AssertEquals("The message should be", "No message has been created. Please check the Log Details.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					form.BrowseButton.PerformClick();

					form.ImportButton.PerformClick();
					AssertEquals("The message should be", "No message has been created. Please check the Log Details.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importLicenseParent = new ImportLicenseAcceptResponseObjectParent(declaration);
			return new ImportLicenseResponseMessageForm(importLicenseParent);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "FileNameTextBox";
		}

		#endregion

		readonly string xmlText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<lote-li>
	<cpfUsuario>000.000.000-01</cpfUsuario>
	<dataHoraEnvioFormatada>07/04/2022 09:00:00</dataHoraEnvioFormatada>
	<idLote>000001</idLote>
	<listaLIVORetorno>
		<li>
			<dtRegistro>07/04/2022</dtRegistro>
			<idSolicitacao>BXI000010021</idSolicitacao>
			<importador>
				<numero>58500398000105</numero>
				<tipoImportador>J</tipoImportador>
			</importador>
			<mensagemDiagnostico>
				<mensagemDiagnostico>ERRO NA COMUNICAÇÃO COM O DRAWBACK ISENÇÃO</mensagemDiagnostico>
			</mensagemDiagnostico>
			<numeroLI/>
		</li>
		<li>
			<dtRegistro>07/04/2022</dtRegistro>
			<idSolicitacao>BXI000010022</idSolicitacao>
			<importador>
				<numero>58500398000105</numero>
				<tipoImportador>J</tipoImportador>
			</importador>
			<mensagemDiagnostico>
				<mensagemDiagnostico>Licenciamento de Importação foi registrado</mensagemDiagnostico>
			</mensagemDiagnostico>
			<numeroLI>2200000001</numeroLI>
		</li>
	</listaLIVORetorno>
	<versao/>
	<versaoValida>true</versaoValida>
</lote-li>";

		readonly string xmlIncorrect = @"<?xml version=""1.0"" encoding=""UTF-8""?>
	<cpfUsuario>000.000.000-01</cpfUsuario>
	<dataHoraEnvioFormatada>07/04/2022 09:00:00</dataHoraEnvioFormatada>
	<idLote>000001</idLote>
	<listaLIVORetorno>
		<li>
			<dtRegistro>07/04/2022</dtRegistro>
			<idSolicitacao>BXI000010021</idSolicitacao>
			<importador>
				<numero>58500398000105</numero>
				<tipoImportador>J</tipoImportador>
			</importador>
			<mensagemDiagnostico>
				<mensagemDiagnostico>ERRO NA COMUNICAÇÃO COM O DRAWBACK ISENÇÃO</mensagemDiagnostico>
			</mensagemDiagnostico>
			<numeroLI/>
		</li>
		<li>
			<dtRegistro>07/04/2022</dtRegistro>
			<idSolicitacao>BXI000010022</idSolicitacao>
			<importador>
				<numero>58500398000105</numero>
				<tipoImportador>J</tipoImportador>
			</importador>
			<mensagemDiagnostico>
				<mensagemDiagnostico>Licenciamento de Importação foi registrado</mensagemDiagnostico>
			</mensagemDiagnostico>
			<numeroLI>2200000001</numeroLI>
		</li>
	</listaLIVORetorno>
	<versao/>
	<versaoValida>true</versaoValida>";
	}
}
