using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AnnexDocCommonWrapperTest : Customs.Business.Testing.DataProviderTestCase<AnnexDocCommonWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null document", () => new AnnexDocCommonWrapper(null, "A"));
				document.SC_DataType = "XXX";
				AssertExceptionThrown<NotSupportedException>("Document type nos supported", () => new AnnexDocCommonWrapper(document, "A"));
				document.SC_DataType = "PDF";
				AssertNoExceptionThrown("Correct document", () => new AnnexDocCommonWrapper(document, "A"));
			});
		}

		public void TestDescription()
		{
			CombineAssertions(() =>
			{
				document.SC_DocType_List.AddPair(DocType, DocDescription);
				document.SC_DocType = DocType;
				AssertEquals("Expected filled Description with the added one", DocDescriptionAdded, wrapper.Description);

				wrapper = new AnnexDocCommonWrapper(document, ZString.Empty);
				AssertEquals("Expected empty Description", ZString.Empty, wrapper.Description);
			});
		}

		public void TestReferenceNumber()
		{
			document.SC_FileName = DocName;
			document.SC_DataType = DocExtension;
			AssertEquals("Expected filled Reference", DocNameWithExtension, wrapper.ReferenceNumber);
		}

		public void TestImage()
		{
			document.SC_ImageData = DocImage;
			AssertEquals("Expected filled Image", DocImage, wrapper.Image);
		}

		public void TestExtension()
		{
			CombineAssertions(() =>
			{
				document.SC_DataType = ZString.Empty;
				AssertEquals("Expected empty Extension", ZString.Empty, wrapper.Extension);

				document.SC_DataType = DocExtension;
				AssertEquals("Expected filled Extension", DocExtension, wrapper.Extension);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			document = docFactory.New<StorageDocs>();
			wrapper = new AnnexDocCommonWrapper(document, DocDescriptionAdded);
		}
		StorageDocs document;
		AnnexDocCommonWrapper wrapper;

		protected override AnnexDocCommonWrapper GetProvider() => wrapper;

		public readonly ZString DocType = "TST";
		public readonly ZString DocDescription = "Test Description";
		public readonly ZString DocDescriptionAdded = "Added Description";
		public readonly ZString DocName = "FileName";
		public readonly ZString DocNameWithExtension = "FileName.pdf";
		public readonly ZBlob DocImage = ZBlob.FromAscii("Test image data");
		public readonly ZString DocExtension = "PDF";
	}
}
