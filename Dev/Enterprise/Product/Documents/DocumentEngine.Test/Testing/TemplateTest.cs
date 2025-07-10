using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DbUpgrader.Data;
using Enterprise.DbUpgrader.Data.Testing;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Build;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	public sealed class TemplateTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSystemDefinedDocumentTypeMeetWithStandard()
		{
			var hasErrors = TemplateSerializationHelper.SystemRefDocTypeHasErrors(Path.Combine(TestFileConstants.DefaultDocumentsDataFileBasePath, @"Documents\Documents.xml"), out var message);
			Assert(message, !hasErrors);
		}

		public void TestClientSpecificMenuItemsForOldUsageOf_GlbCompany_CountryCode()
		{
			var results = new List<string>();

			foreach (string documentXmlPath in GetClientDocumentXmlPaths())
			{
				try
				{
					ClientDocumentsDataFile dataFile = new ClientDocumentsDataFile(documentXmlPath);
					using (DataSet set = dataFile.DataSet)
					{
						DataTable table = set.Tables[StmMenuItemSchema.Constants.TableName];
						if ((table != null) && (table.Rows.Count > 0))
						{
							string clientCode = Path.GetFileName(documentXmlPath).Substring(0, 3);
							foreach (DataRow row in table.Rows)
							{
								string filter = row[StmMenuItemSchema.Constants.SU_FilterList].ToString();
								if (filter.Contains("Company.CountryCode."))
								{
									results.Add(clientCode + " - " + row[StmMenuItemSchema.Constants.SU_MenuName].ToString() + " - " + filter);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					throw new InvalidOperationException(string.Format("Failure Reading: [{0}]", documentXmlPath), ex);
				}
			}

			AssertMultilineASCIIEquals("Should be no Menu Items with a filter containing 'Company.CountryCode.Code'", "", string.Join("\r\n", results.ToArray()));
		}

		[StressTest]
		public void TestAllUDFsDefinedOnTemplatesAreUsedOnThoseTemplates()
		{
			var result = new ZStringBuilder();
			var allTemplates = GetAllTemplates();

			foreach (var template in allTemplates)
			{
				try
				{
					var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
					using (var report = new Report(new DocumentPack(), excelTemplate))
					{
						var udfFields = new List<UDFFieldDefinition>();

						var udfWorksheet = report.UDFSheet;
						if (udfWorksheet != null)
						{
							var udfRootNode = new StringTreeBuilder(udfWorksheet).GetTree();

							foreach (var node in udfRootNode.Children)
							{
								udfFields.Add(new UDFFieldDefinition(node.Value));
							}

							var excelInterface = report.XlInterface;
							for (var workSheetIndex = 0; workSheetIndex < excelInterface.WorkSheets.Count; workSheetIndex++)
							{
								var workSheet = excelInterface.WorkSheets[workSheetIndex];
								if (Report.IsTemplateSheet(workSheet.SheetName))
								{
									var sheetAsString = workSheet.ToString(new CellFormatterExposingFormulae()).ToLowerInvariant();
									foreach (var udfField in udfFields)
									{
										udfField.IsUsed |= sheetAsString.Contains(udfField.FieldTag);
									}
								}
							}

							foreach (var udfField in udfFields)
							{
								if (!udfField.IsUsed)
								{
									result.Append(string.Format("{0} defines [{1}]", excelTemplate.TemplateSourceLocation, udfField.FieldName));
								}
							}
						}
					}
				}
				catch (Exception exception)
				{
					if (exception is InvalidOperationException)
					{
						throw new Exception("Exception Occurred processing Template: " + template.ExcelTemplateFullPath, exception);
					}
				}
			}

			Assert("The following templates define UDF Fields on the UDF Tab but do not use them on the template:-\r\n\r\n" + result.ToStringWithNewLineBetweenAppends(), result.IsEmpty);
		}

		[StressTest]
		public void TestCountMacroUsedCorrectly()
		{
			var result = new ZStringBuilder();
			var countRegex = new Regex(@"<\s*Count\s*\(\s*([^\s.,]+)?\s*(?:,([^,.]+))?\s*\)\s*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var allTemplates = GetAllTemplates();

			foreach (var template in allTemplates)
			{
				try
				{
					using (var excelInterface = new ExcelInterface())
					using (var stream = template.GetSO_TemplateReader())
					{
						excelInterface.LoadExcelFile(stream.ToByteArray());

						foreach (var worksheet in excelInterface.WorkSheets)
						{
							var isInGroupArea = false;
							if (Report.IsTemplateSheet(worksheet.SheetName))
							{
								var startOfFirstAreaAfterConfig = GetStartOfFirstAreaAfterConfig(worksheet);
								for (int row = startOfFirstAreaAfterConfig + 1; row < worksheet.RowCount; row++)
								{
									var area = worksheet[row, 0].ToString();
									if (area.StartsWith("#GroupBy", StringComparison.InvariantCultureIgnoreCase))
									{
										isInGroupArea = true;
										continue;
									}

									if (area.StartsWith("#"))
									{
										isInGroupArea = false;
									}

									if (isInGroupArea)
									{
										for (var col = 0; col < worksheet.ColumnCount; col++)
										{
											var cellValue = worksheet[row, col].ToString().Trim();
											if (!string.IsNullOrEmpty(cellValue))
											{
												var matches = countRegex.Matches(cellValue);
												foreach (Match match in matches)
												{
													var countMacro = match.Value;
													var parameters = countRegex.Split(countMacro).Where(s => !string.IsNullOrEmpty(s)).ToList();
													var tableName = parameters.Count > 0 ? parameters[0].Trim() : string.Empty;
													if (!string.IsNullOrEmpty(tableName))
													{
														result.Append(
															$"[{template.ExcelTemplateFullPath}] - {worksheet.SheetName} (MenuName: {template.SO_Name} | DataContext: {template.SO_DataContext}): Row[{row + 1}] Cell[{col + 1}] >> [{cellValue}]");
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Exception Occurred processing Template: " + template.ExcelTemplateFullPath, ex);
				}
			}

			Assert("The following templates are using <Count> macro with a table name specified in a #GroupBy Area, which will return the number of table rows instead of group rows. Please remove the table name parameter.\r\n\r\n" + result.ToStringWithNewLineBetweenAppends(), result.IsEmpty);
		}

		[StressTest]
		public void TestReportNameMacroUsedCorrectly()
		{
			var result = new ZStringBuilder();
			const string reportNameMacroPattern = "(?:[\\s]*)\"<(?:[\\s]*)Report(?:[\\s]*)Name(?:[\\s]*)(Short)?>\"";
			const string reportNameAtTheBeginningPattern = @"If(?:[\s]*)\(?(?:[\s]*)!?" + reportNameMacroPattern;
			const string reportNameInTheMiddlePattern = @"If(?:[\s]*)\(?.*[!&|]+(?:[\s]*)\(*" + reportNameMacroPattern;
			var regexToFindReportNameUsedAtTheBeginningOfFilter = new Regex(reportNameAtTheBeginningPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var regexToFindReportNameUsedInTheMiddleOfFilter = new Regex(reportNameInTheMiddlePattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var allTemplates = GetAllTemplates();

			foreach (var template in allTemplates)
			{
				try
				{
					using (var excelInterface = new ExcelInterface())
					using (var stream = template.GetSO_TemplateReader())
					{
						excelInterface.LoadExcelFile(stream.ToByteArray());

						foreach (var workSheet in excelInterface.WorkSheets)
						{
							if (Report.IsTemplateSheet(workSheet.SheetName))
							{
								var startOfFirstAreaAfterConfig = GetStartOfFirstAreaAfterConfig(workSheet);
								for (var row = startOfFirstAreaAfterConfig + 1; row < workSheet.RowCount; row++)
								{
									for (var col = 0; col < workSheet.ColumnCount; col++)
									{
										var cellValue = workSheet[row, col].ToString().Trim();
										if (!string.IsNullOrEmpty(cellValue) && (regexToFindReportNameUsedAtTheBeginningOfFilter.IsMatch(cellValue) || regexToFindReportNameUsedInTheMiddleOfFilter.IsMatch(cellValue)))
										{
											result.Append(string.Format("[{0}] - {1} (MenuName: {2} | DataContext: {3}): Row[{4}] Cell[{5}] >> [{6}]", template.ExcelTemplateFullPath, workSheet.SheetName, template.SO_Name, template.SO_DataContext, row + 1, col + 1, cellValue));
										}
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Exception Occurred processing Template: " + template.ExcelTemplateFullPath, ex);
				}
			}

			Assert("The following templates are using <ReportName> in a filter, the result may not be as expected when running in non-English languages as <ReportName> will be translated. \r\nPlease use <ReportNameUntranslated> instead, which will not be translated. \r\n\r\n" + result.ToStringWithNewLineBetweenAppends(), result.IsEmpty);
		}

		public void TestOrganisationMultiSelectionLookupFieldHasChildSerialisedByPK()
		{
			var result = new ZStringBuilder();

			foreach (var template in GetAllTemplates(false, excludeDocuments: true))
			{
				try
				{
					using (var excelInterface = new ExcelInterface())
					using (var stream = template.GetSO_TemplateReader())
					{
						var filters = new List<string>();

						excelInterface.LoadExcelFile(stream.ToByteArray());
						foreach (var workSheet in excelInterface.WorkSheets)
						{
							if (!Report.IsFilterSheetName(workSheet.SheetName))
							{
								continue;
							}

							var root = new StringTreeBuilder(workSheet).GetTree();
							foreach (var filterDef in root.Children)
							{
								var typeNode = filterDef.FindChild("type").Child();
								if (MultipleSelectionLookupBuilder.Regex.IsMatch(typeNode.Value))
								{
									var lookupName = Regex.Match(typeNode.Value, @"(.*)\s+" + MultipleSelectionLookupBuilder.regularExpressionToMatchFilterType, RegexOptions.IgnoreCase).Groups[1].Value;
									var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, lookupName);
									if (collectionProvider != null && collectionProvider.ModuleID == ModuleIDs.Organisation)
									{
										if (!filterDef.ChildExists(FilterBuilderPropertyCodeDescriptionList.Codes.SerialisedByPK))
										{
											filters.Add(filterDef.Value);
										}
									}
								}
							}
						}

						if (filters.Count > 0)
						{
							result.AppendFormat("[{0}] - Filter(s) (TemplateName: {1}, Filters: [{2}])", template.ExcelTemplateFullPath, template.SO_Name, string.Join(",", filters));
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					throw new Exception("Exception Occurred processing Template: " + template.ExcelTemplateFullPath, ex);
				}
			}

			Assert("The following templates' Filters are using Organisation Multiple Selection Lookup referencing OrgCode, please use SerialisedByPK option of these filters which will reference to Org PK. \r\n\r\n" + result.ToStringWithNewLineBetweenAppends(), result.IsEmpty);
		}

		[DeveloperOnlyTest]
		public void TestClientTemplatesAreInCorrectFolders()
		{
			var result = new ZStringBuilder();
			var clientCodeRegex = new Regex(@"Enterprise\\ClientExtensions\\(?<text>\w{3})\\Documents\\", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
			var clientTemplatesFromXML = GetClientFilesOnly();
			foreach (var clientTemplateFromXML in clientTemplatesFromXML)
			{
				var match = clientCodeRegex.Match(clientTemplateFromXML.SourceXMLFileName);
				if (!match.Success)
				{
					Fail("Could not find a client code in " + clientTemplateFromXML.SourceXMLFileName);
				}

				var clientCode = match.Groups["text"].Value;
				var expectedTemplatePath1 = string.Format(@"Enterprise\Product\Documents\ExcelTemplates\Documents\{0}\", clientCode);
				var expectedTemplatePath2 = string.Format(@"Enterprise\Product\Documents\ExcelTemplates\Reports\{0}\", clientCode);
				var actualTemplatePath = clientTemplateFromXML.Template.SO_ExcelTemplatePath;

				if (!actualTemplatePath.StartsWith(expectedTemplatePath1, StringComparison.InvariantCultureIgnoreCase) && !actualTemplatePath.StartsWith(expectedTemplatePath2, StringComparison.InvariantCultureIgnoreCase))
				{
					var shortXmlFileName = clientCode + "Documents.XML";
					result.Append(string.Format(@"Templates in {0} should reside under: [ExcelTemplates\(Documents || Reports)\{1}\] Template path found: [{2}]", shortXmlFileName, clientCode, actualTemplatePath));
				}
			}

			Assert("Client Templates should reside under Client Document Folders.\r\n\r\n" + result.ToStringWithNewLineBetweenAppends(), result.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentTemplatesHaveSameTableSchemaAsInDatabase()
		{
			var result = new ZStringBuilder();
			var dataFiles = new List<DataFile>();
			string errorColumns = "";

			dataFiles.Add(new DocumentsDataFile());
			foreach (string documentXmlPath in GetClientDocumentXmlPaths())
			{
				dataFiles.Add(new ClientDocumentsDataFile(documentXmlPath));
			}

			foreach (var dataFile in dataFiles)
			{
				var fileData = dataFile.LoadDataFromFile();
				var databaseData = dataFile.LoadDataFromDatabase();
				for (int i = 0; i < dataFile.TableNames.Length; i++)
				{
					var sourceTableName = dataFile.TableNames[i];
					var sourceTable = fileData.Tables[sourceTableName];
					var targetTable = databaseData.Tables[sourceTableName];
					if (sourceTable != null)
					{
						if (targetTable == null || !Compare(sourceTable, targetTable, out errorColumns))
						{
							result.Append(dataFile.FileFullPath);
							result.Append(errorColumns);
							result.Append("");
						}
					}
				}
			}

			Assert("The following client documents have a different table schema as in database\r\n\r\n" + result.ToStringWithNewLineBetweenAppends(), result.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentXmlFilesAreCompatibleWithInnerSchema()
		{
			var fullPath = Path.Combine(TestFileConstants.DefaultDocumentsDataFileBasePath, @"Documents\Documents.xml");
			AssertXmlFilesAreCompatibleWithInnerSchema(new List<string>() { fullPath });
		}

		public void TestClientSpecificXmlFilesAreCompatibleWithInnerSchema()
		{
			AssertXmlFilesAreCompatibleWithInnerSchema(GetClientDocumentXmlPaths());
		}

		[StressTest]
		public void TestUDFsArentDefinedDifferentlyOnDifferentTemplates()
		{
			var validatorPack = new ValidatorPack();
			var fields = new Dictionary<string, UDFField>();

			var allTemplates = GetAllTemplates(true);

			foreach (var template in allTemplates)
			{
				try
				{
					var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
					using (var report = new Report(new DocumentPack(), excelTemplate))
					{
						var worksheet = report.UDFSheet;

						var rootNode = new StringTreeBuilder(worksheet).GetTree();

						foreach (var node in rootNode.Children)
						{
							UDFField field;
							if (!fields.TryGetValue(node.Value, out field))
							{
								fields.Add(node.Value, field = new UDFField(node.Value));
							}
							field.AddInstance(new UDFFieldInstance(field, template, node));
						}
					}
				}
				catch (Exception exception)
				{
					if (exception is InvalidOperationException)
					{
						throw new Exception("Exception Occurred processing Template: " + template.ExcelTemplateFullPath, exception);
					}
				}
			}

			var correctDefaultValues = new Dictionary<string, string[]>();

			// Added for System Defined Templates.
			correctDefaultValues.Add("Consignor - Shipper", new string[] { "Bill Of Lading", "<BillOfLading.ConsignorAddress>" });
			correctDefaultValues.Add("Consignee - Importer", new string[] { "Bill Of Lading", "<BillOfLading.ConsigneeAddress>" });
			correctDefaultValues.Add("Origin Port - Place Of Receipt", new string[] { "Bill Of Lading", "<PlaceOfReceiptForBOL>" });
			correctDefaultValues.Add("Destination Port - Place Of Delivery", new string[] { "Bill Of Lading", "<PlaceOfDeliveryForBOL>" });
			correctDefaultValues.Add("Destination", new string[] { "Bill Of Lading", "<DestinationLoco.PortNameAndCountryName>" });

			correctDefaultValues.Add("Sending Forwarder", new string[] { "Bill Of Lading", "<SendingForwarderAddress>" });
			correctDefaultValues.Add("Declared Value", new string[] { "Bill Of Lading", "" });
			correctDefaultValues.Add("Onward Inland Routing", new string[] { "Bill Of Lading", "" });

			// Added for Client Specific Templates - Values based on existing core System Defined templates.
			correctDefaultValues.Add("Notify Party", new string[] { "Bill Of Lading", "<BillOfLading.NotifyParty>" });
			correctDefaultValues.Add("Delivery Agent Address", new string[] { "Bill Of Lading", "<BillOfLading.DeliveryAgent.SelectedAddress.PostalAddressInEnglish>" });
			correctDefaultValues.Add("Port Of Loading", new string[] { "Bill Of Lading", "<BillOfLading.PortOfLoadingDefault>" });
			correctDefaultValues.Add("Port Of Discharge", new string[] { "Bill Of Lading", "<BillOfLading.PortOfDischargeDefault>" });
			correctDefaultValues.Add("Freight Payable At", new string[] { "Bill Of Lading", "<FreightPayableAtForBOL>" });
			correctDefaultValues.Add("Excess Value Declaration", new string[] { "Bill Of Lading", "" });
			correctDefaultValues.Add("Place Of Issue", new string[] { "Bill Of Lading", "<BillOfLading.PlaceOfIssue>" });
			correctDefaultValues.Add("Date Of Issue", new string[] { "Bill Of Lading", "<HouseBillIssueDate>" });
			correctDefaultValues.Add("Shipper Load And Count", new string[] { "Bill Of Lading", "<BillOfLading.ShipperLoadAndCountDefault>" });
			correctDefaultValues.Add("As Agent Option", new string[] { "Bill Of Lading", "AS CARRIER" });
			correctDefaultValues.Add("Consignee", new string[] { "Bill Of Lading", "<Consignee.PostalAddress>" });

			// Added for Client Specific Templates where fields not used on existing core System Defined templates at all.
			correctDefaultValues.Add("Final Destination", new string[] { "Bill Of Lading", "<DestinationLoco.PortName>, <DestinationLoco.CountryCode>" });
			correctDefaultValues.Add("Final Destination - Last Discharge Port", new string[] { "Bill Of Lading", "<Consol.PortOfDischarge.PortName>,<Consol.PortOfDischarge.CountryName>" });
			correctDefaultValues.Add("Temperature Control Instructions", new string[] { "Bill Of Lading", "" });
			correctDefaultValues.Add("Origin", new string[] { "Bill Of Lading", "<OriginLoco.PortName>,<OriginLoco.CountryName>" });
			correctDefaultValues.Add("Prepaid At", new string[] { "Bill Of Lading", "" });
			correctDefaultValues.Add("Originals Required", new string[] { "Forwarding Instruction", "<NoOfOriginalBills>" });
			correctDefaultValues.Add("Copies Required", new string[] { "Forwarding Instruction", "<NoOfCopyBills>" });

			// This one I'm not so sure about...
			// correctDefaultValues.Add("Total Amount", new string[] { "SEKO Payment Voucher", "" });
			correctDefaultValues.Add("Total Amount", new string[] { "Bill Of Lading", "" });
			// Either one of the above could hold true, I would suggest making the SEKO one "Total Payment Amount" and the BOL one something a little more descriptive. Total Amount of WHAT?

			var unMappedResult = new ZStringBuilder();
			foreach (var field in fields.Values)
			{
				if (field.HasDifferingDefinitions)
				{
					if (!correctDefaultValues.ContainsKey(field.FieldName))
					{
						unMappedResult.Append("Field: [" + field.FieldName + "]");
						unMappedResult.Append(field.GetGroupedDefinitions());
					}
					else
					{
						field.SetExpectedDefinitions(correctDefaultValues[field.FieldName]);
					}
				}
			}

			Assert("Please add one correctDefaultValues element for each field listed below where there is a conflict:-\r\n\r\n" + unMappedResult.ToStringWithDelimiterBetweenAppends("\r\n----------------------------------------------------------------------------------------------\r\n"), unMappedResult.IsEmpty);

			var failingTemplates = new Dictionary<string, FailingTemplate>();
			foreach (var field in fields.Values)
			{
				if (field.HasDifferingDefinitions)
				{
					foreach (var fieldInstance in field.Instances)
					{
						if (field.ExpectedTabPage != fieldInstance.TabPage || field.ExpectedDefaultValue != fieldInstance.DefaultValue)
						{
							string templateName = fieldInstance.TemplatePath;
							FailingTemplate template;
							if (!failingTemplates.TryGetValue(templateName, out template))
							{
								failingTemplates[templateName] = template = new FailingTemplate(templateName);
							}
							template.Fields.Add(fieldInstance);
						}
					}
				}
			}

			var failingTemplatesInOrder = new List<FailingTemplate>(failingTemplates.Values);
			failingTemplatesInOrder.Sort(CompareByTemplateName);

			var expectedResult = new ZStringBuilder();
			var actualResult = new ZStringBuilder();
			var templateList = new ZStringBuilder();
			foreach (var failingTemplate in failingTemplatesInOrder)
			{
				templateList.AppendLine(failingTemplate.TemplateName);

				expectedResult.Append("Template: [" + failingTemplate.TemplateName + "]");
				expectedResult.Append("=".PadRight(100, '-'));
				actualResult.Append("Template: [" + failingTemplate.TemplateName + "]");
				actualResult.Append("=".PadRight(100, '-'));
				foreach (var field in failingTemplate.Fields)
				{
					expectedResult.Append(string.Format("Field: [{0}]   Tab: [{1}]   Default: [{2}]", field.Name, field.Field.ExpectedTabPage, field.Field.ExpectedDefaultValue));
					actualResult.Append(string.Format("Field: [{0}]   Tab: [{1}]   Default: [{2}]", field.Name, field.TabPage, field.DefaultValue));
				}
				expectedResult.Append("");
				actualResult.Append("");
			}

			AssertMultilineASCIIEquals("Badly mapped fields were found in UDF tabs. UDF Fields must be defined the same way in all System Defined Templates. The following templates have badly mapped fields:\r\n" + templateList
					, expectedResult.ToStringWithNewLineBetweenAppends()
					, actualResult.ToStringWithNewLineBetweenAppends());
		}

		[StressTest]
		public void TestDocumentFooterLogo()
		{
			var graphicalFooterRegex = new Regex(@"&R.*&G", RegexOptions.CultureInvariant); //If RegEx: "&R.*&G" is contained then any image byte data that may be contained in footer will show
			var errors = new HashSet<string>();
			var logoHexData = new string[] { "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-01-E6-00-00-00-2C-08-00-00-00-00-95-FC-44-75-00-00-00-02-62-4B-47-44-00-00-AA-8D-23-32-00-00-00-0C-63-6D-50-50-4A-43-6D-70-30-37-31-32-00-F8-01-03-EA-CC-60-95-00-00-0E-17-49-44-41-54-78-5E-ED-5A-09-78-14-45-16-AE-CE-05-99-C9-70-44-12-EE-23-04-72-C9-11-96-73-97-D3-15-57-14-57-03-8A-06-15-84-08-42-BC-58-0F-16-41-25-A0-80-BB-92-4F-5C-C5-75-57-05-02-1E-E0-05-AE-8A-80-EB-82-08-04-39-34-21-01-89-40-12-C8-C1-11-08-B9-C9-31-47-ED-AB-57-D5-3D-DD-3D-33-99-01-C7-2C-CB-A4-BE-A4-A7-EA-D5-AB-F7-AA-DE-5F-F5-EA-55-75-4B-94-34-A7-6B-DF-02-7E-D7-FE-10-9B-47-48-48-33-CC-3E-31-0B-9A-61-6E-86-D9-27-2C-E0-13-83-6C-5E-CD-3E-01-B3-E4-6B-91-B6-E4-1E-56-4A-AE-39-AB-5C-73-03-72-8B-62-23-38-03-BE-AC-79-8D-41-22-79-11-6E-05-FD-EF-18-2A-56-D5-48-12-B5-DE-1D-EB-71-17-7C-0F-E6-46-4D-83-30-DF-FE-AF-E0-3A-CE-D5-71-D0-DA-B6-4E-F8-AD-EB-4A-FC-81-4C-C1-78-D0-00-1E-D6-F0-A9-8C-D0-54-69-F7-48-A6-49-3A-D4-D7-63-85-BE-09-B3-1B-CF-4D-C9-2D-5B-85-05-53-16-39-31-E5-E9-3E-65-3A-EA-4D-DB-5C-88-5C-B9-CB-8F-48-96-A4-71-1E-03-E2-09-E3-EB-8F-33-AE-41-E9-81-9E-30-23-8F-6F-86-60-2E-03-12-8A-35-9F-92-2D-74-15-B3-61-CF-3F-7D-75-C9-89-29-7F-2A-D7-13-47-B8-40-D9-B2-EE-A3-0D-1B-D6-7F-6C-F0-18-0F-8F-18-D3-91-2B-CE-73-94-7D-14-66-42-5D-24-6E-E5-BB-F6-13-92-F4-21-21-23-D7-AC-98-74-C1-89-E1-33-F4-D3-44-FA-9D-0B-78-4E-17-20-20-03-3C-42-CF-53-A6-9A-92-C8-5E-BD-7A-45-0E-F7-94-1F-F8-7C-D3-69-A3-81-1A-F1-DC-FE-6F-3E-D8-98-9B-BB-E7-23-9D-85-7B-1C-68-E7-DC-E6-DB-6E-61-33-22-61-D3-65-20-E2-9E-D5-7A-11-FB-DE-2A-C8-3D-AB-CC-E1-C3-30-3B-C7-59-44-DB-B2-7D-9C-B9-F7-D2-61-27-58-F5-EC-71-16-89-45-61-90-42-C7-A8-E6-8C-B5-81-04-CB-CD-53-E7-B2-5C-EA-53-2A-40-CC-D5-50-6F-72-7F-AE-33-37-04-B4-F0-08-47-DA-60-23-FE-5A-C8-2D-95-0D-92-C1-A4-6A-ED-CB-30-BB-30-A2-16-81-0E-67-1A-F4-AB-66-EF-48-2B-34-F5-DB-A5-F6-D4-45-10-B3-49-96-C8-B1-64-E7-86-C3-55-7E-E1-37-CE-6C-43-C8-B9-2F-FC-D6-EC-66-4A-66-0D-B5-4A-E6-A8-DF-43-EE-E2-E6-1D-3F-5F-30-13-43-87-01-13-7F-8B-EA-CB-37-59-25-6A-33-DC-1D-44-0A-D3-0B-AD-09-5D-3E-B4-4A-C4-16-36-C1-B6-E5-93-9C-DA-80-C8-29-B7-21-53-D6-DE-00-4A-AC-B1-A3-88-2D-F3-40-45-9B-19-59-DF-07-40-9B-C1-B8-13-1C-DB-92-59-58-61-21-81-6D-BB-F7-1D-DE-87-77-F4-E2-57-DB-73-4A-41-47-C7-F8-49-43-94-21-BA-DA-A5-7C-84-EE-C1-7A-71-B0-C4-9B-D8-A8-67-89-BA-62-39-D2-52-F6-DE-29-E6-C4-88-53-94-AE-D4-08-9F-4F-69-D9-92-3E-0A-C9-98-54-C4-DA-6F-43-C2-0D-74-6F-52-77-F8-3D-B0-1D-8B-77-ED-B8-99-CF-35-FF-C7-2F-31-A6-24-2C-AC-B4-A4-8D-85-15-3A-84-4E-C3-E2-FB-50-B1-FB-FE-F6-76-1D-C6-D1-A0-93-96-2C-B4-9F-A6-8D-0F-5D-10-7D-74-19-8C-F8-08-CC-CE-9C-32-0F-B7-95-34-DD-A6-B3-45-32-56-4D-D6-50-27-33-92-34-A6-8D-D2-6A-9A-8D-4E-D5-88-F9-9C-FE-A8-0D-9A-C6-54-82-84-BF-22-CF-D3-2F-1A-D9-4F-EF-BA-25-58-8C-B1-FB-DB-57-80-C7-3C-94-11-43-D7-F1-A5-3D-87-0E-66-3F-AD-8E-52-FA-56-2B-8D-86-EE-30-F1-0E-0C-D2-90-6E-AD-E5-BD-94-61-96-2B-7F-15-74-BD-3D-97-1A-09-94-95-FE-C3-B5-C5-95-27-8D-A9-08-2D-2F-3E-5D-4E-2D-67-8A-8B-AB-99-48-F3-28-AC-9E-B0-76-15-A4-D5-AB-56-BD-B3-BE-96-D6-3A-C4-D2-9D-CF-98-E3-D4-62-5A-9C-CB-E8-89-E5-6E-23-46-F1-0C-59-09-C2-EE-63-19-A9-23-27-24-D2-F1-5A-CD-84-C4-D7-50-7A-12-AB-8D-A1-BC-EE-A3-E2-36-EC-A7-DF-25-BA-31-80-53-4C-6D-78-2C-70-2F-A5-3F-F4-C6-5C-F7-E1-A3-98-73-80-B4-CE-39-CC-BF-C8-3A-2E-4F-29-57-6E-70-67-2D-5D-F7-51-55-73-39-03-D1-DB-56-57-AE-4A-E9-D2-F5-49-7A-AA-5F-F7-AE-EF-B2-EE-E4-0B-50-14-AE-D1-66-9A-D3-9A-97-0C-F1-C3-C5-2D-69-D0-C1-4B-2F-2D-1E-88-C4-C9-2F-A4-A4-3C-FF-46-61-3F-96-37-2E-2F-B2-58-CE-3E-82-F4-61-B5-B4-26-5E-AD-6B-65-65-37-2C-FA-8D-4E-79-79-32-8F-BF-FC-BF-A5-74-B3-9A-E7-BA-B3-1F-60-31-91-96-E1-25-58-C8-DC-6D-87-8F-ED-FF-F0-31-98-3A-2B-68-71-0C-23-B5-5E-5E-6C-B6-14-CF-42-B6-B1-7A-98-45-F9-72-CC-E3-21-7A-DE-16-E9-75-98-1B-7F-7D-E3-47-21-5C-4E-A2-B9-B0-76-DE-60-23-D6-9F-A6-C8-B3-94-7E-8A-36-6D-3F-FF-40-B5-B9-E8-76-CC-1B-7E-04-56-16-75-91-60-70-AF-90-1E-62-F9-A0-D5-98-2F-8D-64-85-8E-27-69-B6-EC-9E-5B-B6-32-B5-24-DF-7F-8F-5B-72-DC-B6-06-E0-79-95-1F-E9-DE-A2-74-A9-80-D9-CF-D4-2A-04-B6-E6-87-B1-B8-9C-7E-8D-BF-CF-08-10-0A-53-3A-7D-CB-AB-A4-34-24-55-E1-1E-DD-8D-57-EB-8F-87-BE-F6-C2-8A-5B-B0-D1-F9-0A-C7-15-58-55-C4-F4-C8-83-49-E8-86-E1-EE-44-9B-60-AF-CC-40-CA-D3-CB-06-19-03-3A-4F-C4-7C-18-AC-F9-53-47-59-2E-BA-2B-7B-66-E3-22-4C-9A-8E-95-A1-1D-D8-B3-B2-82-A4-57-B1-4C-6C-CA-C6-5D-7B-F6-EC-FC-2C-3E-1D-CD-9F-F0-07-76-BF-35-05-9B-91-32-42-F7-61-E6-8E-D7-B6-02-D3-37-2B-2C-87-58-A9-C5-30-72-06-C9-79-45-F8-43-BA-2C-DA-31-F4-60-1A-CB-25-3F-80-84-90-4E-EC-59-CB-6B-ED-7B-B3-3C-54-99-A4-AA-44-E5-AC-5E-D4-E1-0F-BB-B3-47-B2-33-46-7B-AD-88-67-EC-86-44-76-95-5E-BB-CC-46-04-A1-3E-51-AF-EA-B6-66-00-6A-B1-AA-EE-E9-BA-ED-91-07-CA-CF-38-2A-62-17-6A-39-7E-B0-62-1E-21-33-28-B5-59-31-16-B3-DD-24-2C-27-FF-B4-3A-41-2D-63-59-C1-90-89-C2-DF-C1-8A-5B-AD-94-7E-8C-B9-99-48-7D-0E-F3-89-0B-17-CC-9F-BF-E0-D9-79-08-61-BB-E3-74-06-52-5F-97-3B-C5-83-B6-8F-B1-58-D3-1F-0B-2B-E9-59-DC-66-DB-17-0A-A6-22-84-2F-BA-8C-6E-E0-1D-88-4C-7E-2F-13-36-70-96-A0-9F-B0-98-A7-2C-9C-0F-E9-D9-67-7A-B0-52-27-5E-E3-0A-66-2E-42-06-57-CE-6A-61-E6-54-3B-9F-E2-09-14-50-C4-7C-10-72-B8-42-95-60-17-0D-34-4C-42-BC-DC-4C-4C-2B-31-E9-1C-FA-68-5F-97-DA-01-08-D5-1E-61-0C-F0-8C-34-92-C0-B8-BF-B0-C3-CC-86-81-06-D3-08-88-90-67-D2-EA-89-83-86-EC-02-CA-79-BE-F9-26-BF-B7-36-2D-0D-FE-D2-56-6F-6C-A0-05-B8-3A-FB-62-84-26-00-5D-08-39-7E-2B-F2-4F-46-AC-73-72-1D-1A-55-61-45-6A-F0-3E-D1-AF-06-0C-93-43-B8-93-AF-BA-1E-5B-6F-A4-DB-31-D2-1A-69-16-4C-DF-A0-63-BF-83-D2-63-CA-CD-9B-61-F0-82-03-50-6B-1E-A3-58-49-C9-F4-75-07-33-C7-44-79-22-54-3A-98-39-6A-0A-9F-03-BB-B6-56-28-E4-FC-1C-0F-BD-7C-B9-C2-81-AE-F7-23-FA-D6-BC-5E-37-9D-74-1D-F3-38-44-58-0C-3B-19-0B-B3-9E-A0-F4-6D-F9-FD-E2-4C-5A-19-45-C8-97-20-F2-3B-B4-7A-48-8E-7A-CA-6C-46-D3-4F-E2-A4-09-68-E3-CF-29-B5-DE-C8-32-41-08-E2-89-EB-1C-21-48-A0-45-78-EC-8D-AB-10-B2-8E-E2-5B-4F-88-A1-59-3A-DD-99-15-DA-E6-D0-57-B0-E5-5C-59-DF-4B-58-64-B3-48-E9-1C-94-8D-89-05-B4-A8-8B-A3-8E-A9-BC-99-8B-AB-5B-49-78-64-76-21-C8-F3-4E-36-6D-4E-B2-57-A8-1B-71-BA-63-23-8D-30-57-0D-1C-E9-F2-22-56-8F-C3-5D-1F-79-07-DC-DF-2A-EA-6D-B3-7D-A9-2D-7A-FB-C1-F5-AD-C9-EA-E3-95-A9-56-12-F1-C2-EB-B8-C8-A4-20-E2-C7-10-CE-B2-B0-52-14-7A-52-39-1D-C2-81-62-28-4D-AA-8E-B1-67-28-6C-E3-A5-B9-2C-D7-AD-17-7B-9E-C3-4D-D8-D0-3E-4C-4E-E1-E1-61-37-93-EC-F3-8C-1A-23-1F-7F-F7-95-B1-62-1C-3F-1E-1D-3F-C7-9E-BD-7B-92-C3-58-E4-41-3B-A4-83-F8-64-1E-7D-C6-A7-BF-91-89-A4-66-C3-BD-B5-A5-17-75-3A-C2-C2-C3-E4-37-A0-62-92-D8-A7-BA-7A-79-E2-22-51-AD-02-DD-DE-AC-5D-8F-2A-4C-1D-76-71-65-EA-EB-96-B1-DC-4D-07-47-21-2A-74-DA-81-AA-5E-A2-CE-5B-AB-BC-86-DD-5F-68-C7-A1-5E-89-0E-79-D8-20-17-D1-06-33-6C-B7-69-10-54-87-EE-A4-2C-D2-86-D5-DC-87-48-5B-80-79-0A-6A-BD-5F-DD-CC-96-C0-48-D2-67-48-3B-80-6F-1D-07-D4-51-BA-15-D7-FD-78-DC-D1-BF-C5-FC-7D-A7-F2-72-E5-94-97-57-2B-62-E8-17-64-59-B3-51-F4-CB-BC-F8-67-2C-CC-A1-F5-78-63-19-F2-93-60-3A-8F-B3-A6-ED-CF-58-2C-FF-64-D6-C0-D6-B2-15-36-ED-47-D7-33-5D-AD-23-BF-5E-58-E3-FF-05-66-3B-A4-76-07-EE-31-CC-FA-E9-DA-18-CC-F5-10-38-F7-8E-EF-17-0F-5E-F6-45-88-69-C6-01-EF-1C-06-73-55-5F-84-B9-82-5F-7A-FC-4D-2D-A2-04-EF-3B-C2-F2-90-F6-2A-D6-4F-83-DC-42-CC-2D-40-EA-2E-84-E0-26-AD-62-1B-BF-0D-01-FF-8E-C9-8C-B7-64-41-BB-B1-90-8B-51-9A-DF-7F-E8-CF-DC-93-8B-28-4B-08-1A-26-E0-A3-F4-52-D6-DF-27-F0-53-59-4A-36-1E-B5-13-9C-0D-CE-BB-9F-15-C8-1A-64-FB-BB-FD-75-D5-C0-81-CE-7C-2F-10-B5-02-2F-4B-9D-F0-F1-6E-BB-54-0F-1F-12-E4-66-66-65-56-B4-08-B4-55-40-A8-0A-0D-D0-F1-0B-DD-79-F9-AC-60-BC-41-2D-27-07-DF-2A-C7-E0-66-4A-F0-BC-43-E2-E1-3F-1B-73-D1-F8-0C-C7-45-B7-4B-FE-28-05-54-CC-FD-9A-9C-C7-03-57-3B-F9-12-FA-DC-49-56-8C-44-DF-5F-FF-5C-21-FB-19-37-82-EC-47-4F-1E-2D-7F-99-90-C5-5E-9B-90-C1-41-74-DE-BB-28-38-B8-6F-F2-C6-D7-B0-83-B6-CE-18-93-6D-DF-81-74-96-72-E7-7E-27-72-0E-30-BB-B4-86-74-F9-BB-9C-A2-CF-1B-19-3D-C6-97-25-F3-32-F6-E8-40-58-13-73-0F-EE-4D-FF-6E-E7-5E-76-91-54-09-FF-38-72-F6-2E-0F-32-99-78-12-0D-58-F1-48-32-A4-D9-F0-3F-2B-9B-64-E0-76-3D-14-DF-5A-D4-23-B8-C1-C3-E0-C8-8A-13-82-9C-82-FF-DA-EA-9E-78-1B-5A-F7-70-1A-7E-8D-52-BF-67-F6-98-54-13-39-7A-92-15-62-7A-88-A1-FC-74-9A-65-BA-83-7E-9A-9E-B0-9E-E5-43-97-04-11-7E-6A-56-AE-AA-33-B0-38-8C-94-BD-37-35-61-93-F8-B4-05-F7-64-12-D1-16-AF-C5-2A-93-DE-45-72-DD-9E-E4-31-AF-88-0B-52-D5-F9-55-38-71-C7-18-58-15-C5-42-6B-BE-A4-F0-C9-5A-A8-9F-4A-C1-21-26-57-F9-11-4D-48-2C-B7-D6-35-70-25-48-A3-59-B3-EF-AA-FA-A8-EC-42-AA-6D-5C-7B-B8-6F-CC-65-43-DD-5D-84-FC-11-59-F6-1F-7F-19-5E-44-C1-8B-24-F8-F4-0A-9C-36-EC-CD-5B-A9-B8-83-12-C0-B0-9F-16-C7-F8-05-17-F9-00-1B-9D-C4-25-15-5B-4E-E9-05-BC-EA-22-E1-0F-2F-79-74-D0-67-14-61-83-34-20-31-39-39-71-30-44-59-7D-6A-84-7F-7F-4A-EE-0F-68-83-14-32-74-52-E2-0D-F8-26-03-6F-BD-AD-A3-59-26-90-1D-E5-58-6A-40-C7-1E-9C-49-F7-B1-2B-94-FE-49-CB-DE-7E-73-F1-78-9C-5F-9D-0B-E8-6A-59-C7-E4-E4-E4-7B-06-B7-84-45-0F-11-02-26-FB-B9-59-EE-B9-02-37-12-38-94-FA-AC-4B-98-15-4E-DD-D1-CB-F9-79-56-F1-C1-CA-7C-B1-03-2C-AB-54-04-D9-7B-A1-EB-97-C3-19-9A-0F-4C-D5-6B-FB-7C-74-83-30-56-7F-0E-1E-6E-E2-EA-AF-D6-4C-6B-FB-41-0E-B8-CA-51-CB-E6-80-37-96-61-BE-A4-7D-03-04-4A-86-5B-2C-68-7A-43-16-B6-FE-12-97-FE-04-C8-D5-D9-59-FD-0F-D3-86-44-B9-47-E2-F7-1E-4A-1F-C4-EC-7A-B9-53-77-EA-38-C8-3C-A8-E1-A7-A4-A8-8B-82-29-17-2F-D4-E1-08-C6-DF-86-DA-53-8B-77-28-AD-D5-EB-78-40-16-AD-87-D9-6E-07-D9-98-B2-C9-EC-F0-CB-4B-C7-6E-3D-95-53-50-A6-86-30-B6-30-B9-4E-AE-9B-06-AA-AB-33-99-53-C0-AE-3D-1F-2B-D3-4F-2C-57-ED-A1-40-35-02-4D-C4-EE-0E-EB-05-B2-F1-D6-51-7E-37-09-B8-27-D1-4A-D8-64-37-D3-6C-58-23-DA-F4-10-3D-81-41-52-74-15-CA-5D-84-B5-CF-B3-EC-32-85-B1-0F-DC-A9-55-3D-A0-6D-97-4A-59-B0-07-D3-43-89-A1-F9-EA-57-92-29-95-09-F9-37-96-6F-93-FB-FC-3E-16-21-CC-BA-57-CB-1C-B1-96-71-54-EA-A8-AF-C9-CD-E4-BD-59-81-5D-69-2D-F0-51-6C-C5-2B-90-2A-FC-36-2F-DB-9F-A2-5A-4D-51-BC-A5-BD-53-2A-B9-2E-1B-A8-96-B0-BC-2E-95-59-C3-1B-D9-7B-A3-CA-AA-4F-D7-C2-51-69-8D-E1-59-69-E9-9A-E1-61-81-FE-C6-C8-C4-C1-64-D9-9D-AD-FD-3B-3F-3D-2F-3A-A6-03-F1-8B-88-8D-33-92-23-7E-A6-10-48-26-7C-B2-64-8C-27-19-B5-F0-6B-E8-1F-C2-A4-DB-0E-19-A1-CE-84-3B-F1-93-4F-E0-BD-24-EC-E4-43-60-72-84-AC-59-33-5A-F8-62-D8-74-47-2D-9D-4E-4E-9E-63-ED-63-F9-DD-35-7C-08-72-92-3D-E3-67-F5-C0-52-EB-F1-9B-F1-12-ED-88-01-94-19-95-CF-40-32-0C-A0-DC-08-F3-E3-EE-89-91-CA-37-44-81-B1-8F-7D-8D-F7-A4-A6-F7-57-0D-57-BE-22-0D-1B-B5-4C-41-BD-A9-3F-12-E2-11-1E-38-36-5D-D0-EC-19-00-57-CE-E5-69-9C-CD-35-34-14-96-DA-8C-9D-F0-E2-EA-48-45-C7-08-6B-0D-09-6A-49-6A-AC-52-70-40-C9-59-65-61-60-F0-2F-D1-1E-26-A0-51-C9-D6-0E-31-B5-E6-D6-33-7A-04-07-F4-44-66-71-B5-64-0C-EB-7A-3D-0F-85-2C-47-B2-F2-2F-DA-A4-E0-F6-91-B1-11-70-8E-AE-CE-87-5E-51-93-78-73-49-DE-78-94-F1-3C-95-7A-F6-87-FC-F2-96-9D-FA-89-0F-4D-8A-4B-41-A1-AD-5B-1B-31-F4-53-15-4A-F1-C2-89-BC-82-D2-7A-E2-DF-A6-5B-D4-F5-72-35-21-E6-EC-C3-F9-E5-56-C9-D0-B1-67-4C-0F-F1-3E-9A-19-BC-C9-CD-CD-FB-DB-A4-6A-D5-07-A2-2B-9F-29-BF-76-CB-E4-7F-30-0D-6B-B5-DF-9C-78-49-A9-77-CF-CD-EE-3B-25-EF-BA-EE-39-BD-CC-D1-A4-F3-EA-4A-FA-6E-C1-93-98-A1-FF-95-B4-75-DB-A6-A9-61-D6-07-CE-6E-3B-E8-0D-06-6D-38-E0-0D-89-BF-82-8C-82-E3-4C-68-14-DE-65-7A-3D-35-3D-CC-5E-1F-C2-35-22-70-7F-09-1B-C8-40-25-4A-F3-EA-B0-9A-61-F6-AA-39-7F-81-30-7E-47-89-1F-71-7A-3F-35-75-08-E6-FD-11-5C-2B-12-BF-28-80-78-58-BA-DD-C9-3B-63-2F-8C-B0-19-66-2F-18-F1-EA-17-D1-EC-B4-AF-7E-8C-BC-D0-C3-66-98-BD-60-C4-AB-5F-44-33-CC-57-3F-46-5E-E8-E1-7F-01-E7-3B-08-BC-17-6D-6E-AB-00-00-00-00-49-45-4E-44-AE-42-60-82",
			"89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-01-E6-00-00-00-2C-08-03-00-00-00-87-49-EB-9B-00-00-00-04-67-41-4D-41-00-00-B1-9E-61-4C-41-F7-00-00-00-FD-69-43-43-50-47-72-61-79-20-47-61-6D-6D-61-20-32-2E-32-00-00-28-CF-63-60-60-DC-E0-E8-E2-E4-CA-24-C0-C0-90-9B-57-52-E4-1E-E4-18-19-11-19-A5-C0-7E-9E-81-8D-81-99-01-0C-12-93-8B-0B-7C-83-DD-42-40-EC-BC-FC-BC-54-06-54-C0-C8-C0-F0-ED-1A-88-64-60-B8-AC-0B-32-8B-81-34-C0-9A-5C-50-54-02-A4-0F-00-B1-4F-4A-6A-71-32-D0-48-1E-20-3B-B3-BC-A4-00-28-CE-58-01-64-8B-24-65-83-D9-3D-20-76-76-48-90-33-90-BD-00-C8-E6-2B-49-AD-00-E9-65-70-CE-2F-A8-2C-CA-4C-CF-28-51-D0-48-D6-54-30-B4-B4-B4-54-70-4C-C9-4F-4A-55-08-AE-2C-2E-49-CD-2D-56-F0-CC-4B-CE-2F-2A-C8-2F-4A-2C-49-4D-D1-53-70-CC-C9-51-08-02-29-2F-56-08-4A-2D-4E-2D-2A-03-0A-32-40-EC-06-03-7E-F7-A2-C4-4A-05-F7-C4-DC-DC-44-05-23-3D-23-06-AA-03-50-18-43-58-9F-43-C0-61-C7-28-76-1E-21-86-00-C9-A5-45-65-B0-70-66-32-66-60-00-00-BF-06-40-2A-4D-E6-4F-EA-00-00-03-00-50-4C-54-45-00-00-00-01-01-01-02-02-02-03-03-03-04-04-04-05-05-05-06-06-06-07-07-07-08-08-08-09-09-09-0A-0A-0A-0B-0B-0B-0C-0C-0C-0D-0D-0D-0E-0E-0E-0F-0F-0F-10-10-10-11-11-11-12-12-12-13-13-13-14-14-14-15-15-15-16-16-16-17-17-17-18-18-18-19-19-19-1A-1A-1A-1B-1B-1B-1C-1C-1C-1D-1D-1D-1E-1E-1E-1F-1F-1F-20-20-20-21-21-21-22-22-22-23-23-23-24-24-24-25-25-25-26-26-26-27-27-27-28-28-28-29-29-29-2A-2A-2A-2B-2B-2B-2C-2C-2C-2D-2D-2D-2E-2E-2E-2F-2F-2F-30-30-30-31-31-31-32-32-32-33-33-33-34-34-34-35-35-35-36-36-36-37-37-37-38-38-38-39-39-39-3A-3A-3A-3B-3B-3B-3C-3C-3C-3D-3D-3D-3E-3E-3E-3F-3F-3F-40-40-40-41-41-41-42-42-42-43-43-43-44-44-44-45-45-45-46-46-46-47-47-47-48-48-48-49-49-49-4A-4A-4A-4B-4B-4B-4C-4C-4C-4D-4D-4D-4E-4E-4E-4F-4F-4F-50-50-50-51-51-51-52-52-52-53-53-53-54-54-54-55-55-55-56-56-56-57-57-57-58-58-58-59-59-59-5A-5A-5A-5B-5B-5B-5C-5C-5C-5D-5D-5D-5E-5E-5E-5F-5F-5F-60-60-60-61-61-61-62-62-62-63-63-63-64-64-64-65-65-65-66-66-66-67-67-67-68-68-68-69-69-69-6A-6A-6A-6B-6B-6B-6C-6C-6C-6D-6D-6D-6E-6E-6E-6F-6F-6F-70-70-70-71-71-71-72-72-72-73-73-73-74-74-74-75-75-75-76-76-76-77-77-77-78-78-78-79-79-79-7A-7A-7A-7B-7B-7B-7C-7C-7C-7D-7D-7D-7E-7E-7E-7F-7F-7F-80-80-80-81-81-81-82-82-82-83-83-83-84-84-84-85-85-85-86-86-86-87-87-87-88-88-88-89-89-89-8A-8A-8A-8B-8B-8B-8C-8C-8C-8D-8D-8D-8E-8E-8E-8F-8F-8F-90-90-90-91-91-91-92-92-92-93-93-93-94-94-94-95-95-95-96-96-96-97-97-97-98-98-98-99-99-99-9A-9A-9A-9B-9B-9B-9C-9C-9C-9D-9D-9D-9E-9E-9E-9F-9F-9F-A0-A0-A0-A1-A1-A1-A2-A2-A2-A3-A3-A3-A4-A4-A4-A5-A5-A5-A6-A6-A6-A7-A7-A7-A8-A8-A8-A9-A9-A9-AA-AA-AA-AB-AB-AB-AC-AC-AC-AD-AD-AD-AE-AE-AE-AF-AF-AF-B0-B0-B0-B1-B1-B1-B2-B2-B2-B3-B3-B3-B4-B4-B4-B5-B5-B5-B6-B6-B6-B7-B7-B7-B8-B8-B8-B9-B9-B9-BA-BA-BA-BB-BB-BB-BC-BC-BC-BD-BD-BD-BE-BE-BE-BF-BF-BF-C0-C0-C0-C1-C1-C1-C2-C2-C2-C3-C3-C3-C4-C4-C4-C5-C5-C5-C6-C6-C6-C7-C7-C7-C8-C8-C8-C9-C9-C9-CA-CA-CA-CB-CB-CB-CC-CC-CC-CD-CD-CD-CE-CE-CE-CF-CF-CF-D0-D0-D0-D1-D1-D1-D2-D2-D2-D3-D3-D3-D4-D4-D4-D5-D5-D5-D6-D6-D6-D7-D7-D7-D8-D8-D8-D9-D9-D9-DA-DA-DA-DB-DB-DB-DC-DC-DC-DD-DD-DD-DE-DE-DE-DF-DF-DF-E0-E0-E0-E1-E1-E1-E2-E2-E2-E3-E3-E3-E4-E4-E4-E5-E5-E5-E6-E6-E6-E7-E7-E7-E8-E8-E8-E9-E9-E9-EA-EA-EA-EB-EB-EB-EC-EC-EC-ED-ED-ED-EE-EE-EE-EF-EF-EF-F0-F0-F0-F1-F1-F1-F2-F2-F2-F3-F3-F3-F4-F4-F4-F5-F5-F5-F6-F6-F6-F7-F7-F7-F8-F8-F8-F9-F9-F9-FA-FA-FA-FB-FB-FB-FC-FC-FC-FD-FD-FD-FE-FE-FE-FF-FF-FF-E2-B0-5D-7D-00-00-00-09-70-48-59-73-00-00-2E-23-00-00-2E-23-01-78-A5-3F-76-00-00-0D-41-49-44-41-54-78-5E-ED-5B-09-5C-95-C5-16-FF-80-DC-B8-20-6A-A1-E5-BE-6F-B9-60-8A-F9-12-97-4A-5F-FE-B4-72-29-0D-5B-95-72-C1-17-6A-4F-CA-ED-25-EE-29-94-0F-15-73-41-10-35-97-97-CA-F3-99-BE-D4-CA-2C-F7-D4-14-E8-F7-14-45-73-81-24-8D-94-45-40-EE-85-F3-CE-9C-F9-E6-5B-EE-02-1F-76-35-F5-DE-53-F1-CD-9C-39-73-66-BE-F3-9F-39-73-E6-DC-2F-09-DC-E4-02-16-90-5C-E0-1D-DD-AF-08-6E-98-5D-62-11-B8-61-76-C3-EC-12-16-70-89-97-74-EF-66-37-CC-0F-A2-05-A4-B2-09-1E-BC-88-C5-F5-76-73-29-30-23-BE-8C-6E-E2-E3-FC-BD-BC-C4-6F-7C-32-73-D6-EC-59-D3-FF-67-7C-8E-AE-07-73-A9-B6-21-98-5F-84-CA-F2-5A-78-EC-85-DF-ED-89-5B-E2-E7-45-21-45-46-45-46-B2-47-64-D4-BC-78-8B-71-93-FF-71-C9-7D-34-3D-8F-64-E3-9A-5C-13-E6-32-1C-37-40-1F-21-11-61-CF-94-19-D5-AD-15-F4-2E-71-60-F2-C5-43-82-83-87-0E-FE-D2-38-20-46-24-17-D1-F0-9D-8A-8C-C8-72-19-D7-84-99-3B-67-3B-C4-DD-F6-66-34-4C-5C-05-2C-34-1E-1F-78-D3-8E-2D-BF-F2-B0-EE-3A-C3-81-C5-CD-81-24-F9-9D-71-40-8C-48-06-93-D2-37-8D-88-CA-32-2E-0A-B3-63-0B-91-05-8F-60-7B-A2-24-75-FB-0E-22-2F-DA-11-8D-B4-46-D9-E3-2B-07-0A-2F-D6-62-A2-AD-73-CA-81-48-D9-A2-79-CF-34-69-DA-B4-69-93-E5-65-4B-2A-12-2E-0C-73-29-9E-DB-6B-45-71-69-36-1C-62-DD-B5-E1-35-07-E2-3B-69-DF-0F-28-07-20-06-44-2D-57-AF-5E-43-BA-65-40-54-88-B8-30-CC-F6-3D-B7-1C-6D-0B-20-ED-99-F2-B7-A6-D4-3A-7A-EB-E6-2D-F8-0F-A3-3D-DA-A3-D9-92-9F-AF-F4-8A-22-C9-8F-B5-5A-8A-7E-CF-CC-CC-76-74-94-6B-04-8B-F2-0A-8D-E1-58-52-98-9F-6F-05-B9-39-EB-4A-A6-CE-83-B8-32-CC-0E-AC-A8-DF-AA-8F-82-CD-AE-39-E8-C5-44-3C-0F-68-FB-5F-8E-8D-8D-5D-B9-0C-7D-F7-DE-D1-41-ED-3B-3C-17-79-1D-1B-33-63-E3-82-48-D9-A8-F8-D8-95-4B-BF-61-E2-59-6B-86-3F-D5-BC-51-A3-C7-9F-0D-3F-C8-7B-5F-C7-A6-D8-E5-6B-71-8C-4B-1B-A3-E6-9D-CE-8B-5B-11-1B-BB-3C-11-8A-B7-0F-EB-D2-BE-E3-90-2F-B8-50-D2-B2-95-B1-B1-CB-F0-80-2F-3E-BE-6C-FE-F2-E2-13-4B-59-9F-1F-A9-29-35-7A-D8-B3-9D-02-02-02-FF-3A-62-D1-71-79-A2-59-6B-87-FF-85-8D-D1-2B-9C-9D-3D-9C-5C-1D-E6-52-3C-B7-C3-0D-BD-94-5A-1A-5F-D5-C2-CC-B7-6D-C4-A1-97-2A-F2-6E-41-78-A6-C7-E8-94-4F-46-4C-67-B7-51-58-A6-90-74-D6-7F-17-31-9E-86-43-21-0D-F0-79-74-0F-55-5F-FE-F6-39-1E-E4-79-8D-25-C7-10-42-95-18-4B-42-2F-5F-49-EA-0C-C3-A8-BA-0E-1B-F6-BF-4E-67-3F-27-53-0F-16-47-5C-9D-D6-4A-E5-8C-FC-CD-0D-B3-BC-CE-75-58-50-C5-CA-6F-0F-B7-F6-B0-A1-24-35-54-8B-32-0C-65-2C-8F-9E-D5-14-6D-C3-4A-E0-4D-9D-EA-6D-F0-63-57-1D-A3-27-73-AB-F3-89-15-3E-CB-C4-1E-CD-0A-67-53-B5-25-A2-29-D3-02-94-31-3F-C9-2A-35-D6-3C-4F-BC-71-40-D1-7B-D5-53-00-2B-AA-EA-14-36-C0-85-77-B4-93-8E-D5-B7-40-BF-9B-45-A3-6E-EE-CE-AA-38-DB-65-38-D4-C7-20-12-EB-F7-8F-0C-AA-33-95-04-37-32-7E-B9-01-96-2B-19-19-79-4C-B9-B9-3B-35-0F-5C-1D-87-14-1F-17-B7-72-43-01-14-74-D0-77-91-A4-3A-57-CC-AD-B5-BC-4A-BF-9E-68-4C-F5-FA-41-DD-79-41-8A-41-65-AF-B1-82-C7-63-9C-11-0C-FD-AC-D5-04-E0-85-EE-02-35-9B-6A-F0-B6-CF-33-AA-B1-47-BB-7C-48-7C-88-73-7C-AB-55-A1-E7-AB-00-C7-9B-51-A9-41-D7-EE-CC-39-20-AD-B1-0F-B3-C6-50-CE-02-19-43-1D-A7-A9-E2-07-8D-B3-61-B6-B6-AD-55-3D-37-A2-6E-BD-BF-C3-C5-76-0D-EA-AD-65-C3-FF-2C-83-A2-48-F5-30-C3-69-3F-5E-F3-0E-E8-DA-88-97-2A-1E-CB-FF-68-46-47-2A-0E-9D-19-11-F1-E1-92-CB-ED-58-D9-14-95-6E-B1-64-FE-8D-F8-5D-0A-E0-66-80-76-AC-98-9C-FA-54-F5-EC-11-11-39-B4-12-15-BD-F6-02-EC-D0-CA-3C-9C-B9-9E-AA-C1-70-BD-2D-7B-FA-BC-BF-EB-A7-33-3F-FC-2B-0C-97-CE-3F-21-A3-25-63-F9-45-65-98-2D-19-A3-48-AC-97-35-CC-8A-0F-73-2E-26-A5-C2-72-7B-43-39-1D-66-87-D9-12-6E-74-78-5F-92-42-E0-1C-EE-9D-25-6C-C2-9F-6B-AD-CE-CA-53-01-B6-10-AF-D6-E4-A3-79-E6-F4-17-A9-EC-CD-42-A4-67-58-A9-0A-BA-57-A4-91-AC-5C-31-9E-CA-59-4D-58-E5-B1-0B-90-22-DC-73-E5-AA-BE-95-A5-C3-87-E9-48-6E-BD-8B-E5-B7-A2-3D-49-CF-0A-80-39-F2-80-9E-BE-55-7D-F0-68-1E-43-D5-28-D8-4D-CF-49-B2-0D-2F-47-D4-DE-CB-9B-3C-12-88-95-4B-67-74-7D-FB-30-3B-7D-EB-DD-17-30-97-BA-DC-24-98-24-49-23-E0-EA-B8-B7-43-BE-65-82-E1-D6-30-6F-05-F8-87-6C-7A-D6-9E-40-E5-06-57-84-B7-0D-20-57-9F-EC-C3-B8-A3-E5-91-E8-94-36-25-C1-72-92-6D-15-91-78-34-25-F9-C8-D6-C2-05-54-9D-C2-97-02-77-BB-F3-A1-84-AF-9B-FE-8B-76-9F-4C-49-FA-FA-80-99-FA-56-DA-07-AB-89-3D-E4-B2-98-7C-6A-C1-51-6F-C6-19-23-33-9E-65-15-7F-07-30-2B-38-33-21-A1-41-94-65-0E-3D-F0-0F-67-DB-13-54-5B-E5-78-46-35-24-89-2B-AA-95-CE-8A-3A-79-5E-62-74-CE-A7-F1-64-59-F1-94-79-B2-66-BD-5A-CD-F4-D4-75-66-FC-F0-F8-F9-C4-29-39-76-01-CB-D9-63-D9-13-25-E9-1D-80-92-62-8A-C5-4A-7A-F3-99-28-54-35-0D-2C-BD-58-CD-FB-24-CD-65-25-B5-F4-C5-04-CB-26-2A-8D-20-2E-5F-08-C1-D3-A6-4C-9E-3C-65-EA-C4-7A-AC-F2-C8-59-78-87-B8-8B-85-75-78-D0-B6-89-AA-37-DB-53-25-06-32-09-EF-5A-02-CE-F4-DA-AC-DA-E2-3A-6C-E4-33-68-12-FA-D9-49-39-23-8B-F3-C4-CD-FC-C6-B4-C9-48-53-27-35-64-B5-DA-A5-C3-CC-55-70-C3-A8-45-3D-CC-5C-40-95-53-7C-9F-DA-47-C1-47-81-59-AB-D8-41-07-35-D2-55-D1-15-DD-44-18-AC-99-97-76-8E-DA-10-4C-FB-02-34-BC-61-98-37-75-33-49-15-5A-CF-63-97-99-8D-1D-BD-7D-83-70-FF-8C-80-BC-41-9D-3A-EF-43-CE-35-7E-F8-86-7E-B6-3A-21-01-FF-4D-88-4F-2C-82-4B-8F-32-56-5B-DA-B6-32-A0-D3-B0-34-81-24-29-29-59-F8-94-F2-06-4A-A1-79-76-31-71-AB-88-FB-6D-11-85-C9-3E-DC-C9-E7-3E-4E-82-89-B0-87-22-AD-6E-66-D9-84-5F-93-63-EF-0F-70-E6-11-A1-C9-3B-70-CA-51-6C-35-F7-B4-1D-A3-6D-59-30-AB-86-11-9B-49-31-94-CA-50-76-9A-C6-8E-8E-5A-E5-01-65-E4-4B-EB-20-03-A2-51-24-20-12-7E-C4-BA-37-6F-67-AB-4E-10-2F-5B-B9-1F-A5-B5-D4-C2-0C-3C-15-59-98-F5-1E-40-2C-65-42-90-46-40-4E-73-49-DA-8E-FD-BE-27-AB-FB-9C-D6-AA-D8-41-A6-1F-CC-59-03-A9-C3-36-4C-66-90-DB-AC-48-20-A6-3D-6C-0B-C1-00-48-E7-29-EF-6C-59-D7-29-FA-E5-0B-63-68-46-BF-D4-61-95-EA-A7-81-7B-F2-F7-C5-78-1F-51-95-AD-22-65-72-58-37-05-5F-82-F4-BA-B6-63-C8-BF-6F-08-BB-68-1D-B4-6A-1E-32-93-C6-54-B6-56-13-FB-5D-5D-14-BC-83-23-FB-6A-F9-9A-F3-C1-7E-07-8D-22-F9-25-6D-B0-93-E7-AA-53-AB-59-03-FA-05-63-0C-E5-6F-2A-4A-2D-F6-64-6C-F0-93-FC-CE-64-B7-90-A4-46-33-17-E3-26-1B-01-B9-6D-24-CF-9D-A8-81-27-3D-9E-10-4E-9D-74-CE-25-DE-2C-2A-E7-D0-2E-AC-91-86-99-8A-86-AC-D4-34-8B-71-0F-50-DA-C4-BB-96-BF-A0-9A-35-FD-97-C2-97-14-66-0D-12-F3-4A-20-35-C1-BC-FA-1D-2D-A7-CE-45-72-72-64-A3-10-7A-89-84-B6-B0-EA-D6-27-34-B8-06-E5-27-D1-D1-AC-19-C3-BF-A6-FF-7A-DE-CD-38-CC-1A-20-35-C6-D6-C3-2C-46-35-0C-B3-A3-0E-36-7C-3E-3A-92-BA-06-D5-57-D4-0F-27-BF-18-7F-33-E5-D8-31-EE-B3-F1-80-9C-0E-45-66-3C-6E-13-30-A8-AE-81-39-46-8C-B4-71-37-B7-91-3C-D8-EF-C6-6F-D0-B8-AF-6B-97-4C-C9-00-C6-F2-C0-50-0C-89-C7-41-1D-30-21-BD-93-80-EA-47-27-FA-5E-2A-BF-76-F1-FC-39-41-E7-CF-17-C8-31-F4-4C-A1-6B-34-A9-8E-E4-D5-0F-A8-32-0E-6E-75-66-4F-1F-F1-A9-C8-35-4A-A8-57-4F-25-99-1B-9B-47-75-F4-13-76-F8-F7-0F-E4-7A-86-6B-C7-F8-59-4E-80-DE-37-30-AB-C0-AB-9E-C2-FE-22-B1-03-B3-D6-C1-68-F1-B1-57-BE-85-49-A6-66-01-ED-02-D0-CB-CE-C2-98-A6-0F-CA-8C-A3-DD-DC-96-60-CE-E6-49-8F-85-DA-AE-57-29-DF-E1-CF-3F-2C-8A-A6-F6-61-58-9A-46-25-1E-37-EF-23-08-7A-EB-07-2C-E1-D9-10-F4-EF-44-3C-86-AE-B8-9F-2A-E7-28-4A-F3-FC-06-52-B9-27-17-BF-7B-73-45-5D-94-44-7B-7E-F2-A7-03-F9-AD-2C-22-85-AE-DA-03-EC-BD-94-73-61-56-47-28-97-D3-66-DD-CA-EC-A0-6E-64-5B-0F-AD-EB-ED-00-66-A3-01-18-73-BA-CC-97-3E-54-A9-C2-0C-DC-5E-21-A8-6E-3C-ED-66-0E-F3-09-CA-38-99-74-1F-E8-F0-E3-BA-1B-37-FD-70-B2-79-34-96-F8-21-BD-9A-B8-A9-94-C3-AA-AC-7E-46-92-16-BE-0B-7E-A5-E5-81-01-37-A7-74-3A-8D-5B-D1-4F-4B-85-94-3D-95-FA-DE-82-B5-54-90-CF-7D-80-25-54-0D-83-92-0F-E4-04-17-0A-AF-A2-D0-E0-C3-DF-A9-7F-D5-3D-0A-0A-69-E1-E2-7B-06-1B-98-B5-5E-D8-E6-98-35-76-36-1B-42-4D-23-44-D3-2A-13-66-55-EA-36-60-26-87-6F-6F-99-DB-F2-0A-F0-C0-9B-78-EC-D0-C1-83-87-8F-65-22-CC-2F-A3-C4-7B-F2-D9-EC-81-67-F3-2A-32-B3-DF-F0-31-A3-91-46-E1-7F-23-93-61-21-F1-C2-49-57-21-45-CB-55-0E-03-E4-07-10-9B-B9-E4-FC-5C-33-85-63-52-A3-55-B4-2B-0B-F7-8F-AA-2B-1D-84-BD-74-34-07-89-18-7A-37-A1-D5-07-57-4B-C9-01-FE-95-52-0D-CC-B1-BC-4B-A5-F9-62-A2-FC-0A-B6-0E-B2-6A-4B-FD-13-E5-2D-FE-09-75-8C-97-3F-6D-6A-B8-86-D8-05-FB-47-D7-F5-4C-91-BB-59-C3-2C-AC-61-13-5A-51-03-6F-15-7F-55-74-AC-02-AB-52-42-30-7D-48-6C-7B-72-5A-9D-F5-56-A7-AE-76-64-DD-B9-5B-46-A4-AD-BF-DC-97-01-F7-CB-92-F4-02-89-FC-70-16-3F-13-69-8C-3F-24-8D-D5-C0-CC-73-50-1A-AA-74-86-27-B8-24-1E-ED-5C-A0-6B-4E-AB-1B-00-BF-51-AA-4B-AA-39-66-F6-BB-9D-B6-C2-06-B9-4B-87-E0-D0-D0-E0-40-74-09-6D-6E-CA-FE-7D-82-98-0F-FF-28-C5-E7-C9-C1-C1-4F-D3-2F-19-94-F5-2E-EE-C1-0A-15-D8-55-8E-51-11-39-F6-2A-27-E1-08-FB-88-A9-7D-C8-DC-D8-A5-33-FA-51-78-57-E7-12-C4-8B-31-86-86-86-BE-12-88-DF-2D-06-8A-9F-AC-55-98-C5-CC-B9-3A-6D-4D-2E-5B-B1-ED-C4-D8-9A-4E-FA-CD-A9-D9-49-BA-61-1C-74-D0-DF-A7-75-F3-11-17-71-07-73-B4-7F-6F-56-D7-63-19-08-53-F3-36-DC-63-83-E2-FF-BB-6A-58-F5-F5-A7-31-9C-EA-3E-77-1C-3A-43-8A-B4-71-37-E7-EB-7F-01-C2-17-E8-6A-B1-90-E9-BD-B9-1F-DF-4E-3B-6B-20-96-F8-BE-26-F2-FA-09-8A-F8-07-5C-2A-BD-02-F0-36-D5-36-88-49-F1-18-5A-43-13-B1-85-DF-92-9A-8B-2F-4C-CF-51-42-1D-AF-60-FC-D7-50-95-2A-AD-C4-0D-6C-3D-C6-5B-42-B5-35-CC-AA-1D-98-06-B9-46-CA-54-F8-C5-D6-51-AD-67-2B-69-BB-BF-95-11-99-AE-32-3A-68-52-67-DA-81-85-97-D0-CD-C6-66-62-62-A2-7A-47-6D-D4-67-63-EF-29-C2-78-6B-E4-83-16-71-0F-81-1C-BC-5C-ED-80-14-F1-6D-AF-62-E0-91-90-46-41-52-8B-5C-1A-78-3A-35-7C-C8-8A-FC-9A-C5-A8-0D-5E-BF-72-DF-D2-E3-F2-31-B0-60-0F-97-87-12-43-F3-DD-AF-90-2F-7D-74-F2-15-D5-9F-17-D6-5B-47-55-0C-B3-5E-D5-0B-37-A2-10-20-C7-8A-BB-C8-1A-66-23-CB-DC-89-32-E5-30-BA-93-46-2D-CF-88-AB-BA-FA-57-F0-32-35-09-3E-05-57-5E-F2-F3-AA-13-3E-B9-45-CB-29-90-D7-A7-55-EB-BD-B0-D1-DB-D7-07-C9-97-FE-32-32-7D-0A-9B-2A-E3-D3-7B-08-4D-B4-78-A0-09-DB-7C-13-59-B9-F0-3D-CA-4B-62-34-C7-E2-38-28-59-D5-43-F6-C5-78-E8-76-9F-93-05-A9-F5-59-FF-8E-7C-79-E0-D5-9A-62-E8-80-51-0D-A9-8F-5F-BF-EF-89-1B-ED-8D-83-99-94-3B-57-B8-37-0E-6E-9A-83-77-E6-41-4D-F8-6F-58-48-15-5A-85-89-30-2E-AE-2B-DD-E7-18-F9-77-9F-2B-BE-2A-B8-EB-5F-8F-68-4E-63-27-E1-67-4C-4D-79-50-06-B8-95-76-E4-50-32-B7-D1-4F-07-CE-83-25-3B-1B-B7-63-5E-76-8E-19-7E-4D-4A-91-29-99-3D-93-53-92-73-18-2F-39-25-29-83-C4-2D-A9-C4-E7-69-4F-38-BB-29-7A-F6-9C-E8-75-DF-53-86-04-6F-4C-27-D7-CC-18-17-36-76-E2-82-FF-9C-61-61-57-2E-8A-26-27-29-FF-7F-07-CF-BB-4C-80-2B-DB-17-CF-8A-5A-27-82-A7-74-36-60-D2-75-F1-96-17-D4-EA-B5-43-EB-3E-0A-0F-0B-1B-3F-3D-7E-BF-D2-8C-87-F7-F1-D5-D3-C7-87-8D-9D-B4-F0-8B-B3-22-B4-C3-BE-06-C3-4F-63-B6-34-20-25-96-9A-01-51-E7-89-D0-A0-CE-53-77-87-34-F1-E4-08-BF-7F-39-9B-EE-FE-DB-FF-09-36-BF-2F-50-E6-C9-11-F9-57-AE-FB-1F-66-67-BF-C1-83-A2-EF-5C-4D-3A-9A-65-77-EF-E4-B7-BA-FB-BB-D9-C9-2F-F0-C0-A8-E3-17-EB-B7-EF-CC-FB-B8-61-BE-33-76-2D-BF-56-9E-63-C1-6F-82-EE-04-B9-61-BE-13-56-BD-1D-9D-DB-62-62-96-C4-2C-51-3E-F9-B9-1D-15-8E-FB-B8-61-76-AE-3D-EF-51-6D-6E-98-EF-51-60-9C-3B-2D-37-CC-CE-B5-E7-3D-AA-CD-0D-F3-3D-0A-8C-73-A7-F5-7F-17-FA-F8-20-4F-B5-CD-DA-00-00-00-00-49-45-4E-44-AE-42-60-82",
			"89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-B7-00-00-00-11-08-03-00-00-00-A8-F5-CD-5E-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-02-DC-50-4C-54-45-00-00-00-09-09-09-05-05-05-03-03-03-02-02-02-04-04-04-0F-0F-0F-14-14-14-1B-1B-1B-11-11-11-07-07-07-13-13-13-0E-0E-0E-1C-1C-1C-12-12-12-19-19-19-1D-1D-1D-1E-1E-1E-06-06-06-1A-1A-1A-0D-0D-0D-18-18-18-17-17-17-0A-0A-0A-0C-0C-0C-10-10-10-16-16-16-08-08-08-01-01-01-21-21-21-30-30-30-36-36-36-35-35-35-34-34-34-39-39-39-32-32-32-23-23-23-2E-2E-2E-2A-2A-2A-2B-2B-2B-2F-2F-2F-25-25-25-2D-2D-2D-24-24-24-26-26-26-3B-3B-3B-3A-3A-3A-3F-3F-3F-22-22-22-38-38-38-28-28-28-3C-3C-3C-27-27-27-3E-3E-3E-29-29-29-2C-2C-2C-33-33-33-20-20-20-3D-3D-3D-37-37-37-43-43-43-53-53-53-4E-4E-4E-55-55-55-57-57-57-56-56-56-47-47-47-52-52-52-54-54-54-4C-4C-4C-4D-4D-4D-45-45-45-46-46-46-41-41-41-5E-5E-5E-59-59-59-5F-5F-5F-5D-5D-5D-51-51-51-50-50-50-40-40-40-48-48-48-4A-4A-4A-5A-5A-5A-5B-5B-5B-42-42-42-4B-4B-4B-5C-5C-5C-4F-4F-4F-44-44-44-58-58-58-71-71-71-74-74-74-6B-6B-6B-78-78-78-72-72-72-7C-7C-7C-7D-7D-7D-6A-6A-6A-67-67-67-7F-7F-7F-61-61-61-7A-7A-7A-7E-7E-7E-68-68-68-6D-6D-6D-60-60-60-75-75-75-6E-6E-6E-69-69-69-63-63-63-7B-7B-7B-62-62-62-76-76-76-65-65-65-64-64-64-70-70-70-6F-6F-6F-79-79-79-66-66-66-77-77-77-98-98-98-95-95-95-8A-8A-8A-8D-8D-8D-8C-8C-8C-90-90-90-86-86-86-91-91-91-9A-9A-9A-80-80-80-9C-9C-9C-94-94-94-9D-9D-9D-81-81-81-9F-9F-9F-99-99-99-8E-8E-8E-93-93-93-97-97-97-88-88-88-82-82-82-87-87-87-89-89-89-85-85-85-8F-8F-8F-9B-9B-9B-83-83-83-96-96-96-92-92-92-8B-8B-8B-84-84-84-9E-9E-9E-A8-A8-A8-A7-A7-A7-B9-B9-B9-A0-A0-A0-A5-A5-A5-AE-AE-AE-AB-AB-AB-A6-A6-A6-B1-B1-B1-B2-B2-B2-B4-B4-B4-B7-B7-B7-A3-A3-A3-AA-AA-AA-A2-A2-A2-A9-A9-A9-AC-AC-AC-BB-BB-BB-B5-B5-B5-A1-A1-A1-BF-BF-BF-B0-B0-B0-BA-BA-BA-B6-B6-B6-A4-A4-A4-B3-B3-B3-BC-BC-BC-AF-AF-AF-BE-BE-BE-CE-CE-CE-C9-C9-C9-C0-C0-C0-DD-DD-DD-CA-CA-CA-C3-C3-C3-C1-C1-C1-D0-D0-D0-C6-C6-C6-D7-D7-D7-DA-DA-DA-D1-D1-D1-D9-D9-D9-D3-D3-D3-DF-DF-DF-DB-DB-DB-C8-C8-C8-DE-DE-DE-D6-D6-D6-D8-D8-D8-D2-D2-D2-CB-CB-CB-CF-CF-CF-D5-D5-D5-DC-DC-DC-CC-CC-CC-D4-D4-D4-C2-C2-C2-CD-CD-CD-C7-C7-C7-EF-EF-EF-ED-ED-ED-FE-FE-FE-FA-FA-FA-F8-F8-F8-E4-E4-E4-F3-F3-F3-FB-FB-FB-F9-F9-F9-EA-EA-EA-F0-F0-F0-E6-E6-E6-EC-EC-EC-E1-E1-E1-FD-FD-FD-EE-EE-EE-FC-FC-FC-E7-E7-E7-F1-F1-F1-E9-E9-E9-F6-F6-F6-F7-F7-F7-F4-F4-F4-EB-EB-EB-E3-E3-E3-E5-E5-E5-F2-F2-F2-F5-F5-F5-E2-E2-E2-E0-E0-E0-E8-E8-E8-FF-FF-FF-CA-3E-04-60-00-00-05-33-49-44-41-54-48-C7-ED-56-F9-53-D3-47-14-7F-62-2B-78-55-C5-D6-DA-23-DC-88-02-8A-57-A5-6A-B8-41-05-51-04-E3-81-78-80-14-A5-08-18-04-DB-02-1A-C0-03-28-F1-E0-A6-82-60-04-C1-03-A5-15-B5-72-04-39-14-0F-54-68-15-48-30-11-30-22-06-81-82-FD-FC-03-DD-04-F8-81-76-C6-DA-76-C6-D6-99-BE-F9-CE-EC-EE-7B-FB-7D-EF-B3-EF-DA-25-BC-9D-44-FF-E3-FE-3D-79-71-74-74-F5-54-A4-6F-10-6E-78-2C-E2-9F-A8-6A-6C-6A-7E-73-B8-25-D2-CC-8D-91-7B-18-ED-15-6C-22-FA-91-68-44-D6-A0-64-B3-91-8E-B1-F1-34-DD-96-A1-9D-57-8F-97-BC-52-13-97-4C-1E-BD-C9-3C-29-0D-B7-B2-B6-B1-B5-B3-B5-DA-E2-4D-D9-2D-F6-3E-4D-43-46-89-34-46-D2-F4-A1-6D-B2-19-94-FD-4A-3D-5B-1D-8E-BD-E1-FC-3E-1E-E5-E8-B4-64-C9-92-A5-D1-44-A6-CB-9C-7D-E5-43-B8-63-CA-CA-C5-15-D7-BE-B8-5A-E9-E7-5B-85-2D-EF-50-74-E4-63-44-B9-B8-EC-01-AA-F7-ED-C7-81-9C-AC-6D-AD-07-97-C7-02-27-B6-D7-34-6F-BF-2E-8A-6B-C3-49-5F-57-FF-78-39-6E-7C-B9-22-20-77-00-77-E9-8E-C0-BC-3F-31-DF-FE-E4-55-52-79-E5-E0-44-71-EA-29-F0-6D-C7-30-61-4C-50-82-50-28-8C-7A-16-CC-9C-4C-51-6A-56-2D-51-13-14-9D-58-49-EF-32-9E-C3-4D-95-64-54-AD-A6-6A-B0-C6-4E-D2-D2-A2-FC-D1-34-86-AD-BC-30-96-F4-C6-92-60-DC-B8-36-AE-4A-4A-CF-0B-D4-43-8E-1A-F7-21-E3-D3-4B-47-01-CA-36-A6-F2-16-14-AD-68-55-76-BD-A8-C1-6D-66-BF-BB-0A-4A-DC-01-8C-D5-07-EB-69-94-31-A3-40-AF-FC-31-06-27-4F-51-D9-0E-C3-FD-C0-F3-56-E0-69-B5-5B-35-60-26-7D-86-8E-36-48-7F-19-42-3E-7E-86-B9-B9-39-67-A6-FD-2A-83-59-07-29-DC-39-08-9B-D4-C6-A9-F9-3D-B2-AB-B3-20-2D-E5-61-9A-89-17-B3-29-53-15-88-0E-7D-9A-A0-6F-D6-4E-74-46-32-87-DC-31-82-26-1A-07-6D-24-FB-B3-34-A9-0F-2D-FC-4A-1A-CD-1C-43-E6-6A-DC-47-04-80-41-75-CF-52-8F-38-1C-B5-F6-AD-0B-41-C4-B9-84-D5-06-3E-B3-76-A2-66-57-68-E1-5D-1E-D7-46-C2-4E-0E-04-B9-CC-6D-93-24-F2-37-62-8D-9F-C7-ED-E6-A3-FC-30-C5-5A-5B-1F-7E-00-AF-8B-78-C8-5D-67-7B-5D-3E-8F-67-5F-0A-CC-F3-73-AA-6C-72-C0-AE-7B-C3-83-12-E9-53-81-2C-3E-1D-4B-2A-81-29-E9-86-EC-F6-8F-EB-27-66-DC-8B-56-62-2E-25-E3-3C-69-FB-AE-5E-47-D4-49-74-0B-28-20-53-C0-95-1C-4B-68-22-2B-DE-60-5A-7D-8D-68-E5-CE-32-CC-21-C3-E0-00-1D-9A-3F-E0-EF-14-20-E0-C2-67-65-BD-E6-37-B8-8D-11-F9-B6-48-29-0A-4C-3D-E0-27-76-92-AD-77-4D-31-39-B4-42-BA-A0-D7-FA-7B-E0-07-1B-A9-CE-93-0B-86-91-9E-71-93-7B-E2-B7-DE-B7-8C-5C-9B-36-E9-4E-6F-4D-85-D9-3D-DB-8B-78-3F-D6-DB-F2-78-62-A9-75-15-F0-79-79-6E-C0-23-4E-D5-B8-E1-B0-77-E9-16-69-DE-A2-8B-54-3C-61-33-16-52-BA-8A-95-43-DA-C0-22-E6-12-A2-0B-38-4A-1F-D8-AD-E2-BA-85-B6-AB-0E-03-4F-0A-03-78-E4-97-41-CB-80-2E-2E-25-22-9B-05-68-64-B9-09-CD-F2-70-76-77-12-0E-F8-9B-55-83-55-D9-78-60-F6-7D-47-B4-9E-72-C0-91-62-AF-3C-91-77-FD-72-B9-A7-7F-7A-E6-77-5B-60-DF-CF-AB-03-44-7C-38-BE-2C-B6-88-D8-57-35-05-85-FC-06-DD-D4-C3-D7-A7-A0-71-BE-9B-67-35-FF-12-E8-FC-DE-D4-8C-62-D8-B0-9C-32-C5-4F-56-88-89-D6-1B-8E-7B-8C-06-87-E3-12-0A-2A-32-4A-6C-D0-F8-B0-E8-51-77-0B-2C-28-10-7D-46-94-FC-8C-A6-D6-43-40-93-D9-AE-E7-0C-BF-81-BA-70-2F-23-67-24-55-05-D3-06-E0-AE-36-B5-F4-B0-FE-BD-86-0E-9B-51-EC-40-86-AA-70-47-AD-12-6C-76-C5-B6-B0-D8-B5-9D-6E-49-94-B5-30-DD-38-CF-45-24-0C-AA-73-46-7C-68-D2-6E-21-0F-73-FA-FD-BE-62-FF-7D-94-AC-D5-7F-9F-2F-0C-79-A9-8D-4C-A7-C7-AE-C2-90-5A-6D-FC-EC-71-46-2F-77-DB-D7-70-48-8B-DD-2F-5E-11-A2-C9-FC-6D-E9-E3-BC-07-D2-B1-05-C3-71-CF-D3-BD-93-17-FE-71-26-15-68-C5-46-0F-A4-77-2F-D1-39-9C-A3-4F-6E-94-10-85-86-D5-6B-10-E7-9B-0D-24-9E-4D-CC-8B-7D-9F-D2-9A-20-22-DF-66-23-CA-07-4E-B2-32-26-B3-0C-81-26-65-C5-13-B9-ED-30-A4-C1-7E-12-2F-4C-65-B3-E4-88-16-D4-09-2E-E1-6E-78-76-FB-D5-FE-FA-86-CE-2B-90-9F-48-78-D0-F3-10-25-92-9A-4C-19-7A-0B-C4-0B-6E-E3-66-5A-11-2E-A3-5F-8C-86-84-B3-6C-82-93-E1-05-A5-ED-A7-95-8D-A9-82-0E-14-86-5F-61-C5-28-16-C5-74-03-D3-95-C3-71-FF-6A-33-83-2B-B3-58-BF-EE-A1-7F-51-D2-82-C5-7A-7A-86-8B-FB-34-2D-6B-21-9A-E6-0E-B8-6B-93-0D-1E-2C-22-9A-1A-08-13-F3-72-20-9F-38-EE-34-31-03-15-06-D3-98-C2-FC-49-D6-2F-1C-A6-12-99-14-CA-90-CE-21-32-4F-FB-4B-FD-BB-C7-44-C3-FB-75-F7-76-53-CA-1F-78-4A-16-32-F5-A7-90-29-14-8A-0E-05-E4-AC-61-29-24-32-95-48-25-53-28-FB-A4-AC-A7-AA-D6-01-64-87-2E-D6-EB-9B-25-12-D5-B5-24-61-52-A9-52-29-53-5F-C2-83-E3-7F-F2-5D-C5-55-F7-AF-7F-EB-5D-F5-B7-49-9A-27-7A-F2-36-E2-7E-1D-FA-0D-5C-70-4A-DC-B0-C2-A4-C0-00-00-00-00-49-45-4E-44-AE-42-60-82" };

			foreach (var template in GetAllTemplates())
			{
				try
				{
					using (var source = template.GetSO_TemplateReader())
					using (Stream stream = new MemoryStream(source.ToByteArray()))
					{
						XlsFile excelFile = new XlsFile();
						excelFile.Open(stream);

						for (int i = 1; i <= excelFile.ImageCount; i++)
						{
							TXlsImgType imageType = TXlsImgType.Bmp;
							byte[] imageFileContent = excelFile.GetImage(i, ref imageType);
							string imageHexData = (imageFileContent != null) ? BitConverter.ToString(imageFileContent) : "";

							foreach (var oldLogo in logoHexData)
							{
								if (imageHexData.Equals(oldLogo))
								{
									errors.Add(template.ExcelTemplateFullPath);
									break;
								}
							}
						}

						var pageFooter = excelFile.PageFooter;
						if (graphicalFooterRegex.IsMatch(pageFooter))
						{
							TXlsImgType imageType = TXlsImgType.Bmp;
							byte[] imageFileContent = excelFile.GetHeaderOrFooterImage(THeaderAndFooterKind.Default, THeaderAndFooterPos.FooterRight, ref imageType);
							string imageHexData = (imageFileContent != null) ? BitConverter.ToString(imageFileContent) : "";

							foreach (var oldLogo in logoHexData)
							{
								if (imageHexData.Equals(oldLogo))
								{
									errors.Add(template.ExcelTemplateFullPath);
									break;
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					errors.Add("Exception: [" + ex.Message + "]");
				}
			}

			Assert(
		@"Documents should no longer contain any ""ediEnterprise"" company logos.

The following documents still have the logo:

" + string.Join("\r\n", errors.ToArray()) + "\r\n\r\nTotal Documents with footer requiring change: " + errors.Count, errors.Count == 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckClientDocumentsXmlFilesAndMakeSureTheXlsFilesTheyReferenceActuallyExist()
		{
			var errors = new List<string>();
			var templateCollection = GetClientFilesOnly();

			foreach (var clientTemplate in templateCollection)
			{
				try
				{
					var template = clientTemplate.Template;
					using (var inStream = File.OpenRead(template.ExcelTemplateFullPath))
					{
						var excelFile = new XlsFile();
						excelFile.Open(inStream);
					}
				}
				catch (Exception ex)
				{
					errors.Add("Exception: [" + ex.Message + "]     Template Filename: [" + clientTemplate.Template.ExcelTemplateFullPath + "]     XML Source: [" + clientTemplate.SourceXMLFileName + "]");
				}
			}

			Assert(@"Client documents that are referenced in XML Schema should be locatable in storage.

If this is not the case then the document in question may have been moved, removed or renamed without proper check in procedure followed.

Please modify the appropriate XML Schema and make the ppropriate changes to fix these discrepancies.

The following client documents are referenced in XML schema but cannot be located in storage:
			
			" + string.Join("\r\n", errors.ToArray()) + "\r\n\r\nTotal Documents : " + errors.Count, errors.Count == 0);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFontColorIsWhiteOnAllColorizableCellsInAllCustomizableSystemDocuments()
		{
			//Uncomment to fix this test.
			//ChangeAllColorizableCellFontsColorToWhite(Factory.LoadTop1<StmTemplateBase>(new ZQuery(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System)));

			var errors = new ZStringBuilder();
			var colorizableCellColors = new List<int>();
			colorizableCellColors.Add(DocBuilderTheme.DefaultPrimaryColor.ToArgb());
			colorizableCellColors.Add(DocBuilderTheme.DefaultSecondaryColor.ToArgb());

			foreach (var template in ConfigurableTemplateTestHelper.GetAllSystemConfigurableTemplates(Factory))
			{
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(template.ExcelTemplateFullPath);
					excelInterface.ActiveWorksheet = 0;

					var workSheet = excelInterface.WorkSheets[0];

					for (var rowIndex = 0; rowIndex < workSheet.RowCount; rowIndex++)
					{
						for (var columnIndex = 0; columnIndex < workSheet.ColumnCount; columnIndex++)
						{
							if (workSheet[rowIndex, columnIndex].ToString().Trim().Length > 0)
							{
								var cellFormat = workSheet.GetCellFormat(rowIndex, columnIndex);
								if (colorizableCellColors.Contains(cellFormat.BackgroundColor.ToArgb()))
								{
									if (cellFormat.TextColor.ToArgb() != Color.White.ToArgb())
									{
										var cellReference = new CellReference(excelInterface, excelInterface.ActiveWorksheet, rowIndex, columnIndex);
										errors.Append(string.Format("Cell [{0}]  Sheet [{1}]  Template Path [{2}]", cellReference.Cell, cellReference.SheetName, template.ExcelTemplateFullPath));
									}
								}
							}
						}
					}
				}
			}

			Assert("The following cells are colorizable but the font is not white:\r\n" + errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		public void TestSystemSectionRepositoryIsFullyMapped()
		{
			using (MacroValueProviders.Barcode.SuppressErrorTemporarily())
			using (DateTimeFormatStrings.ThrowExceptionOnUnknownDateFormatString())
			{
				BusinessObject declaration = Factory.New(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));

				DocumentWrapper genericFreightJobWrapper = DocumentWrapperFactory.GenerateGenericWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, declaration)[0];
				AssertNotNull("Precondition: genericFreightJobWrapper", genericFreightJobWrapper);

				var systemDocumentElementsTemplates = ConfigurableTemplateTestHelper.GetEnglishSystemConfigurableTemplates(Factory);
				if (TestingState.IsRunningOnDAT)
				{
					Assert("Pre-condition: There is only the English system document elements template.", systemDocumentElementsTemplates.Length == 1);
				}
				else
				{
					Assert("Pre-condition: There is the English system document elements template.", systemDocumentElementsTemplates.Length <= 2);
				}

				foreach (StmTemplateBase template in systemDocumentElementsTemplates)
				{
					using (var report = CreateReport(genericFreightJobWrapper, template))
					{
						ReplaceAllMacros(Factory, report);
						AssertEquals(ReportErrorManager.HasNoErrors, report.ErrorManager.ToString());
					}
				}
			}
		}

		public void TestSystemDocumentElementsForAllLanguagesDoesNotUseTooManyStyles()
		{
			var maximumAllowedStyles = 200;
			var errors = new ZStringBuilder();
			var systemDocumentElementsTemplates = ConfigurableTemplateTestHelper.GetAllSystemConfigurableTemplates(Factory);

			Assert("Pre-condition: There is at least one system document elements template.", systemDocumentElementsTemplates.Length > 0);

			foreach (var template in systemDocumentElementsTemplates)
			{
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					using (var stream = template.GetSO_TemplateReader())
					{
						excelInterface.LoadExcelFile(stream.ToByteArray());
						var styleCount = excelInterface.Xls.StyleCount;

						if (styleCount > maximumAllowedStyles)
						{
							errors.Append(string.Format("The template [{0}] contains {1} styles. The number of styles should not be greater than {2}. ", template.SO_Name, styleCount, maximumAllowedStyles));
						}
					}
				}
			}

			string whatToDoIfTheFanGetsCoated = @"

When this happens, what you can do is add the following macro into the offending spreadsheet(s), run it, then get rid of it:-

Sub DeleteAllStyles()
		On Error Resume Next
		For Each s In ActiveWorkbook.Styles
				ActiveWorkbook.Styles(s.Name).Delete
		Next
End Sub

Then you can load the Spreadsheet into the StmTemplate table and run this test again... :-)
";

			Assert(errors.ToStringWithNewLineBetweenAppends() + whatToDoIfTheFanGetsCoated, errors.IsEmpty);
		}

		[StressTest]
		[SnailTest]
		public void TestRecipientNameAndAddressFollowingByContactName_Macro()
		{
			var regex = new Regex(@"<\s*recipient\s*name\s*and\s*address\s*following\s*by\s*contact\s*name\s*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var error = "{0} - Row: {1} Column: {2} Location: {3}.";
			var errors = new StringBuilder();

			foreach (var template in GetAllTemplates())
			{
				try
				{
					using (var stream = template.GetSO_TemplateReader())
					{
						using (ExcelInterface excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream.ToByteArray());
							for (int worksheetIndex = 0; worksheetIndex < excelInterface.WorkSheets.Count; worksheetIndex++)
							{
								var worksheet = excelInterface.WorkSheets[worksheetIndex];
								for (int rowIndex = 0; rowIndex < worksheet.RowCount; rowIndex++)
								{
									for (int columnIndex = 0; columnIndex < worksheet.ColumnCount; columnIndex++)
									{
										string cellContents = worksheet[rowIndex, columnIndex].ToString();
										if (!string.IsNullOrEmpty(cellContents))
										{
											if (regex.IsMatch(cellContents))
											{
												errors.AppendLine(string.Format(error, GetId(template), rowIndex, columnIndex, template.SO_ExcelTemplatePath));
											}

											foreach (Match match in RegexProvider.InnermostMacrosRegex.Matches(cellContents))
											{
												if (regex.IsMatch(match.Value))
												{
													errors.AppendLine(string.Format(error, GetId(template), rowIndex, columnIndex, template.SO_ExcelTemplatePath));
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
				}
			}

			AssertNoErrors(errors, @"The following templates must not use the <RecipientNameAndAddressFollowingByContactName> macro. 
They should use the <RecipientNameAndIntendedRecipientAddress> macro instead as they do the same thing in a slightly different way.
Once this test is passing, please let Ben Govett know so that he can remove this macro from the DocumentEngine altogether.");
		}

		[StressTest]
		public void TestAllTemplateShouldNotDirectlyUseAddressLine1()
		{
			StringBuilder errors = new StringBuilder();
			string error = "{0} Template Name:{1} - Row: {2} Column: {3} Location: {4}.";

			var templates = FilterExcludedTemplates(GetAllTemplates(false, true), GetAddressFormatterTemplateExclusions());

			foreach (var template in templates)
			{
				try
				{
					using (var stream = template.GetSO_TemplateReader())
					{
						using (ExcelInterface excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream.ToByteArray());
							for (int worksheetIndex = 0; worksheetIndex < excelInterface.WorkSheets.Count; worksheetIndex++)
							{
								var worksheet = excelInterface.WorkSheets[worksheetIndex];
								for (int rowIndex = 0; rowIndex < worksheet.RowCount; rowIndex++)
								{
									for (int columnIndex = 0; columnIndex < worksheet.ColumnCount; columnIndex++)
									{
										string cellContents = worksheet[rowIndex, columnIndex].ToString();
										if (!string.IsNullOrEmpty(cellContents))
										{
											var containAddressLine1 = cellContents.Contains("AddressLine1");
											var containAddress1 = cellContents.Contains("Address1") && cellContents != "<CompanyAddress1>" && cellContents != "<CompanyOfficeAddress1>" && cellContents != "<CompanyPostalAddress1>" && !cellContents.Contains("UserAddress1");

											if (containAddressLine1 || containAddress1)
											{
												if (columnIndex > 1)
												{
													var isReport = template.SO_DataContext == nameof(Core.Constants.DataContext.None);
													var leftCellContent = worksheet[rowIndex, columnIndex - 1].ToString();
													if (!isReport || !leftCellContent.Contains("AdditionalAddress"))
													{
														errors.AppendLine(string.Format(error, containAddressLine1 ? "AddressLine1 Detected:" : "Address1 Detected:", template.SO_Name, rowIndex, columnIndex, template.SO_ExcelTemplatePath));
													}
												}
												else
												{
													errors.AppendLine(string.Format(error, containAddressLine1 ? "AddressLine1 Detected:" : "Address1 Detected:", template.SO_Name, rowIndex, columnIndex, template.SO_ExcelTemplatePath));
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
				}
			}

			AssertNoErrors(errors, @"It's wrong for any template to directly use AddressLine1 rather than a formatted address.
															This test is used to flush out the template which contains word 'AddressLine1' or 'Address1'.
															Please confirm is it necessary to modify your template.");
		}

		[StressTest]
		public void TestAllTemplateShouldNotUseUnrestrictedAdditionalAddressInformation()
		{
			StringBuilder errors = new StringBuilder();
			string error = "{0} Template Name:{1} - Row: {2} Column: {3} Location: {4}.";

			var propertyName = "UnrestrictedAdditionalAddressInformation";

			foreach (var template in GetAllTemplates())
			{
				try
				{
					using (var stream = template.GetSO_TemplateReader())
					{
						using (ExcelInterface excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream.ToByteArray());
							for (int worksheetIndex = 0; worksheetIndex < excelInterface.WorkSheets.Count; worksheetIndex++)
							{
								var worksheet = excelInterface.WorkSheets[worksheetIndex];
								for (int rowIndex = 0; rowIndex < worksheet.RowCount; rowIndex++)
								{
									for (int columnIndex = 0; columnIndex < worksheet.ColumnCount; columnIndex++)
									{
										string cellContents = worksheet[rowIndex, columnIndex].ToString();
										if (!string.IsNullOrEmpty(cellContents))
										{
											if (cellContents.Contains(propertyName))
											{
												errors.AppendLine(string.Format(error, propertyName + " Detected:", template.SO_Name, rowIndex, columnIndex, template.SO_ExcelTemplatePath));
											}
										}
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
				}
			}

			AssertNoErrors(errors, string.Join("The following templates must not use word that contains '" + propertyName + "'", "\r\n",
								"This test is used to flush out the templates which contains word '" + propertyName + "'", "\r\n",
								"Please confirm is it necessary to modify your templates."));
		}

		[StressTest]
		public void TestPrintToFitIsTurnedOffForAllTabsThatArentSetToContinuous()
		{
			List<String> actualResults = new List<String>();
			ZQuery query = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			query.AddToFilter(StmTemplateSchema.SO_IsClientSpecific, false);
			query.AddToFilter(StmTemplateSchema.SO_TemplateType, StmTemplateTypes.Codes.Document);
			var templates = Factory.Load<StmTemplateBase>(query);

			object sync = new object();
			threadCountForTestPrintToFitIsTurnedOffForAllTabs = 0;
			foreach (var template in templates)
			{
				lock (sync)
				{
					threadCountForTestPrintToFitIsTurnedOffForAllTabs++;
				}
				WaitCallback workItem = templatePK =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						try
						{
							BusinessObjectFactory threadFactory = new BusinessObjectFactory();
							ZQuery threadQuery = new ZQuery(StmTemplateSchema.PK, templatePK);
							threadQuery.IncludeBlob(StmTemplateSchema.SO_Template);
							threadQuery.IncludeBlob(StmTemplateSchema.SO_UDFFieldCache);
							var threadTemplate = threadFactory.LoadTop1<StmTemplateBase>(threadQuery);
							using (MemoryStream stream = new MemoryStream(threadTemplate.SO_Template))
							{
								XlsFile excelFile = new XlsFile();
								try
								{
									excelFile.Open(stream);
									for (int index = 1; index <= excelFile.SheetCount; index++)
									{
										excelFile.ActiveSheet = index;
										string sheetName = excelFile.ActiveSheetByName;
										if (Report.IsTemplateSheet(sheetName))
										{
											string configUpper = excelFile.GetCellValue(1, 1).ToString().Trim().ToUpperInvariant();
											if (configUpper != Constants.AreaIdentifierTags.Config)
											{
												actualResults.Add("[" + threadTemplate.SO_ExcelTemplatePath + "] - Tab: " + index.ToString() + "-" + sheetName + "   : *** Missing #Config Section ***");
											}
											else if (excelFile.PrintToFit)
											{
												PageStyles pageStyle = PageStyles.Portrait;
												for (int rowIndex = 2; rowIndex < excelFile.RowCount; rowIndex++)
												{
													string lineTextUpper = excelFile.GetCellValue(rowIndex, 1).ToString().ToUpperInvariant();
													if (lineTextUpper.StartsWith(Constants.ConfigAreaParameters.PageStyleSignature))
													{
														pageStyle = (PageStyles)Enum.Parse(typeof(PageStyles), lineTextUpper.Substring(Constants.ConfigAreaParameters.PageStyleSignature.Length), true);
														break;
													}
													else if (lineTextUpper.StartsWith("#"))
													{
														break;
													}
												}
												if (pageStyle != PageStyles.Continuous)
												{
													actualResults.Add("[" + threadTemplate.SO_ExcelTemplatePath + "] - Tab: " + index.ToString() + "-" + sheetName + "   : Should Not have 'Print to Fit' set.");
												}
											}
										}
									}
								}
								catch (FlexCelXlsAdapterException e)
								{
									actualResults.Add("[" + threadTemplate.SO_ExcelTemplatePath + "] - " + e.ToString());
								}
							}
						}
						finally
						{
							lock (sync)
							{
								threadCountForTestPrintToFitIsTurnedOffForAllTabs--;
							}
						}
					}
				};
				ThreadPool.QueueUserWorkItem(workItem, template.PK);
			}
			while (threadCountForTestPrintToFitIsTurnedOffForAllTabs > 0)
			{
				Thread.Sleep(100);
			}

			actualResults.Sort();
			ZStringBuilder actualResult = new ZStringBuilder(actualResults);
			string expectedExclusionsToBeFixedUnderWI00014883 = @"";
			AssertMultilineASCIIEquals("Templates should not have 'Print to Fit' set unless they have PageStyle=Continuous in their #Config section, otherwise paper scaling for Letter output will not work.", expectedExclusionsToBeFixedUnderWI00014883.Trim(), actualResult.ToStringWithNewLineBetweenAppends());
		}
		static volatile int threadCountForTestPrintToFitIsTurnedOffForAllTabs;

		public void TestNoExcelFunctionalityUsedAroundSingleMacrosStoppingTheFieldFromBeingModifiable()
		{
			var failures = new List<string>();
			var potentialFailures = new List<string>();
			var files = new List<string>();
			var excelLeftFunctionWithOneMacroOnlyRegex = new Regex(@"^=LEFT\([\s]*""<(?<MacroString>.+)>""[\s]*,[\s]*(?<Length>[0-9]+)[\s]*\)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var excelUpperFunctionWithOneMacroOnlyRegex = new Regex(@"^=UPPER\([\s]*""<(?<MacroString>.+)>""[\s]*\)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			var query = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			query.AddToFilter(StmTemplateSchema.SO_IsClientSpecific, false);
			query.AddToFilter(StmTemplateSchema.SO_DataContext, SQLComparisonOperator.NotEqual, "None");
			var templates = Factory.Load<StmTemplateBase>(query);
			foreach (var template in templates)
			{
				using (var stream = template.GetSO_TemplateReader())
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream.ToByteArray());
					foreach (var workSheet in excelInterface.WorkSheets)
					{
						for (var rowIndex = 1; rowIndex <= workSheet.RowCount; rowIndex++)
						{
							for (var columnIndex = 1; columnIndex < workSheet.ColumnCount; columnIndex++)
							{
								var cellValue = workSheet[rowIndex, columnIndex];
								var cellFormula = cellValue as TFormula;
								if (cellFormula != null)
								{
									var cellFormulaText = cellFormula.Text;
									if (RegexProvider.OutermostMacroRegex.Matches(cellFormulaText).Count == 1)
									{
										string replacementText = null;
										var shortTemplateName = template.SO_ExcelTemplatePath.ReplaceIgnoringCase(@"Enterprise\Product\Documents\ExcelTemplates\", "");
										var result = shortTemplateName + " - Cell: " + ExcelWorkSheet.GetCellName(rowIndex, columnIndex) + " [" + cellFormulaText + "]";
										Match match;
										if ((match = excelLeftFunctionWithOneMacroOnlyRegex.Match(cellFormulaText)).Success)
										{
											ZString macroString = match.Groups["MacroString"].ToString();
											ZString length = match.Groups["Length"].ToString();
											replacementText = "<Left(\"<" + macroString + ">\", " + length + ")>";
										}
										else if ((match = excelUpperFunctionWithOneMacroOnlyRegex.Match(cellFormulaText)).Success)
										{
											ZString macroString = match.Groups["MacroString"].ToString();
											replacementText = "<Upper(\"<" + macroString + ">\")>";
										}
										else
										{
											potentialFailures.Add(result);
										}

										if (replacementText != null)
										{
											failures.Add(result + " should be " + replacementText);
											if (!files.Contains(template.SO_ExcelTemplatePath))
											{
												files.Add(template.SO_ExcelTemplatePath);
											}
										}
									}
								}
							}
						}
					}
				}
			}

			failures.Sort();
			var failuresSorted = new ZStringBuilder(failures);
			potentialFailures.Sort();
			var potentialFailuresSorted = new ZStringBuilder(potentialFailures);

			Assert("The following cell formulae should be refactored to use DocumentEngine Macros instead so that the cells will be modifiable."
				+ "\r\n\r\n" + failuresSorted.ToStringWithNewLineBetweenAppends()
				+ "\r\n\r\nYou COULD consider changing some of these to DocumentEngine macros IF you want them to be modifiable too..."
				+ "\r\n\r\n" + potentialFailuresSorted.ToStringWithNewLineBetweenAppends()
				, failuresSorted.IsEmpty);
		}

		public void TestAllSystemDefinedUserConfigurableTemplatesHaveBaseSystemConfigurations()
		{
			ZQuery templateQuery = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			templateQuery.AddToFilter(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System);
			var templates = Factory.Load<StmTemplateBase>(templateQuery);
			CombineAssertions(delegate
			{
				foreach (var template in templates)
				{
					ZQuery pivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_SO, template.PK);
					pivotQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_IsSystemDefined, true);
					StmMenuTemplatePivotBase[] pivots = Factory.Load<StmMenuTemplatePivotBase>(pivotQuery);
					foreach (StmMenuTemplatePivotBase pivot in pivots)
					{
						ZQuery configQuery = new ZQuery(StmMenuDocumentConfigSchema.S3_SI, pivot.PK);
						configQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_IsSystem, true);
						StmMenuDocumentConfig[] configs = Factory.Load<StmMenuDocumentConfig>(configQuery);

						var pivotID = pivot.SI_DocumentTitle;
						var menuItem = pivot.Menu;
						if (menuItem != null)
						{
							pivotID += " (" + menuItem.SU_BusinessContext + " | " + menuItem.SU_MenuPath + "/" + menuItem.SU_MenuName + ")";
						}
						pivotID = "[" + pivotID + "] on Template [" + template.SO_Name + "/" + template.SO_ExcelTemplatePath + "]";

						if (configs.Length == 0)
						{
							Fail("There must be at least one System StmMenuDocumentConfig for Pivot " + pivotID);
						}
						else
						{
							var nonTemplateConfigCount = configs.Count(c => !c.S3_IsTemplate);
							Assert("There must be none or one System StmMenuDocumentConfig for Pivot " + pivotID, nonTemplateConfigCount == 0 || nonTemplateConfigCount == 1);
							Assert("There must be at least one System StmMenuDocumentConfigItem on the System StmMenuDocumentConfig for Pivot " + pivotID, configs.All(c => c.ConfigItems.Count > 0));
						}
					}
				}
			});
		}

		[StressTest]
		public void TestUDFCacheContentsOnAllTemplates()
		{
			List<String> expectedResults = new List<String>();
			List<String> actualResults = new List<String>();
			ZQuery query = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			query.AddToFilter(StmTemplateSchema.SO_Name, SQLComparisonOperator.DoesNotStartWith, SectionRepositoryTemplateNames.System);
			query.AddToFilter(TemplatesToExclude());
			var templates = Factory.Load<StmTemplateBase>(query);

			foreach (var template in templates)
			{
				ZQuery templateQuery = new ZQuery(StmTemplateSchema.PK, template.PK);
				templateQuery.IncludeBlob(StmTemplateSchema.SO_Template);
				templateQuery.IncludeBlob(StmTemplateSchema.SO_UDFFieldCache);
				var templateLoaded = Factory.LoadTop1<StmTemplateBase>(templateQuery);
				ZBlob expectedBlob = templateLoaded.GetUpToDateUDFFieldCacheValue();
				ZBlob actualBlob = templateLoaded.SO_UDFFieldCache;
				if (!expectedBlob.IsEmpty || !actualBlob.IsEmpty)
				{
					StringTreeNode expectedUDFs = StringTreeNode.Deserialise_JsonFormat(expectedBlob);
					StringTreeNode actualUDFs = null;
					try
					{
						actualUDFs = StringTreeNode.Deserialise_JsonFormat(actualBlob);
					}
					catch (Exception ex)
					{
						expectedResults.Add(templateLoaded.SO_ExcelTemplatePath + " [" + expectedUDFs.Children.Count.ToString() + "] : " + expectedUDFs.ToString());
						actualResults.Add(templateLoaded.SO_ExcelTemplatePath + " - Exception: [" + ex.Message + "]");
						continue;
					}

					if (expectedUDFs.Children.Count != 0 || actualUDFs.Children.Count != 0)
					{
						expectedResults.Add(templateLoaded.SO_ExcelTemplatePath + " [" + expectedUDFs.Children.Count.ToString() + "] : " + expectedUDFs.ToString());
						actualResults.Add(templateLoaded.SO_ExcelTemplatePath + " [" + actualUDFs.Children.Count.ToString() + "] : " + actualUDFs.ToString());
					}
				}
			}

			expectedResults.Sort();
			actualResults.Sort();
			ZStringBuilder actualResult = new ZStringBuilder(actualResults);
			ZStringBuilder expectedResult = new ZStringBuilder(expectedResults);
			AssertMultilineASCIIEquals("UDF Cache Contents on all Templates.\r\nPlease update your code to latest version, and regenerate Documents.xml.",
				expectedResult.ToStringWithNewLineBetweenAppends(), actualResult.ToStringWithNewLineBetweenAppends());
		}

		[StressTest]
		public void TestAllTemplatesHaveTheRightDocumentProperties()
		{
			ZStringBuilder failureList = new ZStringBuilder();
			var templates = GetAllTemplates();
			foreach (var template in templates)
			{
				using (DocumentPropertiesTestingHelper testHelper = new DocumentPropertiesTestingHelper(template, failureList))
				{
					testHelper.AssertAddingFailure(TPropertyId.Author, "CargoWise One by WiseTech Global (www.wisetechglobal.com)");
					testHelper.AssertAddingFailure(TPropertyId.Category, "");
					testHelper.AssertAddingFailure(TPropertyId.Comments, "");
					testHelper.AssertAddingFailure(TPropertyId.Company, "");
					testHelper.AssertAddingFailure(TPropertyId.Manager, "");
					testHelper.AssertAddingFailure(TPropertyId.Notes, "");
				}
			}

			string errorMessage = failureList.ToStringWithNewLineBetweenAppends() +
				System.Environment.NewLine +
				">>>> NOTE: Check under Excel (top left window icon) > Prepare > Properties" +
				System.Environment.NewLine;

			Assert(errorMessage, failureList.IsEmpty);
		}

		public void TestAllTemplatesAreUsed()
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(StmTemplateBase));
			filter.AddFilterAndZSQLParameterCollection("SO_PK NOT IN ( SELECT SI_SO FROM dbo.StmMenuTemplatePivot )", new ZSqlParameterCollection());

			IEnumerable<StmTemplateBase> templates = new List<StmTemplateBase>(Factory.Load<StmTemplateBase>(filter));
			templates = FilterExcludedTemplates(templates, GetUnusedTemplateExclusions());
			templates = FilterDocBuilderTemplates(templates);

			StringBuilder errors = new StringBuilder();
			foreach (var template in templates)
			{
				errors.AppendLine(GetId(template));
			}
			AssertNoErrors(errors, "The following templates are not used.");
		}

		[StressTest]
		public void TestAllTemplatesHaveUniqueExcelTemplatePaths()
		{
			var usedTemplatePaths = new Dictionary<string, List<StmTemplateBase>>();

			var templates = GetAllTemplates();
			foreach (var template in templates)
			{
				string templatePath = template.SO_ExcelTemplatePath.ToUpper();

				List<StmTemplateBase> templatesUsingPath;
				if (!usedTemplatePaths.TryGetValue(templatePath, out templatesUsingPath))
				{
					usedTemplatePaths[templatePath] = templatesUsingPath = new List<StmTemplateBase>();
				}
				templatesUsingPath.Add(template);
			}

			var errors = new ZStringBuilder();
			foreach (var usedTemplatePath in usedTemplatePaths)
			{
				var templatesUsingPath = usedTemplatePath.Value;
				if (templatesUsingPath.Count > 1)
				{
					errors.Append("");
					errors.Append(string.Format("Template File: [{0}]:-", templatesUsingPath[0].SO_ExcelTemplatePath));

					foreach (var templateUsingPath in templatesUsingPath)
					{
						string stmTemplateID = String.Format("   - StmTemplate SO_Name: [{0}]", templateUsingPath.SO_Name);
						var clientTemplate = templateUsingPath as ClientSpecificStmTemplateBase;
						if (clientTemplate != null)
						{
							stmTemplateID += String.Format(" in Client Documents.XML: [{0}Documents.XML]", clientTemplate.ClientCode);
						}
						errors.Append(stmTemplateID);
					}
				}
			}

			Assert(@"The following templates have been added multiple times in different StmTemplate rows.
Please either remove the duplicates and re-link references to the duplicates to the correct StmTemplate PK, or create
new Source files for templates where there really should be 2 separate templates.
" + errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[StressTest]
		public void TestAllTemplatesContainSameDataAsExcelFileInSystemTemplates()
		{
			var errors = new StringBuilder();
			foreach (var template in GetAllTemplates())
			{
				if (!template.SO_ExcelTemplatePath.IsEmpty)
				{
					var excelTemplatePath = template.ExcelTemplateFullPath;
					if (File.Exists(excelTemplatePath))
					{
						using (var templateData = template.GetSO_TemplateReader())
						using (var fileData = File.OpenRead(excelTemplatePath))
						{
							if (!templateData.ContainsTheSameDataAs(fileData))
							{
								errors.AppendLine(GetId(template) + " [" + excelTemplatePath + "]");
							}
						}
					}
				}
			}
			AssertNoErrors(errors, "The following templates have different data loaded from the Excel template file.");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllTemplatesContainSameDataAsExcelFileInClientTemplates()
		{
			var errors = new StringBuilder();
			foreach (var template in GetAllClientTemplates())
			{
				if (!template.SO_ExcelTemplatePath.IsEmpty)
				{
					var excelTemplatePath = template.ExcelTemplateFullPath;
					if (File.Exists(excelTemplatePath))
					{
						using (var templateData = template.GetSO_TemplateReader())
						using (var fileData = File.OpenRead(excelTemplatePath))
						{
							if (!templateData.ContainsTheSameDataAs(fileData))
							{
								errors.AppendLine(GetId(template) + " [" + excelTemplatePath + "]");
							}
						}
					}
				}
			}
			AssertNoErrors(errors, "The following templates have different data loaded from the Excel template file.");
		}

		[StressTest]
		public void TestAllTemplatesSavesToFieldIsInTheListOfFilters()
		{
			StringBuilder errors = new StringBuilder();
			Report rpt;
			var templates = FilterExcludedTemplates(GetAllTemplates(), GetCountrySpecificTemplateExclusions());
			foreach (var template in templates)
			{
				if (template.SO_DataContext == nameof(Core.Constants.DataContext.None))
				{
					using (rpt = new Report(new DocumentPack(), template.GetExcelTemplate()))
					{
						rpt.PrepareForRender();
						if (rpt.ColumnHeadingManager.SaveToFilterField != null && rpt.FilterCollection[rpt.ColumnHeadingManager.SaveToFilterField] == null)
						{
							errors.AppendLine(GetId(template));
						}
					}
				}
			}
			AssertNoErrors(errors, "The following templates are contained incorrected 'SavesTo' field. SavesTo field must be equal to one of the filters or must be empty.");
		}

		public void TestAllTemplatesWithAnOrganisationSavesToFieldAreFlaggedAsWeb()
		{
			var errors = new List<string>();
			var templates = FilterExcludedTemplates(GetAllTemplates(false, excludeDocuments: true), GetTemplatesWithAnOrganisationSavesToFieldThatAreNotWebSupportable());
			foreach (var template in templates)
			{
				using (var report = new Report(new DocumentPack(), template.GetExcelTemplate()))
				{
					report.PrepareForRender();
					if (report.LinkedLookupField != null && report.LinkedLookupField.CollectionProvider.ModuleID == ModuleIDs.Organisation)
					{
						var pivots = GetPivots(template);
						foreach (var pivot in pivots)
						{
							var menuItem = pivot.MenuItem;
							if (menuItem.SU_MenuType != Core.Constants.StmMenuItemTypes.WebReports)
							{
								errors.Add(menuItem.SU_BusinessContext + " | " + menuItem.SU_MenuPath + "/" + menuItem.SU_MenuName);
							}
						}
					}
				}
			}

			errors.Sort();

			var errorMessage = string.Join(System.Environment.NewLine, errors);
			var assertionMessage = $@"The following reports use a template that contains a 'SavesTo' field pointing to an Organisation filter but are not marked as 'Supports Web':

{errorMessage}

If your report need to be visible on WebTracker, please tick the 'Supports Web' flag on the Customize Reports form.
If it should not be visible, add it to the list {nameof(GetTemplatesWithAnOrganisationSavesToFieldThatAreNotWebSupportable)}";

			Assert(assertionMessage, string.IsNullOrEmpty(errorMessage));
		}

		public void TestAllTemplatesWithoutAnOrganisationSavesToFieldAreNotFlaggedAsWeb()
		{
			var errors = new List<string>();
			var templates = GetAllTemplates(false, excludeDocuments: true);
			foreach (var template in templates)
			{
				using (var report = new Report(new DocumentPack(), template.GetExcelTemplate()))
				{
					report.PrepareForRender();
					if (report.LinkedLookupField == null || report.LinkedLookupField.CollectionProvider.ModuleID != ModuleIDs.Organisation)
					{
						var pivots = GetPivots(template);
						foreach (var pivot in pivots)
						{
							var menuItem = pivot.MenuItem;
							if (pivot.MenuItem.SU_MenuType == Core.Constants.StmMenuItemTypes.WebReports)
							{
								errors.Add(menuItem.SU_BusinessContext + " | " + menuItem.SU_MenuPath + "/" + menuItem.SU_MenuName);
							}
						}
					}
				}
			}

			errors.Sort();

			var errorMessage = string.Join(System.Environment.NewLine, errors);
			var assertionMessage = $@"The following reports are linked to a template that does not contain a 'SavesTo' field pointing to an Organisation but are marked as 'Supports Web':

{errorMessage}

If your report should not be visible on WebTracker, please untick the 'Supports Web' flag on the Customize Reports form.
If it should be visible, add a 'SavesTo' field poiting to an Organisation filter to the template.";

			Assert(assertionMessage, string.IsNullOrEmpty(errorMessage));
		}

		public void TestUSCountrySpecificTemplatesSavesToFieldIsInTheListOfFilters()
		{
			GlbCompany.CurrentCompany.SetCountry("US");
			StringBuilder errors = new StringBuilder();
			Report rpt;
			var templates = GetUSTemplates();
			foreach (var template in templates)
			{
				if (template.SO_DataContext == nameof(Core.Constants.DataContext.None))
				{
					using (rpt = new Report(new DocumentPack(), template.GetExcelTemplate()))
					{
						rpt.PrepareForRender();
						if (rpt.ColumnHeadingManager.SaveToFilterField != null && rpt.FilterCollection[rpt.ColumnHeadingManager.SaveToFilterField] == null)
						{
							errors.AppendLine(GetId(template));
						}
					}
				}
			}

			AssertNoErrors(errors, "The following templates contain incorrect 'SavesTo' field. SavesTo field must be equal to one of the filters or must be empty.");
		}

		[StressTest]
		[RequiresSoftware(RequiredSoftware.OfficeFonts)]
		public void TestAllTemplatesContainValidFonts()
		{
			var errors = new StringBuilder();
			var templatesToTest = FilterExcludedTemplates(GetAllTemplates(), GetValidFontTemplateExclusions());
			foreach (var template in templatesToTest)
			{
				try
				{
					AutoHeight autoHeight = new AutoHeight();
					string error = "{0} - Row: {1} Column: {2} Font: {3}";

					using (var stream = template.GetSO_TemplateReader())
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream.ToByteArray());
						for (int worksheetIndex = 0; worksheetIndex < excelInterface.WorkSheets.Count; worksheetIndex++)
						{
							var worksheet = excelInterface.WorkSheets[worksheetIndex];
							for (int rowIndex = 0; rowIndex < worksheet.RowCount; rowIndex++)
							{
								for (int columnIndex = 0; columnIndex < worksheet.ColumnCount; columnIndex++)
								{
									string cellContents = worksheet[rowIndex, columnIndex].ToString();
									if (!string.IsNullOrEmpty(cellContents))
									{
										foreach (Match match in RegexProvider.InnermostMacrosRegex.Matches(cellContents))
										{
											if (autoHeight.IsResponsibleForReplacing(match.Value, autoHeight.PassToStartReplacingOn))
											{
												TFlxFont flxFont = worksheet.GetCellFlxFont(rowIndex, columnIndex);
												Font font = worksheet.GetCellFont(rowIndex, columnIndex);
												if (flxFont.Name != font.Name && !ValidFontExclusions.Contains(flxFont.Name))
												{
													errors.AppendLine(string.Format(error, GetId(template), rowIndex, columnIndex, flxFont.Name));
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
				}
			}

			AssertNoErrors(errors, "The following templates have invalid fonts. The AutoHeight macro only works with TrueType fonts.");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[StressTest]
		public void TestAllTemplatesContainValidPaths()
		{
			var errors = new StringBuilder();
			foreach (var template in GetAllTemplates())
			{
				if (template.SO_ExcelTemplatePath.IsEmpty || !File.Exists(template.ExcelTemplateFullPath))
				{
					errors.AppendLine(GetId(template));
				}
			}
			AssertNoErrors(errors, "The following templates have empty or invalid Excel template paths.");
		}

		[StressTest]
		[SnailTest]
		public void TestAllTemplatesHaveValidBooleanExpressionsOnRegistryItems()
		{
			var regex = new Regex(@"""(<RegistryItem\([A-Z.]+,[A-Z.]+\)>)""\s*==\s*""([A-Z]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var error = "Cell: {0} Template path: {1}. Boolean expression: {2}";
			var errors = new StringBuilder();
			foreach (var template in GetAllTemplates())
			{
				try
				{
					using (var stream = template.GetSO_TemplateReader())
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream.ToByteArray());
						for (int worksheetIndex = 0; worksheetIndex < excelInterface.WorkSheets.Count; worksheetIndex++)
						{
							var worksheet = excelInterface.WorkSheets[worksheetIndex];
							for (int rowIndex = 0; rowIndex < worksheet.RowCount; rowIndex++)
							{
								for (int columnIndex = 0; columnIndex < worksheet.ColumnCount; columnIndex++)
								{
									var cellContents = worksheet[rowIndex, columnIndex].ToString();
									if (!string.IsNullOrEmpty(cellContents))
									{
										foreach (Match match in regex.Matches(cellContents))
										{
											var macro = match.Groups[1].ToString();
											var expectedValue = match.Groups[2].ToString();
											if (RegistryItem.IsRegistryValueOfBoolType(macro) &&
												!expectedValue.Equals("True", StringComparison.InvariantCultureIgnoreCase) &&
												!expectedValue.Equals("False", StringComparison.InvariantCultureIgnoreCase))
											{
												var booleanExpression = match.Groups[0].ToString();
												errors.AppendLine(string.Format(error, excelInterface.GetCellReference(rowIndex, columnIndex), template.SO_ExcelTemplatePath, booleanExpression));
											}
										}
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
				}
			}

			AssertNoErrors(errors, "The following templates have invalid syntax in boolean expressions on registry items. The result should be either 'True' or 'False'");
		}

		[StressTest]
		public void TestAllTemplatesContainEnglishOnlyStyles()
		{
			var errors = new StringBuilder();
			const string error = "{0} - Style: {1} - TemplatePath: {2}";
			const string systemDocumentElements = "System Document Elements";

			foreach (var template in GetAllTemplates())
			{
				if (template.SO_Name != systemDocumentElements)
				{
					try
					{
						using (var stream = template.GetSO_TemplateReader())
						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream.ToByteArray());
							for (int i = 1; i <= excelInterface.Xls.StyleCount; i++)
							{
								var styleName = excelInterface.Xls.GetStyleName(i);
								if (!((ZString)styleName).IsWesternEuropeanOrEmpty)
								{
									errors.AppendLine(string.Format(error, GetId(template), styleName, template.SO_ExcelTemplatePath));
								}
							}
						}
					}
					catch (Exception e)
					{
						throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
					}
				}
			}

			AssertNoErrors(errors, "The following templates have invalid style names. Style name must not contain Non-English characters.");
		}

		[StressTest]
		public void TestAllTemplatesListContainsExcelTemplatesSolution()
		{
			var templateFileNames = GetAllTemplates().Select(item => Path.GetFileNameWithoutExtension(item.ExcelTemplateFullPath));
			Assert("EmailCoverSheet", templateFileNames.Contains("EmailCoverSheet"));
			Assert("PrintCoverSheet", templateFileNames.Contains("PrintCoverSheet"));
			Assert("PrintTest", templateFileNames.Contains("PrintTest"));
		}

		[StressTest]
		public void TestAllTemplates_NoEnglishCharactersInPageHeaderOrPageFooter()
		{
			var error = "Template path: {0}. Sheet Name: {1}, Page Header / Page Footer: {2}.";
			var errors = new StringBuilder();

			foreach (var template in FilterExcludedTemplates(GetAllTemplates(), GetTemplateWithEnglishCharactersInPageHeaderExclusions()))
			{
				try
				{
					using (var stream = new MemoryStream(template.SO_Template))
					{
						var excelFile = new XlsFile();
						excelFile.Open(stream);

						for (int i = 1; i <= excelFile.SheetCount; i++)
						{
							excelFile.ActiveSheet = i;
							if (Report.IsTemplateSheet(excelFile.SheetName))
							{
								ValidatePage(excelFile, template, excelFile.PageHeader);
								ValidatePage(excelFile, template, excelFile.PageFooter);
							}
						}
					}
				}
				catch (Exception ex)
				{
					throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, ex);
				}
			}

			AssertNoErrors(errors, "The following templates have invalid PageHeader / PageFooter. PageHeader / PageFooter must not contain English characters. It is in excel setting(Page Layout -> Print Titles -> Custom Header / Custom Footer).");

			void ValidatePage(XlsFile excelFile, StmTemplateBase template, string pageHeaderOrPageFooter)
			{
				if (!string.IsNullOrEmpty(pageHeaderOrPageFooter))
				{
					var result = Regex.Replace(pageHeaderOrPageFooter, "&([^\"]|\"([^\"]*)\")", "");
					if (Regex.IsMatch(result, "[A-Za-z]"))
					{
						errors.AppendLine(string.Format(error, template.SO_ExcelTemplatePath, excelFile.SheetName, pageHeaderOrPageFooter));
					}
				}
			}
		}

		[StressTest]
		public void TestDocumentsContentShouldNotBeOutOfPrintArea_TemplateFromDatabase()
		{
			var templates = GetTemplatesFromDatabase();

			AssertDocumentsContentShouldNotBeOutOfPrintArea(templates);
		}

		public void TestDocumentsContentShouldNotBeOutOfPrintArea_TemplatesFromExcelTemplatesSolution()
		{
			var templates = GetTemplatesFromExcelTemplatesSolution();

			AssertDocumentsContentShouldNotBeOutOfPrintArea(templates);
		}

		public void TestDocumentsContentShouldNotBeOutOfPrintArea_TemplatesFromClient()
		{
			var templates = GetAllClientTemplates(false, false);

			AssertDocumentsContentShouldNotBeOutOfPrintArea(templates);
		}

		public void TestDocumentsDoNotHaveAFilterSheet()
		{
			var templates = GetAllTemplates(false, false, true);
			templates = templates.OrderBy(t => t.SO_ExcelTemplatePath);

			var errorBuilder = new StringBuilder();

			using (var excelInterface = new ExcelInterface())
			{
				foreach (var template in templates)
				{
					using (var stream = template.GetSO_TemplateReader())
					{
						excelInterface.LoadExcelFile(stream.ToByteArray());

						foreach (var workSheet in excelInterface.WorkSheets)
						{
							if (Report.FilterSheetNameRegEx.IsMatch(workSheet.SheetName))
							{
								errorBuilder.AppendLine(template.SO_ExcelTemplatePath);
								break;
							}
						}
					}
				}
			}

			var errorMessage = string.Format(@"Filters are for reports only. Please remove the filter sheet from the following documents:
{0}",
				errorBuilder.ToString());
			Assert(errorMessage, errorBuilder.Length == 0);
		}

		[StressTest]
		public void TestEnsureModifiableMacroIsOnlyAppliedAtCellTopLevel()
		{
			var allowedMacrosRegexesInFrontOfModifiableMacro = new List<Regex>()
			{
				ShrinkToFit.RegexToFindMacroAnyWhereInString,
				ShrinkToFitForBillOfLading.RegexToFindMacroAnyWhereInString,
				AutoHeight.RegexToFindMacroAnyWhereInString,
				ExpandToFit.RegexToFindMacroAnyWhereInString
			};
			var templates = GetAllTemplates(false, true);
			templates = templates.OrderBy(t => t.SO_ExcelTemplatePath);

			var errorBuilder = new StringBuilder();

			var templateNameLogged = false;
			var worksheetNameLogged = false;

			using (var excelInterface = new ExcelInterface())
			{
				foreach (var template in templates)
				{
					using (var stream = template.GetSO_TemplateReader())
					{
						excelInterface.LoadExcelFile(stream.ToByteArray());

						foreach (var workSheet in excelInterface.WorkSheets)
						{
							for (int col = 0; col < workSheet.ColumnCount; col++)
							{
								for (int row = 0; row < workSheet.RowCount; row++)
								{
									var intitialCellContent = workSheet[row, col].ToString().Trim();

									if (Modifiable.RegexToFindMacroAnyWhereInString.IsMatch(intitialCellContent))
									{
										var content = intitialCellContent;
										foreach (var regex in allowedMacrosRegexesInFrontOfModifiableMacro)
										{
											content = regex.Replace(content, string.Empty);
										}

										content = content.Trim();

										if (!Modifiable.MacroRegex.IsMatch(content))
										{
											if (!templateNameLogged)
											{
												templateNameLogged = true;
												errorBuilder.AppendLine(string.Format("Template path: {0}", template.SO_ExcelTemplatePath));
											}

											if (!worksheetNameLogged)
											{
												worksheetNameLogged = true;
												errorBuilder.AppendLine(string.Format("    * work sheet name: {0}", workSheet.SheetName));
											}

											errorBuilder.AppendLine(string.Format("        - cell {0}, content: {1}", excelInterface.GetCellReference(row, col), intitialCellContent));
										}
									}
								}
							}
							worksheetNameLogged = false;
						}
					}

					if (templateNameLogged)
					{
						errorBuilder.AppendLine();
					}

					templateNameLogged = false;
				}
			}

			var errorMessage = string.Format(@"The following templates are using the <Modifiable> macro incorrectly. This macro should not be nested inside another macro.
Please place the <Modifiable> macro at the top level of the cells where it is being used in the following templates:

{0}",
				errorBuilder.ToString());
			Assert(errorMessage, errorBuilder.Length == 0);
		}

		public void TestHVLVShipmentsItemDetailsReportTemplateRowCapDefinedCorrectly()
		{
			var template = Factory.LoadTop1<StmTemplateBase>(new ZQuery(StmTemplateSchema.SO_Name, "HVLV Shipments Item Details Report"));
			const string expectedRowCap = "10000";
			const int expectedConfigWorksheetIndex = 0;
			const int expectedConfigColumnIndex = 0;
			string cellContents = "";
			try
			{
				using (var stream = template.GetSO_TemplateReader())
				{
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream.ToByteArray());
						var worksheet = excelInterface.WorkSheets[expectedConfigWorksheetIndex];
						for (int rowIndex = 0; rowIndex < worksheet.RowCount; rowIndex++)
						{
							cellContents = worksheet[rowIndex, expectedConfigColumnIndex].ToString();
							if (!string.IsNullOrEmpty(cellContents) && cellContents.Contains("MaximumNumberOfRowsToShow"))
							{
								break;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
			}
			Assert("HVLV Shipments Item Details Report template expected to define row cap as MaximumNumberOfRowsToShow=" + expectedRowCap,
				cellContents.Contains("MaximumNumberOfRowsToShow=" + expectedRowCap));
		}

		public void TestHVLVShipmentsItemDetailsHasCorrectFilterName()
		{
			var template = Factory.LoadTop1<StmTemplateBase>(new ZQuery(StmTemplateSchema.SO_Name, "HVLV Shipments Item Details Report"));
			const int importOnlyRow = 80;
			const int exportOnlyRow = 81;
			const int importAndexportOnlyColumn = 3;
			const int expectedConfigWorksheetIndex = 2;
			using (var stream = template.GetSO_TemplateReader())
			{
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream.ToByteArray());
					var worksheet = excelInterface.WorkSheets[expectedConfigWorksheetIndex];
					var importOnly = worksheet[importOnlyRow, importAndexportOnlyColumn].ToString();
					var exportOnly = worksheet[exportOnlyRow, importAndexportOnlyColumn].ToString();

					AssertEquals("The statement should contains the word 'region'", importOnly, "Only Import Shipments to your Login Country/Region");
					AssertEquals("The statement should contains the word 'region'", exportOnly, "Only Export Shipments from your Login Country/Region");
				}
			}
		}

		[SnailTest]
		[DeveloperOnlyTest]
		public void TestAllReportsAllContainsPhoneNumberShouldBeInternaltionFormatted()
		{
			var phones = new StringBuilder();
			var faxes = new StringBuilder();
			var mobiles = new StringBuilder();
			var errorsForAll = new StringBuilder();
			string error = "Template Name:{0} - Row: {1} Column: {2} Location: {3}";
			string pattern = "<FormatPhoneNumber\\(\"<ReportData\\.[\\w]+>\", \"INTERNATIONAL\"\\)>";
			foreach (var template in GetAllTemplates(false, excludeDocuments: true))
			{
				try
				{
					using (var stream = template.GetSO_TemplateReader())
					{
						using (ExcelInterface excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(stream.ToByteArray());
							for (int worksheetIndex = 0; worksheetIndex < excelInterface.WorkSheets.Count; worksheetIndex++)
							{
								var worksheet = excelInterface.WorkSheets[worksheetIndex];

								for (int rowIndex = 0; rowIndex < worksheet.RowCount; rowIndex++)
								{
									for (int columnIndex = 0; columnIndex < worksheet.ColumnCount; columnIndex++)
									{
										string cellContents = worksheet[rowIndex, columnIndex].ToString();
										if (!string.IsNullOrEmpty(cellContents))
										{
											if (Regex.IsMatch(cellContents, "reportdata", RegexOptions.IgnoreCase) &&
												Regex.IsMatch(cellContents, "phone", RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, pattern, RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, "data:", RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, "^#", RegexOptions.IgnoreCase))
											{
												phones.AppendLine(string.Format(error, template.SO_Name, rowIndex, columnIndex, template.SO_ExcelTemplatePath));
												phones.AppendLine($"Contents: {cellContents}");
											}

											if (Regex.IsMatch(cellContents, "reportdata", RegexOptions.IgnoreCase) &&
												Regex.IsMatch(cellContents, "mobile", RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, pattern, RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, "data:", RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, "^#", RegexOptions.IgnoreCase))
											{
												mobiles.AppendLine(string.Format(error, template.SO_Name, rowIndex, columnIndex, template.SO_ExcelTemplatePath));
												mobiles.AppendLine($"Contents: {cellContents}");
											}

											if (Regex.IsMatch(cellContents, "reportdata", RegexOptions.IgnoreCase) &&
												Regex.IsMatch(cellContents, "fax", RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, pattern, RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, "data:", RegexOptions.IgnoreCase) &&
												!Regex.IsMatch(cellContents, "^#", RegexOptions.IgnoreCase))
											{
												faxes.AppendLine(string.Format(error, template.SO_Name, rowIndex, columnIndex, template.SO_ExcelTemplatePath));
												faxes.AppendLine($"Contents: {cellContents}");
											}
										}
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
				}
			}

			if (!string.IsNullOrEmpty(phones.ToString()))
			{
				errorsForAll.AppendLine("Using none formatted phone numbers:");
				errorsForAll.Append(phones);
			}

			if (!string.IsNullOrEmpty(mobiles.ToString()))
			{
				errorsForAll.AppendLine("Using none formatted mobile numbers:");
				errorsForAll.Append(mobiles);
			}

			if (!string.IsNullOrEmpty(faxes.ToString()))
			{
				errorsForAll.AppendLine("Using none formatted fax numbers:");
				errorsForAll.Append(faxes);
			}

			AssertNoErrors(errorsForAll,
				@"This template directly references phone numbers. This indicates it is displaying a phone number with the values direct from the database. 
															This should only be done if the document is specific to a country and the format is defined by legal requirements.
															For all others the template should use an formatted phone number macro so as to benefit from the standardization and formatting set by users on the country record. 
															For assistance on how to resolve this please see Michael Kheirabi or Enguerran Gillet who can advise what macro to utilize instead.");
		}

		[SnailTest]
		public void TestDocumentTemplateShouldReplaceCountryToCountryRegion()
		{
			var errors = new StringBuilder();
			var pattern = @"(.*Country(?!\/Region).*)";
			var index = 1;
			var templateCount = 0;
			var whiteList = GetWhiteListTemplates();
			var allTemplates = GetAllTemplates(excludeClientDocuments: false, excludeFormDocuments: true, excludeReports: true).Where(o => !whiteList.Contains(o.SO_ExcelTemplatePath));
			foreach (var template in allTemplates.OrderBy(o => o.SO_ExcelTemplatePath).ThenBy(o => o.SO_Name))
			{
				try
				{
					var hasCounted = false;
					using var stream = template.GetSO_TemplateReader();
					using var excelInterface = new ExcelInterface();
					excelInterface.LoadExcelFile(stream.ToByteArray());
					for (var worksheetIndex = 0; worksheetIndex < excelInterface.WorkSheets.Count; worksheetIndex++)
					{
						var worksheet = excelInterface.WorkSheets[worksheetIndex];

						for (var rowIndex = 0; rowIndex < worksheet.RowCount; rowIndex++)
						{
							for (var columnIndex = 0; columnIndex < worksheet.ColumnCount; columnIndex++)
							{
								var cellContents = worksheet[rowIndex, columnIndex].ToString();
								if (!string.IsNullOrEmpty(cellContents))
								{
									if (!cellContents.StartsWith("#") && !RegexProvider.InnermostMacrosRegex.IsMatch(cellContents) && Regex.IsMatch(cellContents, pattern, RegexOptions.IgnoreCase))
									{
										errors.AppendLine($"{index++}\t[Template Name]:{template.SO_Name}\t[SheetName]: {worksheet.SheetName}\t[Row]: {rowIndex + 1}\t[Column]: {columnIndex + 1}\t[DataContext]: {template.SO_DataContext}\t[Location]: {template.SO_ExcelTemplatePath}\t [Cell Contents]: {cellContents}");
										if (!hasCounted)
										{
											templateCount++;
											hasCounted = true;
										}
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					throw new DocumentEngineException("Error Processing Template: " + template.SO_Name + "\r\n" + template.SO_ExcelTemplatePath, e);
				}
			}
			AssertNoErrors(errors, $"{templateCount} template(s) should replace country to country/region to compliant law. Please confirm with product about the compliance requirement. If you believe it should not be replaced after confirmation, please add the [Location] of the template to the white list in CountryTemplateWhiteList.txt");
		}

		HashSet<string> GetWhiteListTemplates()
		{
			var countryTemplateWhiteList = new HashSet<string>();
			using var resourceRetriever = new EmbeddedResourceRetriever();
			using var stream = resourceRetriever.GetStream("CountryTemplateWhiteList.txt");
			using var reader = new StreamReader(stream);
			foreach (var line in reader.ReadToEnd().SplitByLine())
			{
				countryTemplateWhiteList.Add(line);
			}

			return countryTemplateWhiteList;
		}

		public void TestBillOfLadingTemplatesDontCombineFormedPagesWithLegacy()
		{
			var legacyMethods = new[] { "MainBodySections", "FollowOnBodySections" };
			var formedPagesMethods = new[] { "FormedPages", "FollowOnSection" };
			var errors = new StringBuilder();

			var allTemplates = GetAllTemplates(false, true, true, false);
			foreach (var template in allTemplates
				.Where(t => t.SO_Name.Contains("Bill of Lading", StringComparison.OrdinalIgnoreCase)
					&& t.SO_DataContext == "Shipment"))
			{
				var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					var containsLegacyMethods = TemplateContainsMethods(report.WorkSheetCurrentlyBeingProcessed, legacyMethods);
					var containsFormedPagesMethods = TemplateContainsMethods(report.WorkSheetCurrentlyBeingProcessed, formedPagesMethods);
					if (containsLegacyMethods && containsFormedPagesMethods)
					{
						errors.AppendLine(template.SO_Name);
					}
				}
			}

			Assert("The following templates contains both Legacy Bill of Lading methods and Formed Pages methods. These should not be combined in a single template.\r\n" + errors.ToString(), errors.Length == 0);
		}

		bool TemplateContainsMethods(ExcelWorkSheet worksheet, string[] methods)
		{
			for (var i = 0; i < worksheet.RowCount; i++)
			{
				for (var j = 0; j < worksheet.ColumnCount; j++)
				{
					foreach (var method in methods)
					{
						if (worksheet.GetCell(i, j).Value != null && worksheet.GetCell(i, j).Value.ToString().Contains(method))
						{
							return true;
						}

						if (worksheet.GetCellFormula(i, j).Contains(method))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		IEnumerable<StmTemplateBase> GetAllTemplates(bool excludeClientDocuments, bool excludeFormDocuments = false, bool excludeReports = false, bool excludeDocuments = false)
		{
			foreach (var item in GetTemplatesFromDatabase(excludeFormDocuments, excludeReports, excludeDocuments))
			{
				yield return item;
			}

			if (!excludeDocuments)
			{
				foreach (var item in GetTemplatesFromExcelTemplatesSolution())
				{
					yield return item;
				}
			}

			if (!excludeClientDocuments)
			{
				foreach (var item in GetAllClientTemplates(excludeReports, excludeDocuments))
				{
					yield return item;
				}
			}
		}

		IEnumerable<StmTemplateBase> GetTemplatesFromDatabase(bool excludeFormDocuments = false, bool excludeReports = false, bool excludeDocuments = false)
		{
			var systemTemplatesQuery = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			systemTemplatesQuery.AddToFilter(StmTemplateSchema.SO_IsClientSpecific, false);
			if (excludeFormDocuments)
			{
				systemTemplatesQuery.AddToFilter(StmTemplateSchema.SO_TemplateType, SQLComparisonOperator.NotEqual, StmTemplateTypes.Codes.Form);
			}
			if (excludeReports)
			{
				systemTemplatesQuery.AddToFilter(StmTemplateSchema.SO_DataContext, SQLComparisonOperator.NotEqual, DataContextValue.None.ToString());
			}
			if (excludeDocuments)
			{
				systemTemplatesQuery.AddToFilter(StmTemplateSchema.SO_DataContext, DataContextValue.None.ToString());
			}

			foreach (var item in new List<StmTemplateBase>(Factory.Load<StmTemplateBase>(systemTemplatesQuery)))
			{
				yield return item;
			}
		}

		IEnumerable<StmTemplateBase> GetTemplatesFromExcelTemplatesSolution()
		{
			foreach (var field in typeof(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames).GetFields())
			{
				string templateName = (string)field.GetValue(null);
				var template = Factory.New<StmTemplateBase>();
				template.SO_ExcelTemplatePath = Path.Combine(@"Enterprise\Product\Documents\ExcelTemplates", templateName + ".xls");
				template.SO_Template = new ExcelTemplateReadFromExcelTemplatesSolution(templateName).GetAsByteArray();
				yield return template;
			}
		}

		IEnumerable<StmTemplateBase> GetAllClientTemplates(bool excludeReports = false, bool excludeDocuments = false)
		{
			foreach (string documentXmlPath in GetClientDocumentXmlPaths())
			{
				var clientTemplates = new List<StmTemplateBase>();
				try
				{
					var dataFile = new ClientDocumentsDataFile(documentXmlPath);
					using (DataSet set = dataFile.DataSet)
					{
						DataTable table = set.Tables[StmTemplateSchema.Constants.TableName];
						if ((table != null) && (table.Rows.Count > 0))
						{
							string clientCode = Path.GetFileName(documentXmlPath).Substring(0, 3);
							foreach (DataRow row in table.Rows)
							{
								var context = row[StmTemplateSchema.Constants.SO_DataContext].ToString();

								if (excludeReports && context == DataContextValue.None.ToString())
								{
									continue;
								}

								if (excludeDocuments && context != DataContextValue.None.ToString())
								{
									continue;
								}

								AddStmTemplateAuditColumns(row);

								var template = new ClientSpecificStmTemplateBase(Factory, row);
								template.ClientCode = clientCode;
								clientTemplates.Add(template);
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					throw new InvalidOperationException(string.Format("Failure Reading: [{0}]", documentXmlPath), ex);
				}
				foreach (var template in clientTemplates)
				{
					yield return template;
				}
			}
		}

		string[] GetClientDocumentXmlPaths()
		{
			var localBinFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var documentXmlsPath = Path.Combine(localBinFolder, @"DocumentXmls");
			Assert($"{documentXmlsPath} should exist.", Directory.Exists(documentXmlsPath));
			var clientDocumentXmlPaths = Directory.GetFiles(documentXmlsPath);
			if (clientDocumentXmlPaths.Length == 0)
			{
				clientDocumentXmlPaths = BuildConstants.GetClientDocumentXmlPaths();
			}
			return clientDocumentXmlPaths;
		}

		void AddStmTemplateAuditColumns(DataRow row)
		{
			StmTemplateAuditColumns.ForEach(column =>
			{
				if (!row.Table.Columns.Contains(column.Name))
				{
					row.Table.Columns.Add(new DataColumn(column.Name, column.DataType));
				}
			});
		}

		static IEnumerable<(string Name, Type DataType)> StmTemplateAuditColumns => new[]
		{
			(StmTemplateSchema.Constants.SO_SystemCreateTimeUtc, typeof(DateTime)),
			(StmTemplateSchema.Constants.SO_SystemCreateUser, typeof(string)),
			(StmTemplateSchema.Constants.SO_SystemLastEditTimeUtc , typeof(DateTime)),
			(StmTemplateSchema.Constants.SO_SystemLastEditUser, typeof(string))
		};

		string GetClientSpecificTemplateName(string clientCode, string templateName)
		{
			return clientCode + '\\' + templateName;
		}

		static string GetId(StmTemplateBase template)
		{
			return new TemplateId(template).ToString();
		}

		static TemplateId[] GetUnusedTemplateExclusions()
		{
			return new TemplateId[]
				{
					new TemplateId("AccountingVoucher", "AccountingVoucher"),
					new TemplateId("DisbursementJobsCloseBatch", "Disbursement Jobs Close Batch"),
					new TemplateId("BankReconciliation", "Bank Reconciliation History"),	// Document is hidden to user; referenced by StmTemplate.SO_PK in BankReconciliation
					new TemplateId("Cheques", "Standard"),
					new TemplateId("Cheques", "HKHSBC"),
					new TemplateId("Cheques", "Singapore"),
					new TemplateId("Cheques", "UKStandard"),
					new TemplateId("Cheques", "USStandard"),
					new TemplateId("Cheques", "USStandard Middle"),
					new TemplateId("Cheques", "USStandard Top"),
					new TemplateId("Cheques", "CAStandard Middle"),
					new TemplateId("Cheques", "CA Standard Top DDMMYYYY"),
					new TemplateId("DepositBatch", "Deposit Slip Bank"),
					new TemplateId("DepositBatch", "Deposit Slip Office"),
					new TemplateId("DocumentDailyWorkSheet", "Drivers Daily WorkSheet"),
					new TemplateId("GLJournal", "General Ledger Journal"),
					new TemplateId("None", "Workflow Validation Failed Report"),
					new TemplateId("Order", "Order Import Report"),
					new TemplateId("ProfitShareDetail", "Profit Share Calculation Worksheet"),
					new TemplateId("Quotation", "Quotation Index"),
					new TemplateId("Quotation", "Quotation One Off"),
					new TemplateId("Rating", "Quotation Table Format"),
					new TemplateId("Shipment", "Bill Of Lading Enhanced CargoWise"),
					new TemplateId("TransactionHeader", "Receipt Matching"),
					new TemplateId("ShippingRating", "Shipping Rates Table Format"),
					new TemplateId("ShippingRating", "Shipping Standard Pricing Page"),
					new TemplateId("CASSBilling", "CASS Discrepancies Report"),
					new TemplateId("CommercialInvoice", "NZ Pref Cert Of Origin for AU"),
					new TemplateId("CommissionPayment", "Commission Payment Summary (Global)"),
					new TemplateId("CommissionPayment", "Commission Payment Summary"),
					new TemplateId("MapGenericFreightJob", "ConstantsSheetReference"),
					new TemplateId("ARComplianceDocument", "Electronic GUI Thermal Paper"),
					new TemplateId("UXML", "BeneficiaryForm"),
					new TemplateId("UXML", "GenericCommercialInvoiceForm"),
					new TemplateId("ComplianceReport", "AP VAT Register Report"),
					new TemplateId("ComplianceReport", "AR VAT Register Report"),
					new TemplateId("ComplianceReport", "Liquidazione IVA"),
					new TemplateId("UXML", "NetherlandsMRNImportNotification"),
					new TemplateId("UXML", "NetherlandsMRNExportNotification"),
				};
		}

		string[] ValidFontExclusions => new string[]
		{
			"GulimChe",
			"BatangChe",
			"DFKai-SB"
		};

		static TemplateId[] GetValidFontTemplateExclusions()
		{
			return new TemplateId[]
				{
					new TemplateId("OSP", "ARInvoice", "OSP ARInvoice"),
					new TemplateId("OSP", "None", "OSP Order Status Summary Report"),
				};
		}

		static TemplateId[] GetTemplatesWithAnOrganisationSavesToFieldThatAreNotWebSupportable()
		{
			return new TemplateId[]
				{
					new TemplateId("None", "NZ Import Invoice Line Report"),
					new TemplateId("None", "Dangerous Goods"),
					new TemplateId("None", "Transport Booking HVLV Shipment Details"),
					new TemplateId("None", "TWH Package On Hand with UNDG Code"),
					new TemplateId("None", "US InBond Report"),
					new TemplateId("None", "CA Export Invoice Line Report"),
					new TemplateId("None", "CA Export Invoice Report")
				};
		}

		static TemplateId[] GetCountrySpecificTemplateExclusions()
		{
			return new TemplateId[]
				{
					new TemplateId("None", "Customs Entries by Broker Report US"),
					new TemplateId("None", "New Template"),
					new TemplateId("EDI", "None", "Client Incident Summary report"),
				};
		}

		static TemplateId[] GetAddressFormatterTemplateExclusions()
		{
			return new TemplateId[]
				{
					new TemplateId(null,".CusEntryHeaderAES", "AESPrint"),
					new TemplateId("WLG","Shipment", "Bill Of Lading Asean Copy"),
					new TemplateId("WLG","Shipment", "Bill Of Lading Asean Original"),
					new TemplateId("WLG","Shipment", "Bill Of Lading CV"),
					new TemplateId("WLG","Shipment", "Bill Of Lading CV Preprinted"),
					new TemplateId(null,"Shipment", "Cargo Inspection Request - Icelandic"),
					new TemplateId(null,".CBPForm7512", "CBPForm7512"),
					new TemplateId(null,".CusEntryHeaderENS", "EntryImmediateDelivery"),
					new TemplateId(null,".CusEntryHeader7501", "EntrySummary"),
					new TemplateId(null,".ENSEntryHeader", "FDARECAP"),
					new TemplateId(null,".FSISImportInspectionAndReport", "FSISForm9540-1ImportInspectionApplicationAndReport - Old"),
					new TemplateId(null,".FSISImportInspectionApplication", "FSISForm9540-1ImportInspectionApplicationAndReport"),
					new TemplateId(null,"GenericFreightJob", "GB Waybill"),
					new TemplateId(null,"GenericFreightJob", "Container Yard EIR In"),
					new TemplateId(null,"GenericFreightJob", "Container Yard EIR Out"),
					new TemplateId(null,"AWB", "Hazardous Cargo Label"),
					new TemplateId(null,"AWB", "Laser Air Waybill"),
					new TemplateId(null,"AWB", "Laser Air Waybill (ESP)"),
					new TemplateId(null,"AWB", "Laser Air Waybill (FRA)"),
					new TemplateId(null,"AWB", "Laser Air Waybill (ITA)"),
					new TemplateId(null,"Organisation", "Organisation US IRS 1096 Form"),
					new TemplateId(null,"AWB", "Pre-Printed Air Waybill"),
					new TemplateId(null,"AWB", "Pre-Printed Air Waybill (ITA)"),
					new TemplateId(null,".Protest", "Protest"),
					new TemplateId(null,"GenericFreightJob", "System Document Elements"),
					new TemplateId(null,"AWB", "Universal Laser MAWB"),
					new TemplateId(null,"AWB", "Universal Neutral MAWB"),
					new TemplateId(null,".USCustomsDeliveryOrder", "US CustomsDeliveryOrder"),
					new TemplateId("YAS","AWB", "YAS Multimodal Transport Document"),
					new TemplateId(null,"None", "Organisation Contact List Report For Mail Merge"),
					new TemplateId(null,"UXML", "ShippingInstructionTemplate"),
					new TemplateId(null,".CusEntryHeaderENS", "PPQForm368NoticeOfArrival - Old"),
					new TemplateId(null,"GenericFreightJobRouting", "A8A In Bond (Routing)"),
					new TemplateId(null,"UXML", "ContainerGrossWeightVerification"),
					new TemplateId(null,".JobDeclaration", "PPQForm368NoticeOfArrival"),
					new TemplateId(null,"Shipment", "Verpflichtungsschein"),
					new TemplateId(null,"UXML", "BillOfLadingDHLPreprinted"),
					new TemplateId(null,"Cartage", "Cartage Cover Sheet"),
					new TemplateId(null,"UXML", "ATFForm6A"),
					new TemplateId(null,"None", "Organisation - Similar Organisations Report (Legacy)"),
					new TemplateId(null,"UXML", "RegulatedAgentAviationSecurityDeclarationTemplate"),
					new TemplateId(null,"UXML", "CargoDuesTemplate"),
					new TemplateId(null,"UXML", "BillOfLadingDHL"),
					new TemplateId(null,".CusEntryHeaderENSACE", "EntryImmediateDeliveryACE"),
					new TemplateId(null,"None", "Client - Booking Summary"),
					new TemplateId(null,"None", "US Export Invoice Report"),
					new TemplateId(null,"HouseBill", "BillOfLadingDHL2"),
					new TemplateId(null,"ShippingInstruction", "ShippingInstruction2Template"),
					new TemplateId(null,"None", "US Export Invoice Line Report"),
					new TemplateId(null,"None", "US Export Declaration Report"),
					new TemplateId(null,"LegacyColdCall", "Sales Cold Call Overview"),
					new TemplateId(null,"None", "Organisation - Agent Referral Report"),
					new TemplateId(null,"ConsolidatedTransportBooking", "Cartage Request"),
					new TemplateId(null,"None", "Payments to IRS 1099 Form Eligible Organisations Summary Report"),
					new TemplateId(null,"CombinedCartageAdvice", "Combined Cartage Advice"),
					new TemplateId(null,"None", "Import Cartage and Delivery Report"),
					new TemplateId(null,"UXML", "CartaDePorteTemplate"),
					new TemplateId(null,"CartageAdvice", "Cartage Advice"),
					new TemplateId(null,"CommonContainer", "LocalCartage Summary Sheet"),
					new TemplateId(null,"Declaration", "MPI Food Safety Clearance Permit Application"),
					new TemplateId(null,"Declaration", "Health Clearance"),
					new TemplateId(null,"UXML", "BillOfLadingYusen"),
					new TemplateId(null,"Consol", "Verpflichtungsschein_Consol"),
					new TemplateId(null,"ARInvoice", "ARInvoice"),
					new TemplateId(null,".PGARecap", "PGARECAP"),
					new TemplateId(null,"UXML", "BookingRequestTemplate"),
					new TemplateId(null,"None", "US IRS 1099-MISC and 1099-NEC Forms"),
					new TemplateId(null,"None", "US IRS 1099-MISC and 1099-NEC Forms 2021"),
					new TemplateId(null,"None", "US IRS 1099-MISC and 1099-NEC Forms 2020"),
					new TemplateId("ARF","Shipment", "ARF HAWB"),
					new TemplateId("DLG","Shipment", "DLG HAWB"),
					new TemplateId("EDI","None", "Cloud Services Client Summary report"),
					new TemplateId("GEO","AWB", "GEO AWB dot-matrix CTD"),
					new TemplateId("ROH","ARInvoice", "ROH ARInvoice"),
					new TemplateId("SCP","AWB", "SkyLift AWB"),
					new TemplateId("SEK","AWB", "SEK HK Laser AWB Dot-Matrix"),
					new TemplateId("TIP","AWB", "TIP Pre-Printed Air Waybill Supreme"),
					new TemplateId("TNT","AWB", "TNT Dot Matrix AWB"),
					new TemplateId("UPE","CusHAWB", "Alternate Broker Split Notification"),
					new TemplateId("UPE","Declaration", "Air Arrival Notice"),
					new TemplateId("UPE","CusHAWB", "Shipment Held Letter for Consignee"),
					new TemplateId("UPE","CusHAWB", "Finance Held Letter for Consignor"),
					new TemplateId("UPE","CusHAWB", "Shipment Held Letter for Consignor"),
					new TemplateId("UPE","CusHAWB", "Finance Held Letter for Consignee"),
					new TemplateId("UPE","CusHAWB", "UPE HAWB"),
					new TemplateId("UPE","None", "CMR Jobs Missing Importer Codes"),
					new TemplateId("UPE","CusHAWB", "Tax Invoice"),
					new TemplateId("UPE","None", "CMR Jobs Missing Supplier Codes"),
					new TemplateId("YAS","Shipment", "YAS Bill of Lading DWU"),
					new TemplateId("YAS","Shipment", "YAS Bill of Lading DWB+DWA+DWP"),
					new TemplateId("YAS","AWB", "YAS Laser Air Waybill"),
					new TemplateId("YAS","AWB", "YAS Neutral HAWB"),
					new TemplateId(null,"None","Address Edit Record"),
					new TemplateId(null,"None","Import MID Report"),
					new TemplateId(null,"None","Taiwan Zero Rated GUI Detailed Report"),
					new TemplateId(null,"None","HVLV Shipments Item Details Report"),
					new TemplateId(null,"None","Client - HVLV Shipments Item Details Report"),
					new TemplateId(null,"None","KR Export Entries Report"),
					new TemplateId(null,"None","KR Export Entry Invoice Lines Report"),
					new TemplateId(null,"None","KR Local Export Entries Report"),
					new TemplateId(null,"None","KR Local Export Entry Invoice Lines Report"),
					new TemplateId(null,"None","CA Export Declaratiopn Report"),
					new TemplateId(null,"None","CA Export Invoice Line Report"),
					new TemplateId(null,"None","CA Export Invoice Report"),
				};
		}

		static TemplateId[] GetTemplateWithEnglishCharactersInPageHeaderExclusions()
		{
			return new TemplateId[]
			{
				new TemplateId("CEO", "None", "COSPAK Order Lines Report"),
				new TemplateId(null, "None", "China Posting Report For Bank"),
				new TemplateId(null, "None", "China Reports Breakdown By Categories"),
				new TemplateId(null, "None", "CN AP Accounting Voucher Report"),
				new TemplateId(null, "None", "CN AR Accounting Voucher Report"),
				new TemplateId(null, "None", "GL TrialBalance_Chinese"),
				new TemplateId(null, "CusEntryHeader", "Estimated/Final Tax Report"),
			};
		}

		static IEnumerable<StmTemplateBase> FilterExcludedTemplates(IEnumerable<StmTemplateBase> templates, TemplateId[] exclusions)
		{
			return templates.Where(template => !Contains(exclusions, template));
		}

		ZQuery TemplatesToExclude()
		{
			ZQuery query = new ZQuery(StmTemplateSchema.SO_Name, SQLComparisonOperator.NotEqual, "Customs Entries by Broker Report US");
			return query;
		}

		List<ClientSpecificTemplate> GetClientFilesOnly()
		{
			var result = new List<ClientSpecificTemplate>();

			foreach (string documentXmlPath in GetClientDocumentXmlPaths())
			{
				ClientDocumentsDataFile dataFile = new ClientDocumentsDataFile(documentXmlPath);
				using (DataSet set = dataFile.DataSet)
				{
					DataTable table = set.Tables[StmTemplateSchema.Constants.TableName];
					if ((table != null) && (table.Rows.Count > 0))
					{
						string clientCode = Path.GetFileName(documentXmlPath).Substring(0, 3);
						foreach (DataRow row in table.Rows)
						{
							AddStmTemplateAuditColumns(row);
							ClientSpecificStmTemplateBase template = new ClientSpecificStmTemplateBase(Factory, row);
							template.ClientCode = clientCode;
							result.Add(new ClientSpecificTemplate() { Template = template, SourceXMLFileName = documentXmlPath });
						}
					}
				}
			}

			return result;
		}

		internal static Report CreateReport(DocumentWrapper genericFreightJobWrapper, StmTemplateBase systemDocumentElementTemplate)
		{
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(systemDocumentElementTemplate);
			return new Report(new DocumentPack(new BusinessObjectFactory().New<StmMenuItem>()), excelTemplate, genericFreightJobWrapper, "Test ReportName", ContactType.Consignor, null, DocumentDirection.ANY, false);
		}

		static string GetTableName(string content)
		{
			var parameters = content.Split(':', ',');
			if (parameters.Length > 1 && parameters[1].StartsWith("DATA=", StringComparison.OrdinalIgnoreCase))
			{
				return parameters[1].Substring(5);
			}

			return string.Empty;
		}

		static string FillFullPrefixForMacro(string macro, Stack<string> tableNames, string currentBodyTableName)
		{
			return RegexProvider.InnermostMacrosRegex.Replace(macro, ReplaceWithFullPrefix);

			string ReplaceWithFullPrefix(Match match)
			{
				var clonedTableNames = new Stack<string>(tableNames.Reverse());
				var macroToReplace = match.Groups[0].Value;
				var macroWithAngleBrackets = macroToReplace.Trim();
				var macroWithoutAngleBrackets = macroWithAngleBrackets.Length > 2 ? macroWithAngleBrackets.Substring(1, macroWithAngleBrackets.Length - 2) : "";

				if (!string.IsNullOrEmpty(macroWithoutAngleBrackets))
				{
					while (clonedTableNames.Count > 0)
					{
						var currentTableName = clonedTableNames.Pop();
						if (macroWithoutAngleBrackets.StartsWith(currentTableName, StringComparison.OrdinalIgnoreCase))
						{
							clonedTableNames.ForEach(n => { macroWithoutAngleBrackets = $"{n}.{macroWithoutAngleBrackets}"; });
							return $"<{currentBodyTableName}.{macroWithoutAngleBrackets}>";
						}
					}
				}

				return macroToReplace;
			}
		}

		internal static void ReplaceAllMacros(BusinessObjectFactory factory, Report report)
		{
			factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			report.Renderer.CurrentAreaToProcess = AreaFactory.InstantiateArea(0, 1, report, Constants.AreaIdentifierTags.Config);

			var currentWorkSheet = report.WorkSheetCurrentlyBeingProcessed;

			currentSection = "#Config";

			var sectionBodyTableName = string.Empty;
			var subTableNameCollection = new Stack<string>();

			for (int row = 0; row < currentWorkSheet.RowCount; row++)
			{
				var headerContent = currentWorkSheet[row, 0] as string;
				if (!string.IsNullOrEmpty(headerContent))
				{
					if (headerContent.ToUpperInvariant().StartsWith(Constants.AreaIdentifierTags.ConfigurableSection))
					{
						currentSection = headerContent;
					}

					if (headerContent.StartsWith("#SectionBody:", StringComparison.OrdinalIgnoreCase))
					{
						sectionBodyTableName = GetTableName(headerContent);
					}

					if (headerContent.StartsWith(Constants.SectionForeachTags.BeginForeach, StringComparison.OrdinalIgnoreCase))
					{
						subTableNameCollection.Push(GetTableName(headerContent));
					}

					if (headerContent.StartsWith(Constants.SectionForeachTags.EndForeach, StringComparison.OrdinalIgnoreCase))
					{
						subTableNameCollection.Pop();
					}
				}

				for (var col = 0; col < currentWorkSheet.ColumnCount; col++)
				{
					var cellContent = currentWorkSheet[row, col];
					using (report.ErrorManager.EvaluatingCell(new CellReference(currentWorkSheet.SheetName + " - " + currentSection, row, col)))
					{
						if (cellContent is TFormula cellFormula)
						{
							cellContent = cellFormula.Text;
						}

						var cellContentText = cellContent.ToString();
						if (!string.IsNullOrEmpty(cellContentText) && !cellContentText.StartsWith("#SectionBody:Data", StringComparison.InvariantCultureIgnoreCase))
						{
							if (subTableNameCollection.Count > 0)
							{
								cellContentText = FillFullPrefixForMacro(cellContentText, subTableNameCollection, sectionBodyTableName);
							}

							using (report.ErrorManager.EvaluatingOuterContent(cellContent))
							{
								var cellContentReplacer = new CellContentReplacer(report, cellContentText);
								if (cellContentReplacer.StillContainsAtLeastOneMacro)
								{
									cellContentReplacer.ReplaceMacros();
								}
							}
						}
					}
				}
			}
		}

		internal static string currentSection;

		StmMenuTemplatePivot[] GetPivots(StmTemplate template)
		{
			var filter = new ZQuery(StmMenuTemplatePivotSchema.SI_SO, template.PK);
			return Factory.Load<StmMenuTemplatePivot>(filter);
		}

		void AssertDocumentsContentShouldNotBeOutOfPrintArea(IEnumerable<StmTemplateBase> templates)
		{
			templates = templates.OrderBy(t => t.SO_ExcelTemplatePath);
			string[] amnestyTemplates = { "DrawbackDeliveryCertificateCBP7552" };
			var errorBuilder = new StringBuilder();

			using (var excelInterface = new ExcelInterface())
			{
				foreach (var template in templates)
				{
					if (!amnestyTemplates.Contains(template.SO_Name.ToString()))
					{
						using (var stream = template.GetSO_TemplateReader())
						{
							excelInterface.LoadExcelFile(stream.ToByteArray());
							if (!StmTemplateBaseValidation.IsWholeTemplateContentInsidePrintArea(excelInterface))
							{
								errorBuilder.AppendLine(template.SO_ExcelTemplatePath);
							}
						}
					}
				}
			}

			var errorMessage = string.Format(@"Template content should not be outside of the print area. Please fix the following templates:
{0}",
				errorBuilder.ToString());
			Assert(errorMessage, errorBuilder.Length == 0);
		}

		int GetChartCount(ExcelFile xlsFile, TShapeProperties props)
		{
			var chartCount = 0;
			if (props.ObjectType == TObjectType.Chart)
			{
				chartCount++;
			}

			for (int i = 1; i <= props.ChildrenCount; i++)
			{
				TShapeProperties childProp = props.Children(i);
				chartCount += GetChartCount(xlsFile, childProp);
			}

			return chartCount;
		}

		IEnumerable<StmTemplateBase> FilterDocBuilderTemplates(IEnumerable<StmTemplateBase> templates)
		{
			return templates.Where(template =>
				template.SO_DataContext != nameof(Core.Constants.DataContext.GenericFreightJob) ||
				(!template.SO_Name.StartsWith(SectionRepositoryTemplateNames.System) && !template.SO_Name.StartsWith(SectionRepositoryTemplateNames.User)));
		}

		static void AssertNoErrors(StringBuilder errors, string description)
		{
			if (errors.Length > 0)
			{
				errors.Insert(0, System.Environment.NewLine);
				errors.Insert(0, System.Environment.NewLine);
				errors.Insert(0, description);
				Assertion.Fail(errors.ToString());
			}
			else
			{
				Assertion.AssertionCount++;
			}
		}

		static bool Contains(TemplateId[] ids, StmTemplateBase template)
		{
			if ((ids != null) && (ids.Length > 0))
			{
				foreach (TemplateId id in ids)
				{
					if (id.Matches(template))
					{
						return true;
					}
				}
			}
			return false;
		}

		List<StmTemplateBase> GetUSTemplates()
		{
			return new List<StmTemplateBase>(Factory.Load<StmTemplateBase>(new ZQuery(Enterprise.ZArchitecture.Schema.StmTemplateSchema.SO_Name, "Customs Entries by Broker Report US")));
		}

		IEnumerable<StmTemplateBase> GetAllTemplates()
		{
			return GetAllTemplates(false);
		}

		int CompareByTemplateName(FailingTemplate x, FailingTemplate y) => x.TemplateName.CompareTo(y.TemplateName);

		int GetStartOfFirstAreaAfterConfig(ExcelWorkSheet worksheet)
		{
			int startOfFirstAreaAfterConfig;
			for (startOfFirstAreaAfterConfig = 1; startOfFirstAreaAfterConfig < worksheet.RowCount; startOfFirstAreaAfterConfig++)
			{
				if (worksheet[startOfFirstAreaAfterConfig, 0].ToString().StartsWith("#"))
				{
					break;
				}
			}
			return startOfFirstAreaAfterConfig;
		}

		bool Compare(DataTable source, DataTable target, out string errorColumns)
		{
			var result = true;
			var tableName = target.TableName;
			var errorBuilder = new ZStringBuilder();
			var errorString = @"{0}.{1} with typeof {2} in xml but {3} in database";

			foreach (DataColumn column in target.Columns)
			{
				var sourceColumn = source.Columns[column.ColumnName];
				if (sourceColumn != null)
				{
					var isColumnTypeSame = sourceColumn.DataType == column.DataType;
					if (!isColumnTypeSame)
					{
						errorBuilder.Append(string.Format(errorString, tableName, column.ColumnName, sourceColumn.DataType.ToString(), column.DataType.ToString()));
					}
					result &= isColumnTypeSame;
				}
			}
			errorColumns = errorBuilder.ToStringWithNewLineBetweenAppends();
			return result;
		}

		void AssertXmlFilesAreCompatibleWithInnerSchema(IEnumerable<string> files)
		{
			var errorString = new StringBuilder();

			foreach (var file in files)
			{
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(file);
				var schemaNode = xmlDocument.GetElementsByTagName("xs:schema")[0];
				schemaNode.ParentNode?.RemoveChild(schemaNode);

				xmlDocument.Schemas.Add(null, XmlReader.Create(new StringReader(schemaNode.OuterXml)));

				ValidationEventHandler validationEventHandler = (sender, validationError) =>
				{
					if (validationError.Severity == XmlSeverityType.Error)
					{
						errorString.AppendLine($"[{validationError.Severity}]-{validationError.Message}, File:{file}, ");
					}
				};
				xmlDocument.Validate(validationEventHandler);
			}

			AssertNullOrEmpty(errorString.ToString());
		}

		void ChangeAllColorizableCellFontsColorToWhite(StmTemplateBase template)
		{
			var colorizableCellColors = new List<int>();
			colorizableCellColors.Add(DocBuilderTheme.DefaultPrimaryColor.ToArgb());
			colorizableCellColors.Add(DocBuilderTheme.DefaultSecondaryColor.ToArgb());

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.ExcelTemplateFullPath);
				excelInterface.ActiveWorksheet = 0;

				var workSheet = excelInterface.WorkSheets[0];

				for (int rowIndex = 0; rowIndex < workSheet.RowCount; rowIndex++)
				{
					for (int columnIndex = 0; columnIndex < workSheet.ColumnCount; columnIndex++)
					{
						if (workSheet[rowIndex, columnIndex].ToString().Trim().Length > 0)
						{
							var cellFormat = workSheet.GetCellFormat(rowIndex, columnIndex);
							if (colorizableCellColors.Contains(cellFormat.BackgroundColor.ToArgb()))
							{
								if (cellFormat.TextColor.ToArgb() != Color.White.ToArgb())
								{
									cellFormat.FontSize = 8;
									cellFormat.FontStyle = FontStyle.Bold;
									cellFormat.TextColor = Color.White;
									workSheet.SetCellFormat(rowIndex, columnIndex, cellFormat);
								}
							}
						}
					}
				}

				excelInterface.SaveToFile(template.ExcelTemplateFullPath);
			}
		}

		sealed class ClientSpecificTemplate
		{
			public ClientSpecificStmTemplateBase Template;
			public string SourceXMLFileName;
		}

		sealed class ClientSpecificStmTemplateBase : StmTemplateBase
		{
			string clientCode;
			bool decompressed;

			public ClientSpecificStmTemplateBase(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string ClientCode
			{
				get { return clientCode; }
				set { clientCode = value; }
			}

			public override ZBool SO_IsUserConfigurable
			{
				get { return false; }
				set { }
			}

			public override ZBlob SO_Template
			{
				get
				{
					EnsureSO_Template();
					return base.SO_Template;
				}
				set { base.SO_Template = value; }
			}

			public override Stream GetSO_TemplateReader()
			{
				EnsureSO_Template();
				return base.GetSO_TemplateReader();
			}

			void EnsureSO_Template()
			{
				if (!decompressed)
				{
					decompressed = true;
					base.SO_Template = Compressor.Uncompress(base.SO_Template);
				}
			}
		}

		sealed class FailingTemplate
		{
			public FailingTemplate(string templateName)
			{
				TemplateName = templateName;
				Fields = new List<UDFFieldInstance>();
			}

			public readonly string TemplateName;

			public readonly List<UDFFieldInstance> Fields;
		}

		sealed class UDFField
		{
			internal UDFField(string fieldName)
			{
				FieldName = fieldName;
				HasDifferingDefinitions = false;
			}

			internal readonly string FieldName;

			internal readonly List<UDFFieldInstance> Instances = new List<UDFFieldInstance>();

			internal bool HasDifferingDefinitions
			{
				get;
				private set;
			}

			internal void AddInstance(UDFFieldInstance fieldInstance)
			{
				if (!HasDifferingDefinitions && Instances.Count > 0)
				{
					var firstInstance = Instances[0];
					if (fieldInstance.Definition != firstInstance.Definition)
					{
						HasDifferingDefinitions = true;
					}
				}

				Instances.Add(fieldInstance);
			}

			string AddTitleTo(string definitions)
			{
				return FieldName + "\r\n" + "=".PadRight(LineWidth, '=') + "\r\n" + definitions;
			}

			internal void SetExpectedDefinitions(string[] values)
			{
				ExpectedTabPage = values[0];
				ExpectedDefaultValue = values[1];
			}

			internal string ExpectedTabPage
			{
				get;
				private set;
			}

			internal string ExpectedDefaultValue
			{
				get;
				private set;
			}

			internal string GetExpectedDefinitions()
			{
				var result = new ZStringBuilder();
				foreach (var fieldInstance in Instances)
				{
					result.Append(fieldInstance.TemplatePath + "\r\n" + GetKey(ExpectedTabPage, ExpectedDefaultValue) + "\r\n" + "-".PadRight(LineWidth, '-'));
				}
				return AddTitleTo(result.ToStringWithNewLineBetweenAppends());
			}

			internal string GetActualDefinitions()
			{
				var result = new ZStringBuilder();
				foreach (var fieldInstance in Instances)
				{
					result.Append(fieldInstance.TemplatePath + "\r\n" + GetKey(fieldInstance.TabPage, fieldInstance.DefaultValue) + "\r\n" + "-".PadRight(LineWidth, '-'));
				}
				return AddTitleTo(result.ToStringWithNewLineBetweenAppends());
			}

			const int LineWidth = 50;

			internal string GetGroupedDefinitions()
			{
				var defaultValues = new Dictionary<string, List<UDFFieldInstance>>();
				foreach (var fieldInstance in Instances)
				{
					var key = GetKey(fieldInstance.TabPage, fieldInstance.DefaultValue);
					if (!defaultValues.TryGetValue(key, out var instances))
					{
						defaultValues[key] = instances = new List<UDFFieldInstance>();
					}
					instances.Add(fieldInstance);
				}

				var sortedValues = new List<KeyValuePair<string, List<UDFFieldInstance>>>(defaultValues);
				sortedValues.Sort(CompareFieldImplementations);

				var result = new ZStringBuilder();
				foreach (var defaultValue in sortedValues)
				{
					result.Append(defaultValue.Key);
					foreach (var fieldInstance in defaultValue.Value)
					{
						result.Append("   " + fieldInstance.TemplatePath);
					}
				}
				return result.ToStringWithNewLineBetweenAppends();
			}

			string GetKey(string tabPage, string defaultValue)
			{
				return "Tab: [" + tabPage + "] Default: [" + defaultValue + "]    correctDefaultValues.Add(\"" + FieldName + "\", new string[] { \"" + tabPage + "\", \"" + defaultValue + "\" });";
			}

			int CompareFieldImplementations(KeyValuePair<string, List<UDFFieldInstance>> x, KeyValuePair<string, List<UDFFieldInstance>> y)
			{
				var result = y.Value.Count.CompareTo(x.Value.Count);
				return result == 0 ? x.Key.CompareTo(y.Key) : result;
			}
		}

		sealed class UDFFieldInstance
		{
			internal UDFFieldInstance(UDFField parentField, StmTemplateBase template, StringTreeNode rootNode)
			{
				Field = parentField;
				Template = template;
				Name = rootNode.Value;
				DefaultValue = "";
				TabPage = "";

				level = 0;
				contents = new ZStringBuilder();
				AddChildren(rootNode.Children);
				Definition = contents.ToStringWithNewLineBetweenAppends();
			}

			internal readonly UDFField Field;
			int level;
			readonly ZStringBuilder contents;

			void AddChildren(StringTreeNodeCollection children)
			{
				var sortedChildren = new StringTreeNode[children.Count];
				children.CopyTo(sortedChildren);
				Array.Sort(sortedChildren, CompareByName);
				foreach (var childNode in sortedChildren)
				{
					contents.Append("".PadLeft(level * 3) + childNode.Value);

					DefaultValue = GetSingleChildNodeValueIfMatches(childNode, "Default", DefaultValue);
					TabPage = GetSingleChildNodeValueIfMatches(childNode, "Tab", TabPage);

					level++;
					try
					{
						AddChildren(childNode.Children);
					}
					finally
					{
						level--;
					}
				}
			}

			static string GetSingleChildNodeValueIfMatches(StringTreeNode childNode, string elementName, string currentValue)
			{
				if (childNode.Value.Equals(elementName, StringComparison.InvariantCultureIgnoreCase))
				{
					if (childNode.Children.Count != 1)
					{
						throw new InvalidOperationException(string.Format("Must have one '{1}' child value when specified. Found [{0}] child values instead.", childNode.Children.Count.ToString(), elementName));
					}
					var defaultValue = childNode.Children[0].Value;
					if (!string.IsNullOrEmpty(defaultValue) && !string.IsNullOrEmpty(currentValue))
					{
						throw new InvalidOperationException(string.Format("Cannot have more than one '{2}' element. [{0}] and [{1)]", defaultValue, currentValue, elementName));
					}
					currentValue = defaultValue ?? "";
				}
				return currentValue;
			}

			int CompareByName(StringTreeNode x, StringTreeNode y)
			{
				return x.Value.CompareTo(y.Value);
			}

			readonly StmTemplateBase Template;
			internal readonly string Name;

			internal string TabPage
			{
				get;
				private set;
			}

			internal string DefaultValue
			{
				get;
				private set;
			}

			internal readonly string Definition;

			internal string TemplatePath => "Template: [" + Template.ExcelTemplateFullPath + "]";
		}

		sealed class UDFFieldDefinition
		{
			public UDFFieldDefinition(string fieldName)
			{
				FieldName = fieldName;
				FieldTag = "<" + fieldName.ToLowerInvariant() + ">";
			}

			internal string FieldName { get; private set; }

			internal string FieldTag { get; private set; }

			internal bool IsUsed;
		}

		sealed class DocumentPropertiesTestingHelper : IDisposable
		{
			public DocumentPropertiesTestingHelper(StmTemplateBase stmTemplate, ZStringBuilder failureList)
			{
				TemplateID = !stmTemplate.SO_ExcelTemplatePath.IsEmpty ? stmTemplate.SO_ExcelTemplatePath.ToString() : stmTemplate.PK.ToString();

				using (var stream = stmTemplate.GetSO_TemplateReader())
				{
					ExcelInterface = new ExcelInterface();
					ExcelInterface.LoadExcelFile(stream.ToByteArray());
					DocumentProperties = ExcelInterface.Xls.DocumentProperties;
					FailureList = failureList;
					AddedHeader = false;
				}
			}
			readonly string TemplateID;
			readonly ExcelInterface ExcelInterface;
			readonly ZStringBuilder FailureList;
			readonly TDocumentProperties DocumentProperties;
			bool AddedHeader;

			public void AssertAddingFailure(TPropertyId propertyID, ZString expected)
			{
				ZString actual = "";
				try
				{
					actual = (DocumentProperties.GetStandardProperty(propertyID) ?? "").ToString();
				}
				catch (Exception e)
				{
					if (e is IOException || e is FlexCelXlsAdapterException)
					{
						e = null;
						try
						{
							actual = (ExcelInterface.Xls.DocumentProperties.GetStandardProperty(propertyID) ?? "").ToString();
						}
						catch (Exception internalException)
						{
							e = internalException;
						}
					}
					if (e != null)
					{
						AddFailureLine("*** EXCEPTION getting " + propertyID.ToString() + "*** - " + e.GetType().ToString() + ":" + e.Message);
					}
				}
				if (expected != actual)
				{
					AddFailureLine(propertyID.ToString() + " - Expected: [" + expected + "] - Actual: [" + actual + "]");
				}
			}

			void AddFailureLine(ZString message)
			{
				if (!AddedHeader)
				{
					AddedHeader = true;
					FailureList.Append(("---[" + TemplateID + "]").PadRight(80, '-'));
				}
				FailureList.Append(message);
			}

			void IDisposable.Dispose()
			{
				if (AddedHeader)
				{
					FailureList.Append(".");
				}
				if (ExcelInterface != null)
				{
					ExcelInterface.Dispose();
				}
			}
		}

		struct TemplateId
		{
			readonly string clientCode;
			readonly string dataContext;
			readonly string name;

			public TemplateId(StmTemplateBase template)
				: this(GetClientCode(template), template.SO_DataContext, template.SO_Name)
			{
			}

			public TemplateId(string dataContext, string name)
				: this(null, dataContext, name)
			{
			}

			public TemplateId(string clientCode, string dataContext, string name)
			{
				this.clientCode = clientCode;
				this.dataContext = dataContext;
				this.name = name;
			}

			public string ClientCode
			{
				get { return clientCode; }
			}

			public string DataContext
			{
				get { return dataContext; }
			}

			public string Name
			{
				get { return name; }
			}

			public bool Matches(StmTemplateBase template)
			{
				return (GetClientCode(template) == ClientCode) && (DataContext == template.SO_DataContext) && (Name == template.SO_Name);
			}

			public override string ToString()
			{
				string result = DataContext + " -> " + Name;
				if (!string.IsNullOrEmpty(ClientCode))
				{
					result = ClientCode + " -> " + result;
				}
				return result;
			}

			static string GetClientCode(StmTemplateBase template)
			{
				var clientSpecificTemplate = template as ClientSpecificStmTemplateBase;
				return (clientSpecificTemplate == null) ? null : clientSpecificTemplate.ClientCode;
			}

			public bool IsClientSpecific
			{
				get { return !string.IsNullOrEmpty(ClientCode); }
			}
		}
	}
}
