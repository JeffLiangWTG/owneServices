using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CusEntryHeaderAttachmentGenerator
	{
		public CusEntryHeaderAttachmentGenerator(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			docManagerSupport = entryHeader;
			documentFactory = docManagerSupport.DocManagerInfo.MasterFactory;
			storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(entryHeader, Core.Constants.DocManagerCodes.CustomsEntry);
		}
		readonly CusEntryHeader entryHeader;
		readonly IDocManagerSupport docManagerSupport;
		readonly IStorageMain storageMain;
		readonly IDocumentFactory documentFactory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "For string operation")]
		const string SystemString = "(System)";

		public ZString GenerateAttachments()
		{
			var messages = new ZStringBuilder();

			var settings = GetApplicableSettings(messages);
			if (settings != null && settings.Any())
			{
				var docDataProviders = GetDataProviders(settings.Select(x => x.DataContext));

				var attchmentEDocPairs = new Dictionary<ZString, ZGuid>();
				foreach (var setting in settings)
				{
					var eDocID = SaveDocumentToEDocs(docDataProviders, setting, messages);
					if (eDocID.IsValid)
					{
						attchmentEDocPairs.Add(setting.AttachmentType, eDocID);
					}
				}

				if (attchmentEDocPairs.Any())
				{
					try
					{
						documentFactory.Save();
					}
					catch (ZSaveException e)
					{
						attchmentEDocPairs.Clear();

						ZExceptionReporting.HandleSaveException(e);
						messages.Append(ResString.GetMultilingualString("F8F602C0-F7EE-441A-84D7-83910E27C870", "Documents have failed to be generated due to the following error : ") + e.Message);
					}

					foreach (var attchmentEDocPair in attchmentEDocPairs)
					{
						var attachment = entryHeader.EntryInstruction.Attachments.Cast<EntryInstructionAttachment>().FirstOrDefault(x => x.AttachmentType == attchmentEDocPair.Key);
						if (attachment == null)
						{
							attachment = entryHeader.EntryInstruction.Attachments.AddNew();
							attachment.AttachmentType = attchmentEDocPair.Key;
						}
						attachment.EDoc = attchmentEDocPair.Value;
					}
				}
			}

			return messages.ToStringWithNewLineBetweenAppends();
		}

		List<CNDocTemplateForAttachment> GetApplicableSettings(ZStringBuilder messages)
		{
			var declaration = entryHeader.Declaration;
			OrgHeader organization = null;
			if (declaration.IsImport)
			{
				organization = declaration.Importer;
			}
			else if (declaration.IsExport)
			{
				organization = declaration.Supplier;
			}

			var cnDocTemplateForAttachments = CNCustomsDataRegistry.Instance.CNDocTemplateForAttachment.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).Cast<CNDocTemplateForAttachment>();
			var mathchedDocTemplateForAttachments = organization == null ? cnDocTemplateForAttachments.Where(x => x.OrganizationPK.IsEmpty)
				: cnDocTemplateForAttachments.Where(x => x.OrganizationPK == organization.PK || x.OrganizationPK.IsEmpty);

			var settings = mathchedDocTemplateForAttachments.GroupBy(x => x.AttachmentType).Select(g => g.OrderByDescending(x => x.OrganizationPK).FirstOrDefault()).ToList();

			if (!settings.Any())
			{
				messages.Append(ResString.GetMultilingualString("03FA2516-8A62-4F88-80DD-97430D26E1C9", "No document templates found for generating attachments. Please go to Registry -> {path of the registry} to specify which document templates you want to used for generating attachments for the entry."));
			}

			return settings;
		}

		IBODocDataProvider[] GetDataProviders(IEnumerable<ZString> dataContexts)
		{
			var dataProviders = new List<IBODocDataProvider>();
			dataProviders.Add(BODocDataProvider.Get(entryHeader));

			foreach (var dataContext in dataContexts.Distinct())
			{
				dataProviders.AddRange(entryHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(dataContext), null));
			}

			return dataProviders.ToArray();
		}

		ZGuid SaveDocumentToEDocs(IBODocDataProvider[] docDataProviders, CNDocTemplateForAttachment setting, ZStringBuilder messages)
		{
			var docUniqueKey = ZGuid.Empty;

			var templateName = setting.DocumentTemplate;
			var isSystemDefined = templateName.EndsWith(SystemString, StringComparison.OrdinalIgnoreCase);
			var actualTemplateName = isSystemDefined ? templateName.Substring(0, templateName.Length - SystemString.Length) : templateName;
			var filename = actualTemplateName + ".pdf";

			if (!storageMain.AllEDocs.Cast<IeDoc>().Any(x => x.FileName == filename && !x.IsDeleted))
			{
				var template = GetTemplate(actualTemplateName, setting.DataContext, isSystemDefined);
				if (template == null)
				{
					messages.Append(ResString.GetMultilingualString("64464129-D444-4913-B6CC-DA2759685ABC", "Cannot find Document Template {0}, Data Context {1}.", setting.DocumentTemplate, setting.DataContext));
				}
				else
				{
					docUniqueKey = SaveReportToEDocs(filename, template, docDataProviders, setting.DocumentType, setting.DocumentDescription, messages);
				}
			}
			else
			{
				messages.Append(ResString.GetMultilingualString("EF5C153B-D081-4A91-AD60-805E99FEBF89", "Document {0} could not be generated because another document with the same name exists in eDocs. You may need to delete or rename it first.", filename));
			}

			return docUniqueKey;
		}

		ExcelTemplateReadFromStmTemplateTable GetTemplate(ZString templateName, ZString dataContext, bool isSystemDefined)
		{
			var filter = new ZQuery();
			filter.AddToFilter(StmTemplateSchema.SO_Name, templateName);
			filter.AddToFilter(StmTemplateSchema.SO_DataContext, dataContext);
			filter.AddToFilter(StmTemplateSchema.SO_IsSystemDefined, isSystemDefined);

			var template = entryHeader.Factory.LoadTop1<StmTemplate>(filter);
			return template == null ? null : new ExcelTemplateReadFromStmTemplateTable(template);
		}

		ZGuid SaveReportToEDocs(string filename, ExcelTemplateReadFromStmTemplateTable template, IBODocDataProvider[] docDataProviders, string documentType, string description, ZStringBuilder messages)
		{
			var docUniqueKey = ZGuid.Empty;

			using (var report = new Report(null, template, new DataProviderList(docDataProviders), template.TemplateName, null, DocumentDirection.ANY, false))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				var binaryData = DocumentEngine.FileFormatUtilities.DocumentConverter.ConvertFromExcel(outputStream.ToArray(), OutputFormatType.PDF, ColourDepth.BlackAndWhite);
				var doc = storageMain.AddFileOrDocument(binaryData, filename, documentType, false, description: description);
				docUniqueKey = doc.UniqueKey;

				messages.Append(ResString.GetMultilingualString("C7B0CB09-9939-49CD-87AB-F23107F3E357", "Document {0} has been generated in eDocs.", doc.FileName));
			}

			return docUniqueKey;
		}
	}
}
