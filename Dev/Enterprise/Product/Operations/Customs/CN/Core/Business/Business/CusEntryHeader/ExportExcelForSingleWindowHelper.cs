using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class ExportExcelForSingleWindowHelper
	{
		public ExportExcelForSingleWindowHelper(IEnumerable<CusEntryHeader> entryHeaders)
		{
			EntryHeaders = Argument.NotNull(entryHeaders, nameof(entryHeaders));
		}
		IEnumerable<CusEntryHeader> EntryHeaders { get; set; }

		BusinessObjectFactory Factory => EntryHeaders.FirstOrDefault()?.Factory;

		const string ExcelTemplateName = "CNEntryDataForSingleWindow";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Description with western-only characters allowed.")]
		const string DocumentDescription = "Excel for Single Window Importing";

		StmTemplate ExcelTemplate
		{
			get
			{
				if (fExcelTemplate == null)
				{
					var filter = new ZQuery();
					filter.AddToFilter(StmTemplateSchema.SO_Name, ExcelTemplateName);
					filter.AddToFilter(StmTemplateSchema.SO_DataContext, CusEntryHeaderDocumentSupporter.CustomsDeclarationDocument);
					filter.AddToFilter(StmTemplateSchema.SO_IsSystemDefined, true);

					fExcelTemplate = Factory.LoadTop1<StmTemplate>(filter);
				}
				return fExcelTemplate;
			}
		}
		StmTemplate fExcelTemplate;

		public ZString ExportAndSaveAsEDocs()
		{
			var messages = new ZStringBuilder();

			if (EntryHeaders.Any())
			{
				if (ExcelTemplate == null)
				{
					messages.Append(ResString.GetMultilingualString("0A2D50A6-55B8-4CAD-9B37-D026D71FE8B9", "Cannot find Excel Template {0}, Data Context {1}.", ExcelTemplateName, CusEntryHeaderDocumentSupporter.CustomsDeclarationDocument));
				}
				else
				{
					var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(ExcelTemplate);
					foreach (var entryHeader in EntryHeaders)
					{
						var saveResult = ExportAndSaveAsEDoc(entryHeader, excelTemplate);
						messages.Append(saveResult);
					}
				}
			}

			return messages.ToStringWithNewLineBetweenAppends();
		}

		ZString ExportAndSaveAsEDoc(CusEntryHeader entryHeader, ExcelTemplateReadFromStmTemplateTable excelTemplate)
		{
			ZString message;

			var documentFactory = (entryHeader as IDocManagerSupport).DocManagerInfo.MasterFactory;
			var storageMain = documentFactory.RetrieveExistingOrCreateStorageMain(entryHeader, Core.Constants.DocManagerCodes.CustomsEntry);

			using (var report = new Report(null, excelTemplate, GetDataProviderList(entryHeader), DocumentDescription, null, DocumentDirection.ANY, false))
			using (var stream = new MemoryStream())
			{
				report.Save(stream);
				var doc = storageMain.AddFileOrDocument(GetExcelContents(stream), GetFileName(entryHeader), Core.Constants.RefDocTypes.MiscellaneousDocument, true, description: DocumentDescription);

				try
				{
					documentFactory.Save();
					message = ResString.GetMultilingualString("59E7A71E-D0CF-44AB-B51A-7D02642304D9", "Excel for {0} has been saved to eDocs.", entryHeader.LocalReferenceNumber);
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
					message = ResString.GetMultilingualString("FDAFBC4D-77EB-4970-AC18-15399D95BD75", "Excel for {0} has failed to be generated.", entryHeader.LocalReferenceNumber);
				}
			}

			return message;
		}

		DataProviderList GetDataProviderList(CusEntryHeader entryHeader)
		{
			var dataProviders = new List<IBODocDataProvider>();
			dataProviders.AddRange(entryHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.CustomsDeclarationDocument), null));
			dataProviders.Add(BODocDataProvider.Get(entryHeader));

			return new DataProviderList(dataProviders.ToArray());
		}

		string GetFileName(CusEntryHeader entryHeader) => entryHeader.LocalReferenceNumber + Path.GetExtension(ExcelTemplate.SO_ExcelTemplatePath);

		byte[] GetExcelContents(Stream originStream)
		{
			originStream.Position = 0;

			using (var excelInterface = new ExcelInterface())
			using (var outputStream = new MemoryStream())
			{
				excelInterface.LoadExcelFile(originStream);
				var xlsFile = excelInterface.Xls;
				for (int i = 1; i <= xlsFile.SheetCount; i++)
				{
					xlsFile.ActiveSheet = i;
					xlsFile.SetColHidden(1, false);
					xlsFile.SetColWidth(1, 7000);
				}
				xlsFile.ActiveSheet = 1;
				excelInterface.SaveToStream(outputStream);

				return outputStream.ToArray();
			}
		}
	}
}
