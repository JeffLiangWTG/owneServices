using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocRefDocType))]
	public class DocRefDocTypeTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocRefDocType.New(RefDocType, Factory),
			};
		}

		public void TestDocType()
		{
			ZString docType = new ZString("123");

			RefDocType.RT_DocType = docType;
			AssertEquals("Wrapped Value", docType, DocRefDocType.DocType);
		}

		public void TestDescription()
		{
			ZString description = new ZString("Test Description");

			RefDocType.RT_Desc = description;
			AssertEquals("Wrapped Value", description, DocRefDocType.Description);
		}

		public void TestReferenceType()
		{
			ZString refType = new ZString("123");

			RefDocType.RT_ReferenceType = refType;
			AssertEquals("Wrapped Value", refType, DocRefDocType.ReferenceType);
		}

		public void TestIsActive()
		{
			ZBool isActive = ZBool.True;

			RefDocType.RT_IsActive = isActive;
			AssertEquals("Wrapped Value", isActive, DocRefDocType.IsActive);
		}

		public void TestIsPublished()
		{
			ZBool isPublished = ZBool.True;

			RefDocType.RT_IsPublished = isPublished;
			AssertEquals("Wrapped Value", isPublished, DocRefDocType.IsPublished);
		}

		public void TestIsPublishUpdatable()
		{
			ZBool isPubUpdatable = ZBool.True;

			RefDocType.RT_IsPublishUpdatable = isPubUpdatable;
			AssertEquals("Wrapped Value", isPubUpdatable, DocRefDocType.IsPublishUpdatable);
		}

		public void TestIsSystem()
		{
			ZBool isSystem = ZBool.True;

			RefDocType.RT_IsSystem = isSystem;
			AssertEquals("Wrapped Value", isSystem, DocRefDocType.IsSystem);
		}

		public void TestToString()
		{
			ZString description = new ZString("Test Description");

			RefDocType.RT_Desc = description;
			AssertEquals("Wrapped Value", description, DocRefDocType.ToString());
		}

		public void TestBarcodeTextWithData()
		{
			ZString docType = new ZString("123");
			RefDocType.RT_DocType = docType;

			TextBarcode testBarcode = new TextBarcode("^DOC=123|");
			AssertEquals("Barcode Text", testBarcode.TextAs128sFontString, DocRefDocType.BarcodeText);
		}

		public void TestBarcodeLabel()
		{
			ZString docType = new ZString("123");
			RefDocType.RT_DocType = docType;
			AssertEquals("Barcode Label", "^DOC=123|", DocRefDocType.BarcodeLabel);
		}

		#region Implementation

		protected override void SetUp()
		{
			RefDocType = Factory.New<RefDocType>();
			DocRefDocType = DocRefDocType.New(RefDocType, Factory);
			AssertNotNull("PreCondition: Valid DocRefDocType", DocRefDocType);

			base.SetUp();
		}

		RefDocType RefDocType;
		DocRefDocType DocRefDocType;

		#endregion
	}
}
