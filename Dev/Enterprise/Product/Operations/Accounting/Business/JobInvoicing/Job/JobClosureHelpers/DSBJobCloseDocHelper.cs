using System;
using System.Globalization;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DSBJobCloseDocHelper
	{
		public DSBJobCloseDocHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constructing file name")]
		const string templateName = "Disbursement Jobs Close Batch";

		public void CreateAndAttachBatchDocument(DsbJobCloseBatch batch, GLJournal journal)
		{
			var wrapperProvider = ObjectFactory.Get<IDocDsbJobCloseBatchProvider>() ?? throw new InvalidOperationException("Cannot find IDocDsbJobCloseBatchProvider implementation via ObjectFactory.");

			var wrapper = wrapperProvider.CreateDocDsbJobCloseBatch(batch, Factory);
			var language = Factory.Load<GlbCompany>(batch.JBB_GC)?.Language ?? Env.CurrentUser.Language;
			var template = LoadStmTemplate(Factory, templateName);

			var rawDocument = GetRawDocumentInPdf(template, wrapper, language);
			var eDocFileName = string.Format(CultureInfo.InvariantCulture, (NoResString)"Disbursement Job Close Batch Document - {0}.pdf", batch.JBB_BatchNumber);

			var eDoc = journal.DocManagerInfo.AddFileOrDocument(rawDocument, eDocFileName, Core.Constants.RefDocTypes.MiscellaneousDocument);
			((StorageDocsBase)eDoc).SC_IsSystemGenerated = true;

			journal.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
		}

		StmTemplate LoadStmTemplate(BusinessObjectFactory factory, ZString templateName)
		{
			ZQuery query = new ZQuery(StmTemplateSchema.SO_Name, templateName);
			query.AddToFilter(StmTemplateSchema.SO_DataContext, Enterprise.Core.Constants.DataContext.DisbursementJobsCloseBatch);
			return factory.LoadTop1<StmTemplate>(query);
		}

		byte[] GetRawDocumentInPdf(StmTemplate template, IBODocDataProvider wrapper, string language)
		{
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
			using (var documentPack = new DocumentPack())
			{
				documentPack.Language = language;

				using (var report = new Report(documentPack, excelTemplate, wrapper, (NoResString)"Report", null, DocumentDirection.ANY, false))
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						using (MemoryStream pdfStream = new MemoryStream())
						{
							excelInterface.ExportToPdfAndScale(pdfStream, 100);
							return pdfStream.ToArray();
						}
					}
				}
			}
		}

		BusinessObjectFactory Factory { get; set; }
	}
}
