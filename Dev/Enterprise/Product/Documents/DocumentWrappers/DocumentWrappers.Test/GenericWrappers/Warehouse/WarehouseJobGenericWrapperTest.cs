using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class WarehouseJobGenericWrapperTest : GenericWrapperTest
	{
		#region TestNew

		public void TestNew()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper1 = WarehouseJobGenericWrapper.New(order, Factory);
			AssertEquals(typeof(WarehouseOrderWrapper), wrapper1.GetType());

			var workOrder = Factory.New<WhsWorkOrder>();
			var wrapper2 = WarehouseJobGenericWrapper.New(workOrder, Factory);
			AssertEquals(typeof(WarehouseWorkOrderWrapper), wrapper2.GetType());

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			var wrapper3 = WarehouseJobGenericWrapper.New(dynamicWorkOrder, Factory);
			AssertEquals(typeof(WarehouseDynamicWorkOrderWrapper), wrapper3.GetType());

			var pick = Factory.New<WhsPick>();
			var wrapper4 = WarehouseJobGenericWrapper.New(pick, Factory);
			AssertNull(wrapper4);
		}

		#endregion

		#region LinesCollections

		#region TestPackingLines

		public void TestPackingLines()
		{
			TestPackingLinesCore();
		}

		protected virtual void TestPackingLinesCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.PackingLines.Count);
		}

		#endregion

		#region TestBillOfLadingPackingLines

		public void TestBillOfLadingPackingLines()
		{
			TestBillOfLadingPackingLinesCore();
		}

		protected virtual void TestBillOfLadingPackingLinesCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.BillOfLadingPackingLines.Count);
		}

		#endregion

		#region TestBillOfLadingPackingLinesUS

		public void TestBillOfLadingPackingLinesUS()
		{
			TestBillOfLadingPackingLinesUSCore();
		}

		protected virtual void TestBillOfLadingPackingLinesUSCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.BillOfLadingPackingLinesUS.Count);
		}

		#endregion

		#region TestPackageLabels

		public void TestPackageLabels()
		{
			TestPackageLabelsCore();
		}

		protected virtual void TestPackageLabelsCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.PackageLabels.Count);
		}

		#endregion

		#region TestPackageLabelsForBOM

		public void TestPackageLabelsForBOM()
		{
			TestPackageLabelsForBOMCore();
		}

		protected virtual void TestPackageLabelsForBOMCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.PackageLabelsForBOM.Count);
		}

		#endregion

		#region TestPackages

		public void TestPackages()
		{
			TestPackagesCore();
		}

		protected virtual void TestPackagesCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.Packages.Count);
		}

		#endregion

		#region TestJobLines

		public void TestJobLines()
		{
			TestJobLinesCore();
		}

		protected virtual void TestJobLinesCore()
		{
			AssertEquals(typeof(WarehouseEmptyWrapperCollection), WarehouseJobGenericWrapper.JobLines.GetType());
			AssertEquals(0, WarehouseJobGenericWrapper.JobLines.Count);
		}

		#endregion

		#region TestJobs

		public void TestJobs()
		{
			TestJobsCore();
		}

		protected virtual void TestJobsCore()
		{
			AssertEquals(typeof(WarehouseJobEmptyWrapperCollection), WarehouseJobGenericWrapper.Jobs.GetType());
			AssertEquals(0, WarehouseJobGenericWrapper.Jobs.Count);
		}

		#endregion

		#region TestPickingLines

		public void TestPickingLines()
		{
			TestPickingLinesCore();
		}

		protected virtual void TestPickingLinesCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.PickingLines.Count);
		}

		#endregion

		#region TestContainers

		public void TestContainers()
		{
			TestContainersCore();
		}

		protected virtual void TestContainersCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.Containers.Count);
		}

		#endregion

		#region TestJobChargeLines

		public void TestJobChargeLines()
		{
			TestJobChargeLinesCore();
		}

		protected virtual void TestJobChargeLinesCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.JobChargeLines.Count);
		}

		#endregion

		#region  TestPalletizedInventory

		public void TestPalletizedInventory()
		{
			TestPalletizedInventoryCore();
		}

		protected virtual void TestPalletizedInventoryCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.PalletizedInventory.Count);
		}

		#endregion

		#region TestOrders

		public void TestOrders()
		{
			TestOrdersCore();
		}

		protected virtual void TestOrdersCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Orders);
		}

		#endregion

		#region TestOrderLines

		public void TestOrderLines()
		{
			TestOrderLinesCore();
		}

		protected virtual void TestOrderLinesCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.OrderLines.Count);
		}

		#endregion

		#region TestRolledUpLinesForOrderCopy

		public void TestRolledUpLinesForOrderCopy()
		{
			TestRolledUpLinesForOrderCopyCore();
		}

		protected virtual void TestRolledUpLinesForOrderCopyCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.RolledUpLinesForOrderCopy.Count);
		}

		#endregion

		#region TestServices

		public void TestServices()
		{
			TestServicesCore();
		}

		protected virtual void TestServicesCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.Services.Count);
		}

		#endregion

		#region TestDispatchLoadLists

		public void TestDispatchLoadLists()
		{
			TestDispatchLoadListsCore();
		}

		protected virtual void TestDispatchLoadListsCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.DispatchLoadLists.Count);
		}

		#endregion

		#region TestIsAuthorizedForDispatch

		public void TestIsAuthorizedForDispatch()
		{
			TestIsAuthorizedForDispatchCore();
		}

		protected virtual void TestIsAuthorizedForDispatchCore()
		{
			AssertEquals(ZBool.False, WarehouseJobGenericWrapper.IsAuthorizedForDispatch);
		}

		#endregion

		#region TestReceiveTransportationUnits

		public void TestReceiveTransportationUnits()
		{
			TestReceiveTransportationUnitsCore();
		}

		protected virtual void TestReceiveTransportationUnitsCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.ReceiveTransportationUnits.Count);
		}

		#endregion

		#region TestDispatchTransportationUnits

		public void TestDispatchTransportationUnits()
		{
			TestDispatchTransportationUnitsCore();
		}

		protected virtual void TestDispatchTransportationUnitsCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.DispatchTransportationUnits.Count);
		}

		#endregion

		#region TestJobLinesVariances

		public void TestJobLinesVariances()
		{
			TestJobLinesVariancesCore();
		}

		protected virtual void TestJobLinesVariancesCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.JobLinesVariances.Count);
		}

		#endregion

		#endregion

		#region Properties

		#region TestArrivalDate

		public void TestArrivalDate()
		{
			TestArrivalDateCore();
		}

		protected virtual void TestArrivalDateCore()
		{
			AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.ArrivalDate);
		}

		#endregion

		#region TestBookingDate

		public void TestBookingDate()
		{
			TestBookingDateCore();
		}

		protected virtual void TestBookingDateCore()
		{
			AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.BookingDate);
		}

		#endregion

		#region TestAccountCode

		public void TestAccountCode()
		{
			TestAccountCodeCore();
		}

		protected virtual void TestAccountCodeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.AccountCode);
		}

		#endregion

		#region TestAddresses

		#region TestConsigneeAddress

		public void TestConsigneeAddress()
		{
			TestConsigneeAddressCore();
		}

		protected virtual void TestConsigneeAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.ConsigneeAddress);
		}

		#endregion

		#region TestGoodsBillToAddress

		public void TestGoodsBillToAddress()
		{
			TestGoodsBillToAddressCore();
		}

		protected virtual void TestGoodsBillToAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.GoodsBillToAddress);
		}

		#endregion

		#region TestSOPConsigneeAddress

		public void TestSOPConsigneeAddress()
		{
			TestSOPConsigneeAddressCore();
		}

		protected virtual void TestSOPConsigneeAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.SOPConsigneeAddress);
		}

		#endregion

		#region TestSupplierDocAddress

		public void TestSupplierDocAddress()
		{
			TestSupplierDocAddressCore();
		}

		protected virtual void TestSupplierDocAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.SupplierDocAddress);
		}

		#endregion

		#region TestDropOffAddress

		public void TestDropOffAddress()
		{
			TestDropOffAddressCore();
		}

		protected virtual void TestDropOffAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.DropOffAddress);
		}

		#endregion

		#region TestPickUpAddress

		public void TestPickUpAddress()
		{
			TestPickUpAddressCore();
		}

		protected virtual void TestPickUpAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.PickUpAddress);
		}

		#endregion

		#region TestTransportBillToAddress

		public void TestTransportBillToAddress()
		{
			TestTransportBillToAddressCore();
		}

		protected virtual void TestTransportBillToAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.TransportBillToAddress);
		}

		#endregion

		#region TestDistributionCentreAddress

		public void TestDistributionCentreAddress()
		{
			TestDistributionCentreAddressCore();
		}

		protected virtual void TestDistributionCentreAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.DistributionCentreAddress);
		}

		#endregion

		#region TestTransportCoAddress

		public void TestTransportCoAddress()
		{
			TestTransportCoAddressCore();
		}

		protected virtual void TestTransportCoAddressCore()
		{
			AssertNull(WarehouseJobGenericWrapper.TransportCoAddress);
		}

		#endregion

		#endregion

		#region TestBOLNumber

		public void TestBOLNumber()
		{
			TestBOLNumberCore();
		}

		protected virtual void TestBOLNumberCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.BOLNumber);
		}

		#endregion

		#region TestCartageAdviceClosingText

		public void TestCartageAdviceClosingText()
		{
			TestCartageAdviceClosingTextCore();
		}

		protected virtual void TestCartageAdviceClosingTextCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.CartageAdviceClosingText);
		}

		#endregion

		#region TestCartageAdviceOpeningText

		public void TestCartageAdviceOpeningText()
		{
			TestCartageAdviceOpeningTextCore();
		}

		protected virtual void TestCartageAdviceOpeningTextCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.CartageAdviceOpeningText);
		}

		#endregion

		#region TestCartageDropMode

		public void TestCartageDropMode()
		{
			TestCartageDropModeCore();
		}

		protected virtual void TestCartageDropModeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.CartageDropMode);
		}

		#endregion

		#region TestClient

		public void TestClient()
		{
			TestClientCore();
		}

		protected virtual void TestClientCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Client);
		}

		#endregion

		#region TestClientRequestedBillToParty

		public void TestClientRequestedBillToParty()
		{
			TestClientRequestedBillToPartyCore();
		}

		protected virtual void TestClientRequestedBillToPartyCore()
		{
			AssertNull(WarehouseJobGenericWrapper.ClientRequestedBillToParty);
		}

		#endregion

		#region TestCODAmount

		public void TestCODAmount()
		{
			TestCODAmountCore();
		}

		protected virtual void TestCODAmountCore()
		{
			AssertNull(WarehouseJobGenericWrapper.CODAmount);
		}

		#endregion

		#region TestCODType

		public void TestCODType()
		{
			TestCODTypeCore();
		}

		protected virtual void TestCODTypeCore()
		{
			AssertNull(WarehouseJobGenericWrapper.CODType);
		}

		#endregion

		#region TestConfirmationInstructions

		public void TestConfirmationInstructions()
		{
			TestConfirmationInstructionsCore();
		}

		protected virtual void TestConfirmationInstructionsCore()
		{
			AssertEquals("", WarehouseJobGenericWrapper.ConfirmationInstructions.Value);
			AssertEquals("", WarehouseJobGenericWrapper.ConfirmationInstructions.Label);
		}

		#endregion

		#region SupplierBuyerLink

		public void TestSupplierBuyerLink()
		{
			TestSupplierBuyerLinkCore();
		}

		protected virtual void TestSupplierBuyerLinkCore()
		{
			AssertNull(WarehouseJobGenericWrapper.SupplierBuyerLink);
		}

		#endregion

		#region TestConsignee

		[SetOrgAllowMixedCase(true)]
		public void TestConsignee()
		{
			TestConsigneeCore();
		}

		protected virtual void TestConsigneeCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Consignee);
		}

		#endregion

		#region TestConsolidatedInvoiceRef

		public void TestConsolidatedInvoiceRef()
		{
			TestConsolidatedInvoiceRefCore();
		}

		protected virtual void TestConsolidatedInvoiceRefCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ConsolidatedInvoiceRef);
		}

		#endregion

		#region TestContainerNumberAndTypeLine

		public void TestContainerNumberAndTypeLine()
		{
			TestContainerNumberAndTypeLineCore();
		}

		protected virtual void TestContainerNumberAndTypeLineCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ContainerNumberAndTypeLine);
		}

		#endregion

		#region TestCubicSent

		public void TestCubicSent()
		{
			TestCubicSentCore();
		}

		protected virtual void TestCubicSentCore()
		{
			AssertNull(WarehouseJobGenericWrapper.CubicSent);
		}

		#endregion

		#region TestCurrencyCode

		public void TestCurrencyCode()
		{
			TestCurrencyCodeCore();
		}

		protected virtual void TestCurrencyCodeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.CurrencyCode);
		}

		#endregion

		#region TestCustomerReference

		public void TestCustomerReference()
		{
			TestCustomerReferenceCore();
		}

		protected virtual void TestCustomerReferenceCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.CustomerReference.IsEmpty);
		}

		#endregion

		#region TestCustomsStatus

		public void TestCustomsStatus()
		{
			TestCustomsStatusCore();
		}

		protected virtual void TestCustomsStatusCore()
		{
			AssertNull(WarehouseJobGenericWrapper.CustomsStatus);
		}

		#endregion

		#region TestCP_IssueNo

		public void TestCP_IssueNo()
		{
			TestCP_IssueNoCore();
		}

		protected virtual void TestCP_IssueNoCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.CP_IssueNo);
		}

		#endregion

		#region TestACSEstCode

		public void TestACSEstCode()
		{
			TestACSEstCodeCore();
		}

		protected virtual void TestACSEstCodeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ACSEstCode);
		}

		#endregion

		#region TestATOEstCode

		public void TestATOEstCode()
		{
			TestATOEstCodeCore();
		}

		protected virtual void TestATOEstCodeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ATOEstCode);
		}

		#endregion

		#region TestClientGCR

		public void TestClientGCR()
		{
			TestClientGCRCore();
		}

		protected virtual void TestClientGCRCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ClientGCR);
		}

		#endregion

		#region TestClientCPC

		public void TestClientCPC()
		{
			TestClientCPCCore();
		}

		protected virtual void TestClientCPCCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ClientCPC);
		}

		#endregion

		#region TestWarehouseCCPCode

		public void TestWarehouseCCPCode()
		{
			TestWarehouseCCPCodeCore();
		}

		protected virtual void TestWarehouseCCPCodeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ClientCPC);
		}

		#endregion

		#region TestPackingSlipTitle

		public void TestPackingSlipTitle()
		{
			TestPackingSlipTitleCore();
		}

		protected virtual void TestPackingSlipTitleCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.PackingSlipTitle);
		}

		#endregion

		#region TestABN

		public void TestABN()
		{
			TestABNCore();
		}

		protected virtual void TestABNCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ABN);
		}

		#endregion

		#region TestDebtorCodeAndName

		public void TestDebtorCodeAndName()
		{
			TestDebtorCodeAndNameCore();
		}

		protected virtual void TestDebtorCodeAndNameCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.DebtorCodeAndName);
		}

		#endregion

		#region TestDestination

		public void TestDestination()
		{
			TestDestinationCore();
		}

		protected virtual void TestDestinationCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Destination);
		}

		#endregion

		#region TestDepartmentName

		public void TestDepartmentName()
		{
			TestDepartmentNameCore();
		}

		protected virtual void TestDepartmentNameCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.DepartmentName);
		}

		#endregion

		#region TestDepartmentNumber

		public void TestDepartmentNumber()
		{
			TestDepartmentNumberCore();
		}

		protected virtual void TestDepartmentNumberCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.DepartmentNumber);
		}

		#endregion

		#region TestOrderTypeCodeFirst2Characters

		public void TestOrderTypeCodeFirst2Characters()
		{
			TestOrderTypeCodeFirst2CharactersCore();
		}

		protected virtual void TestOrderTypeCodeFirst2CharactersCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.OrderTypeCodeFirst2Characters);
		}

		#endregion

		#region TestOrderTypeCodeLast4Characters

		public void TestOrderTypeCodeLast4Characters()
		{
			TestOrderTypeCodeLast4CharactersCore();
		}

		protected virtual void TestOrderTypeCodeLast4CharactersCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.OrderTypeCodeLast4Characters);
		}

		#endregion

		#region TestEventTypeCode

		public void TestEventTypeCode()
		{
			TestEventTypeCodeCore();
		}

		protected virtual void TestEventTypeCodeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.EventTypeCode);
		}

		#endregion

		#region TestDocTypeCode

		public void TestDocTypeCode()
		{
			AssertEquals("Empty Doc Type Code", ZString.Empty, ((IDocTypeCode)WarehouseJobGenericWrapper).DocTypeCode);

			((IDocTypeCode)WarehouseJobGenericWrapper).DocTypeCode = "abc";
			AssertEquals("Doc Type Code set", "abc", ((IDocTypeCode)WarehouseJobGenericWrapper).DocTypeCode);
		}

		#endregion

		#region TestDocumentTitle

		public void TestDocumentTitle()
		{
			TestDocumentTitleCore();
		}

		protected virtual void TestDocumentTitleCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.DocumentTitle);
		}

		#endregion

		#region TestTransportZone

		public void TestTransportZone()
		{
			TestTransportZoneCore();
		}

		protected virtual void TestTransportZoneCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.TransportZone);
		}

		#endregion

		#region TestDockDoor

		public void TestDockDoor()
		{
			TestDockDoorCore();
		}

		protected virtual void TestDockDoorCore()
		{
			AssertEquals("Base DockDoor returns empty string.", ZString.Empty, WarehouseJobGenericWrapper.DockDoor);
		}

		#endregion

		#region TestTransportationUnit

		public void TestTransportationUnit()
		{
			TestTransportationUnitCore();
		}

		protected virtual void TestTransportationUnitCore()
		{
			AssertEquals("Base TransportationUnit returns null.", null, WarehouseJobGenericWrapper.TransportationUnit);
		}

		#endregion

		#region TestDropMode

		public void TestDropMode()
		{
			TestDropModeCore();
		}

		protected virtual void TestDropModeCore()
		{
			AssertNull(WarehouseJobGenericWrapper.DropMode);
		}

		#endregion

		#region TestEnableDangerousGoodsDetails

		public void TestEnableDangerousGoodsDetails()
		{
			TestEnableDangerousGoodsDetailsCore();
		}

		protected virtual void TestEnableDangerousGoodsDetailsCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.EnableDangerousGoodsDetails);
		}

		#endregion

		#region TestEnableExtendedLinePrice

		public void TestEnableExtendedLinePrice()
		{
			TestEnableExtendedLinePriceCore();
		}

		protected virtual void TestEnableExtendedLinePriceCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.EnableExtendedLinePrice);
		}

		#endregion

		#region TestFromDate

		public void TestFromDate()
		{
			TestFromDateCore();
		}

		protected virtual void TestFromDateCore()
		{
			AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.FromDate);
		}

		#endregion

		#region TestFulfillRule

		public void TestFulfillRule()
		{
			TestFulfillRuleCore();
		}

		protected virtual void TestFulfillRuleCore()
		{
			AssertNull(WarehouseJobGenericWrapper.FulfillRule);
		}

		#endregion

		#region TestHandlingInstructions

		public void TestHandlingInstructions()
		{
			TestHandlingInstructionsCore();
		}

		protected virtual void TestHandlingInstructionsCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.HandlingInstructions.IsEmpty);
		}

		#endregion

		#region TestHasMulipleStockKeepingUnits

		public void TestHasMultipleStockKeepingUnits()
		{
			TestHasMultipleStockKeepingUnitsCore();
		}

		public void TestHasMultipleStockKeepingUnits_SomeSupplierPartIsNull()
		{
			AssertNoExceptionThrown("should not throw any exception", delegate
			{ TestHasMultipleStockKeepingUnitsCore_SomeSupplierPartIsNull(); });
		}

		protected virtual void TestHasMultipleStockKeepingUnitsCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.HasMultipleStockKeepingUnits);
		}

		protected virtual void TestHasMultipleStockKeepingUnitsCore_SomeSupplierPartIsNull()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.HasMultipleStockKeepingUnits);
		}

		#endregion

		#region TestHasMulipleWeightsUnits

		public void TestHasMultipleWeightUnits()
		{
			TestHasMultipleWeightUnitsCore();
		}

		public void TestHasMultipleWeightUnits_SomeSupplierPartIsNull()
		{
			AssertNoExceptionThrown("should not throw any exception", delegate
			{ TestHasMultipleWeightUnitsCore_SomeSupplierPartIsNull(); });
		}

		protected virtual void TestHasMultipleWeightUnitsCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.HasMultipleWeightUnits);
		}

		protected virtual void TestHasMultipleWeightUnitsCore_SomeSupplierPartIsNull()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.HasMultipleWeightUnits);
		}

		#endregion

		#region TestHasMultopleVolumeUnits

		public void TestHasMultipleVolumeUnits()
		{
			TestHasMultipleVolumeUnitsCore();
		}

		protected virtual void TestHasMultipleVolumeUnitsCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.HasMultipleVolumeUnits);
		}

		#endregion

		#region TestIncoTerm

		public void TestIncoTerm()
		{
			TestIncoTermCore();
		}

		protected virtual void TestIncoTermCore()
		{
			AssertNull(WarehouseJobGenericWrapper.IncoTerm);
		}

		#endregion

		#region TestInsurance

		public void TestInsurance()
		{
			TestInsuranceCore();
		}

		protected virtual void TestInsuranceCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Insurance);
		}

		#endregion

		#region TestInvoiceNumber

		public void TestInvoiceNumber()
		{
			TestInvoiceNumberCore();
		}

		protected virtual void TestInvoiceNumberCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.InvoiceNumber);
		}

		#endregion

		#region TestIsCustomsTransactions

		public void TestIsCustomsTransactions()
		{
			TestIsCustomsTransactionsCore();
		}

		protected virtual void TestIsCustomsTransactionsCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.IsCustomsTransaction);
		}

		#endregion

		#region TestIsPickByBiggestEnabled

		public void TestIsPickByBiggestEnabled()
		{
			var branch = Factory.New<Enterprise.MasterFiles.Business.GlbBranch>();
			var company = Factory.New<Enterprise.MasterFiles.Business.GlbCompany>();
			branch.GB_GC = company.PK;
			if ((WarehouseJobGenericWrapper.Warehouse != null && WarehouseJobGenericWrapper.Warehouse.WrappedObject != null))
			{
				var warehouse = (WhsWarehouse)WarehouseJobGenericWrapper.Warehouse.WrappedObject;
				warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
			}
			var originalRegistryValue = WarehouseDataRegistry.Instance.PickByBiggestType.GetFallBackValueAtAllLevels(company.PK.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			try
			{
				AssertEquals(originalRegistryValue, WarehouseJobGenericWrapper.IsPickByBiggestEnabled);
				if (WarehouseJobGenericWrapper.Warehouse != null && WarehouseJobGenericWrapper.Warehouse.WrappedObject != null)
				{
					// registry setting will only have effect when Warehouse is not null
					WarehouseDataRegistry.Instance.PickByBiggestType.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, !originalRegistryValue);
					AssertEquals(!originalRegistryValue, WarehouseJobGenericWrapper.IsPickByBiggestEnabled);
				}
			}
			finally
			{
				WarehouseDataRegistry.Instance.PickByBiggestType.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, originalRegistryValue);
			}
		}

		#endregion

		#region TestIsWorkOrder

		public void TestIsWorkOrder()
		{
			TestIsWorkOrderCore();
		}

		protected virtual void TestIsWorkOrderCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.IsWorkOrder);
		}

		#endregion

		#region TestIsAuthorisedToLeave

		public void TestIsAuthorisedToLeave()
		{
			TestIsAuthorisedToLeaveCore();
		}

		protected virtual void TestIsAuthorisedToLeaveCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.IsAuthorisedToLeave);
		}

		#endregion

		#region TestJobClient

		public void TestJobClient()
		{
			TestJobClientCore();
		}

		protected virtual void TestJobClientCore()
		{
			AssertNull(WarehouseJobGenericWrapper.JobClient);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			TestJobNumberCore();
		}

		protected virtual void TestJobNumberCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.JobNumber);
		}

		#endregion

		#region TestJobNumberHeading

		public void TestJobNumberHeading()
		{
			TestJobNumberHeadingCore();
		}

		protected virtual void TestJobNumberHeadingCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.JobNumberHeading);
		}

		#endregion

		#region TestJobNumber

		public void TestJobType()
		{
			TestJobTypeCore();
		}

		protected virtual void TestJobTypeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.JobType);
		}

		#endregion

		#region TestPalletsSent

		public void TestPalletsSent()
		{
			TestPalletsSentCore();
		}

		protected virtual void TestPalletsSentCore()
		{
			AssertEquals(ZShort.Zero, WarehouseJobGenericWrapper.PalletsSent);
		}

		#endregion

		#region TestPackagesSent

		public void TestPackagesSent()
		{
			TestPackagesSentCore();
		}

		protected virtual void TestPackagesSentCore()
		{
			AssertNull(WarehouseJobGenericWrapper.PackagesSent);
		}

		#endregion

		#region TestPickingInstructions

		public void TestPickingInstructions()
		{
			TestPickingInstructionsCore();
		}

		protected virtual void TestPickingInstructionsCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.PickingInstructions.IsEmpty);
		}

		#endregion

		#region TestProductLinesCount

		public void TestProductLinesCount()
		{
			TestProductLinesCountCore();
		}

		protected virtual void TestProductLinesCountCore()
		{
			AssertEquals(0, WarehouseJobGenericWrapper.ProductLinesCount);
		}

		#endregion

		#region TestPickNo

		public void TestPickNo()
		{
			TestPickNoCore();
		}

		protected virtual void TestPickNoCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.PickNo.IsEmpty);
		}

		#endregion

		#region TestPickNumberReference

		public void TestPickNumberReference()
		{
			TestPickNumberReferenceCore();
		}

		protected virtual void TestPickNumberReferenceCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.PickNumberReference.IsEmpty);
		}

		#endregion

		#region TestPickOption

		public void TestPickOption()
		{
			TestPickOptionCore();
		}

		protected virtual void TestPickOptionCore()
		{
			AssertNull(WarehouseJobGenericWrapper.PickOption);
		}

		#endregion

		#region TestPrimaryBarcode

		public void TestPrimaryBarcode()
		{
			TestPrimaryBarcodeCore();
		}

		protected virtual void TestPrimaryBarcodeCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.PrimaryBarcode.IsEmpty);
		}

		#endregion

		#region TestPrimaryBarcodeText

		public void TestPrimaryBarcodeText()
		{
			TestPrimaryBarcodeTextCore();
		}

		protected virtual void TestPrimaryBarcodeTextCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.PrimaryBarcodeText);
		}

		#endregion

		#region TestCustomerReferenceBarcode

		public void TestCustomerReferenceBarcode()
		{
			TestCustomerReferenceBarcodeCore();
		}

		protected virtual void TestCustomerReferenceBarcodeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.CustomerReferenceBarcode);
		}

		#endregion

		#region TestPrintDGDetails

		public void TestPrintDGDetails()
		{
			TestPrintDGDetailsCore();
		}

		protected virtual void TestPrintDGDetailsCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.PrintDGDetails);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			TestMasterBillCore();
		}

		protected virtual void TestMasterBillCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.MasterBill);
		}

		#endregion

		#region TestMasterBillHeading

		public void TestMasterBillHeading()
		{
			TestMasterBillHeadingCore();
		}

		protected virtual void TestMasterBillHeadingCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.MasterBillHeading);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			TestHouseBillCore();
		}

		protected virtual void TestHouseBillCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.HouseBill);
		}

		#endregion

		#region TestHouseBillHeading

		public void TestHouseBillHeading()
		{
			TestHouseBillHeadingCore();
		}

		protected virtual void TestHouseBillHeadingCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.HouseBillHeading);
		}

		#endregion

		#region TestOtherReferences

		public void TestOtherReferences()
		{
			TestOtherReferencesCore();
		}

		protected virtual void TestOtherReferencesCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.OtherReferences);
		}

		#endregion

		#region TestReferences

		public void TestReferences()
		{
			TestReferencesCore();
		}

		protected virtual void TestReferencesCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.References);
		}

		#endregion

		#region TestRefencesExtended

		public void TestReferencesExtended()
		{
			TestReferencesExtendedCore();
		}

		protected virtual void TestReferencesExtendedCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ReferencesExtended);
		}

		#endregion

		#region TestReportDescription

		public void TestReportDescription()
		{
			TestReportDescriptionCore();
		}

		protected virtual void TestReportDescriptionCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ReportDescription);
		}

		#endregion

		#region TestRequiredDate

		public void TestRequiredDate()
		{
			TestRequiredDateCore();
		}

		protected virtual void TestRequiredDateCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.RequiredDate.IsEmpty);
		}

		#endregion

		#region TestSeal

		public void TestSeal()
		{
			TestSealCore();
		}

		protected virtual void TestSealCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.Seal.IsEmpty);
		}

		#endregion

		#region TestSecondaryHeading

		public void TestSecondaryHeading()
		{
			TestSecondaryHeadingCore();
		}

		protected virtual void TestSecondaryHeadingCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.SecondaryHeading);
		}

		#endregion

		#region TestSecondaryNumber

		public void TestSecondaryNumber()
		{
			TestSecondaryNumberCore();
		}

		protected virtual void TestSecondaryNumberCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.SecondaryNumber);
		}

		#endregion

		#region TestSecondaryReference

		public void TestSecondaryReference()
		{
			TestSecondaryReferenceCore();
		}

		protected virtual void TestSecondaryReferenceCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SecondaryReference.IsEmpty);
		}

		#endregion

		#region TestHasOversAndUnders

		public void TestHasOversAndUnders()
		{
			TestHasOversAndUndersCore();
		}

		protected virtual void TestHasOversAndUndersCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.HasOversAndUnders);
		}

		#endregion

		#region TestCarrierServiceLevel

		public void TestCarrierServiceLevel()
		{
			TestCarrierServiceLevelCore();
		}

		protected virtual void TestCarrierServiceLevelCore()
		{
			AssertNull(WarehouseJobGenericWrapper.CarrierServiceLevel);
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			TestServiceLevelCore();
		}

		protected virtual void TestServiceLevelCore()
		{
			AssertNull(WarehouseJobGenericWrapper.ServiceLevel);
		}

		#endregion

		#region TestSOPConsigneeAddressLabel

		public void TestSOPConsigneeAddressLabel()
		{
			TestSOPConsigneeAddressLabelCore();
		}

		protected virtual void TestSOPConsigneeAddressLabelCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.SOPConsigneeAddressLabel);
		}

		#endregion

		#region TestSOPOrderNumber

		public void TestSOPOrderNumber()
		{
			TestSOPOrderNumberCore();
		}

		protected virtual void TestSOPOrderNumberCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SOPOrderNumber.IsEmpty);
		}

		#endregion

		#region TestSOPStagingName

		public void TestSOPStagingName()
		{
			TestSOPStagingNameCore();
		}

		protected virtual void TestSOPStagingNameCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SOPStagingAreaName.IsEmpty);
		}

		#endregion

		#region TestStagingLocationString

		public void TestStagingLocationString()
		{
			TestStagingLocationStringCore();
		}

		protected virtual void TestStagingLocationStringCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.StagingLocationString.IsEmpty);
		}

		#endregion

		#region TestUnloadCompleteTime

		[TestDate(2023, 06, 06, 06, 06, 06)]
		public void TestUnloadCompleteTime() => TestUnloadCompleteTimeCore();

		protected virtual void TestUnloadCompleteTimeCore() => AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.UnloadCompleteTime);

		#endregion

		#region TestAllowPartialLoading

		public void TestAllowPartialLoading()
		{
			TestAllowPartialLoadingCore();
		}

		protected virtual void TestAllowPartialLoadingCore()
		{
			AssertEquals(ZBool.False, WarehouseJobGenericWrapper.AllowPartialLoading);
		}

		#endregion

		#region TestSOPSpecialInstructions

		public void TestSOPSpecialInstructions()
		{
			TestSOPSpecialInstructionsCore();
		}

		protected virtual void TestSOPSpecialInstructionsCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SOPSpecialInstructions.IsEmpty);
		}

		#endregion

		#region TestSOPTransportCompany

		public void TestSOPTransportCompany()
		{
			TestSOPTransportCompanyCore();
		}

		protected virtual void TestSOPTransportCompanyCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SOPTransportCompany.IsEmpty);
		}

		#endregion

		#region TestSOPServiceLevel

		public void TestSOPCarrierServiceLevel()
		{
			TestSOPCarrierServiceLevelCore();
		}

		protected virtual void TestSOPCarrierServiceLevelCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SOPCarrierServiceLevel.IsEmpty);
		}

		#endregion

		#region TestSOPRequiredDate

		public void TestSOPRequiredDate()
		{
			TestSOPRequiredDateCore();
		}

		protected virtual void TestSOPRequiredDateCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SOPRequiredDate.IsEmpty);
		}

		#endregion

		#region TestSplitNumber

		public void TestSplitNumber()
		{
			TestSplitNumberCore();
		}

		public void TestSplitNumberCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SplitNumber.IsEmpty);
		}

		#endregion

		#region TestIsSplit

		public void TestIsSplit()
		{
			TestIsSplitCore();
		}

		protected virtual void TestIsSplitCore()
		{
			AssertEquals(ZBool.False, WarehouseJobGenericWrapper.IsSplit);
		}

		#endregion

		#region TestStagingAreaName

		public void TestStagingAreaName()
		{
			TestStagingAreaNameCore();
		}

		protected virtual void TestStagingAreaNameCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.StagingAreaName.IsEmpty);
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			TestStatusCore();
		}

		protected virtual void TestStatusCore()
		{
			Assert(WarehouseJobGenericWrapper.Status.IsEmpty);
		}

		#endregion

		#region TestSubTypeDesc

		public void TestSubTypeDesc()
		{
			TestSubTypeDescCore();
		}

		protected virtual void TestSubTypeDescCore()
		{
			Assert(WarehouseJobGenericWrapper.SubTypeDesc.IsEmpty);
		}

		#endregion

		#region TestStocktakeFields

		#region Test StocktakeNumber

		public void TestStocktakeNumber()
		{
			TestStocktakeNumberCore();
		}

		protected virtual void TestStocktakeNumberCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.StocktakeNumber.IsEmpty);
		}

		#endregion

		#region TestSelectedCycle

		public void TestSelectedCycle()
		{
			TestSelectedCycleCore();
		}

		protected virtual void TestSelectedCycleCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedCycle.IsEmpty);
		}

		#endregion

		#region TestSelectedClient

		public void TestSelectedClient()
		{
			TestSelectedClientCore();
		}

		protected virtual void TestSelectedClientCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedClient.IsEmpty);
		}

		#endregion

		#region TestSelectedSupplierPart

		public void TestSelectedSupplierPart()
		{
			TestSelectedSupplierPartCore();
		}

		protected virtual void TestSelectedSupplierPartCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedSupplierPart.IsEmpty);
		}

		#endregion

		#region TestSelectedCommodityCode

		public void TestSelectedCommodityCode()
		{
			TestSelectedCommodityCodeCore();
		}

		protected virtual void TestSelectedCommodityCodeCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedCommodityCode.IsEmpty);
		}

		#endregion

		#region TestSelectedPickMethod

		public void TestSelectedPickMethod()
		{
			TestSelectedPickMethodCore();
		}

		protected virtual void TestSelectedPickMethodCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedPickMethod.IsEmpty);
		}

		#endregion

		#region TestSelectedRow

		public void TestSelectedRow()
		{
			TestSelectedRowCore();
		}

		protected virtual void TestSelectedRowCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedRow.IsEmpty);
		}

		#endregion

		#region TestSelectedArea

		public void TestSelectedArea()
		{
			TestSelectedAreaCore();
		}

		protected virtual void TestSelectedAreaCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedArea.IsEmpty);
		}

		#endregion

		#region TestSelectedABCCategory

		public void TestSelectedABCCategory()
		{
			TestSelectedABCCategoryCore();
		}

		protected virtual void TestSelectedABCCategoryCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedABCCategory.IsEmpty);
		}

		#endregion

		#region TestSelectedLocation

		public void TestSelectedLocation()
		{
			TestSelectedLocationCore();
		}

		protected virtual void TestSelectedLocationCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedLocation.IsEmpty);
		}

		#endregion

		#region TestSelectedStocktakeType

		public void TestSelectedStocktakeType()
		{
			TestSelectedStocktakeTypeCore();
		}

		protected virtual void TestSelectedStocktakeTypeCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.SelectedStocktakeType.IsEmpty);
		}

		#endregion

		#endregion

		#region TestToDate

		public void TestToDate()
		{
			TestToDateCore();
		}

		protected virtual void TestToDateCore()
		{
			AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.ToDate);
		}

		#endregion

		#region TestTotalExtendedLinePrice

		public void TestTotalExtendedLinePrice()
		{
			TestTotalExtendedLinePriceCore();
		}

		protected virtual void TestTotalExtendedLinePriceCore()
		{
			AssertNull(WarehouseJobGenericWrapper.TotalExtendedLinePrice);
		}

		#endregion

		#region TestPrintPageWithContainerNumber

		public void TestPrintPageWithContainerNumber()
		{
			TestPrintPageWithContainerNumberCore();
		}

		protected virtual void TestPrintPageWithContainerNumberCore()
		{
			AssertEquals(ZBool.False, WarehouseJobGenericWrapper.PrintPageWithContainerNumber);
		}

		#endregion

		#region TestEmergencyContactMessageString

		public void TestEmergencyContactMessageString()
		{
			TestEmergencyContactMessageStringCore();
		}

		protected virtual void TestEmergencyContactMessageStringCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.EmergencyContactMessageString);
		}

		#endregion

		#region TestCurrencySymbol

		public void TestCurrencySymbol()
		{
			TestCurrencySymbolCore();
		}

		protected virtual void TestCurrencySymbolCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.CurrencySymbol);
		}

		#endregion

		#region TestTotalExtendedLinePriceWithSymbol

		public void TestTotalExtendedLinePriceWithSymbol()
		{
			TestTotalExtendedLinePriceWithSymbolCore();
		}

		protected virtual void TestTotalExtendedLinePriceWithSymbolCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.TotalExtendedLinePriceWithSymbol);
		}

		#endregion

		#region TestTotalTopLevelUnitsMet_TotalTopLevelUnitsOrdered

		public void TestTotalTopLevelUnitsMet_TotalTopLevelUnitsOrdered()
		{
			TestTotalTopLevelUnitsMet_TotalTopLevelUnitsOrderedCore();
		}

		protected virtual void TestTotalTopLevelUnitsMet_TotalTopLevelUnitsOrderedCore()
		{
			AssertEquals(ZDecimal.Zero, WarehouseJobGenericWrapper.TotalTopLevelUnitsMet);
			AssertEquals(ZDecimal.Zero, WarehouseJobGenericWrapper.TotalTopLevelUnitsOrdered);
		}

		#endregion

		#region TestTotalGroupedLineUnitsMet

		public void TestTotalGroupedLineUnitsMet()
		{
			AssertEquals(ZDecimal.Zero, WarehouseJobGenericWrapper.TotalGroupedLineUnitsMet);
		}

		protected virtual void TestTotalGroupedLineUnitsMetCore()
		{
			AssertEquals(ZDecimal.Zero, WarehouseJobGenericWrapper.TotalGroupedLineUnitsMet);
		}

		#endregion

		#region TestTotalNumberOfLabels

		public void TestTotalNumberOfLabels()
		{
			TestTotalNumberOfLabelsCore();
		}

		protected virtual void TestTotalNumberOfLabelsCore()
		{
			AssertEquals(ZInt.Zero, WarehouseJobGenericWrapper.TotalNumberOfLabels);
		}

		#endregion

		#region TestTotalNumberOfPackageLabels

		public void TestTotalNumberOfPackageLabels()
		{
			TestTotalNumberOfPackageLabelsCore();
		}

		protected virtual void TestTotalNumberOfPackageLabelsCore()
		{
			AssertEquals(ZInt.Zero, WarehouseJobGenericWrapper.TotalNumberOfPackageLabels);
		}

		#endregion

		#region TestTransportCompany

		public virtual void TestTransportCompany()
		{
			TestTransportCompanyCore();
		}

		protected virtual void TestTransportCompanyCore()
		{
			AssertNull(WarehouseJobGenericWrapper.TransportCompany);
		}

		#endregion

		#region TestTransportReference

		public void TestTransportReference()
		{
			TestTransportReferenceCore();
		}

		protected virtual void TestTransportReferenceCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.TransportReference.IsEmpty);
		}

		#endregion

		#region TestUnitsSent

		public void TestUnitsSent()
		{
			TestUnitsSentCore();
		}

		protected virtual void TestUnitsSentCore()
		{
			AssertEquals(0m, WarehouseJobGenericWrapper.UnitsSent);
		}

		#endregion

		#region TestVehicleReference

		public void TestVehicleReference()
		{
			TestVehicleReferenceCore();
		}

		protected virtual void TestVehicleReferenceCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.VehicleReference.IsEmpty);
		}

		#endregion

		#region TestVendorID

		public void TestVendorID()
		{
			TestVendorIDCore();
		}

		protected virtual void TestVendorIDCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.VendorID);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			TestWarehouseCore();
		}

		protected virtual void TestWarehouseCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Warehouse);
		}

		#endregion

		#region  TestWarehouseCartageCoordinatorName

		public void TestWarehouseCartageCoordinatorName()
		{
			TestWarehouseCartageCoordinatorNameCore();
		}

		protected virtual void TestWarehouseCartageCoordinatorNameCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.WarehouseCartageCoordinatorName);
		}

		#endregion

		#region  TestWarehouseCartageCoordinatorPhone

		public void TestWarehouseCartageCoordinatorPhone()
		{
			TestWarehouseCartageCoordinatorPhoneCore();
		}

		protected virtual void TestWarehouseCartageCoordinatorPhoneCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.WarehouseCartageCoordinatorPhone);
		}

		#endregion

		#region  TestWarehouseCompanyLogo

		public void TestWarehouseCompanyLogo()
		{
			TestWarehouseCompanyLogoCore();
		}

		protected virtual void TestWarehouseCompanyLogoCore()
		{
			AssertNull(WarehouseJobGenericWrapper.WarehouseCompanyLogo);
		}

		#endregion

		#region  TestWarehouseName

		public void TestWarehouseName()
		{
			TestWarehouseNameCore();
		}

		protected virtual void TestWarehouseNameCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.WarehouseName.IsEmpty);
		}

		#endregion

		#region  TestWarehouseNameAndAddress

		public void TestWarehouseNameAndAddress()
		{
			TestWarehouseNameAndAddressCore();
		}

		protected virtual void TestWarehouseNameAndAddressCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.WarehouseNameAndAddress.IsEmpty);
		}

		#endregion

		#region  TestWarehousePhoneAndFax

		public void TestWarehousePhoneAndFax()
		{
			TestWarehousePhoneAndFaxCore();
		}

		protected virtual void TestWarehousePhoneAndFaxCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.WarehousePhoneAndFax);
		}

		#endregion

		#region TestDocketStatus

		public void TestDocketStatus()
		{
			TestDocketStatusCore();
		}

		protected virtual void TestDocketStatusCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.DocketStatus);
		}

		#region TestDocketType

		#endregion

		public void TestDocketType()
		{
			TestDocketTypeCore();
		}

		protected virtual void TestDocketTypeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.DocketType);
		}

		#endregion

		#region TestDocketSubType

		public void TestDocketSubType()
		{
			TestDocketSubTypeCore();
		}

		protected virtual void TestDocketSubTypeCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.DocketSubType);
		}

		#endregion

		#region TestTotalCubic

		public void TestTotalCubic()
		{
			TestTotalCubicCore();
		}

		protected virtual void TestTotalCubicCore()
		{
			AssertEquals(ZDecimal.Zero, WarehouseJobGenericWrapper.TotalCubic);
		}

		#endregion

		#region TestTotalWeight

		public void TestTotalWeight()
		{
			TestTotalWeightCore();
		}

		protected virtual void TestTotalWeightCore()
		{
			AssertEquals(ZDecimal.Zero, WarehouseJobGenericWrapper.TotalWeight);
		}

		#endregion

		#region TestTotalUnits

		public void TestTotalUnits()
		{
			TestTotalUnitsCore();
		}

		protected virtual void TestTotalUnitsCore()
		{
			AssertEquals(ZDecimal.Zero, WarehouseJobGenericWrapper.TotalUnits);
		}

		#endregion

		#region TestTotalPallets

		public void TestTotalPallets()
		{
			TestTotalPalletsCore();
		}

		protected virtual void TestTotalPalletsCore()
		{
			AssertEquals(ZShort.Zero, WarehouseJobGenericWrapper.TotalPallets);
		}

		#endregion

		#region TestTotalInnerPackLines

		public void TestTotalInnerPackLines()
		{
			TestTotalInnerPackLinesCore();
		}

		protected virtual void TestTotalInnerPackLinesCore()
		{
			AssertEquals(ZShort.Zero, WarehouseJobGenericWrapper.TotalInnerPackLines);
		}

		#endregion

		#region TestTotalInners

		public void TestTotalInners()
		{
			TestTotalInnersCore();
		}

		protected virtual void TestTotalInnersCore()
		{
			AssertEquals(ZShort.Zero, WarehouseJobGenericWrapper.TotalInners);
		}

		#endregion

		#region TestTotalOverpacks

		public void TestTotalOverpacks()
		{
			TestTotalOverpacksCore();
		}

		protected virtual void TestTotalOverpacksCore()
		{
			AssertEquals(ZShort.Zero, WarehouseJobGenericWrapper.TotalOverpacks);
		}

		#endregion

		#region TestBranch

		public void TestBranch()
		{
			TestBranchCore();
		}

		protected virtual void TestBranchCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Branch);
		}

		#endregion

		#region TestSupplier

		public void TestSupplier()
		{
			TestSupplierCore();
		}

		protected virtual void TestSupplierCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Supplier);
		}

		#endregion

		#region TestForwarder

		public void TestForwarder()
		{
			TestForwarderCore();
		}

		protected virtual void TestForwarderCore()
		{
			AssertNull(WarehouseJobGenericWrapper.Forwarder);
		}

		#endregion

		#region TestWeightSent

		public void TestWeightSent()
		{
			TestWeightSentCore();
		}

		protected virtual void TestWeightSentCore()
		{
			AssertNull(WarehouseJobGenericWrapper.WeightSent);
		}

		#endregion

		#region TestAssignedLoader

		public void TestAssignedLoader()
		{
			TestAssignedLoaderCore();
		}

		protected virtual void TestAssignedLoaderCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.AssignedLoader);
		}

		#endregion

		#region TestDispatchDriverName

		public void TestDispatchDriverName()
		{
			TestDispatchDriverNameCore();
		}

		protected virtual void TestDispatchDriverNameCore()
		{
			AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.DispatchDriverName);
		}

		#endregion

		#region TestDispatchDriverSignature

		public void TestDispatchDriverSignature()
		{
			TestDispatchDriverSignatureCore();
		}

		protected virtual void TestDispatchDriverSignatureCore()
		{
			AssertEquals(null, WarehouseJobGenericWrapper.DispatchDriverSignature);
		}

		#endregion

		#region TestReceiveDriverName

		public void TestReceiveDriverName()
		{
			TestReceiveDriverNameCore();
		}

		protected virtual void TestReceiveDriverNameCore()
		{
			AssertEquals(String.Empty, WarehouseJobGenericWrapper.ReceiveDriverName);
		}

		#endregion

		#region TestReceiveDriverSignature

		public void TestReceiveDriverSignature()
		{
			TestReceiveDriverSignatureCore();
		}

		protected virtual void TestReceiveDriverSignatureCore()
		{
			AssertEquals(null, WarehouseJobGenericWrapper.ReceiveDriverSignature);
		}

		#endregion

		#region TestHasDispatchDriverSignature

		public void TestHasDispatchDriverSignature()
		{
			TestHasDispatchDriverSignatureCore();
		}

		protected virtual void TestHasDispatchDriverSignatureCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.HasDispatchDriverSignature);
		}

		#endregion

		#region TestHasReceiveDriverSignature

		public void TestHasReceiveDriverSignature()
		{
			TestHasReceiveDriverSignatureCore();
		}

		protected virtual void TestHasReceiveDriverSignatureCore()
		{
			AssertEquals(false, WarehouseJobGenericWrapper.HasReceiveDriverSignature);
		}

		#endregion

		#region TestDeliveryRoute

		public void TestDeliveryRoute()
		{
			TestDeliveryRouteCore();
		}

		protected virtual void TestDeliveryRouteCore()
		{
			AssertEquals("Base DeliveryRoute is empty.", ZString.Empty, WarehouseJobGenericWrapper.DeliveryRoute);
		}

		#endregion

		#region TestLoadNumber

		public void TestLoadNumber()
		{
			TestLoadNumberCore();
		}

		protected virtual void TestLoadNumberCore()
		{
			AssertEquals("Load Number is empty.", ZString.Empty, WarehouseJobGenericWrapper.LoadNumber);
		}

		#endregion

		#region TestAllDCNsAreAuthorized

		public void TestAllDCNsAreAuthorized()
		{
			TestAllDCNsAreAuthorizedCore();
		}

		protected virtual void TestAllDCNsAreAuthorizedCore()
		{
			AssertEquals(ZBool.False, WarehouseJobGenericWrapper.AllDCNsAreAuthorized);
		}

		#endregion

		#region Test TransitWarehouse Properties

		#region TestContainerType

		public void TestContainerType() => TestContainerTypeCore();

		protected virtual void TestContainerTypeCore() => AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.ContainerType);

		#endregion

		#region TransportMode

		public void TestTransportMode() => TestTransportModeCore();

		protected virtual void TestTransportModeCore() => AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.TransportMode);

		#endregion

		#region GateInTime

		[TestDate(2023, 06, 06, 06, 06, 06)]
		public void TestGateInTime() => TestGateInTimeCore();

		protected virtual void TestGateInTimeCore() => AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.GateInTime);

		#endregion

		#region GateOutTime

		[TestDate(2023, 06, 06, 06, 06, 06)]
		public void TestGateOutTime() => TestGateOutTimeCore();

		protected virtual void TestGateOutTimeCore() => AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.GateOutTime);

		#endregion

		#region CutoffTime

		[TestDate(2023, 06, 06, 06, 06, 06)]
		public void TestCutoffTime() => TestCutoffTimeCore();

		protected virtual void TestCutoffTimeCore() => AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.CutoffTime);

		#endregion

		#region ExpectedDispatchTime

		[TestDate(2023, 06, 06, 06, 06, 06)]
		public void TestExpectedDispatchTime() => TestExpectedDispatchTimeCore();

		protected virtual void TestExpectedDispatchTimeCore() => AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.ExpectedDispatchTime);

		#endregion

		#region StartTime

		[TestDate(2023, 06, 06, 06, 06, 06)]
		public void TestStartTime() => TestStartTimeCore();

		protected virtual void TestStartTimeCore() => AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.StartTime);

		#endregion

		#region CompleteTime

		[TestDate(2023, 06, 06, 06, 06, 06)]
		public void TestCompleteTime() => TestCompleteTimeCore();

		protected virtual void TestCompleteTimeCore() => AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.CompleteTime);

		#endregion

		#region FinalizedTime

		[TestDate(2023, 06, 06, 06, 06, 06)]
		public void TestFinalizedTime() => TestFinalizedTimeCore();

		protected virtual void TestFinalizedTimeCore() => AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.FinalizedTime);

		#endregion

		#region FinalisedDate

		public void TestFinalisedDate()
		{
			TestFinalisedDateCore();
		}

		protected virtual void TestFinalisedDateCore()
		{
			AssertEquals(true, WarehouseJobGenericWrapper.FinalisedDate.IsEmpty);
		}

		#endregion

		#region IsAwaitingForwardingChanges

		public void TestIsAwaitingForwardingChanges() => TestIsAwaitingForwardingChangesCore();

		protected virtual void TestIsAwaitingForwardingChangesCore() => AssertEquals(ZBool.False, WarehouseJobGenericWrapper.IsAwaitingForwardingChanges);

		#endregion

		#region IsReadyToStage

		public void TestIsReadyToStage() => TestIsReadyToStageCore();

		protected virtual void TestIsReadyToStageCore() => AssertEquals(ZBool.False, WarehouseJobGenericWrapper.IsReadyToStage);

		#endregion

		#region IsSecure

		public void TestIsSecure() => TestIsSecureCore();

		protected virtual void TestIsSecureCore() => AssertEquals(ZBool.False, WarehouseJobGenericWrapper.IsSecure);

		#endregion

		#region NextDischargePort

		public void TestNextDischargePort() => TestNextDischargePortCore();

		protected virtual void TestNextDischargePortCore() => AssertEquals(ZString.Empty, WarehouseJobGenericWrapper.NextDischargePort);

		#endregion

		#region WarehouseExpectedArrivalTime

		public void TestWarehouseExpectedArrivalTime() => TestGetWarehouseExpectedArrivalTime();

		protected virtual void TestGetWarehouseExpectedArrivalTime()
		{
			AssertEquals(ZDateTime.Empty, WarehouseJobGenericWrapper.WarehouseExpectedArrivalTime);
		}

		#endregion

		#endregion

		#region TestIsFinalised

		public void TestIsFinalised()
		{
			TestIsFinalisedCore();
		}

		protected virtual void TestIsFinalisedCore()
		{
			AssertEquals(ZBool.False, WarehouseJobGenericWrapper.IsFinalised);
		}

		#endregion

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var emptyWrapper = GetNewWarehouseJobGenericWrapper(null);
			AssertEquals("BOLNumber", "", emptyWrapper.BOLNumber);
			AssertNull("CarrierServiceLevel", emptyWrapper.CarrierServiceLevel);
			AssertEquals("CartageAdviceClosingText", "", emptyWrapper.CartageAdviceClosingText);
			AssertEquals("CartageAdviceOpeningText", "", emptyWrapper.CartageAdviceOpeningText);
			AssertEquals("CartageDropMode", "", emptyWrapper.CartageDropMode);
			AssertNull("Client", emptyWrapper.Client);
			AssertNull("ClientRequestedBillToParty", emptyWrapper.ClientRequestedBillToParty);
			AssertNull("CODAmount", emptyWrapper.CODAmount);
			AssertNull("CODType", emptyWrapper.CODType);
			AssertNull("Consignee", emptyWrapper.Consignee);
			AssertNull("ConsigneeAddress", emptyWrapper.ConsigneeAddress);
			AssertEquals("ConsolidatedInvoiceRef", "", emptyWrapper.ConsolidatedInvoiceRef);
			AssertEquals("ContainerNumberAndTypeLine", "", emptyWrapper.ContainerNumberAndTypeLine);
			AssertEquals("CustomerReference", true, emptyWrapper.CustomerReference.IsEmpty);
			AssertNull("Destination", emptyWrapper.Destination);
			AssertEquals("DockDoor", "", emptyWrapper.DockDoor);
			AssertNull("DropMode", emptyWrapper.DropMode);
			AssertEquals("FinalisedDate", true, emptyWrapper.FinalisedDate.IsEmpty);
			AssertNull("FulfillRule", emptyWrapper.FulfillRule);
			AssertNull("GoodsBillToAddress", emptyWrapper.GoodsBillToAddress);
			AssertEquals("HandlingInstructions", true, emptyWrapper.HandlingInstructions.IsEmpty);
			AssertNull("IncoTerm", emptyWrapper.IncoTerm);
			AssertNull("Insurance", emptyWrapper.Insurance);
			AssertEquals("InvoiceNumber", "", emptyWrapper.InvoiceNumber);
			AssertNull("JobClient", emptyWrapper.JobClient);
			AssertEquals("JobNumber", "", emptyWrapper.JobNumber);
			AssertEquals("PickingInstructions", true, emptyWrapper.PickingInstructions.IsEmpty);
			AssertEquals("PickMethod", true, emptyWrapper.PickMethod.IsEmpty);
			AssertEquals("PickNo", true, emptyWrapper.PickNo.IsEmpty);
			AssertEquals("PickNumberReference", true, emptyWrapper.PickNumberReference.IsEmpty);
			AssertNull("PickOption", emptyWrapper.PickOption);
			AssertEquals("PrimaryBarcode", true, emptyWrapper.PrimaryBarcode.IsEmpty);
			AssertEquals("PrimaryBarcodeText", true, emptyWrapper.PrimaryBarcodeText.IsEmpty);
			AssertEquals("References", "", emptyWrapper.References);
			AssertEquals("RequiredDate", true, emptyWrapper.RequiredDate.IsEmpty);
			AssertNull("SalesChannel", emptyWrapper.SalesChannel);
			AssertEquals("SecondaryHeading", "", emptyWrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber", "", emptyWrapper.SecondaryNumber);
			AssertEquals("SecondaryReference", true, emptyWrapper.SecondaryReference.IsEmpty);
			AssertNull("ServiceLevel", emptyWrapper.ServiceLevel);
			AssertEquals("Status", true, emptyWrapper.Status.IsEmpty);
			AssertEquals("TotalNumberOfLabels", 0, emptyWrapper.TotalNumberOfLabels);
			AssertEquals("TotalNumberOfPackageLabels", 0, emptyWrapper.TotalNumberOfPackageLabels);
			AssertNull("TransportBillToAddress", emptyWrapper.TransportBillToAddress);
			TestWrapperMappingsEmpty_TransportCoAddress(emptyWrapper);
			AssertNull("TransportCompany", emptyWrapper.TransportCompany);
			AssertEquals("TransportReference", true, emptyWrapper.TransportReference.IsEmpty);
			AssertNull("Warehouse", emptyWrapper.Warehouse);
			AssertEquals("WarehouseCartageCoordinatorName", "", emptyWrapper.WarehouseCartageCoordinatorName);
			AssertEquals("WarehouseCartageCoordinatorPhone", "", emptyWrapper.WarehouseCartageCoordinatorPhone);
			AssertEquals("WhoCreated", true, emptyWrapper.WhoCreated.IsEmpty);
			AssertEquals("WhoFinalised", true, emptyWrapper.WhoFinalised.IsEmpty);
			AssertEquals("HasShortfallItems", false, emptyWrapper.HasShortfallItems);
			AssertEquals("HasNonPickedItems", false, emptyWrapper.HasNonPickedItems);
			AssertEquals("HouseBill", "", emptyWrapper.HouseBill);
			AssertEquals("MasterBill", "", emptyWrapper.MasterBill);
			AssertEquals("OtherReference", "", emptyWrapper.OtherReferences);
			AssertEquals("VendorID", "", emptyWrapper.VendorID);
			AssertNull("SupplierBuyerLink", emptyWrapper.SupplierBuyerLink);
			AssertEquals("LoadNumber", "", emptyWrapper.LoadNumber);

			TestWrapperMappingsEmpty_NotDependantToBizOProperties(emptyWrapper);
		}

		protected virtual void TestWrapperMappingsEmpty_TransportCoAddress(WarehouseJobGenericWrapper emptyWrapper)
		{
			AssertNull("TransportCoAddress", emptyWrapper.TransportCoAddress);
		}

		protected virtual void TestWrapperMappingsEmpty_NotDependantToBizOProperties(WarehouseJobGenericWrapper emptyWrapper)
		{
			AssertEquals("DocumentTitle", "", emptyWrapper.DocumentTitle);
			AssertEquals("JobNumberHeading", "", emptyWrapper.JobNumberHeading);
			AssertNull("TotalExtendedLinePrice", emptyWrapper.TotalExtendedLinePrice);
			AssertEquals("TotalOuterPackagesWeight", WeightWrapper.Empty.ValueAndUnitCode, emptyWrapper.TotalOuterPackagesWeight.ValueAndUnitCode);
			AssertEquals("TotalOuterPackagesVolume", VolumeWrapper.Empty.ValueAndUnitCode, emptyWrapper.TotalOuterPackagesVolume.ValueAndUnitCode);
			AssertEquals("OuterPackagesContents", "", emptyWrapper.OuterPackagesContents);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		public void TestWarehouseBOWrapper_FactoryCached()
		{
			if (ImplementsWarehouseCore)
			{
				var warehouse = Helper.CreateWarehouse("Whs1");
				var whsObject1 = GetNewWhsBusinessObject();
				SetWhsBusinessObjectWarehouse(whsObject1, warehouse.PK);
				var whsObjectWrapper1 = GetNewWarehouseJobGenericWrapper(whsObject1);

				var whsObject2 = GetNewWhsBusinessObject();
				SetWhsBusinessObjectWarehouse(whsObject2, warehouse.PK);
				var whsObjectWrapper2 = GetNewWarehouseJobGenericWrapper(whsObject2);

				AssertNotEquals("Precondition: whs objects are not the same.", whsObject1.PK, whsObject2.PK);
				AssertEquals("Wrappers share the same instance of WarehouseBO wrapper.", whsObjectWrapper1.Warehouse, whsObjectWrapper2.Warehouse);
				AssertEquals(warehouse, whsObjectWrapper1.Warehouse.WrappedObject);
				AssertEquals(warehouse, whsObjectWrapper2.Warehouse.WrappedObject);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestWarehouseBOWrapper_FactoryCached_DifferentWarehouse()
		{
			if (ImplementsWarehouseCore)
			{
				var warehouse1 = Helper.CreateWarehouse("Whs1");
				var whsObject1 = GetNewWhsBusinessObject();
				SetWhsBusinessObjectWarehouse(whsObject1, warehouse1.PK);
				var whsObjectWrapper1 = GetNewWarehouseJobGenericWrapper(whsObject1);

				var warehouse2 = Helper.CreateWarehouse("Whs2");
				var whsObject2 = GetNewWhsBusinessObject();
				SetWhsBusinessObjectWarehouse(whsObject2, warehouse2.PK);
				var whsObjectWrapper2 = GetNewWarehouseJobGenericWrapper(whsObject2);

				AssertNotEquals("Precondition: whs objects are not the same.", whsObject1.PK, whsObject2.PK);
				AssertNotEquals("Wrappers don't share the same instance of WarehouseBO wrapper.", whsObjectWrapper1.Warehouse, whsObjectWrapper2.Warehouse);
				AssertEquals(warehouse1, whsObjectWrapper1.Warehouse.WrappedObject);
				AssertEquals(warehouse2, whsObjectWrapper2.Warehouse.WrappedObject);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestWarehouseBOWrapper_FactoryCached_DifferentFactory()
		{
			if (ImplementsWarehouseCore)
			{
				var warehouse = Helper.CreateWarehouse("Whs1");
				Factory.Save();

				var whsObject1 = GetNewWhsBusinessObject();
				SetWhsBusinessObjectWarehouse(whsObject1, warehouse.PK);
				var whsObjectWrapper1 = GetNewWarehouseJobGenericWrapper(whsObject1);

				var newFactory = new BusinessObjectFactory();
				var whsObject2 = GetNewWhsBusinessObjectInSpecifiedFactory(newFactory);
				SetWhsBusinessObjectWarehouse(whsObject2, warehouse.PK);
				var whsObjectWrapper2 = GetNewWarehouseJobGenericWrapperInSpecifiedFactory(whsObject2, newFactory);

				AssertNotEquals("Wrappers don't share the same instance of WarehouseBO wrapper.", whsObjectWrapper1.Warehouse, whsObjectWrapper2.Warehouse);
				AssertEquals(warehouse, whsObjectWrapper1.Warehouse.WrappedObject);

				var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
				AssertEquals(warehouseInNewFactory, whsObjectWrapper2.Warehouse.WrappedObject);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool ImplementsWarehouseCore => true;

		protected virtual void SetWhsBusinessObjectWarehouse(BusinessObject bizO, ZGuid warehousePK)
		{
		}

		protected virtual BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return null;
		}

		protected virtual WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return null;
		}

		#endregion

		#region Implementation

		#region Warehouse

		protected WhsWarehouse Warehouse
		{
			get { return warehouse ?? (warehouse = Factory.New<WhsWarehouse>()); }
		}
		WhsWarehouse warehouse;

		#endregion

		#region WhsBusinessObject

		protected BusinessObject WhsBusinessObject => whsBusinessObject ?? (whsBusinessObject = GetNewWhsBusinessObject());
		BusinessObject whsBusinessObject;

		protected abstract BusinessObject GetNewWhsBusinessObject();

		#endregion

		#region WarehouseGenericJobWrapper

		WarehouseJobGenericWrapper WarehouseJobGenericWrapper
		{
			get { return warehouseJobGenericWrapper ?? (warehouseJobGenericWrapper = GetNewWarehouseJobGenericWrapper(WhsBusinessObject)); }
		}
		WarehouseJobGenericWrapper warehouseJobGenericWrapper;

		protected abstract WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO);

		#endregion

		#region Helper

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		#region GetNewDocumentWrapper

		protected sealed override DocBaseWrapper GetNewDocumentWrapper()
		{
			return GetNewWarehouseJobGenericWrapper(WhsBusinessObject);
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			base.SetUp();
		}

		#endregion

		#region ExpectedFieldMap

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
WarehouseJob
======================================================================
Name                                    Type
----------------------------------------------------------------------
ConsigneeAddress                        Address
DistributionCentreAddress               Address
DropOffAddress                          Address
GoodsBillToAddress                      Address
PickUpAddress                           Address
SupplierDocAddress                      Address
TransportBillToAddress                  Address
TransportCoAddress                      Address
CarrierServiceLevel                     CarrierServiceLevel
CODType                                 CodeAndDescription
CustomsStatus                           CodeAndDescription
DropMode                                CodeAndDescription
FulfillRule                             CodeAndDescription
IncoTerm                                CodeAndDescription
PickOption                              CodeAndDescription
SalesChannel                            CodeAndDescription
ServiceLevel                            CodeAndDescription
TransportationUnit                      Equipment
ConfirmationInstructions                LabelValuePair
CustomerReference                       LabelValuePair
FinalisedDate                           LabelValuePair
HandlingInstructions                    LabelValuePair
PickingInstructions                     LabelValuePair
PickMethod                              LabelValuePair
PickNo                                  LabelValuePair
PickNumberReference                     LabelValuePair
PrimaryBarcode                          LabelValuePair
RequiredDate                            LabelValuePair
SecondaryReference                      LabelValuePair
SOPCarrierServiceLevel                  LabelValuePair
SOPOrderNumber                          LabelValuePair
SOPRequiredDate                         LabelValuePair
SOPSpecialInstructions                  LabelValuePair
SOPStagingAreaName                      LabelValuePair
SOPTransportCompany                     LabelValuePair
SplitNumber                             LabelValuePair
StagingAreaName                         LabelValuePair
StagingLocationString                   LabelValuePair
Status                                  LabelValuePair
TotalLoadedPackages                     LabelValuePair
TotalLoadedUnits                        LabelValuePair
TransportReference                      LabelValuePair
VehicleReference                        LabelValuePair
WarehouseName                           LabelValuePair
WhoCreated                              LabelValuePair
WhoFinalised                            LabelValuePair
CODAmount                               Money
Insurance                               Money
TotalExtendedLinePrice                  Money
Client                                  Organisation
ClientRequestedBillToParty              Organisation
Consignee                               Organisation
Consignor                               Organisation
Forwarder                               Organisation
JobClient                               Organisation
Supplier                                Organisation
TransportCompany                        Organisation
CarrierAccount                          OrgCarrierAccount
Destination                             PlaceAndDate
SupplierBuyerLink                       SupplierBuyerLink
PackagesSent                            ValueAndUnit
CubicSent                               Volume
TotalLoadedVolume                       Volume
TotalOuterPackagesVolume                Volume
Warehouse                               WarehouseBO
TotalLoadedWeight                       Weight
TotalOuterPackagesWeight                Weight
WeightSent                              Weight
ABN                                     String
AccountCode                             String
ACSEstCode                              String
AllDCNsAreAuthorized                    Bool
AllowPartialLoading                     Bool
ArrivalDate                             DateTime
AssignedLoader                          String
ATOEstCode                              String
BOLNumber                               String
BookingDate                             DateTime
CartageAdviceClosingText                MultilingualString
CartageAdviceOpeningText                MultilingualString
CartageDropMode                         String
ClientCPC                               String
ClientGCR                               String
CompleteTime                            DateTime
ConsolidatedInvoiceRef                  String
ContainerNumberAndTypeLine              String
ContainerType                           String
CP_IssueNo                              String
CurrencyCode                            String
CurrencySymbol                          String
CustomAttribute1                        String
CustomAttribute2                        String
CustomAttribute3                        String
CustomAttribute4                        String
CustomAttribute5                        String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomerReferenceBarcode                String
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CutoffTime                              DateTime
DebtorCodeAndName                       String
DeliveryRoute                           String
DepartmentName                          String
DepartmentNumber                        String
DispatchDriverName                      String
DockDoor                                String
DocketStatus                            String
DocketSubType                           String
DocketType                              String
DocumentTitle                           String
EmergencyContactMessageString           String
EnableDangerousGoodsDetails             Bool
EnableExtendedLinePrice                 Bool
EventTypeCode                           String
ExpectedDispatchTime                    DateTime
FinalizedTime                           DateTime
FromDate                                DateTime
GateInTime                              DateTime
GateOutTime                             DateTime
HasDispatchDriverSignature              Bool
HasMultipleStockKeepingUnits            Bool
HasMultipleVolumeUnits                  Bool
HasMultipleWeightUnits                  Bool
HasNonPickedItems                       Bool
HasOversAndUnders                       Bool
HasReceiveDriverSignature               Bool
HasShortfallItems                       Bool
HouseBill                               String
HouseBillHeading                        String
InvoiceNumber                           String
IsAuthorisedToLeave                     Bool
IsAuthorizedForDispatch                 Bool
IsAwaitingForwardingChanges             Bool
IsCustomsTransaction                    Bool
IsFinalised                             Bool
IsReadyToStage                          Bool
IsSecure                                Bool
IsSplit                                 Bool
IsWorkOrder                             Bool
IsWorkOrderPick                         Bool
JobNumber                               String
JobNumberHeading                        String
JobType                                 String
LoadNumber                              String
MasterBill                              String
MasterBillHeading                       String
NextDischargePort                       String
OrderTypeCodeFirst2Characters           String
OrderTypeCodeLast4Characters            String
OtherReferences                         String
OuterPackagesContents                   String
PackingSlipTitle                        String
PalletsSent                             Short
PrimaryBarcodeText                      String
PrintDGDetails                          String
PrintPageWithContainerNumber            Bool
ProductLinesCount                       Int
ReceiveDriverName                       String
References                              String
ReferencesExtended                      String
ReportDescription                       MultilingualString
Seal                                    String
SecondaryHeading                        String
SecondaryNumber                         String
SelectedABCCategory                     String
SelectedArea                            MultilingualString
SelectedClient                          String
SelectedCommodityCode                   String
SelectedCycle                           String
SelectedLocation                        String
SelectedPickMethod                      String
SelectedRow                             String
SelectedStocktakeType                   String
SelectedSupplierPart                    String
SOPConsigneeAddressLabel                String
StartTime                               DateTime
StocktakeNumber                         String
SubTypeDesc                             String
ToDate                                  DateTime
TotalCubic                              Decimal
TotalExtendedLinePriceWithSymbol        String
TotalGroupedLineUnitsMet                Decimal
TotalInnerPackLines                     Short
TotalInners                             Short
TotalNumberOfLabels                     Int
TotalNumberOfPackageLabels              Int
TotalOverpacks                          Short
TotalPallets                            Short
TotalTopLevelUnitsMet                   Decimal
TotalTopLevelUnitsOrdered               Decimal
TotalUnits                              Decimal
TotalWeight                             Decimal
TransportMode                           String
TransportZone                           String
UnitsSent                               Decimal
UnloadCompleteTime                      DateTime
VehicleNumber                           String
VendorID                                String
WarehouseCartageCoordinatorName         String
WarehouseCartageCoordinatorPhone        String
WarehouseCCPCode                        String
WarehouseExpectedArrivalTime            DateTime
WarehouseNameAndAddress                 MultilingualString
WarehousePhoneAndFax                    String
WarehouseReference                      String
WorkOrderLevels10th                     String
WorkOrderLevels1st                      String
WorkOrderLevels2nd                      String
WorkOrderLevels3rd                      String
WorkOrderLevels4th                      String
WorkOrderLevels5th                      String
WorkOrderLevels6th                      String
WorkOrderLevels7th                      String
WorkOrderLevels8th                      String
WorkOrderLevels9th                      String

Containers                              Container Collection
LoadedPackages                          Package Collection
Packages                                Package Collection
UNDGs                                   UNDGSubstance Collection
JobLinesVariances                       WarehouseGroupedLinesForVariance Collection
DispatchLoadLists                       WarehouseJob Collection
DispatchTransportationUnits             WarehouseJob Collection
Jobs                                    WarehouseJob Collection
Orders                                  WarehouseJob Collection
ReceiveTransportationUnits              WarehouseJob Collection
BillOfLadingPackingLines                WarehouseJobLine Collection
BillOfLadingPackingLinesUS              WarehouseJobLine Collection
BOMStagingAreaParts                     WarehouseJobLine Collection
JobLines                                WarehouseJobLine Collection
PackingLines                            WarehouseJobLine Collection
PalletizedInventory                     WarehouseJobLine Collection
PickingLines                            WarehouseJobLine Collection
VarianceLines                           WarehouseJobLine Collection
WorkOrderLines                          WarehouseJobLine Collection
";
			}
		}

		#endregion

		#endregion
	}
}
