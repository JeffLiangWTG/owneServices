using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class DataItemIDProviderTest : TestCaseWithFactory
	{
		public void TestGetItemID()
		{
			var exportHeader = new ExportHeaderDataProviderForTest();
			AssertEquals("A706", DataItemIDProvider.GetItemID(typeof(ExportHeaderDataProviderForTest), nameof(exportHeader.InvoiceAmount)));
			AssertEquals("A107", DataItemIDProvider.GetItemID(typeof(ExportHeaderDataProviderForTest), nameof(exportHeader.PaymentMethod)));
			var importHeader = new ImportHeaderDataProviderForTest();
			AssertEquals("A807", DataItemIDProvider.GetItemID(typeof(ImportHeaderDataProviderForTest), nameof(importHeader.InvoiceAmount)));
			AssertEquals("A808", DataItemIDProvider.GetItemID(typeof(ImportHeaderDataProviderForTest), nameof(importHeader.PaymentMethod)));
			var derivedHeader = new ExportDerivedHeaderDataProviderForTest();
			AssertEquals("A706", DataItemIDProvider.GetItemID(typeof(ExportDerivedHeaderDataProviderForTest), nameof(derivedHeader.InvoiceAmount)));
			AssertEquals("A107", DataItemIDProvider.GetItemID(typeof(ExportDerivedHeaderDataProviderForTest), nameof(derivedHeader.PaymentMethod)));
			AssertEquals("A505", DataItemIDProvider.GetItemID(typeof(ExportDerivedHeaderDataProviderForTest), nameof(derivedHeader.Description)));
		}

		public void TestGetCustomsFeeID()
		{
			var importHeader = new ImportHeaderDataProviderForTest();
			AssertEquals(EntryTaxTypeList.Codes.CUD, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A815));
			AssertEquals(EntryTaxTypeList.Codes.IND, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A816));
			AssertEquals(EntryTaxTypeList.Codes.ENV, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A818));
			AssertEquals(EntryTaxTypeList.Codes.ACT, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A817));
			AssertEquals(EntryTaxTypeList.Codes._5AB, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A820));
			AssertEquals(EntryTaxTypeList.Codes.CAP, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A821));
			AssertEquals(EntryTaxTypeList.Codes.VAT, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A819));
			AssertEquals(EntryTaxTypeList.Codes._5AC, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A822));
			AssertEquals(EntryTaxTypeList.Codes._5AY, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A823));
			AssertEquals(ZString.Empty, DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), ImportAmendmentDataItemIDList.Codes.A502));
		}
	}

	class ExportHeaderDataProviderForTest : IDeclarationHeaderForTest
	{
		[DataItemID("A706")]
		public ZDecimal InvoiceAmount { get; set; }

		[DataItemID("A107")]
		public ZString PaymentMethod { get; set; }
	}

	class ImportHeaderDataProviderForTest : IDeclarationHeaderForTest
	{
		[DataItemID("A807")]
		public ZDecimal InvoiceAmount { get; set; }

		[DataItemID("A808")]
		public ZString PaymentMethod { get; set; }
	}

	interface IDeclarationHeaderForTest
	{
		ZDecimal InvoiceAmount { get; }
		ZString PaymentMethod { get; }
	}

	class ExportDerivedHeaderDataProviderForTest : ExportHeaderDataProviderForTest, IDeclarationDerivedHeaderForTest
	{
		[DataItemID("A505")]
		public ZString Description { get; set; }
	}

	interface IDeclarationDerivedHeaderForTest
	{
		ZString Description { get; set; }
	}
}
