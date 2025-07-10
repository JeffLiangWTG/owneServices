using System;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.IO.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Client.SWL.Business.Testing
{
	public class ShipnetCarrierObjectTestCase : ShipnetTestCase
	{
#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: carrier")]
#else
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'carrier')")]
#endif
		public void TestConstructorWithNullCarrier()
		{
			ShipnetCarrierObject shipnetObject = ShipnetCarrierObject.New(null, null);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Carrier Dummy Carrier does not exist in database")]
		public void TestConstructorWithNotSavedCarrier()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Dummy Carrier";
			ShipnetCarrierObject shipnetObject = ShipnetCarrierObject.New(carrier, Buffer);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Carrier Dummy Carrier is not a Shipnet Carrier")]
		public void TestConstructorWithNotShipnetCarrier()
		{
			OrgHeader carrier = CreateNewShippingLine("Dummy Carrier");
			Factory.Save();
			ShipnetCarrierObject shipnetObject = ShipnetCarrierObject.New(carrier, Buffer);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: notify")]
#else
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'notify')")]
#endif
		public void TestConstructorWithNullNotify()
		{
			OrgHeader carrier = ShipnetCarrier;
			Factory.Save();
			ShipnetCarrierObject shipnetObject = ShipnetCarrierObject.New(carrier, null);
		}

		public void TestAdd()
		{
			OrgHeader carrier = ShipnetCarrier;
			AgencyShipment importShipment = AgencyTestData.NewAgencyShipment(Factory, "S00001001", AgencyTestData.Vessel1, "voyage", "USLAX", "AUSYD");
			Job job = TestObjectCreator.CreateJob(importShipment);
			ARInvoice aRInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 1);
			aRInvoice.AH_JH = job.PK;
			Factory.Save();
			ShipnetARInvoice shipnetInvoice = Factory.Load<ShipnetARInvoice>(aRInvoice.PK);
			shipnetInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			AssertEquals("PreCondition: ShipnetInvoice is not valid", false, shipnetInvoice.IsValidShipnetType);
			ShipnetCarrierObject shipnetObject = ShipnetCarrierObject.New(carrier, Buffer);
			shipnetObject.Add(shipnetInvoice);
			string expectedMessage = "Cannot add an invalid invoice type";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			Buffer.Clear();
			shipnetInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			AssertEquals("PreCondition: ShipnetInvoice is valid", true, shipnetInvoice.IsValidShipnetType);
			shipnetObject.Add(shipnetInvoice);
			AssertMultilineASCIIEquals("Buffer should contain transaction #", string.Format("Generating records for Invoice {0}", aRInvoice.AH_TransactionNum), Buffer.AsString);
			Buffer.Clear();
			shipnetInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			AssertEquals("PreCondition: ShipnetInvoice is valid", true, shipnetInvoice.IsValidShipnetType);
			shipnetObject.Add(shipnetInvoice);
			AssertMultilineASCIIEquals("Buffer should contain transaction #", string.Format("Generating records for Invoice {0}", aRInvoice.AH_TransactionNum), Buffer.AsString);
			Buffer.Clear();
			shipnetInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			AssertEquals("PreCondition: ShipnetInvoice is not valid", false, shipnetInvoice.IsValidShipnetType);
			shipnetObject.Add(shipnetInvoice);
			expectedMessage = "Cannot add an invalid invoice type";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
		}

		[TestDate(2006, 11, 23, 2, 23, 42)]
		public void TestExportWithCorrectlySetupData_UsingFileMethod()
		{
			string newTempDirectory = Temp.GetNewTempSubdirectory();
			try
			{
				string tempBackupDirectory = Path.Combine(newTempDirectory, "FGN" + ShipnetCarrier.OH_Code);
				SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newTempDirectory);
				SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier.CompanyData.PK, ShipnetCarrierSettings);
				ShipnetCarrierObject shipnetObject = GetShipnetObjectFillWithCorrectData(ShipnetCarrier);
				string testFilenameOnly = GetExpectedExportFilename(TestFile.Filename);
				ZString exportedFile = Path.Combine(TestDirectory, testFilenameOnly);
				ZString ftpedFile = Path.Combine(UnitTestFtp.Instance.DummyFTPDirectory, testFilenameOnly);
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					ZString expectedCorrectSetupFile = resourceRetriever.SaveResourceToFile("Business.TestHelper.ExpectedCorrectSetupFile.CSV");
					UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
					Env.OutgoingMailManager.EmailsCreated.Clear();
					DeleteIfExists(exportedFile);
					AssertEquals("PreCondition: Shipnet Carrier has been setup to do File", ShipnetExportCommunicationsTransportMappingList.Codes.File, ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport);
					TempDirectory.DeleteDirectory(tempBackupDirectory);
					shipnetObject.Export();
					AssertEquals("Buffer should not contain any errors\r\n" + Buffer.AsString, false, Buffer.HasErrors);
					AssertEquals("No Email Sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					AssertEquals("Ftped File should not exist: " + ftpedFile, false, File.Exists(ftpedFile));
					AssertASCIIFilesHasSameData(expectedCorrectSetupFile, exportedFile);
					ZString backupFile = Path.Combine(tempBackupDirectory, testFilenameOnly);
					AssertEquals("Backup File should exist: " + backupFile, true, File.Exists(backupFile));
					AssertASCIIFilesHasSameData(expectedCorrectSetupFile, backupFile);
				}
				AssertContains(string.Format("10 record(s) generated for SHIPNET SHIPPING LINE to {0}", ShipnetCarrierSettings.CommunicationMode.EK_Destination), Buffer.AsString);
			}
			finally
			{
				TempDirectory.DeleteDirectory(newTempDirectory);
				UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
			}
		}

		[TestDate(2006, 11, 23, 2, 23, 42)]
		public void TestExportWithCorrectlySetupData_UsingEmailMethod()
		{
			string newTempDirectory = Temp.GetNewTempSubdirectory();
			try
			{
				string tempBackupDirectory = Path.Combine(newTempDirectory, "FGN" + ShipnetCarrier.OH_Code);
				SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newTempDirectory);
				ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
				ShipnetCarrierSettings.CommunicationMode.EK_ServerAddressSubject = "DUMMY TESTING EMAIL SUBJECT";
				ShipnetCarrierSettings.CommunicationMode.EK_Destination = "DUMMYUSER@EMAIL.COM";
				SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier.CompanyData.PK, ShipnetCarrierSettings);
				ShipnetCarrierObject shipnetObject = GetShipnetObjectFillWithCorrectData(ShipnetCarrier);
				string testFilenameOnly = GetExpectedExportFilename(TestFile.Filename);
				ZString exportedFile = Path.Combine(TestDirectory, testFilenameOnly);
				ZString ftpedFile = Path.Combine(UnitTestFtp.Instance.DummyFTPDirectory, testFilenameOnly);
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					ZString expectedCorrectSetupFile = resourceRetriever.SaveResourceToFile("Business.TestHelper.ExpectedCorrectSetupFile.CSV");
					UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
					Env.OutgoingMailManager.EmailsCreated.Clear();
					DeleteIfExists(exportedFile);
					TempDirectory.DeleteDirectory(tempBackupDirectory);
					shipnetObject.Export();
					AssertEquals("Exported To file should not exist: " + exportedFile, false, File.Exists(exportedFile));
					AssertEquals("Ftped File should not exist: " + ftpedFile, false, File.Exists(ftpedFile));
					AssertEquals("Buffer should not contain any errors\r\n" + Buffer.AsString, false, Buffer.HasErrors);
					AssertEquals("Email Sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
					EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals("Email Subject", "DUMMY TESTING EMAIL SUBJECT", email.Subject);
					AssertEquals("Email Recipient", 1, email.Recipients.Count);
					AssertEquals("Email Recipient", "DUMMYUSER@EMAIL.COM", email.Recipients[0]);
					AssertEquals("Email Attachment", 1, email.Attachments.Count);
					AssertEquals("Email Attachment Filename", testFilenameOnly, email.Attachments[0].DisplayName);
					AssertASCIIFileSameAsString(expectedCorrectSetupFile, Encoding.UTF8.GetString(email.Attachments[0].Data));
					ZString backupFile = Path.Combine(tempBackupDirectory, testFilenameOnly);
					AssertEquals("Backup File should exist: " + backupFile, true, File.Exists(backupFile));
					AssertASCIIFilesHasSameData(expectedCorrectSetupFile, backupFile);
				}
				AssertContains(string.Format("10 record(s) generated for SHIPNET SHIPPING LINE to {0}", ShipnetCarrierSettings.CommunicationMode.EK_Destination), Buffer.AsString);
			}
			finally
			{
				TempDirectory.DeleteDirectory(newTempDirectory);
				UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
			}
		}

		[TestDate(2006, 11, 23, 2, 23, 42)]
		public void TestExportWithCorrectlySetupData_UsingFtpMethod()
		{
			string newTempDirectory = Temp.GetNewTempSubdirectory();
			using (var ftpTestHelper = new FtpTestHelper())
			{
				try
				{
					ftpTestHelper.Start();
					string tempBackupDirectory = Path.Combine(newTempDirectory, "FGN" + ShipnetCarrier.OH_Code);
					SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newTempDirectory);
					ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
					ShipnetCarrierSettings.CommunicationMode.EK_ServerAddressSubject = ftpTestHelper.ServerAddress.ToString();
					ShipnetCarrierSettings.CommunicationMode.EK_LoginName = ftpTestHelper.UserName;
					ShipnetCarrierSettings.CommunicationMode.EK_Password = ftpTestHelper.Password;
					ShipnetCarrierSettings.CommunicationMode.EK_PortNumber = 0;
					ShipnetCarrierSettings.CommunicationMode.EK_Destination = "";
					SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier.CompanyData.PK, ShipnetCarrierSettings);
					ShipnetCarrierObject shipnetObject = GetShipnetObjectFillWithCorrectData(ShipnetCarrier);
					string testFilenameOnly = GetExpectedExportFilename(TestFile.Filename);
					ZString exportedFile = Path.Combine(TestDirectory, testFilenameOnly);
					ZString ftpedFile = Path.Combine(UnitTestFtp.Instance.DummyFTPDirectory, testFilenameOnly);
					using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
					{
						ZString expectedCorrectSetupFile = resourceRetriever.SaveResourceToFile("Business.TestHelper.ExpectedCorrectSetupFile.CSV");
						UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
						Env.OutgoingMailManager.EmailsCreated.Clear();
						DeleteIfExists(exportedFile);
						TempDirectory.DeleteDirectory(tempBackupDirectory);
						shipnetObject.Export();
						AssertEquals("Exported To file should not exist: " + exportedFile, false, File.Exists(exportedFile));
						AssertEquals("No Email Sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
						AssertEquals("Buffer should not contain any errors\r\n" + Buffer.AsString, false, Buffer.HasErrors);
						AssertASCIIFilesHasSameData(expectedCorrectSetupFile, ftpedFile);
						ZString backupFile = Path.Combine(tempBackupDirectory, testFilenameOnly);
						AssertEquals("Backup File should exist: " + backupFile, true, File.Exists(backupFile));
						AssertASCIIFilesHasSameData(expectedCorrectSetupFile, backupFile);
					}
					AssertContains(string.Format("10 record(s) generated for SHIPNET SHIPPING LINE to {0}", ShipnetCarrierSettings.CommunicationMode.EK_Destination), Buffer.AsString);
				}
				finally
				{
					TempDirectory.DeleteDirectory(newTempDirectory);
					UnitTestFtp.Instance.ResetFtpNotTransferBecauseInTestMode();
				}
			}
		}

		[TestDate(2006, 11, 23, 2, 23, 42)]
		public void TestExportHandlingErrors()
		{
			FreightDataRegistry.Instance.VesselCustomAttribute1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Attribute 1");
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Dummy User";
			staff.GS_EmailAddress = "DUMMYUSER@EMAIL.COM";
			SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, SWLDataRegistry.Instance.ShipnetNotificationEmailGroup.DefaultValue);
			AgencyShipment importShipment = AgencyTestData.NewAgencyShipment(Factory, "S00001001", VesselWithAttribute.RV_Code, "voyage", "USLAX", "AUSYD");
			importShipment.JS_HouseBill = "OBL1234567";
			Job importJob = TestObjectCreator.CreateJob(importShipment);
			ExchangeRate importRate = importJob.ExchangeRates.AddNew();
			importRate.JF_RX_NKRateCurrency = TestObjectCreator.GBP.RX_Code;
			ARInvoice aRInvoice1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.LocalCurrency, 1);
			aRInvoice1.AH_JH = importJob.PK;
			aRInvoice1.AH_OH = TestObjectCreator.LocalClient.PK;
			aRInvoice1.AH_InvoiceDate = new ZDateTime(2006, 3, 25);
			aRInvoice1.AH_PostDate = aRInvoice1.AH_InvoiceDate.AddDays(1);
			aRInvoice1.AH_DueDate = aRInvoice1.AH_InvoiceDate.AddDays(2);
			AssertEquals("PreCondition: Shipnet Setting should have 2 Charge Groups", 2, ShipnetCarrierSettings.ChargeGroups.Count);
			AssertEquals("PreCondition: Charge Code CC2 should not be setup as Shipnet Charge Code", false, ShipnetCarrierSettings.ChargeGroups[0].Charges.ContainsChargePK(TestObjectCreator.CC2.PK));
			AssertEquals("PreCondition: Charge Code CC2 should not be setup as Shipnet Charge Code", false, ShipnetCarrierSettings.ChargeGroups[1].Charges.ContainsChargePK(TestObjectCreator.CC2.PK));
			ARInvoiceLine aRInvoiceLineWithUnknownCode = TestObjectCreator.CreateARInvoiceLine(aRInvoice1, importJob, TestObjectCreator.CC2, TestObjectCreator.LocalCurrency, 1, "Line With Known Charge Desc", 0);
			aRInvoiceLineWithUnknownCode.AL_OSExTaxAmount = 596m;
			aRInvoiceLineWithUnknownCode.AL_OSTaxAmount = 59.6m;
			TestObjectCreator.CreateJobCharge(aRInvoiceLineWithUnknownCode, importJob, TestObjectCreator.CC2, TestObjectCreator.LocalCurrency);
			AgencyShipment exportShipment = AgencyTestData.NewAgencyShipment(Factory, "S00001002", VesselWithoutAttribute.RV_Code, "voyage", "AUSYD", "USLAX");
			exportShipment.JS_HouseBill = "HOUSEBILL2";
			Job exportJob = TestObjectCreator.CreateJob(exportShipment);
			ExchangeRate exportRate = exportJob.ExchangeRates.AddNew();
			exportRate.JF_RX_NKRateCurrency = TestObjectCreator.GBP.RX_Code;
			ARInvoice aRInvoice2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.LocalCurrency, 1);
			aRInvoice2.AH_JH = exportJob.PK;
			aRInvoice2.AH_OH = TestObjectCreator.LocalClient.PK;
			aRInvoice2.AH_InvoiceDate = new ZDateTime(2006, 4, 25);
			aRInvoice2.AH_PostDate = aRInvoice2.AH_InvoiceDate.AddDays(1);
			aRInvoice2.AH_DueDate = aRInvoice2.AH_InvoiceDate.AddDays(2);
			ARInvoiceLine aRInvoiceLineWithKnownCode = TestObjectCreator.CreateARInvoiceLine(aRInvoice2, exportJob, TestChargeWithGST, TestObjectCreator.LocalCurrency, 1, "Line Without Known Charge Desc", 0);
			aRInvoiceLineWithKnownCode.AL_OSExTaxAmount = 600m;
			aRInvoiceLineWithKnownCode.AL_OSTaxAmount = 60m;
			TestObjectCreator.CreateJobCharge(aRInvoiceLineWithKnownCode, exportJob, TestChargeWithGST, TestObjectCreator.LocalCurrency);
			AgencyShipment nonIEShipment = AgencyTestData.NewAgencyShipment(Factory, "S00001003", VesselWithoutAttribute.RV_Code, "voyage", "NZAKL", "USLAX");
			nonIEShipment.JS_HouseBill = "HOUSEBILL3";
			Job nonIEJob = TestObjectCreator.CreateJob(nonIEShipment);
			ARInvoice aRInvoice3 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.LocalCurrency, 1);
			aRInvoice3.AH_JH = nonIEJob.PK;
			aRInvoice3.AH_OH = TestObjectCreator.LocalClient.PK;
			aRInvoice3.AH_InvoiceDate = new ZDateTime(2006, 1, 25);
			aRInvoice3.AH_PostDate = aRInvoice3.AH_InvoiceDate.AddDays(1);
			aRInvoice3.AH_DueDate = aRInvoice3.AH_InvoiceDate.AddDays(2);
			ARInvoiceLine aRInvoiceLineWithGSTFree = TestObjectCreator.CreateARInvoiceLine(aRInvoice3, nonIEJob, TestChargeWithGSTFree, TestObjectCreator.LocalCurrency, 1, "Line Without Known Charge Desc", 0);
			aRInvoiceLineWithGSTFree.AL_OSExTaxAmount = 600m;
			aRInvoiceLineWithGSTFree.AL_OSTaxAmount = 60m;
			TestObjectCreator.CreateJobCharge(aRInvoiceLineWithGSTFree, nonIEJob, TestChargeWithGSTFree, TestObjectCreator.LocalCurrency);
			SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier.CompanyData.PK, ShipnetCarrierSettings);
			aRInvoice1.AH_FullyPaidDate = ZDateTime.Empty;
			aRInvoice2.AH_FullyPaidDate = ZDateTime.Empty;
			aRInvoice3.AH_FullyPaidDate = ZDateTime.Empty;
			aRInvoice1.AH_TransactionNum = "INV00001001";
			aRInvoice2.AH_TransactionNum = "1002";
			aRInvoice3.AH_TransactionNum = "INV00001003";
			aRInvoice1.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRInvoice2.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRInvoice3.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();
			ShipnetARInvoice shipnetInvoice1 = Factory.Load<ShipnetARInvoice>(aRInvoice1.PK);
			ShipnetARInvoice shipnetInvoice2 = Factory.Load<ShipnetARInvoice>(aRInvoice2.PK);
			ShipnetARInvoice shipnetInvoice3 = Factory.Load<ShipnetARInvoice>(aRInvoice3.PK);
			ShipnetCarrierObject shipnetObject = ShipnetCarrierObject.New(ShipnetCarrier, Buffer);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Buffer.Clear();
			string testFilenameOnly = GetExpectedExportFilename(TestFile.Filename);
			ZString exportedFile = Path.Combine(TestDirectory, testFilenameOnly);
			DeleteIfExists(exportedFile);
			AssertEquals("PreCondition: Shipnet Carrier has been setup to do File", ShipnetExportCommunicationsTransportMappingList.Codes.File, ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport);
			shipnetObject.Export();
			string expectedMessage = string.Format("Shipnet Carrier {0} has no data to export.", ShipnetCarrier.OH_FullName);
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals("Email Sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email Subject", "Error Encountered During Exporting Shipnet Data for Carrier " + ShipnetCarrier.OH_FullName, email.Subject);
			AssertEquals("Email Recipient", 1, email.Recipients.Count);
			AssertEquals("Email Recipient", "DUMMYUSER@EMAIL.COM", email.Recipients[0]);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
			AssertEquals("Exported File not should exist: " + exportedFile, false, File.Exists(exportedFile));
			Buffer.Clear();
			shipnetObject.Add(shipnetInvoice1);
			shipnetObject.Add(shipnetInvoice2);
			shipnetObject.Add(shipnetInvoice3);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			shipnetObject.Export();
			AssertEquals("Email Sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email Subject", "Error Encountered During Exporting Shipnet Data for Carrier " + ShipnetCarrier.OH_FullName, email.Subject);
			AssertEquals("Email Recipient", 1, email.Recipients.Count);
			AssertEquals("Email Recipient", "DUMMYUSER@EMAIL.COM", email.Recipients[0]);
			expectedMessage = "Invoice INV00001001 cannot be converted exactly to USD Currency as Shipping Booking S00001001 does not have USD exchange rate";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
			expectedMessage = "Invoice 1002 cannot be converted exactly to USD Currency as Shipping Booking S00001002 does not have USD exchange rate";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
			expectedMessage = "Invoice INV00001003 cannot be converted exactly to USD Currency as Shipping Booking S00001003 is not Import or Export";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
			expectedMessage = "Error: The following charge code(s) were not setup:\r\nZZCC2";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
			AssertEquals("Exported File should exist: " + exportedFile, true, File.Exists(exportedFile));
			ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport = "";
			SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier.CompanyData.PK, ShipnetCarrierSettings);
			Factory.Save();
			Buffer.Clear();
			shipnetObject = ShipnetCarrierObject.New(ShipnetCarrier, Buffer);
			shipnetObject.Add(shipnetInvoice3);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			shipnetObject.Export();
			AssertEquals("Email Sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email Subject", "Error Encountered During Exporting Shipnet Data for Carrier " + ShipnetCarrier.OH_FullName, email.Subject);
			AssertEquals("Email Recipient", 1, email.Recipients.Count);
			AssertEquals("Email Recipient", "DUMMYUSER@EMAIL.COM", email.Recipients[0]);
			expectedMessage = "Invoice INV00001003 cannot be converted exactly to USD Currency as Shipping Booking S00001003 is not Import or Export";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
			expectedMessage = string.Format("Unknown code (Unknown Delivery Method for Carrier {0}.", ShipnetCarrier.OH_FullName);
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
			ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			ShipnetCarrierSettings.CommunicationMode.EK_Destination = "";
			SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier.CompanyData.PK, ShipnetCarrierSettings);
			Buffer.Clear();
			TestObjectCreator.USD.RX_Code = "ZZD";
			Factory.Save();
			shipnetObject = ShipnetCarrierObject.New(ShipnetCarrier, Buffer);
			shipnetObject.Add(shipnetInvoice3);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			shipnetObject.Export();
			AssertEquals("Email Sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email Subject", "Error Encountered During Exporting Shipnet Data for Carrier " + ShipnetCarrier.OH_FullName, email.Subject);
			AssertEquals("Email Recipient", 1, email.Recipients.Count);
			AssertEquals("Email Recipient", "DUMMYUSER@EMAIL.COM", email.Recipients[0]);
			expectedMessage = "Cannot find USD Currency in system";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
			expectedMessage = "There was an error encountered while delivering file ";
			AssertEquals(string.Format("Buffer should contain error '{0}'. Buffer contain:{1}{2}", expectedMessage, System.Environment.NewLine, Buffer.AsString), true, Buffer.AsString.IndexOf(expectedMessage) >= 0);
			AssertEquals(string.Format("Email Body should contain error '{0}'. Email Body contain:{1}{2}", expectedMessage, System.Environment.NewLine, email.Body), true, email.Body.IndexOf(expectedMessage) >= 0);
		}

		public void TestOverriddenNewDelegate()
		{
			OrgHeader carrier = ShipnetCarrier;
			Factory.Save();
			AssertEquals("ShipnetCarrierObject Type", typeof(ShipnetCarrierObject), ShipnetCarrierObject.New(carrier, Buffer).GetType());
			ShipnetCarrierObjectNewDelegate.Initialise();
			AssertEquals("ShipnetCarrierObject Type", typeof(ShipnetCarrierObjectNewDelegate), ShipnetCarrierObject.New(carrier, Buffer).GetType());
		}

		[TestDate(2006, 11, 23, 2, 23, 42)]
		public void TestExportUsingFileMethod_InaccessibleBackupFolder()
		{
			var tempBackupDirectory = Path.Combine(SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.Value, "FGN" + ShipnetCarrier.OH_Code);
			SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(ShipnetCarrier.CompanyData.PK, ShipnetCarrierSettings);
			var shipnetObject = GetShipnetObjectFillWithCorrectData(ShipnetCarrier);
			var exportedFile = Path.Combine(TestDirectory, GetExpectedExportFilename(TestFile.Filename));
			DeleteIfExists(exportedFile);
			TempDirectory.DeleteDirectory(tempBackupDirectory);
			CreateFolderAndSetInaccessible(tempBackupDirectory);
			AssertNoExceptionThrown(() => shipnetObject.Export());
			ClearUpForTestBackupTempFile_TargetFileNotAccessible(tempBackupDirectory, exportedFile);
		}

		void CreateFolderAndSetInaccessible(ZString folder)
		{
			Directory.CreateDirectory(folder);
			Directory.Delete(folder);
			var security = new DirectorySecurity();
			security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny));
			var directoryInfo = Directory.CreateDirectory(folder);
			directoryInfo.SetAccessControl(security);
		}

		void ClearUpForTestBackupTempFile_TargetFileNotAccessible(ZString folder, ZString tempFile)
		{
			var directoryInfo = new DirectoryInfo(folder);
			var security = new DirectorySecurity();
			security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
			directoryInfo.SetAccessControl(security);
			File.Delete(tempFile);
			Directory.Delete(folder);
		}

		#region Implementation

		protected ShipnetCarrierObject GetShipnetObjectFillWithCorrectData(OrgHeader carrier)
		{
			ZGuid localClient1 = TestObjectCreator.LocalClient.PK;
			ZGuid localClient2 = TestObjectCreator.LocalClient2.PK;
			FreightDataRegistry.Instance.VesselCustomAttribute1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Attribute 1");
			AgencyShipment importShipment = AgencyTestData.NewAgencyShipment(Factory, "S00001001", VesselWithAttribute.RV_Code, "105", "USLAX", "AUSYD");
			importShipment.JS_HouseBill = "OBL1234567";
			Job importJob = TestObjectCreator.CreateJob(importShipment);
			ExchangeRate importRate = importJob.ExchangeRates.AddNew();
			importRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			importRate.JF_BaseRate = 0.755m;
			ARInvoice aRInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 0.745m);
			aRInvoice.AH_JH = importJob.PK;
			aRInvoice.AH_OH = localClient1;
			aRInvoice.AH_InvoiceDate = new ZDateTime(2006, 3, 25);
			aRInvoice.AH_PostDate = aRInvoice.AH_InvoiceDate.AddDays(1);
			aRInvoice.AH_DueDate = aRInvoice.AH_InvoiceDate.AddDays(2);
			ARInvoiceLine aRInvoiceLine1 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, importJob, TestChargeWithGST, TestObjectCreator.USD, 0.745m, "Line 1 Charge Desc", 0);
			aRInvoiceLine1.AL_OSExTaxAmount = 596m;
			aRInvoiceLine1.AL_OSTaxAmount = 59.6m;
			TestObjectCreator.CreateJobCharge(aRInvoiceLine1, importJob, TestChargeWithGST, TestObjectCreator.USD);
			ARInvoiceLine aRInvoiceLine2 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, importJob, TestChargeWithGSTFree, TestObjectCreator.USD, 0.745m, "Line 2 Charge Desc", 0);
			aRInvoiceLine2.AL_OSExTaxAmount = 149m;
			aRInvoiceLine2.AL_OSTaxAmount = 0m;
			TestObjectCreator.CreateJobCharge(aRInvoiceLine2, importJob, TestChargeWithGSTFree, TestObjectCreator.USD);
			ARCreditNote aRCreditNote = (ARCreditNote)TestObjectCreator.CreateInvoice(typeof(ARCreditNote), TestObjectCreator.GBP, 0.655m);
			aRCreditNote.AH_JH = importJob.PK;
			aRCreditNote.AH_OH = localClient2;
			aRCreditNote.AH_InvoiceDate = new ZDateTime(2006, 3, 22);
			aRCreditNote.AH_PostDate = aRCreditNote.AH_InvoiceDate.AddDays(1);
			aRCreditNote.AH_DueDate = aRCreditNote.AH_InvoiceDate.AddDays(2);
			ARCreditNoteLine aRCreditNoteLine = TestObjectCreator.CreateARCreditNoteLine(aRCreditNote, importJob, TestChargeWithGST, 0, TestObjectCreator.GBP, 0.655m, "Charge Desc");
			aRCreditNoteLine.AL_TaxRateNumerator = 25;
			aRCreditNoteLine.AL_TaxRateDenominator = 4;
			aRCreditNoteLine.AL_OSExTaxAmount = 655m;
			aRCreditNoteLine.AL_OSTaxAmount = 65.5m;
			TestObjectCreator.CreateJobCharge(aRCreditNoteLine, importJob, TestChargeWithGST, TestObjectCreator.GBP);
			AgencyShipment exportShipment = AgencyTestData.NewAgencyShipment(Factory, "S00001002", VesselWithoutAttribute.RV_Code, "134", "USLAX", "AUSYD");
			exportShipment.JS_HouseBill = "";
			Job exportJob = TestObjectCreator.CreateJob(exportShipment);
			ExchangeRate exportRate = exportJob.ExchangeRates.AddNew();
			exportRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			exportRate.JF_BaseRate = 0.766m;
			ARInvoice aRInvoice2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 0.755m);
			aRInvoice2.AH_JH = exportJob.PK;
			aRInvoice2.AH_OH = TestObjectCreator.LocalClient.PK;
			aRInvoice2.AH_InvoiceDate = new ZDateTime(2006, 4, 25);
			aRInvoice2.AH_PostDate = aRInvoice2.AH_InvoiceDate.AddDays(1);
			aRInvoice2.AH_DueDate = aRInvoice2.AH_InvoiceDate.AddDays(2);
			ARInvoiceLine aRInvoiceLine3 = TestObjectCreator.CreateARInvoiceLine(aRInvoice2, exportJob, TestChargeWithGST, TestObjectCreator.USD, 0.755m, "Line 3 Charge Desc", 0);
			aRInvoiceLine3.AL_OSExTaxAmount = 600m;
			aRInvoiceLine3.AL_OSTaxAmount = 60m;
			TestObjectCreator.CreateJobCharge(aRInvoiceLine3, exportJob, TestChargeWithGST, TestObjectCreator.USD);
			ARInvoiceLine aRInvoiceLine4 = TestObjectCreator.CreateARInvoiceLine(aRInvoice2, exportJob, TestChargeWithGSTFree, TestObjectCreator.USD, 0.755m, "Line 4 Charge Desc", 0);
			aRInvoiceLine4.AL_OSExTaxAmount = 150m;
			aRInvoiceLine4.AL_OSTaxAmount = 0m;
			TestObjectCreator.CreateJobCharge(aRInvoiceLine4, exportJob, TestChargeWithGSTFree, TestObjectCreator.USD);
			ARCreditNote aRCreditNote2 = (ARCreditNote)TestObjectCreator.CreateInvoice(typeof(ARCreditNote), TestObjectCreator.GBP, 0.655m);
			aRCreditNote2.AH_JH = exportJob.PK;
			aRCreditNote2.AH_OH = TestObjectCreator.LocalClient2.PK;
			aRCreditNote2.AH_InvoiceDate = new ZDateTime(2006, 4, 22);
			aRCreditNote2.AH_PostDate = aRCreditNote2.AH_InvoiceDate.AddDays(1);
			aRCreditNote2.AH_DueDate = aRCreditNote2.AH_InvoiceDate.AddDays(2);
			ARCreditNoteLine aRCreditNoteLine2 = TestObjectCreator.CreateARCreditNoteLine(aRCreditNote2, exportJob, TestChargeWithGST, 0, TestObjectCreator.GBP, 0.555m, "Charge Desc");
			aRCreditNoteLine2.AL_OSExTaxAmount = 700m;
			aRCreditNoteLine2.AL_OSTaxAmount = 70m;
			TestObjectCreator.CreateJobCharge(aRCreditNoteLine2, exportJob, TestChargeWithGST, TestObjectCreator.GBP);
			aRInvoice.AH_TransactionNum = "INV00001001";
			aRCreditNote.AH_TransactionNum = "CRD00001001";
			aRInvoice2.AH_TransactionNum = "1002";
			aRCreditNote2.AH_TransactionNum = "CRD00001002";
			aRInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRCreditNote.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRInvoice2.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRCreditNote2.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();
			ShipnetARInvoice shipnetInvoice = Factory.Load<ShipnetARInvoice>(aRInvoice.PK);
			ShipnetARInvoice shipnetCreditNote = Factory.Load<ShipnetARInvoice>(aRCreditNote.PK);
			ShipnetARInvoice shipnetInvoice2 = Factory.Load<ShipnetARInvoice>(aRInvoice2.PK);
			ShipnetARInvoice shipnetCreditNote2 = Factory.Load<ShipnetARInvoice>(aRCreditNote2.PK);
			ShipnetCarrierObject shipnetObject = ShipnetCarrierObject.New(carrier, Buffer);
			shipnetObject.Add(shipnetInvoice);
			shipnetObject.Add(shipnetCreditNote);
			shipnetObject.Add(shipnetInvoice2);
			shipnetObject.Add(shipnetCreditNote2);
			return shipnetObject;
		}

		#region Buffer

		protected NotificationBuffer Buffer
		{
			get
			{
				if (fBuffer == null)
				{
					fBuffer = new NotificationBuffer();
				}

				return fBuffer;
			}
		}

		NotificationBuffer fBuffer;

		#endregion

		class ShipnetCarrierObjectNewDelegate : ShipnetCarrierObject
		{
			ShipnetCarrierObjectNewDelegate(OrgHeader carrier, INotifications notify) : base(carrier, notify)
			{
			}

			public static void Initialise()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}

			public new static ShipnetCarrierObjectNewDelegate New(OrgHeader carrier, INotifications notify)
			{
				return new ShipnetCarrierObjectNewDelegate(carrier, notify);
			}
		}

		#endregion
	}
}
