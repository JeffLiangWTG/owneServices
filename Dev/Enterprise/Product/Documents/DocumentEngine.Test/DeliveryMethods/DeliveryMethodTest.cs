using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	class DeliveryMethodTest : TestCaseWithFactory
	{
		public void TestDeliverEmptyReportWithAnEmptyReportContingencyTypeNull()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = true;
			deliveryMethod.EmptyReportContingency = null;
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", true, deliveryMethod.IsDelivered);
		}

		public void TestDeliverEmptyReportWithAnEmptyReportContingencyTypeNotSet()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = true;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(string.Empty, "unit.test@cargowise.com");
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", true, deliveryMethod.IsDelivered);
		}

		public void TestDeliverEmptyReportWithAnEmptyReportContingencyTypeSendReport()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = true;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendReport, "unit.test@cargowise.com");
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", true, deliveryMethod.IsDelivered);
		}

		public void TestDeliverEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotification()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = true;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, "unit.test@cargowise.com");
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", false, deliveryMethod.IsDelivered);
		}

		public void TestDeliverEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotification_EmailRecipientExceedMaxLength()
		{
			var deliveryMethod = new DummyDeliveryMethod();
			var emailAddress = string.Empty;

			for (int i = 0; i <= 30; i++)
			{
				emailAddress += string.Format("email{0}@edi.com.au, ", i);
			}

			deliveryMethod.ReportHasNoDataRows = true;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, emailAddress);
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", false, deliveryMethod.IsDelivered);
		}

		public void TestDeliverEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotification_ShouldRespectEmailDestinationOverrideRegistrySetting_WithoutEmailOverride()
		{
			AssertDeliverEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotification_ShouldRespectEmailDestinationOverrideRegistrySetting("unit.test@cargowise.com", string.Empty, "unit.test@cargowise.com");
		}

		public void TestDeliverEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotification_ShouldRespectEmailDestinationOverrideRegistrySetting_WithEmailOverride()
		{
			AssertDeliverEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotification_ShouldRespectEmailDestinationOverrideRegistrySetting("unit.test@cargowise.com", "override@cargowise.com", "override@cargowise.com");
		}

		void AssertDeliverEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotification_ShouldRespectEmailDestinationOverrideRegistrySetting(string emptyReportContigencyEmail, string emailDestinationOverride, string expectedEmailAddress)
		{
			TestCaseHelper.ClearTable(MailRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(MailAttachment.Schema.TableName);
			TestCaseHelper.ClearTable(MailItem.Schema.TableName);

			Env.Registry.EmailDestinationOverride = emailDestinationOverride;

			var deliveryMethod = new DummyDeliveryMethod();
			deliveryMethod.ReportHasNoDataRows = true;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, emptyReportContigencyEmail);
			deliveryMethod.Deliver();

			var emails = Factory.Load<MailItem>(new ZQuery());
			AssertEquals("There should be ONE email created", 1, emails.Length);
			AssertEquals("There should be ONE recipient", 1, emails[0].MailRecipients.Count);
			AssertEquals(expectedEmailAddress, emails[0].MailRecipients[0].EmailAddress);
			if (!string.IsNullOrEmpty(emailDestinationOverride))
			{
				Assert("override message", emails[0].MI_Body.Contains(string.Format("(This message was redirected to {0} as it was sent from a non-production system. Originally the email was addressed to {1}.)", emailDestinationOverride, emptyReportContigencyEmail)));
			}
			else
			{
				Assert("No override message", !emails[0].MI_Body.Contains("(This message was redirected to"));
			}
		}

		public void TestDeliverEmptyReportWithAnEmptyReportContingencyTypeSendNothing()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = true;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendNothing, "unit.test@cargowise.com");
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", false, deliveryMethod.IsDelivered);
		}

		public void TestDeliverNonEmptyReportWithAnEmptyReportContingencyTypeNull()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = false;
			deliveryMethod.EmptyReportContingency = null;
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", true, deliveryMethod.IsDelivered);
		}

		public void TestDeliverNonEmptyReportWithAnEmptyReportContingencyTypeNotSet()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = false;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(string.Empty, "unit.test@cargowise.com");
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", true, deliveryMethod.IsDelivered);
		}

		public void TestDeliverNonEmptyReportWithAnEmptyReportContingencyTypeSendReport()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = false;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendReport, "unit.test@cargowise.com");
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", true, deliveryMethod.IsDelivered);
		}

		public void TestDeliverNonEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotification()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = false;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, "unit.test@cargowise.com");
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", true, deliveryMethod.IsDelivered);
		}

		public void TestDeliverNonEmptyReportWithAnEmptyReportContingencyTypeSendEmailNotificationIncludingCcAndBcc()
		{
			// Case 1: TO + CC + BCC
			// Arrange
			TestCaseHelper.ClearTable(AutoMailDBRecipients.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBAttachments.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBItems.Schema.TableName);
			var deliveryMethod = new DummyDeliveryMethod
			{
				ReportHasNoDataRows = true,
				EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, "stone@stone.com", "stonecc@stone.com", "stonebcc@stone.com")
			};
			// Act
			deliveryMethod.Deliver();
			// Assert
			var emails = (new BusinessObjectFactory()).Load<MailItem>(new ZQuery());
			AssertEquals("There should be 1 email created.", 1, emails.Length);
			AssertEquals("There should be 3 recipients.", 3, emails[0].MailRecipients.Count);
			AssertEquals("stone@stone.com", emails[0].MailRecipients[0].EmailAddress);
			AssertEquals("stonecc@stone.com", emails[0].MailRecipients[1].EmailAddress);
			AssertEquals("stonebcc@stone.com", emails[0].MailRecipients[2].EmailAddress);

			// Case 2: TO only
			// Arrange
			TestCaseHelper.ClearTable(AutoMailDBRecipients.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBAttachments.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBItems.Schema.TableName);
			deliveryMethod = new DummyDeliveryMethod
			{
				ReportHasNoDataRows = true,
				EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, "stone@stone.com")
			};
			// Act
			deliveryMethod.Deliver();
			// Assert
			emails = (new BusinessObjectFactory()).Load<MailItem>(new ZQuery());
			AssertEquals("There should be 1 email created.", 1, emails.Length);
			AssertEquals("There should be 1 recipient.", 1, emails[0].MailRecipients.Count);
			AssertEquals("stone@stone.com", emails[0].MailRecipients[0].EmailAddress);
			AssertEquals("TO", emails[0].MailRecipients[0].MR_RecipientType);

			// Case 3: CC only
			// Arrange
			TestCaseHelper.ClearTable(AutoMailDBRecipients.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBAttachments.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBItems.Schema.TableName);
			deliveryMethod = new DummyDeliveryMethod
			{
				ReportHasNoDataRows = true,
				EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, ZString.Empty, "stonecc@stone.com", ZString.Empty)
			};
			// Act
			deliveryMethod.Deliver();
			// Assert
			emails = (new BusinessObjectFactory()).Load<MailItem>(new ZQuery());
			AssertEquals("There should be 1 email created.", 1, emails.Length);
			AssertEquals("There should be 1 recipient.", 1, emails[0].MailRecipients.Count);
			AssertEquals("stonecc@stone.com", emails[0].MailRecipients[0].EmailAddress);
			AssertEquals("CC", emails[0].MailRecipients[0].MR_RecipientType);

			// Case 4: 1 BCC only
			// Arrange
			TestCaseHelper.ClearTable(AutoMailDBRecipients.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBAttachments.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBItems.Schema.TableName);
			deliveryMethod = new DummyDeliveryMethod
			{
				ReportHasNoDataRows = true,
				EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, ZString.Empty, ZString.Empty, "stonebcc@stone.com")
			};
			// Act
			deliveryMethod.Deliver();
			// Assert
			emails = (new BusinessObjectFactory()).Load<MailItem>(new ZQuery());
			AssertEquals("There should be 1 email created.", 1, emails.Length);
			AssertEquals("There should be 1 recipient.", 1, emails[0].MailRecipients.Count);
			AssertEquals("stonebcc@stone.com", emails[0].MailRecipients[0].EmailAddress);
			AssertEquals("Only 1 BCC should be conerted to TO.", "TO", emails[0].MailRecipients[0].MR_RecipientType);

			// Case 5: 2 BCCs
			// Arrange
			TestCaseHelper.ClearTable(AutoMailDBRecipients.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBAttachments.Schema.TableName);
			TestCaseHelper.ClearTable(AutoMailDBItems.Schema.TableName);
			deliveryMethod = new DummyDeliveryMethod
			{
				ReportHasNoDataRows = true,
				EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, ZString.Empty, ZString.Empty, "stonebcc1@stone.com,stonebcc2@stone.com")
			};
			// Act
			deliveryMethod.Deliver();
			// Assert
			emails = (new BusinessObjectFactory()).Load<MailItem>(new ZQuery());
			AssertEquals("There should be 1 email created.", 1, emails.Length);
			AssertEquals("There should be 2 recipients.", 2, emails[0].MailRecipients.Count);
			AssertEquals("stonebcc1@stone.com", emails[0].MailRecipients[0].EmailAddress);
			AssertEquals("BCC", emails[0].MailRecipients[0].MR_RecipientType);
			AssertEquals("stonebcc2@stone.com", emails[0].MailRecipients[1].EmailAddress);
			AssertEquals("BCC", emails[0].MailRecipients[1].MR_RecipientType);
		}

		public void TestDeliverNonEmptyReportWithAnEmptyReportContingencyTypeSendNothing()
		{
			var deliveryMethod = new DummyDeliveryMethod();

			deliveryMethod.ReportHasNoDataRows = false;
			deliveryMethod.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendNothing, "unit.test@cargowise.com");
			deliveryMethod.Deliver();

			AssertEquals("deliveryMethod.IsDelivered", true, deliveryMethod.IsDelivered);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesWithOLEObjectInsideNoExceptionThrownInUserInteractiveMode()
		{
			Globals.IsUserInteractive = true;
			Globals.IsWeb = false;

			var xLSInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo1.Name = "XLSInfo1";
			xLSInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "SimpleTestWithOLEObject.xls", FileMode.Open, FileAccess.Read), "xls");
			var xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo2.Name = "XLSInfo2";
			xLSInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "SimpleTestWithOLEObject.xls", FileMode.Open, FileAccess.Read), "xls");

			var method = new DeliveryMethodThatJustMergesExcelFilesForTesting();
			method.AddFile(xLSInfo1);
			method.AddFile(xLSInfo2);

			AssertNoExceptionThrown(() => method.Deliver());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesWithOLEObjectInsideNoExceptionThrownInNonUserInteractiveMode()
		{
			Globals.IsUserInteractive = false;
			Globals.IsWeb = false;

			var xLSInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo1.Name = "XLSInfo1";
			xLSInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "SimpleTestWithOLEObject.xls", FileMode.Open, FileAccess.Read), "xls");
			var xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo2.Name = "XLSInfo2";
			xLSInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "SimpleTestWithOLEObject.xls", FileMode.Open, FileAccess.Read), "xls");

			var method = new DeliveryMethodThatJustMergesExcelFilesForTesting();
			method.AddFile(xLSInfo1);
			method.AddFile(xLSInfo2);

			AssertNoExceptionThrown(() => method.Deliver());
			AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesWithOLEObjectInsideNoExceptionThrownInWebMode()
		{
			Globals.IsUserInteractive = true;
			Globals.IsWeb = true;

			var xLSInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo1.Name = "XLSInfo1";
			xLSInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "SimpleTestWithOLEObject.xls", FileMode.Open, FileAccess.Read), "xls");
			var xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo2.Name = "XLSInfo2";
			xLSInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "SimpleTestWithOLEObject.xls", FileMode.Open, FileAccess.Read), "xls");

			var method = new DeliveryMethodThatJustMergesExcelFilesForTesting();
			method.AddFile(xLSInfo1);
			method.AddFile(xLSInfo2);

			AssertNoExceptionThrown(() => method.Deliver());
			AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[ExpectNoExceptions]
		public void TestAutoDeliveryNoContactsUsePrinter()
		{
			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_QueueName = "Zubin Queue";
			Factory.Save();

			try
			{
				var instructions = new DeliveryInstructions();
				instructions.DocumentPackCount = 2;
				instructions.AutoDeliverMultiDocPack = true;

				DeliveryMethod.IsPrinterSpecifiedForTesting = true;
				var method = DeliveryMethod.FromContact(null, instructions);
				AssertNull("No method should be returned as a print queue is not specified", method);

				DeliveryMethod.IsPrinterSpecifiedForTesting = false;
				instructions.PrinterDelivery.PrintQueuePK = printQueue.PK;
				method = DeliveryMethod.FromContact(null, instructions);
				AssertNotNull("Printer method should be returned as a print queue IS specified", method);
				method.DeliveryInfosForTesting.Add(new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document));
				method.Infos[0].SetFileContents(SimpleTestXls, "xls");
				method.Deliver();
			}
			finally
			{
				DeliveryMethod.IsPrinterSpecifiedForTesting = false;
			}
		}

		public void TestPrintJobWithoutPrinterCannotBeSavedToDatabase()
		{
			var previousPrintJobsCount = Factory.Load<StmPrintJob>(new ZQuery()).Length;
			var instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.Print);
			var method = DeliveryMethod.FromContact(null, instructions);
			method.DeliveryInfosForTesting.Add(new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document));
			method.Infos[0].SetFileContents(SimpleTestXls, "xls");
			method.Deliver();

			var warningMessage = @"No Printer for []. A printer must be specified when using a delivery method of 'PRN'. Delivery instructions on delivery info are not set. Printer has not been set. Please close the form and reopen it again";
			var currentPrintJobsCount = Factory.Load<StmPrintJob>(new ZQuery()).Length;

			CombineAssertions(() =>
			{
				AssertEquals(previousPrintJobsCount, currentPrintJobsCount);
				AssertEquals(warningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestNoExceptionThrownIfNoCurrentUser()
		{
			EnvProxy.Instance.SetUserContext(null);

			AssertNoExceptionThrown(delegate
			{
				var method = new DeliveryMethod();
			});
		}

		// internal class DummyMethodForTesting : DeliveryMethod
		// {
		// 	public bool SetPropertiesFromDeliveryInstructionsCalled;

		// 	protected override void SetPropertiesFromDeliveryInstructions(DeliveryInstructions instructions)
		// 	{
		// 		SetPropertiesFromDeliveryInstructionsCalled = true;
		// 		base.SetPropertiesFromDeliveryInstructions(instructions);
		// 	}
		// }

		public void TestSetPropertiesFromDeliveryInstructionsIsCalled()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.DummyDestinationForTesting;
			DummyMethodForTesting method = (DummyMethodForTesting)DeliveryMethod.FromContact(null, instructions);
			Assert("SetPropertiesFromDeliveryInstructions Called", method.SetPropertiesFromDeliveryInstructionsCalled);
		}

		public void TestAddDeliveryInfos()
		{
			DeliveryMethod method = new DeliveryMethod();

			DeliveryInfo infoA = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			infoA.FileContents.WriteByte((byte)'a');
			method.AddFile(infoA);

			DeliveryInfo infoB = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			infoB.FileContents.WriteByte((byte)'b');
			method.AddFile(infoB);

			AssertEquals("Count", 2, method.DeliveryInfosForTesting.Count);
			AssertEquals("File Count", 2, method.FileCount);
			AssertEquals("Array version length", 2, method.Infos.Count);

			AssertEquals(infoA, method.DeliveryInfosForTesting[0]);
			method.DeliveryInfosForTesting[0].FileContents.Position = 0;
			AssertEquals('a', method.DeliveryInfosForTesting[0].FileContents.ReadByte());

			AssertEquals(infoB, method.DeliveryInfosForTesting[1]);
			method.DeliveryInfosForTesting[1].FileContents.Position = 0;
			AssertEquals('b', method.DeliveryInfosForTesting[1].FileContents.ReadByte());
		}

		[ExpectNoExceptions]
		public void TestDeliver()
		{
			DeliveryMethod method = new DeliveryMethod();
			method.Deliver();
		}

		public void TestFromContact()
		{
			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			DeliveryInstructions instructions;

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.Preview);
			DeliveryMethod returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Excel preview should be returned", returnedMethod is ExcelPreview);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.Print);
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Printer should be returned", returnedMethod is Printer);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.DocManager);
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("DocManager should be returned", returnedMethod is DocManager);
			Assert("Should have SendToEDocs flag set", returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.None);
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Base type should be returned", returnedMethod.GetType() == typeof(DeliveryMethod));
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.Disk);
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Disk should be returned", returnedMethod is Disk);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.DocConfigPreview);
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Excel preview should be returned", returnedMethod is ExcelPreview);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.Auto);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Print should be returned", returnedMethod is Printer);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = "PDF";
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("email should be returned", returnedMethod is Email);
			AssertEquals("PDF should be returned", AttachmentTypeList.Codes.Pdf, ((Email)returnedMethod).AttachmentType);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			contact.AttachmentType = "PDF";
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("email should be returned", returnedMethod is Email);
			AssertEquals("PDF should be returned", AttachmentTypeList.Codes.Pdf, ((Email)returnedMethod).AttachmentType);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			contact.Fax = "123456";
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Fax should be returned", returnedMethod is Fax);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = "Xls";
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Email should be returned", returnedMethod is Email);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			contact.AttachmentType = "Xls";
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Email should be returned", returnedMethod is Email);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Fax should be returned", returnedMethod is Fax);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Printer should be returned", returnedMethod is Printer);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			returnedMethod = DeliveryMethod.FromContact(null, instructions);
			Assert("Printer should be returned", returnedMethod is Printer);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			returnedMethod = DeliveryMethod.FromContact(null, instructions);
			Assert("Printer should be returned", returnedMethod is Printer);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			returnedMethod = DeliveryMethod.FromContact(null, instructions);
			Assert("Printer should be returned", returnedMethod is Printer);
			AssertEquals("Should NOT have SendToEDocs flag set", false, returnedMethod.SendToEDocs);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			Assert("Ftp should be returned", returnedMethod is Ftp);

			instructions = GetDeliveryInstructionsForDestination(DeliveryInstructionDestination.TakenFromContact);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EDoc;
			returnedMethod = DeliveryMethod.FromContact(contact, instructions);
			AssertType<DocManager>("Doc Manager should be returned", returnedMethod);
			AssertEquals("Should not have SendToEDocs flag set", false, returnedMethod.SendToEDocs);
		}

		DeliveryInstructions GetDeliveryInstructionsForDestination(DeliveryInstructionDestination destination)
		{
			return new DeliveryInstructions() { Destination = destination };
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Support for DestinationType <UserCancelled> has not been implemented.")]
		public void TestFromInvalidDestination()
		{
			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			DeliveryInstructions instructions = new DeliveryInstructions();

			instructions.Destination = DeliveryInstructionDestination.UserCancelled;
			DeliveryMethod.FromContact(contact, instructions);
		}

		public void TestXlsShouldBeConvertedToXlsxAutomaticallyIfRegistryItemSet()
		{
			var method = GetDeliveryMethodWithTooManyColumnsTemplates();

			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var mergedStream = method.MergeFilesIntoOneXLS())
			using (var mergedExcelFile = new ExcelInterface())
			{
				mergedExcelFile.LoadExcelFile(mergedStream);
				AssertEquals(TFileFormats.Xlsx, mergedExcelFile.Xls.FileFormatWhenOpened);
			}
		}

		[GuiTest]
		public void TestConfirmationPopupIfNotSwitchToXlsxAutomatically()
		{
			var method = GetDeliveryMethodWithTooManyColumnsTemplates();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			using (Globals.SetIsUserInteractiveForTest(true))
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var mergedStream = method.MergeFilesIntoOneXLS())
			using (var mergedExcelFile = new ExcelInterface())
			{
				mergedExcelFile.LoadExcelFile(mergedStream);

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text,
					ExcelLimitationsHelper.Messages.GetTooManyForExcel2003WithFormatSwitchQuestion(
						new DocumentEngineTooManyColumnsForThisFileFormatException()));
				AssertEquals(TFileFormats.Xlsx, mergedExcelFile.Xls.FileFormatWhenOpened);
				using (Globals.SetIsUserInteractiveForTest(false))
				{
					AssertExceptionThrown<FlexCelException>(() => method.MergeFilesIntoOneXLS());
				}
			}
		}

		DeliveryMethod GetDeliveryMethodWithTooManyColumnsTemplates()
		{
			var info1 = GetDeliveryInfo(true);
			var info2 = GetDeliveryInfo(false);
			var method = new DeliveryMethodThatJustMergesExcelFilesForTesting();
			method.AddFile(info1);
			method.AddFile(info2);
			return method;

			DeliveryInfo GetDeliveryInfo(bool exceeded)
			{
				var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				var format = exceeded ? ExcelFileFormatOptionList.Codes.XLSX : ExcelFileFormatOptionList.Codes.XLS;
				var templateStream = new MemoryStream();
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1, exceeded ? TExcelFileFormat.v2007 : TExcelFileFormat.v2003);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";

					workSheet[3, 0] = "#SectionBody:Data=Collection";
					if (exceeded)
					{
						workSheet[4, FlxConsts.Max_Columns97_2003 + 10] = "<Collection.Z0_Number>";
					}
					else
					{
						workSheet[4, 6] = "<Collection.Z0_Number>";
					}
					workSheet[5, 0] = "#SectionFooter";

					workSheet[6, 0] = "#PageFooter";
					workSheet[7, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream, format);
					info.SetFileContents(templateStream, format);
				}

				return info;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesIntoOneXLS()
		{
			var xlsInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xlsInfo1.SheetNames.Add(new SheetName { StrictName = "Sheet1", EntireName = "Sheet1" });
			xlsInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "NewStyleTemplate_with_Scaling.xls", FileMode.Open, FileAccess.Read), "xls");
			var xlsInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xlsInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "NewStyleTemplate_with_Scaling.xls", FileMode.Open, FileAccess.Read), "xls");

			var tifInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			var tifInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);

			var method = new DeliveryMethod();
			method.AddFile(xlsInfo1);
			method.AddFile(xlsInfo2);
			method.AddFile(tifInfo1);
			method.AddFile(tifInfo2);

			using (var mergedStream = method.MergeFilesIntoOneXLS())
			{
				using (var excelFile = new ExcelInterface())
				{
					excelFile.LoadExcelFile(mergedStream);
					AssertEquals("Merged file has 4 sheets (two merged sheets, the scaling sheet, and the original sheet hidden in the new file used for merging). The TIF files should have been ignored.", 4, excelFile.SheetCount);

					AssertEquals(1, xlsInfo1.SheetNames.Count);
					AssertEquals(1, xlsInfo2.SheetNames.Count);
					AssertEquals(0, tifInfo1.SheetNames.Count);
					AssertEquals(0, tifInfo2.SheetNames.Count);
					AssertEquals("1 - Sheet1", xlsInfo1.SheetNames.First().StrictName);
					AssertEquals("2 - Sheet1", xlsInfo2.SheetNames.First().StrictName);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesIntoOneXLSPreserveSheetProtectionNoSheetProtected()
		{
			var xLSInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "merge1.xls", FileMode.Open, FileAccess.Read), "xls");
			var xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "merge2.xls", FileMode.Open, FileAccess.Read), "xls");

			var method = new DeliveryMethod();
			method.AddFile(xLSInfo1);
			method.AddFile(xLSInfo2);

			using (var mergedStream = method.MergeFilesIntoOneXLS())
			using (var mergedExcelFile = new ExcelInterface())
			{
				mergedExcelFile.LoadExcelFile(mergedStream);

				mergedExcelFile.Xls.ActiveSheet = 1;
				Assert("Sheet 1 should not be password-protected", !mergedExcelFile.Xls.Protection.HasSheetPassword);

				mergedExcelFile.Xls.ActiveSheet = 2;
				Assert("Sheet 2 should not be password-protected", !mergedExcelFile.Xls.Protection.HasSheetPassword);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesIntoOneXLSPreserveSheetProtectionOneSheetProtected()
		{
			var xLSInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "merge1.xls", FileMode.Open, FileAccess.Read), "xls");
			var xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "Merge2_ProtectedSheet.xls", FileMode.Open, FileAccess.Read), "xls");

			var method = new DeliveryMethod();
			method.AddFile(xLSInfo1);
			method.AddFile(xLSInfo2);

			using (var mergedStream = method.MergeFilesIntoOneXLS())
			using (var mergedExcelFile = new ExcelInterface())
			{
				mergedExcelFile.LoadExcelFile(mergedStream);

				mergedExcelFile.Xls.ActiveSheet = 1;
				Assert("Sheet 1 should not be password-protected", !mergedExcelFile.Xls.Protection.HasSheetPassword);

				mergedExcelFile.Xls.ActiveSheet = 2;
				Assert("Sheet 2 should be password-protected", mergedExcelFile.Xls.Protection.HasSheetPassword);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesIntoOneXLSPreserveSheetProtectionAllSheetsProtected()
		{
			var xLSInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "Merge1_ProtectedSheet.xls", FileMode.Open, FileAccess.Read), "xls");
			var xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "Merge2_ProtectedSheet.xls", FileMode.Open, FileAccess.Read), "xls");

			var method = new DeliveryMethod();
			method.AddFile(xLSInfo1);
			method.AddFile(xLSInfo2);

			using (var mergedStream = method.MergeFilesIntoOneXLS())
			using (var mergedExcelFile = new ExcelInterface())
			{
				mergedExcelFile.LoadExcelFile(mergedStream);

				mergedExcelFile.Xls.ActiveSheet = 1;
				Assert("Sheet 1 should be password-protected", mergedExcelFile.Xls.Protection.HasSheetPassword);

				mergedExcelFile.Xls.ActiveSheet = 2;
				Assert("Sheet 2 should be password-protected", mergedExcelFile.Xls.Protection.HasSheetPassword);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesIntoOneXLSWithLongSheetNameCauseDuplicateSheetName()
		{
			var xLSInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "MergeFilesIntoOneXLSWithLongSheetNameCauseDuplicateSheetName.xls", FileMode.Open, FileAccess.Read), "xls");
			var xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "MergeFilesIntoOneXLSWithLongSheetNameCauseDuplicateSheetName.xls", FileMode.Open, FileAccess.Read), "xls");

			var method = new DeliveryMethod();
			method.AddFile(xLSInfo1);
			method.AddFile(xLSInfo2);

			using (var mergedStream = method.MergeFilesIntoOneXLS())
			using (var mergedExcelFile = new ExcelInterface())
			{
				mergedExcelFile.LoadExcelFile(mergedStream);
				var sheetNames = mergedExcelFile.WorkSheets.Select(x => x.SheetName).ToList();

				AssertCollectionContains("1 - GL PL Period Analysis", sheetNames);
				AssertCollectionContains("1 - GL PL Period Analysis With", sheetNames);
				AssertCollectionContains("1 - GL PL Period Analysis Wi(1)", sheetNames);
				AssertCollectionContains("2 - GL PL Period Analysis", sheetNames);
				AssertCollectionContains("2 - GL PL Period Analysis With", sheetNames);
				AssertCollectionContains("2 - GL PL Period Analysis Wi(1)", sheetNames);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesIntoOneTIF()
		{
			var info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			info1.SetFileContents(new FileStream(UnitTestingConstants.TestDocumentScanningFilesPath + "5pages.tif", FileMode.Open, FileAccess.Read), "tif");
			var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			info2.SetFileContents(new FileStream(UnitTestingConstants.TestDocumentScanningFilesPath + "small.JPG", FileMode.Open, FileAccess.Read), "tif");
			var info3 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			info3.SetFileContents(new FileStream(UnitTestingConstants.TestDocumentScanningFilesPath + "small.gif", FileMode.Open, FileAccess.Read), "tif");
			var info4 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info4.SetFileContents(SimpleTestXls, "xls");
			info4.IsCoverSheet = true;

			var method = new DeliveryMethod();
			method.AddFile(info1);
			method.AddFile(info2);
			method.AddFile(info3);
			method.AddFile(info4);

			using (var stream = method.MergeFilesIntoOneTIF())
			{
				AssertEquals("5 pages from first tif, 1 from jpg, 1 from gif and 2 converted from xls cover sheet", 9, GetTotalPages(Image.FromStream(stream)));
			}
		}

		int GetTotalPages(Image img)
		{
			Guid[] supportedDimensions = img.FrameDimensionsList;
			foreach (Guid dimension in supportedDimensions)
			{
				if (dimension.Equals(FrameDimension.Page.Guid))
				{
					return img.GetFrameCount(FrameDimension.Page);
				}
			}
			return 1;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesIntoOneXLSKeepsRowHeights()
		{
			var xLSInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "Merge1.xls", FileMode.Open, FileAccess.Read), "xls");
			var xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			xLSInfo2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "Merge2.xls", FileMode.Open, FileAccess.Read), "xls");

			var method = new DeliveryMethod();
			method.AddFile(xLSInfo1);
			method.AddFile(xLSInfo2);

			using (var mergedStream = method.MergeFilesIntoOneXLS())
			using (var mergedExcelFile = new ExcelInterface())
			{
				mergedExcelFile.LoadExcelFile(mergedStream);
				using (var originalExcelFile = new ExcelInterface())
				{
					originalExcelFile.LoadExcelFile(xLSInfo1.FileContents);

					var rowCount = originalExcelFile.Xls.RowCount;
					for (var i = 1; i <= rowCount; i++)
					{
						AssertEquals("Row heights must be the same for row " + i + " in sheet " + mergedExcelFile.WorkSheets[0].SheetName, originalExcelFile.WorkSheets[0].GetRowHeight(i), mergedExcelFile.WorkSheets[0].GetRowHeight(i));
					}
				}
			}
		}

		public void TestGetFirstXLSDeliveryInfo()
		{
			DeliveryInfo tIFInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			DeliveryInfo tIFInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			DeliveryInfo xLSInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
			DeliveryInfo xLSInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);

			DeliveryMethod method = new DeliveryMethod();
			method.AddFile(tIFInfo);
			method.AddFile(xLSInfo);
			method.AddFile(xLSInfo2);

			DeliveryInfo firstXLSInfo = method.GetFirstXLSDeliveryInfo();
			AssertEquals("First XLS info returned even though tif was first in line", xLSInfo, firstXLSInfo);

			method = new DeliveryMethod();
			method.AddFile(tIFInfo);
			method.AddFile(tIFInfo2);
			AssertEquals("Nothing returned - no xls file in the delivery method", null, method.GetFirstXLSDeliveryInfo());
		}

		public void TestGetFirstXlsDeliveryInfo_ShouldRetrieveDocumentMatchingPrimaryDocPack()
		{
			var menuItem = Factory.New<StmMenuItem>();

			var childTemplatePivot1 = CreateChildTemplateAndPivot(menuItem);
			var childTemplatePivot2 = CreateChildTemplateAndPivot(menuItem);
			var childMenuPivot1 = CreateChildMenuAndPivot(menuItem);
			var childMenuPivot2 = CreateChildMenuAndPivot(menuItem);

			menuItem.SU_PrimaryDocPackItemId = childMenuPivot1.PK;

			var pack = new DocumentPack(menuItem);
			var instructions = new DeliveryInstructions(pack);

			var deliveryInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) { Instructions = instructions, ParentPivotPK = childTemplatePivot1.PK };
			var deliveryInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) { Instructions = instructions, ParentPivotPK = childTemplatePivot2.PK };
			var deliveryInfo3 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) { Instructions = instructions, ParentPivotPK = childMenuPivot1.PK };
			var deliveryInfo4 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) { Instructions = instructions, ParentPivotPK = childMenuPivot2.PK };

			var deliveryMethod = new DeliveryMethod();
			deliveryMethod.AddFile(deliveryInfo1);
			deliveryMethod.AddFile(deliveryInfo2);
			deliveryMethod.AddFile(deliveryInfo3);
			deliveryMethod.AddFile(deliveryInfo4);

			AssertEquals(deliveryInfo3, deliveryMethod.GetFirstXLSDeliveryInfo());
		}

		public void TestGetFirstXlsDeliveryInfoWhenNoPrimaryDocumentSpecified_ShouldRetrieveFirstDeliveryInfo()
		{
			var menuItem = Factory.New<StmMenuItem>();

			var childTemplatePivot1 = CreateChildTemplateAndPivot(menuItem);
			var childTemplatePivot2 = CreateChildTemplateAndPivot(menuItem);
			var childMenuPivot1 = CreateChildMenuAndPivot(menuItem);
			var childMenuPivot2 = CreateChildMenuAndPivot(menuItem);

			var pack = new DocumentPack(menuItem);
			var instructions = new DeliveryInstructions(pack);

			var deliveryInfo1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) { Instructions = instructions, ParentPivotPK = childTemplatePivot1.PK };
			var deliveryInfo2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) { Instructions = instructions, ParentPivotPK = childTemplatePivot2.PK };
			var deliveryInfo3 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) { Instructions = instructions, ParentPivotPK = childMenuPivot1.PK };
			var deliveryInfo4 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document) { Instructions = instructions, ParentPivotPK = childMenuPivot2.PK };

			var deliveryMethod = new DeliveryMethod();
			deliveryMethod.AddFile(deliveryInfo1);
			deliveryMethod.AddFile(deliveryInfo2);
			deliveryMethod.AddFile(deliveryInfo3);
			deliveryMethod.AddFile(deliveryInfo4);

			AssertEquals(deliveryInfo1, deliveryMethod.GetFirstXLSDeliveryInfo());
		}

		StmMenuMenuPivot CreateChildMenuAndPivot(StmMenuItem parent)
		{
			var childMenu = Factory.New<StmMenuItem>();
			var pivot = Factory.New<StmMenuMenuPivot>();
			pivot.SF_SU_Inward = parent.PK;
			pivot.SF_SU_Outward = childMenu.PK;

			return pivot;
		}

		StmMenuTemplatePivot CreateChildTemplateAndPivot(StmMenuItem parent)
		{
			var template = Factory.New<StmTemplate>();
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = parent.PK;
			pivot.SI_SO = template.PK;

			return pivot;
		}

		public void TestCoverSheetIsFirstPage()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			AssertEquals("Precondition: there should be no print jobs in database", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			var docPack = new DocumentPack();
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				using (var report = new Report(docPack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
				{
					docPack.Add(report);
					var instructions = new DeliveryInstructions(docPack);
					instructions.IncludeCoverNote = true;
					instructions.CoverNote = "Test";
					instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					instructions.SetAndSaveDeliveryGroupSubjectLine(docPack, new PrintTask.ReportSubjectLineMapping(null, "test"));

					var contact = instructions.Recipients[0];
					contact.DeliveryMethod = "EML";
					contact.Email = "test@test.com";

					docPack.Run(instructions);
					AssertEquals("Must be 1 print job", 1, Factory.GetDatabaseCount(typeof(StmPrintJob)));

					var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
					AssertNotNull(printJob);

					using (var tmpFile = TempFile.NewWithExtension("xls"))
					{
						using (var stream = new FileStream(tmpFile.Filename, FileMode.Create))
						{
							stream.Write(printJob.SP_CustomProperties, 0, printJob.SP_CustomProperties.Length);
						}

						using (var excel = new ExcelInterface())
						{
							excel.LoadExcelFile(tmpFile.Filename);
							AssertEquals("First page must be 'Cover Sheet'", "Email Cover Sheet", excel.WorkSheets[0].SheetName);
						}
					}
				}
			}
		}

		public void TestCloseDeliverInfos()
		{
			var deliveryMethod = new DeliveryMethod();
			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			using (var stream = new MemoryStream())
			{
				var isOpenField = typeof(MemoryStream).GetField("_isOpen", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertNotNull("Reflection correcteness", isOpenField);

				deliveryInfo.SetFileContents(stream, "xxx");
				deliveryMethod.AddFile(deliveryInfo);

				AssertNotNull(deliveryInfo.FileContents);
				AssertEquals(true, isOpenField.GetValue(stream));

				deliveryMethod.ReleaseDeliveryInfos();

				AssertNull(deliveryInfo.FileContents);
				AssertEquals(false, isOpenField.GetValue(stream));
			}
		}

		protected override void SetUp()
		{
			originalGlobalsIsUserInteractive = Globals.IsUserInteractive;
			originalGlobalsIsWeb = Globals.IsWeb;

			Globals.IsUserInteractive = true;
			Globals.IsWeb = false;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbGroup postMasterGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";

			GlbStaff templateAuthor = factory.New<GlbStaff>();
			templateAuthor.GS_EmailAddress = "templateAuthor@sample.org";
			templateAuthor.GS_Code = "_X_";
			templateAuthor.GS_LoginName = "templateAuthorSample";
			factory.Save();
		}

		protected override void TearDown()
		{
			Globals.IsUserInteractive = originalGlobalsIsUserInteractive;
			Globals.IsWeb = originalGlobalsIsWeb;
		}

		bool originalGlobalsIsUserInteractive;
		bool originalGlobalsIsWeb;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		Stream SimpleTestXls => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SimpleTest.xls");

		sealed class DeliveryMethodThatJustMergesExcelFilesForTesting : DeliveryMethod
		{
			protected override void DeliverCore(INotifications notifications = null)
			{
				MergeFilesIntoOneXLS();
			}
		}

		sealed class DummyDeliveryMethod : DeliveryMethod
		{
			internal bool IsDelivered;

			protected override void DeliverCore(INotifications notifications = null)
			{
				IsDelivered = true;
			}
		}
	}
}
