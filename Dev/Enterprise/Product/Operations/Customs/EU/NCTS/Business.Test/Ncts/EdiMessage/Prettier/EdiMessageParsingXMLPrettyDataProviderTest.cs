using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class EdiMessageParsingXMLPrettyDataProviderTest : TestCaseWithFactory
	{
		public void TestMessage() => Assert(ReferenceEquals(message, parsingXMLProvider.Message));
		public void TestMessageType() => AssertEquals("CC015C", parsingXMLProvider.MessageType);
		public void TestMessageRecipient() => AssertEquals("NTA.GB", parsingXMLProvider.MessageRecipient);
		public void TestLRN() => AssertEquals("LRN", parsingXMLProvider.LRN);
		public void TestMRN() => AssertEquals("MRN", parsingXMLProvider.MRN);
		public void TestCustomsOfficeOfDeparture() => AssertEquals("Customs Office Of Departure", parsingXMLProvider.CustomsOfficeOfDeparture);
		public void TestCustomsOfficeOfDestination() => AssertEquals("Customs Office Of Destination", parsingXMLProvider.CustomsOfficeOfDestination);
		public void TestDeclarationType() => AssertEquals("Declaration Type", parsingXMLProvider.DeclarationType);
		public void TestAdditionalDeclarationType() => AssertEquals("Additional Declaration Type", parsingXMLProvider.AdditionalDeclarationType);
		public void TestReducedDatasetIndicator() => AssertEquals("Reduced Dataset Indicator", parsingXMLProvider.ReducedDatasetIndicator);
		public void TestSimplifiedProcedure() => AssertEquals("Simplified Procedure", parsingXMLProvider.SimplifiedProcedure);
		public void TestSecurity() => AssertEquals("Security", parsingXMLProvider.Security);
		public void TestBindingItinerary() => AssertEquals("Binding Itinerary", parsingXMLProvider.BindingItinerary);
		public void TestPrincipleEORI() => AssertEquals("Principle EORI", parsingXMLProvider.PrincipleEORI);
		public void TestRepresentativeEORI() => AssertEquals("Representative EORI", parsingXMLProvider.RepresentativeEORI);
		public void TestConsigneeEORI() => AssertEquals("Consignee EORI", parsingXMLProvider.ConsigneeEORI);
		public void TestConsignorEORI() => AssertEquals("Consignor EORI", parsingXMLProvider.ConsignorEORI);

		public void TestGuarantees()
		{
			NCTSPrettierGuaranteeData[] expectedGuaranties = {
				new (type: "Guarantee Type 1",
					grn: "GRN (Guarantee Reference Number) 1",
					otherNumber: "Other Guarantee Reference 1",
					amount: "Amount To Be Covered 1",
					currency: "Currency 1"),
				new (type: "Guarantee Type 2",
					grn: "GRN (Guarantee Reference Number) 2",
					otherNumber: "Other Guarantee Reference 2",
					amount: "Amount To Be Covered 2",
					currency: "Currency 2"),
			};
			AssertContainsExactElementsInExactOrder(expectedGuaranties, parsingXMLProvider.Guarantees);
		}

		public void TestFillSharedFields()
		{
			var sharedFields = new NCTSPrettierSharedFields();
			parsingXMLProvider.FillSharedFields(sharedFields);
			AssertContainsExactElementsInAnyOrder(expected: new [] {
					("Message Type", "CC015C"),
					("Declaration Type", "Declaration Type"),
					("Additional Declaration Type", "Additional Declaration Type"),
					("LRN (Local Reference Number)", "LRN"),
					("MRN (Movement Reference Number)", "MRN"),
					("Customs Office (Departure)", "Customs Office Of Departure"),
					("Customs Office (Destination)", "Customs Office Of Destination"),
					("Reduced Dataset", "Reduced Dataset Indicator"),
					("Simplified Procedure", "Simplified Procedure"),
					("Security", "Security"),
					("Binding Itinerary", "Binding Itinerary"),
					("Principle EORI", "Principle EORI"),
					("Representative EORI", "Representative EORI"),
					("Consignee EORI", "Consignee EORI"),
					("Consignor EORI", "Consignor EORI") },
				actual: sharedFields);
		}

		public void TestAdditionalBlocks() => CombineAssertions(() =>
		{
			var additionalBlocks = parsingXMLProvider.AdditionalBlocks;
			AssertEquals("Count", 2, additionalBlocks.Count);

			var additionalBlock = additionalBlocks.First();
			AssertType<NCTSPrettierGuaranteesTable>("first additional block is NCTSPrettierGuaranteesTable", additionalBlock);
			AssertEquals("guaranteesTableHTML", ExpectedGuaranteesHtml.Replace("\r\n", string.Empty).Replace("\t", string.Empty), additionalBlock.ToString());

			additionalBlock = additionalBlocks.Skip(1).First();
			AssertType<NCTSPrettierConsignmentsTable>("second additional block is NCTSPrettierGuaranteesTable", additionalBlock);
			AssertEquals("consignmentsTableHTML", ExpectedConsignmentsHtml.Replace("\r\n", string.Empty).Replace("\t", string.Empty), additionalBlock.ToString());
		});

		public void TestConsignments()
		{
			NCTSPrettierConsignmentData[] expectedConsignments =
			{
				new (ucrReference: "UCR Reference Number 1", goodsItems: new NCTSPrettierGoodsItemData[] {
					new (itemNumber: "Declaration Goods Item 1", ucrReference: "Goods Item UCR Reference Number 1", description: "Commodity Description Of Goods 1", harmonizedSubHeadingCode: "Commodity Harmonized SubHeading Code 1"),
					new (itemNumber: "Declaration Goods Item 2", ucrReference: "Goods Item UCR Reference Number 2", description: "Commodity Description Of Goods 2", harmonizedSubHeadingCode: "Commodity Harmonized SubHeading Code 2"),
				}),
				new (ucrReference: "UCR Reference Number 2", goodsItems: new NCTSPrettierGoodsItemData[] {
					new (itemNumber: "Declaration Goods Item 1", ucrReference: "Goods Item UCR Reference Number 1", description: "Commodity Description Of Goods 1", harmonizedSubHeadingCode: "Commodity Harmonized SubHeading Code 1"),
				}),
			};
			AssertContainsExactElementsInExactOrder(expectedConsignments, parsingXMLProvider.Consignments);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var retriever = new EmbeddedResourceRetriever();
			var testMessageContent = retriever.GetString(NctsEdiMessagePrettierTests.TestFilePath);

			message = Factory.New<EDIMessage>();
			message.EM_MessageText = testMessageContent;
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			parsingXMLProvider = new EdiMessageParsingXMLPrettyDataProvider(message);
		}

		EDIMessage message;
		EdiMessageParsingXMLPrettyDataProvider parsingXMLProvider;

		const string ExpectedGuaranteesHtml = @"<H3>Guarantees</H3>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
	<tr>
		<td width=""25%"">Type</td>
		<td width=""25%"">GRN Number</td>
		<td width=""25%"">Other Number</td>
		<td width=""25%"">Amount</td>
	</tr>
	<tr>
		<td>Guarantee Type 1</td>
		<td>GRN (Guarantee Reference Number) 1</td>
		<td>Other Guarantee Reference 1</td>
		<td>Amount To Be Covered 1 Currency 1</td>
	</tr>
	<tr>
		<td>Guarantee Type 2</td>
		<td>GRN (Guarantee Reference Number) 2</td>
		<td>Other Guarantee Reference 2</td>
		<td>Amount To Be Covered 2 Currency 2</td>
	</tr>
</table>";

		const string ExpectedConsignmentsHtml = @"<H3>Consignments</H3>
<p>
	<strong>Number of Consignments: </strong>2<br>
	<strong>Number of Consignment Items: </strong>3
</p>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
	<tr>
		<td width=""25%"">UCR Reference</td>
		<td width=""75%"">Goods Items</td>
	</tr>
	<tr>
		<td>UCR Reference Number 1</td>
		<td>
			<p>
				<table border=""0"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
					<tr>
						<td class=""tdGoodsItemsTitle"">Item Number</td>
						<td class=""tdGoodsItemsTitle"">UCR Reference</td>
						<td class=""tdGoodsItemsTitle"">Description</td>
						<td class=""tdGoodsItemsTitle"">Code</td>
					</tr>
					<tr>
						<td>Declaration Goods Item 1</td>
						<td>Goods Item UCR Reference Number 1</td>
						<td>Commodity Description Of Goods 1</td>
						<td>Commodity Harmonized SubHeading Code 1</td>
					</tr>
					<tr>
						<td>Declaration Goods Item 2</td>
						<td>Goods Item UCR Reference Number 2</td>
						<td>Commodity Description Of Goods 2</td>
						<td>Commodity Harmonized SubHeading Code 2</td>
					</tr>
				</table>
			</p>
		</td>
	</tr>
	<tr>
		<td>UCR Reference Number 2</td>
		<td>
			<p>
				<table border=""0"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
					<tr>
						<td class=""tdGoodsItemsTitle"">Item Number</td>
						<td class=""tdGoodsItemsTitle"">UCR Reference</td>
						<td class=""tdGoodsItemsTitle"">Description</td>
						<td class=""tdGoodsItemsTitle"">Code</td>
					</tr>
					<tr>
						<td>Declaration Goods Item 1</td>
						<td>Goods Item UCR Reference Number 1</td>
						<td>Commodity Description Of Goods 1</td>
						<td>Commodity Harmonized SubHeading Code 1</td>
					</tr>
				</table>
			</p>
		</td>
	</tr>
</table>";
	}
}
