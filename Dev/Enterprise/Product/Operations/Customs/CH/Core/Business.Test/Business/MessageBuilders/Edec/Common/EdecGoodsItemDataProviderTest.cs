using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class EdecGoodsItemDataProviderTest : TestCaseWithFactory
{
	protected abstract string MessageType { get; }

	protected abstract EdecGoodsItemDataProvider GetEdecGoodsItemDataProvider(CusEntryLine entryLine);

	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null", GetEdecGoodsItemDataProvider(null));
			AssertNotNull("Not Null", GetEdecGoodsItemDataProvider(entryLine));
		});
	}

	public virtual void TestProvider()
	{
		invoiceLine.JI_Weight = 100;
		invoiceLine.JI_WeightUQ = "KG";
		invoiceLine.JI_GrossMassConfirmation = true;
		invoiceLine.JI_NetWeight = 80;
		invoiceLine.JI_NetWeightUQ = "KG";
		invoiceLine.JI_NetMassConfirmation = true;
		invoiceLine.JI_CustomsThirdQuantity = 50;
		invoiceLine.JI_AdditionalUnitConfirmation = true;
		invoiceLine.JI_PermitObligation = RefCusCodeTestHelper.ValidPermitObligationCode;
		invoiceLine.JI_NonCustomsLawObligation = RefCusCodeTestHelper.ValidNonCustomsLawObligationCode;

		entryLine.CL_LineNumber = 1;
		entryLine.CL_Description = "Description";

		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(messageBuilder.TraderItemID), "1", messageBuilder.TraderItemID);
			AssertEquals(nameof(messageBuilder.Description), "Description", messageBuilder.Description);
			AssertEquals(nameof(messageBuilder.CommodityCode), "1000.1000", messageBuilder.CommodityCode);
			AssertEquals(nameof(messageBuilder.CommodityCodeConfirmation), false, messageBuilder.CommodityCodeConfirmation);
			AssertEquals(nameof(messageBuilder.GrossMass), 100m, messageBuilder.GrossMass);
			AssertEquals(nameof(messageBuilder.GrossMassConfirmation), true, messageBuilder.GrossMassConfirmation);
			AssertEquals(nameof(messageBuilder.NetMass), 80m, messageBuilder.NetMass);
			AssertEquals(nameof(messageBuilder.NetMassConfirmation), true, messageBuilder.NetMassConfirmation);
			AssertEquals(nameof(messageBuilder.AdditionalUnit), 50m, messageBuilder.AdditionalUnit);
			AssertEquals(nameof(messageBuilder.AdditionalUnitConfirmation), true, messageBuilder.AdditionalUnitConfirmation);
			AssertEquals(nameof(messageBuilder.PermitObligation), RefCusCodeTestHelper.ValidPermitObligationCode, messageBuilder.PermitObligation);
			AssertEquals(nameof(messageBuilder.NonCustomsLawObligation), RefCusCodeTestHelper.ValidNonCustomsLawObligationCode, messageBuilder.NonCustomsLawObligation);
		});
	}

	public void TestGrossMass()
	{
		declaration.JE_MessageType = MessageType;
		var messageBuilder = GetConfiguredGoodsItemDataProvider((i, v) => i.JI_WeightUQ = v, (i, v) => i.JI_Weight = v, Core.Constants.Weight.Grams, 1000, Core.Constants.Weight.Kilograms, 1);

		AssertEquals(nameof(messageBuilder.GrossMass), 2m, messageBuilder.GrossMass);
	}

	public void TestNetMass()
	{
		declaration.JE_MessageType = MessageType;
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		AssertNull("When MessageType is Import and CustomsSecondQuantity is not set, NetMass should be null", messageBuilder.NetMass);

		messageBuilder = GetConfiguredGoodsItemDataProvider((i, v) => i.JI_NetWeightUQ = v, (i, v) => i.JI_NetWeight = v, Core.Constants.Weight.Grams, 1000, Core.Constants.Weight.Kilograms, 1);

		AssertEquals(nameof(messageBuilder.NetMass), 2m, messageBuilder.NetMass);
	}

	public void TestAdditionalUnit()
	{
		declaration.JE_MessageType = MessageType;
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		AssertNull("When MessageType is Import and CustomsThirdQuantity is not set, AdditionalUnit should be null", messageBuilder.AdditionalUnit);

		messageBuilder = GetConfiguredGoodsItemDataProvider((i, v) => i.JI_CustomsThirdUnitQty = v, (i, v) => i.JI_CustomsThirdQuantity = v, "NAR", 1.01m, Core.Constants.Weight.Kilograms, 1);

		AssertEquals(nameof(messageBuilder.AdditionalUnit), 2.1m, messageBuilder.AdditionalUnit);
	}

	protected EdecGoodsItemDataProvider GetConfiguredGoodsItemDataProvider(Action<JobComInvoiceLine, string> unitSetter, Action<JobComInvoiceLine, decimal> valueSetter, string unit1, decimal value1, string unit2, decimal value2)
	{
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine1);
		unitSetter(invoiceLine1, unit1);
		valueSetter(invoiceLine1, value1);

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine2);
		unitSetter(invoiceLine2, unit2);
		valueSetter(invoiceLine2, value2);

		return GetEdecGoodsItemDataProvider(entryLine);
	}

	public void TestDescription()
	{
		entryLine.CL_Description = "CL_Description";
		invoiceLine.JI_Description = "JI_Description";

		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("Description should be CL_Description when CL_Description is not empty", "CL_Description", messageBuilder.Description);

			entryLine.CL_Description = "";
			messageBuilder = GetEdecGoodsItemDataProvider(entryLine);

			AssertEquals("Description should be JI_Description when CL_Description is empty", "JI_Description", messageBuilder.Description);
		});
	}

	public void TestCommodityCodeConfirmation()
	{
		invoiceLine.JI_Tariff = "10001000";

		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("When CommodityCode is different from the specified commodity codes then CommodityCodeConfirmation should be: ", false, messageBuilder.CommodityCodeConfirmation);

			var commodityCodeForConfirmation = new string[] { "35011090", "35019091", "35019099", "35021190", "35021990" };
			foreach (string commodityCode in commodityCodeForConfirmation)
			{
				invoiceLine.JI_Tariff = commodityCode;
				messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
				AssertEquals($"CommodityCodeConfirmation should be true for CommodityCode: {commodityCode}", true, messageBuilder.CommodityCodeConfirmation);
			}
		});
	}

	public void TestPermits()
	{
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("No permits - count", 0, messageBuilder.Permits.Count());

			var permit1a = invoiceLine.Permits.AddNew();
			permit1a.CSI_Code = "1a";
			var permit1b = invoiceLine.Permits.AddNew();
			permit1b.CSI_Code = "1b";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var permit2 = invoiceLine.Permits.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine2);
			permit2.CSI_Code = "2";
			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			var permit3 = invoiceLine3.Permits.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine3);
			permit3.CSI_Code = "3";

			messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
			AssertEquals("Count", 4, messageBuilder.Permits.Count());
			AssertEquals(nameof(permit1a), "1a", messageBuilder.Permits.ElementAt(0).PermitType);
			AssertEquals(nameof(permit1b), "1b", messageBuilder.Permits.ElementAt(1).PermitType);
			AssertEquals(nameof(permit2), "2", messageBuilder.Permits.ElementAt(2).PermitType);
			AssertEquals(nameof(permit3), "3", messageBuilder.Permits.ElementAt(3).PermitType);

			AssertSame("Cached", messageBuilder.Permits, messageBuilder.Permits);
		});
	}

	public void TestPermitsCombined()
	{
		var permit1a = invoiceLine.Permits.AddNew();
		permit1a.CSI_Code = "1";
		permit1a.CSI_IssuerType = "1";
		permit1a.CSI_ReferenceNumber = "1";
		permit1a.CSI_DateOfIssue = new ZDate(2022, 1, 1);
		permit1a.CSI_Description = "1";
		var permit1b = invoiceLine.Permits.AddNew();
		permit1b.CSI_Code = "1";
		permit1b.CSI_IssuerType = "1";
		permit1b.CSI_ReferenceNumber = "1";
		permit1b.CSI_DateOfIssue = new ZDate(2022, 1, 1);
		permit1b.CSI_Description = "1";
		var permit2 = invoiceLine.Permits.AddNew();
		permit2.CSI_Code = "2";
		permit2.CSI_IssuerType = "1";
		permit2.CSI_ReferenceNumber = "1";
		permit2.CSI_DateOfIssue = new ZDate(2022, 1, 1);
		permit2.CSI_Description = "1";
		var permit3 = invoiceLine.Permits.AddNew();
		permit3.CSI_Code = "1";
		permit3.CSI_IssuerType = "3";
		permit3.CSI_ReferenceNumber = "1";
		permit3.CSI_DateOfIssue = new ZDate(2022, 1, 1);
		permit3.CSI_Description = "1";
		var permit4 = invoiceLine.Permits.AddNew();
		permit4.CSI_Code = "1";
		permit4.CSI_IssuerType = "1";
		permit4.CSI_ReferenceNumber = "4";
		permit4.CSI_DateOfIssue = new ZDate(2022, 1, 1);
		permit4.CSI_Description = "1";
		var permit5 = invoiceLine.Permits.AddNew();
		permit5.CSI_Code = "1";
		permit5.CSI_IssuerType = "1";
		permit5.CSI_ReferenceNumber = "1";
		permit5.CSI_DateOfIssue = new ZDate(2022, 1, 5);
		permit5.CSI_Description = "1";
		var permit6 = invoiceLine.Permits.AddNew();
		permit6.CSI_Code = "1";
		permit6.CSI_IssuerType = "1";
		permit6.CSI_ReferenceNumber = "1";
		permit6.CSI_DateOfIssue = new ZDate(2022, 1, 1);
		permit6.CSI_Description = "6";
		var permit7 = invoiceLine.Permits.AddNew();
		permit7.CSI_Code = "1";
		permit7.CSI_IssuerType = "1";
		permit7.CSI_ReferenceNumber = "1";
		permit7.CSI_DateOfIssue = new ZDate(2022, 1, 1);
		permit7.CSI_Description = "1";
		permit7.PermitItemDetails.AddNew();

		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("Count", 7, messageBuilder.Permits.Count());
			AssertEquals("Permit 1", "1", messageBuilder.Permits.ElementAt(0).PermitType);
			AssertEquals("Permit 2", "2", messageBuilder.Permits.ElementAt(1).PermitType);
			AssertEquals("Permit 3", "3", messageBuilder.Permits.ElementAt(2).PermitAuthority);
			AssertEquals("Permit 4", "4", messageBuilder.Permits.ElementAt(3).PermitNumber);
			AssertEquals("Permit 5", new DateTime(2022, 1, 5), messageBuilder.Permits.ElementAt(4).IssueDate);
			AssertEquals("Permit 6", "6", messageBuilder.Permits.ElementAt(5).AdditionalInformation);
			AssertEquals("Permit 7", "1", messageBuilder.Permits.ElementAt(6).PermitType);
		});
	}

	public void TestSpecialMention()
	{
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2a = invoice2.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine2a);
		var invoiceLine2b = invoice2.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine2b);

		invoiceLine.SpecialMentions = "Line 1a\r\nLine 1b";
		invoiceLine2a.SpecialMentions = "Line 2a";
		invoiceLine2b.SpecialMentions = "Line 2b";

		var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			var specialMentionDataProviders = dataProvider.SpecialMentions;
			AssertEquals("Count", 4, specialMentionDataProviders.Count());
			AssertEquals("Text[0]", "Line 1a", specialMentionDataProviders.ElementAt(0).Text);
			AssertEquals("Text[1]", "Line 1b", specialMentionDataProviders.ElementAt(1).Text);
			AssertEquals("Text[2]", "Line 2a", specialMentionDataProviders.ElementAt(2).Text);
			AssertEquals("Text[3]", "Line 2b", specialMentionDataProviders.ElementAt(3).Text);
			AssertEquals("Seq#[0]", 1, specialMentionDataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("Seq#[1]", 2, specialMentionDataProviders.ElementAt(1).SequenceNumber);
			AssertEquals("Seq#[2]", 3, specialMentionDataProviders.ElementAt(2).SequenceNumber);
			AssertEquals("Seq#[3]", 4, specialMentionDataProviders.ElementAt(3).SequenceNumber);

			AssertSame("Cached", dataProvider.SpecialMentions, dataProvider.SpecialMentions);
		});
	}

	public void TestSpecialMentionMax99Lines()
	{
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine2);

		ZStringBuilder eightyLines = new ZStringBuilder();
		for (int n = 1; n <= 80; n++)
		{
			eightyLines.AppendFormat("Line {0}\r\n", n.ToString());
		}

		invoiceLine.SpecialMentions = eightyLines.ToString();
		invoiceLine2.SpecialMentions = eightyLines.ToString();

		var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			var specialMentionDataProviders = dataProvider.SpecialMentions;
			AssertEquals("Count", 99, specialMentionDataProviders.Count());
		});
	}

	public void TestStatistic()
	{
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertNotNull("Statistic should be defined", messageBuilder.Statistic);
			AssertEquals(true, messageBuilder.Statistic is IEdecGoodsItemStatistic);
		});
	}

	public void TestDetails()
	{
		invoiceLine.Vehicles.AddNew();
		invoiceLine.Tobaccos.AddNew();
		invoiceLine.AdditionalInformations.AddNew();

		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, messageBuilder.GoodsItemDetails.Count());
			AssertEquals(true, messageBuilder.GoodsItemDetails.FirstOrDefault() is IEdecGoodsItemDetail);
		});
	}

	public void TestPackagings()
	{
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine2);

		var packaging1 = declaration.Packages.AddNew();
		packaging1.CW_PackType = "ABC";
		packaging1.CW_MarksAndNos = "MARK AND NOS 1";

		var packaging2 = declaration.Packages.AddNew();
		packaging2.CW_PackType = "EFG";
		packaging2.CW_MarksAndNos = "MARK AND NOS 2";

		var packageInvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine1.CHC_CW = packaging1.PK;
		packageInvoiceLine1.CHC_NumberOfPacks = 5;

		var packageInvoiceLine2 = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine2.CHC_CW = packaging2.PK;
		packageInvoiceLine2.CHC_NumberOfPacks = 7;

		var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			var packagingDataProvides = dataProvider.Packagings;
			AssertEquals("Count", 2, packagingDataProvides.Count());
			AssertEquals("1. Packaging Type", "ABC", packagingDataProvides.ElementAt(0).PackagingType);
			AssertEquals("1. Packaging Reference Number", "MARK AND NOS 1", packagingDataProvides.ElementAt(0).PackagingReferenceNumber);
			AssertEquals("1. Packaging Quantity", 5m, packagingDataProvides.ElementAt(0).Quantity);
			AssertEquals("2. Packaging Type", "EFG", packagingDataProvides.ElementAt(1).PackagingType);
			AssertEquals("1. Packaging Reference Number", "MARK AND NOS 2", packagingDataProvides.ElementAt(1).PackagingReferenceNumber);
			AssertEquals("1. Packaging Quantity", 7m, packagingDataProvides.ElementAt(1).Quantity);
		});
	}

	public void TestProducedDocuments()
	{
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine2);

		var supportingInfoFromLine = invoiceLine.SupportingDocuments.AddNew();
		supportingInfoFromLine.CSI_Code = "EFG";
		supportingInfoFromLine.CSI_ReferenceNumber = "456";
		supportingInfoFromLine.CSI_ReferenceNumber2 = "789";
		supportingInfoFromLine.CSI_DateOfIssue = new ZDate(2022, 2, 1);

		var supportingInfoDuplicate = invoiceLine2.SupportingDocuments.AddNew();
		supportingInfoDuplicate.CSI_Code = "ABC";
		supportingInfoDuplicate.CSI_ReferenceNumber = "123";
		supportingInfoDuplicate.CSI_ReferenceNumber2 = "456";
		supportingInfoDuplicate.CSI_DateOfIssue = new ZDate(2022, 1, 1);

		invoice.SupportingDocuments.Add(supportingInfoDuplicate.Clone());
		var supportingInfoFromHeader = invoice.SupportingDocuments.AddNew();
		supportingInfoFromHeader.CSI_Code = "HIJ";
		supportingInfoFromHeader.CSI_ReferenceNumber = "789";
		supportingInfoFromHeader.CSI_ReferenceNumber2 = "XYZ";
		supportingInfoFromHeader.CSI_DateOfIssue = new ZDate(2022, 3, 1);

		var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			var producedDocumentProvider = dataProvider.ProducedDocuments.OrderBy(x => x.DocumentType);
			AssertEquals("Count", 3, producedDocumentProvider.Count());
			AssertEquals("2. ProducedDocument Additional Information", "456", producedDocumentProvider.ElementAt(0).AdditionalInformation);
			AssertEquals("2. ProducedDocument Document Reference Number", "123", producedDocumentProvider.ElementAt(0).DocumentReferenceNumber);
			AssertEquals("2. ProducedDocument Document Type", "ABC", producedDocumentProvider.ElementAt(0).DocumentType);
			AssertEquals("2. ProducedDocument Issue Date", new ZDate(2022, 1, 1), producedDocumentProvider.ElementAt(0).IssueDate);
			AssertEquals("1. ProducedDocument Additional Information", "789", producedDocumentProvider.ElementAt(1).AdditionalInformation);
			AssertEquals("1. ProducedDocument Document Reference Number", "456", producedDocumentProvider.ElementAt(1).DocumentReferenceNumber);
			AssertEquals("1. ProducedDocument Document Type", "EFG", producedDocumentProvider.ElementAt(1).DocumentType);
			AssertEquals("1. ProducedDocument Issue Date", new ZDate(2022, 2, 1), producedDocumentProvider.ElementAt(1).IssueDate);
			AssertEquals("3. ProducedDocument Additional Information", "XYZ", producedDocumentProvider.ElementAt(2).AdditionalInformation);
			AssertEquals("3. ProducedDocument Document Reference Number", "789", producedDocumentProvider.ElementAt(2).DocumentReferenceNumber);
			AssertEquals("3. ProducedDocument Document Type", "HIJ", producedDocumentProvider.ElementAt(2).DocumentType);
			AssertEquals("3. ProducedDocument Issue Date", new ZDate(2022, 3, 1), producedDocumentProvider.ElementAt(2).IssueDate);
		});
	}

	public void TestNonCustomsLaws()
	{
		var dataProvider = GetEdecGoodsItemDataProvider(entryLine);

		AssertEquals($"Is {nameof(EdecNonCustomsLawDataProvider)}", true, dataProvider.NonCustomsLaws is IEnumerable<EdecNonCustomsLawDataProvider>);
		AssertSame("Cached", dataProvider.NonCustomsLaws, dataProvider.NonCustomsLaws);
	}

	public void TestRefinement()
	{
		CombineAssertions(() =>
		{
			var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
			AssertNull("Empty InAndOutwardsProcessing", dataProvider.Refinement);

			dataProvider = GetEdecGoodsItemDataProvider(entryLine);
			invoiceLine.InAndOutwardProcessingBillingType = "1";
			AssertNotNull("Non-empty InAndOutwardsProcessing", dataProvider.Refinement);

			AssertSame("Cached", dataProvider.NonCustomsLaws, dataProvider.NonCustomsLaws);
		});
	}

	public void TestNotifications()
	{
		CombineAssertions(() =>
		{
			var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
			AssertNotNull(dataProvider.Notifications);
			AssertSame("Cached", dataProvider.Notifications, dataProvider.Notifications);
		});
	}

	public virtual void TestAdditionalTaxes()
	{
		var tariffDetail = invoiceLine.AdditionalTaxes.AddNew();
		tariffDetail.BZ_Tariff = "123-456";
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		AssertNull(messageBuilder.AdditionalTaxes);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
	}

	protected internal JobDeclaration declaration;
	protected internal JobComInvoiceHeader invoice;
	protected internal JobComInvoiceLine invoiceLine;
	protected internal CusEntryLine entryLine;
}
