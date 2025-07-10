using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(PreviousDocument))]
	public class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
	{
		public void TestIsTemporaryStorageDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			AssertEquals("A previous document without type defined should not be considered as a temporary storage document.", false, previousDocument.IsTemporaryStorageDocument);

			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			AssertEquals("IST type of previous document should be considered as a temporary storage document.", true, previousDocument.IsTemporaryStorageDocument);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			AssertEquals("N337 type of previous document should be considered as a temporary storage document.", true, previousDocument.IsTemporaryStorageDocument);

			previousDocument.CSI_Code = "ANY";
			AssertEquals("Only IST and N337 types of previous document should be considered as temporary storage documents.", false, previousDocument.IsTemporaryStorageDocument);
		}

		public void TestIsUCC6TemporaryStorage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			AssertEquals("Declaration should be flagged as UCC6 when JE_ApplicationCode is set to DeltaIE.", true, declaration.IsUCC6);

			var previousDocument = declaration.PreviousDocuments.AddNew();
			AssertEquals("A previous document without type defined should not be considered as a temporary storage.", false, previousDocument.IsUCC6TemporaryStorage);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			AssertEquals("N337 type of previous document should be considered as a temporary storage.", true, previousDocument.IsUCC6TemporaryStorage);

			previousDocument.CSI_Code = "ANY";
			AssertEquals("Only N337 type of previous document should be considered as temporary storage.", false, previousDocument.IsUCC6TemporaryStorage);
		}

		public void TestCSI_UnitOfQuantity_readOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var previousDocument = declaration.PreviousDocuments.AddNew();

			AssertEquals("Expected CSI_UnitOfQuantity to be editable (ReadOnly = false) when CSI_Code is not set.", false, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			AssertEquals("Expected CSI_UnitOfQuantity to become read-only when CSI_Code is set to 'Temporary Storage'.", true, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);

			previousDocument.CSI_Code = "ANY";
			AssertEquals("Expected CSI_UnitOfQuantity to be editable (ReadOnly = false) when CSI_Code is set to a value other than 'Temporary Storage' (N337).", false, previousDocument.CSI_UnitOfQuantityInfo.ReadOnly);
		}

		public void TestCSI_Code_SetsUnitOfQuantityToKGM_WhenTemporaryStorage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var previousDocument = declaration.PreviousDocuments.AddNew();
			AssertEquals("CSI_UnitOfQuantity should be empty when CSI_Code is not set.", ZString.Empty, previousDocument.CSI_UnitOfQuantity);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			AssertEquals("CSI_UnitOfQuantity should be set to KGM when CSI_Code is set to Temporary Storage", "KGM", previousDocument.CSI_UnitOfQuantity);

			previousDocument.CSI_Code = "ANY";
			AssertEquals("Expected CSI_UnitOfQuantity should retain its previous value when CSI_Code is set to a value other than 'Temporary Storage' (N337).", "KGM", previousDocument.CSI_UnitOfQuantity);
		}

		public void TestReferenceNumberFieldType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();
			CombineAssertions("Default case", () =>
			{
				AssertEquals("ReferenceNumber Field should be Text by default", nameof(FieldType.Text), previousDocument.ReferenceNumberFieldType);
				AssertEquals("ReferenceNumber should not show as CodeFindBox by default", false, previousDocument.ShowCodeFindBoxForReferenceNumber);
				AssertEquals("ReferenceNumber should show as TextBox by default", true, previousDocument.ShowTextBoxForReferenceNumber);
			});

			CombineAssertions("IST previous document code case", () =>
			{
				previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.IST;
				AssertEquals("ReferenceNumber Field should be TextCodeFindBox for IST code", nameof(FieldType.TextCodeFindBox), previousDocument.ReferenceNumberFieldType);
				AssertEquals("ReferenceNumber should show as CodeFindBox for IST code", true, previousDocument.ShowCodeFindBoxForReferenceNumber);
				AssertEquals("ReferenceNumber should not show as TextBox for IST code", false, previousDocument.ShowTextBoxForReferenceNumber);
			});

			CombineAssertions("N337 previous document code case", () =>
			{
				previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
				AssertEquals("ReferenceNumber Field should be TextCodeFindBox for N337 code", nameof(FieldType.TextCodeFindBox), previousDocument.ReferenceNumberFieldType);
				AssertEquals("ReferenceNumber should show as CodeFindBox for N337 code", true, previousDocument.ShowCodeFindBoxForReferenceNumber);
				AssertEquals("ReferenceNumber should not show as TextBox for N337 code", false, previousDocument.ShowTextBoxForReferenceNumber);
			});
		}

		public void TestLookupsShouldBeFR()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			Assert(previousDocument.Lookups is PreviousDocumentLookups);
		}

		public void TestValidationType()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			AssertType<DeltaGPreviousDocumentValidation>("Validation should be defaulted to DeltaG type when the document's declaration UCC6 status can't be determined ", previousDocument.Validation);

			var declaration = Factory.New<JobDeclaration>();
			previousDocument = declaration.PreviousDocuments.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertType<DeltaGPreviousDocumentValidation>("Validation should be of type DeltaG when the document's declaration is not UCC6.", previousDocument.Validation);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIEPreviousDocumentValidation>("Validation should be of type DeltaIE when the document's declaration is UCC6.", previousDocument.Validation);

			previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertType<DeltaGPreviousDocumentValidation>("Validation should be of type DeltaG when the document's declaration is not UCC6.", previousDocument.Validation);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIEPreviousDocumentValidation>("Validation should be of type DeltaIE when the document's declaration is UCC6.", previousDocument.Validation);

			previousDocument = declaration.Invoices.AddNew().InvoiceLines.AddNew().PreviousDocuments.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertType<DeltaGPreviousDocumentValidation>("Validation should be of type DeltaG when the document's declaration is not UCC6.", previousDocument.Validation);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIEPreviousDocumentValidation>("Validation should be of type DeltaIE when the document's declaration is UCC6.", previousDocument.Validation);
		}

		protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.PreviousDocuments.AddNew();
			var invoice = declaration.Invoices.AddNew();
			yield return invoice.PreviousDocuments.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.PreviousDocuments.AddNew();
			var product = factory.New<EU.Business.MasterFiles.OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;
			yield return (PreviousDocument)pivot.PreviousDocuments.AddNew();
		}
	}
}
