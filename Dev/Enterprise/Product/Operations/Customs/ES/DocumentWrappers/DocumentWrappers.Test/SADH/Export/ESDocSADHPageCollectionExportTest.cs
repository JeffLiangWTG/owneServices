using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using ESCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	[TestedType(typeof(ESDocSADHPageCollectionExport))]
	sealed class ESDocSADHPageCollectionExportTest : DocSADHPageCollectionTest<ESDocSADHPageCollectionExport>
	{
		public void TestPagesGenerated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList.Codes.A;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Description = "Desc";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Description = "Desc";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = "CLP";
			var entryLine1 = entryHeader.AllEntryLines[0];
			var entryLine2 = entryHeader.AllEntryLines[1];

			AddDocumentsToEntryLine(entryLine1, 99);
			AddDocumentsToEntryLine(entryLine2, 50);

			var pagesCollection = new ESDocSADHPageCollectionExport(entryHeader.MergedLines, Factory);

			AssertEquals("Should generate three pages. First, second with 2 extra lines from first entry and second entryline and third page with extra line from second entryline", 3, pagesCollection.Count);
		}

		protected override ESDocSADHPageCollectionExport GetNewDocumentWrapperCollection()
		{
			return new ESDocSADHPageCollectionExport(EntryLineCollection, Factory);
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
		{
			var result = ESDocSADHPageExport.New(Factory, EntryLineCollection.AddNew());
			collection.Add(result);
			return result;
		}

		new ESCusEntryLineCollection EntryLineCollection
		{
			get
			{
				if (entryHeaderCollection == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
					invoiceHeader.JobComInvoiceLines.AddNew();
					var entryHeader = declaration.CustomsEntryHeaders.AddNew();
					entryHeader.MergedLines.AddNew();

					entryHeaderCollection = entryHeader.MergedLines;
				}
				return entryHeaderCollection;
			}
		}
		ESCusEntryLineCollection entryHeaderCollection;

		protected override string CountryToUseForTesting => Core.Constants.CountryCodes.Spain;

		void AddDocumentsToEntryLine(ESCusEntryLine entryLine, int documentsToCreate)
		{
			for (var i = 0; i < documentsToCreate; i++)
			{
				AddSupDoc(entryLine.PK, entryLine.TablePrefix, "C" + entryLine.SupportingDocuments.Count() + i, "ABCDEFGHIJKL");
			}
		}

		SupportingDocument AddSupDoc(ZGuid parentPK, ZString parentTableCode, ZString code, ZString refNum)
		{
			var suppDoc = Factory.New<SupportingDocument>();
			suppDoc.CSI_ParentID = parentPK;
			suppDoc.CSI_ParentTableCode = parentTableCode;
			suppDoc.CSI_Code = code;
			suppDoc.CSI_ReferenceNumber = refNum;
			suppDoc.CSI_DateOfExpiry = new ZDateTime(2020, 03, 12);
			suppDoc.CSI_Status = DocumentStatus.Accepted;
			suppDoc.CSI_SubType = "";
			return suppDoc;
		}
	}
}
