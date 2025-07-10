using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class CustomsEntryHeaderDataObjectWriterForTest : CustomsEntryHeaderDataObjectWriter
	{
		public CustomsEntryHeaderDataObjectWriterForTest(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		public List<CustomsSupportingInformation> CreateCollection(CusEntryHeader entryHeader)
		{
			return CreateCollection(entryHeader, base.writeManager);
		}
	}

	public class CustomsEntryHeaderDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestSupportingDocuments()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			EU.Business.Declaration.MultiLineAddInfos.SupportingDocument aSuppDoc = invoice.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "123";

			aSuppDoc = invoice.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "456";

			aSuppDoc = declaration.SupportingDocuments.AddNew();
			aSuppDoc.CSI_Code = "789";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders[0].MergedLines[0].Header;

			var writer = new CustomsEntryHeaderDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryHeader)), new UniversalDataObjectWriterHelper(declaration.Factory, Core.Constants.CountryCodes.UnitedKingdom));
			var suppDocs = writer.CreateCollection(entryHeader);

			AssertEquals("Incorrect number of supporting docs", 3, suppDocs.Count);
		}
	}
}

