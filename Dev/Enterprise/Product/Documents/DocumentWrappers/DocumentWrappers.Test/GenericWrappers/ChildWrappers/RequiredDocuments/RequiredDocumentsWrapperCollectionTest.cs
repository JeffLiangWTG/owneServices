using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RequiredDocumentsWrapperCollection))]
	sealed class RequiredDocumentsWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<RequiredDocumentsWrapperCollection>
	{
		public void TestAdditionalJobsForRequiredDocuments()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.RequiredDocuments.AddIfNotExists(Core.Constants.RefDocTypes.CommercialInvoice, JobRequiredDocument.DocUsage.Both);

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT123";
			shipment.DocsAndCartage.RequiredDocuments.AddIfNotExists(Core.Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both);

			FreightWrapper[] additionalJobs = FreightWrapper.New(shipment, Factory);

			RequiredDocumentsWrapperCollection wrapperForDocs = new RequiredDocumentsWrapperCollection(consol.RequiredDocuments, additionalJobs, Factory);
			SetDocumentDirection(wrapperForDocs, DocumentDirection.ANY);
			AssertEquals("Two unreceived documents should be displayed", @"- Commercial Invoice
Shipment: SHIPMENT123
- Airway Bill/Ocean Bill of Lading", wrapperForDocs.MissingRequiredDocuments);
		}

		public void TestMissingRequiredDocuments()
		{
			AssertEquals("Missing docs field should be blank", ZString.Empty, WrapperForDocs.MissingRequiredDocuments);

			Docs.AddIfNotExists(Core.Constants.RefDocTypes.CommercialInvoice, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals("Unreceived system defined doc should be displayed",
				"- Commercial Invoice", WrapperForDocs.MissingRequiredDocuments);

			Docs.AddIfNotExists("COR", JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals("Unreceived system defined doc should be displayed",
				true, WrapperForDocs.MissingRequiredDocuments.Contains("\n- Certificate of Receipt"));

			Docs.AddIfNotExists(Core.Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals("Unreceived system defined docs should be displayed",
				true, WrapperForDocs.MissingRequiredDocuments.Contains("\n- Airway Bill/Ocean Bill of Lading"));

			RefDocType refDocType = Factory.New<RefDocType>();
			refDocType.RT_DocType = "ZDR";
			refDocType.RT_Desc = "Test doc description";
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			Factory.Save();

			Docs.AddIfNotExists("ZDR", JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals("Unreceived registry docs should be displayed after system defined docs",
				true, WrapperForDocs.MissingRequiredDocuments.Contains("\n- Test doc description"));

			JobRequiredDocument miscDoc = Docs.AddNew();
			miscDoc.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			miscDoc.EQ_DocDescription = "Miscellaneous doc description";
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals("Miscellaneous documents should be displayed after registry documents",
				true, WrapperForDocs.MissingRequiredDocuments.Contains("\n- Miscellaneous doc description"));

			Docs.ToggleDocReceived(Core.Constants.RefDocTypes.MasterBill, true);
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals("Received document should no longer be displayed",
				false, WrapperForDocs.MissingRequiredDocuments.Contains("- Ocean Bill Of Lading"));

			Docs.ToggleDocReceived("ZDR", true);
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals("Received document should no longer be displayed",
				false, WrapperForDocs.MissingRequiredDocuments.Contains("- Test doc description"));
		}

		public void TestMissingRequiredDocumentsNoName()
		{
			AssertEquals("", WrapperForDocs.MissingRequiredDocuments);

			Docs.AddIfNotExists(Core.Constants.RefDocTypes.MasterBill, JobRequiredDocument.DocUsage.Both);
			WrapperForDocs = CreateWrapperCollection(Docs);

			AssertEquals("- Airway Bill/Ocean Bill of Lading", WrapperForDocs.MissingRequiredDocuments);

			Docs.AddIfNotExists(Core.Constants.RefDocTypes.PackingList, JobRequiredDocument.DocUsage.Both);
			Docs.GetDocByType(Core.Constants.RefDocTypes.PackingList).EQ_OriginalDocRequired = true;
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals(@"- Airway Bill/Ocean Bill of Lading
- Packing List - Original Required", WrapperForDocs.MissingRequiredDocuments);

			JobRequiredDocument miscDoc1 = Docs.AddNew();
			miscDoc1.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Docs.GetDocByType(Core.Constants.RefDocTypes.PackingList).EQ_OriginalDocRequired = false;
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals(@"- Airway Bill/Ocean Bill of Lading
- Packing List", WrapperForDocs.MissingRequiredDocuments);

			miscDoc1.EQ_DocDescription = "User Doc1 Name";
			WrapperForDocs = CreateWrapperCollection(Docs);
			AssertEquals(@"- Airway Bill/Ocean Bill of Lading
- User Doc1 Name
- Packing List", WrapperForDocs.MissingRequiredDocuments);
		}

		#region Implementation

		RequiredDocumentsWrapperCollection CreateWrapperCollection(JobRequiredDocumentDependentCollection docs)
		{
			var result = new RequiredDocumentsWrapperCollection(docs, Factory);
			SetDocumentDirection(result, DocumentDirection.ANY);
			return result;
		}

		void SetDocumentDirection(RequiredDocumentsWrapperCollection collection, DocumentDirection direction)
		{
			foreach (RequiredDocumentsWrapper wrapper in collection)
			{
				wrapper.SetDocumentDirectionForTesting(direction.ToString());
			}
		}

		protected override RequiredDocumentsWrapperCollection GetNewDocumentWrapperCollection()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobRequiredDocumentDependentCollection docs = new JobRequiredDocumentDependentCollection(shipment.DocsAndCartage, Factory);

			return new RequiredDocumentsWrapperCollection(docs, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobRequiredDocument doc = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			return new RequiredDocumentsWrapper(doc, Factory);
		}

		CommonShipment Shipment;
		JobRequiredDocumentDependentCollection Docs;
		RequiredDocumentsWrapperCollection WrapperForDocs;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment = CommonShipment.New(Factory);
			Docs = new JobRequiredDocumentDependentCollection(Shipment.DocsAndCartage, Factory);
			WrapperForDocs = new RequiredDocumentsWrapperCollection(Docs, Factory);
		}

		#endregion
	}
}
