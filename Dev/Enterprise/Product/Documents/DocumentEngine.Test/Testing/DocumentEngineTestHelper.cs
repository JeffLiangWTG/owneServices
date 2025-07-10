using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	public class DocumentEngineTestHelper : DocumentEngineTestHelperBase
	{
		public static void ClearTemplates()
		{
			TestCaseHelper.ClearTable(StmMenuDocumentConfigItem.Schema.TableName);
			TestCaseHelper.ClearTable(StmMenuDocumentConfig.Schema.TableName);
			TestCaseHelper.ClearTable(StmMenuTemplatePivot.Schema.TableName);
			TestCaseHelper.ClearTable(StmTemplate.Schema.TableName);
		}

		internal static Report GetNewReportWithNoExceptionOnErrors(BusinessObjectFactory factory, ExcelTemplate excelTemplate)
		{
			DocumentCommand stmMenuItem = factory.New<DocumentCommand>();
			DocumentPack pack = new DocumentPack(stmMenuItem);
			Report report = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest);
			((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
			return report;
		}

		internal ExcelWorkSheet GetEvaluatedWorkSheet(string cellValue, BusinessObject parent)
		{
			ExcelWorkSheet result = null;

			DocumentCommand documentCommand = parent.Factory.New<DocumentCommand>();

			using (Stream templateStream = GetNewTemplateStream(cellValue))
			{
				using (DocumentPack pack = new DocumentPack(documentCommand))
				{
					ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

					using (Report report = new Report(pack, excelTemplate, BODocDataProvider.Get(parent), cellValue, ContactType.NoContactType, null, DocumentDirection.ANY, false))
					{
						using (MemoryStream saveStream = new MemoryStream())
						{
							report.Save(saveStream);

							using (ExcelInterface excelInterface = new ExcelInterface())
							{
								excelInterface.LoadExcelFile(saveStream);
								result = excelInterface.WorkSheets[0];
							}
						}
					}
				}
			}

			return result;
		}

		internal string GetEvaluatedCellValue(string cellValue, BusinessObject parent)
		{
			string result = "";

			DocumentCommand documentCommand = parent.Factory.New<DocumentCommand>();

			using (Stream templateStream = GetNewTemplateStream(cellValue))
			{
				using (DocumentPack pack = new DocumentPack(documentCommand))
				{
					ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

					using (Report report = new Report(pack, excelTemplate, BODocDataProvider.Get(parent), cellValue, ContactType.NoContactType, null, DocumentDirection.ANY, false))
					{
						using (MemoryStream saveStream = new MemoryStream())
						{
							report.Save(saveStream);

							using (ExcelInterface excelInterface = new ExcelInterface())
							{
								excelInterface.LoadExcelFile(saveStream);
								result = excelInterface.WorkSheets[0][0, 1].ToString();
							}
						}
					}
				}
			}

			return result;
		}

		internal Stream GetNewTemplateStream(string cellValue)
		{
			MemoryStream templateStream = new MemoryStream();

			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=TemplateFromStream";
				workSheet[2, 0] = "#SectionBody";
				workSheet[3, 1] = cellValue;
				workSheet[4, 0] = "#EndOfReport";

				excelInterface.SaveToStream(templateStream);
			}

			return templateStream;
		}

		internal static OrgHeader GetNewOrganization(string name, BusinessObjectFactory factory)
		{
			OrgHeader result = factory.New<OrgHeader>();
			result.OH_Code = name;
			result.OH_FullName = name + "Company";
			return result;
		}

		internal static OrgHeader GetNewOrganizationWithMainAddress(string name, BusinessObjectFactory factory)
		{
			var result = GetNewOrganization(name, factory);

			FillInAddress(name, result.Addresses.MainAddress);

			var contact = result.Contacts.AddNew();
			contact.OC_ContactName = name;
			contact.OC_Email = "org.contact@test.com";

			return result;
		}

		internal static OrgAddress AddAddress(string name, OrgHeader organisation, OrgAddressType addressType)
		{
			var result = organisation.Addresses.AddNew(addressType, true);
			result.OA_CompanyNameOverride = organisation.OH_FullName + " t/as " + name;
			FillInAddress(name, result);
			return result;
		}

		static void FillInAddress(string name, OrgAddress address)
		{
			address.OA_Address1 = name + "Address1";
			address.OA_Address2 = name + "Address2";
			address.OA_City = name + "Ville";
			address.OA_Email = "org.address@test.com";
			address.OA_State = name.ToUpper().Substring(0, 3);
			address.OA_PostCode = "9" + address.OA_State + "9";
			address.OA_RL_NKRelatedPortCode = "US" + address.OA_State;
		}

		internal static DocDeliveryContact GetNewDocDeliveryContact(string name, BusinessObjectFactory factory)
		{
			OrgHeader organization = GetNewOrganization(name, factory);
			return GetNewDocDeliveryContact(name, organization, factory);
		}

		internal static DocDeliveryContact GetNewDocDeliveryContact(string name, OrgHeader organization, BusinessObjectFactory factory)
		{
			OrgAddress address = organization.Addresses.AddNew();
			address.OA_Address1 = name + "Address1";
			address.OA_Address2 = name + "Address2";
			address.OA_City = name + "Ville";
			address.OA_State = name.ToUpper().Substring(0, 3);
			address.OA_PostCode = "9" + address.OA_State + "9";
			address.OA_RL_NKRelatedPortCode = "US" + address.OA_State;

			OrgContact contact = organization.Contacts.AddNew();
			contact.OC_ContactName = name;

			return GetNewDocDeliveryContact(name, organization, address, factory);
		}

		internal static DocDeliveryContact GetNewDocDeliveryContact(string name, OrgHeader organization, OrgAddress address, BusinessObjectFactory factory)
		{
			var result = new DocDeliveryContact(factory);
			result.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			result.Email = "unit.test@cw1.com";
			result.Name = name;
			result.OrgHeaderPK = organization.PK;
			result.CompanyName = organization.OH_FullName;
			result.Address1 = address.OA_Address1;
			result.Address2 = address.OA_Address2;
			result.City = address.OA_City;
			result.PostCode = address.OA_PostCode;
			result.State = address.OA_State;
			result.UNLOCO = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, address.OA_RL_NKRelatedPortCode));

			return result;
		}

		internal static ReportCommand CreateReportCommandWithExcelTemplate(ZString menuName, string docType, string referenceType, ExcelTemplate excelTemplate, BusinessObjectFactory factory)
		{
			ReportCommand result = factory.New<ReportCommand>();
			result.SU_MenuName = menuName;

			StmTemplateBase template = factory.New<StmTemplateBase>();
			template.SO_Name = ZString.Format("{0} Template", result.SU_MenuName);
			template.SO_Template = excelTemplate.GetAsByteArray();

			StmMenuTemplatePivotBase menuTemplatePivot = factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_DocumentTitle = result.SU_MenuName;
			menuTemplatePivot.SI_SU = result.PK;
			menuTemplatePivot.SI_SO = template.PK;

			if (!string.IsNullOrEmpty(docType))
			{
				var refDocType = factory.New<RefDocType>();
				refDocType.RT_SE_NKDocumentReceivedEvent = Events.ExportReceivalAdvisePrinted.Code;
				refDocType.RT_DocType = docType;
				refDocType.RT_ReferenceType = string.IsNullOrEmpty(referenceType) ? "ALL" : referenceType;
				menuTemplatePivot.SI_RT_DocType = refDocType.PK;
			}

			return result;
		}

		public static ReportCommand CreateReportCommandWithExcelTemplate(ZString menuName, ExcelTemplate excelTemplate, BusinessObjectFactory factory)
		{
			return CreateReportCommandWithExcelTemplate(menuName, string.Empty, string.Empty, excelTemplate, factory);
		}

		internal static void GenerateTemplateStreamFromStringAndSavetoFile(Stream stream, string contents, string filePath)
		{
			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				SaveContentToStream(stream, contents, excelInterface);
				excelInterface.SaveToFile(filePath);
			}
		}

		internal static void GenerateTemplateStream(Stream stream, List<KeyValuePair<string, string>> sheetContents)
		{
			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(sheetContents.Count);

				for (int sheetIndex = 0; sheetIndex < sheetContents.Count; sheetIndex++)
				{
					GenerateExcelWorkSheetFromString(excelInterface.WorkSheets[sheetIndex], sheetContents[sheetIndex].Value);
				}

				excelInterface.Xls.PrintPaperSize = TPaperSize.A4;
				excelInterface.SetOrientation(Orientation.Portrait);

				for (int sheetIndex = 0; sheetIndex < sheetContents.Count; sheetIndex++)
				{
					var worksheet = excelInterface.WorkSheets[sheetIndex];
					worksheet.SheetNameOverride = sheetContents[sheetIndex].Key;
					worksheet.UpdateSheetName();
				}

				excelInterface.SaveToStream(stream);
			}
		}

		public static byte[] CreateTemplateFromStringAndSaveToFile(string contents, string filePath)
		{
			byte[] result = null;

			using (var stream = new MemoryStream())
			{
				GenerateTemplateStreamFromStringAndSavetoFile(stream, contents, filePath);
				result = stream.CopyToByteArray();
			}

			return result;
		}

		public static StmTemplateBase CreateTemplateFromString(BusinessObjectFactory factory, ZString name, string contents)
		{
			return CreateTemplateFromString(factory, name, contents, nameof(Enterprise.Core.Constants.DataContext.UnitTest));
		}

		public static StmTemplateBase CreateTemplateFromString(BusinessObjectFactory factory, ZString name, string contents, string dataContext)
		{
			var result = factory.New<StmTemplateBase>();

			result.SO_Name = name;
			result.SO_Template = CreateTemplateFromString(contents);
			result.SO_DataContext = dataContext;

			return result;
		}

		internal static Report CreateReportFromExcelTemplateContents(DocumentPack documentPack, Stream templateStream, string contents)
		{
			GenerateTemplateStreamFromString(templateStream, contents);
			var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);
			return new Report(documentPack, excelTemplate);
		}

		internal static Report CreateReportFromExcelTemplateContents(DocumentPack documentPack, IBODocDataProvider docDataProvider, Stream templateStream, string contents)
		{
			GenerateTemplateStreamFromString(templateStream, contents);
			var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);
			return new Report(documentPack, excelTemplate, docDataProvider, "Test", null, DocumentDirection.ANY, false);
		}

		public static ExcelTemplate CreateExcelTemplateFromString(string templateName, string templateSourceLocation, Dictionary<string, string> templateContent)
		{
			var templateTestHelper = new TemplateTestHelper();
			foreach (var worksheetName in templateContent.Keys)
			{
				templateTestHelper.AddWorkSheet(worksheetName, templateContent[worksheetName]);
			}
			var templateBlob = templateTestHelper.CreateTemplateBlob();
			return new ExcelTemplateReadFromByteArray(templateName, templateSourceLocation, templateBlob);
		}

		internal static ExcelTemplate CreateExcelTemplateFromString(string templateName, string templateSourceLocation, string contents)
		{
			using (var stream = new MemoryStream())
			{
				GenerateTemplateStreamFromString(stream, contents);
				return new ExcelTemplateReadFromByteArray(templateName, templateSourceLocation, stream.CopyToByteArray());
			}
		}

		internal static ExcelWorkSheet CreateExcelWorkSheetFromString(string contents)
		{
			ExcelWorkSheet result = null;

			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				excelInterface.ActiveWorksheet = 0;
				result = excelInterface.WorkSheets[0];
				GenerateExcelWorkSheetFromString(result, contents);
			}

			return result;
		}

		internal static string ExecuteDocumentCommand(DocumentCommand documentCommand, IDocumentSupportable parent)
		{
			return ExecuteDocumentCommand(documentCommand, parent, null).OutputAsString;
		}

		internal class ExecuteDocumentCommandResult
		{
			internal ZBlob Output;
			internal string OutputAsString;
		}

		internal static ExecuteDocumentCommandResult ExecuteDocumentCommand(DocumentCommand documentCommand, IDocumentSupportable parent, UserControlProviderList userFieldList)
		{
			return ExecuteDocumentCommand(documentCommand, parent, userFieldList, null);
		}

		internal static ExecuteDocumentCommandResult ExecuteDocumentCommand(DocumentCommand documentCommand, IDocumentSupportable parent, UserControlProviderList userFieldList, string languageCode)
		{
			var result = new ExecuteDocumentCommandResult();

			documentCommand.Parent = parent;

			using (var printSet = new DocumentPrintSet(documentCommand, userFieldList))
			{
				var factory = documentCommand.Factory;
				var deliveryInstructions = new DeliveryInstructions();
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				recipient.Email = "unit.test@cargowise.com";
				recipient.AttachmentType = OrgConstants.AttachmentType.XLS;

				if (!string.IsNullOrEmpty(languageCode))
				{
					deliveryInstructions.Language = languageCode;
				}

				var query = new ZQuery();
				var printJobs = new StmPrintJobCollection(factory);
				printJobs.Load();
				Assertion.AssertEquals("Pre-condition: There should be no print jobs.", 0, printJobs.Count);
				printSet.Run(deliveryInstructions);
				printJobs.Load();
				Assertion.AssertEquals("There should only be 1 print job.", 1, printJobs.Count);
				var printJob = printJobs[0];

				result.Output = printJob.SP_CustomProperties;

				using (var excelFile = new ExcelInterface())
				{
					using (var stream = new MemoryStream(printJob.SP_CustomProperties))
					{
						excelFile.LoadExcelFile(stream);
					}

					result.OutputAsString = excelFile.WorkSheets[0].ToString();
				}

				printJobs.RemoveAndDeleteAll();
			}

			return result;
		}

		internal static string GetTemplateContents(StmTemplate template)
		{
			return GetTemplateContents(template.SO_Template);
		}

		internal static string GetTemplateContents(byte[] template)
		{
			var result = new ZStringBuilder();

			using (var excelInterface = new ExcelInterface())
			{
				using (var stream = new MemoryStream(template))
				{
					excelInterface.LoadExcelFile(stream);

					foreach (var workSheet in excelInterface.WorkSheets)
					{
						result.Append(workSheet.ToString());
					}
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		internal static TemplateSection FindTemplateSection(TemplateSectionCollection templateSections, ZString sectionType, ZString sectionName)
		{
			TemplateSection result = null;

			foreach (TemplateSection templateSection in templateSections)
			{
				if (templateSection.TypeCode.EqualsIgnoringCase(sectionType) && templateSection.SectionName.EqualsIgnoringCase(sectionName))
				{
					result = templateSection;
					break;
				}
			}

			return result;
		}

		internal static string GetPrintTitlesRangeFormula(ExcelInterface excelInterface, int sheetIndex)
		{
			var result = string.Empty;
			int workSheetNumber = excelInterface.WorkSheets[sheetIndex].WorkSheetNumber;
			var namedRange = excelInterface.Xls.GetNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), workSheetNumber);
			if (namedRange != null)
			{
				result = namedRange.RangeFormula;
			}

			return result;
		}
	}
}
