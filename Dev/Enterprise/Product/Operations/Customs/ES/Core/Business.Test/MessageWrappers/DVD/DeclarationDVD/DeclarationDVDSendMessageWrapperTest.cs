using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DeclarationDVDSendMessageWrapper))]
	public class DeclarationDVDSendMessageWrapperTest : DVDCommonSendMessageWrapperAbstractTest<DeclarationDVDSendMessageWrapper>
	{
		public void TestHeader()
		{
			var header = wrapper.Header;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Header", header);
				AssertSame("Cached Header", wrapper.Header, header);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		public void TestUCRInHeaderOrLines()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_CommercialReference = "reference";
				AssertEquals("Expected filled UCRReferenceNumber in Header when there is only one line", "reference", wrapper.Header.UCRReferenceNumber);
				AssertEquals("Expected empty UCRReferenceNumber in Lines when there is only one line", ZString.Empty, wrapper.Lines.ToList()[0].UCRReferenceNumber);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				invoiceLine2.ZG_CommercialReference = "reference2";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty UCRReferenceNumber in Header when there are multiple lines with different codes", ZString.Empty, wrapper.Header.UCRReferenceNumber);
				AssertContainsExactElementsInAnyOrder("Expected filled UCRReferenceNumber in Lines when there are multiple lines with different codes", new ZString[] { "reference", "reference2" }, wrapper.Lines.Select(x => x.UCRReferenceNumber).ToArray());

				invoiceLine2.ZG_CommercialReference = "reference";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled UCRReferenceNumber in Header when there are multiple lines with same codes", "reference", wrapper.Header.UCRReferenceNumber);
				AssertContainsExactElementsInAnyOrder("Expected empty UCRReferenceNumber in Lines when there are multiple lines with same codes", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.Lines.Select(x => x.UCRReferenceNumber).ToArray());
			});
		}

		public void TestAddSupplyActorsInHeaderOrLines()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CusSupplyChainActorReferences.AddNew();
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled AdditionalSupplyActors in Header when there are no supplyChainActors declared int the lines", 1, wrapper.Header.AdditionalSupplyActors.Count);
				AssertEquals("Expected empty AdditionalSupplyActors in Lines when there are no supplyChainActors declared int the lines", 0, wrapper.Lines.ToList()[0].AdditionalSupplyActors.Count);

				invoiceLine.CusSupplyChainActorReferences.AddNew();
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty AdditionalSupplyActors in Header when there are supplyChainActors declared int the lines", 0, wrapper.Header.AdditionalSupplyActors.Count);
				AssertEquals("Expected filled AdditionalSupplyActors in Lines when there are supplyChainActors declared int the lines", 2, wrapper.Lines.ToList()[0].AdditionalSupplyActors.Count);
			});
		}

		public void TestAddCountryOfDestinationInHeaderOrLines()
		{
			CombineAssertions(() =>
			{
				declaration.JE_GoodsDestination = "FR";
				invoiceLine.ZG_CountryOfDestination = ZString.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CountryOfDestination in Header when there is only one line and the field is empty", "FR", wrapper.Header.CountryOfDestination);
				AssertEquals("Expected empty CountryOfDestination in Lines when there is only one line and the field is empty", ZString.Empty, wrapper.Lines.ToList()[0].CountryOfDestination);

				invoiceLine.ZG_CountryOfDestination = "ES";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CountryOfDestination in Header when there is only one line and the field is different than in declaration", ZString.Empty, wrapper.Header.CountryOfDestination);
				AssertEquals("Expected filled CountryOfDestination in Lines when there is only one line and the field is different than in declaration", "ES", wrapper.Lines.ToList()[0].CountryOfDestination);

				invoiceLine.ZG_CountryOfDestination = "FR";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CountryOfDestination in Header when there is only one line and the field is the same as in declaration", "FR", wrapper.Header.CountryOfDestination);
				AssertEquals("Expected empty CountryOfDestination in Lines when there is only one line and the field is the same as in declaration", ZString.Empty, wrapper.Lines.ToList()[0].CountryOfDestination);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				invoiceLine2.ZG_CountryOfDestination = "ES";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CountryOfDestination in Header when there are multiple lines with different codes", ZString.Empty, wrapper.Header.CountryOfDestination);
				AssertContainsExactElementsInAnyOrder("Expected filled CountryOfDestination in Lines when there are multiple lines with different codes", new ZString[] { "FR", "ES" }, wrapper.Lines.Select(x => x.CountryOfDestination).ToArray());

				invoiceLine2.ZG_CountryOfDestination = ZString.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CountryOfDestination in Header when there are multiple lines with same codes as in declaration or empty", "FR", wrapper.Header.CountryOfDestination);
				AssertContainsExactElementsInAnyOrder("Expected empty CountryOfDestination in Lines when there are multiple lines with same codes as in declaration or empty", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.Lines.Select(x => x.CountryOfDestination).ToArray());
			});
		}

		public void TestAddCountryOfExportInHeaderOrLines()
		{
			CombineAssertions(() =>
			{
				declaration.JE_GoodsOrigin = "FR";
				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CountryOfExport in Header when there is only one line and the field is empty", "FR", wrapper.Header.CountryOfExport);
				AssertEquals("Expected empty CountryOfExport in Lines when there is only one line and the field is empty", ZString.Empty, wrapper.Lines.ToList()[0].CountryOfExport);

				invoiceLine.ZG_CountryOfSupply = "ES";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CountryOfExport in Header when there is only one line and the field is different than in declaration", ZString.Empty, wrapper.Header.CountryOfExport);
				AssertEquals("Expected filled CountryOfExport in Lines when there is only one line and the field is different than in declaration", "ES", wrapper.Lines.ToList()[0].CountryOfExport);

				invoiceLine.ZG_CountryOfSupply = "FR";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CountryOfExport in Header when there is only one line and the field is the same as in declaration", "FR", wrapper.Header.CountryOfExport);
				AssertEquals("Expected empty CountryOfExport in Lines when there is only one line and the field is the same as in declaration", ZString.Empty, wrapper.Lines.ToList()[0].CountryOfExport);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				invoiceLine2.ZG_CountryOfSupply = "ES";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CountryOfExport in Header when there are multiple lines with different codes", ZString.Empty, wrapper.Header.CountryOfExport);
				AssertContainsExactElementsInAnyOrder("Expected filled CountryOfExport in Lines when there are multiple lines with different codes", new ZString[] { "FR", "ES" }, wrapper.Lines.Select(x => x.CountryOfExport).ToArray());

				invoiceLine2.ZG_CountryOfSupply = ZString.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CountryOfExport in Header when there are multiple lines with same codes as in declaration or empty", "FR", wrapper.Header.CountryOfExport);
				AssertContainsExactElementsInAnyOrder("Expected empty CountryOfExport in Lines when there are multiple lines with same codes as in declaration or empty", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.Lines.Select(x => x.CountryOfExport).ToArray());
			});
		}

		public void TestAddPreviousDocumentsInHeaderOrLines()
		{
			CombineAssertions(() =>
			{
				var prevdoc1 = declaration.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9001";

				var prevdoc2 = invoiceHeader.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9002";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled PreviousDocuments in Header when no docs are declared in lines", 2, wrapper.Header.PreviousDocuments.Count);
				AssertEquals("Expected empty PreviousDocuments in Lines when no docs are declared in lines", 0, wrapper.Lines.ToList()[0].PreviousDocuments.Count);

				var prevdoc3 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9003";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty PreviousDocuments in Header when there is at least one doc declared in lines", 0, wrapper.Header.PreviousDocuments.Count);
				AssertEquals("Expected filled PreviousDocuments in Lines when there is at least one doc declared in lines", 3, wrapper.Lines.ToList()[0].PreviousDocuments.Count);
			});
		}

		protected override DeclarationDVDSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new DeclarationDVDSendMessageWrapper(cusEntryHeader, certificateData);
	}
}
