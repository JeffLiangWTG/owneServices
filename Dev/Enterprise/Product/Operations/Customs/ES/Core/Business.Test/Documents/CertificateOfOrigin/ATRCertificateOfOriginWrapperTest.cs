using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Documents.DocDataObjects;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin.Testing
{
	sealed class ATRCertificateOfOriginWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when Entry Header has no declaration", () => new ATRCertificateOfOriginWrapper(Factory.New<CusEntryHeader>()));

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertNoExceptionThrown("No exception expected", () => new ATRCertificateOfOriginWrapper(entryHeader));
		}

		public void TestEntryHeader()
		{
			var wrapper = new ATRCertificateOfOriginWrapper_ForTest(entryHeader);
			AssertType<CusEntryHeader>(wrapper.EntryHeader_Exposed);
		}

		public void TestTotalATRCertificateItem()
		{
			AssertType<TotalATRCertificateItem>(wrapper.TotalATRCertificateItem);
		}

		public void TestCustomsEndorsement()
		{
			AssertType<CustomsEndorsementWrapper>(wrapper.CustomsEndorsement);
		}

		public void TestATRBoxItemBuilder()
		{
			AssertType<ATRBoxItemsWrapper>(wrapper.ATRBoxItemBuilder);
		}

		public void TestExporterDeclaration()
		{
			AssertType<ExporterDeclarationWrapper>(wrapper.ExporterDeclaration);
		}

		public void TestReferenceDateFormat()
		{
			AssertEquals("ReferenceDateFormat expected dd-MM-yyyy for ES", "dd-MM-yyyy", wrapper.ReferenceDateFormat);
		}

		public void TestShouldAddTotalCertificateItem()
		{
			AssertEquals("ShouldAddTotalCertificateItem  expected true for ES", true, wrapper.ShouldAddTotalCertificateItem);
		}

		public void TestARTNumberCaption()
		{
			AssertEquals("ART Number Caption expected A.TR.1 Nº", "A.TR.1 Nº", wrapper.ARTNumberCaption);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			wrapper = new ATRCertificateOfOriginWrapper(entryHeader);
		}
		IATRCertificateOfOrigin wrapper;
		CusEntryHeader entryHeader;
		JobDeclaration declaration;
	}

	class ATRCertificateOfOriginWrapper_ForTest : ATRCertificateOfOriginWrapper
	{
		public ATRCertificateOfOriginWrapper_ForTest(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public CusEntryHeader EntryHeader_Exposed => base.EntryHeader;
	}
}
