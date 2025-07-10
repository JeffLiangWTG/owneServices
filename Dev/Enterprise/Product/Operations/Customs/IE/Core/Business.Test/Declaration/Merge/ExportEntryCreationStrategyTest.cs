using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExportEntryCreationStrategyTest : EU.Business.Declaration.Testing.EntryCreationStrategyTest
	{
		public void TestGetKeyForHeaderContainsJZ_ValuationCode()
		{
			invoice.JZ_ValuationCode = "10";
			var entryCreationStrategy = new ExportEntryCreationStrategyForTest(declaration);
			Assert(entryCreationStrategy.GetKeyForLine(invoiceLine).Contains(invoice.JZ_ValuationCode));
		}

		public void TestCH_MessageTypeToNewEntryHeader()
		{
			AssertEquals("CH_MessageTypes may not be same, so populate it one by one", ZString.Empty, new ExportEntryCreationStrategyForTest(declaration).CH_MessageTypeToNewEntryHeader);
		}

		public override void TestGetKeyForHeader()
		{
			base.TestGetKeyForHeader();

			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = "HERE";
			invoice.JZ_IncoTermPlace = "BOB'S PLACE";
			invoice.JZ_AdditionalTerms = "THESE Terms";
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var key = entryCreationStrategy.GetKeyForHeader(invoiceLine);

			CombineAssertions("MergeKeys for Header", () =>
			{
				Assert("JZ_IncoTerm", key.Contains(new ZString("FOB")));
				Assert("ZG_AgreedPlaceCode", key.Contains(new ZString("HERE")));
				Assert("JZ_IncoTermPlace", key.Contains(new ZString("BOB'S PLACE")));
				Assert("JZ_AdditionalTerms", key.Contains(new ZString("THESE Terms")));
				Assert("JZ_RX_NKInvoice_Currency", key.Contains(new ZString(Core.Constants.CurrencyCodes.EuropeanUnion)));
			});
		}

		public void TestGetKeyForLine()
		{
			invoice.JZ_ValuationCode = "A";
			invoice.JZ_UCR = "INVUCR";
			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Italy;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;

			var key = entryCreationStrategy.GetKeyForLine(invoiceLine);

			CombineAssertions("MergeKeys for Line", () =>
			{
				Assert("JZ_ValuationCode", key.Contains(new ZString("A")));
				Assert("JZ_UCR", key.Contains(new ZString("INVUCR")));
				Assert("JI_RN_NKCountryOfExport added via EntryLineConfiguration", key.Contains(new ZString(Core.Constants.CountryCodes.Italy)));
				Assert("JI_CustomsUnitQty", key.Contains(new ZString(Core.Constants.Weight.Kilograms)));
			});
		}

		public void TestAddLineLevelOrganisationMergeKeys_ExporterSupplier()
		{
			var exporter = Factory.New<OrgHeader>();
			var exporterAddressPK = exporter.MainAddress.PK;
			invoiceLine.JI_OA_ExporterAddress = exporterAddressPK;

			var invSupplier = Factory.New<OrgHeader>();
			var invSupplierAddressPK = invSupplier.MainAddress.PK;
			invoice.JZ_OA_SupplierAddress = invSupplierAddressPK;

			var decSupplier = Factory.New<OrgHeader>();
			var supDocAddress = declaration.SupplierDocumentaryAddress;
			supDocAddress.OrganisationPK = decSupplier.PK;
			var decSupplierAddressPK = supDocAddress.PK;

			var key = entryCreationStrategy.GetKeyForLine(invoiceLine);

			CombineAssertions(() =>
			{
				Assert("ExporterAddress is not empty", key.Contains(exporterAddressPK));
				Assert("SupplierAddress is not empty", !key.Contains(invSupplierAddressPK));
				Assert("SupplierDocumentaryAddress is not empty", !key.Contains(decSupplierAddressPK));
			});

			invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
			key = entryCreationStrategy.GetKeyForLine(invoiceLine);
			CombineAssertions(() =>
			{
				Assert("ExporterAddress is empty", !key.Contains(exporterAddressPK));
				Assert("SupplierAddress is not empty", key.Contains(invSupplierAddressPK));
				Assert("SupplierDocumentaryAddress is not empty", !key.Contains(decSupplierAddressPK));
			});

			invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
			key = entryCreationStrategy.GetKeyForLine(invoiceLine);
			CombineAssertions(() =>
			{
				Assert("ExporterAddress is empty", !key.Contains(exporterAddressPK));
				Assert("SupplierAddress is empty", !key.Contains(invSupplierAddressPK));
				Assert("SupplierDocumentaryAddress is not empty", key.Contains(decSupplierAddressPK));
			});
		}

		public void TestAddLineLevelOrganisationMergeKeys_ConsigneeBuyerImporter()
		{
			var consignee = Factory.New<OrgHeader>();
			var consigneeAddressPK = consignee.MainAddress.PK;
			invoiceLine.JI_OA_ConsigneeAddress = consigneeAddressPK;

			var buyer = Factory.New<OrgHeader>();
			var buyerAddressPK = buyer.MainAddress.PK;
			invoice.JZ_OA_BuyerAddress = buyerAddressPK;

			var importer = Factory.New<OrgHeader>();
			var impDocAddress = declaration.ImporterDocumentaryAddress;
			impDocAddress.OrganisationPK = importer.PK;
			var importerAddressPK = impDocAddress.PK;

			var key = entryCreationStrategy.GetKeyForLine(invoiceLine);

			CombineAssertions(() =>
			{
				Assert("ConsigneeAddress is not empty", key.Contains(consigneeAddressPK));
				Assert("BuyerAddress is not empty", !key.Contains(buyerAddressPK));
				Assert("ImporterDocumentaryAddress is not empty", !key.Contains(importerAddressPK));
			});

			invoiceLine.JI_OA_ConsigneeAddress = ZGuid.Empty;
			key = entryCreationStrategy.GetKeyForLine(invoiceLine);
			CombineAssertions(() =>
			{
				Assert("ConsigneeAddress is empty", !key.Contains(consigneeAddressPK));
				Assert("BuyerAddress is not empty", key.Contains(buyerAddressPK));
				Assert("ImporterDocumentaryAddress is not empty", !key.Contains(importerAddressPK));
			});

			invoice.JZ_OA_BuyerAddress = ZGuid.Empty;
			key = entryCreationStrategy.GetKeyForLine(invoiceLine);
			CombineAssertions(() =>
			{
				Assert("ConsigneeAddress is empty", !key.Contains(consigneeAddressPK));
				Assert("BuyerAddress is empty", !key.Contains(buyerAddressPK));
				Assert("ImporterDocumentaryAddress is not empty", key.Contains(importerAddressPK));
			});
		}

		public void TestGetAdditionalInfoKeys()
		{
			AssertArrayEqualsByElements(new[]
			{
				AdditionalInfo.Schema.CSI_SubType,
				AdditionalInfo.Schema.CSI_Code,
				AdditionalInfo.Schema.CSI_ReferenceNumber,
				AdditionalInfo.Schema.CSI_Description
			}, entryCreationStrategy.GetAdditionalInfoKeys());
		}

		public void TestMergeKeyContainsNationalCodes()
		{
			invoiceLine.JI_NationalAdditionalCode1 = "ABCD";
			invoiceLine.JI_NationalAdditionalCode2 = "EFGH";
			var mergeKey = entryCreationStrategy.GetKeyForLine(invoiceLine);
			AssertEquals("NationalCodes", true, mergeKey.Contains(new ZString("ABCD_EFGH")));
		}

		public void TestMergeKeyContainsPackagesPivot()
		{
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			invoiceLine.ContainersPivot.RemoveAndDeleteAll();
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(x =>
			{
				x.IsLinked = x.PackagePk == package1.PK || x.PackagePk == package2.PK;
			});
			var mergeKey = entryCreationStrategy.GetKeyForLine(invoiceLine);
			AssertEquals("PackagesPivot.MergeKey", true, mergeKey.Contains(invoiceLine.PackagesPivot.MergeKey));
		}

		protected override string[] GetExpectedCusAuthorizationUsageKeys() => new[]
		{
			CusAuthorizationUsage.Schema.AGC_Code,
			CusAuthorizationUsage.Schema.AGC_Number
		};

		public void TestGetPreviousDocumentKeys()
		{
			AssertArrayEqualsByElements(new[]
			{
				PreviousDocument.Schema.CSI_ItemNumber,
				PreviousDocument.Schema.CSI_PackType,
				PreviousDocument.Schema.CSI_PackQty,
				PreviousDocument.Schema.CSI_UnitOfQuantity,
				PreviousDocument.Schema.CSI_Quantity,
				PreviousDocument.Schema.CSI_Code,
				PreviousDocument.Schema.CSI_ReferenceNumber
			}, entryCreationStrategy.GetPreviousDocumentKeys());
		}

		public void TestGetPreviousDocumentHeaderKeys()
		{
			AssertArrayEqualsByElements(new[]
			{
				PreviousDocument.Schema.CSI_Code,
				PreviousDocument.Schema.CSI_ReferenceNumber
			}, entryCreationStrategy.GetPreviousDocumentHeaderKeys());
		}

		protected override string[] GetExpectedSupportingDocumentKeys() => new[]
		{
			SupportingDocument.Schema.CSI_ItemNumber,
			SupportingDocument.Schema.CSI_Code,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_AdditionalDescription,
			SupportingDocument.Schema.CSI_DateOfExpiry
		};

		protected override string[] GetExpectedCusSupplyChainActorReferenceKeys() => new[]
		{
			EU.Business.Declaration.CusSupplyChainActorReference.Schema.CFR_Code,
			EU.Business.Declaration.CusSupplyChainActorReference.Schema.CFR_Reference
		};

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			return jobDeclaration;
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryCreationStrategy = new ExportEntryCreationStrategy(declaration);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		ExportEntryCreationStrategy entryCreationStrategy;

		sealed class ExportEntryCreationStrategyForTest : ExportEntryCreationStrategy
		{
			public ExportEntryCreationStrategyForTest(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
			{
			}

			public new ZString CH_MessageTypeToNewEntryHeader => base.CH_MessageTypeToNewEntryHeader;
		}
	}
}
