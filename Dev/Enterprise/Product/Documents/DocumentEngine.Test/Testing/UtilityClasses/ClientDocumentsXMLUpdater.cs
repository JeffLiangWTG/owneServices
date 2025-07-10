using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.BuildTools;
using CargoWise.EntityFramework;
using Enterprise.DbUpgrader.Data;
using Enterprise.DocumentEngine.Build;
using Enterprise.DocumentEngine.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	public class ClientDocumentsXMLUpdater
	{
		public ClientDocumentsXMLUpdater()
		{
			factory = new BusinessObjectFactory();
		}

		readonly BusinessObjectFactory factory;
		static readonly Regex documentClientCodeRegex = new Regex(@"Enterprise\\Product\\Documents\\ExcelTemplates\\Documents\\(?<code>\w{3})\\", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		static readonly Regex reportClientCodeRegex = new Regex(@"Enterprise\\Product\\Documents\\ExcelTemplates\\Reports\\(?<code>\w{3})\\", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		public static string GetTemplateClientCode(string templateFilePath)
		{
			var documentMatch = documentClientCodeRegex.Match(templateFilePath);
			var reportMatch = reportClientCodeRegex.Match(templateFilePath);
			return documentMatch.Success ? documentMatch.Groups["code"].Value : reportMatch.Success ? reportMatch.Groups["code"].Value : null;
		}

		public void WriteAllCheckedOutClientTemplatesToXml()
		{
			string[] filesWithPendingChanges = SourceControl.EnterpriseDatabase.GetFilesWithPendingChanges();
			foreach (string fileWithPendingChange in filesWithPendingChanges)
			{
				if (GetTemplateClientCode(fileWithPendingChange) != null)
				{
					WriteClientTemplateToXml(fileWithPendingChange);
				}
			}
		}

		DocumentsSetupController setupController;
		DocumentsSetupController SetupController
		{
			get { return setupController ?? (setupController = new DocumentsSetupController()); }
		}

		public void WriteClientTemplateToXml(string templateFilePath)
		{
			if (!SetupController.IsCheckedOutByMe)
			{
				SetupController.FullCheckOut();
			}

			string clientCode = GetTemplateClientCode(templateFilePath) ?? throw new InvalidOperationException(String.Format(@"Could not get Client Code. Client templates should reside under: [Enterprise\Product\Documents\ExcelTemplates\(Documents || Reports)\(Client Code)\"));
			string documentXmlPath = BuildConstants.GetClientDocumentXmlPath(clientCode);
			if (!SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(documentXmlPath))
			{
				SourceControl.EnterpriseDatabase.CheckOut(documentXmlPath, false);
			}

			WriteTemplateToXml(documentXmlPath, templateFilePath);
		}

		public void WriteTemplateToXml(string documentXmlPath, string templateFilePath)
		{
			ClientDocumentsDataFile dataFile = new ClientDocumentsDataFile(documentXmlPath);
			using (DataSet set = dataFile.DataSet)
			{
				DataTable table = set.Tables[StmTemplateSchema.Constants.TableName] ?? throw new InvalidOperationException(string.Format("Could not find the StmTemplate table in the Client Documents.XML file [{0}].", documentXmlPath));
				foreach (DataRow row in table.Rows)
				{
					var rowFileName = BuildConstants.LocalEnterprisePath + row[StmTemplateSchema.Constants.SO_ExcelTemplatePath];
					if (rowFileName.ToUpper().Equals(templateFilePath.ToUpper()))
					{
						var template = factory.New<StmTemplateBase>();
						template.SO_Name = (string)row[StmTemplateSchema.Constants.SO_Name];
						template.SO_Template = File.ReadAllBytes(templateFilePath);

						row[StmTemplateSchema.Constants.SO_Template] = (byte[])template.SO_Template;
						row[StmTemplateSchema.Constants.SO_UDFFieldCache] = (byte[])template.SO_UDFFieldCache;
						dataFile.WriteXml(set, documentXmlPath, XmlWriteMode.WriteSchema);
						break;
					}
				}
			}
		}

		public string ScanAndFixClientMenuPathsRemovingLeadingAndTrailingSlashes()
		{
			var clientDocumentXMLPaths = BuildConstants.GetClientDocumentXmlPaths();
			var results = new List<string>();
			results.Add("Reading " + clientDocumentXMLPaths.Length + " Client Document.XML files...");
			int filesFoundWithProblems = 0;
			int problemsFound = 0;
			try
			{
				foreach (string clientDocumentXMLPath in clientDocumentXMLPaths)
				{
					ClientDocumentsDataFile dataFile = new ClientDocumentsDataFile(clientDocumentXMLPath);
					using (DataSet set = dataFile.DataSet)
					{
						DataTable table = set.Tables[StmMenuItemSchema.Constants.TableName] ?? throw new InvalidOperationException(string.Format("Could not find the StmMenuItem table in the Client Documents.XML file [{0}].", clientDocumentXMLPath));

						bool fileHasProblems = false;
						foreach (DataRow row in table.Rows)
						{
							string menuPath = (string)row[StmMenuItemSchema.Constants.SU_MenuPath];
							if (menuPath.StartsWith("/") || menuPath.EndsWith("/"))
							{
								fileHasProblems = true;
								problemsFound++;
								row[StmMenuItemSchema.Constants.SU_MenuPath] = menuPath.Trim('/', ' ');
							}
						}

						if (fileHasProblems)
						{
							filesFoundWithProblems++;

							if (!SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(clientDocumentXMLPath))
							{
								SourceControl.EnterpriseDatabase.CheckOut(clientDocumentXMLPath, false);
							}

							dataFile.WriteXml(set, clientDocumentXMLPath, XmlWriteMode.WriteSchema);
							results.Add("fixed:- " + clientDocumentXMLPath);
						}
					}
				}
				results.Add("Fixed " + filesFoundWithProblems + " files with " + problemsFound + " problems.");
			}
			catch (Exception exception)
			{
				results.Add(exception.ToString());
			}

			return string.Join("\r\n", results.ToArray());
		}
	}
}
