using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Barcode.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.ExcelTemplates;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Scanning;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class FreightJobWrapperNonInheritedTest : TestCaseWithFactory
	{
		#region TestWrappersHaveWrappedChildBusinessObjectWhenItHasChildBizObjToWrap

		public void TestWrappersHaveWrappedChildBusinessObjectWhenItHasChildBizObjToWrap()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "123456";
			shipment.OuterPackLines.AddNew().SetContainer(consol, container);
			Factory.Save();

			var wrappers = FreightWrapper.New(shipment, container, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			var freightWrapper = wrappers[0] as IDocWrapperWithChildBizObjToWrap;
			AssertEquals(freightWrapper.WrappedChildBusinessObject, container);
		}

		#endregion

		#region TestScanningBarcodes

		public void TestScanningBarcodes()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var freightWrapper = FreightWrapper.New(packageJob, Factory)[0];

			foreach (var barcode in PackingBarcodes.Barcodes)
			{
				AssertNotNull("Did not contain barcode '" + barcode + "'.",
					freightWrapper.ScanningBarcodes.Cast<CodeAndDescriptionWrapper>().First(w => w.Code == new TextBarcode(barcode).TextAs128sFontString && w.Description == barcode));
			}
		}

		#endregion

		#region TestNewForAccTransactionHeader

		public void TestNewForAccTransactionHeader()
		{
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranch currentCompanyBranch = currentCompany.Branches[0];

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";

			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = org1.PK;
			invoice.AH_TransactionNum = "00001001";
			invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice.AH_GB = currentCompanyBranch.PK;
			invoice.AH_ConsolidatedInvoiceRef = "C00009999";

			Factory.Save();

			FreightWrapper freightWrapper = FreightWrapper.New(invoice, Factory)[0];
			AssertEquals("FreightWrapper should be of type FreightWrapperFromInvoice.", typeof(FreightWrapperFromInvoice), freightWrapper.GetType());
		}

		#endregion

		#region TestCartageInfo_Child_Container

		public void TestCartageInfo_Child_Container()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Constants.ContainerModes.FCL;

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Constants.ContainerModes.FCL;

			var declarationCPOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var declarationCPAddress = declarationCPOrg.Addresses.AddNew();
			declarationCPAddress.OA_Address1 = "DeclarationContPark";
			declaration.ContainerYardDocAddress.E2_OA_Address = declarationCPAddress.PK;

			FreightWrapper[] wrappers = FreightWrapper.New(declaration, container, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			var freightWrapper = wrappers[0];

			AssertEquals("JourneyOnePickUpAddress", "DeclarationContPark", freightWrapper.CartageInfo.JourneyOnePickUpAddress.Address1);

			var containerCPOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var containerCPAddress = containerCPOrg.Addresses.AddNew();
			containerCPAddress.OA_Address1 = "ContainerContPark";

			container.JobContainer.JC_OA_DepartureContainerYardAddress = containerCPAddress.PK;
			AssertEquals("JourneyOnePickUpAddress", "ContainerContPark", freightWrapper.CartageInfo.JourneyOnePickUpAddress.Address1);
		}

		#endregion

		#region TestCartageInfo_Declaration_And_Freight_Container

		public void TestCartageInfo_Declaration_And_Freight_Container()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			OrgHeader pickupOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			OrgAddress pickupAddress = pickupOrg.Addresses.AddNew();
			pickupAddress.OA_Address1 = "PickupAddress";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;

			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA1234567";
			shipment.OuterPackLines.AddNew().SetContainer(consol, container);

			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Constants.ContainerModes.FCL;
			declaration.JE_JS = shipment.PK;

			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.CusContainers.AddNew().CO_ContainerNumber = container.JC_ContainerNum;

			FreightWrapper[] wrappers = FreightWrapper.New(shipment, container, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			FreightWrapper freightWrapper = wrappers[0];
			freightWrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP));

			AssertEquals("JourneyOneDeliverToAddress", "PickupAddress", freightWrapper.CartageInfo.JourneyOneDeliverToAddress.Address1);
		}

		#endregion

		#region TestCartageInfo_Child_ContainerLeg

		public void TestCartageInfo_Child_ContainerLeg()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonCartageLeg leg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();

			FreightWrapper[] wrappers = FreightWrapper.New(cartage, leg, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
		}

		#endregion

		#region TestCartageInfo_Child_CartageContainer

		public void TestCartageInfo_Child_CartageContainer()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];

			FreightWrapper[] wrappers = FreightWrapper.New(cartage, container, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			AssertEquals("Should be container pk", container, ((DocBaseWrapper)wrappers[0].CartageInfo.WrappedObject).WrappedObject);
		}

		#endregion

		#region TestRemoveUnrelatedLocalTransportLegs

		public void TestRemoveUnrelatedLocalTransportLegs()
		{
			var cartage = Factory.New<CommonCartage>();
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			var move1 = cartage.GetBookedMoves(container1)[0];
			var move2 = cartage.GetBookedMoves(container2)[0];
			var leg1A = move1.CartageLegs.AddNew();
			var leg1B = move1.CartageLegs.AddNew();
			var leg2A = move2.CartageLegs.AddNew();
			var leg2B = move2.CartageLegs.AddNew();

			// Container 1
			var wrappers = FreightWrapper.New(cartage, container1, Factory);
			AssertEquals(1, wrappers.Length);

			var cartageWrapper = (FreightWrapperFromCartage)wrappers[0];
			AssertEquals(1, cartageWrapper.Containers.Count);
			AssertEquals(container1, cartageWrapper.Containers[0].WrappedObject);

			var expectedLegs = cartageWrapper.LocalTransportLegs.Cast<GenericWrapper>().Select(l => l.WrappedObject);
			AssertContainsExactElementsInAnyOrder(new[] { leg1A, leg1B }, expectedLegs);

			// Container 2
			wrappers = FreightWrapper.New(cartage, container2, Factory);
			AssertEquals(1, wrappers.Length);

			cartageWrapper = (FreightWrapperFromCartage)wrappers[0];
			AssertEquals(1, cartageWrapper.Containers.Count);
			AssertEquals(container2, cartageWrapper.Containers[0].WrappedObject);

			expectedLegs = cartageWrapper.LocalTransportLegs.Cast<GenericWrapper>().Select(l => l.WrappedObject);
			AssertContainsExactElementsInAnyOrder(new[] { leg2A, leg2B }, expectedLegs);
		}

		#endregion

		#region TestFreightWrapperWithService_Shipment

		public void TestFreightWrapperWithService_Shipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol, container1);
			packline2.SetContainer(consol, container2);

			var service1 = shipment.DocsAndCartage.Services.AddNew();
			service1.ES_Booked = new ZDateTime(2009, 1, 1);

			var service2 = shipment.DocsAndCartage.Services.AddNew();
			service2.ES_Booked = new ZDateTime(2009, 3, 3);

			var wrappers = FreightWrapper.New(shipment, service2, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			var freightWrapper = wrappers[0];

			AssertEquals("Only 1 service wrapped", 1, freightWrapper.Services.Count);
			AssertEquals("Correct service wrapped", new ZDateTime(2009, 3, 3), freightWrapper.Services[0].DateBooked);
		}

		#endregion

		#region TestFreightWrapperWithService_ForwardingContainer

		public void TestFreightWrapperWithService_ForwardingContainer()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol, container1);
			packline2.SetContainer(consol, container2);

			var service1 = container1.Services.AddNew();
			service1.ES_Booked = new ZDateTime(2009, 1, 1);

			var service2 = container2.Services.AddNew();
			service2.ES_Booked = new ZDateTime(2009, 3, 3);

			var wrappers = FreightWrapper.New(container2, service2, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);

			var freightWrapper = wrappers[0];
			AssertEquals("Only 1 service wrapped", 1, freightWrapper.Services.Count);
			AssertEquals("FreightWrapperFromConsol", typeof(FreightWrapperFromConsol), freightWrapper.GetType());
			AssertEquals("Correct service wrapped", new ZDateTime(2009, 3, 3), freightWrapper.Services[0].DateBooked);
			AssertEquals("Only 1 container wrapped", 1, freightWrapper.Containers.Count);
			AssertEquals("Correct container wrapped", container2, freightWrapper.Containers[0].WrappedObject);
		}

		#endregion

		#region TestFreightWrapperWithService_CusContainer

		public void TestFreightWrapperWithService_CusContainer()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();

			var service1 = container1.Services.AddNew();
			service1.ES_Booked = new ZDateTime(2009, 1, 1);

			var service2 = container2.Services.AddNew();
			service2.ES_Booked = new ZDateTime(2009, 3, 3);

			var wrappers = FreightWrapper.New(container2.JobContainer, service2, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);

			var freightWrapper = wrappers[0];
			AssertEquals("Only 1 service wrapped", 1, freightWrapper.Services.Count);
			AssertEquals("FreightWrapperFromDeclaration", true, typeof(FreightWrapperFromDeclaration).IsAssignableFrom(freightWrapper.GetType()));
			AssertEquals("Correct service wrapped", new ZDateTime(2009, 3, 3), freightWrapper.Services[0].DateBooked);
			AssertEquals("Only 1 container wrapped", 1, freightWrapper.Containers.Count);
			AssertEquals("Correct container wrapped", container2, freightWrapper.Containers[0].WrappedObject);
		}

		#endregion

		#region TestFreightWrapperWithService_CFSContainer

		public void TestFreightWrapperWithService_CFSContainer()
		{
			var shipment = Factory.New<CFSShipment>();
			var consol = shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol, container1);
			packline2.SetContainer(consol, container2);

			var service1 = container1.Services.AddNew();
			service1.ES_Booked = new ZDateTime(2009, 1, 1);

			var service2 = container2.Services.AddNew();
			service2.ES_Booked = new ZDateTime(2009, 3, 3);

			var wrappers = FreightWrapper.New(container2, service2, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);

			var freightWrapper = wrappers[0];
			AssertEquals("Only 1 service wrapped", 1, freightWrapper.Services.Count);
			AssertEquals("FreightWrapperFromCFSContainer", typeof(FreightWrapperFromCFSContainer), freightWrapper.GetType());
			AssertEquals("Correct service wrapped", new ZDateTime(2009, 3, 3), freightWrapper.Services[0].DateBooked);
			AssertEquals("Only 1 container wrapped", 1, freightWrapper.Containers.Count);
			AssertEquals("Correct container wrapped", container2, freightWrapper.Containers[0].WrappedObject);
		}

		#endregion

		#region TestFreightWrapperWithService_CartageContainer

		public void TestFreightWrapperWithService_CartageContainer()
		{
			var cartage = Factory.New<CommonCartage>();
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;

			var service1 = container1.Services.AddNew();
			service1.ES_Booked = new ZDateTime(2009, 1, 1);

			var service2 = container2.Services.AddNew();
			service2.ES_Booked = new ZDateTime(2009, 3, 3);

			var wrappers = FreightWrapper.New(container2, service2, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);

			var freightWrapper = wrappers[0];
			AssertEquals("Only 1 service wrapped", 1, freightWrapper.Services.Count);
			AssertEquals("FreightWrapperFromCartage", typeof(FreightWrapperFromCartage), freightWrapper.GetType());
			AssertEquals("Correct service wrapped", new ZDateTime(2009, 3, 3), freightWrapper.Services[0].DateBooked);
			AssertEquals("Only 1 container wrapped", 1, freightWrapper.Containers.Count);
			AssertEquals("Correct container wrapped", container2, freightWrapper.Containers[0].WrappedObject);
		}

		#endregion

		#region TestNotClearedByAgent

		public void TestNotClearedByAgent()
		{
			FreightDataRegistry.Instance.NotClearedByAgentStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "~~~IIIIII");
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Germany);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CusEntryNumber entryNumber = shipment.CusEntryNumbers.AddNew();
			entryNumber.CE_EntryType = CusEntryNumberTypes.EU.T1;
			entryNumber.CE_EntryNum = "asdasd";
			entryNumber.CE_IssueDate = new ZDateTime(2008, 10, 9);
			entryNumber.CE_ExpiryDate = new ZDateTime(2008, 10, 15);

			FreightWrapper wrapper = FreightWrapper.New(shipment, Factory)[0];

			AssertEquals("wrapper.NotClearedByAgentStatement", "~~~IIIIII", wrapper.NotClearedByAgentStatement);
			AssertEquals("wrapper.NotClearedByAgentNumber", "asdasd", wrapper.NotClearedByAgentNumber);
			AssertEquals("wrapper.NotClearedByAgentIssueDate", new ZDateTime(2008, 10, 9), wrapper.NotClearedByAgentIssueDate);
			AssertEquals("wrapper.NotClearedByAgentExpiryDate", new ZDateTime(2008, 10, 15), wrapper.NotClearedByAgentExpiryDate);
		}

		#endregion

		#region TestFreightWrapperNewFromPackageHeader

		public void TestFreightWrapperNewFromPackageHeader()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var loosePackageHeader1 = packageJob.LoosePackageIDs.AddNew();
			var loosePackageHeader2 = packageJob.LoosePackageIDs.AddNew();

			loosePackageHeader1.CurrentPackageJob = packageJob;
			loosePackageHeader2.CurrentPackageJob = packageJob;
			loosePackageHeader1.KPH_PackageID = "ABC";
			loosePackageHeader2.KPH_PackageID = "DEF";

			var dummy = Factory.New<DummyBusinessObject>();
			var wrappersEmpty = FreightWrapper.New(dummy, loosePackageHeader1, Factory);
			AssertNull(wrappersEmpty);

			var wrappers1 = FreightWrapper.New(packageJob, loosePackageHeader1, Factory);
			AssertEquals(1, wrappers1.Length);
			AssertEquals(typeof(FreightWrapperFromPkgPackageJob), wrappers1[0].GetType());
			AssertEquals(loosePackageHeader1.PK, wrappers1[0].Packages[0].WrappedObjectPK);

			var wrappers2 = FreightWrapper.New(packageJob, loosePackageHeader2, Factory);
			AssertEquals(1, wrappers2.Length);
			AssertEquals(typeof(FreightWrapperFromPkgPackageJob), wrappers2[0].GetType());
			AssertEquals(loosePackageHeader2.PK, wrappers2[0].Packages[0].WrappedObjectPK);
		}

		#endregion

		#region TestFreightWrapperNewFromDeclarationWithChildInvoice

		public void TestFreightWrapperNewFromDeclarationWithChildInvoice()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			FreightWrapper[] wrappers = FreightWrapper.New(declaration, invoice, Factory);

			AssertEquals(1, wrappers.Length);
			AssertEquals(typeof(FreightWrapperFromDeclaration<BaseJobDeclaration, Customs.General.DocDeclaration>), wrappers[0].GetType());
			AssertEquals(declaration.PK, wrappers[0].WrappedObjectPK);
		}

		#endregion

		#region TestFreightWrapperNewFromShipmentWithChildContainer

		public void TestFreightWrapperNewFromShipmentWithChildContainer()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			ForwardingContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "OOCL0000006";
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			ForwardingContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "OOCL0000013";
			container2.JC_ContainerMode = Constants.ContainerModes.FCL;

			ForwardingContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "OOCL0000027";
			container3.JC_ContainerMode = Constants.ContainerModes.FCL;

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "NZAKL";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PLT";
			packLine1.JL_JC = container1.PK;

			var pk = UNDGSubstanceLoader.LoadSubstances(Factory, "0010", "", "IMO").First().PK;
			packLine1.UNDGs.AddNew().DI_DG = pk;
			packLine1.UNDGs.AddNew().DI_DG = pk;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			packLine2.JL_F3_NKPackType = "PLT";
			packLine2.JL_JC = container2.PK;

			packLine2.UNDGs.AddNew().DI_DG = pk;

			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			FreightWrapper[] wrappers = FreightWrapper.New(shipment, container1, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			FreightWrapper freightWrapper = wrappers[0];
			AssertEquals("freightWrapper.Containers.Count", 1, freightWrapper.Containers.Count);
			ContainerWrapper containerWrapper = freightWrapper.Containers[0];
			AssertEquals("containerWrapper.WrappedObjectPK", container1.PK, containerWrapper.WrappedObjectPK);
			AssertEquals("freightWrapper.UNDGs.Count", 2, freightWrapper.UNDGs.Count);

			wrappers = FreightWrapper.New(shipment, container2, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			freightWrapper = wrappers[0];
			AssertEquals("freightWrapper.Containers.Count", 1, freightWrapper.Containers.Count);
			containerWrapper = freightWrapper.Containers[0];
			AssertEquals("containerWrapper.WrappedObjectPK", container2.PK, containerWrapper.WrappedObjectPK);
			AssertEquals("freightWrapper.UNDGs.Count", 1, freightWrapper.UNDGs.Count);

			wrappers = FreightWrapper.New(shipment, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			freightWrapper = wrappers[0];
			AssertEquals("freightWrapper.Containers.Count", 2, freightWrapper.Containers.Count);
			AssertEquals("freightWrapper.UNDGs.Count", 3, freightWrapper.UNDGs.Count);
		}

		#endregion

		#region TestFreightWrapperNewFromCartageWithChildCartageLeg

		public void TestFreightWrapperNewFromCartageWithChildCartageLeg()
		{
			var cartage = Factory.New<CommonCartage>();
			var containerMove1 = cartage.ContainerBookedMoves.AddNew();
			var containerMove2 = cartage.ContainerBookedMoves.AddNew();

			var container1 = containerMove1.Container;
			var container2 = containerMove2.Container;

			var cartageLeg1 = containerMove1.CartageLegs.AddNew();
			var cartageLeg2 = containerMove2.CartageLegs.AddNew();

			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			var looseMove2 = cartage.LooseBookedMoves.AddNew();

			var cartageLeg3 = looseMove1.CartageLegs.AddNew();
			var cartageLeg4 = looseMove2.CartageLegs.AddNew();

			var wrappers = FreightWrapper.New(cartage, cartageLeg1, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			var freightWrapper = wrappers[0];
			AssertEquals("freightWrapper.Containers.Count", 1, freightWrapper.Containers.Count);
			var containerWrapper = freightWrapper.Containers[0];
			AssertEquals("containerWrapper.WrappedObjectPK", container1.PK, containerWrapper.WrappedObjectPK);
			AssertEquals("freightWrapper.Packages.Count", 0, freightWrapper.Packages.Count);

			wrappers = FreightWrapper.New(cartage, cartageLeg2, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			freightWrapper = wrappers[0];
			AssertEquals("freightWrapper.Containers.Count", 1, freightWrapper.Containers.Count);
			containerWrapper = freightWrapper.Containers[0];
			AssertEquals("containerWrapper.WrappedObjectPK", container2.PK, containerWrapper.WrappedObjectPK);
			AssertEquals("freightWrapper.Packages.Count", 0, freightWrapper.Packages.Count);

			wrappers = FreightWrapper.New(cartage, cartageLeg3, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			freightWrapper = wrappers[0];
			AssertEquals("freightWrapper.Containers.Count", 1, freightWrapper.Packages.Count);
			var packageWrapper = freightWrapper.Packages[0];
			AssertEquals("packageWrapper.WrappedObjectPK", looseMove1.PK, packageWrapper.WrappedObjectPK);
			AssertEquals("freightWrapper.Containers.Count", 0, freightWrapper.Containers.Count);

			wrappers = FreightWrapper.New(cartage, cartageLeg4, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			freightWrapper = wrappers[0];
			AssertEquals("freightWrapper.Containers.Count", 1, freightWrapper.Packages.Count);
			packageWrapper = freightWrapper.Packages[0];
			AssertEquals("packageWrapper.WrappedObjectPK", looseMove2.PK, packageWrapper.WrappedObjectPK);
			AssertEquals("freightWrapper.Containers.Count", 0, freightWrapper.Containers.Count);

			wrappers = FreightWrapper.New(cartage, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			freightWrapper = wrappers[0];
			AssertEquals("freightWrapper.Containers.Count", 2, freightWrapper.Containers.Count);
			AssertEquals("freightWrapper.Packages.Count", 2, freightWrapper.Packages.Count);
		}

		#endregion

		#region TestFreightWrapperWithUnrelatedPacklinesForContainer

		public void TestFreightWrapperWithUnrelatedPacklinesForContainer()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			var shipment1 = consol.Shipments.AddNew();
			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 10;
			packline1.JL_JC = container1.PK;

			var shipment2 = consol.Shipments.AddNew();
			var packline2 = shipment2.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 12;
			packline2.JL_JC = container1.PK;

			var packline3 = shipment2.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 12;
			packline3.JL_JC = container2.PK;

			var wrappers = FreightWrapper.New(consol, container1, Factory);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			AssertEquals("wrappers.Length", 1, wrappers[0].Containers.Count);
			AssertEquals("wrappers.Length", 2, wrappers[0].Packages.Count);

			wrappers = FreightWrapper.New(consol, container2, Factory);
			AssertEquals("wrappers.Length", 1, wrappers[0].Containers.Count);
			AssertEquals("wrappers.Length", 1, wrappers[0].Packages.Count);

			wrappers = FreightWrapper.New(consol, container3, Factory);
			AssertEquals("wrappers.Length", 1, wrappers[0].Containers.Count);
			AssertEquals("wrappers.Length", 0, wrappers[0].Packages.Count);
		}

		#endregion

		#region TestFreightWrapperNewFromARCreditNote

		public void TestFreightWrapperNewFromARCreditNote()
		{
			ARCreditNote invoice = Factory.New<ARCreditNote>();
			FreightWrapper wrapper = FreightWrapper.New(invoice, Factory)[0];

			AssertEquals(typeof(FreightWrapperFromInvoice), wrapper.GetType());
			AssertEquals(invoice, wrapper.WrappedObject);
			AssertEquals(invoice, wrapper.ARInvoice.WrappedObject);
			AssertEquals(invoice, ((IBODocDataProvider)wrapper).BusinessObjectToLogAgainst);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Job header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "00001000";
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.AH_JH = header.PK;
			Factory.Save();
			wrapper = FreightWrapper.New(invoice, Factory)[0];

			AssertEquals(typeof(FreightWrapperFromShipment), wrapper.GetType());
			AssertEquals(shipment, wrapper.FreightShipment);
			AssertEquals(invoice, wrapper.ARInvoice.WrappedObject);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "L00001000";
			invoice.AH_JH = ZGuid.Empty;
			invoice.AH_ConsolidatedInvoiceRef = "L00001000";
			wrapper = FreightWrapper.New(invoice, Factory)[0];

			AssertEquals(typeof(FreightWrapperFromConsol), wrapper.GetType());
			AssertEquals(consol, wrapper.Consol);
			AssertEquals(invoice, wrapper.ARInvoice.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperNewFromARInvoice

		public void TestFreightWrapperNewFromARInvoice()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			FreightWrapper wrapper = FreightWrapper.New(invoice, Factory)[0];

			AssertEquals(typeof(FreightWrapperFromInvoice), wrapper.GetType());
			AssertEquals(invoice, wrapper.WrappedObject);
			AssertEquals(invoice, wrapper.ARInvoice.WrappedObject);
			AssertEquals(invoice, ((IBODocDataProvider)wrapper).BusinessObjectToLogAgainst);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Job header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "00001000";
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.AH_JH = header.PK;
			Factory.Save();
			wrapper = FreightWrapper.New(invoice, Factory)[0];

			AssertEquals(typeof(FreightWrapperFromShipment), wrapper.GetType());
			AssertEquals(shipment, wrapper.FreightShipment);
			AssertEquals(invoice, wrapper.ARInvoice.WrappedObject);
			AssertEquals(invoice, ((IBODocDataProvider)wrapper).BusinessObjectToLogAgainst);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			wrapper = FreightWrapper.New(invoice, Factory)[0];

			Assert(wrapper is FreightWrapperFromDeclaration);
			AssertEquals(invoice, ((IBODocDataProvider)wrapper).BusinessObjectToLogAgainst);

			var dataSource = new DocBuilderDataSource();
			dataSource.Freight = true;
			DocumentsDataRegistry.Instance.DocBuilderDataSource.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dataSource);
			wrapper = FreightWrapper.New(invoice, Factory)[0];
			Assert(wrapper is FreightWrapperFromShipment);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "L00001000";
			invoice.AH_JH = ZGuid.Empty;
			invoice.AH_ConsolidatedInvoiceRef = "L00001000";
			wrapper = FreightWrapper.New(invoice, Factory)[0];

			AssertEquals(typeof(FreightWrapperFromConsol), wrapper.GetType());
			AssertEquals(consol, wrapper.Consol);
			AssertEquals(invoice, wrapper.ARInvoice.WrappedObject);
		}

		public void TestFreightWrapperNewFromARInvoice_StandaloneDeclaration()
		{
			var invoice = Factory.New<ARInvoice>();
			var declaration = Factory.New<BaseJobDeclaration>();
			Job header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "00001000";
			header.JH_ParentID = declaration.PK;
			header.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			invoice.AH_JH = header.PK;
			Factory.Save();
			var wrapper = FreightWrapper.New(invoice, Factory)[0];

			Assert(wrapper is FreightWrapperFromDeclaration);
			AssertEquals(invoice, wrapper.ARInvoice.WrappedObject);
			AssertEquals(invoice, ((IBODocDataProvider)wrapper).BusinessObjectToLogAgainst);
		}

		#endregion

		public void TestFreightWrapperNewFromCashAdvanceRequest()
		{
			var request = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			var wrapper = FreightWrapper.New(request, Factory)[0];
			AssertNull("Expect null since request is not linked to any job", wrapper);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "00001000";
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			request.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			request.CAH_Ledger = LedgerTypes.AccountsReceivable;
			request.CAH_OH_Organization = (new TestObjectCreator(Factory)).AALSHI.PK;
			request.CAH_JH_Job = header.PK;
			request.CAH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			request.CAH_OSAmount = 100m;
			request.CAH_LocalAmount = 100m;
			request.CAH_OSPaidAmount = 0m;
			request.CAH_LocalPaidAmount = 0m;
			request.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			wrapper = FreightWrapper.New(request, Factory)[0];

			AssertEquals(typeof(FreightWrapperFromShipment), wrapper.GetType());
			AssertNotNull(wrapper.GenericTransactionHeader);
			AssertEquals(request, wrapper.GenericTransactionHeader.WrappedObject);
			AssertEquals(typeof(DocCashAdvanceRequestHeader), wrapper.GenericTransactionHeader.HeaderPlugIn.GetType());
		}

		#region TestFreightWrapperForShipmentDeclaration

		public void TestFreightWrapperForShipmentDeclaration()
		{
			var shipment = Factory.New<ForwardingShipmentWithSpecialDeclarationForDocumentsForTest>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			shipment.DeclarationForTest = declaration;

			var wrapper = FreightWrapper.New(shipment, Factory)[0];
			Assert("Should wrap declaration", wrapper is FreightWrapperFromDeclaration);
			Assert(shipment.HasTouchedDeclarationForDocuments);

			var dataSource = new DocBuilderDataSource();
			dataSource.Freight = true;
			DocumentsDataRegistry.Instance.DocBuilderDataSource.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dataSource);
			shipment.HasTouchedDeclarationForDocuments = false;
			wrapper = FreightWrapper.New(shipment, Factory)[0];
			Assert("Should wrap shipment as registry item is indicated", wrapper is FreightWrapperFromShipment);
			Assert("Should not have touched DeclarationForDocuments if rego says don't bother", !shipment.HasTouchedDeclarationForDocuments);
		}

		class ForwardingShipmentWithSpecialDeclarationForDocumentsForTest : ForwardingShipment
		{
			public ForwardingShipmentWithSpecialDeclarationForDocumentsForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public override Integration.Customs.IBaseJobDeclaration DeclarationForDocuments
			{
				get
				{
					HasTouchedDeclarationForDocuments = true;
					return DeclarationForTest;
				}
			}

			public BaseJobDeclaration DeclarationForTest { get; set; }

			public bool HasTouchedDeclarationForDocuments { get; set; }
		}

		#endregion

		#region TestFreightWrapperForHVLVConsignment

		public void TestFreightWrapperForHVLVConsignment()
		{
			var consignment = Factory.New<HVLVConsignment>();

			var wrapper = FreightWrapper.NewFreightWrapper(consignment, Factory);
			AssertType<FreightWrapperFromHVLVConsignment>(wrapper);

			wrapper = FreightWrapper.New(consignment, consignment.Items.AddNew(), Factory).Single();
			AssertType<FreightWrapperFromHVLVConsignment>(wrapper);
		}

		#endregion

		#region TestFreightWrapperForOrganization

		public void TestFreightWrapperForOrganization()
		{
			var orgHeader = Factory.New<OrgHeader>();

			var wrapper = FreightWrapper.NewFreightWrapper(orgHeader, Factory);
			AssertEquals(typeof(FreightWrapperFromOrgBO), wrapper.GetType());
		}

		#endregion

		#region TestFreightWrapperForWhsItemBO

		public void TestFreightWrapperForWhsItemBO()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			var wrapperFoeASN = FreightWrapper.NewFreightWrapper(asn, Factory);
			AssertEquals(typeof(FreightWrapperFromWhsItemReceiveASN), wrapperFoeASN.GetType());
			AssertEquals(asn, wrapperFoeASN.WrappedObject);

			var rcn = Factory.New<WhsItemReceiveConsignment>();
			var wrapperFoeRCN = FreightWrapper.NewFreightWrapper(rcn, Factory);
			AssertEquals(typeof(FreightWrapperFromWhsItemReceiveConsignment), wrapperFoeRCN.GetType());
			AssertEquals(rcn, wrapperFoeRCN.WrappedObject);

			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			var wrapperFoeRTU = FreightWrapper.NewFreightWrapper(rtu, Factory);
			AssertEquals(typeof(FreightWrapperFromWhsItemReceiveTransportationUnit), wrapperFoeRTU.GetType());
			AssertEquals(rtu, wrapperFoeRTU.WrappedObject);

			var dcn = Factory.New<WhsItemDispatchConsignment>();
			var wrapperFoeDCN = FreightWrapper.NewFreightWrapper(dcn, Factory);
			AssertEquals(typeof(FreightWrapperFromWhsItemDispatchConsignment), wrapperFoeDCN.GetType());
			AssertEquals(dcn, wrapperFoeDCN.WrappedObject);

			var dll = Factory.New<WhsItemDispatchLoadList>();
			var wrapperFoeDLL = FreightWrapper.NewFreightWrapper(dll, Factory);
			AssertEquals(typeof(FreightWrapperFromWhsItemDispatchLoadList), wrapperFoeDLL.GetType());
			AssertEquals(dll, wrapperFoeDLL.WrappedObject);

			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			var wrapperFoeDTU = FreightWrapper.NewFreightWrapper(dtu, Factory);
			AssertEquals(typeof(FreightWrapperFromWhsItemDispatchTransportationUnit), wrapperFoeDTU.GetType());
			AssertEquals(dtu, wrapperFoeDTU.WrappedObject);

			var trf = Factory.New<WhsItemTransferHeader>();
			var wrapperFoeTRF = FreightWrapper.NewFreightWrapper(trf, Factory);
			AssertEquals(typeof(FreightWrapperFromWhsItemTransferHeader), wrapperFoeTRF.GetType());
			AssertEquals(trf, wrapperFoeTRF.WrappedObject);

			var hu = Factory.New<PkgHandlingUnit>();
			var wrapperFoeHU = FreightWrapper.NewFreightWrapper(hu, Factory);
			AssertEquals(typeof(FreightWrapperFromPkgHandlingUnit), wrapperFoeHU.GetType());
			AssertEquals(hu, wrapperFoeHU.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperNewFromWhsBO

		public void TestFreightWrapperNewFromWhsBO()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var wrapperForAdjustment = FreightWrapper.New(adjustment, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromWhsBO), wrapperForAdjustment.GetType());
			AssertEquals(adjustment, wrapperForAdjustment.WrappedObject);

			var receive = Factory.New<WhsReceive>();
			var wrapperForReceive = FreightWrapper.New(receive, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromWhsBO), wrapperForReceive.GetType());
			AssertEquals(receive, wrapperForReceive.WrappedObject);

			var order = Factory.New<WhsOrder>();
			var wrapperForOrder = FreightWrapper.New(order, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromWhsBO), wrapperForOrder.GetType());
			AssertEquals(order, wrapperForOrder.WrappedObject);

			var workOrder = Factory.New<WhsWorkOrder>();
			var wrapperForWorkOrder = FreightWrapper.New(workOrder, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromWhsBO), wrapperForWorkOrder.GetType());
			AssertEquals(workOrder, wrapperForWorkOrder.WrappedObject);

			var pick = Factory.New<WhsPick>();
			var wrapperForPick = FreightWrapper.New(pick, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromWhsBO), wrapperForPick.GetType());
			AssertEquals(pick, wrapperForPick.WrappedObject);

			var stocktake = Factory.New<WhsStocktake>();
			var wrapperForStocktake = FreightWrapper.New(stocktake, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromWhsBO), wrapperForStocktake.GetType());
			AssertEquals(stocktake, wrapperForStocktake.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperNewFromPkgPackage

		public void TestFreightWrapperNewFromPkgPackage()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var packageHasPackageJob = packageJob.Packages.AddNew();
			AssertEquals(typeof(PkgPackage), packageHasPackageJob.GetType());

			var packageWrapper = FreightWrapper.New(packageHasPackageJob, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromPkgPackageJob), packageWrapper.GetType());
			AssertEquals(packageJob, packageWrapper.WrappedObject);

			var packageWithoutPackageJob = Factory.New<PkgPackage>();
			var packageWrapper2 = FreightWrapper.New(packageWithoutPackageJob, Factory)[0];
			AssertNull(packageWrapper2);
		}

		#endregion

		#region TestFreightWrapperNewFromPkgPackageJob

		public void TestFreightWrapperNewFromPkgPackageJob()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var packageJobWrapper = FreightWrapper.New(packageJob, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromPkgPackageJob), packageJobWrapper.GetType());
			AssertEquals(packageJob, packageJobWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperNewFromRunSheetBO

		public void TestFreightWrapperNewFromRunSheetBO()
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			var runSheetWrapper = FreightWrapper.New(runSheet, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromRunSheet), runSheetWrapper.GetType());
			AssertEquals(runSheet, runSheetWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperNewFromQuotedBookings

		public void TestFreightWrapperNewFromQuotedBookings()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var bookingForShipment = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			var quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;
			var bookingForQuote = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var bookingWithQuote = QuotedBooking.New(quote.PK, shipment.PK, Factory);

			var wrapperForBookingForShipment = FreightWrapper.NewFreightWrapper(bookingForShipment, Factory);
			var wrapperForBookingForQuote = FreightWrapper.NewFreightWrapper(bookingForQuote, Factory);
			var wrapperForBookingWithQuote = FreightWrapper.NewFreightWrapper(bookingWithQuote, Factory);
			AssertEquals(typeof(FreightWrapperFromQuotedBooking), wrapperForBookingForShipment.GetType());
			AssertEquals(typeof(FreightWrapperFromOneOffQuote), wrapperForBookingForQuote.GetType());
			AssertEquals(typeof(FreightWrapperFromQuotedBooking), wrapperForBookingWithQuote.GetType());
		}

		#endregion

		#region TestFreightWrapperFromDtbBookingConsignment

		public void TestFreightWrapperFromDtbBookingConsignment()
		{
			var consignment = Factory.New<DtbBookingConsignment>();
			var consignmentWrapper = FreightWrapper.New(consignment, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromDtbBookingConsignment), consignmentWrapper.GetType());
			AssertEquals(consignment, consignmentWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperFromConsignment

		public void TestFreightWrapperFromConsignment()
		{
			var consignment = Factory.New<DtbConsignment>();
			var consignmentWrapper = FreightWrapper.New(consignment, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromDtbConsignment), consignmentWrapper.GetType());
			AssertEquals(consignment, consignmentWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperFromDtbLinehaulManifest

		public void TestFreightWrapperFromDtbLinehaulManifest()
		{
			var manifest = Factory.New<DtbLinehaulManifest>();
			var manifestWrapper = FreightWrapper.New(manifest, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromDtbLinehaulManifest), manifestWrapper.GetType());
			AssertEquals(manifest, manifestWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperFromGateTransportCYDetailBO

		public void TestFreightWrapperFromGateTransportCYDetailBO()
		{
			var gateTransport = Factory.New<GateTransport>();
			var gateTransportCYDetail = gateTransport.GateTransportCYDetails.AddNew();

			var gateTransportWrapper = FreightWrapper.New(gateTransportCYDetail, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromGateTransport), gateTransportWrapper.GetType());
			AssertEquals(gateTransport, gateTransportWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperFromReceiveAdviceBO

		public void TestFreightWrapperFromReceiveAdviceBO()
		{
			var cydReceiveAdvice = Factory.New<CYDReceiveAdvice>();

			var cydReceiveAdviceWrapper = FreightWrapper.New(cydReceiveAdvice, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromCYDReceiveAdvice), cydReceiveAdviceWrapper.GetType());
			AssertEquals(cydReceiveAdvice, cydReceiveAdviceWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperFromCYDReleaseAdviceBO

		public void TestFreightWrapperFromCYDReleaseAdviceBO()
		{
			var cydReleaseAdvice = Factory.New<CYDReleaseAdvice>();

			var cydReleaseAdviceWrapper = FreightWrapper.New(cydReleaseAdvice, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromCYDReleaseAdvice), cydReleaseAdviceWrapper.GetType());
			AssertEquals(cydReleaseAdvice, cydReleaseAdviceWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperFromCYDTransportationUnit

		public void TestFreightWrapperFromCYDTransportationUnit()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();

			var freightWrapper = FreightWrapper.New(transportationUnit, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromCYDTransportationUnit), freightWrapper.GetType());
			AssertEquals(transportationUnit, freightWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperFromMNRWorkOrder

		public void TestFreightWrapperFromMNRWorkOrder()
		{
			var workOrderHeader = Factory.New<MNRWorkOrderHeader>();

			var freightWrapper = FreightWrapper.New(workOrderHeader, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromMNRWorkOrder), freightWrapper.GetType());
			AssertEquals(workOrderHeader, freightWrapper.WrappedObject);
		}

		#endregion

		#region TestFreightWrapperFromCYDAdHocServiceOrder

		public void TestFreightWrapperFromCYDAdHocServiceOrder()
		{
			var adHocServiceOrder = Factory.New<CYDAdHocServiceOrder>();

			var freightWrapper = FreightWrapper.New(adHocServiceOrder, Factory)[0];
			AssertEquals(typeof(FreightWrapperFromCYDAdHocServiceOrder), freightWrapper.GetType());
			AssertEquals(adHocServiceOrder, freightWrapper.WrappedObject);
		}

		#endregion

		#region TestStaticNewConstructorReturnsDeclarationStyleWrapperIfShipmentHasDeclarationAttached

		public void TestStaticNewConstructorReturnsDeclarationStyleWrapperIfShipmentHasDeclarationAttached()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			var wrappers = FreightWrapper.New(shipment, Factory);
			AssertNotNull("wrappers should not be null", wrappers);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			var wrapper = wrappers[0];
			AssertEquals("wrapper.GetType()", typeof(FreightWrapperFromDeclaration<,>).MakeGenericType(new[] { declaration.GetType(), ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IDocDeclaration>() }), wrapper.GetType());
		}

		#endregion

		#region TestCargoControlNumbersForCanada

		public void TestCargoControlNumbersForCanada()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			var shipment = Factory.New<ForwardingShipment>();
			cargoControlNumber = shipment.Numbers.AddNew();
			cargoControlNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			previousCargoControlNumber = shipment.Numbers.AddNew();
			previousCargoControlNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			AssertReferenceNumbers("1234 00000001", ZString.Empty, "1234 00000001", ZString.Empty, shipment);

			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			cargoControlNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			previousCargoControlNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var wrappers = FreightWrapper.New(shipment, Factory);
			AssertEquals("Cargo control number should be in correct format", "123400000001", wrappers[0].CargoControlNumberForCanada);
			AssertEquals("Previous Cargo control number should be in correct format", "123400000001", wrappers[0].PreviousCargoControlNumberForCanada);

			AssertReferenceNumbers("081-12345678", "081-12345678", "082-87654321", "082-87654321", shipment);
			AssertReferenceNumbers("081- 12345678", "081-12345678", "082- 87654321", "082-87654321", shipment);
			AssertReferenceNumbers("1234999999-01", "1234999999-01", "1234888888-02", "1234888888-02", shipment);
			AssertReferenceNumbers("1234 999999-01", "1234999999-01", "1234 888888-02", "1234888888-02", shipment);
			AssertReferenceNumbers("1234-999999-01", "1234999999-01", "1234-888888-02", "1234888888-02", shipment);
			AssertReferenceNumbers("1234999999 01", "1234999999 01", "1234-888888#02", "1234888888#02", shipment);
			AssertReferenceNumbers("1234", "1234", "12345", "12345", shipment);
			AssertReferenceNumbers("123-", "123-", "123-", "123-", shipment);
			AssertReferenceNumbers("1234-", "1234", "1234-", "1234", shipment);

			shipment.Numbers.RemoveAll();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			consol.Numbers.Add(cargoControlNumber);
			consol.Numbers.Add(previousCargoControlNumber);

			AssertReferenceNumbers("081-12345678", "081-12345678", "082-87654321", "082-87654321", consol);
			AssertReferenceNumbers("081- 12345678", "081-12345678", "082- 87654321", "082-87654321", consol);
			AssertReferenceNumbers("1234999999-01", "1234999999-01", "1234888888-02", "1234888888-02", consol);
			AssertReferenceNumbers("1234 999999-01", "1234999999-01", "1234 888888-02", "1234888888-02", consol);
			AssertReferenceNumbers("1234-999999-01", "1234999999-01", "1234-888888-02", "1234888888-02", consol);
			AssertReferenceNumbers("1234999999 01", "1234999999 01", "1234-888888#02", "1234888888#02", consol);
			AssertReferenceNumbers("1234", "1234", "ABCDE", "ABCDE", consol);
			AssertReferenceNumbers("123-", "123-", "X23-", "X23-", consol);
			AssertReferenceNumbers("1234-", "1234", "1234-", "1234", consol);
			AssertReferenceNumbers("123--", "123-", "123--", "123-", consol);

			GlbCompany.CurrentCompany.SetCountry(currentCountry);
		}

		CusEntryNumber cargoControlNumber;
		CusEntryNumber previousCargoControlNumber;

		void AssertReferenceNumbers(ZString unformattedCargoControlNo, ZString formattedCargoControlNo,
									ZString unformattedPreviousCargoControlNo, ZString formattedPreviousCargoControlNo,
									BusinessObject objectToWrap)
		{
			cargoControlNumber.CE_EntryNum = unformattedCargoControlNo;
			previousCargoControlNumber.CE_EntryNum = unformattedPreviousCargoControlNo;

			var wrappers = FreightWrapper.New(objectToWrap, Factory);
			AssertEquals("Cargo control number should be in correct format", formattedCargoControlNo, wrappers[0].CargoControlNumberForCanada);
			AssertEquals("Previous Cargo control number should be in correct format", formattedPreviousCargoControlNo, wrappers[0].PreviousCargoControlNumberForCanada);
		}

		#endregion

		#region TestTransportAddressesWithWarehousing

		public void TestTransportAddressesWithWarehousing()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("AAA", Factory);
			var addressWithWarehousing = organisation.MainAddress;
			var addressWithConstraints = organisation.Addresses.AddNew(OrgAddressType.Pickup, true);
			var addressWithNone = organisation.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);

			addressWithWarehousing.OA_DockLeveler = true;
			addressWithConstraints.OA_LoadingUnloadingConstraints = "yo";

			var addressWrappers = new AddressWrapperCollection(organisation, Factory);
			AssertEquals("Should contain the 3 org Addresses", 3, addressWrappers.Count);

			addressWrappers.Add(new AddressWrapper(addressWithWarehousing, ContactType.Consignee, Factory));
			AssertEquals("Should contain 4 Addresses, one duplicate so we can check distinct", 4, addressWrappers.Count);

			var dummy = Factory.New<DummyBusinessObject>();
			var mock = new Mock<FreightWrapper>(dummy, Factory) { CallBase = true };
			mock.Protected().Setup<AddressWrapperCollection>("GetTransportAddresses").Returns(addressWrappers);
			AssertContainsExactElementsInAnyOrder(new[] { addressWithWarehousing, addressWithConstraints }, mock.Object.TransportAddressesWithWarehousing.Cast<AddressWrapper>().Select(a => a.WrappedObject));
			mock.VerifyAll();
		}

		#endregion

		#region TestUnAllocatedMeasures

		public void TestUnAllocatedMeasures()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTCNR";
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TESTCNE";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_ActualWeight = 6000m;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualVolume = 35m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicYards;
			shipment.JS_OuterPacks = 265;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packLineOne = shipment.OuterPackLines.AddNew();
			packLineOne.JL_JC = container.PK;
			packLineOne.JL_PackageCount = 120;
			packLineOne.JL_ActualWeight = 4000;
			packLineOne.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLineOne.JL_ActualVolume = 15m;
			packLineOne.JL_ActualVolumeUQ = Constants.Volume.CubicYards;

			var packLineTwo = shipment.OuterPackLines.AddNew();
			packLineTwo.JL_JC = container.PK;
			packLineTwo.JL_PackageCount = 145;
			packLineTwo.JL_ActualWeight = 2000m;
			packLineTwo.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLineTwo.JL_ActualVolume = 20m;
			packLineTwo.JL_ActualVolumeUQ = Constants.Volume.CubicYards;

			var wrapper = FreightWrapper.New(shipment, Factory);
			AssertEquals(0m, wrapper[0].UnAllocatedWeight);
			AssertEquals(0m, wrapper[0].UnAllocatedVolume);
			AssertEquals(0, wrapper[0].UnAllocatedPackages);

			packLineOne.JL_JC = ZGuid.Empty;
			AssertEquals(4000m, wrapper[0].UnAllocatedWeight);
			AssertEquals(15m, wrapper[0].UnAllocatedVolume);
			AssertEquals(120, wrapper[0].UnAllocatedPackages);

			packLineTwo.JL_JC = ZGuid.Empty;
			AssertEquals(6000m, wrapper[0].UnAllocatedWeight);
			AssertEquals(35m, wrapper[0].UnAllocatedVolume);
			AssertEquals(265, wrapper[0].UnAllocatedPackages);
		}

		public void TestUnAllocatedMeasuresUnitMismatch()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var container1 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 6000m;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 35000m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicDecimetres;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			packLine1.JL_ActualWeight = 1000m;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine1.JL_ActualVolume = 10m;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			packLine2.JL_ActualWeight = 2m;
			packLine2.JL_ActualWeightUQ = Constants.Weight.Tonnes;
			packLine2.JL_ActualVolume = 20m;
			packLine2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			var wrapper = FreightWrapper.New(shipment, Factory);
			AssertEquals("UnAllocatedWeight in KG", 3000m, wrapper[0].UnAllocatedWeight);
			AssertEquals("UnAkkicatedVikyne in M3 ", 5m, wrapper[0].UnAllocatedVolume);
		}

		#endregion

		[SetOrgAllowMixedCase(true)]
		public void TestNotifyPartyWorksForShipmentBOAndDeclarationBO()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var warpper = FreightWrapper.New(shipment, Factory);
			AssertEquals("Nortifyparty.CompanyName is empty.", ZString.Empty, warpper[0].NotifyParty.CompanyName);

			var shipmentNotify = shipment.NotifyPartyDocumentaryAddress;
			shipmentNotify.E2_AddressOverride = true;
			shipmentNotify.E2_CompanyName = "SHIPMENT NOTIFY PARTY";
			warpper = FreightWrapper.New(shipment, Factory);
			AssertEquals("Notifyparty.CompanyName is from Shipment.", "SHIPMENT NOTIFY PARTY", warpper[0].NotifyParty.CompanyName);

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var declarationNotify = declaration.NotifyPartyDocumentaryAddress;
			declarationNotify.E2_AddressOverride = true;
			declarationNotify.E2_CompanyName = "Declaration NOTIFY PARTY";
			warpper = FreightWrapper.New(shipment, Factory);
			AssertEquals("Notifyparty.CompanyName is from Declaration.", "Declaration NOTIFY PARTY", warpper[0].NotifyParty.CompanyName);

			declarationNotify.E2_AddressOverride = false;
			shipmentNotify.E2_AddressOverride = false;
			warpper = FreightWrapper.New(shipment, Factory);
			AssertEquals("Notifyparty.CompanyName is from Shipment.", ZString.Empty, warpper[0].NotifyParty.CompanyName);

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_JS = ZGuid.Empty;
			warpper = FreightWrapper.New(shipment, Factory);
			AssertEquals("Notifyparty.CompanyName is from Declaration.IMPORTER", "IMPORTER", warpper[0].NotifyParty.CompanyName);
		}

		#region TestFreightWrapperNewFromDeclarationWithChildTransport

		public void TestFreightWrapperNewFromDeclarationWithChildTransport()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var transport = declaration.TransportsIncludingRelated.AddNew();
			transport.JW_LegOrder = 1;
			FreightWrapper[] wrappers = FreightWrapper.New(declaration, transport, Factory);

			AssertEquals(1, wrappers.Length);
			AssertEquals(typeof(FreightWrapperFromDeclaration<BaseJobDeclaration, Customs.General.DocDeclaration>), wrappers[0].GetType());
			AssertEquals("WrappedObjectPK", declaration.PK, wrappers[0].WrappedObjectPK);
			AssertEquals("InterestedRoute.LegNo", 1, wrappers[0].InterestedRoute.LegNo);
		}

		#endregion

		public void TestFreightWrapperNewFromCartageLeg()
		{
			var cartage = Factory.New<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			var cartageLeg = containerMove.CartageLegs.AddNew();

			var wrapper = FreightWrapper.NewFreightWrapper(cartageLeg, Factory);
			AssertNull("Does not generate a wrapper", wrapper);
		}

		public void TestReportVisualizerContentShouldNotBeSameWhenDocumentWrapperWithCommonPickupDeliveryConfirmToWrap()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			var consol = Factory.New<ForwardingConsol>();
			shipment.Consols.Add(consol);

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			packLine1.SetContainer(consol, container1);
			packLine2.SetContainer(consol, container2);

			var confirm1 = shipment.PickupConfirms.AddNew();
			var confirm2 = shipment.PickupConfirms.AddNew();

			confirm1.EU_JC = container1.PK;
			confirm2.EU_JC = container2.PK;

			Factory.Save();

			var wrapper1 = FreightWrapper.New(shipment, confirm1, Factory);
			var wrapper2 = FreightWrapper.New(shipment, confirm2, Factory);
			var dataProviderList1 = new DataProviderList(wrapper1);
			var dataProviderList2 = new DataProviderList(wrapper2);
			AssertTwoDifferentReportsByDataProviderList(dataProviderList1, dataProviderList2);
		}

		public void TestReportVisualizerContentShouldNotBeSameWhenDocumentWrapperWithBaseJobDeclarationToWrap()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			Factory.Save();

			var wrapper1 = FreightWrapper.New(declaration, invoice1, Factory);
			var wrapper2 = FreightWrapper.New(declaration, invoice2, Factory);
			var dataProviderList1 = new DataProviderList(wrapper1);
			var dataProviderList2 = new DataProviderList(wrapper2);
			AssertTwoDifferentReportsByDataProviderList(dataProviderList1, dataProviderList2);
		}

		void AssertTwoDifferentReportsByDataProviderList(DataProviderList dataProviderList1, DataProviderList dataProviderList2)
		{
			using (var pack = new DocumentEngine.DocumentPack(Factory.New<DocumentEngine.DocumentCommand>()))
			using (var report1 = new DocumentEngine.Report(pack, null, dataProviderList1, "report1", null, DocumentDirection.ANY, false))
			using (var report2 = new DocumentEngine.Report(pack, null, dataProviderList2, "report2", null, DocumentDirection.ANY, false))
			{
				AssertNotEquals("report1's visualizer content should not be same as report2's.", report1.VisualizerContentNote.PK, report2.VisualizerContentNote.PK);
			}
		}

		class MockVisualizerNotSupporter : NonPersistentBusinessObject, DocumentEngineIntegration.IVisualizerNoteSupporter
		{
			public MockVisualizerNotSupporter(BusinessObject parentBusinessObject)
			{
				this.parentBusinessObject = parentBusinessObject;
			}

			readonly BusinessObject parentBusinessObject;

			ZGuid DocumentEngineIntegration.IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.BrettsGuid;

			ZGuid DocumentEngineIntegration.IVisualizerNoteSupporter.PK => parentBusinessObject.PK;

			string DocumentEngineIntegration.IVisualizerNoteSupporter.TableCode => "MNT";
		}

		public void TestReportGetVisualizerNote()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			var consol = Factory.New<ForwardingConsol>();
			shipment.Consols.Add(consol);

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			packLine1.SetContainer(consol, container1);
			packLine2.SetContainer(consol, container2);

			var confirm1 = shipment.PickupConfirms.AddNew();
			var confirm2 = shipment.PickupConfirms.AddNew();

			confirm1.EU_JC = container1.PK;
			confirm2.EU_JC = container2.PK;

			var stmMenuItem = Factory.New<DocumentEngine.DocumentCommand>();
			var visualizeNote1 = Factory.NewWithValidTestData<VisualizerNote>();
			visualizeNote1.DD_ParentID = shipment.PK;
			visualizeNote1.DD_ParentRelatedID = container1.PK;
			visualizeNote1.DD_SU = stmMenuItem.PK;
			visualizeNote1.DD_DocumentData = ZBlob.FromAscii("abc");
			visualizeNote1.DD_ParentTableCode = "JS";

			var visualizeNote2 = Factory.NewWithValidTestData<VisualizerNote>();
			visualizeNote2.DD_ParentID = shipment.PK;
			visualizeNote2.DD_ParentRelatedID = ZGuid.BrettsGuid;
			visualizeNote2.DD_SU = stmMenuItem.PK;
			visualizeNote2.DD_DocumentData = ZBlob.FromAscii("abc");
			visualizeNote2.DD_ParentTableCode = "JS";

			var visualizeNote3 = Factory.NewWithValidTestData<VisualizerNote>();
			visualizeNote3.DD_ParentID = shipment.PK;
			visualizeNote3.DD_ParentRelatedID = Guid.Empty;
			visualizeNote3.DD_SU = stmMenuItem.PK;
			visualizeNote3.DD_DocumentData = ZBlob.FromAscii("abc");
			visualizeNote3.DD_ParentTableCode = "JS";

			Factory.Save();

			var wrapper = FreightWrapper.New(shipment, confirm1, Factory);
			var dataProviderList = new DataProviderList(wrapper);
			using (var pack = new DocumentEngine.DocumentPack(stmMenuItem))
			using (var report = new DocumentEngine.Report(pack, null, dataProviderList, "report1", null, DocumentDirection.ANY, false))
			{
				AssertEquals(visualizeNote1.PK, report.VisualizerContentNote.PK);
			}

			using (var pack = new DocumentEngine.DocumentPack(stmMenuItem))
			using (var report = new DocumentEngine.Report(pack, null, BODocDataProvider.Get(new MockVisualizerNotSupporter(shipment)), "report2", null, DocumentDirection.ANY, false))
			{
				AssertEquals(visualizeNote2.PK, report.VisualizerContentNote.PK);
			}

			using (var pack = new DocumentEngine.DocumentPack(stmMenuItem))
			using (var report = new DocumentEngine.Report(pack, null, BODocDataProvider.Get(shipment), "report3", null, DocumentDirection.ANY, false))
			{
				AssertEquals(visualizeNote3.PK, report.VisualizerContentNote.PK);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestIncotermMappings()
		{
			var shipment = Factory.New<ForwardingShipment>();

			foreach (var mapping in incotermCodeMappings)
			{
				shipment.JS_INCO = mapping.Key;
				var wrapper = new FreightWrapperFromShipment(shipment, Factory);

				AssertEquals($"Incoterm '{mapping.Key}' should be mapped to '{mapping.Value.Code}' on the freight wrapper.", mapping.Value.Code, wrapper.IncoTerm.Code);
				AssertEquals($"Incoterm '{mapping.Key}' should have its description mapped to '{mapping.Value.Description}' on the freight wrapper.", mapping.Value.Description, wrapper.IncoTerm.Description);
			}
		}

		readonly Dictionary<string, (string Code, string Description)> incotermCodeMappings = new Dictionary<string, (string, string)>
		{
			{ Constants.IncoTerms.FreeCarrierSeller, (Constants.IncoTerms.FreeCarrier, "Free Carrier") },
			{ Constants.IncoTerms.FreeCarrierBuyer, (Constants.IncoTerms.FreeCarrier, "Free Carrier") }
		};

		public void TestDepartureOrArrivalTextUntranslatedMacroShouldNotBeTranslated()
		{
			var templateContent =
				@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=Test Report]
{A}-[#SectionBody]
{B}-[<DepartureOrArrivalText>]
{B}-[<DepartureOrArrivalTextUntranslated>]
{A}-[#EndOfReport]";

			var shipment = Factory.New<ForwardingShipment>();
			var wrapper = new FreightWrapperFromShipment(shipment, Factory);

			using (var excelInterface = new ExcelInterface())
			using (var templateStream = new MemoryStream())
			{
				excelInterface.NewExcelFile(1);
				excelInterface.ActiveWorksheet = 0;
				DocumentEngineTestHelper.GenerateExcelWorkSheetFromString(excelInterface.WorkSheets[0], templateContent);
				excelInterface.Xls.SheetName = "Document";
				excelInterface.SaveToStream(templateStream);

				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);
				using (var resourceStrings = Res.GetLanguageInstance(SharedConstants.Languages.ChineseSimplified).UseMockData())
				{
					resourceStrings.Put("FreightWrapper|Arrival", new ResourceStringData("", "到达", "", "到达", ""));

					using (var documentPack = new DocumentEngine.DocumentPack())
					using (var report = new DocumentEngine.Report(documentPack, excelTemplate, wrapper, "report", null, DocumentDirection.ARV, false))
					using (var outputStream = new MemoryStream())
					{
						documentPack.Language = SharedConstants.Languages.ChineseSimplified;
						report.Save(outputStream);
						using (var xlInterface = new ExcelInterface())
						{
							xlInterface.LoadExcelFile(outputStream);
							var workSheet = xlInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", "{B}-[到达]\r\n{B}-[Arrival]", workSheet.ToString());
						}
					}
				}
			}
		}
	}
}
