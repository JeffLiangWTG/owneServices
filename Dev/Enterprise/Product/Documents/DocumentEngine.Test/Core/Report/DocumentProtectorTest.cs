using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	public class DocumentProtectorTest : TransactionedTestCase
	{
		public void TestPasswordProtecter()
		{
			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[SheetNameOverride=SheetNameOverride1]
{A}-[#EndOfReport]
");

			content.Add("Sheet2",
				@"{A}-[#Config]
{A}-[SheetNameOverride=SheetNameOverride2]
{A}-[#EndOfReport]
");
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, content);
			var factory = new BusinessObjectFactory();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSC";
			staff.GS_FullName = "StaffFullName";
			staff.ExcelPasswordForModifying = "ModifyingPWD";
			staff.ExcelPasswordForOpening = "OpeningPWD";
			factory.Save();

			using (var documentPack = new DocumentPack())
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(documentPack, excelTemplate, new DataProviderList(new DummyDocWrapper()), "Some Document", null, DocumentDirection.ANY, true, true, null))
			using (SystemDataRegistry.Instance.AllocatePasswordProtectedExcelSpreadsheets.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = new DocDeliveryContact(factory);
				contact.StaffCode = staff.GS_Code;
				contact.AttachmentType = "XLS";

				using (var outputStream = new MemoryStream())
				using (var excelInterface = new ExcelInterface())
				{
					report.Save(contact, contact, outputStream);
					excelInterface.LoadExcelFile(outputStream);

					AssertEquals(2, excelInterface.Xls.SheetCount);

					for (var i = 1; i <= excelInterface.Xls.SheetCount; i++)
					{
						excelInterface.Xls.ActiveSheet = i;
						AssertEquals(true, excelInterface.Xls.Protection.HasSheetPassword);
					}
				}

				var printJob = factory.NewWithValidTestData<StmPrintJob>();
				printJob.SP_DocumentType = "SOA";
				printJob.SP_JobType = "DDS";
				printJob.SP_EmailAttachmentFormat = "XLSX";
				report.Protector.PasswordProtectForOpening(printJob, "test.xlsx");

				AssertEquals("OpeningPWD", TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(printJob.SP_ExcelEncryptedPassword));
				AssertEquals($"SOA|test.xlsx|Allocated with the Staff StaffFullName both open and modify password.|{printJob.PK}", report.Protector.ExcelProtectedInfoForAddingEvent);
			}
		}

		public void TestEncryptExcelFileOpeningAccess()
		{
			var filePath = PathValidation.GetFilePathWithValidLength(StmPrintJob.GetUniqueHumanReadableFilePath(Temp.TempPath, Path.GetFileNameWithoutExtension("TestEncryptExcelFileOpeningAccess"), "." + "xlsx"));
			var openPassword = "13579";
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				excelInterface.SaveToFile(filePath);
				DocumentProtector.EncryptExcelFileOpeningAccess(filePath, TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(openPassword));
			}

			try
			{
				using (var excelInterface = new ExcelInterface())
				{
					AssertExceptionThrown<ExcelInterfaceException>(message: "Encrypted Excel can not be opened without password",
						codeToRun: () =>
						{
							excelInterface.LoadExcelFile(filePath);
						});

					excelInterface.Xls.Protection.OpenPassword = openPassword;
					AssertNoExceptionThrown(message: "Encrypted Excel can be opened with password", codeToRun: () =>
					{
						excelInterface.LoadExcelFile(filePath);
					});
				}
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		public void TestGetExcelProtectedInfoForAddingEvent()
		{
			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[SheetNameOverride=SheetNameOverride1]
{A}-[#EndOfReport]
");
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, content);
			var factory = new BusinessObjectFactory();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TSC";
			staff.GS_FullName = "StaffFullName";
			staff.ExcelPasswordForModifying = "ModifyingPWD";
			staff.ExcelPasswordForOpening = "OpeningPWD";
			factory.Save();

			using (var documentPack = new DocumentPack())
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(documentPack, excelTemplate, new DataProviderList(new DummyDocWrapper()), "Some Document", null, DocumentDirection.ANY, true, true, null))
			using (SystemDataRegistry.Instance.AllocatePasswordProtectedExcelSpreadsheets.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contact = new DocDeliveryContact(factory);
				contact.StaffCode = staff.GS_Code;

				using (var outputStream = new MemoryStream())
				using (var excelInterface = new ExcelInterface())
				{
					report.Save(contact, contact, outputStream);
				}

				var printJob = factory.NewWithValidTestData<StmPrintJob>();
				printJob.SP_DocumentType = "SOA";
				printJob.SP_JobType = "PRN";
				report.Protector.PasswordProtectForOpening(printJob, "test.xlsx");

				AssertEquals("Excel Protected Info should be empty", ZString.Empty, report.Protector.ExcelProtectedInfoForAddingEvent);
			}
		}
	}
}
