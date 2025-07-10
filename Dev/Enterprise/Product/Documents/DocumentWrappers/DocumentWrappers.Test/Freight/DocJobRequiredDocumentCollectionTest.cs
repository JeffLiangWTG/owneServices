using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocJobRequiredDocumentCollection))]
	sealed class DocJobRequiredDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocJobRequiredDocumentCollection>
	{
		public void TestGetDocByType()
		{
			DocJobRequiredDocument matchingWrapper = WrapperForDocs.GetDocByType(Core.Constants.RefDocTypes.MasterBill);
			AssertNull("GetDocByType should return null if there's no matching doc in the collection", matchingWrapper);

			JobRequiredDocument originalBill = Docs.AddNew();
			originalBill.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			originalBill.EQ_DateReceived = new ZDateTimeOffset(2005, 5, 27);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);

			matchingWrapper = WrapperForDocs.GetDocByType(Core.Constants.RefDocTypes.MasterBill);
			AssertNotNull("GetDocByType shouldn't return null if there's a matching document in the collection", matchingWrapper);
			AssertEquals("GetDocByType should return correct document", Core.Constants.RefDocTypeDescriptions.MasterBill, matchingWrapper.Description);
			AssertEquals("GetDocByType should return correct document", new ZDateTimeOffset(2005, 5, 27), matchingWrapper.DateReceived);
		}

		public void TestMissingRequiredDocuments()
		{
			AssertEquals("Missing docs field should be blank", ZString.Empty, WrapperForDocs.MissingRequiredDocuments);
			//			SortInfo sortInfo = new SortInfo(JobRequiredDocumentSchema.Constants.EQ_DocType, ListSortDirection.Ascending);

			Docs.AddIfNotExists(Core.Constants.RefDocTypes.CommercialInvoice, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("Unreceived system defined doc should be displayed",
				"- Commercial Invoice", WrapperForDocs.MissingRequiredDocuments);

			Docs.AddIfNotExists("COR", JobRequiredDocument.DocUsage.Both);
			//			Docs.Sort(sortInfo);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("Unreceived system defined doc should be displayed",
				true, WrapperForDocs.MissingRequiredDocuments.Contains("\n- Certificate of Receipt"));

			Docs.AddIfNotExists(Core.Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both);
			//			Docs.Sort(sortInfo);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("Unreceived system defined docs should be displayed",
				true, WrapperForDocs.MissingRequiredDocuments.Contains("\n- Airway Bill/Ocean Bill of Lading"));

			RefDocType refDocType = Factory.New<RefDocType>();
			refDocType.RT_DocType = "ZDR";
			refDocType.RT_Desc = "Test doc description";
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			Factory.Save();

			Docs.AddIfNotExists("ZDR", JobRequiredDocument.DocUsage.Both);
			//			Docs.Sort(sortInfo);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("Unreceived registry docs should be displayed after system defined docs",
				true, WrapperForDocs.MissingRequiredDocuments.Contains("\n- Test doc description"));

			JobRequiredDocument miscDoc = Docs.AddNew();
			miscDoc.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			miscDoc.EQ_DocDescription = "Miscellaneous doc description";
			//			Docs.Sort(sortInfo);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("Miscellaneous documents should be displayed after registry documents",
				true, WrapperForDocs.MissingRequiredDocuments.Contains("\n- Miscellaneous doc description"));

			Docs.ToggleDocReceived(Core.Constants.RefDocTypes.MasterBill, true);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("Received document should no longer be displayed",
				false, WrapperForDocs.MissingRequiredDocuments.Contains("- Ocean Bill Of Lading"));

			Docs.ToggleDocReceived("ZDR", true);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("Received document should no longer be displayed",
				false, WrapperForDocs.MissingRequiredDocuments.Contains("- Test doc description"));
		}

		public void TestMissingRequiredDocumentsNoName()
		{
			AssertEquals("", WrapperForDocs.MissingRequiredDocuments);

			Docs.AddIfNotExists(Core.Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);

			AssertEquals("- Airway Bill/Ocean Bill of Lading", WrapperForDocs.MissingRequiredDocuments);

			Docs.AddIfNotExists(Core.Constants.RefDocTypes.PackingList, JobRequiredDocument.DocUsage.Both);
			Docs.GetDocByType(Core.Constants.RefDocTypes.PackingList).EQ_OriginalDocRequired = true;
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("- Airway Bill/Ocean Bill of Lading\n- Packing List - Original Required", WrapperForDocs.MissingRequiredDocuments);

			JobRequiredDocument miscDoc1 = Docs.AddNew();
			miscDoc1.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Docs.GetDocByType(Core.Constants.RefDocTypes.PackingList).EQ_OriginalDocRequired = false;
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("- Airway Bill/Ocean Bill of Lading\n- Packing List", WrapperForDocs.MissingRequiredDocuments);

			miscDoc1.EQ_DocDescription = "User Doc1 Name";
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertEquals("- Airway Bill/Ocean Bill of Lading\n- User Doc1 Name\n- Packing List", WrapperForDocs.MissingRequiredDocuments);
		}

		public void TestOriginalBill()
		{
			Docs.AddIfNotExists(Core.Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertNotNull("OriginalBill", WrapperForDocs.OriginalBill);
			AssertEquals("OriginalBill is of type of DocJobRequiredDocument", typeof(DocJobRequiredDocument), WrapperForDocs.OriginalBill.GetType());
		}

		public void TestOriginalOceanBill()
		{
			Docs.AddIfNotExists(Core.Constants.RefDocTypes.OceanMasterBill, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertNotNull("OriginalOceanBill", WrapperForDocs.OriginalOceanBill);
			AssertEquals("OriginalOceanBill is of type of DocJobRequiredDocument", typeof(DocJobRequiredDocument), WrapperForDocs.OriginalOceanBill.GetType());
		}

		public void TestPackingDeclaration()
		{
			Docs.AddIfNotExists(Core.Constants.RefDocTypes.QuarantinePackingDeclaration, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertNotNull("PackingDeclaration", WrapperForDocs.PackingDeclaration);
			AssertEquals("PackingDeclaration is of type of DocJobRequiredDocument", typeof(DocJobRequiredDocument), WrapperForDocs.PackingDeclaration.GetType());
		}

		public void TestCertificateOfOrigin()
		{
			Docs.AddIfNotExists(Core.Constants.RefDocTypes.CertificateOfOrigin, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertNotNull("CertificateOfOrigin", WrapperForDocs.CertificateOfOrigin);
			AssertEquals("CertificateOfOrigin is of type of DocJobRequiredDocument", typeof(DocJobRequiredDocument), WrapperForDocs.CertificateOfOrigin.GetType());
		}

		public void TestCommercialInvoice()
		{
			Docs.AddIfNotExists(Core.Constants.RefDocTypes.CommercialInvoice, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertNotNull("CommercialInvoice", WrapperForDocs.CommercialInvoice);
			AssertEquals("CommercialInvoice is of type of DocJobRequiredDocument", typeof(DocJobRequiredDocument), WrapperForDocs.CommercialInvoice.GetType());
		}

		public void TestFumigationCertificate()
		{
			Docs.AddIfNotExists(Core.Constants.RefDocTypes.FumigationCertificate, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertNotNull("FumigationCertificate", WrapperForDocs.FumigationCertificate);
			AssertEquals("FumigationCertificate is of type of DocJobRequiredDocument", typeof(DocJobRequiredDocument), WrapperForDocs.FumigationCertificate.GetType());
		}

		public void TestPackingList()
		{
			Docs.AddIfNotExists(Core.Constants.RefDocTypes.PackingList, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
			AssertNotNull("PackingList", WrapperForDocs.PackingList);
			AssertEquals("PackingList is of type of DocJobRequiredDocument", typeof(DocJobRequiredDocument), WrapperForDocs.PackingList.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var doc = Factory.New<JobRequiredDocument>();
			return DocJobRequiredDocument.New(doc, Factory);
		}

		protected override DocJobRequiredDocumentCollection GetCollectionToTest()
		{
			return new DocJobRequiredDocumentCollection(Factory);
		}

		CommonShipment Shipment;
		JobRequiredDocumentDependentCollection Docs;
		DocJobRequiredDocumentCollection WrapperForDocs;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment = CommonShipment.New(Factory);
			Docs = new JobRequiredDocumentDependentCollection(Shipment.DocsAndCartage, Factory);
			WrapperForDocs = new DocJobRequiredDocumentCollection(Docs, Factory);
		}

		#endregion
	}
}
