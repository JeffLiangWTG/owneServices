using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using ZDateTime = CargoWise.Types.ZDateTime;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class CusEntryInstructionInwardRelatedFieldChangeCheckerTest : TestCaseWithFactory
	{
		public void TestNoUpdateWhenFieldNotChanged()
		{
			AssertEquals(false, ShouldUpdateBWH);
		}

		public void TestUpdate_DeclarationSupplierDocumentaryAddressChanged()
		{
			var org = Factory.New<OrgHeader>();
			declaration.SupplierDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdate_DeclarationImporterDocumentaryAddressChanged()
		{
			var org = Factory.New<OrgHeader>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdate_DeclarationJE_RL_NKPortOfLoadingChanged()
		{
			declaration.JE_RL_NKPortOfLoading = "DEABC";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdate_DeclarationJE_RL_NKPortOfFirstArrivalChanged()
		{
			declaration.JE_RL_NKPortOfFirstArrival = "DEABC";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdate_DeclarationJE_TransportModeChanged()
		{
			declaration.JE_TransportMode = "SEA";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdate_DeclarationJE_OA_ConsigneeAddressChanged()
		{
			declaration.JE_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdate_DeclarationJE_OA_SellerAddressChanged()
		{
			declaration.JE_OA_SellerAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdate_Declaration_JE_HouseBill_Changed()
		{
			declaration.JE_HouseBill = "BILL123";
			AssertEquals("Field should not affect BWH Inventory", false, ShouldUpdateBWH);
		}

		public void TestUpdateInvoice_JZ_InvoiceNumber()
		{
			invoiceHeader.JZ_InvoiceNumber = "5";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoice_JZ_InvoiceDate()
		{
			invoiceHeader.JZ_InvoiceDate = ZDateTime.BrettsBirthday;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoice_JZ_RX_NKInvoice_Currency()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoice_JZ_IncoTerm()
		{
			invoiceHeader.JZ_IncoTerm = "DEF";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoice_JZ_IncoTermPlace()
		{
			invoiceHeader.JZ_IncoTermPlace = "Place";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoice_JZ_ValuationCode()
		{
			invoiceHeader.JZ_ValuationCode = "10";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoice_JZ_NoOfPacks()
		{
			invoiceHeader.JZ_NoOfPacks = 1;
			AssertEquals("Field should not affect BWH Inventory", false, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceHeaderSupportingDocument_CSI_Code()
		{
			invoiceHeaderSupportingDocument.CSI_Code = "N321";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceHeaderSupportingDocument_CSI_ReferenceNumber()
		{
			invoiceHeaderSupportingDocument.CSI_ReferenceNumber = "REF-123";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceHeaderSupportingDocument_CSI_DateOfIssue()
		{
			invoiceHeaderSupportingDocument.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestDeleteInvoiceHeaderSupportingDocument()
		{
			invoiceHeader.SupportingDocuments.RemoveAndDelete(invoiceHeader.SupportingDocuments[0]);
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestAddInvoiceHeaderSupportingDocument()
		{
			invoiceHeader.SupportingDocuments.AddNew();
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_Tariff()
		{
			invoiceLine.JI_Tariff = "1234";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_CountryOfOrigin()
		{
			invoiceLine.JI_CountryOfOrigin = "IE";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_ZG_CountryOfSupply()
		{
			invoiceLine.JI_Tariff = "1234";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_PrimaryPreference()
		{
			invoiceLine.JI_PrimaryPreference = "200";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_LinePrice()
		{
			invoiceLine.JI_LinePrice = 1234m;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_PartPK()
		{
			invoiceLine.JI_OP = Factory.NewWithValidTestData<OrgSupplierPart>().PK;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_NetWeight()
		{
			invoiceLine.JI_NetWeight = 1234m;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_NetWeightUQ()
		{
			invoiceLine.JI_NetWeightUQ = "G";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_CustomsSecondQuantity()
		{
			invoiceLine.JI_CustomsSecondQuantity = 1234m;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_CustomsSecondUnitQty()
		{
			invoiceLine.JI_CustomsSecondUnitQty = "G";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_CustomsThirdQuantity()
		{
			invoiceLine.JI_CustomsThirdQuantity = 1234m;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_CustomsThirdUnitQty()
		{
			invoiceLine.JI_CustomsThirdUnitQty = "G";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_BondedWhsQuantity()
		{
			invoiceLine.JI_BondedWhsQuantity = 123m;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_BondedWhsUnitQty()
		{
			invoiceLine.JI_CustomsThirdUnitQty = "G";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_CustomsQuantity()
		{
			invoiceLine2.JI_CustomsQuantity = 60;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLine_JI_CustomsUnitQty()
		{
			invoiceLine2.JI_CustomsUnitQty = "G";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLineSupportingDocument_CSI_Code()
		{
			invoiceLineSupportingDocument.CSI_Code = "N321";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLineSupportingDocument_CSI_ReferenceNumber()
		{
			invoiceLineSupportingDocument.CSI_ReferenceNumber = "REF-123";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLineSupportingDocument_CSI_DateOfIssue()
		{
			invoiceLineSupportingDocument.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLineSupportingDocument_CSI_Status()
		{
			invoiceLineSupportingDocument.CSI_Status = "J";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLineSupportingDocument_CSI_Quantity()
		{
			invoiceLineSupportingDocument.CSI_Quantity = 53m;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateInvoiceLineSupportingDocument_CSI_UnitOfQuantity()
		{
			invoiceLineSupportingDocument.CSI_UnitOfQuantity = "NAR";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestDeleteInvoiceLineSupportingDocument()
		{
			invoiceLine.SupportingDocuments.RemoveAndDelete(invoiceLine.SupportingDocuments[0]);
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestAddInvoiceLineSupportingDocument()
		{
			invoiceLine.SupportingDocuments.AddNew();
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateCharge_J7_Amount()
		{
			invoiceLineCharge.J7_Amount = ZDecimal.Zero;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateCharge_J7_ChargeType()
		{
			invoiceLineCharge.J7_ChargeType = "123";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateCharge_J7_RX_NKCurrency()
		{
			invoiceLineCharge.J7_RX_NKCurrency = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateCharge_J7_IsDutiable()
		{
			invoiceLineCharge.J7_IsDutiable = true;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateCharge_J7_IsGSTApplicable()
		{
			invoiceLineCharge.J7_IsGSTApplicable = true;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateCharge_J7_IsIncludedInITOT()
		{
			invoiceLineCharge.J7_IsIncludedInITOT = true;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateCharge_J7_IsStatisticalValueApplicable()
		{
			invoiceLineCharge.J7_IsStatisticalValueApplicable = true;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestChargeAdded()
		{
			invoiceLine.Charges.AddNew();
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestChargeRemoved()
		{
			invoiceLine.Charges.RemoveAndDelete(invoiceLineCharge);
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateApportionedCharge_J7_Amount()
		{
			invoiceLineApportionedCharge.J7_Amount = ZDecimal.Zero;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateApportionedCharge_J7_ChargeType()
		{
			invoiceLineApportionedCharge.J7_ChargeType = "123";
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateApportionedCharge_J7_RX_NKCurrency()
		{
			invoiceLineApportionedCharge.J7_RX_NKCurrency = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateApportionedCharge_J7_IsDutiable()
		{
			invoiceLineApportionedCharge.J7_IsDutiable = true;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateApportionedCharge_J7_IsGSTApplicable()
		{
			invoiceLineApportionedCharge.J7_IsGSTApplicable = true;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateApportionedCharge_J7_IsIncludedInITOT()
		{
			invoiceLineApportionedCharge.J7_IsIncludedInITOT = true;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestUpdateApportionedCharge_J7_IsStatisticalValueApplicable()
		{
			invoiceLineApportionedCharge.J7_IsStatisticalValueApplicable = true;
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestApportionedChargeAdded()
		{
			invoiceLine.ApportionedCharges.AddNew();
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestApportionedChargeRemoved()
		{
			invoiceLine.ApportionedCharges.RemoveAndDelete(invoiceLineApportionedCharge);
			AssertEquals(true, ShouldUpdateBWH);
		}

		public void TestBondedWhsFieldsChangedAndShouldUpdate_DoesNotThrowWhenNoDocketLinesLoaded()
		{
			declaration.JE_UCR = "ABCD123";
			AssertNoExceptionThrown(() => _ = entryInstruction.BondedWhsFieldsChangedAndShouldUpdate());
		}

		public void TestBondedWhsFieldsChangedAndShouldUpdate_ShouldNotThrow_WhenInvoiceLineDeleted()
		{
			invoiceLine2.Delete();

			var shouldUpdate = false;
			AssertNoExceptionThrown(() => shouldUpdate = entryInstruction.BondedWhsFieldsChangedAndShouldUpdate());
			Assert("Deleted invoice line means update is needed", shouldUpdate);
		}

		protected override void SetUp()
		{
			var helper = new WhsDataTestHelper(Factory);
			helper.Importer.MainAddress.OA_RN_NKCountryCode = "DE";
			declaration = helper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0001", "ENT001", 1000m, true);

			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, declaration.Branch.EntityPK.ToGuid(), Guid.Empty, false))
			{
				entryInstruction = declaration.CustomsEntryInstructions[0];

				entryInstruction.CEI_Style = "VZL";

				declaration.JE_OA_ImporterAddress = helper.Importer.MainAddress.PK;
				declaration.JE_OA_SellerAddress = helper.Supplier.MainAddress.PK;
				declaration.SupplierAddressOrgPK = helper.Supplier.MainAddress.PK;
				declaration.JE_RL_NKPortOfLoading = "DEWIB";
				declaration.JE_RL_NKPortOfFirstArrival = "DEWIB";
				declaration.JE_TransportMode = "AIR";
				declaration.JE_OA_ConsigneeAddress = helper.Supplier.MainAddress.PK;
				declaration.JE_OA_SellerAddress = helper.Supplier.MainAddress.PK;

				var entryHeader = declaration.CustomsEntryHeaders[0];

				invoiceHeader = (JobComInvoiceHeader)declaration.Invoices.Single();
				invoiceHeader.JZ_ValuationCode = "42";
				invoiceHeader.JZ_IncoTerm = "ABC";
				invoiceHeader.JZ_IncoTermPlace = "Berlin";
				invoiceHeader.JZ_InvoiceNumber = "1";
				invoiceHeader.JZ_InvoiceDate = ZDateTime.Today;

				invoiceHeaderSupportingDocument = invoiceHeader.SupportingDocuments.AddNew();
				invoiceHeaderSupportingDocument.CSI_Code = "N380";
				invoiceHeaderSupportingDocument.CSI_ReferenceNumber = "1";
				invoiceHeaderSupportingDocument.CSI_DateOfIssue = ZDateTime.Today;

				invoiceLine = invoiceHeader.JobComInvoiceLines.Cast<JobComInvoiceLine>().Single();
				invoiceLine.JI_Weight = 98m;
				invoiceLine.JI_CountryOfOrigin = "DE";
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.JI_NetPrice = 100000m;
				invoiceLine.JI_NetWeight = 103m;
				invoiceLine.JI_CustomsSecondQuantity = 10m;
				invoiceLine.JI_CustomsThirdQuantity = 100m;
				invoiceLine.JI_CustomsFourthQuantity = 20m;
				invoiceLine.JI_BondedWhsQuantity = 200m;

				invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				var newEntryLine = entryHeader.MergedLines.AddNew();
				newEntryLine.CL_LineNumber = 2;
				invoiceLine2.JI_CL = newEntryLine.PK;
				invoiceLine2.JI_CEI = entryInstruction.PK;

				invoiceLine2.JI_PreviousEntryNumber = invoiceLine.JI_PreviousEntryNumber;
				invoiceLine2.JI_PreviousEntryLineNumber = 2;
				invoiceLine2.ZG_CountryOfSupply = "DE";
				invoiceLine.JI_BondedWhsQuantity -= 1;
				invoiceLine2.JI_BondedWhsQuantity = 1;
				invoiceLine2.JI_BondedWhsUnitQty = "KG";
				invoiceLine2.JI_CEI = invoiceLine.JI_CEI;
				invoiceLine2.JI_PartNo = invoiceLine.JI_PartNo;
				invoiceLine2.JI_InvoiceQuantity = 1;
				invoiceLine2.JI_InvoiceUQ = "NO";
				invoiceLine2.JI_CustomsUnitQty = "KG";
				invoiceLine2.JI_CustomsQuantity = 1 * 10m;
				invoiceLine2.JI_LinePrice = 101m;
				invoiceLine2.JI_NetPrice = 102;

				invoiceLine2.JI_Procedure = invoiceLine.JI_Procedure;

				invoiceLineCharge = invoiceLine.Charges.AddNew();
				invoiceLineCharge.J7_Amount = 10m;
				invoiceLineCharge.J7_ChargeType = "001";
				invoiceLineCharge.J7_RX_NKCurrency = "EUR";
				invoiceLineCharge.J7_IsDutiable = false;
				invoiceLineCharge.J7_IsGSTApplicable = false;
				invoiceLineCharge.J7_IsIncludedInITOT = false;
				invoiceLineCharge.J7_IsStatisticalValueApplicable = false;

				invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
				invoiceLineSupportingDocument.CSI_Code = "N380";
				invoiceLineSupportingDocument.CSI_ReferenceNumber = "REF123";
				invoiceLineSupportingDocument.CSI_DateOfIssue = ZDateTime.Today;
				invoiceLineSupportingDocument.CSI_Status = "N";
				invoiceLineSupportingDocument.CSI_Quantity = 10m;
				invoiceLineSupportingDocument.CSI_UnitOfQuantity = "KG";

				Factory.Save();

				invoiceLineApportionedCharge = invoiceLine.ApportionedCharges.AddNew();
				invoiceLineApportionedCharge.J7_Amount = 10m;
				invoiceLineApportionedCharge.J7_ChargeType = "002";
				invoiceLineApportionedCharge.J7_RX_NKCurrency = "EUR";
				invoiceLineApportionedCharge.J7_IsDutiable = false;
				invoiceLineApportionedCharge.J7_IsGSTApplicable = false;
				invoiceLineApportionedCharge.J7_IsIncludedInITOT = false;
				invoiceLineApportionedCharge.J7_IsStatisticalValueApplicable = false;

				Factory.Save();

				entryHeader.PublishShipmentForWHSInward(false);
				entryHeader.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

				Factory.Save();
			}
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine invoiceLine2;
		SupportingDocument invoiceHeaderSupportingDocument;
		SupportingDocument invoiceLineSupportingDocument;
		InvoiceLineCharge invoiceLineCharge;
		InvoiceLineApportionCharge invoiceLineApportionedCharge;

		CusEntryInstruction entryInstruction;

		bool ShouldUpdateBWH => entryInstruction.BondedWhsFieldsChangedAndShouldUpdate();
	}
}
