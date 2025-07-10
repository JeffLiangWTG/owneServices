using Enterprise.Client.UPE.DocumentImaging;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(DocumentImageType))]
	public class DocumentImageTypeTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Bound Properties
		public void TestUPSCodeMustBeUnique()
		{
			DocumentImageTypeCollection collection = new DocumentImageTypeCollection();
			DocumentImageType type1 = collection.AddNew();
			type1.UPSCode = "XXX";
			AssertNoErrors("Unique code should not have an error", type1.UPSCodeInfo);
			DocumentImageType type2 = collection.AddNew();
			type2.UPSCode = "XXX";
			AssertHasErrors("Duplicate code should have an error", type2.UPSCodeInfo);
		}

		public void TestDocTypeCodeMandatory()
		{
			DocumentImageType.DocTypeCode = "";
			AssertHasErrors("DocTypeCode mandatory", DocumentImageType.DocTypeCodeInfo);
		}

		public void TestDescriptionMandatory()
		{
			DocumentImageType.Description = "";
			AssertHasErrors("Description mandatory", DocumentImageType.DescriptionInfo);
		}

		#endregion
		#region Lookups
		public void TestDocTypeCode_List()
		{
			RefDocType allDocType = Factory.New<RefDocType>();
			allDocType.RT_ReferenceType = DocManagerReferenceTypes.All;
			allDocType.RT_DocType = "XXX";
			RefDocType declarationDocType = Factory.New<RefDocType>();
			declarationDocType.RT_ReferenceType = "XXX";
			declarationDocType.RT_DocType = "UPS";
			RefDocType airCargoWithoutDecDocType = Factory.New<RefDocType>();
			airCargoWithoutDecDocType.RT_ReferenceType = DocManagerReferenceTypes.AirCargo;
			airCargoWithoutDecDocType.RT_DocType = "UP1";
			RefDocType declarationWithoutAirCargoDocType = Factory.New<RefDocType>();
			declarationWithoutAirCargoDocType.RT_ReferenceType = DocManagerReferenceTypes.Declaration;
			declarationWithoutAirCargoDocType.RT_DocType = "UP2";
			Factory.Save();
			AssertEquals("Should include 'all'", true, DocumentImageType.DocTypeCode_List.ContainsCode("XXX"));
			AssertEquals("Should contain a doc type if the code exists for air cargo *and* declaration", false, DocumentImageType.DocTypeCode_List.ContainsCode("UPS"));
			AssertEquals("Should not include an air cargo  doc type if the same code doesn't exist for a dec", true, DocumentImageType.DocTypeCode_List.ContainsCode("UP1"));
			AssertEquals("Should not include a declaration doc type if the same code doesn't exist for air cargo", true, DocumentImageType.DocTypeCode_List.ContainsCode("UP2"));
		}

		#endregion
		#region Implementation
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			DocumentImageType.UPSCode = "UPS";
			DocumentImageType.DocTypeCode = "EDI";
			DocumentImageType.Description = "Image Description";
			DocumentImageType.MoveJobToClassOnImport = true;
			DocumentImageType.NotifyOnImport = true;
			return DocumentImageType;
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		DocumentImageType DocumentImageType
		{
			get
			{
				if (fDocumentImageType == null)
				{
					fDocumentImageType = new DocumentImageType();
				}

				return fDocumentImageType;
			}
		}

		DocumentImageType fDocumentImageType;
		#endregion
	}
}
