using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class CustomsEntryLineDataObjectWriterForTest : CustomsEntryLineDataObjectWriter
	{
		public CustomsEntryLineDataObjectWriterForTest(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		public List<CustomsSupportingInformation> CreateCollection(CusEntryLine entryLine)
		{
			return CreateCollection(entryLine, base.writeManager);
		}
	}

	public class CustomsEntryLineDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestSupportingDocuments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);

				var gbGroup = helper.CreateNewOrGetExistingDataGrouping("GB");

				var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				var attributeNameValuePairs = new Dictionary<string, string[]>();

				attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
					new string[] { importCodeType, exportCodeType }, "380", "Test 380", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
					new string[] { importCodeType, exportCodeType }, "456", "Test 456", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				attributeNameValuePairs.Clear();
				attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
					new string[] { importCodeType, exportCodeType }, "789", "Test 789", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

				Factory.Save();

				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = "CHF";
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

				EU.Business.Declaration.MultiLineAddInfos.SupportingDocument suppDoc = invoice.SupportingDocuments.AddNew();
				suppDoc.CSI_Code = "789";

				suppDoc = invoiceLine.SupportingDocuments.AddNew();
				suppDoc.CSI_Code = "380";

				suppDoc = invoiceLine.SupportingDocuments.AddNew();
				suppDoc.CSI_Code = "456";

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

				var writer = new CustomsEntryLineDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryHeader)), new UniversalDataObjectWriterHelper(declaration.Factory, Core.Constants.CountryCodes.UnitedKingdom));
				var suppDocs = writer.CreateCollection(entryLine);

				AssertEquals("Incorrect number of supporting docs", 2, suppDocs.Count);
			}
		}
	}
}
