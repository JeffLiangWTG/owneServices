using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;
using JobCharge = Enterprise.MasterFiles.Business.JobCharge;
using WeightVolumeDisplayTypes = Enterprise.Core.WeightAndVolumeDisplayTypes;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocShipment))]
	public class DocShipmentTest : ZArchitecture.Business.Testing.DocumentWrapperTestCase
	{
		public void TestINCOTermForFCA()
		{
			foreach (var inco in Constants.IncoTerms.Incoterms2020)
			{
				Shipment.JS_INCO = inco;

				var shipmentWrapper = DocShipment.New(Shipment, Factory);

				if (inco == Constants.IncoTerms.FreeCarrierBuyer || inco == Constants.IncoTerms.FreeCarrierSeller)
				{
					AssertEquals("FC1 and FC2 should be mapped to FCA when displayed on doucment(Code)", Constants.IncoTerms.FreeCarrier, shipmentWrapper.INCO);
					AssertEquals("FC1 and FC2 should be mapped to FCA when displayed on doucment(Description)", Constants.IncoTerms.Descriptions.FreeCarrier, shipmentWrapper.INCODescription);
				}
				else
				{
					AssertEquals("Official Incoterm other than FC1 and FC2 should be mapped as it is(Code)", inco, shipmentWrapper.INCO);
					AssertEquals("Official Incoterm other than FC1 and FC2 should be mapped as it is(Description)", Constants.IncoTerms.Descriptions.DefaultCodeDescriptionPairs[inco], shipmentWrapper.INCODescription);
				}
			}
		}

		public void TestColoadMasterShipment()
		{
			var masterShipment = Factory.New<CommonShipment>();
			var subShipment = Factory.New<CommonShipment>();
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			var subShipmentWrapper = DocShipment.New(subShipment, Factory);
			var masterShipmentWrapper = subShipmentWrapper.ColoadMasterShipment;
			AssertNotNull("Precondition", masterShipmentWrapper);
			AssertType<CommonShipment>(masterShipmentWrapper.WrappedObject);
			AssertEquals(masterShipment.PK, ((CommonShipment)masterShipmentWrapper.WrappedObject).PK);
		}

		public void TestDirectShipment_WithPreCarriageOnForwardingConsols()
		{
			var today = ZDateTime.Today;
			var agentPreCarriageConsol = CreateConsol("AGT Pre-carriage", "SEA", "FCL", "AGT", "USCHI", "USLAX", today, today.AddDays(1));
			agentPreCarriageConsol.JK_MasterBillNum = "1111";
			var directDepartureConsol = CreateConsol("DRT Departure", "SEA", "FCL", "DRT", "USLAX", "SGSIN", today.AddDays(2), today.AddDays(3));
			directDepartureConsol.JK_MasterBillNum = "2222";
			var directArrivalConsol = CreateConsol("DRT Arrival", "SEA", "FCL", "DRT", "SGSIN", "AUSYD", today.AddDays(4), today.AddDays((5)));
			directArrivalConsol.JK_MasterBillNum = "3333";
			var agentOnForwardingConsol = CreateConsol("AGT On-forwarding", "SEA", "FCL", "AGT", "AUSYD", "AUMEL", today.AddDays(6), today.AddDays(7));
			agentOnForwardingConsol.JK_MasterBillNum = "4444";

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_UniqueConsignRef = "DRT shipment";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			shipment.Consols.AddRange(agentPreCarriageConsol, directDepartureConsol, directArrivalConsol, agentOnForwardingConsol);

			Factory.Save();

			var shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("Departure Consol should be Direct", "DRT Departure", shipmentWrapper.DepartureCommonConsol.JK_UniqueConsignRef);
			AssertEquals("Arrival Consol should be Direct", "DRT Arrival", shipmentWrapper.ArrivalCommonConsol.JK_UniqueConsignRef);

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Bill of ladding should be Departure Consol's BOL", directDepartureConsol.JK_MasterBillNum, shipmentWrapper.MasterBillNumber);

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Bill of ladding should be Arrival Consol's BOL", directArrivalConsol.JK_MasterBillNum, shipmentWrapper.MasterBillNumber);

			var collection = shipmentWrapper.CompleteRouting;
			CombineAssertions("Routing should include legs from pre-carriage and on-forwarding consol", () =>
			{
				AssertEquals(4, collection.Count);
				AssertEquals("USCHI", collection[0].PortOfLoading.Code);
				AssertEquals("USLAX", collection[0].PortOfDischarge.Code);
				AssertEquals("USLAX", collection[1].PortOfLoading.Code);
				AssertEquals("SGSIN", collection[1].PortOfDischarge.Code);
				AssertEquals("SGSIN", collection[2].PortOfLoading.Code);
				AssertEquals("AUSYD", collection[2].PortOfDischarge.Code);
				AssertEquals("AUSYD", collection[3].PortOfLoading.Code);
				AssertEquals("AUMEL", collection[3].PortOfDischarge.Code);
			});
		}

		public void TestColoadMasterShipment_DepartureConsol()
		{
			var masterConsolSEA = CreateConsol("C00010001", "SEA", "GRP", "USLAX", "AUMEL");
			var masterShipment = CreateShipment(masterConsolSEA, "S00010001", "CLD", "SEA", "LCL", "USLAX", "AUMEL");
			var subShipment1 = CreateShipment(masterConsolSEA, "S00010002", "STD", "SEA", "LCL", "USLAX", "AUMEL");
			var subShipment2 = CreateShipment(masterConsolSEA, "S00010003", "STD", "SEA", "LCL", "USLAX", "AUSYD");
			var subShipment3 = CreateShipment(masterConsolSEA, "S00010004", "STD", "SEA", "LCL", "USLAX", "AUSYD");
			var subShipment4 = CreateShipment(masterConsolSEA, "S00010005", "STD", "SEA", "LCL", "USLAX", "AUSYD");

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment3.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment4.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var subShipmentConsol1 = CreateConsol("C00010002", "RAI", "STD", "USCHI", "USLAX");
			subShipment1.Consols.Add(subShipmentConsol1);

			var subShipmentConsol2 = CreateConsol("C00010003", "RAI", "STD", "USCHI", "USLAX");
			subShipment2.Consols.Add(subShipmentConsol2);
			subShipment3.Consols.Add(subShipmentConsol2);

			var subShipmentConsol3 = CreateConsol("C00010004", "RAI", "STD", "USCHI", "USLAX");
			subShipment4.Consols.Add(subShipmentConsol3);

			Factory.Save();

			var masterShipmentWrapper = DocShipment.New(masterShipment, Factory);
			var subShipmentWrapper1 = DocShipment.New(subShipment1, Factory);
			var subShipmentWrapper2 = DocShipment.New(subShipment2, Factory);
			var subShipmentWrapper3 = DocShipment.New(subShipment3, Factory);
			var subShipmentWrapper4 = DocShipment.New(subShipment4, Factory);

			AssertEquals("SEA Shipment, Departure Consol should be SEA", "C00010001", masterShipmentWrapper.DepartureCommonConsol.JK_UniqueConsignRef);
			AssertEquals("SEA Shipment, Departure Consol should be SEA", "C00010001", subShipmentWrapper1.DepartureCommonConsol.JK_UniqueConsignRef);
			AssertEquals("SEA Shipment, Departure Consol should be SEA", "C00010001", subShipmentWrapper2.DepartureCommonConsol.JK_UniqueConsignRef);
			AssertEquals("SEA Shipment, Departure Consol should be SEA", "C00010001", subShipmentWrapper3.DepartureCommonConsol.JK_UniqueConsignRef);
			AssertEquals("SEA Shipment, Departure Consol should be SEA", "C00010001", subShipmentWrapper4.DepartureCommonConsol.JK_UniqueConsignRef);

			subShipment1.JS_TransportMode = "RAI";
			subShipmentWrapper1 = DocShipment.New(subShipment1, Factory);
			AssertEquals("RAI Shipment, Departure Consol should be RAI Consol", "C00010002", subShipmentWrapper1.DepartureCommonConsol.JK_UniqueConsignRef);
			AssertEquals("RAI Consol", Core.Constants.TransportModes.Rail, subShipmentWrapper1.DepartureCommonConsol.JK_TransportMode);

			subShipment2.JS_TransportMode = "ROA";
			subShipmentWrapper2 = DocShipment.New(subShipment2, Factory);
			AssertEquals("ROA Shipment, no matched consol, Departure Consol is the First Consol", "C00010001", subShipmentWrapper2.DepartureCommonConsol.JK_UniqueConsignRef);
			AssertEquals("RAI Consol", Constants.TransportModes.Sea, subShipmentWrapper2.DepartureCommonConsol.JK_TransportMode);

			subShipment4.JS_TransportMode = "AIR";
			subShipmentWrapper4 = DocShipment.New(subShipment4, Factory);
			AssertEquals("AIR Shipment, no matched consol, Departure Consol is the First Consol", "C00010001", subShipmentWrapper4.DepartureCommonConsol.JK_UniqueConsignRef);
		}

		public void TestConsolIsDifferentForBillOfLading()
		{
			ShipmentWrapper.CommonShipment.JS_TransportMode = "SEA";

			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = "AIR";
			consol.JK_UniqueConsignRef = "C0001111";

			CommonConsol consol2 = Shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "USLAX";
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.JK_TransportMode = "SEA";
			consol2.JK_UniqueConsignRef = "C0002222";

			AssertEquals(consol.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);

			var documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.Shipment, true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);

			AssertEquals(consol2.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);
		}

		public void TestInsuranceCurrencyTest()
		{
			Shipment.JS_RX_NKInsuranceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals("Should have the same currency", Core.Constants.CurrencyCodes.UnitedKingdom, ShipmentWrapper.InsuranceCurrency.Code);
		}

		public void TestLineOrg()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test Organisation Name1";
			org2.OH_FullName = "Test Organisation Name2";

			Factory.Save();

			AssertNull(ShipmentWrapper.LineOrg);

			CommonConsol consol = CreateImportConsol(Shipment);
			CreateASailing();
			Transport transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;

			Shipment.Sailing.Voyage.JV_OH_Line = org1.PK;
			AssertEquals("Test Organisation Name1", ShipmentWrapper.LineOrg.Name);

			Shipment.JS_OA_BookedShippingLineAddress = org2.MainAddress.PK;
			AssertEquals("Test Organisation Name2", ShipmentWrapper.LineOrg.Name);
		}

		public void TestNumbersCollectionIsProxiedFromShipment()
		{
			CusEntryNumber entryNumber = Shipment.Numbers.AddNew();
			entryNumber.CE_EntryNum = "LALALAND";
			entryNumber.CE_EntryType = "WOW";
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var numberWrappers = ShipmentWrapper.Numbers as IBODocDataProviderCollection;
			CusEntryNumber entryNumberReadBackFromTheDepths = (CusEntryNumber)BODocDataProvider.GetBusinessObject(numberWrappers["WOW"]);
			AssertEquals(entryNumber, entryNumberReadBackFromTheDepths);
			CusEntryNumber bunkusEntryNumber = (CusEntryNumber)numberWrappers["DUD"];
			AssertNull(bunkusEntryNumber);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocShipment.New(Shipment, Factory),
				DocShipment.New(Factory, Shipment.PK)
			};
		}

		#region TestShowConsignorConsignee

		public void TestShowConsignorConsignee()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			DocShipment docShipment = DocShipment.New(shipment, Factory);
			Assert(!docShipment.ShowConsignorConsignee);

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsConsignee = true;
			shipment.ConsigneePK = orgHeader.PK;

			Assert(docShipment.ShowConsignorConsignee);
		}

		#endregion

		public void TestIsRoad()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Assert(!ShipmentWrapper.IsRoad);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			Assert(ShipmentWrapper.IsRoad);
		}

		public void TestShowChargeable()
		{
			AssertEquals(true, ShipmentWrapper.ShowChargeable);
		}

		public void TestContainerPackModeOverride()
		{
			Shipment.JS_HBLContainerPackModeOverride = "BLAH!!!";
			AssertEquals("BLAH!!!", ShipmentWrapper.ContainerPackModeOverride);
		}

		public void TestDangerousGoodsStatement()
		{
			DocumentsDataRegistry.Instance.DangerousGoodsStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "111");
			AssertEquals("111", ShipmentWrapper.DangerousGoodsStatement);
			DocumentsDataRegistry.Instance.DangerousGoodsStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "222");
			AssertEquals("222", ShipmentWrapper.DangerousGoodsStatement);
		}

		public void TestHasDangerousGoods()
		{
			AssertEquals("Precondition: No OuterPackLines", 0, Shipment.OuterPackLines.Count);
			AssertEquals(false, ShipmentWrapper.HasDangerousGoods);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();

			AssertEquals(false, ShipmentWrapper.HasDangerousGoods);

			Shipment.OuterPackLines[2].UNDGs.AddNew().DI_DG = Factory.NewWithValidTestData<UNDGSubstance>().PK;

			AssertEquals(true, ShipmentWrapper.HasDangerousGoods);
		}

		#region HAWB Terms

		public void TestHAWBTerms()
		{
			DocumentsDataRegistry.Instance.DomesticHAWBTerms.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZString.Empty);
			DocumentsDataRegistry.Instance.BookingInternationalHAWBTerms.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZString.Empty);
			AssertEquals(ZString.Empty, ShipmentWrapper.HAWBTerms);

			DocumentsDataRegistry.Instance.DomesticHAWBTerms.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HAWB Terms Test 1");
			DocumentsDataRegistry.Instance.BookingInternationalHAWBTerms.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "INT TERMS 1");
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + "XYZ";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + "XYZ";
			AssertEquals("HAWB Terms Test 1", ShipmentWrapper.HAWBTerms);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "INBOM";
			AssertEquals("INT TERMS 1", ShipmentWrapper.HAWBTerms);
		}

		#endregion

		#region Container Legs

		public void TestDeliveryLegsSorting()
		{
			PackLine packline = Shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm leg1 = Shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm leg2 = Shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm leg3 = Shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm leg4 = Shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm leg5 = Shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm leg6 = Shipment.DeliveryConfirms.AddNew();

			leg1.EU_PickupDeliveryTime = ZDateTime.Now.Date;
			leg2.EU_PickupDeliveryTime = ZDateTime.Now.AddDays(-2).Date;
			leg3.EU_PickupDeliveryTime = ZDateTime.Now.AddDays(+2).Date;
			CommonConfirmDivot newDivot = leg3.GetDivot(packline);
			newDivot.J8_PackagesDelivered = 3;
			leg4.EU_PickupDeliveryTime = ZDateTime.Now.AddDays(+2).Date;
			newDivot = leg4.GetDivot(packline);
			newDivot.J8_PackagesDelivered = 2;
			leg5.EU_PickupDeliveryTime = ZDateTime.Now.AddDays(+4).Date;
			newDivot = leg5.GetDivot(packline);
			newDivot.J8_PackagesDelivered = 2;
			leg5.EU_GoodsSignForBy = "BLAHB";
			leg6.EU_PickupDeliveryTime = ZDateTime.Now.AddDays(+4).Date;
			newDivot = leg6.GetDivot(packline);
			newDivot.J8_PackagesDelivered = 2;
			leg6.EU_GoodsSignForBy = "BLAHA";

			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals(6, ShipmentWrapper.DeliveryLegs.Count);

			AssertEquals("Leg2 should be on the first place", leg2.PK, ((CommonPickupDeliveryConfirm)ShipmentWrapper.DeliveryLegs[0].WrappedObject).PK);
			AssertEquals("Leg1 should be on the second place", leg1.PK, ((CommonPickupDeliveryConfirm)ShipmentWrapper.DeliveryLegs[1].WrappedObject).PK);
			AssertEquals("Leg4 should be on the third place", leg4.PK, ((CommonPickupDeliveryConfirm)ShipmentWrapper.DeliveryLegs[2].WrappedObject).PK);
			AssertEquals("Leg3 should be on the fourth place", leg3.PK, ((CommonPickupDeliveryConfirm)ShipmentWrapper.DeliveryLegs[3].WrappedObject).PK);
			AssertEquals("Leg6 should be on the fifth place", leg6.PK, ((CommonPickupDeliveryConfirm)ShipmentWrapper.DeliveryLegs[4].WrappedObject).PK);
			AssertEquals("Leg5 should be on the last place", leg5.PK, ((CommonPickupDeliveryConfirm)ShipmentWrapper.DeliveryLegs[5].WrappedObject).PK);
		}

		#endregion

		public void TestPickUpFromDate()
		{
			AssertNotNull("Precondition: Docs and Cartage reference should not be null", Shipment.DocsAndCartage);
			ZDateTime date = ZDateTime.Now;
			Shipment.DocsAndCartage.JP_EstimatedPickup = date;
			AssertEquals(date, ShipmentWrapper.PickUpFromDate);
		}

		public void TestIsDangerousGoodsShipment()
		{
			AssertEquals("Precondition: Should have no Outer pack lines", 0, Shipment.OuterPackLines.Count);
			AssertEquals(ZBool.False, ShipmentWrapper.IsDangerousGoodsShipment);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertEquals("Precondition: Should have three Outer pack lines", 3, Shipment.OuterPackLines.Count);

			AssertEquals(ZBool.False, ShipmentWrapper.IsDangerousGoodsShipment);

			var uNDG1 = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			Shipment.OuterPackLines[0].UNDGs.AddNew().DI_DG = uNDG1.PK;
			AssertEquals(ZBool.True, ShipmentWrapper.IsDangerousGoodsShipment);

			var uNDG2 = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			Shipment.OuterPackLines[1].UNDGs.AddNew().DI_DG = uNDG2.PK;
			AssertEquals(ZBool.True, ShipmentWrapper.IsDangerousGoodsShipment);

			var uNDG3 = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			Shipment.OuterPackLines[2].UNDGs.AddNew().DI_DG = uNDG3.PK;
			AssertEquals(ZBool.True, ShipmentWrapper.IsDangerousGoodsShipment);
		}

		public void TestIsDomesticThirdParty()
		{
			Shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectThirdParty;
			AssertEquals(ZBool.True, ShipmentWrapper.IsDomesticThirdParty);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticThirdParty);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectCOD;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticThirdParty);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticThirdParty);
		}

		public void TestIsDomesticPrepaid()
		{
			Shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals(ZBool.True, ShipmentWrapper.IsDomesticPrepaid);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticPrepaid);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectCOD;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticPrepaid);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectThirdParty;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticPrepaid);
		}

		public void TestIsDomesticCollect()
		{
			Shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			AssertEquals(ZBool.True, ShipmentWrapper.IsDomesticCollect);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticCollect);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectCOD;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticCollect);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectThirdParty;
			AssertEquals(ZBool.False, ShipmentWrapper.IsDomesticCollect);
		}

		public void TestFCCODAmount()
		{
			AssertNull("Precondition: Job reference should return null", ShipmentWrapper.Job);
			AssertEquals(ZDecimal.Zero, ShipmentWrapper.FCCODAmount);

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = Shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			job.Charges.RemoveAll();
			AssertEquals("Precondition: Job JH_TotalRevenue incorrect", 0m, job.JH_TotalRevenue);
			AssertEquals(0m, ShipmentWrapper.FCCODAmount);

			AddCharge(job, 10m);
			AssertEquals("Precondition: Job JH_TotalRevenue incorrect", 10m, job.JH_TotalRevenue);
			AssertEquals(10m, ShipmentWrapper.FCCODAmount);

			AddCharge(job, 10m);
			AssertEquals("Precondition: Job JH_TotalRevenue incorrect", 20m, job.JH_TotalRevenue);
			AssertEquals(20m, ShipmentWrapper.FCCODAmount);
		}

		void AddCharge(Job job, ZDecimal revenueAmount)
		{
			Charge aCharge = job.Charges.AddNew();
			aCharge.JR_LocalSellAmt = revenueAmount;
		}

		public void TestShipperCODAmount()
		{
			Shipment.JS_ShipperCODAmount = ZDecimal.Zero;
			AssertEquals(ZDecimal.Zero, Shipment.JS_ShipperCODAmount);

			Shipment.JS_ShipperCODAmount = 10m;
			AssertEquals(10m, Shipment.JS_ShipperCODAmount);

			Shipment.JS_ShipperCODAmount = 20m;
			AssertEquals(20m, Shipment.JS_ShipperCODAmount);
		}

		public void TestOuterPack1()
		{
			AssertEquals(0, Shipment.OuterPackLines.Count);
			AssertNull(ShipmentWrapper.OuterPack1);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertNotNull(ShipmentWrapper.OuterPack1);
			AssertEquals(Shipment.OuterPackLines[0], (PackLine)ShipmentWrapper.OuterPack1.WrappedObject);
		}

		public void TestOuterPack2()
		{
			AssertEquals(0, Shipment.OuterPackLines.Count);
			AssertNull(ShipmentWrapper.OuterPack2);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack2);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertNotNull(ShipmentWrapper.OuterPack2);
			AssertEquals(Shipment.OuterPackLines[1], (PackLine)ShipmentWrapper.OuterPack2.WrappedObject);
		}

		public void TestOuterPack3()
		{
			AssertEquals(0, Shipment.OuterPackLines.Count);
			AssertNull(ShipmentWrapper.OuterPack3);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack3);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack3);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertNotNull(ShipmentWrapper.OuterPack3);
			AssertEquals(Shipment.OuterPackLines[2], (PackLine)ShipmentWrapper.OuterPack3.WrappedObject);
		}

		public void TestOuterPack4()
		{
			AssertEquals(0, Shipment.OuterPackLines.Count);
			AssertNull(ShipmentWrapper.OuterPack4);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack4);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack4);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack4);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertNotNull(ShipmentWrapper.OuterPack4);
			AssertEquals(Shipment.OuterPackLines[3], (PackLine)ShipmentWrapper.OuterPack4.WrappedObject);
		}

		public void TestOuterPack5()
		{
			AssertEquals(0, Shipment.OuterPackLines.Count);
			AssertNull(ShipmentWrapper.OuterPack5);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack5);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack5);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack5);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertNotNull(ShipmentWrapper.OuterPack5);
			AssertEquals(Shipment.OuterPackLines[4], (PackLine)ShipmentWrapper.OuterPack5.WrappedObject);
		}

		public void TestOuterPack6()
		{
			AssertEquals(0, Shipment.OuterPackLines.Count);
			AssertNull(ShipmentWrapper.OuterPack6);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack6);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack6);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack6);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertNotNull(ShipmentWrapper.OuterPack6);
			AssertEquals(Shipment.OuterPackLines[5], (PackLine)ShipmentWrapper.OuterPack6.WrappedObject);
		}

		public void TestOuterPack7()
		{
			AssertEquals(0, Shipment.OuterPackLines.Count);
			AssertNull(ShipmentWrapper.OuterPack7);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack7);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack7);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack7);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertNotNull(ShipmentWrapper.OuterPack7);
			AssertEquals(Shipment.OuterPackLines[6], (PackLine)ShipmentWrapper.OuterPack7.WrappedObject);
		}

		public void TestOuterPack8()
		{
			AssertEquals(0, Shipment.OuterPackLines.Count);
			AssertNull(ShipmentWrapper.OuterPack8);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack8);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack8);

			Shipment.OuterPackLines.AddNew();
			AssertNull(ShipmentWrapper.OuterPack8);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();
			AssertNotNull(ShipmentWrapper.OuterPack8);
			AssertEquals(Shipment.OuterPackLines[7], (PackLine)ShipmentWrapper.OuterPack8.WrappedObject);
		}

		public void TestMilestones()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ProcessTask pT1 = shipment.WorkflowItems.MilestonesIncludingRelated.AddNew();
			ProcessTask pT2 = shipment.WorkflowItems.MilestonesIncludingRelated.AddNew();
			var result = DocShipment.New(shipment, shipment.Factory);
			AssertEquals(result.Milestones.Count, 2);
		}

		public void TestGetOuterPacksDetailUsesPackTypeFromPackline()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.JL_Description = "LINE1";
			line1.JL_ActualVolume = 10m;
			line1.JL_ActualWeight = 5m;
			line1.JL_PackageCount = 15;
			line1.JL_F3_NKPackType = "PLT";
			line1.JL_ActualVolumeUQ = "M3";
			line1.JL_ActualWeightUQ = "KG";

			DocShipment docShipment = DocShipment.New(shipment, shipment.Factory);
			ZString expectedPlt = "CONTAINER/ WGT/ VOL/ PKG" + System.Environment.NewLine + "/ 5KG/ 10M3/ 15PLT";
			AssertEquals("Should have packline details with container", expectedPlt, docShipment.GetOuterPacksDetail(null));

			line1.JL_F3_NKPackType = "KEG";
			ZString expectedKeg = "CONTAINER/ WGT/ VOL/ PKG" + System.Environment.NewLine + "/ 5KG/ 10M3/ 15KEG";
			AssertEquals("Should have packline details with container", expectedKeg, docShipment.GetOuterPacksDetail(null));
		}

		public void TestCompanyCode()
		{
			Shipment.JS_UniqueConsignRef = "AA12321";

			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "A11";
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch2);

			GlbCompany company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "B22";
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();
			company3.Branches.Add(branch3);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				DocShipment wrapper2 = DocShipment.New(Shipment, Factory);
				AssertEquals(company2.GC_Code, wrapper2.CompanyCode);
			}
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch3.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				DocShipment wrapper3 = DocShipment.New(Shipment, Factory);
				AssertEquals(company3.GC_Code, wrapper3.CompanyCode);
			}
		}

		public void TestCountryCode()
		{
			Shipment.JS_UniqueConsignRef = "AA12321";

			RefCountry country2 = Factory.NewWithValidTestData<RefCountry>();
			country2.RN_Code = "A1";

			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "A11";
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch2);
			company2.GC_RN_NKCountryCode = country2.Code.ToString();

			RefCountry country3 = Factory.NewWithValidTestData<RefCountry>();
			country3.RN_Code = "B2";

			GlbCompany company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "B22";
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();
			company3.Branches.Add(branch3);
			company3.GC_RN_NKCountryCode = country3.Code.ToString();

			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				DocShipment wrapper2 = DocShipment.New(Shipment, Factory);
				AssertEquals(country2.RN_Code, wrapper2.CountryCode);
			}
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch3.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				DocShipment wrapper3 = DocShipment.New(Shipment, Factory);
				AssertEquals(country3.RN_Code, wrapper3.CountryCode);
			}
		}

		public void TestWarehouseLocation()
		{
			Shipment.JS_IsForwardRegistered = false;
			Shipment.JS_WarehouseLocation = "location";
			AssertEquals(Shipment.JS_WarehouseLocation, ShipmentWrapper.WarehouseLocation);

			Shipment.JS_IsForwardRegistered = true;
			AssertEquals(ZString.Empty, ShipmentWrapper.WarehouseLocation);
		}

		public void TestInspectedShipmentText()
		{
			AssertEquals("", ShipmentWrapper.InspectedShipmentText);

			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "APPROVED");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NOT APPROVED");
			Shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("", ShipmentWrapper.InspectedShipmentText);

			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("NOT APPROVED", ShipmentWrapper.InspectedShipmentText);

			Shipment.JS_InspectionTypeCode = "APP";
			AssertEquals("APPROVED", ShipmentWrapper.InspectedShipmentText);
		}

		#region Document Shipment

		#region FreightLabels

		public void TestStaticNewMethodWithDocumentShipmentForFreightLabels()
		{
			DocShipment nullShipment = DocShipment.New((DocumentShipment)null, Factory);
			AssertNull("Shipment wrapper is null", nullShipment);

			DocumentShipment documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.FreightLabels);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertNotNull("Shipment wrapper is not null", ShipmentWrapper);
		}

		public void TestDocumentShipmentPropertiesForFreightLabels()
		{
			DocumentShipment documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.FreightLabels);
			documentShipment.IncludeConsignee = true;
			documentShipment.IncludeConsignor = true;
			documentShipment.IncludeNone = true;
			documentShipment.NumberOfLabelsToPrint = 0;

			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("Include Consignee", true, ShipmentWrapper.IncludeConsignee);
			AssertEquals("Include Consignor", true, ShipmentWrapper.IncludeConsignor);
			AssertEquals("Include None", true, ShipmentWrapper.IncludeNone);
			AssertEquals("NumberOfLabelsToPrint", 0, ShipmentWrapper.NumberOfLabelsToPrint);

			documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.FreightLabels);
			documentShipment.IncludeConsignor = false;
			documentShipment.IncludeConsignee = false;
			documentShipment.IncludeNone = false;
			documentShipment.NumberOfLabelsToPrint = 17;

			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("Include Consignee", false, ShipmentWrapper.IncludeConsignee);
			AssertEquals("Include Consignor", false, ShipmentWrapper.IncludeConsignor);
			AssertEquals("Include None", false, ShipmentWrapper.IncludeNone);
			AssertEquals("NumberOfLabelsToPrint", 17, ShipmentWrapper.NumberOfLabelsToPrint);
		}

		public void TestDocumentShipmentPropertiesForChargeSheet()
		{
			DocumentShipment documentShipment = new DocumentShipment(Shipment, Constants.DataContext.ChargeSheet);
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DebtorToSelectFromForPrinting debtor = new DebtorToSelectFromForPrinting(header);
			documentShipment.Debtor = debtor;
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("Debtor", header.PK, ShipmentWrapper.Debtor.PK);
		}

		#endregion

		#region Letter Of Indemnity

		public void TestStaticNewMethodWithDocumentShipmentForLetterOfIndemnity()
		{
			DocShipment nullShipment = DocShipment.New((DocumentShipment)null, Factory);
			AssertNull("Shipment wrapper is null", nullShipment);

			DocumentShipment documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.LetterOfIndemnity);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertNotNull("Shipment wrapper is not null", ShipmentWrapper);
		}

		DocumentShipment SetupDocumentShipmentForLetterOfIndemnity(bool change)
		{
			DocumentShipment documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.LetterOfIndemnity);
			documentShipment.OldGoodsDescription = "OldGoodsDescription";
			documentShipment.OldMarksAndNumbers = "OldMarksAndNumbers";
			documentShipment.OldWeightUnit = "KG";
			documentShipment.OldVolumeUnit = "M3";
			documentShipment.OldWeight = ZDecimal.ParseSafe("15.545", 0);
			documentShipment.OldVolume = ZDecimal.ParseSafe("17.666", 0);
			documentShipment.NewGoodsDescription = "NewGoodsDescription";
			documentShipment.NewMarksAndNumbers = "NewMarksAndNumbers";
			documentShipment.NewWeightUnit = "KG";
			documentShipment.NewVolumeUnit = "M3";
			documentShipment.NewWeight = ZDecimal.ParseSafe("1.111", 0);
			documentShipment.NewVolume = ZDecimal.ParseSafe("2.222", 0);
			documentShipment.ChangeGoodsDescription = change;
			documentShipment.ChangeMarksAndNumbers = change;
			documentShipment.ChangeWeight = change;
			documentShipment.ChangeVolume = change;
			return documentShipment;
		}

		public void TestOldGoodsDescription()
		{
			DocumentShipment documentShipment = SetupDocumentShipmentForLetterOfIndemnity(true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".OldGoodsDescription with change", "OldGoodsDescription", ShipmentWrapper.OldGoodsDescription);

			documentShipment = SetupDocumentShipmentForLetterOfIndemnity(false);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".OldGoodsDescription with no change", "OldGoodsDescription", ShipmentWrapper.OldGoodsDescription);
		}

		public void TestOldMarksAndNumbers()
		{
			DocumentShipment documentShipment = SetupDocumentShipmentForLetterOfIndemnity(true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".OldMarksAndNumbers with change", "OldMarksAndNumbers", ShipmentWrapper.OldMarksAndNumbers);

			documentShipment = SetupDocumentShipmentForLetterOfIndemnity(false);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".OldMarksAndNumbers with no change", "OldMarksAndNumbers", ShipmentWrapper.OldMarksAndNumbers);
		}

		public void TestOldWeight()
		{
			DocumentShipment documentShipment = SetupDocumentShipmentForLetterOfIndemnity(true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".OldWeight with change", "15.545 KG", ShipmentWrapper.OldWeight);

			documentShipment = SetupDocumentShipmentForLetterOfIndemnity(false);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".OldWeight with no change", "15.545 KG", ShipmentWrapper.OldWeight);
		}

		public void TestOldVolume()
		{
			DocumentShipment documentShipment = SetupDocumentShipmentForLetterOfIndemnity(true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".OldVolume with change", "17.666 M3", ShipmentWrapper.OldVolume);

			documentShipment = SetupDocumentShipmentForLetterOfIndemnity(false);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".OldVolume with no change", "17.666 M3", ShipmentWrapper.OldVolume);
		}

		public void TestNewGoodsDescription()
		{
			DocumentShipment documentShipment = SetupDocumentShipmentForLetterOfIndemnity(true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".NewGoodsDescription with change", "NewGoodsDescription", ShipmentWrapper.NewGoodsDescription);

			documentShipment = SetupDocumentShipmentForLetterOfIndemnity(false);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".NewGoodsDescription with no change", "As above - no amendment required", ShipmentWrapper.NewGoodsDescription);
		}

		public void TestNewMarksAndNumbers()
		{
			DocumentShipment documentShipment = SetupDocumentShipmentForLetterOfIndemnity(true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".NewMarksAndNumbers with change", "NewMarksAndNumbers", ShipmentWrapper.NewMarksAndNumbers);

			documentShipment = SetupDocumentShipmentForLetterOfIndemnity(false);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".NewMarksAndNumbers with no change", "As above - no amendment required", ShipmentWrapper.NewMarksAndNumbers);
		}

		public void TestNewWeight()
		{
			DocumentShipment documentShipment = SetupDocumentShipmentForLetterOfIndemnity(true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".NewWeight with change", "1.111 KG", ShipmentWrapper.NewWeight);

			documentShipment = SetupDocumentShipmentForLetterOfIndemnity(false);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".NewWeight with no change", "As above - no amendment required", ShipmentWrapper.NewWeight);
		}

		public void TestNewVolume()
		{
			DocumentShipment documentShipment = SetupDocumentShipmentForLetterOfIndemnity(true);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".NewVolume with change", "2.222 M3", ShipmentWrapper.NewVolume);

			documentShipment = SetupDocumentShipmentForLetterOfIndemnity(false);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals(".NewVolume with no change", "As above - no amendment required", ShipmentWrapper.NewVolume);
		}

		#endregion

		#region Standard Shipping Note

		public void TestConfirmationToPrint()
		{
			DocumentShipment documentShipment = new DocumentShipment(Shipment, Constants.DataContext.Shipment);
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertNull("No confirmation to print", ShipmentWrapper.ConfirmationToPrint);

			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();

			documentShipment.ConfirmationToPrint = confirm;
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("Confirmation to print was set", confirm.UniqueID, ShipmentWrapper.ConfirmationToPrint.ConfirmationID);
		}

		#endregion

		#endregion

		#region Template Constants

		public void TestChargeDescriptionColumnWidthConstant()
		{
			AssertEquals("Default should be 6", 6, ShipmentWrapper.ChargeDescriptionColumnWidth);
			AssertEquals("Legacy default", 6, ShipmentWrapper.ChargesDiscriptionWidth);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionColumnWidth, 1);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("Only correct spelling", 1, ShipmentWrapper.ChargeDescriptionColumnWidth);
			AssertEquals("Legacy", 6, ShipmentWrapper.ChargesDiscriptionWidth);
		}

		public void TestChargeDescriptionColumnWidthConstantFallback()
		{
			AssertEquals("Default should be 6", 6, ShipmentWrapper.ChargeDescriptionColumnWidth);
			AssertEquals("Legacy default", 6, ShipmentWrapper.ChargesDiscriptionWidth);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 2);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("Wrong spelling is used if correct spelling isn't present", 2, ShipmentWrapper.ChargeDescriptionColumnWidth);
			AssertEquals("Repeat after me; description has an 'e'", 2, ShipmentWrapper.ChargesDiscriptionWidth);
		}

		public void TestChargeDescriptionColumnWidthConstantOverride()
		{
			AssertEquals("Default should be 6", 6, ShipmentWrapper.ChargeDescriptionColumnWidth);
			AssertEquals("Legacy default", 6, ShipmentWrapper.ChargesDiscriptionWidth);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 3);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionColumnWidth, 4);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("Correct spelling overrides incorrect", 4, ShipmentWrapper.ChargeDescriptionColumnWidth);
			AssertEquals("It's called 'spelling', people.", 3, ShipmentWrapper.ChargesDiscriptionWidth);
		}

		public void TestChargesDescriptionFollowOnPageWidthConstant()
		{
			AssertEquals("Default should be 80 as there's nothing on the line other than the charge code there's no reason to shorten it", 80, ShipmentWrapper.ChargesDescriptionFollowOnPageWidth);

			var constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDescriptionFollowOnPageWidth, 1);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("Should have been set to the new constant value", 1, ShipmentWrapper.ChargesDescriptionFollowOnPageWidth);
		}

		public void TestSetBOLTemplateConstants()
		{
			AssertEquals("UseMultiPage", false, ShipmentWrapper.UseMultiPage);
			AssertEquals("ShowDetailHeadingInMainBody", false, ShipmentWrapper.ShowDetailHeadingInMainBody);
			AssertEquals("ShowContainerHeadingInMainBody", false, ShipmentWrapper.ShowContainerHeadingInMainBody);
			AssertEquals("ShowChargesHeadingInMainBody", false, ShipmentWrapper.ShowChargesHeadingInMainBody);
			AssertEquals("ShowExtraSectionHeadingInMainBody", false, ShipmentWrapper.ShowExtraSectionHeadingInMainBody);
			AssertEquals("ShowBOLClauseSectionHeadingInMainBody", false, ShipmentWrapper.ShowBOLClauseSectionHeadingInMainBody);
			AssertEquals("IncludeBOLClauseInGoodsDescription", true, ShipmentWrapper.IncludeBOLClauseInGoodsDescription);
			AssertEquals("IncludeContainersInMarksAndNumbersSection", false, ShipmentWrapper.IncludeContainersInMarksAndNumbersSection);
			AssertEquals("IncludeExtraSectionInMarksAndNumbersSection", false, ShipmentWrapper.IncludeExtraSectionInMarksAndNumbersSection);
			AssertEquals("IncludeBOLClauseSectionInMarksAndNumbersSection", false, ShipmentWrapper.IncludeBOLClauseSectionInMarksAndNumbersSection);
			AssertEquals("ShowRORHeadingInMainBody", true, ShipmentWrapper.ShowRORHeadingInMainBody);
			AssertEquals("IncludeRORInMarksAndNumbersSection", true, ShipmentWrapper.IncludeRORInMarksAndNumbersSection);
			AssertEquals("InterleavePacksAndContainers", false, ShipmentWrapper.InterleavePacksAndContainers);
			AssertEquals("IncludeEmergencyContactWithUNDG", false, ShipmentWrapper.IncludeEmergencyContactWithUNDG);
			AssertEquals("NumberOfRORRows", 6, ShipmentWrapper.NumberOfRORRows);
			AssertEquals("NoOfExtraSectionRows", 6, ShipmentWrapper.NoOfExtraSectionRows);
			AssertEquals("DimensionsDecimalPlaces", 3, ShipmentWrapper.DimensionsDecimalPlaces);
			AssertEquals("AreaDecimalPlaces", 3, ShipmentWrapper.AreaDecimalPlaces);
			AssertEquals("NumberOfBOLClauseRows", 3, ShipmentWrapper.NumberOfBOLClauseRows);
			AssertEquals("UseImperialUnits", false, ShipmentWrapper.UseImperialUnits);
			AssertEquals("BOLWeightUnit", "KG", ShipmentWrapper.BOLWeightUnit);
			AssertEquals("BOLVolumeUnit", "M3", ShipmentWrapper.BOLVolumeUnit);
			AssertEquals("BOLLengthUnit", "M", ShipmentWrapper.BOLLengthUnit);
			AssertEquals("SuppressZeroCharges", false, ShipmentWrapper.SuppressZeroCharges);
			AssertEquals("ConvertUnits", true, ShipmentWrapper.ConvertUnits);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.UseMultiPage, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowDetailHeadingInMainBody, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerHeadingInMainBody, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowChargesHeadingInMainBody, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowExtraSectionHeadingInMainBody, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowBOLClauseSectionHeadingInMainBody, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.IncludeBOLClauseInGoodsDescription, false);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.IncludeContainersInMarksAndNumbersSection, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.IncludeExtraSectionInMarksAndNumbersSection, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.IncludeBOLClauseSectionInMarksAndNumbersSection, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowRORHeadingInMainBody, false);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.IncludeRORInMarksAndNumbersSection, false);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.InterleavePacksAndContainers, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.IncludeEmergencyContactWithUNDG, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfRORRows, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NoOfExtraSectionRows, 2);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DimensionsDecimalPlaces, 4);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AreaDecimalPlaces, 5);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfBOLClauseRows, 6);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.UseImperialUnits, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.SuppressZeroCharges, true);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ConvertUnits, false);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("UseMultiPage", true, ShipmentWrapper.UseMultiPage);
			AssertEquals("ShowDetailHeadingInMainBody", true, ShipmentWrapper.ShowDetailHeadingInMainBody);
			AssertEquals("ShowContainerHeadingInMainBody", true, ShipmentWrapper.ShowContainerHeadingInMainBody);
			AssertEquals("ShowChargesHeadingInMainBody", true, ShipmentWrapper.ShowChargesHeadingInMainBody);
			AssertEquals("ShowExtraSectionHeadingInMainBody", true, ShipmentWrapper.ShowExtraSectionHeadingInMainBody);
			AssertEquals("ShowBOLClauseSectionHeadingInMainBody", true, ShipmentWrapper.ShowBOLClauseSectionHeadingInMainBody);
			AssertEquals("IncludeBOLClauseInGoodsDescription", false, ShipmentWrapper.IncludeBOLClauseInGoodsDescription);
			AssertEquals("IncludeContainersInMarksAndNumbersSection", true, ShipmentWrapper.IncludeContainersInMarksAndNumbersSection);
			AssertEquals("IncludeExtraSectionInMarksAndNumbersSection", true, ShipmentWrapper.IncludeExtraSectionInMarksAndNumbersSection);
			AssertEquals("IncludeBOLClauseSectionInMarksAndNumbersSection", true, ShipmentWrapper.IncludeBOLClauseSectionInMarksAndNumbersSection);
			AssertEquals("ShowRORHeadingInMainBody", false, ShipmentWrapper.ShowRORHeadingInMainBody);
			AssertEquals("IncludeRORInMarksAndNumbersSection", false, ShipmentWrapper.IncludeRORInMarksAndNumbersSection);
			AssertEquals("InterleavePacksAndContainers", true, ShipmentWrapper.InterleavePacksAndContainers);
			AssertEquals("NumberOfRORRows", 1, ShipmentWrapper.NumberOfRORRows);
			AssertEquals("NoOfExtraSectionRows", 2, ShipmentWrapper.NoOfExtraSectionRows);
			AssertEquals("DimensionsDecimalPlaces", 4, ShipmentWrapper.DimensionsDecimalPlaces);
			AssertEquals("AreaDecimalPlaces", 5, ShipmentWrapper.AreaDecimalPlaces);
			AssertEquals("NumberOfBOLClauseRows", 6, ShipmentWrapper.NumberOfBOLClauseRows);
			AssertEquals("UseImperialUnits", true, ShipmentWrapper.UseImperialUnits);
			AssertEquals("BOLWeightUnit", "LB", ShipmentWrapper.BOLWeightUnit);
			AssertEquals("BOLVolumeUnit", "CF", ShipmentWrapper.BOLVolumeUnit);
			AssertEquals("BOLLengthUnit", "FT", ShipmentWrapper.BOLLengthUnit);
			AssertEquals("IncludeEmergencyContactWithUNDG", true, ShipmentWrapper.IncludeEmergencyContactWithUNDG);
			AssertEquals("SuppressZeroCharges", true, ShipmentWrapper.SuppressZeroCharges);
			AssertEquals("ConvertUnits", false, ShipmentWrapper.ConvertUnits);
		}

		public void TestSetBOLColumnWidthTemplateConstants()
		{
			AssertEquals("ContainerWeightAndUQWidth", 14, ShipmentWrapper.ContainerWeightAndUQWidth);
			AssertEquals("ContainerTareAndUQWidth", 14, ShipmentWrapper.ContainerTareAndUQWidth);
			AssertEquals("ContainerGrossAndUQWidth", 14, ShipmentWrapper.ContainerGrossAndUQWidth);
			AssertEquals("ContainerVolumeAndUQWidth", 14, ShipmentWrapper.ContainerVolumeAndUQWidth);
			AssertEquals("ChargeCodeColumnWidth", 0, ShipmentWrapper.ChargeCodeColumnWidth);
			AssertEquals("CollectChargesColumnWidth", 11, ShipmentWrapper.CollectChargesColumnWidth);
			AssertEquals("CollectCurrencyColumnWidth", 3, ShipmentWrapper.CollectCurrencyColumnWidth);
			AssertEquals("PrepaidChargesColumnWidth", 11, ShipmentWrapper.PrepaidChargesColumnWidth);
			AssertEquals("PrepaidCurrencyColumnWidth", 3, ShipmentWrapper.PrepaidCurrencyColumnWidth);
			AssertEquals("AllChargesColumnWidth", 11, ShipmentWrapper.AllChargesColumnWidth);
			AssertEquals("AllChargesCurrencyColumnWidth", 3, ShipmentWrapper.AllChargesCurrencyColumnWidth);
			AssertEquals("PackRefNumberColumnWidth", 20, ShipmentWrapper.PackRefNumberColumnWidth);
			AssertEquals("PackDescriptionColumnWidth", 0, ShipmentWrapper.PackDescriptionColumnWidth);
			AssertEquals("PackShortDescriptionColumnWidth", 20, ShipmentWrapper.PackShortDescriptionColumnWidth);
			AssertEquals("PackMarksAndNumbersColumnWidth", 16, ShipmentWrapper.PackMarksAndNumbersColumnWidth);
			AssertEquals("PackUNDGColumnWidth", 0, ShipmentWrapper.PackUNDGColumnWidth);
			AssertEquals("PackCommodityCodeColumnWidth", 4, ShipmentWrapper.PackCommodityCodeColumnWidth);
			AssertEquals("PackCommodityDescriptionColumnWidth", 16, ShipmentWrapper.PackCommodityDescriptionColumnWidth);
			AssertEquals("PackCountColumnWidth", 9, ShipmentWrapper.PackCountColumnWidth);
			AssertEquals("PackCountAndTypeColumnWidth", 11, ShipmentWrapper.PackCountAndTypeColumnWidth);
			AssertEquals("PackWeightColumnWidth", 14, ShipmentWrapper.PackWeightColumnWidth);
			AssertEquals("PackWeightAndUQColumnWidth", 14, ShipmentWrapper.PackWeightAndUQColumnWidth);
			AssertEquals("PackVolumeColumnWidth", 14, ShipmentWrapper.PackVolumeColumnWidth);
			AssertEquals("PackVolumeAndUQColumnWidth", 14, ShipmentWrapper.PackVolumeAndUQColumnWidth);
			AssertEquals("PackLengthColumnWidth", 11, ShipmentWrapper.PackLengthColumnWidth);
			AssertEquals("PackLengthAndUQColumnWidth", 11, ShipmentWrapper.PackLengthAndUQColumnWidth);
			AssertEquals("PackWidthColumnWidth", 11, ShipmentWrapper.PackWidthColumnWidth);
			AssertEquals("PackWidthAndUQColumnWidth", 11, ShipmentWrapper.PackWidthAndUQColumnWidth);
			AssertEquals("PackHeightColumnWidth", 11, ShipmentWrapper.PackHeightColumnWidth);
			AssertEquals("PackHeightAndUQColumnWidth", 11, ShipmentWrapper.PackHeightAndUQColumnWidth);
			AssertEquals("PackAreaColumnWidth", 14, ShipmentWrapper.PackAreaColumnWidth);
			AssertEquals("PackVehicleColorColumnWidth", 11, ShipmentWrapper.PackVehicleColourColumnWidth);
			AssertEquals("PackVehicleMakeColumnWidth", 14, ShipmentWrapper.PackVehicleMakeColumnWidth);
			AssertEquals("PackVehicleModelColumnWidth", 14, ShipmentWrapper.PackVehicleModelColumnWidth);
			AssertEquals("PackVehicleNumberOfDoorsColumnWidth", 5, ShipmentWrapper.PackVehicleNumberOfDoorsColumnWidth);
			AssertEquals("PackVehicleTransmissionColumnWidth", 6, ShipmentWrapper.PackVehicleTransmissionColumnWidth);
			AssertEquals("PackVehicleYearColumnWidth", 4, ShipmentWrapper.PackVehicleYearColumnWidth);
			AssertEquals("BOLClauseColumnWidth", 20, ShipmentWrapper.BOLClauseColumnWidth);
			AssertEquals("PackDescriptionAndUNDGColumnWidth", 0, ShipmentWrapper.PackDescriptionAndUNDGColumnWidth);
			AssertEquals("PackDimensionsColumnWidth", 20, ShipmentWrapper.PackDimensionsColumnWidth);
			AssertEquals("PackHarmonizedCodeColumnWidth", 15, ShipmentWrapper.PackHarmonizedCodeColumnWidth);
			AssertEquals("PackVehicleFullDetailsColumnWidth", 36, ShipmentWrapper.PackVehicleFullDetailsColumnWidth);
			AssertEquals("PackRefNumberAndMarksAndNumbersColumnWidth", 0, ShipmentWrapper.PackRefNumberAndMarksAndNumbersColumnWidth);
			AssertEquals("PackContainsUNDGColumnWidth", 1, ShipmentWrapper.PackContainsUNDGColumnWidth);
			AssertEquals("PackUNDGAndShortDescriptionColumnWidth", 0, ShipmentWrapper.PackUNDGAndShortDescriptionColumnWidth);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeColumnWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnWidth, 12);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectCurrencyColumnWidth, 2);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnWidth, 13);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidCurrencyColumnWidth, 4);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesColumnWidth, 14);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesCurrencyColumnWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberColumnWidth, 5);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionColumnWidth, 6);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackShortDescriptionColumnWidth, 7);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackMarksAndNumbersColumnWidth, 8);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGColumnWidth, 9);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityCodeColumnWidth, 10);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityDescriptionColumnWidth, 11);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCountColumnWidth, 12);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCountAndTypeColumnWidth, 13);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightColumnWidth, 15);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightAndUQColumnWidth, 16);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeColumnWidth, 17);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeAndUQColumnWidth, 18);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthColumnWidth, 19);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthAndUQColumnWidth, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthColumnWidth, 21);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthAndUQColumnWidth, 22);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightColumnWidth, 23);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightAndUQColumnWidth, 24);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackAreaColumnWidth, 25);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleColorColumnWidth, 26);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleMakeColumnWidth, 27);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleModelColumnWidth, 28);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleNumberOfDoorsColumnWidth, 29);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleTransmissionColumnWidth, 30);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleYearColumnWidth, 31);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.BOLClauseColumnWidth, 32);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionAndUNDGColumnWidth, 33);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDimensionsColumnWidth, 34);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHarmonizedCodeColumnWidth, 10);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleFullDetailsColumnWidth, 35);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberAndMarksAndNumbersColumnWidth, 36);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackContainsUNDGColumnWidth, 37);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndUQWidth, 38);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndUQWidth, 39);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndUQWidth, 40);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndUQWidth, 41);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGAndShortDescriptionColumnWidth, 42);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("ChargeCodeColumnWidth", 1, ShipmentWrapper.ChargeCodeColumnWidth);
			AssertEquals("CollectChargesColumnWidth", 12, ShipmentWrapper.CollectChargesColumnWidth);
			AssertEquals("CollectCurrencyColumnWidth", 2, ShipmentWrapper.CollectCurrencyColumnWidth);
			AssertEquals("PrepaidChargesColumnWidth", 13, ShipmentWrapper.PrepaidChargesColumnWidth);
			AssertEquals("PrepaidCurrencyColumnWidth", 4, ShipmentWrapper.PrepaidCurrencyColumnWidth);
			AssertEquals("AllChargesColumnWidth", 14, ShipmentWrapper.AllChargesColumnWidth);
			AssertEquals("AllChargesCurrencyColumnWidth", 1, ShipmentWrapper.AllChargesCurrencyColumnWidth);
			AssertEquals("PackRefNumberColumnWidth", 5, ShipmentWrapper.PackRefNumberColumnWidth);
			AssertEquals("PackDescriptionColumnWidth", 6, ShipmentWrapper.PackDescriptionColumnWidth);
			AssertEquals("PackShortDescriptionColumnWidth", 7, ShipmentWrapper.PackShortDescriptionColumnWidth);
			AssertEquals("PackMarksAndNumbersColumnWidth", 8, ShipmentWrapper.PackMarksAndNumbersColumnWidth);
			AssertEquals("PackUNDGColumnWidth", 9, ShipmentWrapper.PackUNDGColumnWidth);
			AssertEquals("PackCommodityCodeColumnWidth", 10, ShipmentWrapper.PackCommodityCodeColumnWidth);
			AssertEquals("PackCommodityDescriptionColumnWidth", 11, ShipmentWrapper.PackCommodityDescriptionColumnWidth);
			AssertEquals("PackCountColumnWidth", 12, ShipmentWrapper.PackCountColumnWidth);
			AssertEquals("PackCountAndTypeColumnWidth", 13, ShipmentWrapper.PackCountAndTypeColumnWidth);
			AssertEquals("PackWeightColumnWidth", 15, ShipmentWrapper.PackWeightColumnWidth);
			AssertEquals("PackWeightAndUQColumnWidth", 16, ShipmentWrapper.PackWeightAndUQColumnWidth);
			AssertEquals("PackVolumeColumnWidth", 17, ShipmentWrapper.PackVolumeColumnWidth);
			AssertEquals("PackVolumeAndUQColumnWidth", 18, ShipmentWrapper.PackVolumeAndUQColumnWidth);
			AssertEquals("PackLengthColumnWidth", 19, ShipmentWrapper.PackLengthColumnWidth);
			AssertEquals("PackLengthAndUQColumnWidth", 20, ShipmentWrapper.PackLengthAndUQColumnWidth);
			AssertEquals("PackWidthColumnWidth", 21, ShipmentWrapper.PackWidthColumnWidth);
			AssertEquals("PackWidthAndUQColumnWidth", 22, ShipmentWrapper.PackWidthAndUQColumnWidth);
			AssertEquals("PackHeightColumnWidth", 23, ShipmentWrapper.PackHeightColumnWidth);
			AssertEquals("PackHeightAndUQColumnWidth", 24, ShipmentWrapper.PackHeightAndUQColumnWidth);
			AssertEquals("PackAreaColumnWidth", 25, ShipmentWrapper.PackAreaColumnWidth);
			AssertEquals("PackVehicleColorColumnWidth", 26, ShipmentWrapper.PackVehicleColourColumnWidth);
			AssertEquals("PackVehicleMakeColumnWidth", 27, ShipmentWrapper.PackVehicleMakeColumnWidth);
			AssertEquals("PackVehicleModelColumnWidth", 28, ShipmentWrapper.PackVehicleModelColumnWidth);
			AssertEquals("PackVehicleNumberOfDoorsColumnWidth", 29, ShipmentWrapper.PackVehicleNumberOfDoorsColumnWidth);
			AssertEquals("PackVehicleTransmissionColumnWidth", 30, ShipmentWrapper.PackVehicleTransmissionColumnWidth);
			AssertEquals("PackVehicleYearColumnWidth", 31, ShipmentWrapper.PackVehicleYearColumnWidth);
			AssertEquals("BOLClauseColumnWidth", 32, ShipmentWrapper.BOLClauseColumnWidth);
			AssertEquals("PackDescriptionAndUNDGColumnWidth", 33, ShipmentWrapper.PackDescriptionAndUNDGColumnWidth);
			AssertEquals("PackDimensionsColumnWidth", 34, ShipmentWrapper.PackDimensionsColumnWidth);
			AssertEquals("PackHarmonizedCodeColumnWidth", 10, ShipmentWrapper.PackHarmonizedCodeColumnWidth);
			AssertEquals("PackVehicleFullDetailsColumnWidth", 35, ShipmentWrapper.PackVehicleFullDetailsColumnWidth);
			AssertEquals("PackRefNumberAndMarksAndNumbersColumnWidth", 36, ShipmentWrapper.PackRefNumberAndMarksAndNumbersColumnWidth);
			AssertEquals("PackContainsUNDGColumnWidth", 37, ShipmentWrapper.PackContainsUNDGColumnWidth);
			AssertEquals("ContainerWeightAndUQWidth", 38, ShipmentWrapper.ContainerWeightAndUQWidth);
			AssertEquals("ContainerTareAndUQWidth", 39, ShipmentWrapper.ContainerTareAndUQWidth);
			AssertEquals("ContainerGrossAndUQWidth", 40, ShipmentWrapper.ContainerGrossAndUQWidth);
			AssertEquals("ContainerVolumeAndUQWidth", 41, ShipmentWrapper.ContainerVolumeAndUQWidth);
			AssertEquals("PackUNDGAndShortDescriptionColumnWidth", 42, ShipmentWrapper.PackUNDGAndShortDescriptionColumnWidth);
		}

		public void TestSetBOLTemplateColumnIndexConstants()
		{
			AssertEquals("MarksAndNumbersIndex", 1, ShipmentWrapper.MarksAndNumbersIndex);
			AssertEquals("PackagesIndex", 0, ShipmentWrapper.PackagesIndex);
			AssertEquals("GoodsDescIndex", 3, ShipmentWrapper.GoodsDescIndex);
			AssertEquals("GrossWeightIndex", 4, ShipmentWrapper.GrossWeightIndex);
			AssertEquals("VolumeMeasurementIndex", 5, ShipmentWrapper.VolumeMeasurementIndex);
			AssertEquals("ContainerNumberIndex", 1, ShipmentWrapper.ContainerNumberIndex);
			AssertEquals("ContainerSealIndex", 2, ShipmentWrapper.ContainerSealIndex);
			AssertEquals("ContainerTypeIndex", 3, ShipmentWrapper.ContainerTypeIndex);
			AssertEquals("ContainerWeightIndex", 4, ShipmentWrapper.ContainerWeightIndex);
			AssertEquals("ContainerTareIndex", 5, ShipmentWrapper.ContainerTareIndex);
			AssertEquals("ContainerGrossIndex", 6, ShipmentWrapper.ContainerGrossIndex);
			AssertEquals("ContainerVolumeIndex", 7, ShipmentWrapper.ContainerVolumeIndex);
			AssertEquals("ContainerPackagesIndex", 8, ShipmentWrapper.ContainerPackagesIndex);
			AssertEquals("ContainerModeIndex", 0, ShipmentWrapper.ContainerModeIndex);
			AssertEquals("ContainerTemperatureSettingIndex", 0, ShipmentWrapper.ContainerTemperatureSettingIndex);
			AssertEquals("ContainerHumiditySettingIndex", 0, ShipmentWrapper.ContainerHumiditySettingIndex);
			AssertEquals("ContainerWeightAndUQIndex", 0, ShipmentWrapper.ContainerWeightAndUQIndex);
			AssertEquals("ContainerTareAndUQIndex", 0, ShipmentWrapper.ContainerTareAndUQIndex);
			AssertEquals("ContainerGrossAndUQIndex", 0, ShipmentWrapper.ContainerGrossAndUQIndex);
			AssertEquals("ContainerVolumeAndUQIndex", 0, ShipmentWrapper.ContainerVolumeAndUQIndex);
			AssertEquals("ChargeCodeIndex", 0, ShipmentWrapper.ChargeCodeIndex);
			AssertEquals("ChargeDescriptionIndex", 1, ShipmentWrapper.ChargeDescriptionIndex);
			AssertEquals("CollectChargesColumnIndex", 2, ShipmentWrapper.CollectChargesColumnIndex);
			AssertEquals("CollectCurrencyColumnIndex", 3, ShipmentWrapper.CollectCurrencyColumnIndex);
			AssertEquals("PrepaidChargesColumnIndex", 4, ShipmentWrapper.PrepaidChargesColumnIndex);
			AssertEquals("PrepaidCurrencyColumnIndex", 5, ShipmentWrapper.PrepaidCurrencyColumnIndex);
			AssertEquals("AllChargesColumnIndex", 0, ShipmentWrapper.AllChargesColumnIndex);
			AssertEquals("AllChargesCurrencyColumnIndex", 0, ShipmentWrapper.AllChargesCurrencyColumnIndex);
			AssertEquals("PackRefNumberColumnIndex", 1, ShipmentWrapper.PackRefNumberColumnIndex);
			AssertEquals("PackDescriptionColumnIndex", 0, ShipmentWrapper.PackDescriptionColumnIndex);
			AssertEquals("PackShortDescriptionColumnIndex", 0, ShipmentWrapper.PackShortDescriptionColumnIndex);
			AssertEquals("PackMarksAndNumbersColumnIndex", 0, ShipmentWrapper.PackMarksAndNumbersColumnIndex);
			AssertEquals("PackUNDGColumnIndex", 0, ShipmentWrapper.PackUNDGColumnIndex);
			AssertEquals("PackCommodityCodeColumnIndex", 0, ShipmentWrapper.PackCommodityCodeColumnIndex);
			AssertEquals("PackCommodityDescriptionColumnIndex", 0, ShipmentWrapper.PackCommodityDescriptionColumnIndex);
			AssertEquals("PackCountColumnIndex", 2, ShipmentWrapper.PackCountColumnIndex);
			AssertEquals("PackCountAndTypeColumnIndex", 0, ShipmentWrapper.PackCountAndTypeColumnIndex);
			AssertEquals("PackWeightColumnIndex", 3, ShipmentWrapper.PackWeightColumnIndex);
			AssertEquals("PackWeightAndUQColumnIndex", 0, ShipmentWrapper.PackWeightAndUQColumnIndex);
			AssertEquals("PackVolumeColumnIndex", 4, ShipmentWrapper.PackVolumeColumnIndex);
			AssertEquals("PackVolumeAndUQColumnIndex", 0, ShipmentWrapper.PackVolumeAndUQColumnIndex);
			AssertEquals("PackLengthColumnIndex", 5, ShipmentWrapper.PackLengthColumnIndex);
			AssertEquals("PackLengthAndUQColumnIndex", 0, ShipmentWrapper.PackLengthAndUQColumnIndex);
			AssertEquals("PackWidthColumnIndex", 6, ShipmentWrapper.PackWidthColumnIndex);
			AssertEquals("PackWidthAndUQColumnIndex", 0, ShipmentWrapper.PackWidthAndUQColumnIndex);
			AssertEquals("PackHeightColumnIndex", 7, ShipmentWrapper.PackHeightColumnIndex);
			AssertEquals("PackHeightAndUQColumnIndex", 0, ShipmentWrapper.PackHeightAndUQColumnIndex);
			AssertEquals("PackAreaColumnIndex", 8, ShipmentWrapper.PackAreaColumnIndex);
			AssertEquals("PackVehicleColorColumnIndex", 0, ShipmentWrapper.PackVehicleColourColumnIndex);
			AssertEquals("PackVehicleMakeColumnIndex", 0, ShipmentWrapper.PackVehicleMakeColumnIndex);
			AssertEquals("PackVehicleModelColumnIndex", 0, ShipmentWrapper.PackVehicleModelColumnIndex);
			AssertEquals("PackVehicleNumberOfDoorsColumnIndex", 0, ShipmentWrapper.PackVehicleNumberOfDoorsColumnIndex);
			AssertEquals("PackVehicleTransmissionColumnIndex", 0, ShipmentWrapper.PackVehicleTransmissionColumnIndex);
			AssertEquals("PackVehicleYearColumnIndex", 0, ShipmentWrapper.PackVehicleYearColumnIndex);
			AssertEquals("PackDescriptionAndUNDGColumnIndex", 0, ShipmentWrapper.PackDescriptionAndUNDGColumnIndex);
			AssertEquals("PackDimensionsColumnIndex", 0, ShipmentWrapper.PackDimensionsColumnIndex);
			AssertEquals("PackHarmonizedCodeColumnIndex", 0, ShipmentWrapper.PackHarmonizedCodeColumnIndex);
			AssertEquals("PackVehicleFullDetailsColumnIndex", 0, ShipmentWrapper.PackVehicleFullDetailsColumnIndex);
			AssertEquals("PackRefNumberAndMarksAndNumbersColumnIndex", 0, ShipmentWrapper.PackRefNumberAndMarksAndNumbersColumnIndex);
			AssertEquals("PackContainsUNDGColumnIndex", 0, ShipmentWrapper.PackContainsUNDGColumnIndex);
			AssertEquals("PackUNDGAndShortDescriptionColumnIndex", 0, ShipmentWrapper.PackUNDGAndShortDescriptionColumnIndex);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersIndex, 2);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackagesIndex, 3);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescIndex, 4);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightIndex, 5);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementIndex, 6);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberIndex, 7);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealIndex, 8);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeIndex, 9);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightIndex, 10);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareIndex, 11);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossIndex, 12);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeIndex, 13);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesIndex, 14);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeIndex, 15);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTemperatureSettingIndex, 16);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerHumiditySettingIndex, 17);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeIndex, 18);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionIndex, 19);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnIndex, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectCurrencyColumnIndex, 21);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnIndex, 22);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidCurrencyColumnIndex, 23);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesColumnIndex, 44);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesCurrencyColumnIndex, 45);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberColumnIndex, 24);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionColumnIndex, 25);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackShortDescriptionColumnIndex, 26);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackMarksAndNumbersColumnIndex, 27);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGColumnIndex, 28);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityCodeColumnIndex, 29);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityDescriptionColumnIndex, 30);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCountColumnIndex, 31);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCountAndTypeColumnIndex, 32);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightColumnIndex, 33);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightAndUQColumnIndex, 34);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeColumnIndex, 35);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeAndUQColumnIndex, 36);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthColumnIndex, 37);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthAndUQColumnIndex, 38);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthColumnIndex, 39);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthAndUQColumnIndex, 40);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightColumnIndex, 41);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightAndUQColumnIndex, 42);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackAreaColumnIndex, 43);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleColorColumnIndex, 44);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleMakeColumnIndex, 45);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleModelColumnIndex, 46);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleNumberOfDoorsColumnIndex, 47);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleTransmissionColumnIndex, 48);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleYearColumnIndex, 49);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionAndUNDGColumnIndex, 50);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDimensionsColumnIndex, 51);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleFullDetailsColumnIndex, 52);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberAndMarksAndNumbersColumnIndex, 53);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackContainsUNDGColumnIndex, 54);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHarmonizedCodeColumnIndex, 55);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndUQIndex, 56);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndUQIndex, 57);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndUQIndex, 58);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndUQIndex, 59);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGAndShortDescriptionColumnIndex, 60);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("MarksAndNumbersIndex", 2, ShipmentWrapper.MarksAndNumbersIndex);
			AssertEquals("PackagesIndex", 3, ShipmentWrapper.PackagesIndex);
			AssertEquals("GoodsDescIndex", 4, ShipmentWrapper.GoodsDescIndex);
			AssertEquals("GrossWeightIndex", 5, ShipmentWrapper.GrossWeightIndex);
			AssertEquals("VolumeMeasurementIndex", 6, ShipmentWrapper.VolumeMeasurementIndex);
			AssertEquals("ContainerNumberIndex", 7, ShipmentWrapper.ContainerNumberIndex);
			AssertEquals("ContainerSealIndex", 8, ShipmentWrapper.ContainerSealIndex);
			AssertEquals("ContainerTypeIndex", 9, ShipmentWrapper.ContainerTypeIndex);
			AssertEquals("ContainerWeightIndex", 10, ShipmentWrapper.ContainerWeightIndex);
			AssertEquals("ContainerTareIndex", 11, ShipmentWrapper.ContainerTareIndex);
			AssertEquals("ContainerGrossIndex", 12, ShipmentWrapper.ContainerGrossIndex);
			AssertEquals("ContainerVolumeIndex", 13, ShipmentWrapper.ContainerVolumeIndex);
			AssertEquals("ContainerPackagesIndex", 14, ShipmentWrapper.ContainerPackagesIndex);
			AssertEquals("ContainerModeIndex", 15, ShipmentWrapper.ContainerModeIndex);
			AssertEquals("ContainerTemperatureSettingIndex", 16, ShipmentWrapper.ContainerTemperatureSettingIndex);
			AssertEquals("ContainerHumiditySettingIndex", 17, ShipmentWrapper.ContainerHumiditySettingIndex);
			AssertEquals("ChargeCodeIndex", 18, ShipmentWrapper.ChargeCodeIndex);
			AssertEquals("ChargeDescriptionIndex", 19, ShipmentWrapper.ChargeDescriptionIndex);
			AssertEquals("CollectChargesColumnIndex", 20, ShipmentWrapper.CollectChargesColumnIndex);
			AssertEquals("CollectCurrencyColumnIndex", 21, ShipmentWrapper.CollectCurrencyColumnIndex);
			AssertEquals("PrepaidChargesColumnIndex", 22, ShipmentWrapper.PrepaidChargesColumnIndex);
			AssertEquals("PrepaidCurrencyColumnIndex", 23, ShipmentWrapper.PrepaidCurrencyColumnIndex);
			AssertEquals("AllChargesColumnIndex", 44, ShipmentWrapper.AllChargesColumnIndex);
			AssertEquals("AllChargesCurrencyColumnIndex", 45, ShipmentWrapper.AllChargesCurrencyColumnIndex);
			AssertEquals("PackRefNumberColumnIndex", 24, ShipmentWrapper.PackRefNumberColumnIndex);
			AssertEquals("PackDescriptionColumnIndex", 25, ShipmentWrapper.PackDescriptionColumnIndex);
			AssertEquals("PackShortDescriptionColumnIndex", 26, ShipmentWrapper.PackShortDescriptionColumnIndex);
			AssertEquals("PackMarksAndNumbersColumnIndex", 27, ShipmentWrapper.PackMarksAndNumbersColumnIndex);
			AssertEquals("PackUNDGColumnIndex", 28, ShipmentWrapper.PackUNDGColumnIndex);
			AssertEquals("PackCommodityCodeColumnIndex", 29, ShipmentWrapper.PackCommodityCodeColumnIndex);
			AssertEquals("PackCommodityDescriptionColumnIndex", 30, ShipmentWrapper.PackCommodityDescriptionColumnIndex);
			AssertEquals("PackCountColumnIndex", 31, ShipmentWrapper.PackCountColumnIndex);
			AssertEquals("PackCountAndTypeColumnIndex", 32, ShipmentWrapper.PackCountAndTypeColumnIndex);
			AssertEquals("PackWeightColumnIndex", 33, ShipmentWrapper.PackWeightColumnIndex);
			AssertEquals("PackWeightAndUQColumnIndex", 34, ShipmentWrapper.PackWeightAndUQColumnIndex);
			AssertEquals("PackVolumeColumnIndex", 35, ShipmentWrapper.PackVolumeColumnIndex);
			AssertEquals("PackVolumeAndUQColumnIndex", 36, ShipmentWrapper.PackVolumeAndUQColumnIndex);
			AssertEquals("PackLengthColumnIndex", 37, ShipmentWrapper.PackLengthColumnIndex);
			AssertEquals("PackLengthAndUQColumnIndex", 38, ShipmentWrapper.PackLengthAndUQColumnIndex);
			AssertEquals("PackWidthColumnIndex", 39, ShipmentWrapper.PackWidthColumnIndex);
			AssertEquals("PackWidthAndUQColumnIndex", 40, ShipmentWrapper.PackWidthAndUQColumnIndex);
			AssertEquals("PackHeightColumnIndex", 41, ShipmentWrapper.PackHeightColumnIndex);
			AssertEquals("PackHeightAndUQColumnIndex", 42, ShipmentWrapper.PackHeightAndUQColumnIndex);
			AssertEquals("PackAreaColumnIndex", 43, ShipmentWrapper.PackAreaColumnIndex);
			AssertEquals("PackVehicleColorColumnIndex", 44, ShipmentWrapper.PackVehicleColourColumnIndex);
			AssertEquals("PackVehicleMakeColumnIndex", 45, ShipmentWrapper.PackVehicleMakeColumnIndex);
			AssertEquals("PackVehicleModelColumnIndex", 46, ShipmentWrapper.PackVehicleModelColumnIndex);
			AssertEquals("PackVehicleNumberOfDoorsColumnIndex", 47, ShipmentWrapper.PackVehicleNumberOfDoorsColumnIndex);
			AssertEquals("PackVehicleTransmissionColumnIndex", 48, ShipmentWrapper.PackVehicleTransmissionColumnIndex);
			AssertEquals("PackVehicleYearColumnIndex", 49, ShipmentWrapper.PackVehicleYearColumnIndex);
			AssertEquals("PackDescriptionAndUNDGColumnIndex", 50, ShipmentWrapper.PackDescriptionAndUNDGColumnIndex);
			AssertEquals("PackDimensionsColumnIndex", 51, ShipmentWrapper.PackDimensionsColumnIndex);
			AssertEquals("PackVehicleFullDetailsColumnIndex", 52, ShipmentWrapper.PackVehicleFullDetailsColumnIndex);
			AssertEquals("PackRefNumberAndMarksAndNumbersColumnIndex", 53, ShipmentWrapper.PackRefNumberAndMarksAndNumbersColumnIndex);
			AssertEquals("PackContainsUNDGColumnIndex", 54, ShipmentWrapper.PackContainsUNDGColumnIndex);
			AssertEquals("PackHarmonizedCodeColumnIndex", 55, ShipmentWrapper.PackHarmonizedCodeColumnIndex);
			AssertEquals("ContainerWeightAndUQIndex", 56, ShipmentWrapper.ContainerWeightAndUQIndex);
			AssertEquals("ContainerTareAndUQIndex", 57, ShipmentWrapper.ContainerTareAndUQIndex);
			AssertEquals("ContainerGrossAndUQIndex", 58, ShipmentWrapper.ContainerGrossAndUQIndex);
			AssertEquals("ContainerVolumeAndUQIndex", 59, ShipmentWrapper.ContainerVolumeAndUQIndex);
			AssertEquals("PackUNDGAndShortDescriptionColumnIndex", 60, ShipmentWrapper.PackUNDGAndShortDescriptionColumnIndex);
		}

		public void TestSetBOLLeftPaddingTemplateConstants()
		{
			AssertEquals("MarksAndNumbersLeftPadding", 0, ShipmentWrapper.MarksAndNumbersLeftPadding);
			AssertEquals("PackagesLeftPadding", 1, ShipmentWrapper.PackagesLeftPadding);
			AssertEquals("GoodsDescLeftPadding", 1, ShipmentWrapper.GoodsDescLeftPadding);
			AssertEquals("GrossWeightLeftPadding", 1, ShipmentWrapper.GrossWeightLeftPadding);
			AssertEquals("VolumeMeasurementLeftPadding", 1, ShipmentWrapper.VolumeMeasurementLeftPadding);
			AssertEquals("ContainerNumberLeftPadding", 0, ShipmentWrapper.ContainerNumberLeftPadding);
			AssertEquals("ContainerSealLeftPadding", 1, ShipmentWrapper.ContainerSealLeftPadding);
			AssertEquals("ContainerTypeLeftPadding", 1, ShipmentWrapper.ContainerTypeLeftPadding);
			AssertEquals("ContainerWeightLeftPadding", 1, ShipmentWrapper.ContainerWeightLeftPadding);
			AssertEquals("ContainerTareLeftPadding", 1, ShipmentWrapper.ContainerTareLeftPadding);
			AssertEquals("ContainerGrossLeftPadding", 1, ShipmentWrapper.ContainerGrossLeftPadding);
			AssertEquals("ContainerVolumeLeftPadding", 1, ShipmentWrapper.ContainerVolumeLeftPadding);
			AssertEquals("ContainerPackagesLeftPadding", 1, ShipmentWrapper.ContainerPackagesLeftPadding);
			AssertEquals("ContainerModeLeftPadding", 1, ShipmentWrapper.ContainerModeLeftPadding);
			AssertEquals("ContainerWeightAndUQLeftPadding", 1, ShipmentWrapper.ContainerWeightAndUQLeftPadding);
			AssertEquals("ContainerTareAndUQLeftPadding", 1, ShipmentWrapper.ContainerTareAndUQLeftPadding);
			AssertEquals("ContainerGrossAndUQLeftPadding", 1, ShipmentWrapper.ContainerGrossAndUQLeftPadding);
			AssertEquals("ContainerVolumeAndUQLeftPadding", 1, ShipmentWrapper.ContainerVolumeAndUQLeftPadding);
			AssertEquals("ChargeCodeLeftPadding", 0, ShipmentWrapper.ChargeCodeLeftPadding);
			AssertEquals("ChargeDescriptionLeftPadding", 0, ShipmentWrapper.ChargeDescriptionLeftPadding);
			AssertEquals("CollectChargesColumnLeftPadding", 1, ShipmentWrapper.CollectChargesColumnLeftPadding);
			AssertEquals("CollectCurrencyColumnLeftPadding", 1, ShipmentWrapper.CollectCurrencyColumnLeftPadding);
			AssertEquals("PrepaidChargesColumnLeftPadding", 1, ShipmentWrapper.PrepaidChargesColumnLeftPadding);
			AssertEquals("PrepaidCurrencyColumnLeftPadding", 1, ShipmentWrapper.PrepaidCurrencyColumnLeftPadding);
			AssertEquals("AllChargesColumnLeftPadding", 1, ShipmentWrapper.AllChargesColumnLeftPadding);
			AssertEquals("AllChargesCurrencyColumnLeftPadding", 1, ShipmentWrapper.AllChargesCurrencyColumnLeftPadding);
			AssertEquals("PackRefNumberColumnLeftPadding", 0, ShipmentWrapper.PackRefNumberColumnLeftPadding);
			AssertEquals("PackDescriptionColumnLeftPadding", 0, ShipmentWrapper.PackDescriptionColumnLeftPadding);
			AssertEquals("PackShortDescriptionColumnLeftPadding", 1, ShipmentWrapper.PackShortDescriptionColumnLeftPadding);
			AssertEquals("PackMarksAndNumbersColumnLeftPadding", 1, ShipmentWrapper.PackMarksAndNumbersColumnLeftPadding);
			AssertEquals("PackUNDGColumnLeftPadding", 1, ShipmentWrapper.PackUNDGColumnLeftPadding);
			AssertEquals("PackCommodityCodeColumnLeftPadding", 1, ShipmentWrapper.PackCommodityCodeColumnLeftPadding);
			AssertEquals("PackCommodityDescriptionColumnLeftPadding", 1, ShipmentWrapper.PackCommodityDescriptionColumnLeftPadding);
			AssertEquals("PackCountColumnLeftPadding", 1, ShipmentWrapper.PackCountColumnLeftPadding);
			AssertEquals("PackCountAndTypeColumnLeftPadding", 1, ShipmentWrapper.PackCountAndTypeColumnLeftPadding);
			AssertEquals("PackWeightColumnLeftPadding", 1, ShipmentWrapper.PackWeightColumnLeftPadding);
			AssertEquals("PackWeightAndUQColumnLeftPadding", 1, ShipmentWrapper.PackWeightAndUQColumnLeftPadding);
			AssertEquals("PackVolumeColumnLeftPadding", 1, ShipmentWrapper.PackVolumeColumnLeftPadding);
			AssertEquals("PackVolumeAndUQColumnLeftPadding", 1, ShipmentWrapper.PackVolumeAndUQColumnLeftPadding);
			AssertEquals("PackLengthColumnLeftPadding", 1, ShipmentWrapper.PackLengthColumnLeftPadding);
			AssertEquals("PackLengthAndUQColumnLeftPadding", 1, ShipmentWrapper.PackLengthAndUQColumnLeftPadding);
			AssertEquals("PackWidthColumnLeftPadding", 1, ShipmentWrapper.PackWidthColumnLeftPadding);
			AssertEquals("PackWidthAndUQColumnLeftPadding", 1, ShipmentWrapper.PackWidthAndUQColumnLeftPadding);
			AssertEquals("PackHeightColumnLeftPadding", 1, ShipmentWrapper.PackHeightColumnLeftPadding);
			AssertEquals("PackHeightAndUQColumnLeftPadding", 1, ShipmentWrapper.PackHeightAndUQColumnLeftPadding);
			AssertEquals("PackAreaColumnLeftPadding", 1, ShipmentWrapper.PackAreaColumnLeftPadding);
			AssertEquals("PackHarmonizedCodeColumnLeftPadding", 1, ShipmentWrapper.PackHarmonizedCodeColumnLeftPadding);
			AssertEquals("BOLClauseColumnLeftPadding", 0, ShipmentWrapper.BOLClauseColumnLeftPadding);
			AssertEquals("PackVehicleColorColumnLeftPadding", 1, ShipmentWrapper.PackVehicleColourColumnLeftPadding);
			AssertEquals("PackVehicleMakeColumnLeftPadding", 1, ShipmentWrapper.PackVehicleMakeColumnLeftPadding);
			AssertEquals("PackVehicleModelColumnLeftPadding", 1, ShipmentWrapper.PackVehicleModelColumnLeftPadding);
			AssertEquals("PackVehicleNumberOfDoorsColumnLeftPadding", 1, ShipmentWrapper.PackVehicleNumberOfDoorsColumnLeftPadding);
			AssertEquals("PackVehicleTransmissionColumnLeftPadding", 1, ShipmentWrapper.PackVehicleTransmissionColumnLeftPadding);
			AssertEquals("PackVehicleYearColumnLeftPadding", 1, ShipmentWrapper.PackVehicleYearColumnLeftPadding);
			AssertEquals("PackDescriptionAndUNDGColumnLeftPadding", 1, ShipmentWrapper.PackDescriptionAndUNDGColumnLeftPadding);
			AssertEquals("PackDimensionsColumnLeftPadding", 1, ShipmentWrapper.PackDimensionsColumnLeftPadding);
			AssertEquals("PackVehicleFullDetailsColumnLeftPadding", 1, ShipmentWrapper.PackVehicleFullDetailsColumnLeftPadding);
			AssertEquals("PackRefNumberAndMarksAndNumbersColumnLeftPadding", 0, ShipmentWrapper.PackRefNumberAndMarksAndNumbersColumnLeftPadding);
			AssertEquals("PackContainsUNDGColumnLeftPadding", 1, ShipmentWrapper.PackContainsUNDGColumnLeftPadding);
			AssertEquals("PackUNDGAndShortDescriptionColumnLeftPadding", 1, ShipmentWrapper.PackUNDGAndShortDescriptionColumnLeftPadding);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersLeftPadding, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackagesLeftPadding, 2);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescLeftPadding, 3);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightLeftPadding, 4);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementLeftPadding, 5);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberLeftPadding, 6);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealLeftPadding, 7);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeLeftPadding, 8);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightLeftPadding, 9);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareLeftPadding, 10);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossLeftPadding, 11);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeLeftPadding, 12);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesLeftPadding, 13);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeLeftPadding, 14);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeLeftPadding, 15);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionLeftPadding, 16);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnLeftPadding, 17);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectCurrencyColumnLeftPadding, 18);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnLeftPadding, 19);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidCurrencyColumnLeftPadding, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesColumnLeftPadding, 42);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesCurrencyColumnLeftPadding, 43);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberColumnLeftPadding, 21);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionColumnLeftPadding, 22);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackShortDescriptionColumnLeftPadding, 23);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackMarksAndNumbersColumnLeftPadding, 24);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGColumnLeftPadding, 25);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityCodeColumnLeftPadding, 26);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityDescriptionColumnLeftPadding, 27);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCountColumnLeftPadding, 28);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCountAndTypeColumnLeftPadding, 29);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightColumnLeftPadding, 30);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightAndUQColumnLeftPadding, 31);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeColumnLeftPadding, 32);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeAndUQColumnLeftPadding, 33);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthColumnLeftPadding, 34);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthAndUQColumnLeftPadding, 35);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthColumnLeftPadding, 36);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthAndUQColumnLeftPadding, 37);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightColumnLeftPadding, 38);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightAndUQColumnLeftPadding, 39);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackAreaColumnLeftPadding, 40);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.BOLClauseColumnLeftPadding, 41);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleColorColumnLeftPadding, 42);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleMakeColumnLeftPadding, 43);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleModelColumnLeftPadding, 44);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleNumberOfDoorsColumnLeftPadding, 45);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleTransmissionColumnLeftPadding, 46);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleYearColumnLeftPadding, 47);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionAndUNDGColumnLeftPadding, 48);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDimensionsColumnLeftPadding, 49);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleFullDetailsColumnLeftPadding, 50);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberAndMarksAndNumbersColumnLeftPadding, 51);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackContainsUNDGColumnLeftPadding, 52);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHarmonizedCodeColumnLeftPadding, 53);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndUQLeftPadding, 54);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndUQLeftPadding, 55);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndUQLeftPadding, 56);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndUQLeftPadding, 57);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGAndShortDescriptionColumnLeftPadding, 58);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("MarksAndNumbersLeftPadding", 1, ShipmentWrapper.MarksAndNumbersLeftPadding);
			AssertEquals("PackagesLeftPadding", 2, ShipmentWrapper.PackagesLeftPadding);
			AssertEquals("GoodsDescLeftPadding", 3, ShipmentWrapper.GoodsDescLeftPadding);
			AssertEquals("GrossWeightLeftPadding", 4, ShipmentWrapper.GrossWeightLeftPadding);
			AssertEquals("VolumeMeasurementLeftPadding", 5, ShipmentWrapper.VolumeMeasurementLeftPadding);
			AssertEquals("ContainerNumberLeftPadding", 6, ShipmentWrapper.ContainerNumberLeftPadding);
			AssertEquals("ContainerSealLeftPadding", 7, ShipmentWrapper.ContainerSealLeftPadding);
			AssertEquals("ContainerTypeLeftPadding", 8, ShipmentWrapper.ContainerTypeLeftPadding);
			AssertEquals("ContainerWeightLeftPadding", 9, ShipmentWrapper.ContainerWeightLeftPadding);
			AssertEquals("ContainerTareLeftPadding", 10, ShipmentWrapper.ContainerTareLeftPadding);
			AssertEquals("ContainerGrossLeftPadding", 11, ShipmentWrapper.ContainerGrossLeftPadding);
			AssertEquals("ContainerVolumeLeftPadding", 12, ShipmentWrapper.ContainerVolumeLeftPadding);
			AssertEquals("ContainerPackagesLeftPadding", 13, ShipmentWrapper.ContainerPackagesLeftPadding);
			AssertEquals("ContainerModeLeftPadding", 14, ShipmentWrapper.ContainerModeLeftPadding);
			AssertEquals("ChargeCodeLeftPadding", 15, ShipmentWrapper.ChargeCodeLeftPadding);
			AssertEquals("ChargeDescriptionLeftPadding", 16, ShipmentWrapper.ChargeDescriptionLeftPadding);
			AssertEquals("CollectChargesColumnLeftPadding", 17, ShipmentWrapper.CollectChargesColumnLeftPadding);
			AssertEquals("CollectCurrencyColumnLeftPadding", 18, ShipmentWrapper.CollectCurrencyColumnLeftPadding);
			AssertEquals("PrepaidChargesColumnLeftPadding", 19, ShipmentWrapper.PrepaidChargesColumnLeftPadding);
			AssertEquals("PrepaidCurrencyColumnLeftPadding", 20, ShipmentWrapper.PrepaidCurrencyColumnLeftPadding);
			AssertEquals("AllChargesColumnLeftPadding", 42, ShipmentWrapper.AllChargesColumnLeftPadding);
			AssertEquals("AllChargesCurrencyColumnLeftPadding", 43, ShipmentWrapper.AllChargesCurrencyColumnLeftPadding);
			AssertEquals("PackRefNumberColumnLeftPadding", 21, ShipmentWrapper.PackRefNumberColumnLeftPadding);
			AssertEquals("PackDescriptionColumnLeftPadding", 22, ShipmentWrapper.PackDescriptionColumnLeftPadding);
			AssertEquals("PackShortDescriptionColumnLeftPadding", 23, ShipmentWrapper.PackShortDescriptionColumnLeftPadding);
			AssertEquals("PackMarksAndNumbersColumnLeftPadding", 24, ShipmentWrapper.PackMarksAndNumbersColumnLeftPadding);
			AssertEquals("PackUNDGColumnLeftPadding", 25, ShipmentWrapper.PackUNDGColumnLeftPadding);
			AssertEquals("PackCommodityCodeColumnLeftPadding", 26, ShipmentWrapper.PackCommodityCodeColumnLeftPadding);
			AssertEquals("PackCommodityDescriptionColumnLeftPadding", 27, ShipmentWrapper.PackCommodityDescriptionColumnLeftPadding);
			AssertEquals("PackCountColumnLeftPadding", 28, ShipmentWrapper.PackCountColumnLeftPadding);
			AssertEquals("PackCountAndTypeColumnLeftPadding", 29, ShipmentWrapper.PackCountAndTypeColumnLeftPadding);
			AssertEquals("PackWeightColumnLeftPadding", 30, ShipmentWrapper.PackWeightColumnLeftPadding);
			AssertEquals("PackWeightAndUQColumnLeftPadding", 31, ShipmentWrapper.PackWeightAndUQColumnLeftPadding);
			AssertEquals("PackVolumeColumnLeftPadding", 32, ShipmentWrapper.PackVolumeColumnLeftPadding);
			AssertEquals("PackVolumeAndUQColumnLeftPadding", 33, ShipmentWrapper.PackVolumeAndUQColumnLeftPadding);
			AssertEquals("PackLengthColumnLeftPadding", 34, ShipmentWrapper.PackLengthColumnLeftPadding);
			AssertEquals("PackLengthAndUQColumnLeftPadding", 35, ShipmentWrapper.PackLengthAndUQColumnLeftPadding);
			AssertEquals("PackWidthColumnLeftPadding", 36, ShipmentWrapper.PackWidthColumnLeftPadding);
			AssertEquals("PackWidthAndUQColumnLeftPadding", 37, ShipmentWrapper.PackWidthAndUQColumnLeftPadding);
			AssertEquals("PackHeightColumnLeftPadding", 38, ShipmentWrapper.PackHeightColumnLeftPadding);
			AssertEquals("PackHeightAndUQColumnLeftPadding", 39, ShipmentWrapper.PackHeightAndUQColumnLeftPadding);
			AssertEquals("PackAreaColumnLeftPadding", 40, ShipmentWrapper.PackAreaColumnLeftPadding);
			AssertEquals("BOLClauseColumnLeftPadding", 41, ShipmentWrapper.BOLClauseColumnLeftPadding);
			AssertEquals("PackVehicleColorColumnLeftPadding", 42, ShipmentWrapper.PackVehicleColourColumnLeftPadding);
			AssertEquals("PackVehicleMakeColumnLeftPadding", 43, ShipmentWrapper.PackVehicleMakeColumnLeftPadding);
			AssertEquals("PackVehicleModelColumnLeftPadding", 44, ShipmentWrapper.PackVehicleModelColumnLeftPadding);
			AssertEquals("PackVehicleNumberOfDoorsColumnLeftPadding", 45, ShipmentWrapper.PackVehicleNumberOfDoorsColumnLeftPadding);
			AssertEquals("PackVehicleTransmissionColumnLeftPadding", 46, ShipmentWrapper.PackVehicleTransmissionColumnLeftPadding);
			AssertEquals("PackVehicleYearColumnLeftPadding", 47, ShipmentWrapper.PackVehicleYearColumnLeftPadding);
			AssertEquals("PackDescriptionAndUNDGColumnLeftPadding", 48, ShipmentWrapper.PackDescriptionAndUNDGColumnLeftPadding);
			AssertEquals("PackDimensionsColumnLeftPadding", 49, ShipmentWrapper.PackDimensionsColumnLeftPadding);
			AssertEquals("PackVehicleFullDetailsColumnLeftPadding", 50, ShipmentWrapper.PackVehicleFullDetailsColumnLeftPadding);
			AssertEquals("PackRefNumberAndMarksAndNumbersColumnLeftPadding", 51, ShipmentWrapper.PackRefNumberAndMarksAndNumbersColumnLeftPadding);
			AssertEquals("PackContainsUNDGColumnLeftPadding", 52, ShipmentWrapper.PackContainsUNDGColumnLeftPadding);
			AssertEquals("PackHarmonizedCodeColumnLeftPadding", 53, ShipmentWrapper.PackHarmonizedCodeColumnLeftPadding);
			AssertEquals("ContainerWeightAndUQLeftPadding", 54, ShipmentWrapper.ContainerWeightAndUQLeftPadding);
			AssertEquals("ContainerTareAndUQLeftPadding", 55, ShipmentWrapper.ContainerTareAndUQLeftPadding);
			AssertEquals("ContainerGrossAndUQLeftPadding", 56, ShipmentWrapper.ContainerGrossAndUQLeftPadding);
			AssertEquals("ContainerVolumeAndUQLeftPadding", 57, ShipmentWrapper.ContainerVolumeAndUQLeftPadding);
			AssertEquals("PackUNDGAndShortDescriptionColumnLeftPadding", 58, ShipmentWrapper.PackUNDGAndShortDescriptionColumnLeftPadding);
		}

		public void TestSetBOLColumnCaptionTemplateConstants()
		{
			AssertEquals("MarksAndNumbersCaption", "Marks & Numbers", ShipmentWrapper.MarksAndNumbersCaption);
			AssertEquals("PackagesCaption", "Packs", ShipmentWrapper.PackagesCaption);
			AssertEquals("GoodsDescCaption", "Goods Description", ShipmentWrapper.GoodsDescCaption);
			AssertEquals("GrossWeightCaption", "Gross Wt", ShipmentWrapper.GrossWeightCaption);
			AssertEquals("VolumeMeasurementCaption", "Volume", ShipmentWrapper.VolumeMeasurementCaption);
			AssertEquals("ContainerNumberCaption", "Cn. No", ShipmentWrapper.ContainerNumberCaption);
			AssertEquals("ContainerSealCaption", "Seal", ShipmentWrapper.ContainerSealCaption);
			AssertEquals("ContainerTypeCaption", "Type", ShipmentWrapper.ContainerTypeCaption);
			AssertEquals("ContainerWeightCaption", "Net (kg)", ShipmentWrapper.ContainerWeightCaption);
			AssertEquals("ContainerTareCaption", "Tare (kg)", ShipmentWrapper.ContainerTareCaption);
			AssertEquals("ContainerGrossCaption", "Gross (kg)", ShipmentWrapper.ContainerGrossCaption);
			AssertEquals("ContainerVolumeCaption", "Volume (M3)", ShipmentWrapper.ContainerVolumeCaption);
			AssertEquals("ContainerPackagesCaption", "Packs", ShipmentWrapper.ContainerPackagesCaption);
			AssertEquals("ContainerModeCaption", "Mode", ShipmentWrapper.ContainerModeCaption);
			AssertEquals("ContainerTemperatureSettingCaption", "Temp.", ShipmentWrapper.ContainerTemperatureSettingCaption);
			AssertEquals("ContainerHumiditySettingCaption", "Humidity", ShipmentWrapper.ContainerHumiditySettingCaption);
			AssertEquals("ContainerWeightAndUQCaption", "Net", ShipmentWrapper.ContainerWeightAndUQCaption);
			AssertEquals("ContainerTareAndUQCaption", "Tare", ShipmentWrapper.ContainerTareAndUQCaption);
			AssertEquals("ContainerGrossAndUQCaption", "Gross", ShipmentWrapper.ContainerGrossAndUQCaption);
			AssertEquals("ContainerVolumeAndUQCaption", "Volume", ShipmentWrapper.ContainerVolumeAndUQCaption);
			AssertEquals("ChargeCodeCaption", "Code", ShipmentWrapper.ChargeCodeCaption);
			AssertEquals("ChargeDescriptionCaption", "Charge Description", ShipmentWrapper.ChargeDescriptionCaption);
			AssertEquals("CollectChargesColumnCaption", "Collect", ShipmentWrapper.CollectChargesColumnCaption);
			AssertEquals("CollectCurrencyColumnCaption", "", ShipmentWrapper.CollectCurrencyColumnCaption);
			AssertEquals("PrepaidChargesColumnCaption", "Prepaid", ShipmentWrapper.PrepaidChargesColumnCaption);
			AssertEquals("PrepaidCurrencyColumnCaption", "", ShipmentWrapper.PrepaidCurrencyColumnCaption);
			AssertEquals("AllChargesColumnCaption", "Amount", ShipmentWrapper.AllChargesColumnCaption);
			AssertEquals("AllChargesCurrencyColumnCaption", "", ShipmentWrapper.AllChargesCurrencyColumnCaption);
			AssertEquals("PackRefNumberColumnCaption", "VIN/Serial", ShipmentWrapper.PackRefNumberColumnCaption);
			AssertEquals("PackDescriptionColumnCaption", "Description", ShipmentWrapper.PackDescriptionColumnCaption);
			AssertEquals("PackShortDescriptionColumnCaption", "Description", ShipmentWrapper.PackShortDescriptionColumnCaption);
			AssertEquals("PackMarksAndNumbersColumnCaption", "Marks & Numbers", ShipmentWrapper.PackMarksAndNumbersColumnCaption);
			AssertEquals("PackUNDGColumnCaption", "UNDG", ShipmentWrapper.PackUNDGColumnCaption);
			AssertEquals("PackCommodityCodeColumnCaption", "Code", ShipmentWrapper.PackCommodityCodeColumnCaption);
			AssertEquals("PackCommodityDescriptionColumnCaption", "Commodity", ShipmentWrapper.PackCommodityDescriptionColumnCaption);
			AssertEquals("PackCountColumnCaption", "Count", ShipmentWrapper.PackCountColumnCaption);
			AssertEquals("PackCountAndTypeColumnCaption", "Packs", ShipmentWrapper.PackCountAndTypeColumnCaption);
			AssertEquals("PackWeightColumnCaption", "Weight (KG)", ShipmentWrapper.PackWeightColumnCaption);
			AssertEquals("PackWeightAndUQColumnCaption", "Weight", ShipmentWrapper.PackWeightAndUQColumnCaption);
			AssertEquals("PackVolumeColumnCaption", "Volume (M3)", ShipmentWrapper.PackVolumeColumnCaption);
			AssertEquals("PackVolumeAndUQColumnCaption", "Volume", ShipmentWrapper.PackVolumeAndUQColumnCaption);
			AssertEquals("PackLengthColumnCaption", "Length (M)", ShipmentWrapper.PackLengthColumnCaption);
			AssertEquals("PackLengthAndUQColumnCaption", "Length", ShipmentWrapper.PackLengthAndUQColumnCaption);
			AssertEquals("PackWidthColumnCaption", "Width (M)", ShipmentWrapper.PackWidthColumnCaption);
			AssertEquals("PackWidthAndUQColumnCaption", "Width", ShipmentWrapper.PackWidthAndUQColumnCaption);
			AssertEquals("PackHeightColumnCaption", "Height (M)", ShipmentWrapper.PackHeightColumnCaption);
			AssertEquals("PackHeightAndUQColumnCaption", "Height", ShipmentWrapper.PackHeightAndUQColumnCaption);
			AssertEquals("PackHarmonizedCodeColumnCaption", "Harmonized Code", ShipmentWrapper.PackHarmonizedCodeColumnCaption);
			AssertEquals("PackAreaColumnCaption", "M2", ShipmentWrapper.PackAreaColumnCaption);
			AssertEquals("PackVehicleColorColumnCaption", "Color", ShipmentWrapper.PackVehicleColourColumnCaption);
			AssertEquals("PackVehicleMakeColumnCaption", "Make", ShipmentWrapper.PackVehicleMakeColumnCaption);
			AssertEquals("PackVehicleModelColumnCaption", "Model", ShipmentWrapper.PackVehicleModelColumnCaption);
			AssertEquals("PackVehicleNumberOfDoorsColumnCaption", "Doors", ShipmentWrapper.PackVehicleNumberOfDoorsColumnCaption);
			AssertEquals("PackVehicleTransmissionColumnCaption", "Trans.", ShipmentWrapper.PackVehicleTransmissionColumnCaption);
			AssertEquals("PackVehicleYearColumnCaption", "Year", ShipmentWrapper.PackVehicleYearColumnCaption);
			AssertEquals("BOLClauseColumnCaption", "BOL Clause", ShipmentWrapper.BOLClauseColumnCaption);
			AssertEquals("PackDescriptionAndUNDGColumnCaption", "Description", ShipmentWrapper.PackDescriptionAndUNDGColumnCaption);
			AssertEquals("PackDimensionsColumnCaption", "Vol M3 / Dims M", ShipmentWrapper.PackDimensionsColumnCaption);
			AssertEquals("PackVehicleFullDetailsColumnCaption", "Vehicle Details", ShipmentWrapper.PackVehicleFullDetailsColumnCaption);
			AssertEquals("PackRefNumberAndMarksAndNumbersColumnCaption", "VIN/Serial", ShipmentWrapper.PackRefNumberAndMarksAndNumbersColumnCaption);
			AssertEquals("PackContainsUNDGColumnCaption", "UNDG", ShipmentWrapper.PackContainsUNDGColumnCaption);
			AssertEquals("PackUNDGAndShortDescriptionColumnCaption", "Description", ShipmentWrapper.PackUNDGAndShortDescriptionColumnCaption);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersCaption, "Caption1");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackagesCaption, "Caption2");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescCaption, "Caption3");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightCaption, "Caption4");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementCaption, "Caption5");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberCaption, "Caption6");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealCaption, "Caption7");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeCaption, "Caption8");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightCaption, "Caption9");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareCaption, "Caption10");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossCaption, "Caption11");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeCaption, "Caption12");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesCaption, "Caption13");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeCaption, "Caption14");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTemperatureSettingCaption, "Caption15");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerHumiditySettingCaption, "Caption16");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeCaption, "Caption17");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionCaption, "Caption18");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnCaption, "Caption19");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectCurrencyColumnCaption, "Caption20");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnCaption, "Caption21");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidCurrencyColumnCaption, "Caption22");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesColumnCaption, "Caption22a");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesCurrencyColumnCaption, "Caption22b");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberColumnCaption, "Caption23");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionColumnCaption, "Caption24");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackShortDescriptionColumnCaption, "Caption25");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackMarksAndNumbersColumnCaption, "Caption26");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGColumnCaption, "Caption27");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityCodeColumnCaption, "Caption28");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityDescriptionColumnCaption, "Caption29");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCountColumnCaption, "Caption30");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackCountAndTypeColumnCaption, "Caption31");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightColumnCaption, "Caption32");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightAndUQColumnCaption, "Caption33");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeColumnCaption, "Caption34");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeAndUQColumnCaption, "Caption35");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthColumnCaption, "Caption36");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthAndUQColumnCaption, "Caption37");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthColumnCaption, "Caption38");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthAndUQColumnCaption, "Caption39");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightColumnCaption, "Caption40");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightAndUQColumnCaption, "Caption41");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackAreaColumnCaption, "Caption42");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleColorColumnCaption, "Caption43");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleMakeColumnCaption, "Caption44");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleModelColumnCaption, "Caption45");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleNumberOfDoorsColumnCaption, "Caption46");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleTransmissionColumnCaption, "Caption47");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleYearColumnCaption, "Caption48");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.BOLClauseColumnCaption, "Caption49");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionAndUNDGColumnCaption, "Caption50");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackDimensionsColumnCaption, "Caption51");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleFullDetailsColumnCaption, "Caption52");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberAndMarksAndNumbersColumnCaption, "Caption53");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackContainsUNDGColumnCaption, "Caption54");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackHarmonizedCodeColumnCaption, "Caption55");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndUQCaption, "Caption56");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndUQCaption, "Caption57");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndUQCaption, "Caption58");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndUQCaption, "Caption59");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGAndShortDescriptionColumnCaption, "Caption60");

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("MarksAndNumbersCaption", "Caption1", ShipmentWrapper.MarksAndNumbersCaption);
			AssertEquals("PackagesCaption", "Caption2", ShipmentWrapper.PackagesCaption);
			AssertEquals("GoodsDescCaption", "Caption3", ShipmentWrapper.GoodsDescCaption);
			AssertEquals("GrossWeightCaption", "Caption4", ShipmentWrapper.GrossWeightCaption);
			AssertEquals("VolumeMeasurementCaption", "Caption5", ShipmentWrapper.VolumeMeasurementCaption);
			AssertEquals("ContainerNumberCaption", "Caption6", ShipmentWrapper.ContainerNumberCaption);
			AssertEquals("ContainerSealCaption", "Caption7", ShipmentWrapper.ContainerSealCaption);
			AssertEquals("ContainerTypeCaption", "Caption8", ShipmentWrapper.ContainerTypeCaption);
			AssertEquals("ContainerWeightCaption", "Caption9", ShipmentWrapper.ContainerWeightCaption);
			AssertEquals("ContainerTareCaption", "Caption10", ShipmentWrapper.ContainerTareCaption);
			AssertEquals("ContainerGrossCaption", "Caption11", ShipmentWrapper.ContainerGrossCaption);
			AssertEquals("ContainerVolumeCaption", "Caption12", ShipmentWrapper.ContainerVolumeCaption);
			AssertEquals("ContainerPackagesCaption", "Caption13", ShipmentWrapper.ContainerPackagesCaption);
			AssertEquals("ContainerModeCaption", "Caption14", ShipmentWrapper.ContainerModeCaption);
			AssertEquals("ContainerTemperatureSettingCaption", "Caption15", ShipmentWrapper.ContainerTemperatureSettingCaption);
			AssertEquals("ContainerHumiditySettingCaption", "Caption16", ShipmentWrapper.ContainerHumiditySettingCaption);
			AssertEquals("ChargeCodeCaption", "Caption17", ShipmentWrapper.ChargeCodeCaption);
			AssertEquals("ChargeDescriptionCaption", "Caption18", ShipmentWrapper.ChargeDescriptionCaption);
			AssertEquals("CollectChargesColumnCaption", "Caption19", ShipmentWrapper.CollectChargesColumnCaption);
			AssertEquals("CollectCurrencyColumnCaption", "Caption20", ShipmentWrapper.CollectCurrencyColumnCaption);
			AssertEquals("PrepaidChargesColumnCaption", "Caption21", ShipmentWrapper.PrepaidChargesColumnCaption);
			AssertEquals("PrepaidCurrencyColumnCaption", "Caption22", ShipmentWrapper.PrepaidCurrencyColumnCaption);
			AssertEquals("AllChargesColumnCaption", "Caption22a", ShipmentWrapper.AllChargesColumnCaption);
			AssertEquals("AllChargesCurrencyColumnCaption", "Caption22b", ShipmentWrapper.AllChargesCurrencyColumnCaption);
			AssertEquals("PackRefNumberColumnCaption", "Caption23", ShipmentWrapper.PackRefNumberColumnCaption);
			AssertEquals("PackDescriptionColumnCaption", "Caption24", ShipmentWrapper.PackDescriptionColumnCaption);
			AssertEquals("PackShortDescriptionColumnCaption", "Caption25", ShipmentWrapper.PackShortDescriptionColumnCaption);
			AssertEquals("PackMarksAndNumbersColumnCaption", "Caption26", ShipmentWrapper.PackMarksAndNumbersColumnCaption);
			AssertEquals("PackUNDGColumnCaption", "Caption27", ShipmentWrapper.PackUNDGColumnCaption);
			AssertEquals("PackCommodityCodeColumnCaption", "Caption28", ShipmentWrapper.PackCommodityCodeColumnCaption);
			AssertEquals("PackCommodityDescriptionColumnCaption", "Caption29", ShipmentWrapper.PackCommodityDescriptionColumnCaption);
			AssertEquals("PackCountColumnCaption", "Caption30", ShipmentWrapper.PackCountColumnCaption);
			AssertEquals("PackCountAndTypeColumnCaption", "Caption31", ShipmentWrapper.PackCountAndTypeColumnCaption);
			AssertEquals("PackWeightColumnCaption", "Caption32", ShipmentWrapper.PackWeightColumnCaption);
			AssertEquals("PackWeightAndUQColumnCaption", "Caption33", ShipmentWrapper.PackWeightAndUQColumnCaption);
			AssertEquals("PackVolumeColumnCaption", "Caption34", ShipmentWrapper.PackVolumeColumnCaption);
			AssertEquals("PackVolumeAndUQColumnCaption", "Caption35", ShipmentWrapper.PackVolumeAndUQColumnCaption);
			AssertEquals("PackLengthColumnCaption", "Caption36", ShipmentWrapper.PackLengthColumnCaption);
			AssertEquals("PackLengthAndUQColumnCaption", "Caption37", ShipmentWrapper.PackLengthAndUQColumnCaption);
			AssertEquals("PackWidthColumnCaption", "Caption38", ShipmentWrapper.PackWidthColumnCaption);
			AssertEquals("PackWidthAndUQColumnCaption", "Caption39", ShipmentWrapper.PackWidthAndUQColumnCaption);
			AssertEquals("PackHeightColumnCaption", "Caption40", ShipmentWrapper.PackHeightColumnCaption);
			AssertEquals("PackHeightAndUQColumnCaption", "Caption41", ShipmentWrapper.PackHeightAndUQColumnCaption);
			AssertEquals("PackAreaColumnCaption", "Caption42", ShipmentWrapper.PackAreaColumnCaption);
			AssertEquals("PackVehicleColorColumnCaption", "Caption43", ShipmentWrapper.PackVehicleColourColumnCaption);
			AssertEquals("PackVehicleMakeColumnCaption", "Caption44", ShipmentWrapper.PackVehicleMakeColumnCaption);
			AssertEquals("PackVehicleModelColumnCaption", "Caption45", ShipmentWrapper.PackVehicleModelColumnCaption);
			AssertEquals("PackVehicleNumberOfDoorsColumnCaption", "Caption46", ShipmentWrapper.PackVehicleNumberOfDoorsColumnCaption);
			AssertEquals("PackVehicleTransmissionColumnCaption", "Caption47", ShipmentWrapper.PackVehicleTransmissionColumnCaption);
			AssertEquals("PackVehicleYearColumnCaption", "Caption48", ShipmentWrapper.PackVehicleYearColumnCaption);
			AssertEquals("BOLClauseColumnCaption", "Caption49", ShipmentWrapper.BOLClauseColumnCaption);
			AssertEquals("PackDescriptionAndUNDGColumnCaption", "Caption50", ShipmentWrapper.PackDescriptionAndUNDGColumnCaption);
			AssertEquals("PackDimensionsColumnCaption", "Caption51", ShipmentWrapper.PackDimensionsColumnCaption);
			AssertEquals("PackVehicleFullDetailsColumnCaption", "Caption52", ShipmentWrapper.PackVehicleFullDetailsColumnCaption);
			AssertEquals("PackRefNumberAndMarksAndNumbersColumnCaption", "Caption53", ShipmentWrapper.PackRefNumberAndMarksAndNumbersColumnCaption);
			AssertEquals("PackContainsUNDGColumnCaption", "Caption54", ShipmentWrapper.PackContainsUNDGColumnCaption);
			AssertEquals("PackHarmonizedCodeColumnCaption", "Caption55", ShipmentWrapper.PackHarmonizedCodeColumnCaption);
			AssertEquals("ContainerWeightAndUQCaption", "Caption56", ShipmentWrapper.ContainerWeightAndUQCaption);
			AssertEquals("ContainerTareAndUQCaption", "Caption57", ShipmentWrapper.ContainerTareAndUQCaption);
			AssertEquals("ContainerGrossAndUQCaption", "Caption58", ShipmentWrapper.ContainerGrossAndUQCaption);
			AssertEquals("ContainerVolumeAndUQCaption", "Caption59", ShipmentWrapper.ContainerVolumeAndUQCaption);
			AssertEquals("PackUNDGAndShortDescriptionColumnCaption", "Caption60", ShipmentWrapper.PackUNDGAndShortDescriptionColumnCaption);
		}

		public void TestSetTemplateConstants()
		{
			AssertEquals("MarksAndNumbersWidth", 1, ShipmentWrapper.MarksAndNumbersWidth);
			AssertEquals("GoodsDescriptionWidth", 1, ShipmentWrapper.GoodsDescWidth);
			AssertEquals("PackagesWidth", 1, ShipmentWrapper.PackagesWidth);
			AssertEquals("MarksAndNumbersAndGoodsDescriptionHeight", 1, ShipmentWrapper.MarksAndNumbsAndDescHeight);
			AssertEquals("MarksAndNumberHeight", 1, ShipmentWrapper.MarksAndNumbersHeight);
			AssertEquals("GoodsDescriptionHeight", 1, ShipmentWrapper.GoodsDescriptionHeight);
			AssertEquals("NumberOfContainerRows", 1, ShipmentWrapper.NoOfContainerRows);
			AssertEquals("GrossWeightWidth", 10, ShipmentWrapper.GrossWeightWidth);
			AssertEquals("VolumeMeasurementWidth", 10, ShipmentWrapper.VolumeMeasurementWidth);

			AssertEquals("NoOfChargesRows", 0, ShipmentWrapper.NoOfCollectChargesRows);
			AssertEquals("CollectChargesColumnWidth", 11, ShipmentWrapper.CollectChargesColumnWidth);
			AssertEquals("PrepaidChargesColumnWidth", 11, ShipmentWrapper.PrepaidChargesColumnWidth);

			AssertEquals("IncludePackageCountInBOLGoodsDescription", 1, ShipmentWrapper.IncludePackageCountInBOLGoodsDescription);
			AssertEquals("NoOfTransportPlanningRows", 1, ShipmentWrapper.NoOfTransportPlanningRows);
			AssertEquals("MarksAndNumbersAndGoodsDescriptionGap", 3, ShipmentWrapper.MarksAndNumbersAndDescGap);
			AssertEquals("GoodsDescriptionAndGrossWeightGap", 0, ShipmentWrapper.GoodsDescriptionAndGrossWeightGap);
			AssertEquals("GrossWeightAndMeasurementGap", 0, ShipmentWrapper.GrossWeightAndMeasurementGap);
			AssertEquals("ChargesWidth", 1, ShipmentWrapper.CollectChargesWidth);
			AssertEquals("ShowPackageCount", 0, ShipmentWrapper.ShowPackageCount);

			AssertEquals("ShowContainerSeal", 1, ShipmentWrapper.ShowContainerSeal);
			AssertEquals("ShowContainerType", 1, ShipmentWrapper.ShowContainerType);
			AssertEquals("ShowContainerWeight", 1, ShipmentWrapper.ShowContainerWeight);
			AssertEquals("ShowContainerVolume", 1, ShipmentWrapper.ShowContainerVolume);
			AssertEquals("ShowContainerPackages", 1, ShipmentWrapper.ShowContainerPackages);
			AssertEquals("ShowContainerMode", 1, ShipmentWrapper.ShowContainerMode);
			AssertEquals("ShowMasterHeadingWithBillNumber", 1, ShipmentWrapper.ShowMasterHeadingWithBillNumber);
			AssertEquals("HBLCode", ZString.Empty, ShipmentWrapper.HBLCode);

			AssertEquals("ChargeInfoDescriptionWidth", 25, ShipmentWrapper.ChargeInfoDescriptionWidth);
			AssertEquals("ChargeInfoGapWidth", 1, ShipmentWrapper.ChargeInfoGapWidth);
			AssertEquals("ChargeInfoAmountWidth", 15, ShipmentWrapper.ChargeInfoAmountWidth);
			AssertEquals("ChargeInfoEnableColumnarFormat", false, ShipmentWrapper.ChargeInfoEnableColumnarFormat);

			AssertEquals("MaxLineLength", 112, ShipmentWrapper.MaxLineLength);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 2);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 3);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumberHeight, 4);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionHeight, 5);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightWidth, 11);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementWidth, 12);

			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoDescriptionWidth, 30);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoGapWidth, 2);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoAmountWidth, 17);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoEnableColumnarFormat, true);

			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 6);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 6);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDescriptionFollowOnPageWidth, 80);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionAndPrepaidChargesGap, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionAndCollectChargesGap, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnWidth, 11);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnWidth, 11);

			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfTransportPlanningRows, 3);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionGap, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionAndGrossWeightGap, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightAndMeasurementGap, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);

			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerSeal, 0);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerType, 0);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerWeight, 0);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerVolume, 0);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerPackages, 0);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerMode, 0);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ShowMasterHeadingWithBillNumber, 0);

			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.HBLCode, "bob");
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MaxLineLength, 33);

			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(constants);
			AssertEquals("MarksAndNumbersWidth", 1, ShipmentWrapper.MarksAndNumbersWidth);
			AssertEquals("GoodsDescriptionWidth", 2, ShipmentWrapper.GoodsDescWidth);
			AssertEquals("GrossWeightWidth", 11, ShipmentWrapper.GrossWeightWidth);
			AssertEquals("VolumeMeasurementWidth", 12, ShipmentWrapper.VolumeMeasurementWidth);
			AssertEquals("MarksAndNumbersAndGoodsDescriptionHeight", 3, ShipmentWrapper.MarksAndNumbsAndDescHeight);
			AssertEquals("MarksAndNumberHeight", 4, ShipmentWrapper.MarksAndNumbersHeight);
			AssertEquals("GoodsDescriptionHeight", 5, ShipmentWrapper.GoodsDescriptionHeight);
			AssertEquals("NumberOfContainerRows", 6, ShipmentWrapper.NoOfContainerRows);

			AssertEquals("NoOfChargesRows", 6, ShipmentWrapper.NoOfCollectChargesRows);
			AssertEquals("MarksAndNumbersAndGoodsDescriptionGap", 1, ShipmentWrapper.MarksAndNumbersAndDescGap);
			AssertEquals("GoodsDescriptionAndGrossWeightGap", 1, ShipmentWrapper.GoodsDescriptionAndGrossWeightGap);
			AssertEquals("CollectChargesColumnWidth", 11, ShipmentWrapper.CollectChargesColumnWidth);
			AssertEquals("PrepaidChargesColumnWidth", 11, ShipmentWrapper.PrepaidChargesColumnWidth);

			AssertEquals("IncludePackageCountInBOLGoodsDescription", 0, ShipmentWrapper.IncludePackageCountInBOLGoodsDescription);
			AssertEquals("NoOfTransportPlanningRows", 3, ShipmentWrapper.NoOfTransportPlanningRows);
			AssertEquals("GrossWeightAndMeasurementGap", 1, ShipmentWrapper.GrossWeightAndMeasurementGap);
			AssertEquals("ChargesWidth", 1, ShipmentWrapper.CollectChargesWidth);
			AssertEquals("ShowPackageCount", 1, ShipmentWrapper.ShowPackageCount);

			AssertEquals("ShowContainerSeal", 0, ShipmentWrapper.ShowContainerSeal);
			AssertEquals("ShowContainerType", 0, ShipmentWrapper.ShowContainerType);
			AssertEquals("ShowContainerWeight", 0, ShipmentWrapper.ShowContainerWeight);
			AssertEquals("ShowContainerVolume", 0, ShipmentWrapper.ShowContainerVolume);
			AssertEquals("ShowContainerPackages", 0, ShipmentWrapper.ShowContainerPackages);
			AssertEquals("ShowContainerMode", 0, ShipmentWrapper.ShowContainerMode);
			AssertEquals("ShowMasterHeadingWithBillNumber", 0, ShipmentWrapper.ShowMasterHeadingWithBillNumber);

			AssertEquals("ChargeInfoDescriptionWidth", 30, ShipmentWrapper.ChargeInfoDescriptionWidth);
			AssertEquals("ChargeInfoGapWidth", 2, ShipmentWrapper.ChargeInfoGapWidth);
			AssertEquals("ChargeInfoAmountWidth", 17, ShipmentWrapper.ChargeInfoAmountWidth);
			AssertEquals("ChargeInfoEnableColumnarFormat", true, ShipmentWrapper.ChargeInfoEnableColumnarFormat);

			AssertEquals("HBLCode", "bob", ShipmentWrapper.HBLCode);
			AssertEquals("MaxLineLength", 33, ShipmentWrapper.MaxLineLength);
		}

		public void TestContainerConstants()
		{
			AssertEquals("ContainerNumberWidth default", 15, ShipmentWrapper.ContainerNumberWidth);
			AssertEquals("ContainerSealWidth default", 20, ShipmentWrapper.ContainerSealWidth);
			AssertEquals("ContainerTypeWidth default", 8, ShipmentWrapper.ContainerTypeWidth);
			AssertEquals("ContainerWeightWidth default", 14, ShipmentWrapper.ContainerWeightWidth);
			AssertEquals("ContainerVolumeWidth default", 14, ShipmentWrapper.ContainerVolumeWidth);
			AssertEquals("ContainerPackagesWidth default", 12, ShipmentWrapper.ContainerPackagesWidth);
			AssertEquals("ContainerModeWidth default", 10, ShipmentWrapper.ContainerModeWidth);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesWidth, 1);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeWidth, 1);

			ShipmentWrapper.SetTemplateConstants(constants);
			AssertEquals("ContainerNumberWidth", 15, ShipmentWrapper.ContainerNumberWidth);
			AssertEquals("ContainerSealWidth", 20, ShipmentWrapper.ContainerSealWidth);
			AssertEquals("ContainerTypeWidth", 8, ShipmentWrapper.ContainerTypeWidth);
			AssertEquals("ContainerWeightWidth", 14, ShipmentWrapper.ContainerWeightWidth);
			AssertEquals("ContainerVolumeWidth", 14, ShipmentWrapper.ContainerVolumeWidth);
			AssertEquals("ContainerPackagesWidth", 12, ShipmentWrapper.ContainerPackagesWidth);
			AssertEquals("ContainerModeWidth", 10, ShipmentWrapper.ContainerModeWidth);

			constants.Clear();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberWidth, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealWidth, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeWidth, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightWidth, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeWidth, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesWidth, 20);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeWidth, 20);

			ShipmentWrapper.SetTemplateConstants(constants);
			AssertEquals("ContainerNumberWidth", 20, ShipmentWrapper.ContainerNumberWidth);
			AssertEquals("ContainerSealWidth", 20, ShipmentWrapper.ContainerSealWidth);
			AssertEquals("ContainerTypeWidth", 20, ShipmentWrapper.ContainerTypeWidth);
			AssertEquals("ContainerWeightWidth", 20, ShipmentWrapper.ContainerWeightWidth);
			AssertEquals("ContainerVolumeWidth", 20, ShipmentWrapper.ContainerVolumeWidth);
			AssertEquals("ContainerPackagesWidth", 20, ShipmentWrapper.ContainerPackagesWidth);
			AssertEquals("ContainerModeWidth", 20, ShipmentWrapper.ContainerModeWidth);
		}

		#endregion

		#region Commodity Tests
		public void TestCommodity()
		{
			var hazardous = Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "HAZ"));
			var general = Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "GEN"));

			AssertNotNull(ShipmentWrapper.Commodity);
			AssertEquals(0, ShipmentWrapper.Commodity.Count);

			var line = Shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals("Commdity is general", general.RH_Description, ShipmentWrapper.Commodity.Description);
			AssertEquals("Commdity code is GEN", general.RH_Code, ShipmentWrapper.Commodity.Code);

			var line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("Commdity is mixed", general.RH_Description + ", " + hazardous.RH_Description, ShipmentWrapper.Commodity.Description);
			AssertEquals("Commdity code is mixed", general.RH_Code + ", " + hazardous.RH_Code, ShipmentWrapper.Commodity.Code);
		}

		public void TestHazCat()
		{
			AssertEquals("", ShipmentWrapper.HazCat);
			AssertEquals("", ShipmentWrapper.HazCatCodeWithIMOHeading);

			var line = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals("", ShipmentWrapper.HazCat);
			AssertEquals("", ShipmentWrapper.HazCatCodeWithIMOHeading);

			var uNDG = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_Code, "2478c"));
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line2.UNDGs.AddNew().DI_DG = uNDG.PK;
			line2.UNDGs[0].Substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code;

			ZString expected = "HAZ - UN2478, ISOCYANATES, FLAMMABLE, TOXIC, N.O.S., class 3 (6.1), PG III, MARINE POLLUTANT";
			AssertEquals(expected, ShipmentWrapper.HazCat);
			AssertEquals("IMO Code: " + expected, ShipmentWrapper.HazCatCodeWithIMOHeading);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line.UNDGs.AddNew().DI_DG = uNDG.PK;
			AssertEquals(expected, ShipmentWrapper.HazCat);
			AssertEquals("IMO Code: " + expected, ShipmentWrapper.HazCatCodeWithIMOHeading);

			UNDGSubstance uNDG2 = UNDGSubstanceLoader.LoadSubstances(Factory, "2357", "", "IMO").First();
			line.UNDGs.AddNew().DI_DG = uNDG2.PK;
			expected = "HAZ - UN2478, ISOCYANATES, FLAMMABLE, TOXIC, N.O.S., class 3 (6.1), PG III, MARINE POLLUTANT; HAZ - UN2357, CYCLOHEXYLAMINE, class 8 (3), PG II, (27.0C c.c.)";
			AssertEquals(expected, ShipmentWrapper.HazCat);
			AssertEquals("IMO Code: " + expected, ShipmentWrapper.HazCatCodeWithIMOHeading);
		}

		public void TestCommodityDescWithHeading()
		{
			var hazardous = Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "HAZ"));
			var general = Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "GEN"));
			var line = Shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals("Type: " + general.RH_Description, ShipmentWrapper.CommodityDescriptionWithHeading);

			var line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals("Type: " + general.RH_Description, ShipmentWrapper.CommodityDescriptionWithHeading);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			UNDGSubstance uNDG = UNDGSubstanceLoader.LoadSubstances(Factory, "2478", "c", "IMO").First();
			line.UNDGs.AddNew().DI_DG = uNDG.PK;
			AssertEquals("Type: " + hazardous.RH_Description + ", " + general.RH_Description + "  " + "IMO Code: " + "HAZ - UN2478, ISOCYANATES, FLAMMABLE, TOXIC, N.O.S., class 3 (6.1), PG III", ShipmentWrapper.CommodityDescriptionWithHeading);

			line.UNDGs[0].Substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code;
			AssertEquals("Type: " + hazardous.RH_Description + ", " + general.RH_Description + "  " + "IMO Code: " + "HAZ - UN2478, ISOCYANATES, FLAMMABLE, TOXIC, N.O.S., class 3 (6.1), PG III, MARINE POLLUTANT", ShipmentWrapper.CommodityDescriptionWithHeading);
		}

		#endregion

		#region Shipment Details Tests

		public void TestBookingCutOffDateFromSailing()
		{
			CommonContainer cont = Factory.New<CommonContainer>();
			ForwardingShipment ship = Factory.New<ForwardingShipment>();
			DocShipment docShip = DocShipment.New(ship, Factory);

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			ZDateTime cutOffDate = ZDateTime.Today.AddDays(8);
			origin.JA_DGCutOff = cutOffDate;
			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_DepotCutOff = cutOffDate;
			ship.JS_JX = sailing.PK;
			AssertEquals("CutOffOrAvailableDate", cutOffDate, docShip.BookingCutOffDate);
		}

		public void TestTransportModeAndPackingMode()
		{
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			CommonConsol consol = GetNewConsol();
			consol.JK_AgentType = "DRT";
			consol.Shipments.Add(Shipment);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("", ShipmentWrapper.TransportModeAndPackingMode);
			consol.JK_AgentType = "AGT";

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			ZString expected = "\r\nTransport Mode " + Core.Constants.TransportModeDescriptions.Sea + "    Container Mode FCL";
			AssertEquals("ShipmentWrapper.TransportModeAndPackingMode Sea", expected, ShipmentWrapper.TransportModeAndPackingMode);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			expected = "\r\nTransport Mode " + Core.Constants.TransportModeDescriptions.Air + "    Container Mode LSE";
			AssertEquals("ShipmentWrapper.TransportModeAndPackingMode Air", expected, ShipmentWrapper.TransportModeAndPackingMode);
		}

		public void TestChargesDisplay()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges;
			AssertEquals("ChargesDisplay", DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges, ShipmentWrapper.ChargesDisplay);

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			AssertEquals("ChargesDisplay", DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges, ShipmentWrapper.ChargesDisplay);

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;
			AssertEquals("ChargesDisplay", DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed, ShipmentWrapper.ChargesDisplay);
		}

		public void TestReceivalDepotReference()
		{
			Shipment.JS_CFSReference = "myref1_";
			AssertEquals("myref1_", ShipmentWrapper.ReceivalDepotReference);

			Shipment.JS_CFSReference = "";
			AssertEquals("", ShipmentWrapper.ReceivalDepotReference);
		}

		public void TestShipmentNoHouseBillNoAndPorts()
		{
			Shipment.JS_UniqueConsignRef = "S00001009";
			Shipment.JS_HouseBill = "HAWB 123456789";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";

			ZString expectedString = "REF: " + Shipment.JS_UniqueConsignRef + ", " + Shipment.JS_HouseBill;
			expectedString += " (" + Shipment.JS_RL_NKOrigin + "/" + Shipment.JS_RL_NKDestination + ")";

			AssertEquals("Shipment No, House Bill No and Ports", expectedString, ShipmentWrapper.ShipmentNoHouseBillNoAndPorts);

			Shipment.JS_HouseBill = Shipment.JS_UniqueConsignRef;
			expectedString = "REF: " + Shipment.JS_UniqueConsignRef;
			expectedString += " (" + Shipment.JS_RL_NKOrigin + "/" + Shipment.JS_RL_NKDestination + ")";

			AssertEquals("Shipment No and Ports", expectedString, ShipmentWrapper.ShipmentNoHouseBillNoAndPorts);
		}

		public void TestClientOwnerOrderReference()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentWrapper = DocShipment.New(shipment, Factory);

			#region Set up ShippersReference

			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_BookingReference = "BookingReference";

			#endregion

			Assert("Precondition: Shippers Reference should not be empty", !ShipmentWrapper.ShippersReference.IsEmpty);

			#region Set up Declaration

			var jobHeaderBisObj = Factory.NewJobForTesting<JobHeader>();
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeaderBisObj.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = shipment.PK;
			jobHeaderBisObj.JH_JobNum = shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			AssertNull("DeclarationForCurrentBranch", ShipmentWrapper.DeclarationForCurrentBranch);

			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec1 = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec1.JE_JS = shipment.PK;
			dec1.JE_GB = jobHeaderBisObj.JH_GB;
			dec1.JE_OwnerRef = "OWNERS REFERENCE";
			Factory.Save();

			AssertNotNull("DeclarationForCurrentBranch", ShipmentWrapper.DeclarationForCurrentBranch);
			AssertEquals("DeclarationForCurrentBranch is of type Enterprise.DocumentWrappers.Customs.AU.DocDeclaration", typeof(Customs.AU.DocDeclaration), ShipmentWrapper.DeclarationForCurrentBranch.GetType());

			GlbCompany.CurrentCompany.SetCountry(storedCompany);

			#endregion

			AssertNotNull("Precondition: Declaration should not be null", ShipmentWrapper.DeclarationForShipmentBranch);
			Assert("Precondition: Declaration's OwnerRef should not be empty", !ShipmentWrapper.DeclarationForShipmentBranch.OwnerRef.IsEmpty);

			#region Set up Order Numbers

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var order1 = shipment.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "Order number 1";
			order1.JD_InvoiceNumber = "123";
			order1.SupplierPK = consignor.PK;

			var order2 = shipment.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "Order number 2";
			order2.JD_InvoiceNumber = "321";
			order2.SupplierPK = consignor.PK;

			#endregion

			Assert("Order numbers", !ShipmentWrapper.OrderNumbers.IsEmpty);

			AssertEquals("CLIENT / OWNER / ORDER REFERENCE", ShipmentWrapper.ClientOwnerOrderReferenceHeading);
			ZString expectedString = ShipmentWrapper.ShippersReference + " / " + ShipmentWrapper.DeclarationForShipmentBranch.OwnerRef + " / " + ShipmentWrapper.OrderNumbers;
			AssertEquals(expectedString, ShipmentWrapper.ClientOwnerOrderReference);

			dec1.JE_OwnerRef = "Order number 1,Order number 2";
			expectedString = ShipmentWrapper.ShippersReference + " / " + ShipmentWrapper.OrderNumbers; // "BookingReference / Order number 1,Order number 2"
			AssertEquals("If Owner reference is duplicate of order number, do not include in string again.", expectedString, ShipmentWrapper.ClientOwnerOrderReference);

			dec1.JE_OwnerRef = "number 1";
			expectedString = ShipmentWrapper.ShippersReference + " / " + ShipmentWrapper.DeclarationForShipmentBranch.OwnerRef + " / " + ShipmentWrapper.OrderNumbers; // "BookingReference / number 1 /Order number 1,Order number 2"
			AssertEquals("Owner reference must be exact match of order to be excluded.", expectedString, ShipmentWrapper.ClientOwnerOrderReference);

			dec1.JE_OwnerRef = "";
			expectedString = ShipmentWrapper.ShippersReference + " / " + ShipmentWrapper.OrderNumbers; // "BookingReference / Order number 1,Order number 2"
			AssertEquals("Owner reference slash delimiter should not show when Owner Ref is empty.", expectedString, ShipmentWrapper.ClientOwnerOrderReference);

			dec1.JE_JS = ZGuid.Empty;

			AssertNull("Declaration is null", ShipmentWrapper.DeclarationForCurrentBranch);
			AssertEquals("CLIENT / ORDER REFERENCE", ShipmentWrapper.ClientOwnerOrderReferenceHeading);
			expectedString = ShipmentWrapper.ShippersReference + " / " + ShipmentWrapper.OrderNumbers;
			AssertEquals(expectedString, ShipmentWrapper.ClientOwnerOrderReference);
		}

		public void TestTransportMode()
		{
			//Please do not change this test / Wrapper.TransportMode as they are used in #if statements in Excel templates
			//so I would need the exact text.
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AIR", ShipmentWrapper.TransportMode);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("SEA", ShipmentWrapper.TransportMode);
		}

		public void TestInitialTransportMode()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			consol.Shipments.Add(Shipment);

			AssertEquals(1, consol.Transports.Count);

			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModes.Air, ShipmentWrapper.InitialTransportMode);

			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(Core.Constants.TransportModes.Sea, ShipmentWrapper.InitialTransportMode);
		}

		public void TestInitialTransportModeDescription()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			consol.Shipments.Add(Shipment);

			AssertEquals(1, consol.Transports.Count);

			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Air Freight", ShipmentWrapper.InitialTransportModeDescription);

			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Sea Freight", ShipmentWrapper.InitialTransportModeDescription);

			consol.Transports[0].JW_TransportMode = "AAA";
			AssertEquals("AAA", ShipmentWrapper.InitialTransportModeDescription);
		}

		public void TestDescriptionForGoods()
		{
			Shipment.JS_GoodsDescription = "Test short desc";
			AssertEquals("Short description", "Test short desc", ShipmentWrapper.DescriptionForGoods);

			Shipment.DetailedGoodsDescriptionNoteText = "Detailed goods description\nLine Two";
			Factory.Save();

			AssertEquals("Detailed description", "Detailed goods description\nLine Two", ShipmentWrapper.DescriptionForGoods);
		}

		public void TestManifestGoodsDescription()
		{
			AssertEquals("Manifest goods description", "", ShipmentWrapper.ManifestGoodsDescription);

			Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description, "Test description");
			Factory.Save();
			AssertEquals("Manifest goods description", "Test description", ShipmentWrapper.ManifestGoodsDescription);
		}

		public void TestManifestDescriptionOfGoods()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CountryExportStatementSettingCollection countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			CountryExportStatementSetting countrySetting = countrySettingCollection.AddNew();
			countrySetting.CountryCode = GlbBranch.CurrentBranch.Country.Code;
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "AES";
			statementSetting.Statement = "STATEMENT FOR TESTING";
			statementSetting.Visibility = "UDF";
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "NZAKL";

			Shipment.JS_GoodsDescription = "Short Description";
			AssertEquals("Manifest description of goods", "Short Description", ShipmentWrapper.ManifestDescriptionOfGoods);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.ManifestDescriptionOfGoods);
			AssertContains("Manifest description of goods", "Short Description", ShipmentWrapper.ManifestDescriptionOfGoods);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description, "Manifest description");
			Factory.Save();
			AssertEquals("Manifest description of goods", "Manifest description", ShipmentWrapper.ManifestDescriptionOfGoods);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.ManifestDescriptionOfGoods);
			AssertContains("Manifest description of goods", "Manifest description", ShipmentWrapper.ManifestDescriptionOfGoods);
		}

		public void TestManifestDetailedDescriptionOfGoods()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CountryExportStatementSettingCollection countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			CountryExportStatementSetting countrySetting = countrySettingCollection.AddNew();
			countrySetting.CountryCode = GlbBranch.CurrentBranch.Country.Code;
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "AES";
			statementSetting.Statement = "STATEMENT FOR TESTING";
			statementSetting.Visibility = "UDF";
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "NZAKL";

			Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Detailed description");
			AssertEquals("Manifest detailed description of goods", "Detailed description", ShipmentWrapper.ManifestDetailedDescriptionOfGoods);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.ManifestDetailedDescriptionOfGoods);
			AssertContains("Manifest detailed description of goods", "Detailed description", ShipmentWrapper.ManifestDetailedDescriptionOfGoods);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description, "Manifest description");
			Factory.Save();
			AssertEquals("Manifest detailed description of goods", "Manifest description", ShipmentWrapper.ManifestDetailedDescriptionOfGoods);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.ManifestDetailedDescriptionOfGoods);
			AssertContains("Manifest detailed description of goods", "Manifest description", ShipmentWrapper.ManifestDetailedDescriptionOfGoods);
		}

		public void TestManifestDetailedDescriptionOfGoodsLine()
		{
			Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description, "blah \n blah");
			Factory.Save();
			AssertEquals("Manifest detailed description of goods", "blah   blah", ShipmentWrapper.ManifestDetailedDescriptionOfGoodsLine);
		}

		public void TestContainers()
		{
			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();

			AssertEquals("Containers should be zero element collection", 0, ShipmentWrapper.Containers.Count);

			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CNSHA";

			CommonContainer arrivalContainer1 = consol.Containers.AddNew();
			arrivalContainer1.JC_ContainerNum = "ARRIVAL1";
			packLine1.SetContainer(consol, arrivalContainer1);

			CommonContainer arrivalContainer2 = consol.Containers.AddNew();
			arrivalContainer1.JC_ContainerNum = "ARRIVAL2";
			packLine2.SetContainer(consol, arrivalContainer2);

			CommonConsol middleConsol = Shipment.Consols.AddNew();
			middleConsol.JK_RL_NKLoadPort = "AUBNE";
			middleConsol.JK_RL_NKDischargePort = "SGSIN";

			CommonContainer middleContainer1 = middleConsol.Containers.AddNew();
			middleContainer1.JC_ContainerNum = "MIDDLE1";
			packLine1.SetContainer(middleConsol, middleContainer1);

			CommonContainer middleContainer2 = middleConsol.Containers.AddNew();
			middleContainer2.JC_ContainerNum = "MIDDLE2";
			packLine2.SetContainer(middleConsol, middleContainer2);

			CommonConsol departureConsol = Shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			CommonContainer departureContainer1 = departureConsol.Containers.AddNew();
			departureContainer1.JC_ContainerNum = "DEPART1";
			packLine1.SetContainer(departureConsol, departureContainer1);

			CommonContainer departureContainer2 = departureConsol.Containers.AddNew();
			departureContainer2.JC_ContainerNum = "DEPART2";
			packLine2.SetContainer(departureConsol, departureContainer2);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));

			AssertEquals("Consol", consol.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);
			AssertEquals("Containers count", 2, ShipmentWrapper.Containers.Count);
			AssertEquals("ArrivalContainer1", arrivalContainer1.JC_ContainerNum, ShipmentWrapper.Containers[0].ContainerNumber);
			AssertEquals("ArrivalContainer2", arrivalContainer2.JC_ContainerNum, ShipmentWrapper.Containers[1].ContainerNumber);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("DepartureConsol", departureConsol.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);
			AssertEquals("Containers count", 2, ShipmentWrapper.Containers.Count);
			AssertEquals("DepartureContainer1", departureContainer1.JC_ContainerNum, ShipmentWrapper.Containers[0].ContainerNumber);
			AssertEquals("DepartureContainer2", departureContainer2.JC_ContainerNum, ShipmentWrapper.Containers[1].ContainerNumber);
		}

		public void TestNotAllocatedWeight_Units()
		{
			CommonConsol consol = Shipment.Consols.AddNew();

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container1.JC_GrossWeight = 3628.739m;

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";
			container2.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container2.JC_GrossWeight = 907.185m;

			Shipment.JS_ActualWeight = 10000m;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;

			PackLine packLine1 = Shipment.OuterPackLines[0];
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packLine1.JL_ActualWeight = 2000m;
			packLine1.SetContainer(container1.PK);

			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packLine2.JL_ActualWeight = 6000m;
			packLine2.SetContainer(container1.PK);

			PackLine packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packLine3.JL_ActualWeight = 2000m;
			packLine3.SetContainer(container2.PK);

			AssertEquals("", ShipmentWrapper.NotAllocatedWeight);
		}

		public void TestNotAllocatedWeightAndVolumeAndPackagesAndIsNotAllocatedPresent()
		{
			PackLine packLine = Shipment.OuterPackLines.AddNew();

			Shipment.JS_ActualWeight = 100.500M;
			Shipment.JS_ActualVolume = 9.050M;
			Shipment.JS_OuterPacks = 15;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			packLine.JL_ActualWeight = 60M;
			packLine.JL_ActualVolume = 5M;
			packLine.JL_PackageCount = 7;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Empty since there is no Arrival consol", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since there is no Arrival consol", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since there is no Arrival consol", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since there is no Arrival consol", false, ShipmentWrapper.IsNotAllocatedPresent);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Empty since there is no Departure consol", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since there is no Departure consol", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since there is no Departure consol", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since there is no Departure consol", false, ShipmentWrapper.IsNotAllocatedPresent);

			CommonConsol consol = Shipment.Consols.AddNew();
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Empty since there are no containers on the consol", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since there are no containers on the consol", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since there is no Arrival consol", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since there is no containers on the consol", false, ShipmentWrapper.IsNotAllocatedPresent);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Empty since there are no containers on the consol", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since there are no containers on the consol", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since there is no Departure consol", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since there is no containers on the consol", false, ShipmentWrapper.IsNotAllocatedPresent);

			CommonContainer container1 = consol.Containers.AddNew();
			packLine.SetContainer(consol, container1);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Displays not allocated weight", "40.5 KG", ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Displays not allocated volume", "4.05 M3", ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Displays not allocated packages", "8", ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("True since there is discrepancy between the Shipment Details & Pack Lines.", true, ShipmentWrapper.IsNotAllocatedPresent);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Displays not allocated weight", "40.5 KG", ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Displays not allocated volume", "4.05 M3", ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Displays not allocated packages", "8", ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("True since there is discrepancy between the Shipment Details & Pack Lines", true, ShipmentWrapper.IsNotAllocatedPresent);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Empty since shipment mode is Bulk", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since shipment mode is Bulk", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since shipment mode is Bulk", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since shipment mode is Bulk", false, ShipmentWrapper.IsNotAllocatedPresent);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Empty since shipment mode is Bulk", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since shipment mode is Bulk", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since shipment mode is Bulk", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since shipment mode is Bulk", false, ShipmentWrapper.IsNotAllocatedPresent);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Empty since shipment mode is Liquid", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since shipment mode is Liquid", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since shipment mode is Liquid", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since shipment mode is Liquid", false, ShipmentWrapper.IsNotAllocatedPresent);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Empty since shipment mode is Liquid", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since shipment mode is Liquid", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since shipment mode is Liquid", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since shipment mode is Liquid", false, ShipmentWrapper.IsNotAllocatedPresent);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			Shipment.OuterPackLines.RemoveAll();
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Empty since there are no outer packlines", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since there are no outer packlines", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since there are no outer packlines", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since there are no outer packlines", false, ShipmentWrapper.IsNotAllocatedPresent);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Empty since there are no outer packlines", ZString.Empty, ShipmentWrapper.NotAllocatedWeight);
			AssertEquals("Empty since there are no outer packlines", ZString.Empty, ShipmentWrapper.NotAllocatedVolume);
			AssertEquals("Empty since there are no outer packlines", ZString.Empty, ShipmentWrapper.NotAllocatedPackages);
			AssertEquals("False since there are no outer packlines", false, ShipmentWrapper.IsNotAllocatedPresent);
		}

		public void TestContainerPackagesCount()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			CommonConsol consol = CreateExportConsol(Shipment);

			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			var container1 = consol.Containers.AddNew();
			packLine1.SetContainer(consol, container1);

			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			var container2 = consol.Containers.AddNew();
			packLine2.SetContainer(consol, container2);

			AssertEquals("Container Count", Shipment.Containers.Count(), ShipmentWrapper.ContainerPackagesCount);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			Shipment.JS_OuterPacks = 1209;
			AssertEquals("Outer packs", Shipment.JS_OuterPacks, ShipmentWrapper.ContainerPackagesCount);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
			AssertEquals("Return Nothing", 0, ShipmentWrapper.ContainerPackagesCount);
		}

		public void TestDateOfIssue()
		{
			Shipment.JS_HouseBillIssueDate = ZDateTime.Empty;
			AssertEquals("House bill issue date is empty", ZDateTime.Empty, ShipmentWrapper.DateOfIssue);

			Shipment.JS_HouseBillIssueDate = ZDateTime.Today;
			AssertEquals("Date part only of house bill issue date", ZDateTime.Today.Date, ShipmentWrapper.DateOfIssue);
		}

		public void TestOnBoardDate()
		{
			Shipment.JS_ShippedOnBoardDate = ZDateTime.Empty;
			AssertEquals("Shipped on board date is empty", ZDateTime.Empty, ShipmentWrapper.OnBoardDate);

			Shipment.JS_ShippedOnBoardDate = ZDateTime.Today;
			AssertEquals("Date part only of shipped on board date", ZDateTime.Today.Date, ShipmentWrapper.OnBoardDate);
		}

		public void TestFirstConsol()
		{
			AssertEquals("No Consols", null, ShipmentWrapper.Consol);

			CommonConsol consol1 = CreateExportConsol(Shipment);
			consol1.JK_UniqueConsignRef = "1";
			AssertEquals("Doc Consol is one", consol1.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);

			CommonConsol consol2 = CreateExportConsol(Shipment);
			consol2.JK_UniqueConsignRef = "2";
			AssertEquals("Doc Consol is one", consol1.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);

			consol1.JK_UniqueConsignRef = "2";
			consol2.JK_UniqueConsignRef = "1";
			AssertEquals("Doc Consol is two", consol2.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);
		}

		public void TestPackages()
		{
			Shipment.JS_OuterPacks = 34;
			Shipment.JS_F3_NKPackType = "PLT";
			Shipment.JS_F3_NKTotalCountPackType = "CTN";

			string innerPack = "12 " + Shipment.JS_F3_NKTotalCountPackType + " (INNER)";
			string outerPack = "34 " + Shipment.JS_F3_NKPackType + " (OUTER)";

			Shipment.JS_TotalPackageCount = 0;

			AssertEquals("Package details shows outer packs only", outerPack, ShipmentWrapper.Packages);

			Shipment = Factory.Load<CommonShipment>(Shipment.PK);
			Shipment.JS_OuterPacks = 0;
			Shipment.JS_TotalPackageCount = 12;

			AssertEquals("Package details shows inner packs only", innerPack, ShipmentWrapper.Packages);

			Shipment.JS_OuterPacks = 34;
			AssertEquals("Package details should show outer and inner packs", outerPack + ", " + innerPack, ShipmentWrapper.Packages);
		}

		public void TestCarrierBookingRef()
		{
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("CarrierBookingRef", ZString.Empty, ShipmentWrapper.CarrierBookingRef);

			CommonConsol consol = Shipment.Consols.AddNew();
			AssertEquals("CarrierBookingRef", ZString.Empty, ShipmentWrapper.CarrierBookingRef);

			consol.JK_BookingReference = "12345";
			AssertEquals("CarrierBookingRef", "12345", ShipmentWrapper.CarrierBookingRef);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("CarrierBookingRef", ZString.Empty, ShipmentWrapper.CarrierBookingRef);
		}

		public void TestOuterPacksPackType()
		{
			AssertEquals("PLT", ShipmentWrapper.OuterPacksPackType);

			Shipment.JS_F3_NKPackType = "BOX";
			AssertEquals("BOX", ShipmentWrapper.OuterPacksPackType);

			Shipment.JS_F3_NKPackType = "XXX";
			AssertEquals("XXX", ShipmentWrapper.OuterPacksPackType);
		}

		public void TestEquipmentType()
		{
			AssertEquals("EquipmentType", ZString.Empty, ShipmentWrapper.EquipmentType);

			Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Shipment.DocsAndCartage.Lookups.PickupEquipmentNeededList[0].Code;
			Shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Shipment.DocsAndCartage.Lookups.DeliveryEquipmentNeededList[0].Code;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			string expected = Shipment.DocsAndCartage.Lookups.PickupEquipmentNeededList[0].Code + " - " + Shipment.DocsAndCartage.Lookups.PickupEquipmentNeededList[0].Description;
			AssertEquals("EquipmentType", expected, ShipmentWrapper.EquipmentType);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			expected = Shipment.DocsAndCartage.Lookups.DeliveryEquipmentNeededList[0].Code + " - " + Shipment.DocsAndCartage.Lookups.DeliveryEquipmentNeededList[0].Description;
			AssertEquals("EquipmentType", expected, ShipmentWrapper.EquipmentType);
		}

		public void TestOuterPacksPackTypeDescription()
		{
			AssertEquals("Pallet(s)", ShipmentWrapper.OuterPacksPackTypeDescription);

			Shipment.JS_F3_NKPackType = "BOX";
			AssertEquals("Box(s)", ShipmentWrapper.OuterPacksPackTypeDescription);

			Shipment.JS_F3_NKPackType = "BAG";
			AssertEquals("Bag(s)", ShipmentWrapper.OuterPacksPackTypeDescription);
		}

		public void TestHXDNumber()
		{
			AssertEquals("HXDNumber", ZString.Empty, ShipmentWrapper.HXDNumber);

			JobRequiredDocument hXDDoc = Shipment.DocsAndCartage.RequiredDocuments.AddNew(Enterprise.Core.Constants.RefDocTypes.HeXiaoDan);
			hXDDoc.EQ_DocNumber = "HXD1234";
			AssertEquals("HXDNumber", "HXD1234", ShipmentWrapper.HXDNumber);
		}

		public void TestJobCharge()
		{
			Shipment.JS_UniqueConsignRef = "9999";

			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "TTT";
			orgHeaderBisObj.OH_FullName = "Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Address 2";

			jobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;
			AccChargeCode testCode1 = CreateChargeCode("TESTCH1");
			AccChargeCode testCode2 = CreateChargeCode("TESTCH2");
			AccChargeCode testCode3 = CreateChargeCode("TESTCH3");

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, testCode1.PK);
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, testCode2.PK);
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName), 20.000M, testCode3.PK);

			Factory.Save();
			AssertEquals(2, ShipmentWrapper.JobHeader.JobChargesForLocalClient.Count);
		}

		public void TestShipmentContainerNumbers()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			AssertEquals("No container numbers", ZString.Empty, ShipmentWrapper.ShipmentContainerNumbers);

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRXU1234567";

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CRXU1234568";

			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container2);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			Assert("Contains Container 1", ShipmentWrapper.ShipmentContainerNumbers.Contains(container1.JC_ContainerNum));
			Assert("Contains Container 2", ShipmentWrapper.ShipmentContainerNumbers.Contains(container2.JC_ContainerNum));
		}

		public void TestOuterPacksDetail()
		{
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("OuterPacksDetail", ZString.Empty, ShipmentWrapper.OuterPacksDetail);

			CommonConsol eXPConsol = Shipment.Consols.AddNew();
			eXPConsol.JK_RL_NKLoadPort = "AUSYD";
			eXPConsol.JK_RL_NKDischargePort = "SGSIN";

			CommonContainer container1 = eXPConsol.Containers.AddNew();
			container1.JC_ContainerNum = "CRXU1234567";
			CommonContainer container2 = eXPConsol.Containers.AddNew();
			container2.JC_ContainerNum = "CRXU1234568";

			CommonConsol iMPConsol = Shipment.Consols.AddNew();
			iMPConsol.JK_RL_NKLoadPort = "SGSIN";
			iMPConsol.JK_RL_NKDischargePort = "USLAX";

			CommonContainer container3 = iMPConsol.Containers.AddNew();
			container3.JC_ContainerNum = "CRXU1234569";

			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			PackLine line1 = Shipment.OuterPackLines.Count > 0 ? Shipment.OuterPackLines[0] : Shipment.OuterPackLines.AddNew();
			line1.JL_ActualWeight = 1500;
			line1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			line1.JL_ActualVolume = 14;
			line1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			line1.JL_PackageCount = 20;
			ZString heading = "CONTAINER/ WGT/ VOL/ PKG" + System.Environment.NewLine;
			ZString expected = "CRXU1234568/ 1500KG/ 14M3/ 20PLT";
			AssertEquals("OuterPacksDetail - packline not attached to container", heading + expected, ShipmentWrapper.OuterPacksDetail);

			line1.SetContainer(eXPConsol, container1);
			line1.SetContainer(iMPConsol, container3);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			expected = "CRXU1234567/ 1500KG/ 14M3/ 20PLT";
			AssertEquals("OuterPacksDetail - container from export consol", heading + expected, ShipmentWrapper.OuterPacksDetail);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			expected = "CRXU1234569/ 1500KG/ 14M3/ 20PLT";
			AssertEquals("OuterPacksDetail - container from import consol", heading + expected, ShipmentWrapper.OuterPacksDetail);

			PackLine line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_ActualWeight = 2;
			line2.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			line2.JL_ActualVolume = 13470;
			line2.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			line2.JL_PackageCount = 12;

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			expected = "CRXU1234567/ 1500KG/ 14M3/ 20PLT" + System.Environment.NewLine + "CRXU1234568/ 2T/ 13470L/ 12PLT";
			AssertEquals("OuterPacksDetail - container from export container", heading + expected, ShipmentWrapper.OuterPacksDetail);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			line2.SetContainer(eXPConsol, container2);
			line2.SetContainer(iMPConsol, container3);
			expected = "CRXU1234567/ 1500KG/ 14M3/ 20PLT" + System.Environment.NewLine + "CRXU1234568/ 2T/ 13470L/ 12PLT";
			AssertEquals("OuterPacksDetail", heading + expected, ShipmentWrapper.OuterPacksDetail);
		}

		public void TestFreightChargeType()
		{
			AssertEquals("No freight charge type", ZString.Empty, ShipmentWrapper.FreightChargeType);

			Shipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			AssertEquals("Freight charge type", "FREIGHT COLLECT", ShipmentWrapper.FreightChargeType);

			Shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			AssertEquals("Freight charge type", "FREIGHT PREPAID", ShipmentWrapper.FreightChargeType);
		}

		public void TestOuterPackLineTotalPacks()
		{
			AssertEquals("No outer pack lines", Shipment.TotalOuterPacks.ToString(), ShipmentWrapper.OuterPackLineTotalPacks);

			var packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 3;
			AssertEquals("Pack line pack count", "6", ShipmentWrapper.OuterPackLineTotalPacks);
		}

		public void TestOuterPackLineTotalWeight()
		{
			AssertEquals("No outer pack lines", Shipment.TotalOuterPacksWeight.ToString(), ShipmentWrapper.OuterPackLineTotalWeight);

			var packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 1;
			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 2;
			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualWeight = 3;
			AssertEquals("Pack line weight total", "6", ShipmentWrapper.OuterPackLineTotalWeight);
		}

		public void TestOuterPackLineTotalWeightInTonnes()
		{
			AssertEquals("No outer pack lines", "0", ShipmentWrapper.OuterPackLineTotalWeightInTonnes);

			var packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 1;
			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 2;
			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualWeight = 3;
			AssertEquals("Pack line weight total", "0.006", ShipmentWrapper.OuterPackLineTotalWeightInTonnes);
		}

		public void TestOuterPackLineTotalVolume()
		{
			AssertEquals("No outer pack lines", Shipment.TotalOuterPacksVolume.ToString(), ShipmentWrapper.OuterPackLineTotalVolume);

			var packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 1;
			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualVolume = 2;
			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualVolume = 3;
			AssertEquals("Pack line volume total", "6", ShipmentWrapper.OuterPackLineTotalVolume);
		}

		public void TestOuterPackLineComments()
		{
			AssertEquals("No pack line comments", ZString.Empty, ShipmentWrapper.OuterPackLineComments);

			var packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_Pillaged = 1;
			packLine1.JL_Damaged = 1;
			packLine1.JL_OutturnComment = "Test 1";
			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_Pillaged = 2;
			packLine2.JL_Damaged = 2;
			packLine2.JL_OutturnComment = "Test 2";
			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_Pillaged = 3;
			packLine3.JL_Damaged = 3;
			packLine3.JL_OutturnComment = "Test 3";

			AssertEquals("Pack line comments", "Total Pillaged: 6\r\nTotal Damaged: 6\r\nTest 1 Test 2 Test 3 ", ShipmentWrapper.OuterPackLineComments);
		}

		public void TestHBLShipmentConsolMode()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("HBL Mode is CY/CFS", "CY/CY", ShipmentWrapper.HBLShipmentConsolMode);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("HBL Mode is CFS/CY", "CFS/CFS", ShipmentWrapper.HBLShipmentConsolMode);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("HBL Mode is CY/CY", "CFS/CY", ShipmentWrapper.HBLShipmentConsolMode);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("No HBL Mode", ZString.Empty, ShipmentWrapper.HBLShipmentConsolMode);
		}

		public void TestSCACValueForManifest()
		{
			AssertEquals("SCAC value is empty", "", ShipmentWrapper.SCACValueForManifest);

			ShipmentWrapper.IsUSConsol = true;
			ShipmentWrapper.IsExportConsol = true;
			ShipmentWrapper.IsSeaConsol = true;
			Shipment.JS_JS_ColoadMasterShipment = Shipment.PK;

			var headerBisObject = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			Shipment.ConsignorPK = headerBisObject.PK;
			ZString expectedValue = headerBisObject.SCACCode != "" ? headerBisObject.SCACCode : new ZString("*MISSING*");
			AssertEquals("SCAC value is not empty", expectedValue, ShipmentWrapper.SCACValueForManifest);
		}

		public void TestShippedOnBoardDate()
		{
			Shipment.JS_ShippedOnBoard = ZString.Empty;
			AssertEquals("OnBoard Should return empty", "", ShipmentWrapper.OnBoard);

			Shipment.JS_ShippedOnBoard = new ZString(Shipment.Lookups.JS_ShippedOnBoard_List.GetCodeFromDescription("Shipped"));
			Shipment.JS_ShippedOnBoardDate = ZDateTime.Now.Date;

			ZString onBoardDesc = new ZString(Shipment.Lookups.JS_ShippedOnBoard_List.GetDescriptionFromCode(Shipment.JS_ShippedOnBoard));
			ZString expectedDesc = onBoardDesc.ToUpper() + " ON BOARD";

			Assert(expectedDesc == ShipmentWrapper.OnBoard);
			Assert(Shipment.JS_ShippedOnBoardDate == ShipmentWrapper.ShippedOnBoardDate);

			Shipment.JS_ShippedOnBoard = new ZString(Shipment.Lookups.JS_ShippedOnBoard_List.GetCodeFromDescription("Received For Shipment"));
			onBoardDesc = new ZString(Shipment.Lookups.JS_ShippedOnBoard_List.GetDescriptionFromCode(Shipment.JS_ShippedOnBoard));
			expectedDesc = onBoardDesc.ToUpper();
			Assert(expectedDesc == ShipmentWrapper.OnBoard);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestDocShipment()
		{
			CommonConsol consol = CreateExportConsol(Shipment);

			Shipment.JS_GoodsDescription = "Goodies";
			Shipment.JS_TransportMode = "SEA";
			Shipment.JS_PackingMode = "FCL";
			Shipment.ConsignorPK = ZArchitecture.Core.Utilities.GetGuidFromRandomRowInTable(OrgHeaderSchema.Constants.PK, OrgHeaderSchema.Constants.TableName);
			Shipment.ConsigneePK = ZArchitecture.Core.Utilities.GetGuidFromRandomRowInTable(OrgHeaderSchema.Constants.PK, OrgHeaderSchema.Constants.TableName);
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "USCHI";
			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.Indemnity;
			Shipment.JS_OuterPacks = 34;
			Shipment.JS_TotalPackageCount = 100;
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Carton;
			Shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Box;
			Shipment.JS_E_DEP = Env.Time.CurrentLocalDate;
			Shipment.JS_E_ARV = Env.Time.CurrentLocalDate.AddDays(2);
			Shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			Shipment.JS_GoodsValue = 1324.56m;
			Shipment.JS_InsuranceValue = 1457.32m;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Goods description", "Goodies", ShipmentWrapper.GoodsDescription);

			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, Shipment.JS_RL_NKOrigin));
			AssertEquals("Freight is payable at Origin", unloco.RL_PortName, ShipmentWrapper.FreightPayableAt);
			AssertEquals("Place of Receipt is Origin", unloco.RL_PortName, ShipmentWrapper.PlaceOfReceipt);

			unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, Shipment.JS_RL_NKDestination));
			AssertEquals("Place of Delivery is Destination", unloco.RL_PortName, ShipmentWrapper.PlaceOfDelivery);

			AssertEquals("Release type LOI", "Letter of Indemnity", ShipmentWrapper.ReleaseType);

			try
			{
				DocumentsDataRegistry.Instance.ShipmentDelayAlertAlertText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "A test message for alert");
				AssertEquals("Alert text", "A test message for alert", ShipmentWrapper.AlertText);
			}
			finally
			{
				((Integration.IRegistryItemInternals)DocumentsDataRegistry.Instance.OrderDelayAlertText).ClearCache();
			}

			AssertEquals("Container Mode", Constants.ContainerModes.FCL, ShipmentWrapper.ContainerMode);
			AssertEquals("Transport Mode is 'Sea FCL'", "FCL Sea", ShipmentWrapper.HeadingTransportMode);
			AssertEquals("Sea shipment, house bill heading is 'HOUSE BILL OF LADING'", "HOUSE BILL OF LADING", ShipmentWrapper.HouseBillHeading);
			AssertEquals("ETD", ZDateTime.Today, ShipmentWrapper.ETD);
			AssertEquals("ETD String", ZDateTime.Today.ToString("dd-MMM-yy"), ShipmentWrapper.ETDString);
			AssertEquals("ETA", ZDateTime.Today.AddDays(2), ShipmentWrapper.ETA);
			AssertEquals("ETA String", ZDateTime.Today.AddDays(2).ToString("dd-MMM-yy"), ShipmentWrapper.ETAString);
			AssertEquals("Seal number heading is 'Seal No.'", "SEAL NO.", ShipmentWrapper.SealNumberHeading);
			AssertEquals("Seal heading is 'Seal'", "SEAL", ShipmentWrapper.SealHeading);
			AssertEquals("Goods Value", 1324.56m, ShipmentWrapper.GoodsValue);
			AssertEquals("Insurance Value", 1457.32m, ShipmentWrapper.InsuranceValue);

			string packageDetails = "34 " + Shipment.JS_F3_NKPackType + " (OUTER)";
			packageDetails += ", " + "100 " + Shipment.JS_F3_NKTotalCountPackType + " (INNER)";
			AssertEquals("Package details should display outer and inner package", packageDetails, ShipmentWrapper.Packages);

			AssertNull("Notify Party null", ShipmentWrapper.NotifyParty);

			AssertEquals("Marks and numbers empty", "", ShipmentWrapper.MarksAndNumbers);
			AssertEquals("Detailed goods description empty default from short goods", "Goodies", ShipmentWrapper.DetailedDescriptionOfGoods);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestETDAndETAString()
		{
			AssertEquals("ETD is empty", ZString.Empty, ShipmentWrapper.ETDString);
			AssertEquals("ETA is empty", ZString.Empty, ShipmentWrapper.ETAString);

			Shipment.JS_TransportMode = "SEA";
			Shipment.JS_E_DEP = Env.Time.CurrentLocalDate;
			Shipment.JS_E_ARV = Env.Time.CurrentLocalDate.AddDays(2);

			AssertEquals("ETD only shows date", ZDateTime.Today.ToString("dd-MMM-yy"), ShipmentWrapper.ETDString);
			AssertEquals("ETA only shows date", ZDateTime.Today.AddDays(2).ToString("dd-MMM-yy"), ShipmentWrapper.ETAString);

			Shipment.JS_TransportMode = "AIR";
			AssertEquals("ETD shows date and time", ZDateTime.Today.ToLongTimeString(), ShipmentWrapper.ETDString);
			AssertEquals("ETA shows date and time", ZDateTime.Today.AddDays(2).ToLongTimeString(), ShipmentWrapper.ETAString);
		}

		public void TestNotifyPartyNoOverrideAddress()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "A Company";
			header.MainAddress.OA_Address1 = "1400 Notify Lane, Somewhere";
			Shipment.ConsigneePK = header.PK;

			var notifyParty = Factory.New<OrgContact>();
			notifyParty.OC_ContactName = "Andrew Smith";
			notifyParty.OC_OH = header.PK;

			Shipment.NotifyPartyDocumentaryAddress.ContactPK = notifyParty.PK;

			AssertEquals("Notify Party is Andrew Smith", "Andrew Smith".ToUpper(), ShipmentWrapper.NotifyParty.Name.Split('\n')[0].ToUpper());
			AssertEquals("Notify Party Company is the Consignee company", header.OH_FullName.ToUpper(), ShipmentWrapper.NotifyParty.Name.Split('\n')[1].ToUpper());
			AssertEquals("Notify Party address is Consignee address", header.MainAddress.OA_Address1.TrimEnd().ToUpper(), ShipmentWrapper.NotifyParty.PostalAddress.Split('\n')[2].ToUpper());
		}

		public void TestNotifyPartyWithOverrideAddress()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "A Company";
			header.MainAddress.OA_Address1 = "1400 Notify Lane, Somewhere";
			Shipment.ConsigneePK = header.PK;

			var notifyParty = Factory.New<OrgContact>();
			notifyParty.OC_ContactName = "Kelli Johnson";
			notifyParty.OC_OH = header.PK;

			Shipment.NotifyPartyDocumentaryAddress.ContactPK = notifyParty.PK;

			AssertEquals("Notify Party is Kelli Johnson", "Kelli Johnson".ToUpper(), ShipmentWrapper.NotifyParty.Name.Split('\n')[0].ToUpper());
			AssertEquals("Notify Party Company is the override company", header.OH_FullName.ToUpper(), ShipmentWrapper.NotifyParty.Name.Split('\n')[1].ToUpper());
			AssertEquals("Notify Party address is override company address", header.MainAddress.OA_Address1.ToUpper(), ShipmentWrapper.NotifyParty.PostalAddress.Split('\n')[2].ToUpper());

			Shipment.NotifyPartyDocumentaryAddress.E2_Contact = "Bob";
			AssertEquals("When contact name is only overriden - Notify Party is BOB", "BOB".ToUpper(), ShipmentWrapper.NotifyParty.Name.Split('\n')[0].ToUpper());
			AssertEquals("When contact name is only overriden - Notify Party Company is the override company", header.OH_FullName.ToUpper(), ShipmentWrapper.NotifyParty.Name.Split('\n')[1].ToUpper());
			AssertEquals("When contact name is only overriden - Notify Party address is override company address", header.MainAddress.OA_Address1.ToUpper(), ShipmentWrapper.NotifyParty.PostalAddress.Split('\n')[2].ToUpper());

			Shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			Shipment.NotifyPartyDocumentaryAddress.E2_Contact = "Other Name";
			Shipment.NotifyPartyDocumentaryAddress.E2_Phone = "1212";
			Shipment.NotifyPartyDocumentaryAddress.E2_Fax = "3434";
			Shipment.NotifyPartyDocumentaryAddress.E2_Email = "other@email.com";

			AssertEquals("When Address is overriden - Notify Party is Other Name", "Other Name".ToUpper(), ShipmentWrapper.NotifyParty.Name.Split('\n')[0].ToUpper());
			AssertEquals("When Address is overriden - Notify Party Company is the override company", header.OH_FullName.ToUpper(), ShipmentWrapper.NotifyParty.Name.Split('\n')[1].ToUpper());
			AssertEquals("When Address is overriden - Notify Party address is override company address", header.MainAddress.OA_Address1.ToUpper(), ShipmentWrapper.NotifyParty.PostalAddress.Split('\n')[2].ToUpper());
			AssertEquals("When Address is overriden - Notify Party is override phone", "1212", ShipmentWrapper.NotifyParty.Phone);
			AssertEquals("When Address is overriden - Notify Party is override fax", "3434", ShipmentWrapper.NotifyParty.Fax);
			AssertEquals("When Address is overriden - Notify Party is override email", "other@email.com".ToUpper(), ShipmentWrapper.NotifyParty.Email.ToUpper());
		}

		public void TestNotifyParty2()
		{
			OrgHeader notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.OH_FullName = "NOTIFY PARTY TWO";
			Shipment.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty2.PK;

			AssertEquals("Notify Party2", "NOTIFY PARTY TWO", ShipmentWrapper.NotifyParty2.Name);
		}

		public void TestNotifyParty3()
		{
			OrgHeader notifyParty3 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3.OH_FullName = "NOTIFY PARTY THREE";
			Shipment.NotifyParty3DocumentaryAddress.OrganisationPK = notifyParty3.PK;

			AssertEquals("Notify Party3", "NOTIFY PARTY THREE", ShipmentWrapper.NotifyParty3.Name);
		}

		#region Pick Up Address

		public void TestPickupAddress()
		{
			AssertNull("PickupAddress", ShipmentWrapper.PickupAddress);

			var originalAddress = Factory.New<OrgAddress>();
			originalAddress.OA_Address1 = "Original Street";
			Shipment.ConsignorPickupAddress.E2_OA_Address = originalAddress.PK;
			AssertNotNull("PickupAddress", ShipmentWrapper.PickupAddress);
			AssertEquals("PickupAddress is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.PickupAddress.GetType());

			Shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			Shipment.ConsignorPickupAddress.E2_CompanyName = "Zubin COmpany";
			Shipment.ConsignorPickupAddress.E2_Address1 = "Zubin Street";
			Shipment.ConsignorPickupAddress.E2_Address2 = "Some Other Street";
			Shipment.ConsignorPickupAddress.E2_City = "Some Suburb";
			Shipment.ConsignorPickupAddress.E2_Postcode = "1133";
			Shipment.ConsignorPickupAddress.E2_State = "QLD";

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertNotNull("PickupAddress", ShipmentWrapper.PickupAddress);
			AssertEquals("Override", "Zubin Street", ShipmentWrapper.PickupAddress.Address1);

			Shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			Shipment.ConsignorPickupAddress.E2_OA_Address = originalAddress.PK;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Don't override", "Original Street", ShipmentWrapper.PickupAddress.Address1);
		}

		public void TestPickupAddressFallback()
		{
			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			consignor1.MainAddress.OA_Address1 = "FIRST CONSIGNOR PICKUP ADDRESS";
			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			consignor2.MainAddress.OA_Address1 = "SECOND CONSIGNOR PICKUP ADDRESS";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = consignor1.PK;

			var shipmentWrapper = DocShipment.New(shipment, Factory);
			((IShipmentWithDocsAndCartage)shipment).CartageExporterDocAddress.E2_OA_Address = consignor1.MainAddress.PK;

			AssertNotNull(shipmentWrapper.Consignor);
			AssertNotNull(shipmentWrapper.DocsAndCartage.PickupAddress);
			AssertEquals("Should default to docs and cartage pick up address", consignor1.MainAddress.OA_Address1, shipmentWrapper.PickupAddress.ToString());

			shipment.ConsignorPK = ZGuid.Empty;
			shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull(shipmentWrapper.Consignor);
			AssertNull(shipmentWrapper.DocsAndCartage.PickupAddress);
			AssertNull("Should be null as it cannot fall back on either the consignor or the docs and cartage pick up address", shipmentWrapper.PickupAddress);

			shipment.ConsignorPK = consignor2.PK;
			((IShipmentWithDocsAndCartage)shipment).CartageExporterDocAddress.E2_OA_Address = ZGuid.Empty;      //this sets the docs and cartage pick up address to null

			AssertNotNull(shipmentWrapper.Consignor);
			AssertNull(shipmentWrapper.DocsAndCartage.PickupAddress);
			AssertNotNull("Should fall back to consignor pick up address when docs and cartage address is null", shipmentWrapper.PickupAddress);
			AssertEquals(consignor2.MainAddress.OA_Address1, shipmentWrapper.PickupAddress.ToString());
		}

		#endregion

		#region Delivery Address

		public void TestDeliveryAddress()
		{
			AssertNull("DeliveryAddress", ShipmentWrapper.DeliveryAddress);

			var originalAddress = Factory.New<OrgAddress>();
			originalAddress.OA_Address1 = "Original Street";
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = originalAddress.PK;
			AssertNotNull("DeliveryAddress", ShipmentWrapper.DeliveryAddress);
			AssertEquals("DeliveryAddress is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.DeliveryAddress.GetType());

			Shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			Shipment.ConsigneeDeliveryAddress.E2_CompanyName = "Zubin COmpany";
			Shipment.ConsigneeDeliveryAddress.E2_Address1 = "Zubin Street";
			Shipment.ConsigneeDeliveryAddress.E2_Address2 = "Some Other Street";
			Shipment.ConsigneeDeliveryAddress.E2_City = "Some Suburb";
			Shipment.ConsigneeDeliveryAddress.E2_Postcode = "1133";
			Shipment.ConsigneeDeliveryAddress.E2_State = "QLD";

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertNotNull("DeliveryAddress", ShipmentWrapper.DeliveryAddress);
			AssertEquals("Override", "Zubin Street", ShipmentWrapper.DeliveryAddress.Address1);

			Shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = originalAddress.PK;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Don't override", "Original Street", ShipmentWrapper.DeliveryAddress.Address1);
		}

		public void TestDeliveryAddressFallback()
		{
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.MainAddress.OA_Address1 = "FIRST CONSIGNEE DELIVERY ADDRESS";
			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.MainAddress.OA_Address1 = "SECOND CONSIGNEE DELIVERY ADDRESS";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = consignee1.PK;

			var shipmentWrapper = DocShipment.New(shipment, Factory);
			((IShipmentWithDocsAndCartage)shipment).CartageExporterDocAddress.E2_OA_Address = consignee1.MainAddress.PK;

			AssertNotNull(shipmentWrapper.Consignee);
			AssertNotNull(shipmentWrapper.DocsAndCartage.DeliveryAddress);
			AssertEquals("Should default to docs and cartage delivery address", consignee1.MainAddress.OA_Address1, shipmentWrapper.DeliveryAddress.ToString());

			shipment.ConsigneePK = ZGuid.Empty;
			shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull(shipmentWrapper.Consignee);
			AssertNull(shipmentWrapper.DocsAndCartage.DeliveryAddress);
			AssertNull("Should be null as it cannot fall back on either the consignee or the docs and cartage delivery address", shipmentWrapper.DeliveryAddress);

			shipment.ConsigneePK = consignee2.PK;
			((IShipmentWithDocsAndCartage)shipment).CartageImporterDocAddress.E2_OA_Address = ZGuid.Empty;      //this sets the docs and cartage delivery address to null

			AssertNotNull(shipmentWrapper.Consignee);
			AssertNull(shipmentWrapper.DocsAndCartage.DeliveryAddress);
			AssertNotNull("Should fall back to consignee delivery address when docs and cartage address is null", shipmentWrapper.DeliveryAddress);
			AssertEquals(consignee2.MainAddress.OA_Address1, shipmentWrapper.DeliveryAddress.ToString());
		}

		#endregion

		public void TestChangeShipmentSettings()
		{
			Shipment.JS_PackingMode = "LCL";
			Shipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;

			Shipment.JS_GoodsDescription = "Goodies";
			AssertEquals("Goods description", "Goodies", ShipmentWrapper.GoodsDescription);

			var origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "UAIEV");
			Shipment.JS_RL_NKOrigin = origin.RL_Code;
			AssertEquals("Place of Receipt is Origin", origin.RL_PortName, ShipmentWrapper.PlaceOfReceipt);

			var destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			Shipment.JS_RL_NKDestination = destination.RL_Code;
			AssertEquals("Freight is payable at Destination", destination.RL_PortName, ShipmentWrapper.FreightPayableAt);
			AssertEquals("Place of Delivery is Destination", destination.RL_PortName, ShipmentWrapper.PlaceOfDelivery);

			Shipment.JS_TransportMode = "AIR";

			AssertEquals("Transport mode 'Air'", "Air", ShipmentWrapper.HeadingTransportMode);
			AssertEquals("Air shipment, house bill heading is 'HAWB'", "HAWB", ShipmentWrapper.HouseBillHeading);

			AssertEquals("Seal number heading is 'Rate Class'", "RATE CLASS", ShipmentWrapper.SealNumberHeading);
			AssertEquals("Seal heading is 'Rate Class'", "RATE CLASS", ShipmentWrapper.SealHeading);
		}

		public void TestContainerCollection()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var destination = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.GB_RL_NKHomePort));
			Shipment.JS_RL_NKDestination = destination.RL_Code;

			var packLine2 = Shipment.OuterPackLines.AddNew();
			var packLine3 = Shipment.OuterPackLines.AddNew();

			CommonConsol consol = CreateExportConsol(Shipment);
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			container1.JC_SealNum = "SEAL1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";
			container2.JC_SealNum = "SEAL2";

			packLine2.SetContainer(consol, container1);
			packLine3.SetContainer(consol, container2);

			AssertNotNull("Container Collection not null", ShipmentWrapper.Containers);
			AssertEquals("Container collection has 2 container objects", 2, ShipmentWrapper.Containers.Count);
		}

		public void TestContainerOuterPackLineComments()
		{
			AssertEquals("", ShipmentWrapper.ContainerOuterPackLineComments);

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();

			AssertEquals("", ShipmentWrapper.ContainerOuterPackLineComments);

			line1.JL_OutturnComment = "Outturn comment\nLine Two";
			line2.JL_OutturnComment = "Second packline outturn comment";

			AssertEquals("Packline outturn comments", "Outturn comment\nLine Two\nSecond packline outturn comment", ShipmentWrapper.ContainerOuterPackLineComments);
		}

		public void TestOuterPackLineDimensions()
		{
			AssertEquals("", ShipmentWrapper.OuterPackLineDimensions);

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();

			AssertEquals("", ShipmentWrapper.OuterPackLineDimensions);

			line1.JL_Length = 0.99M;
			line1.JL_UnitOfDimension = "M";

			ZString expected = "(L): 0.99  (W): 0  (H): 0 M";
			AssertEquals("Packline dimension", expected, ShipmentWrapper.OuterPackLineDimensions);

			line2.JL_Height = 10.33M;
			line2.JL_UnitOfDimension = "M";
			expected = "(L): 0.99  (W): 0  (H): 0 M\n(L): 0  (W): 0  (H): 10.33 M";
			AssertEquals("Packline dimension", expected, ShipmentWrapper.OuterPackLineDimensions);
		}

		public void TestOuterPackLineCountAndDimensions()
		{
			AssertEquals("", ShipmentWrapper.OuterPackLineCountAndDimensions);

			var line1 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 7;
			var line2 = Shipment.OuterPackLines.AddNew();

			AssertEquals("7 PLT", ShipmentWrapper.OuterPackLineCountAndDimensions);

			line1.JL_F3_NKPackType = "PKG";
			AssertEquals("7 PKG", ShipmentWrapper.OuterPackLineCountAndDimensions);

			line1.JL_Length = 0.99M;
			line1.JL_UnitOfDimension = "M";

			ZString expected = "(L): 0.99  (W): 0  (H): 0 M X 7 PKG";
			AssertEquals("Packline dimension", expected, ShipmentWrapper.OuterPackLineCountAndDimensions);

			line2.JL_Height = 10.33M;
			line2.JL_UnitOfDimension = "M";
			expected = "(L): 0.99  (W): 0  (H): 0 M X 7 PKG\n(L): 0  (W): 0  (H): 10.33 M X 0 PLT";
			AssertEquals("Packline dimension", expected, ShipmentWrapper.OuterPackLineCountAndDimensions);

			line2.JL_PackageCount = 3;
			expected = "(L): 0.99  (W): 0  (H): 0 M X 7 PKG\n(L): 0  (W): 0  (H): 10.33 M X 3 PLT";
			AssertEquals("Packline dimension", expected, ShipmentWrapper.OuterPackLineCountAndDimensions);

			line2.JL_F3_NKPackType = "PKG";
			expected = "(L): 0.99  (W): 0  (H): 0 M X 7 PKG\n(L): 0  (W): 0  (H): 10.33 M X 3 PKG";
			AssertEquals("Packline dimension", expected, ShipmentWrapper.OuterPackLineCountAndDimensions);
		}

		public void TestContainerOuterPackLineDimensionAndComments()
		{
			AssertEquals("", ShipmentWrapper.ContainerOuterPackLineDimensionAndComments);

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();

			AssertEquals("", ShipmentWrapper.ContainerOuterPackLineDimensionAndComments);

			line1.JL_Length = 0.99M;
			line1.JL_UnitOfDimension = "M";
			line1.JL_OutturnComment = "Outturn comment\nLine Two";
			line2.JL_OutturnComment = "Second packline outturn comment";

			ZString expected = "(L): 0.99  (W): 0  (H): 0 M\n\nOutturn comment\nLine Two\nSecond packline outturn comment";
			AssertEquals("Packline dimension and outturn comments", expected, ShipmentWrapper.ContainerOuterPackLineDimensionAndComments);

			line2.JL_Height = 10.33M;
			line2.JL_UnitOfDimension = "M";
			expected = "(L): 0.99  (W): 0  (H): 0 M\n(L): 0  (W): 0  (H): 10.33 M\n\nOutturn comment\nLine Two\nSecond packline outturn comment";
			AssertEquals("Packline dimension and outturn comments", expected, ShipmentWrapper.ContainerOuterPackLineDimensionAndComments);

			line2.JL_OutturnComment = "";
			expected = "(L): 0.99  (W): 0  (H): 0 M\n(L): 0  (W): 0  (H): 10.33 M\n\nOutturn comment\nLine Two";
			AssertEquals("Packline dimension and outturn comments", expected, ShipmentWrapper.ContainerOuterPackLineDimensionAndComments);

			line1.JL_OutturnComment = "";
			expected = "(L): 0.99  (W): 0  (H): 0 M\n(L): 0  (W): 0  (H): 10.33 M";
			AssertEquals("Packline dimension and outturn comments", expected, ShipmentWrapper.ContainerOuterPackLineDimensionAndComments);

			line1.JL_OutturnComment = "Outturn comment\nLine Two";
			line2.JL_OutturnComment = "Second packline outturn comment";
			line1.JL_Length = 0M;
			line2.JL_Height = 0M;

			expected = "Outturn comment\nLine Two\nSecond packline outturn comment";
			AssertEquals("Packline dimension and outturn comments", expected, ShipmentWrapper.ContainerOuterPackLineDimensionAndComments);
		}

		public void TestCurrentCompany()
		{
			AssertNotNull(ShipmentWrapper.CurrentCompany);
		}

		public void TestReleaseType()
		{
			ZString releaseType = ZString.Empty;
			ZString expectedString = ZString.Empty;

			Shipment.JS_ReleaseType = releaseType;
			AssertEquals("Release type", expectedString, ShipmentWrapper.ReleaseType);

			releaseType = Core.Constants.ShipmentReleaseTypes.OriginalReqSurrender;
			expectedString = Shipment.Lookups.JS_ReleaseType_List.GetDescriptionFromCode(releaseType);
			Shipment.JS_ReleaseType = releaseType;
			AssertEquals("Release type", expectedString, ShipmentWrapper.ReleaseType);

			releaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			expectedString = Shipment.Lookups.JS_ReleaseType_List.GetDescriptionFromCode(releaseType);
			Shipment.JS_ReleaseType = releaseType;
			AssertEquals("Release type", expectedString, ShipmentWrapper.ReleaseType);
		}

		public void TestReleaseTypeWithHeading()
		{
			AssertEquals("", ShipmentWrapper.ReleaseTypeWithHeading);

			Shipment.JS_ReleaseType = "LOI";
			AssertEquals("Release: Letter of Indemnity", ShipmentWrapper.ReleaseTypeWithHeading);
		}

		public void TestDisbursmentJobCharge()
		{
			Shipment.JS_UniqueConsignRef = "666";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "AVTEST";
			orgHeaderBisObj.OH_FullName = "Angie's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;
			AccChargeCode dSBChargeCode = CreateChargeCode("DSBChg");
			dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeCode mRGChargeCode = CreateChargeCode("MRGChg");
			mRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AccChargeCode rEVChargeCode = CreateChargeCode("REVChg");
			rEVChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, dSBChargeCode.PK);
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, mRGChargeCode.PK);
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName), 20.000M, rEVChargeCode.PK);
			JobCharge lineCharge4 = CreateLineCharge(jobHeaderBisObj, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName), 20.000M, dSBChargeCode.PK);
			JobCharge lineCharge5 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20.000M, dSBChargeCode.PK);

			Factory.Save();
			AssertEquals(2, ShipmentWrapper.JobHeader.JobChargesForLocalClient.DisbursementTypeJobCharges.Count);
			AssertEquals(10.000M, ShipmentWrapper.JobHeader.JobChargesForLocalClient.DisbursementTypeJobCharges[0].LocalSellAmount);
			AssertEquals(20.000M, ShipmentWrapper.JobHeader.JobChargesForLocalClient.DisbursementTypeJobCharges[1].LocalSellAmount);
		}

		public void TestJobChargesWithNonZeroAmount()
		{
			Shipment.JS_UniqueConsignRef = "666";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "AVTEST";
			orgHeaderBisObj.OH_FullName = "Angie's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;
			AccChargeCode dSBChargeCode = CreateChargeCode("DSBChg");
			dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeCode mRGChargeCode = CreateChargeCode("MRGChg");
			mRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AccChargeCode rEVChargeCode = CreateChargeCode("REVChg");
			rEVChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, dSBChargeCode.PK);
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 0.000M, mRGChargeCode.PK);
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName), 20.000M, rEVChargeCode.PK);
			JobCharge lineCharge4 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20.000M, dSBChargeCode.PK);
			JobCharge lineCharge5 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 50.000M, dSBChargeCode.PK);

			Factory.Save();
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals(3, ShipmentWrapper.JobHeader.JobChargesForLocalClient.NonZeroJobCharges.Count);
			AssertEquals(10.000M, ShipmentWrapper.JobHeader.JobChargesForLocalClient.NonZeroJobCharges[0].LocalSellAmount);
			AssertEquals(20.000M, ShipmentWrapper.JobHeader.JobChargesForLocalClient.NonZeroJobCharges[1].LocalSellAmount);
			AssertEquals(50.000M, ShipmentWrapper.JobHeader.JobChargesForLocalClient.NonZeroJobCharges[2].LocalSellAmount);
		}

		public void TestNonZeroDSBTypeCharges()
		{
			Shipment.JS_UniqueConsignRef = "666";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "AVTEST";
			orgHeaderBisObj.OH_FullName = "Angie's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;
			AccChargeCode dSBChargeCode = CreateChargeCode("DSBChg");
			dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeCode mRGChargeCode = CreateChargeCode("MRGChg");
			mRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AccChargeCode rEVChargeCode = CreateChargeCode("REVChg");
			rEVChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, dSBChargeCode.PK);
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20.000M, mRGChargeCode.PK);
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, ZArchitecture.Core.Utilities.GetGuidFromTopRowInTable(OrgHeader.Schema.PK, OrgHeader.Schema.TableName), 20.000M, dSBChargeCode.PK);
			JobCharge lineCharge4 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20.000M, dSBChargeCode.PK);
			JobCharge lineCharge5 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 0.000M, dSBChargeCode.PK);

			Factory.Save();
			AssertEquals(2, ShipmentWrapper.JobHeader.JobChargesForLocalClient.NonZeroDisbursementJobCharges.Count);
			AssertEquals(10.000M, ShipmentWrapper.JobHeader.JobChargesForLocalClient.NonZeroDisbursementJobCharges[0].LocalSellAmount);
			AssertEquals(20.000M, ShipmentWrapper.JobHeader.JobChargesForLocalClient.NonZeroDisbursementJobCharges[1].LocalSellAmount);
		}

		public void TestFreightCollectAmount()
		{
			Shipment.JS_UniqueConsignRef = "666";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "VGTEST";
			orgHeaderBisObj.OH_FullName = "Vahid's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			AssertEquals("JobChargesForAgentCollect.Count", 2, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("FreightCollectAmount", 0.000M, ShipmentWrapper.FreightCollectAmount);
		}
		public void TestFreightCollectAmount2()
		{
			Shipment.JS_UniqueConsignRef = "666";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "VGTEST";
			orgHeaderBisObj.OH_FullName = "Vahid's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "BRK";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			Shipment.JS_INCO = Constants.IncoTerms.ExWorks;
			AssertEquals("JobChargesForAgentCollect.Count", 2, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("FreightCollectAmount", 10.000M, ShipmentWrapper.FreightCollectAmount);
		}
		public void TestFreightCollectAmount3()
		{
			Shipment.JS_UniqueConsignRef = "666";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "VGTEST";
			orgHeaderBisObj.OH_FullName = "Vahid's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			Shipment.JS_INCO = Constants.IncoTerms.ExWorks;

			chargeCode2.AC_ChargeGroup = "FRT";
			AssertEquals("JobChargesForAgentCollect.Count", 2, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("FreightCollectAmount", 40.000M, ShipmentWrapper.FreightCollectAmount);
		}
		public void TestFreightCollectAmount4()
		{
			Shipment.JS_UniqueConsignRef = "666";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "VGTEST";
			orgHeaderBisObj.OH_FullName = "Vahid's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			Shipment.JS_INCO = Constants.IncoTerms.ExWorks;
			chargeCode2.AC_ChargeGroup = "FRT";

			lineCharge3.JR_OH_SellAccount = orgHeaderBisObj.PK;
			AssertEquals("JobChargesForAgentCollect.Count", 3, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("FreightCollectAmount", 110.000M, ShipmentWrapper.FreightCollectAmount);
		}
		public void TestFreightCollectAmount5()
		{
			Shipment.JS_UniqueConsignRef = "666";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "VGTEST";
			orgHeaderBisObj.OH_FullName = "Vahid's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			Shipment.JS_INCO = Constants.IncoTerms.ExWorks;
			chargeCode2.AC_ChargeGroup = "FRT";
			lineCharge3.JR_OH_SellAccount = orgHeaderBisObj.PK;

			jobHeaderBisObj.AgentCollectPK = ZGuid.Empty;
			AssertEquals("JobChargesForAgentCollect.Count", 0, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("FreightCollectAmount", 0.000M, ShipmentWrapper.FreightCollectAmount);
		}

		public void TestFreightCODAmount()
		{
			Shipment.JS_UniqueConsignRef = "121";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "DATEST";
			orgHeaderBisObj.OH_FullName = "Da's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			AssertEquals("JobChargesForAgentCollect.Count", 2, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("None posted: FreightCODAmount", 0.000M, ShipmentWrapper.FreightCODAmount);
		}

		public void TestFreightCODAmount2()
		{
			Shipment.JS_UniqueConsignRef = "121";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "DATEST";
			orgHeaderBisObj.OH_FullName = "Da's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line1 = (AccTransactionLines)invoice.Lines.AddNew();
			lineCharge1.JR_AL_ARLine = line1.PK;
			var line2 = (AccTransactionLines)invoice.Lines.AddNew();
			lineCharge2.JR_AL_ARLine = line2.PK;

			AssertEquals("JobChargesForAgentCollect.Count", 2, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("1&2 posted: FreightCODAmount", 40.000M, ShipmentWrapper.FreightCODAmount);
		}

		public void TestFreightCODAmount3()
		{
			Shipment.JS_UniqueConsignRef = "121";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "DATEST";
			orgHeaderBisObj.OH_FullName = "Da's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line1 = (AccTransactionLines)invoice.Lines.AddNew();
			lineCharge1.JR_AL_ARLine = line1.PK;
			var line2 = (AccTransactionLines)invoice.Lines.AddNew();
			lineCharge2.JR_AL_ARLine = line2.PK;

			lineCharge3.JR_OH_SellAccount = orgHeaderBisObj.PK;
			var line3 = (AccTransactionLines)invoice.Lines.AddNew();
			lineCharge3.JR_AL_ARLine = line3.PK;
			AssertEquals("JobChargesForAgentCollect.Count", 3, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("1&2&3 posted: FreightCODAmount", 110.000M, ShipmentWrapper.FreightCODAmount);
		}

		public void TestFreightCODAmount4()
		{
			Shipment.JS_UniqueConsignRef = "121";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "DATEST";
			orgHeaderBisObj.OH_FullName = "Da's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.AgentCollectPK = orgHeaderBisObj.PK;
			AccChargeCode chargeCode1 = CreateChargeCode("DSBChg");
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode1.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode2 = CreateChargeCode("MRGChg");
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeGroup = "FRT";
			AccChargeCode chargeCode3 = CreateChargeCode("REVChg");
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode3.AC_ChargeGroup = "FRT";

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode1.PK);
			lineCharge1.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, chargeCode2.PK);
			lineCharge2.JR_OH_SellAccount = orgHeaderBisObj.PK;
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, chargeCode3.PK);

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line1 = (AccTransactionLines)invoice.Lines.AddNew();
			lineCharge1.JR_AL_ARLine = line1.PK;
			var line2 = (AccTransactionLines)invoice.Lines.AddNew();
			lineCharge2.JR_AL_ARLine = line2.PK;

			lineCharge3.JR_OH_SellAccount = orgHeaderBisObj.PK;
			var line3 = (AccTransactionLines)invoice.Lines.AddNew();
			lineCharge3.JR_AL_ARLine = line3.PK;

			jobHeaderBisObj.AgentCollectPK = ZGuid.Empty;
			AssertEquals("JobChargesForAgentCollect.Count", 0, ShipmentWrapper.JobHeader.JobChargesForAgentCollect.Count);
			AssertEquals("No agent: FreightCODAmount", 0.000M, ShipmentWrapper.FreightCODAmount);
		}

		public void TestFRTandORGTotalsAndChargesWithoutFRTandORG()
		{
			Shipment.JS_UniqueConsignRef = "19851014";
			JobHeader jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "SGTEST";
			orgHeaderBisObj.OH_FullName = "Sergey's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;

			AccChargeCode fRTChargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));

			AccChargeCode fRTChargeCode2 = CreateChargeCode("FRTmy");
			fRTChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			fRTChargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			AccChargeCode fRTChargeCode3 = CreateChargeCode("BAFmy");
			fRTChargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			fRTChargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			AccChargeCode oRGChargeCode = CreateChargeCode("ORGmy");
			oRGChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			oRGChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			AccChargeCode destChargeCode = CreateChargeCode("DESmy");
			destChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			destChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			AccChargeCode bRKChargeCode = CreateChargeCode("BRKmy");
			bRKChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			bRKChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.BrokerageOnly;

			AccChargeCode iNSChargeCode = CreateChargeCode("INSmy");
			iNSChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			iNSChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Insurance;

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, fRTChargeCode1.PK, 10);
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20.000M, fRTChargeCode2.PK);
			JobCharge lineCharge3 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, fRTChargeCode3.PK);
			JobCharge lineCharge4 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 40.000M, oRGChargeCode.PK, 10);
			JobCharge lineCharge5 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 50.000M, destChargeCode.PK);
			JobCharge lineCharge6 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 60.000M, bRKChargeCode.PK);
			JobCharge lineCharge7 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 70.000M, iNSChargeCode.PK);

			Factory.Save();

			AssertEquals(7, ShipmentWrapper.JobHeader.JobChargesForDebtorOrLocalClient.NonZeroJobCharges.Count);
			AssertEquals(10.000M, ShipmentWrapper.TotalSellAmountOfFreightCharges);
			AssertEquals(40.000M, ShipmentWrapper.TotalSellAmountOfOriginChargeGroupCharges);
			AssertEquals(5, ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges.Count);
			AssertEquals("INSmy", ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges[0].ChargeCode.Code);
			AssertEquals("FRTmy", ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges[1].ChargeCode.Code);
			AssertEquals("BAFmy", ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges[2].ChargeCode.Code);
			AssertEquals("DESmy", ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges[3].ChargeCode.Code);
			AssertEquals("BRKmy", ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges[4].ChargeCode.Code);
			Factory.Save();

			AssertEquals(ShipmentWrapper.TotalSellAmountOfFreightCharges, 10.000M);
			AssertEquals(ShipmentWrapper.TotalTaxAmountOfFreightCharges, 1.000M);
			AssertEquals(ShipmentWrapper.SumTotalSellAndTaxAmountOfFreightCharges, ShipmentWrapper.TotalTaxAmountOfFreightCharges + ShipmentWrapper.TotalSellAmountOfFreightCharges);

			Factory.Save();

			AssertEquals(ShipmentWrapper.TotalSellAmountOfOriginChargeGroupCharges, 40.000M);
			AssertEquals(ShipmentWrapper.TotalTaxAmountOfOriginChargeGroupCharges, 4.000M);
			AssertEquals(ShipmentWrapper.SumTotalSellAndTaxAmountOfOriginChargeGroupCharges, ShipmentWrapper.TotalSellAmountOfOriginChargeGroupCharges + ShipmentWrapper.TotalTaxAmountOfOriginChargeGroupCharges);
		}

		public void TestNonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupChargesIsCachedOnFactoryLevel()
		{
			Shipment.JS_UniqueConsignRef = "19851014";
			var jobHeaderBisObj = CreateJobHeader(Shipment);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "SGTEST";
			orgHeaderBisObj.OH_FullName = "Sergey's OrgHeader Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Test Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Test Address 2";

			jobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;

			var chargeCode = CreateChargeCode("NONFRT");
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			var lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10.000M, chargeCode.PK, 10);

			Factory.Save();

			var collection = ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges;
			AssertEquals("One element in the collection", 1, collection.Count);

			var newWrapper = GetNewShipmentWrapper();
			AssertNotSame("Different wrappers", newWrapper, ShipmentWrapper);
			AssertNotSame("Different collection as they are cached on Factory level by DocWrapper PK", newWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges, collection);

			lineCharge1.JR_Desc = "Something to have change";
			Factory.Save();

			AssertNotSame("Cached value was reset", collection, ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges);
			AssertEquals("Count", collection.Count, ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges.Count);
			AssertNotSame("Element", collection[0], ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges[0]);
			AssertSame("Wrapped Job Charge", collection[0].WrappedObject, ShipmentWrapper.NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges[0].WrappedObject);
		}

		public void TestBankAccount()
		{
			AccBankAccount[] defaultBankAccounts = (AccBankAccount[])Factory.Load(typeof(AccBankAccount), new ZQuery(AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, ZBool.True));
			foreach (AccBankAccount account in defaultBankAccounts)
			{
				account.AB_IsDefaultReceiptBankAccount = ZBool.False;
			}

			var headerBisObj = Factory.NewWithValidTestData<AccGLHeader>();

			var accountBisObj = Factory.New<AccBankAccount>();
			accountBisObj.AB_BankName = "Test Bank Account";
			accountBisObj.AB_BankAddress = "123 Address Test";
			accountBisObj.AB_GC = GlbCompany.CurrentCompany.PK;
			accountBisObj.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			accountBisObj.AB_GB = GlbBranch.CurrentBranch.PK;
			accountBisObj.AB_IsDefaultReceiptBankAccount = ZBool.True;
			accountBisObj.AB_AG = headerBisObj.PK;
			accountBisObj.AB_Code = "ABCBANK";
			Factory.Save();

			AssertEquals("Test Bank Account", ShipmentWrapper.BankAccount.BankName);
			AssertEquals("123 Address Test", ShipmentWrapper.BankAccount.BankAddress);
		}

		public void TestDisbursementNoteTitle()
		{
			AssertEquals("Disbursement Note Title", Env.Registry.DisbursementNoteTitle.Trim().ToUpper(), ShipmentWrapper.DisbursementNoteTitle.ToString());
		}

		public void TestWeight()
		{
			Shipment.JS_DocumentedWeight = 10.5M;
			Shipment.JS_ActualWeight = 8.00M;
			Shipment.JS_ManifestedWeight = 5.050M;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			ShipmentWrapper.SetReportNameForTesting("Pre-Alert");
			if (Env.Registry.PreAlertWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Actual)
			{
				AssertEquals("Should be the actual weight", "8", ShipmentWrapper.Weight);
			}
			else if (Env.Registry.PreAlertWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Client)
			{
				AssertEquals("Should be the Documented weight", "10.5", ShipmentWrapper.Weight);
			}
			else if (Env.Registry.PreAlertWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Carrier)
			{
				AssertEquals("Should be the Carrier weight", "5.05", ShipmentWrapper.Weight);
			}
			else
			{
				throw new Exception("The Weight Display option is returning invalid value");
			}
		}

		public void TestVolume()
		{
			Shipment.JS_DocumentedVolume = 10.5M;
			Shipment.JS_ActualVolume = 8.00M;
			Shipment.JS_ManifestedVolume = 5.050M;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			ShipmentWrapper.SetReportNameForTesting("Outturn Report");
			if (Env.Registry.OutturnReportWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Actual)
			{
				AssertEquals("Should be the actual volume", "8", ShipmentWrapper.Volume);
			}
			else if (Env.Registry.OutturnReportWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Client)
			{
				AssertEquals("Should be the Documented volume", "10.5", ShipmentWrapper.Volume);
			}
			else if (Env.Registry.OutturnReportWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Carrier)
			{
				AssertEquals("Should be the Carrier volume", "5.05", ShipmentWrapper.Volume);
			}
			else
			{
				throw new Exception("The Weight Display option is returning invalid value");
			}
		}

		public void TestChargeable()
		{
			Shipment.JS_DocumentedChargeable = 10.5M;
			Shipment.JS_ActualChargeable = 8.00M;
			Shipment.JS_ManifestedChargeable = 5.050M;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			ShipmentWrapper.SetReportNameForTesting("Booking Confirmation");
			if (Env.Registry.BookingConfirmationWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Actual)
			{
				AssertEquals("Should be the actual chargeable", "8", ShipmentWrapper.Chargeable);
			}
			else if (Env.Registry.BookingConfirmationWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Client)
			{
				AssertEquals("Should be the Documented chargeable", "10.5", ShipmentWrapper.Chargeable);
			}
			else if (Env.Registry.BookingConfirmationWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Carrier)
			{
				AssertEquals("Should be the Carrier chargeable", "5.05", ShipmentWrapper.Chargeable);
			}
			else
			{
				throw new Exception("The Weight Display option is returning invalid value");
			}
		}

		public void TestColoadShipments()
		{
			ShipmentWrapper.SetReportNameForTesting("Agents Instructions");
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));

			var coloadShipment1 = Shipment.CoLoadShipments.AddNew();
			var coloadShipment2 = Shipment.CoLoadShipments.AddNew();

			AssertEquals(2, ShipmentWrapper.ColoadShipments.Count);
			AssertEquals("Agents Instructions", ShipmentWrapper.ColoadShipments[0].ReportName);
			AssertEquals("DEP", ShipmentWrapper.ColoadShipments[0].DocumentDirection);
		}

		public void TestIsForwardRegistered()
		{
			Assert(ShipmentWrapper.IsForwardRegistered);
			Shipment.JS_IsForwardRegistered = ZBool.False;
			AssertEquals(ZBool.False, ShipmentWrapper.IsForwardRegistered);
		}

		public void TestColoadHouseBills()
		{
			AssertEquals("", ShipmentWrapper.ColoadHouseBills);

			var coloadShipment1 = Shipment.CoLoadShipments.AddNew();
			var coloadShipment2 = Shipment.CoLoadShipments.AddNew();
			coloadShipment1.JS_HouseBill = "COLOAD HOUSE 123";
			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals("COLOAD HOUSE 123", ShipmentWrapper.ColoadHouseBills);

			coloadShipment2.JS_HouseBill = "COLOAD HOUSE 90210";
			Assert(ShipmentWrapper.ColoadHouseBills.Contains("COLOAD HOUSE 123"));
			Assert(ShipmentWrapper.ColoadHouseBills.Contains("COLOAD HOUSE 90210"));
		}

		public void TestMissingRequiredDocumentsInstruction()
		{
			AssertEquals("RequestForMissingDocumentsInstruction", Env.Registry.ShipmentRequestForMissingDocumentsClause, ShipmentWrapper.RequestForMissingDocumentsInstruction);
		}

		public void TestShipmentOrBrokerageNumber()
		{
			Shipment.JS_UniqueConsignRef = "TREE";
			AssertEquals(ShipmentWrapper.ShipmentNumber, ShipmentWrapper.ShipmentOrBrokerageNumber);
		}

		public void TestDeclarationOrConsolNumber()
		{
			AssertEquals("Precondition: shipment has no consols", 0, Shipment.Consols.Count);
			AssertEquals(ZString.Empty, ShipmentWrapper.DeclarationOrConsolNumber);

			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "LAKE";
			AssertEquals(ShipmentWrapper.Consol.ConsolNumber, ShipmentWrapper.DeclarationOrConsolNumber);
		}

		public void TestContext()
		{
			AssertEquals("SHIPMENT", ShipmentWrapper.Context);
		}

		public void TestAUDeclarationForCurrentBranch()
		{
			ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			AssertNull("DeclarationForCurrentBranch", ShipmentWrapper.DeclarationForCurrentBranch);

			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec1 = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec1.JE_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.GB_GC)).PK;
			dec1.JE_JS = Shipment.PK;
			AssertNull("DeclarationForCurrentBranch", ShipmentWrapper.DeclarationForCurrentBranch);

			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec2 = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec2.JE_GB = GlbBranch.CurrentBranch.PK;
			dec2.JE_JS = Shipment.PK;
			Factory.Save();
			AssertNotNull("DeclarationForCurrentBranch", ShipmentWrapper.DeclarationForCurrentBranch);
			AssertEquals("DeclarationForCurrentBranch is of type Enterprise.DocumentWrappers.Customs.AU.DocDeclaration", typeof(Customs.AU.DocDeclaration), ShipmentWrapper.DeclarationForCurrentBranch.GetType());

			GlbCompany.CurrentCompany.SetCountry(storedCompany);
		}

		public void TestDeclarationForShipmentBranch()
		{
			ZString storedCompany = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var jobHeaderBisObj = Factory.NewJobForTesting<JobHeader>();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)).PK;

			var departmentBisObj = Factory.LoadTop1<GlbDepartment>(new ZQuery());
			jobHeaderBisObj.JH_GE = departmentBisObj.PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec1 = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();

			dec1.JE_JS = Shipment.PK;
			dec1.JE_GB = jobHeaderBisObj.JH_GB;

			var jobDecBranch = Factory.Load<GlbBranch>(dec1.JE_GB);

			AssertEquals("CurrentBranch is different from Declaration Branch", true, GlbBranch.CurrentBranch.GB_Code != jobDecBranch.GB_Code);
			AssertNotNull("DeclarationWithoutBranchCheck", ShipmentWrapper.DeclarationForShipmentBranch);

			GlbBranch shipmentBranch = jobHeaderBisObj.Branch;

			AssertEquals("Declaration Branch should be same as the Shipment Branch", jobDecBranch.GB_Code, shipmentBranch.GB_Code);

			GlbCompany.CurrentCompany.SetCountry(storedCompany);
		}

		public void TestPlaceOfReceipt()
		{
			Shipment.JS_RL_NKOrigin = "";
			AssertEquals("PlaceOfReceipt", "", ShipmentWrapper.PlaceOfReceipt);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Shipment.JS_RL_NKOrigin = uNLOCO.RL_Code;

			AssertEquals("PlaceOfReceipt", uNLOCO.RL_PortName, ShipmentWrapper.PlaceOfReceipt);
		}

		public void TestPlaceOfDelivery()
		{
			Shipment.JS_RL_NKDestination = "";
			AssertEquals("PlaceOfDelivery", "", ShipmentWrapper.PlaceOfDelivery);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Shipment.JS_RL_NKDestination = uNLOCO.RL_Code;

			AssertEquals("PlaceOfDelivery", uNLOCO.RL_PortName, ShipmentWrapper.PlaceOfDelivery);
		}

		public void TestWeightInKG()
		{
			AssertEquals("0", ShipmentWrapper.WeightInKG);

			Shipment.JS_ActualWeight = 1000.55M;
			Shipment.JS_UnitOfWeight = "KG";
			AssertEquals("1000.55", ShipmentWrapper.WeightInKG);

			Shipment.JS_UnitOfWeight = "G";
			AssertEquals("1.001", ShipmentWrapper.WeightInKG);
		}

		public void TestVolumeInM3()
		{
			AssertEquals("0", ShipmentWrapper.VolumeInM3);

			Shipment.JS_ActualVolume = 500.12M;
			Shipment.JS_UnitOfVolume = "M3";
			AssertEquals("500.12", ShipmentWrapper.VolumeInM3);

			Shipment.JS_UnitOfVolume = "L";
			AssertEquals("0.5", ShipmentWrapper.VolumeInM3);
		}

		public void TestShipmentConsolsTransportPlannings()
		{
			Shipment.Consols.RemoveAll();
			AssertEquals("ShipmentConsolsTransportPlannings.Count", 0, ShipmentWrapper.ShipmentConsolsTransportPlannings.Count);

			CommonConsol consol1 = Shipment.Consols.AddNew();
			AssertEquals("ShipmentConsolsTransportPlannings.Count", 1, ShipmentWrapper.ShipmentConsolsTransportPlannings.Count);

			CommonConsol consol2 = Shipment.Consols.AddNew();
			AssertEquals("ShipmentConsolsTransportPlannings.Count", 2, ShipmentWrapper.ShipmentConsolsTransportPlannings.Count);

			Transport transport = consol2.Transports.AddNew();
			AssertEquals("ShipmentConsolsTransportPlannings.Count", 3, ShipmentWrapper.ShipmentConsolsTransportPlannings.Count);
		}

		public void TestHasTransportPlanningDetails()
		{
			Shipment.Consols.RemoveAll();

			Shipment.Transports.RemoveAll();
			AssertEquals("HasTransportPlanningDetails", false, ShipmentWrapper.HasTransportPlanningDetails);

			Shipment.Transports.AddNew();
			AssertEquals("HasTransportPlanningDetails", true, ShipmentWrapper.HasTransportPlanningDetails);
		}

		public void PackLines_HasCurrentConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.InnerPackLines.AddNew();
			shipment.InnerPackLines.AddNew();

			DocShipment docShipment = DocShipment.New(shipment, Factory);
			AssertNull("InnerPackline 1 has null Current consol", docShipment.PackLines[0].CurrentConsol);
			AssertNull("InnerPackline 2 has null Current consol", docShipment.PackLines[1].CurrentConsol);

			CommonConsol consol = GetNewConsol();
			DocBaseConsol docBaseConsol = DocBaseConsol.New(consol, Factory);
			docShipment.CurrentConsol = docBaseConsol;
			AssertEquals("InnerPackline 1 has Current consol", docBaseConsol, docShipment.PackLines[0].CurrentConsol);
			AssertEquals("InnerPackline 2 has Current consol", docBaseConsol, docShipment.PackLines[1].CurrentConsol);
		}

		public void OuterPackLineCollection_HasCurrentConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();

			DocShipment docShipment = DocShipment.New(shipment, Factory);
			AssertNull("OuterPackLine 1 has null Current consol", docShipment.OuterPackLineCollection[0].CurrentConsol);
			AssertNull("OuterPackLine 2 has null Current consol", docShipment.OuterPackLineCollection[1].CurrentConsol);

			CommonConsol consol = GetNewConsol();
			DocBaseConsol docBaseConsol = DocBaseConsol.New(consol, Factory);
			docShipment.CurrentConsol = docBaseConsol;
			AssertEquals("OuterPackLine 1 has Current consol", docBaseConsol, docShipment.OuterPackLineCollection[0].CurrentConsol);
			AssertEquals("OuterPackLine 2 has Current consol", docBaseConsol, docShipment.OuterPackLineCollection[1].CurrentConsol);
		}

		public void TestOuterPackLineCollection_ForMasterShipment()
		{
			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_UniqueConsignRef = "MASTER";
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			CommonShipment worker1 = masterShipment.CoLoadShipments.AddNew();
			worker1.JS_UniqueConsignRef = "WORKER1";
			PackLine worker1PackLine = worker1.OuterPackLines.AddNew();
			CommonShipment worker2 = masterShipment.CoLoadShipments.AddNew();
			worker1.JS_UniqueConsignRef = "WORKER2";
			PackLine worker2PackLine = worker2.OuterPackLines.AddNew();

			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { worker1PackLine, worker2PackLine }, masterShipment.OuterPackLines);

			DocShipment docShipment = DocShipment.New(masterShipment, Factory);

			AssertEquals(2, docShipment.OuterPackLineCollection.Count);
			AssertEquals(docShipment.OuterPackLineCollection[0].Shipment.CommonShipment, masterShipment);
			AssertEquals(docShipment.OuterPackLineCollection[1].Shipment.CommonShipment, masterShipment);
		}

		public void TestIsPrepaid()
		{
			Shipment.JS_INCO = "";
			AssertEquals("IsPrepaid", false, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.CarriagePaidTo;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.CarriageAndInsurancePaidTo;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.DeliveredAtFrontier;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.DeliveredExShip;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.DeliveredExQuay;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.DeliveredDutyUnpaid;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);

			Shipment.JS_INCO = Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);
		}

		public void TestIsCollect()
		{
			Shipment.JS_INCO = "";
			AssertEquals("IsCollect", false, ShipmentWrapper.IsCollect);

			Shipment.JS_INCO = Constants.IncoTerms.ExWorks;
			AssertEquals("IsCollect", true, ShipmentWrapper.IsCollect);

			Shipment.JS_INCO = Constants.IncoTerms.FreeCarrier;
			AssertEquals("IsCollect", true, ShipmentWrapper.IsCollect);

			Shipment.JS_INCO = Constants.IncoTerms.FreeAlongsideShip;
			AssertEquals("IsCollect", true, ShipmentWrapper.IsCollect);

			Shipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			AssertEquals("IsCollect", true, ShipmentWrapper.IsCollect);
		}

		public void TestIsMasterCoLoadShipment()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("IsMasterColoadShipment", true, ShipmentWrapper.IsMasterCoLoadShipment);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("IsMasterColoadShipment", false, ShipmentWrapper.IsMasterCoLoadShipment);

			var coloadShipment = Factory.New<ForwardingShipment>();
			coloadShipment.JS_JS_ColoadMasterShipment = Shipment.PK;
			Shipment.CoLoadShipments.Load();
			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals("IsMasterColoadShipment", true, ShipmentWrapper.IsMasterCoLoadShipment);
		}

		public void TestIsMasterCoLoadShipment_BlindCoLoadMaster()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals("Expected to be coload shipment", true, ShipmentWrapper.IsCoload);
			AssertEquals("Expected to be master coload shipment", true, ShipmentWrapper.IsMasterCoLoadShipment);
			AssertEquals("Expected to be false as there are no subshipments attached", false, ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments);

			var coloadShipment = Factory.New<ForwardingShipment>();
			coloadShipment.JS_JS_ColoadMasterShipment = Shipment.PK;
			Shipment.CoLoadShipments.Load();

			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals("Expected to now be true as coload shipment has a sub shipment", true, ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals("Expected to no longer be a coload shipment as the shipment type has changed", false, ShipmentWrapper.IsCoload);
		}

		public void TestIsMasterCoLoadShipmentWithSubShipments()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("IsMasterColoadShipmentWithSubShipments", false, ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var coloadShipment = Factory.New<ForwardingShipment>();
			coloadShipment.JS_JS_ColoadMasterShipment = Shipment.PK;
			Shipment.CoLoadShipments.Load();
			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals("IsMasterColoadShipmentWithSubShipments", false, ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("IsMasterColoadShipmentWithSubShipments", true, ShipmentWrapper.IsMasterCoLoadShipmentWithSubShipments);
		}

		public void TestIsExport()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.Code;
			AssertEquals("IsExport", true, ShipmentWrapper.IsExport);

			Shipment.JS_RL_NKOrigin = (GlbBranch.CurrentBranch.Country.Code == "AU") ? "SGSIN" : "AUSYD";
			AssertEquals("IsExport", false, ShipmentWrapper.IsExport);
		}

		public void TestIsImport()
		{
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.Code;
			AssertEquals("IsImport", true, ShipmentWrapper.IsImport);

			Shipment.JS_RL_NKDestination = (GlbBranch.CurrentBranch.Country.Code == "AU") ? "SGSIN" : "AUSYD";
			AssertEquals("IsImport", false, ShipmentWrapper.IsImport);
		}

		public void TestIsOffshore()
		{
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.Code;
			Shipment.JS_RL_NKOrigin = "HKHKG";
			AssertEquals("IsOffshore", false, ShipmentWrapper.IsOffshore);

			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.Code;
			Shipment.JS_RL_NKDestination = "SGSIN";
			AssertEquals("IsOffshore", false, ShipmentWrapper.IsOffshore);

			Shipment.JS_RL_NKOrigin = "HKHKG";
			Shipment.JS_RL_NKDestination = "SGSIN";
			AssertEquals("IsOffshore", true, ShipmentWrapper.IsOffshore);
		}

		public void TestIsDomestic()
		{
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.Code;
			Shipment.JS_RL_NKOrigin = "HKHKG";
			AssertEquals("IsDomestic", false, ShipmentWrapper.IsDomestic);

			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.Code;
			Shipment.JS_RL_NKDestination = "SGSIN";
			AssertEquals("IsDomestic", false, ShipmentWrapper.IsDomestic);

			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "USMEM";
			AssertEquals("IsDomestic", true, ShipmentWrapper.IsDomestic);
		}

		public void TestIsDomesticAndFreightCOD()
		{
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "USMEM";
			AssertEquals("IsDomestic", true, ShipmentWrapper.IsDomestic);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			AssertEquals(false, ShipmentWrapper.IsDomesticAndFreightCOD);

			Shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectCOD;
			AssertEquals(true, ShipmentWrapper.IsDomesticAndFreightCOD);

			Shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals(false, ShipmentWrapper.IsDomesticAndFreightCOD);
		}

		public void TestPrintAsContainers()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Print as Containers", ZBool.False, ShipmentWrapper.PrintAsContainers);
			var consol = Shipment.Consols.AddNew();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("Print as Containers", ZBool.False, ShipmentWrapper.PrintAsContainers);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Print as Containers", ZBool.True, ShipmentWrapper.PrintAsContainers);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Print as Containers", ZBool.False, ShipmentWrapper.PrintAsContainers);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Print as Containers", ZBool.True, ShipmentWrapper.PrintAsContainers);
		}

		public void TestPickupDate()
		{
			AssertEquals(ZDateTime.Empty, ShipmentWrapper.PickupDate);
			Shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, ShipmentWrapper.PickupDate);
		}

		public void TestAvailableDateForFCL()
		{
			TestAvailableDateForFCL(Core.Constants.ContainerModes.FCL);
		}

		public void TestAvailableDateForULD()
		{
			TestAvailableDateForFCL(Core.Constants.ContainerModes.ULD);
		}

		public void TestAvailableDateForBulk()
		{
			TestAvailableDateForFCL(Core.Constants.ContainerModes.Bulk);
		}

		public void TestAvailableDateForLiquid()
		{
			TestAvailableDateForFCL(Core.Constants.ContainerModes.Liquid);
		}

		public void TestAvailableDateForBuyersConsol()
		{
			TestAvailableDateForFCL(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModes.BuyersConsol);
		}

		void TestAvailableDateForFCL(ZString fCLContainerMode)
		{
			TestAvailableDateForFCL(fCLContainerMode, fCLContainerMode);
		}

		void TestAvailableDateForFCL(ZString fCLShipmentContainerMode, ZString fCLConsolContainerMode)
		{
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKOrigin = "USCHI";
			Shipment.JS_PackingMode = fCLShipmentContainerMode;

			AssertEquals("No Consol", ZDateTime.Empty, ShipmentWrapper.AvailableDate);

			CommonConsol consol = CreateImportConsol(Shipment);
			consol.JK_ConsolMode = fCLConsolContainerMode;
			CreateASailing();

			Transport transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;

			transport.JW_TerminalAvailabilityDate = ZDateTime.Today.AddDays(1);
			AssertEquals("w/ sailing date", ZDateTime.Today.AddDays(1), ShipmentWrapper.AvailableDate);

			transport.JW_TerminalAvailabilityDate = ZDateTime.Today.AddDays(11);
			AssertEquals("w/ consol and sailing date", ZDateTime.Today.AddDays(11), ShipmentWrapper.AvailableDate);

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			container1.JC_FCLAvailable = ZDateTime.Today.AddDays(21);
			container2.JC_FCLAvailable = ZDateTime.Today.AddDays(6);
			container3.JC_FCLAvailable = ZDateTime.Today.AddDays(31);
			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			PackLine packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = container3.PK;
			AssertEquals("w/ container, consol & sailing date", ZDateTime.Today.AddDays(6), ShipmentWrapper.AvailableDate);

			Shipment.DocsAndCartage.JP_FCLAvailable = ZDateTime.Today.AddDays(4);
			AssertEquals("w/ shipment, container, consol & sailing date", ZDateTime.Today.AddDays(4), ShipmentWrapper.AvailableDate);
		}

		public void TestAvailableDateForLCL()
		{
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKOrigin = "USCHI";
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			AssertEquals("No Consol", ZDateTime.Empty, ShipmentWrapper.AvailableDate);

			CommonConsol consol = CreateImportConsol(Shipment);
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			CreateASailing();

			Transport transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;

			transport.JW_DepotAvailabilityDate = ZDateTime.Today.AddDays(3);
			AssertEquals("w/ sailing date", ZDateTime.Today.AddDays(3), ShipmentWrapper.AvailableDate);

			transport.JW_DepotAvailabilityDate = ZDateTime.Today.AddDays(13);
			AssertEquals("w/ consol and sailing date", ZDateTime.Today.AddDays(13), ShipmentWrapper.AvailableDate);

			Sailing.JX_DepotAvailabilityDate = ZDateTime.Empty;
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container2);
			PackLine packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.SetContainer(consol, container3);

			container1.JC_LCLAvailable = ZDateTime.Today.AddDays(23);
			container2.JC_LCLAvailable = ZDateTime.Today.AddDays(8);
			container3.JC_LCLAvailable = ZDateTime.Today.AddDays(33);
			AssertEquals("w/ container, consol & sailing date", ZDateTime.Today.AddDays(8), ShipmentWrapper.AvailableDate);

			Shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Today.AddDays(5);
			AssertEquals("w/ shipment, container, consol & sailing date", ZDateTime.Today.AddDays(5), ShipmentWrapper.AvailableDate);
		}

		public void TestStorageDateForFCL()
		{
			TestStorageDateForFCL(Core.Constants.ContainerModes.FCL);
		}

		public void TestStorageDateForULD()
		{
			TestStorageDateForFCL(Core.Constants.ContainerModes.ULD);
		}

		public void TestStorageDateForBulk()
		{
			TestStorageDateForFCL(Core.Constants.ContainerModes.Bulk);
		}

		public void TestStorageDateForLiquid()
		{
			TestStorageDateForFCL(Core.Constants.ContainerModes.Liquid);
		}

		public void TestStorageDateForBuyersConsol()
		{
			TestStorageDateForFCL(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModes.BuyersConsol);
		}

		void TestStorageDateForFCL(ZString fCLContainerMode)
		{
			TestStorageDateForFCL(fCLContainerMode, fCLContainerMode);
		}

		void TestStorageDateForFCL(ZString fCLShipmentContainerMode, ZString fCLConsolContainerMode)
		{
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKOrigin = "USCHI";
			Shipment.JS_PackingMode = fCLShipmentContainerMode;

			AssertEquals("No Consol", ZDateTime.Empty, ShipmentWrapper.StorageCommenceDate);

			CommonConsol consol = CreateImportConsol(Shipment);
			consol.JK_ConsolMode = fCLConsolContainerMode;
			CreateASailing();

			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;

			Sailing.Destination.JB_StorageDate = ZDateTime.Today.AddDays(50);

			Factory.Save();

			AssertEquals("w/ sailing date", ZDateTime.Today.AddDays(50), ShipmentWrapper.StorageCommenceDate);

			transport.JW_TerminalStorageDate = ZDateTime.Today.AddDays(40);
			AssertEquals("w/ consol and sailing date", ZDateTime.Today.AddDays(40), ShipmentWrapper.StorageCommenceDate);

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container2);
			PackLine packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.SetContainer(consol, container3);

			container1.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(30);
			container2.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(10);
			container3.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(20);
			AssertEquals("w/ container, consol & sailing date", ZDateTime.Today.AddDays(10), ShipmentWrapper.StorageCommenceDate);

			Shipment.DocsAndCartage.JP_FCLStorageCommences = ZDateTime.Today.AddDays(5);
			AssertEquals("w/ shipment, container, consol & sailing date", ZDateTime.Today.AddDays(5), ShipmentWrapper.StorageCommenceDate);
		}

		public void TestStorageDateForLCL()
		{
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKOrigin = "USCHI";
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			AssertEquals("No Consol", ZDateTime.Empty, ShipmentWrapper.StorageCommenceDate);

			CommonConsol consol = CreateImportConsol(Shipment);
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			CreateASailing();

			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = Sailing.PK;

			Sailing.JX_DepotStorageDate = ZDateTime.Today.AddDays(50);

			Factory.Save();

			AssertEquals("w/ sailing date", ZDateTime.Today.AddDays(50), ShipmentWrapper.StorageCommenceDate);

			transport.JW_DepotStorageDate = ZDateTime.Today.AddDays(40);
			AssertEquals("w/ consol and sailing date", ZDateTime.Today.AddDays(40), ShipmentWrapper.StorageCommenceDate);

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container2);
			PackLine packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.SetContainer(consol, container3);

			transport.JW_DepotStorageDate = ZDateTime.Empty;
			container1.JC_LCLStorageCommences = ZDateTime.Today.AddDays(30);
			container2.JC_LCLStorageCommences = ZDateTime.Today.AddDays(10);
			container3.JC_LCLStorageCommences = ZDateTime.Today.AddDays(20);

			AssertEquals("w/ container, consol & sailing date", ZDateTime.Today.AddDays(10), ShipmentWrapper.StorageCommenceDate);

			Shipment.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Today.AddDays(5);

			AssertEquals("w/ shipment, container, consol & sailing date", ZDateTime.Today.AddDays(5), ShipmentWrapper.StorageCommenceDate);
		}

		public void TestCutOffOrAvailableDate()
		{
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("CutOffOrAvailableDate", ShipmentWrapper.BookingCutOffDate, ShipmentWrapper.CutOffOrAvailableDate);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("CutOffOrAvailableDate", ShipmentWrapper.AvailableDate, ShipmentWrapper.CutOffOrAvailableDate);
		}

		public void TestPickupOrStorageCommenceDate()
		{
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("PickupOrStorageCommenceDate", ShipmentWrapper.PickupDate, ShipmentWrapper.PickupOrStorageCommenceDate);
			AssertEquals("DEP Heading", "PICKUP DATE", ShipmentWrapper.PickupOrStorageCommenceDateHeading);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("PickupOrStorageCommenceDate", ShipmentWrapper.StorageCommenceDate, ShipmentWrapper.PickupOrStorageCommenceDate);
			AssertEquals("ARV Heading", "STORAGE STARTS", ShipmentWrapper.PickupOrStorageCommenceDateHeading);
		}

		public void TestIDocCartageAdviceDates()
		{
			Shipment.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2011, 4, 18);
			Shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2011, 4, 20);
			Shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2011, 4, 22);
			Shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2011, 4, 25);

			Transport transport1 = Shipment.Transports.AddNew();
			transport1.JW_ETD = new ZDateTime(2011, 2, 1);
			transport1.JW_ETA = new ZDateTime(2011, 2, 15);

			Transport transport2 = Shipment.Transports.AddNew();
			transport2.JW_ETD = new ZDateTime(2011, 1, 1);
			transport2.JW_ETA = new ZDateTime(2011, 1, 15);
			transport2.JW_TerminalCutOff = new ZDateTime(2011, 1, 18);
			transport2.JW_DepotCutOff = new ZDateTime(2011, 1, 20);
			transport2.JW_TerminalReceivalCommences = new ZDateTime(2011, 1, 22);
			transport2.JW_DepotReceivalCommences = new ZDateTime(2011, 1, 25);

			Transport transport3 = Shipment.Transports.AddNew();
			transport3.JW_ETD = new ZDateTime(2011, 4, 1);
			transport3.JW_ETA = new ZDateTime(2011, 4, 15);

			Transport transport4 = Shipment.Transports.AddNew();
			transport4.JW_ETD = new ZDateTime(2011, 3, 1);
			transport4.JW_ETA = new ZDateTime(2011, 3, 15);

			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), ShipmentWrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 18), ShipmentWrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), ShipmentWrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 22), ShipmentWrapper.CartageStorageCommenceDate);

			Shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 20), ShipmentWrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 20), ShipmentWrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 25), ShipmentWrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 25), ShipmentWrapper.CartageStorageCommenceDate);
		}

		public void TestFCLReceivalsAndCutOffDates()
		{
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Precondition FCLReceivalsDate", ZDateTime.Empty, ShipmentWrapper.FCLReceivalsDate);
			AssertEquals("Precondition FCLCutOffDate", ZDateTime.Empty, ShipmentWrapper.FCLCutOffDate);

			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_TerminalReceivalCommences = ZDateTime.Today.AddDays(1);
			transport.JW_TerminalCutOff = ZDateTime.Today.AddDays(2);

			AssertEquals("FCLReceival from Consol", ZDateTime.Today.AddDays(1), ShipmentWrapper.FCLReceivalsDate);
			AssertEquals("FCLCutOff from Consol", ZDateTime.Today.AddDays(2), ShipmentWrapper.FCLCutOffDate);
		}

		public void TestLCLReceivalsAndCutOffDates()
		{
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Precondition LCLReceivalsDate", ZDateTime.Empty, ShipmentWrapper.LCLReceivalsDate);
			AssertEquals("Precondition LCLCutOffDate", ZDateTime.Empty, ShipmentWrapper.LCLCutOffDate);

			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_DepotReceivalCommences = ZDateTime.Today.AddDays(3);
			transport.JW_DepotCutOff = ZDateTime.Today.AddDays(4);

			AssertEquals("LCLReceival from Consol", ZDateTime.Today.AddDays(3), ShipmentWrapper.LCLReceivalsDate);
			AssertEquals("LCLCutOff from Consol", ZDateTime.Today.AddDays(4), ShipmentWrapper.LCLCutOffDate);
		}

		public void TestBondDate()
		{
			ZDateTime eTADate;
			ZDateTime expectedDate;
			ZDateTime.TryParseExact("17/04/1982", out eTADate, "dd/MM/yyyy");
			ZDateTime.TryParseExact("31/05/1982", out expectedDate, "dd/MM/yyyy");
			Shipment.JS_E_ARV = eTADate;
			AssertEquals("Bond Date", expectedDate, ShipmentWrapper.BondDate);
		}

		public void TestGoodsDescriptionForManifest()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CountryExportStatementSettingCollection countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			CountryExportStatementSetting countrySetting = countrySettingCollection.AddNew();
			countrySetting.CountryCode = GlbBranch.CurrentBranch.Country.Code;
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "AES";
			statementSetting.Statement = "STATEMENT FOR TESTING";
			statementSetting.Visibility = "UDF";
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "NZAKL";
			Shipment.JS_GoodsDescription = "Short Goods Description";
			Shipment.DetailedGoodsDescriptionNoteText = "Detailed Goods Description";
			ShipmentWrapper.SetReportNameForTesting("Manifest");
			AssertEquals("Should return short description", "Short Goods Description", ShipmentWrapper.GoodsDescriptionForManifest);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.GoodsDescriptionForManifest);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			ShipmentWrapper.SetReportNameForTesting("Manifest Detailed (Landscape)");
			AssertEquals("Should return long description", "Detailed Goods Description", ShipmentWrapper.GoodsDescriptionForManifest);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.GoodsDescriptionForManifest);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			ShipmentWrapper.SetReportNameForTesting("Manifest Freighted (Landscape)");
			AssertEquals("Should return long description", "Detailed Goods Description", ShipmentWrapper.GoodsDescriptionForManifest);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.GoodsDescriptionForManifest);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			Shipment.Notes.AddNew(ZBool.False, PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description, "Manifest Goods Description.");
			ShipmentWrapper.SetReportNameForTesting("Manifest");
			AssertEquals("Should return manifest description", "Manifest Goods Description.", ShipmentWrapper.GoodsDescriptionForManifest);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.GoodsDescriptionForManifest);

			Shipment.DocsAndCartage.JP_ExportStatement = "";
			ShipmentWrapper.SetReportNameForTesting("Manifest Landscape Detailed");
			AssertEquals("Should return manifest description", "Manifest Goods Description.", ShipmentWrapper.GoodsDescriptionForManifest);
			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			AssertContains("Should contain statement", statementSetting.Statement, ShipmentWrapper.GoodsDescriptionForManifest);
		}

		public void TestActualRCVString()
		{
			AssertEquals("ActualRCVString", "", ShipmentWrapper.ActualRCVString);

			Shipment.JS_A_RCV = DateTime.Today;
			Assert("ActualRCVString", !ShipmentWrapper.ActualRCVString.IsEmpty);
		}

		public void TestInterimAndDateReceived()
		{
			Shipment.JS_InterimReceipt = "Interim123456";
			Shipment.JS_A_RCV = DateTime.Today;
			Assert("InterimAndDateReceived", !ShipmentWrapper.InterimAndDateReceived.IsEmpty);

			Shipment.JS_A_RCV = ZDateTime.Empty;
			AssertEquals("InterimAndDateReceived", "Interim: Interim123456", ShipmentWrapper.InterimAndDateReceived);

			Shipment.JS_InterimReceipt = "";
			AssertEquals("InterimAndDateReceived", "", ShipmentWrapper.InterimAndDateReceived);

			Shipment.JS_A_RCV = ZDateTime.Today.AddDays(1);
			Assert("InterimAndDateReceived", !ShipmentWrapper.InterimAndDateReceived.IsEmpty);
		}

		public void TestPackLineCustomAttributes()
		{
			PackLine line1 = Shipment.OuterPackLines.AddNew();
			PackLine line2 = Shipment.OuterPackLines.AddNew();
			PackLine line3 = Shipment.OuterPackLines.AddNew();

			line2.JL_CustomAttrib1 = "ATTRB1";
			line2.JL_CustomAttrib3 = "ATTRB3";
			line2.JL_CustomAttrib4 = "ATTRB4";

			AssertEquals("Consignee Reference", "ATTRB1\nATTRB3\nATTRB4", ShipmentWrapper.PackLineCustomAttributes);
		}

		public void TestExportDate()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_ATD = ZDateTime.Today;
			AssertEquals("ExportDate should be today", ZDateTime.Today, ShipmentWrapper.ExportDate);
		}

		public void TestShipmentCreateDate()
		{
			Shipment.JS_SystemCreateTimeUtc = ZDateTime.Now;
			AssertNotEquals(ZDateTime.Empty, ShipmentWrapper.ShipmentCreateDate);
			AssertEquals(Shipment.JS_SystemCreateTimeUtc, ShipmentWrapper.ShipmentCreateDate);
		}

		public void TestDateOfArrival()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_ATA = ZDateTime.Today;
			AssertEquals("DateOfArrival should be today", ZDateTime.Today, ShipmentWrapper.DateOfArrival);
		}

		public void TestContainerDeliveryMode()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var destination = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.GB_RL_NKHomePort));
			Shipment.JS_RL_NKDestination = destination.RL_Code;

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("DeliveryMode is blank as there are no containers", "", ShipmentWrapper.ContainerDeliveryMode);

			var packLine = Shipment.OuterPackLines.AddNew();

			CommonConsol consol = CreateExportConsol(Shipment);
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("DeliveryMode is blank as the container doesn't have a delivery mode set", "", ShipmentWrapper.ContainerDeliveryMode);

			container.JC_DeliveryMode = "CY/CY";

			packLine.SetContainer(consol, container);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertNotNull("Container Collection not null", ShipmentWrapper.Containers);
			AssertEquals("Container collection has 1 container objects", 1, ShipmentWrapper.Containers.Count);
			AssertEquals("DeliveryMode is CY/CY", "CY/CY", ShipmentWrapper.ContainerDeliveryMode);
		}

		public void TestAgentNotes()
		{
			StmNote agentNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.AgentNotes.Description, "Agent Notes Stuff\nLine Two");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should not be included.");

			AssertEquals("Detailed goods description", "Agent Notes Stuff\nLine Two", ShipmentWrapper.AgentNotes);
		}

		public void TestMasterBillHeading()
		{
			AssertNull("Consol", ShipmentWrapper.Consol);

			Shipment.JS_IsDirectBooking = ZBool.False;
			AssertEquals("MasterBillHeading empty if not direct booking", "", ShipmentWrapper.MasterBillHeading);

			Shipment.JS_IsDirectBooking = ZBool.True;
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("MasterBillHeading empty if transportmode not Air", "", ShipmentWrapper.MasterBillHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("MasterBillHeading return MAWB", "MAWB", ShipmentWrapper.MasterBillHeading);

			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("MasterBillHeading from consol", "MASTER", ShipmentWrapper.MasterBillHeading);
		}

		public void TestMasterBillNum()
		{
			AssertNull("Consol", ShipmentWrapper.Consol);

			Shipment.JS_IsDirectBooking = ZBool.False;
			AssertEquals("MasterBillNum empty if not direct booking", "", ShipmentWrapper.MasterBillNum);

			Shipment.JS_IsDirectBooking = ZBool.True;
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("MasterBillNum empty if transportmode not Air", "", ShipmentWrapper.MasterBillNum);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.JS_HouseBill = "03200001234";
			AssertEquals("MasterBillNum return formated MAWB", "032-0000 1234", ShipmentWrapper.MasterBillNum);

			Shipment.JS_HouseBill = "032000";
			AssertEquals("MasterBillNum return formated MAWB, if JS_Housebill shorter then 11 symbols", "032-000", ShipmentWrapper.MasterBillNum);

			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "03200001122";
			AssertEquals("MasterBillNum return consol masterbill, Air transport mode", "032-0000 1122", ShipmentWrapper.MasterBillNum);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "03200001122";
			AssertEquals("MasterBillNum return consol masterbill, not Air transport mode", "03200001122", ShipmentWrapper.MasterBillNum);
		}

		#endregion

		#region Other Tests (these should be sorted)

		public void TestAddingADocFCLContainerToTheContainerCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			DocShipment shimentWrapper = DocShipment.New(shipment.Factory, shipment.PK);

			AgencyShipmentContainer container = Factory.New<AgencyShipmentContainer>();
			ShipmentWrapper.Containers.Add(DocAgencyContainer.New(container, Factory));

			AssertNotNull(ShipmentWrapper.ContainerVolumeHeading);
			AssertNotNull(ShipmentWrapper.ContainerWeightHeading);
			AssertNotNull(ShipmentWrapper.ContainerPackageHeading);
		}

		public void TestPackCargoLocation()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			CommonConsol consol = shipment.Consols.AddNew();
			CreateASailing();

			Transport transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;

			PackLine packs = shipment.OuterPackLines.AddNew();
			packs.JL_PackageCount = 10;
			packs.JL_Outturn = 10;
			PackLocation location = packs.PackLocations.AddNew();
			PackLocation location2 = packs.PackLocations.AddNew();
			location.JQ_NoPackages = 4;
			location.JQ_WarehouseLocation = "HERE";
			location2.JQ_NoPackages = 6;
			location2.JQ_WarehouseLocation = "THERE";
			CommonPickupDeliveryConfirm delivery = shipment.DestinationCFSDepartures.AddNew(); //PackType auto set
			delivery.GetDivot(packs).J8_PackagesDelivered = 10;
			DocPickupDeliveryConfirm containerLegWrapper = DocPickupDeliveryConfirm.New(delivery, Factory);
			Assert("Pack location should not be blank", !containerLegWrapper.CargoLocationAndPacks.IsEmpty);
			Factory.Save();
			Assert("Pack location should not be blank", !containerLegWrapper.CargoLocationAndPacks.IsEmpty);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			GatePassShipment shipment2 = factory2.Load<GatePassShipment>(shipment.PK);
			DocPickupDeliveryConfirm containerLegWrapper2 = DocPickupDeliveryConfirm.New(shipment2.DestinationCFSDepartures[0], Factory);
			Assert("Pack location should not be blank", !containerLegWrapper2.CargoLocationAndPacks.IsEmpty);
			factory2.Save();
			Assert("Pack location should not be blank", !containerLegWrapper2.CargoLocationAndPacks.IsEmpty);
		}

		public void TestActualWeightRounded()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_ActualWeight = 12.3M;
			AssertEquals("Rounded Weight", 12.5M, shipmentWrapper.ActualWeightRounded);

			shipment.JS_ActualWeight = 12.6M;
			AssertEquals("Rounded Weight", 12.5M, shipmentWrapper.ActualWeightRounded);

			shipment.JS_ActualWeight = 12.2M;
			AssertEquals("Rounded Weight", 12M, shipmentWrapper.ActualWeightRounded);

			shipment.JS_ActualWeight = 12.8M;
			AssertEquals("Rounded Weight", 13M, shipmentWrapper.ActualWeightRounded);
		}

		public void TestPackageSizesAndCounts()
		{
			const string ExpectedResult = "30X50X60/3\r\n30X50X55/6";
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].JL_Length = 0.3m;
			shipment.OuterPackLines[0].JL_Width = 0.5m;
			shipment.OuterPackLines[0].JL_Height = 0.6m;
			shipment.OuterPackLines[0].JL_PackageCount = 3;
			shipment.OuterPackLines[1].JL_Length = 0.3m;
			shipment.OuterPackLines[1].JL_Width = 0.5m;
			shipment.OuterPackLines[1].JL_Height = 0.55m;
			shipment.OuterPackLines[1].JL_PackageCount = 6;
			AssertEquals("Package Sizes and Counts 2 packlines", ExpectedResult, shipmentWrapper.PackageSizesAndCounts);
		}

		public void TestInsuranceCovered()
		{
			DocumentsDataRegistry.Instance.FOBAndienungInsuranceCoveredText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "covered by");
			DocumentsDataRegistry.Instance.FOBAndienungInsuranceNotCoveredText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "not covered by");

			Shipment.JS_InsuranceValue = 100;
			AssertEquals("covered by text", "covered by", ShipmentWrapper.InsuranceCovered);

			Shipment.JS_InsuranceValue = 0;
			AssertEquals("not covered by text", "not covered by", ShipmentWrapper.InsuranceCovered);
		}

		public void TestJob()
		{
			AssertNull(ShipmentWrapper.Job);

			var job = Factory.NewJobForTesting<Job>();
			AssertNull(ShipmentWrapper.Job);

			job.JH_ParentID = Shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			AssertNotNull(ShipmentWrapper.Job);
			AssertEquals(typeof(DocJobInvoicingJob), ShipmentWrapper.Job.GetType());
		}

		public void TestJobHeader()
		{
			AssertNull("JobHeader", ShipmentWrapper.JobHeader);

			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = Shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			AssertNotNull("JobHeader", ShipmentWrapper.JobHeader);
			AssertEquals("JobHeader is of type DocJobHeader", typeof(DocJobHeader), ShipmentWrapper.JobHeader.GetType());
		}

		public void TestJobHeaderWithDebtor()
		{
			DocumentShipment documentShipment = new DocumentShipment(Shipment, Constants.DataContext.ChargeSheet);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DebtorToSelectFromForPrinting debtor = new DebtorToSelectFromForPrinting(org);
			documentShipment.Debtor = debtor;
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);

			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = Shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			AssertNotNull("JobHeader", ShipmentWrapper.JobHeader);
			AssertEquals("JobHeader is of type DocJobHeader", typeof(DocJobHeader), ShipmentWrapper.JobHeader.GetType());
			AssertEquals("JobHeader Debtor", org.PK, ShipmentWrapper.JobHeader.Debtor.PK);
		}

		public void TestTotalOuterPackLinePackages()
		{
			AssertEquals("No Outer PackLine", 0, ShipmentWrapper.TotalOuterPackLinePackages);

			var packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 3;
			AssertEquals("Pack Line Pack Count", 6, ShipmentWrapper.TotalOuterPackLinePackages);
		}

		public void TestJobHeaderOnMultiCompanies()
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = companyA.PK;

			var companyB = Factory.NewWithValidTestData<GlbCompany>();
			var branchB = Factory.NewWithValidTestData<GlbBranch>();
			branchB.GB_GC = companyB.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchA.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var aShipment = Factory.New<ForwardingShipment>();
				ShipmentWrapper = DocShipment.New(aShipment, Factory);

				var header = Factory.NewJobForTesting<JobHeader>();
				header.JH_ParentID = aShipment.PK;
				header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				header.JH_GC = companyA.PK;
				header.JH_GB = branchA.PK;

				AssertNotNull("Shipment JobHeader on CompanyA not null", ShipmentWrapper.JobHeader);
			}
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchB.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertNull("Shipment JobHeader on CompanyB null", ShipmentWrapper.JobHeader);
			}
		}

		public void TestShipmentTransportPlannings()
		{
			AssertEquals("Shipment Transport Planning count", 0, ShipmentWrapper.ShipmentTransportPlanning.Count);

			Shipment.Transports.AddNew();
			Shipment.Transports.AddNew();
			Shipment.Transports.AddNew();
			AssertEquals("Shipment Transport Planning count", 3, ShipmentWrapper.ShipmentTransportPlanning.Count);
		}

		public void TestStaffTrainingAccreditationNumber()
		{
			GlbStaff betty = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(betty.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Blank by default", "", ShipmentWrapper.StaffTrainingAccreditationNumber);

				GenRegCertAccredMaintList cert1 = betty.Certificates.AddNew();
				cert1.XZ_Type = "DTA";
				cert1.XZ_RefNumber = "111-222-333";

				GenRegCertAccredMaintList cert2 = betty.Certificates.AddNew();
				cert2.XZ_Type = "ZZZ";
				cert2.XZ_RefNumber = "3-4-5";

				Factory.Save();

				AssertEquals("Correct number", cert1.XZ_RefNumber, ShipmentWrapper.StaffTrainingAccreditationNumber);

				cert1.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-10);
				Factory.Save();
				AssertEquals("Blank, as expired", ZString.Empty, ShipmentWrapper.StaffTrainingAccreditationNumber);

				cert1.XZ_ExpiryOrDueDate = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("Correct number - as no expiry specified", cert1.XZ_RefNumber, ShipmentWrapper.StaffTrainingAccreditationNumber);
			}
		}

		public void TestDeclarationNumber()
		{
			AssertEquals(ZString.Empty, ShipmentWrapper.DeclarationNumber);

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			BaseJobDeclaration declaration1 = BaseJobDeclaration.New(Factory);
			declaration1.DeclarationNumber = "123";
			declaration1.JE_JS = Shipment.PK;
			AssertEquals("123", ShipmentWrapper.DeclarationNumber);
		}

		public void TestDisplayLogo()
		{
			DocumentsDataRegistry.Instance.DisplayLogo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Should return false", ZBool.False, ShipmentWrapper.DisplayLogo);

			DocumentsDataRegistry.Instance.DisplayLogo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Should return true", ZBool.True, ShipmentWrapper.DisplayLogo);
		}

		public void TestDeclarationForDocuments()
		{
			AssertNull("Declaration For Documents", ShipmentWrapper.DeclarationForDocuments);

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_JS = Shipment.PK;
			AssertNotNull("Declaration For Documents", ShipmentWrapper.DeclarationForDocuments);
		}

		#endregion

		#region Heading Tests

		public void TestHeadingTransportModeWithPackingMode()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;

			ZString expected = Core.Constants.TransportModeDescriptions.Sea.Replace(" Freight", "");
			AssertEquals("No packing mode in heading", expected, ShipmentWrapper.HeadingTransportMode);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			expected = Shipment.JS_PackingMode + " " + Core.Constants.TransportModeDescriptions.Sea.Replace(" Freight", "");
			AssertEquals("Packing mode in heading", expected, ShipmentWrapper.HeadingTransportMode);
		}

		public void TestHeadingTransportMode()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModeDescriptions.Air.Replace(" Freight", ""), ShipmentWrapper.HeadingTransportMode);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(Core.Constants.TransportModeDescriptions.Rail.Replace(" Freight", ""), ShipmentWrapper.HeadingTransportMode);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals(Core.Constants.TransportModeDescriptions.AirSea.Replace(" Freight", ""), ShipmentWrapper.HeadingTransportMode);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertEquals(Core.Constants.TransportModeDescriptions.SeaAir.Replace(" Freight", ""), ShipmentWrapper.HeadingTransportMode);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(Core.Constants.TransportModeDescriptions.Road.Replace(" Freight", ""), ShipmentWrapper.HeadingTransportMode);
		}

		public void TestCusEntryNumHeadingForManifest()
		{
			Shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			Shipment.CustomsEntryNumber = "Test";
			AssertEquals("Heading is ECN", CusEntryNumberTypes.Australia.ECN + ":", ShipmentWrapper.CusEntryNumHeadingForManifest);

			Shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			AssertEquals("Heading is CRN", CusEntryNumberTypes.Australia.CRN + ":", ShipmentWrapper.CusEntryNumHeadingForManifest);
		}

		public void TestCusEntryNumHeading()
		{
			ZString savedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";

				Shipment.JS_RL_NKOrigin = "AUSYD";
				Shipment.JS_RL_NKDestination = "DEHAM";

				AssertEquals("No Heading", ZString.Empty, ShipmentWrapper.CusEntryNumHeading);

				Shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
				AssertEquals("No Heading", ZString.Empty, ShipmentWrapper.CusEntryNumHeading);

				Shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
				AssertEquals("No Heading", ZString.Empty, ShipmentWrapper.CusEntryNumHeading);

				Shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CAN;
				AssertEquals("No Heading", ZString.Empty, ShipmentWrapper.CusEntryNumHeading);

				Shipment.CustomsEntryNumberType = CusEntryNumberTypes.HongKong.ExportLicense;
				AssertEquals("No Heading", ZString.Empty, ShipmentWrapper.CusEntryNumHeading);

				Shipment.CustomsEntryNumberType = CusEntryNumberTypes.HongKong.ImportLicense;
				AssertEquals("No Heading", ZString.Empty, ShipmentWrapper.CusEntryNumHeading);

				Shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX1;
				AssertEquals("Heading is EX1", CusEntryNumberTypes.Australia.EX1, ShipmentWrapper.CusEntryNumHeading);

				Shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
				Shipment.CustomsEntryNumber = "Test";
				AssertEquals("Heading is CRN:", CusEntryNumberTypes.Australia.CRN + ": ", ShipmentWrapper.CusEntryNumHeading);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = savedCountry;
			}
		}

		public void TestColoadHouseDisplay()
		{
			AssertEquals("", ShipmentWrapper.ColoadHouseDisplay);

			Shipment.JS_JS_ColoadMasterShipment = (Factory.New<CommonShipment>()).PK;
			Factory.Save();
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("(co-load house)", ShipmentWrapper.ColoadHouseDisplay);
		}

		public void TestMasterColoadDisplay()
		{
			AssertEquals("", ShipmentWrapper.MasterHouseDisplay);

			Shipment.JS_JS_ColoadMasterShipment = (Factory.New<CommonShipment>()).PK;
			Factory.Save();
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Master:", ShipmentWrapper.MasterHouseDisplay);
		}

		public void TestColoadMasterHouseBillHeading()
		{
			Shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("HOUSE BILL OF LADING", ShipmentWrapper.ColoadMasterHouseBillHeading);

			Shipment.CoLoadShipments.AddNew();
			Shipment.CoLoadShipments.AddNew();
			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals("MASTER HOUSE", ShipmentWrapper.ColoadMasterHouseBillHeading);
		}

		public void TestContainerWeightHeading()
		{
			AssertEquals("Default should be empty", "", ShipmentWrapper.ContainerWeightHeading);

			CommonConsol consol = CreateImportConsol(Shipment);
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();
			var line3 = Shipment.OuterPackLines.AddNew();
			line1.JL_ActualWeight = 1M;
			line2.JL_ActualWeight = 3M;
			line3.JL_ActualWeight = 4M;
			line1.Containers.RemoveAll();
			line2.Containers.RemoveAll();
			line3.Containers.RemoveAll();

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Lines not assigned to a container, heading should be empty", "", ShipmentWrapper.ContainerWeightHeading);

			line1.SetContainer(container1.PK);
			line2.SetContainer(container2.PK);
			line3.SetContainer(container2.PK);
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Should not be empty", "WEIGHT", ShipmentWrapper.ContainerWeightHeading);

			line1.JL_ActualWeight = 0M;
			line2.JL_ActualWeight = 0M;
			line3.JL_ActualWeight = 0M;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Lines doesnt specify weight, Heading is empty", "", ShipmentWrapper.ContainerWeightHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			Shipment.JS_ActualWeight = 10M;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerWeightHeading", "", ShipmentWrapper.ContainerWeightHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerWeightHeading", "", ShipmentWrapper.ContainerWeightHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerWeightHeading", "WEIGHT", ShipmentWrapper.ContainerWeightHeading);
		}

		public void TestContainerVolumeHeading()
		{
			AssertEquals("Default should be empty", "", ShipmentWrapper.ContainerVolumeHeading);

			CommonConsol consol = CreateImportConsol(Shipment);
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();
			var line3 = Shipment.OuterPackLines.AddNew();
			line1.JL_ActualVolume = 1M;
			line2.JL_ActualVolume = 3M;
			line3.JL_ActualVolume = 4M;
			line1.Containers.RemoveAll();
			line2.Containers.RemoveAll();
			line3.Containers.RemoveAll();

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Lines not assigned to a container, heading should be empty", "", ShipmentWrapper.ContainerVolumeHeading);

			line1.SetContainer(container1.PK);
			line2.SetContainer(container2.PK);
			line3.SetContainer(container2.PK);
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Should not be empty", "VOLUME", ShipmentWrapper.ContainerVolumeHeading);

			line1.JL_ActualVolume = 0M;
			line2.JL_ActualVolume = 0M;
			line3.JL_ActualVolume = 0M;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Lines doesnt specify volume, Heading is empty", "", ShipmentWrapper.ContainerVolumeHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			Shipment.JS_ActualVolume = 10M;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerVolumeHeading", "", ShipmentWrapper.ContainerVolumeHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerVolumeHeading", "", ShipmentWrapper.ContainerVolumeHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerVolumeHeading", "VOLUME", ShipmentWrapper.ContainerVolumeHeading);
		}

		public void TestContainerPackageHeading()
		{
			AssertEquals("Default should be empty", "", ShipmentWrapper.ContainerPackageHeading);

			CommonConsol consol = CreateImportConsol(Shipment);
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();
			var line3 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 1;
			line2.JL_PackageCount = 3;
			line3.JL_PackageCount = 4;
			line1.Containers.RemoveAll();
			line2.Containers.RemoveAll();
			line3.Containers.RemoveAll();

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Lines not assigned to a container, heading should be empty", "", ShipmentWrapper.ContainerPackageHeading);

			line1.SetContainer(container1.PK);
			line2.SetContainer(container2.PK);
			line3.SetContainer(container2.PK);
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Should not be empty", "PACKS", ShipmentWrapper.ContainerPackageHeading);

			line1.JL_PackageCount = 0;
			line2.JL_PackageCount = 0;
			line3.JL_PackageCount = 0;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Lines doesnt specify packages, Heading is empty", "", ShipmentWrapper.ContainerPackageHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			Shipment.JS_OuterPacks = 10;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerPackageHeading", "", ShipmentWrapper.ContainerPackageHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerPackageHeading", "", ShipmentWrapper.ContainerPackageHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerPackageHeading", "", ShipmentWrapper.ContainerPackageHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("ContainerPackageHeading", "PACKS", ShipmentWrapper.ContainerPackageHeading);
		}

		public void TestOwnerRefAndOrderRefHeading()
		{
			AssertEquals("ORDER NUMBERS / REFERENCE", ShipmentWrapper.OwnerRefAndOrderRefHeading);
		}

		public void TestConsigneeOrgHeading()
		{
			AssertEquals("CONSIGNEE", ShipmentWrapper.ConsigneeOrgHeading);
		}

		public void ConsignorOrgHeading()
		{
			AssertEquals("CONSIGNOR", ShipmentWrapper.ConsignorOrgHeading);
		}

		#endregion

		#region Customs Entry Number Tests

		public void TestUniqueConsignmentReference()
		{
			string realEntryNumberFromCustoms = "191-12345679";
			var dec = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			dec.JE_JS = shipment.PK;

			CusEntryHeader entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = realEntryNumberFromCustoms;

			AssertEquals("191-12345679", shipmentWrapper.UniqueConsignmentReference);
		}

		public void TestCustomsEntryNumForECN()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Customs entry number should be blank", ZString.Empty, shipmentWrapper.CustomsEntryNumForECN);

			shipment.CustomsEntryNumber = "Test";
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
			AssertEquals("Customs entry number should be blank", ZString.Empty, shipmentWrapper.CustomsEntryNumForECN);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			AssertEquals("Customs entry number should not be empty", "Test", shipmentWrapper.CustomsEntryNumForECN);
		}

		public void TestCusEntryNumForManifest()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Number is empty", ZString.Empty, shipmentWrapper.CusEntryNumForManifest);

			shipment.CustomsEntryNumber = "Test";
			AssertEquals("Number is not empty", shipment.CustomsEntryNumber, shipmentWrapper.CusEntryNumForManifest);
		}

		public void TestSendingarnumer()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Iceland);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.Australia.CRN;
			cusEntryNum.CE_EntryNum = "test";
			cusEntryNum.CE_ParentID = shipment.PK;
			cusEntryNum.CE_ParentTable = "JobShipment";
			cusEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("The sendingarnumer for that shipment should be  - test", "test", shipmentWrapper.Sendingarnumer);
		}

		public void TestSendingarnumerWithoutCheckDigit()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Iceland);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.Australia.CRN;
			cusEntryNum.CE_EntryNum = "G-123-4567-8-AU-SYD-W123-P";
			cusEntryNum.CE_ParentID = shipment.PK;
			cusEntryNum.CE_ParentTable = "JobShipment";
			cusEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("The sendingarnumer withour check digit", "G-123-4567-8-AU-SYD-W123", shipmentWrapper.SendingarnumerWithoutCheckDigit);
		}

		public void TestSendingarnumerGGGG()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Iceland);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.Australia.CRN;
			cusEntryNum.CE_EntryNum = "G-123-4567-8-AU-SYD-W123-P";
			cusEntryNum.CE_ParentID = shipment.PK;
			cusEntryNum.CE_ParentTable = "JobShipment";
			cusEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("The sendingarnumer GGGG", "W123", shipmentWrapper.SendingarnumerGGGG);
		}

		public void TestECNFromDeclaration()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			Enterprise.Customs.AU.Declaration.Business.JobDeclaration declaration = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.DeclarationNumber = "Entry Num Dec";
			AssertEquals("ECN should be 'Entry Num Dec'", "Entry Num Dec", shipmentWrapper.ECN);
		}

		public void TestHBLCustomsEntryNumberList()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CusEntryNumber cus1 = shipment.CusEntryNumbers.AddNew();
			cus1.CE_EntryType = "CUS";
			cus1.CE_EntryNum = "111";
			CusEntryNumber cus2 = shipment.CusEntryNumbers.AddNew();
			cus2.CE_EntryType = "ENT";
			cus2.CE_EntryNum = "222";
			CusEntryNumber cus3 = shipment.CusEntryNumbers.AddNew();
			cus3.CE_EntryType = "NUM";
			cus3.CE_EntryNum = "333";

			DocShipment wrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Displays all numbers comma separated with type", "CUS: 111, ENT: 222, NUM: 333", wrapper.HBLCustomsEntryNumberList);
		}

		public void TestColoadShipmentPermitNumbers()
		{
			string savedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "GB";

				CommonShipment masterShipment = Factory.New<CommonShipment>();
				DocShipment shipmentWrapper = DocShipment.New(masterShipment, Factory);

				masterShipment.JS_RL_NKOrigin = "AUBNE";
				masterShipment.JS_RL_NKDestination = "DEHAM";

				CommonShipment coloadShipment1 = Factory.New<CommonShipment>();
				CommonShipment coloadShipment2 = Factory.New<CommonShipment>();
				CommonShipment coloadShipment3 = Factory.New<CommonShipment>();

				coloadShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
				coloadShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
				coloadShipment3.JS_JS_ColoadMasterShipment = masterShipment.PK;

				coloadShipment1.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;

				coloadShipment2.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX3;

				coloadShipment3.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;
				coloadShipment3.CustomsEntryNumber = "CRN3333333";

				Assert("Codes without numbers are displayed for GB", shipmentWrapper.ColoadShipmentPermitNumbers.Contains("ECN"));
				Assert("Exemption codes are displayed as normal for non-AU", shipmentWrapper.ColoadShipmentPermitNumbers.Contains("EX3"));
				Assert("Codes with numbers are displayed for GB", shipmentWrapper.ColoadShipmentPermitNumbers.Contains("CRN: CRN3333333"));

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";

				masterShipment = Factory.New<CommonShipment>();
				shipmentWrapper = DocShipment.New(masterShipment, Factory);

				masterShipment.JS_RL_NKOrigin = "AUBNE";
				masterShipment.JS_RL_NKDestination = "DEHAM";

				coloadShipment1 = Factory.New<CommonShipment>();
				coloadShipment2 = Factory.New<CommonShipment>();
				coloadShipment3 = Factory.New<CommonShipment>();

				coloadShipment1.JS_RL_NKOrigin = "AUBNE";
				coloadShipment1.JS_RL_NKDestination = "DEHAM";
				coloadShipment2.JS_RL_NKOrigin = "AUBNE";
				coloadShipment2.JS_RL_NKDestination = "DEHAM";
				coloadShipment3.JS_RL_NKOrigin = "AUBNE";
				coloadShipment3.JS_RL_NKDestination = "DEHAM";

				coloadShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
				coloadShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
				coloadShipment3.JS_JS_ColoadMasterShipment = masterShipment.PK;

				coloadShipment1.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;

				coloadShipment2.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX3;

				coloadShipment3.CustomsEntryNumber = "CRN3333333";
				coloadShipment3.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CRN;

				Assert("Codes without numbers are not displayed for AU export", !shipmentWrapper.ColoadShipmentPermitNumbers.Contains("ECN"));
				Assert("Exemption codes are displayed for AU export", shipmentWrapper.ColoadShipmentPermitNumbers.Contains("EX3"));
				Assert("Codes with numbers are displayed for AU export", shipmentWrapper.ColoadShipmentPermitNumbers.Contains("CRN: CRN3333333"));
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = savedCountry;
			}
		}

		#endregion

		#region OrgAddress Tests

		public void TestReturnEmptyToAndReturnEmptyToHeading()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var newOrg = Factory.New<OrgHeader>();
			newOrg.OH_FullName = "Container Return Ltd.";
			OrgAddress address = newOrg.Addresses.AddNew();
			address.OA_Address1 = "123 OneToThree St.";
			address.OA_Address2 = "Mayhem Tower";
			consol.JK_OA_ContainerYardEmptyReturnAddress = address.PK;

			AssertEquals("Return Empty To heading", "RETURN EMPTY TO", shipmentWrapper.ReturnEmptyToHeading);

			shipment.JS_PackingMode = "LCL";
			AssertEquals("Return Empty To heading", "", shipmentWrapper.ReturnEmptyToHeading);
		}

		public void TestImportPickupAddress()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			CommonConsol consol = CreateImportConsol(shipment);
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			address.OA_Address1 = "UnpackDepotAddress";
			consol.JK_OA_UnpackDepotAddress = address.PK;

			AssertNotNull("Pickup From Address is not null", shipmentWrapper.ImportPickUpAddress);
			AssertEquals("Pickup From Address field is Arrival Consol's Arrival depot address", address.OA_Address1.Trim(), shipmentWrapper.ImportPickUpAddress.Address1);
			AssertEquals("Pickup From City field is Arrival Consol's Arrival depot address", address.OA_City.Trim(), shipmentWrapper.ImportPickUpAddress.City);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Pickup From Address field is Arrival Consol's Arrival depot address UnpackDepotAddress", address.OA_Address1.Trim(), shipmentWrapper.ImportPickUpAddress.Address1);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OA_UnpackDepotAddress = address.PK;
			AssertEquals("Pickup From Address field is Arrival Consol's Arrival depot address", address.OA_Address1.Trim(), shipmentWrapper.ImportPickUpAddress.Address1);
			AssertEquals("Pickup From City field is Arrival Consol's Arrival depot address", address.OA_City.Trim(), shipmentWrapper.ImportPickUpAddress.City);

			var cTOArrival = Factory.NewWithValidTestData<OrgAddress>();
			cTOArrival.OA_Address1 = "ArrivalCTOAddress";
			consol.JK_OA_ArrivalCTOAddress = cTOArrival.PK;

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("PickUp From Address field is CTO Arrival Address", cTOArrival.OA_Address1.Trim(), shipmentWrapper.ImportPickUpAddress.Address1);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("PickUp From Address field is CTO Arrival Address", cTOArrival.OA_Address1.Trim(), shipmentWrapper.ImportPickUpAddress.Address1);

			var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
			importReleaseDepot.OA_Address1 = "ImportReleaseDepot";
			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
			AssertEquals("Should Be Import Release Depot", importReleaseDepot.OA_Address1, shipmentWrapper.ImportPickUpAddress.Address1);
		}

		public void TestImportSeaFCLPickupAddress()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_PackingMode = "FCL";

			CommonConsol consol = CreateImportConsol(shipment);
			consol.JK_ConsolMode = "FCL";

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_ArrivalCTOAddress = address.PK;
			AssertEquals("Pickup From Address field is Arrival Consol's Arrival CTO address", address.OA_Address1, shipmentWrapper.ImportPickUpAddress.Address1);
			AssertEquals("Pickup From City field is Arrival Consol's Arrival CTO address", address.OA_City, shipmentWrapper.ImportPickUpAddress.City);

			consol.JK_OA_UnpackDepotAddress = address.PK;
			shipment.JS_PackingMode = "LCL";
			AssertEquals("Pickup From Address field is Arrival Consol's Pack Depot address", address.OA_Address1, shipmentWrapper.ImportPickUpAddress.Address1);
			AssertEquals("Pickup From City field is Arrival Consol's Pack Depot address", address.OA_City, shipmentWrapper.ImportPickUpAddress.City);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			consol.JK_OA_UnpackDepotAddress = address.PK;
			consol.JK_OA_ArrivalCTOAddress = address.PK;
			AssertEquals("Pickup From Address field is Arrival Consol's Arrival CTO address", address.OA_Address1, shipmentWrapper.ImportPickUpAddress.Address1);
			AssertEquals("Pickup From City field is Arrival Consol's Arrival CTO address", address.OA_City, shipmentWrapper.ImportPickUpAddress.City);
		}

		public void TestExportReceivingDepot()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));

			CommonConsol consol = CreateExportConsol(shipment);
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var consolAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consolAddress.OA_Address1 = "123 This Street";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			consol.JK_OA_DepartureCTOAddress = consolAddress.PK;
			AssertEquals("Should Be Consol CTO Address", consolAddress.OA_Address1, shipmentWrapper.ExportReceivingDepot.Address1);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			var consolPackDepotAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consolPackDepotAddress.OA_Address1 = "123 That Street";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;
			AssertEquals("Should Be Consol Pack Depot Address", consolPackDepotAddress.OA_Address1, shipmentWrapper.ExportReceivingDepot.Address1);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Should Be Consol Pack Depot Address", consolPackDepotAddress.OA_Address1, shipmentWrapper.ExportReceivingDepot.Address1);

			var exportReceivingDepot = Factory.New<OrgAddress>();
			exportReceivingDepot.OA_Address1 = "123 Fake Street";
			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;

			AssertEquals("Should Be Shipment Export Receiving Depot", exportReceivingDepot.OA_Address1, shipmentWrapper.ExportReceivingDepot.Address1);
		}

		public void TestImportReleaseDepot()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));

			var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;

			AssertEquals("Should Be Shipment Import Release Depot", importReleaseDepot.OA_Address1, shipmentWrapper.ImportReleaseDepot.Address1);
		}

		#region Delivery Order

		public void TestDeliveryOrderPickup()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			var header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "Test Org";
			header1.MainAddress.OA_Address1 = "123 Maple Rd";
			header1.OH_Code = "OHCode";
			shipment.ConsignorPK = header1.PK;
			Assert(shipmentWrapper.DeliveryOrderPickup.Contains("TEST ORG\n123 MAPLE RD"));

			OrgHeader header2 = OrgHeader.New(Factory);
			header2.MainAddress.OA_Address1 = "56 Oak Ln";
			shipment.ConsignorPickupAddress.E2_OA_Address = header2.MainAddress.PK;
			Assert(shipmentWrapper.DeliveryOrderPickup.Contains("56 OAK LN"));

			var address3 = Factory.New<OrgAddress>();
			address3.OA_Address1 = "99 Mike Pl";
			shipment.ConsignorPickupAddress.E2_OA_Address = address3.PK;
			Assert(shipmentWrapper.DeliveryOrderPickup.Contains("99 MIKE PL"));
		}

		public void TestDeliveryOrderDeliver()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			var header1 = Factory.New<OrgHeader>();
			header1.OH_FullName = "Test Org";
			header1.MainAddress.OA_Address1 = "45 TREEHOUSE";
			header1.OH_Code = "OHCode";
			shipment.ConsigneePK = header1.PK;
			Assert(shipmentWrapper.DeliveryOrderDeliver.Contains("TEST ORG\n45 TREEHOUSE"));

			OrgHeader header2 = OrgHeader.New(Factory);
			header2.MainAddress.OA_Address1 = "111 VILE RD";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = header2.MainAddress.PK;
			Assert(shipmentWrapper.DeliveryOrderDeliver.Contains("111 VILE RD"));

			var address3 = Factory.New<OrgAddress>();
			address3.OA_Address1 = "0 RED RD";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			Assert(shipmentWrapper.DeliveryOrderDeliver.Contains("0 RED RD"));
		}

		#endregion

		#endregion

		#region Coload Master Manifest Tests

		public void TestColoadMasterManifestVolumeSEAMode()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_DocumentedVolume = 10.5M;
			shipment.JS_ActualVolume = 8.00M;
			shipment.JS_ManifestedVolume = 5.05M;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");
			if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Actual)
			{
				AssertEquals("Should be the actual volume", "8 M3", shipmentWrapper.ColoadMasterManifestVolume);
			}
			else if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Client)
			{
				AssertEquals("Should be the client volume", "10.5 M3", shipmentWrapper.ColoadMasterManifestVolume);
			}
			else if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Carrier)
			{
				AssertEquals("Should be the actual volume", "5.05 M3", shipmentWrapper.ColoadMasterManifestVolume);
			}
			else
			{
				throw new Exception("The Weight Display option returns invalid value");
			}

			AssertEquals("VOLUME", shipmentWrapper.ColoadMasterManifestVolumeHeading);
		}

		public void TestColoadMasterManifestVolumeAIRMode()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_DocumentedVolume = 10.5M;
			shipment.JS_ActualVolume = 8.00M;
			shipment.JS_ManifestedVolume = 5.05M;
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");
			if (Env.Registry.CoLoadMasterManifestDisplayVolumeWhenAir)
			{
				if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Actual)
				{
					AssertEquals("Should be the actual volume", "8 M3", shipmentWrapper.ColoadMasterManifestVolume);
				}
				else if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Client)
				{
					AssertEquals("Should be the client volume", "10.5 M3", shipmentWrapper.ColoadMasterManifestVolume);
				}
				else if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Carrier)
				{
					AssertEquals("Should be the actual volume", "5.05 M3", shipmentWrapper.ColoadMasterManifestVolume);
				}
				else
				{
					throw new Exception("The Weight Display option returns invalid value");
				}

				AssertEquals("VOLUME", shipmentWrapper.ColoadMasterManifestVolumeHeading);
			}
			else
			{
				AssertEquals("", shipmentWrapper.ColoadMasterManifestVolume);
				AssertEquals("", shipmentWrapper.ColoadMasterManifestVolumeHeading);
			}
		}

		public void TestColoadMasterManifestChargeableAIRMode()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualChargeable = 8.00M;
			shipment.JS_DocumentedChargeable = 10.5M;
			shipment.JS_ManifestedChargeable = 5.050M;

			if (Env.Registry.CoLoadMasterManifestDisplayChargeableWhenAir)
			{
				if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Actual)
				{
					AssertEquals("Should be the actual chargeable", "8 KG", shipmentWrapper.ColoadMasterManifestChargeable);
				}
				else if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Client)
				{
					AssertEquals("Should be the client chargeable", "10.5 KG", shipmentWrapper.ColoadMasterManifestChargeable);
				}
				else if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Carrier)
				{
					AssertEquals("Should be the actual chargeable", "5.05 KG", shipmentWrapper.ColoadMasterManifestChargeable);
				}
				else
				{
					throw new Exception("The Weight Display option returns invalid value");
				}
				AssertEquals("CHARGEABLE", shipmentWrapper.ColoadMasterManifestChargeableHeading);
			}
			else
			{
				AssertEquals("", shipmentWrapper.ColoadMasterManifestChargeable);
				AssertEquals("", shipmentWrapper.ColoadMasterManifestChargeableHeading);
			}
		}

		public void TestColoadMasterManifestChargeableSEAMode()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_ActualChargeable = 8.00M;
			shipment.JS_DocumentedChargeable = 10.5M;
			shipment.JS_ManifestedChargeable = 5.050M;

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");

			if (Env.Registry.CoLoadMasterManifestDisplayChargeableWhenSea)
			{
				if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Actual)
				{
					AssertEquals("Should be the actual chargeable", "8 M3", shipmentWrapper.ColoadMasterManifestChargeable);
				}
				else if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Client)
				{
					AssertEquals("Should be the client chargeable", "10.5 M3", shipmentWrapper.ColoadMasterManifestChargeable);
				}
				else if (Env.Registry.CoLoadMasterManifestWeightAndVolumeDisplay == WeightVolumeDisplayTypes.Codes.Carrier)
				{
					AssertEquals("Should be the carrier chargeable", "5.05 M3", shipmentWrapper.ColoadMasterManifestChargeable);
				}
				else
				{
					throw new Exception("The Weight Display option returns invalid value");
				}
				AssertEquals("CHARGEABLE", shipmentWrapper.ColoadMasterManifestChargeableHeading);
			}
			else
			{
				AssertEquals("", shipmentWrapper.ColoadMasterManifestChargeable);
				AssertEquals("", shipmentWrapper.ColoadMasterManifestChargeableHeading);
			}
		}

		public void TestExportColoadMasterManifestTransportHeading()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("", shipmentWrapper.ColoadMasterManifestTransportHeading);

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");

			CommonConsol consol = CreateExportConsol(shipment);
			consol.JK_TransportMode = "SEA";
			AssertEquals("VESSEL / VOYAGE / IMO(Lloyds)", shipmentWrapper.ColoadMasterManifestTransportHeading);

			consol.JK_TransportMode = "AIR";
			AssertEquals("FLIGHT & DATE", shipmentWrapper.ColoadMasterManifestTransportHeading);
		}

		public void TestImportColoadMasterManifestTransportHeading()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("", shipmentWrapper.ColoadMasterManifestTransportHeading);

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");

			CommonConsol consol = CreateImportConsol(shipment);
			consol.JK_TransportMode = "SEA";
			AssertEquals("VESSEL / VOYAGE / IMO(Lloyds)", shipmentWrapper.ColoadMasterManifestTransportHeading);

			consol.JK_TransportMode = "AIR";
			AssertEquals("FLIGHT & DATE", shipmentWrapper.ColoadMasterManifestTransportHeading);
		}

		public void TestColoadMasterManifestTransportInfo()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("", shipmentWrapper.ColoadMasterManifestTransportHeading);

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");

			CommonConsol consol = CreateImportConsol(shipment);
			consol.JK_TransportMode = "SEA";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "11111";
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			transport.JW_Vessel = vessel.RV_Code;

			ZString expected = vessel.RV_Code + " / 11111 / ";
			expected += vessel.RV_LloydsNumber.IsEmpty ? ZString.Empty : vessel.RV_LloydsNumber;
			AssertEquals(expected, shipmentWrapper.ColoadMasterManifestTransportInfo);
		}

		public void TestColoadMasterManifestShippingLine()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("", shipmentWrapper.ColoadMasterManifestShippingLine);

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "CNSHA";

			CommonConsol departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "AUBNE";

			var departureLine = Factory.New<OrgHeader>();
			departureLine.OH_FullName = "TEST DEPARTURE LINE";
			departureConsol.SetDefaultShippingLineAddress(departureLine);
			AssertEquals("Line name matches", "TEST DEPARTURE LINE", shipmentWrapper.ColoadMasterManifestShippingLine);

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));

			var arrivalLine = Factory.New<OrgHeader>();
			arrivalLine.OH_FullName = "TEST ARRIVAL LINE";
			consol.SetDefaultShippingLineAddress(arrivalLine);
			AssertEquals("Line name matches", "TEST ARRIVAL LINE", shipmentWrapper.ColoadMasterManifestShippingLine);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestImportColoadMasterManifestTransportInfo()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("", shipmentWrapper.ColoadMasterManifestTransportHeading);

			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			shipmentWrapper.SetReportNameForTesting("Co-Load Master Manifest");

			CommonConsol consol = CreateImportConsol(shipment);
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_RL_NKLoadPort = "SGSIN";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "11111";
			transport.JW_ETD = ZDateTime.Today;
			transport.JW_ETA = ZDateTime.Today.AddHours(10);
			AssertEquals("11111 / AUBNE / " + ZDateTime.Today.ToString("dd-MMM-yy"), shipmentWrapper.ColoadMasterManifestTransportInfo);
		}

		#endregion

		#region Consignee Consignor Tests
		public void TestConsigneeAddress()
		{
			AssertEquals("No consignee, address should be empty", "", ShipmentWrapper.ConsigneeAddress);

			OrgHeader consignee = OrgHeader.New(Factory);
			consignee.OH_FullName = "CONSIGNEE";
			consignee.Addresses[0].OA_Address1 = "Address1";
			consignee.Addresses[0].OA_Address2 = "Address2";
			Shipment.ConsigneePK = consignee.PK;

			AssertEquals("Should use Consignee's default address", "CONSIGNEE\nADDRESS1\nADDRESS2\n" + Env.CurrentCompany.Country.Description.ToUpper(), ShipmentWrapper.ConsigneeAddress);

			var addr1 = consignee.Addresses.AddNew();
			addr1.OA_Address1 = "Blah1";
			addr1.OA_Address2 = "Blah2";
			Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = addr1.PK;

			AssertEquals("Should use DocsCartage Consignee address", "CONSIGNEE\nBLAH1\nBLAH2\n" + Env.CurrentCompany.Country.Description.ToUpper(), ShipmentWrapper.ConsigneeAddress);
		}

		public void TestConsignorAddress()
		{
			AssertEquals("No consignor, address should be empty", "", ShipmentWrapper.ConsignorAddress);

			OrgHeader consignor = OrgHeader.New(Factory);
			consignor.OH_FullName = "CONSIGNOR";
			consignor.Addresses[0].OA_Address1 = "Address1";
			consignor.Addresses[0].OA_Address2 = "Address2";
			Shipment.ConsignorPK = consignor.PK;

			AssertEquals("Should use Consignor's default address", "CONSIGNOR\nADDRESS1\nADDRESS2\n" + Env.CurrentCompany.Country.Description.ToUpper(), ShipmentWrapper.ConsignorAddress);

			var addr1 = consignor.Addresses.AddNew();
			addr1.OA_Address1 = "Blah1";
			addr1.OA_Address2 = "Blah2";
			Shipment.ConsignorDocumentaryAddress.E2_OA_Address = addr1.PK;

			AssertEquals("Should use DocsCartage Consignor address", "CONSIGNOR\nBLAH1\nBLAH2\n" + Env.CurrentCompany.Country.Description.ToUpper(), ShipmentWrapper.ConsignorAddress);
		}
		public void TestConsigneeContacts()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("", shipmentWrapper.ConsigneeContactName);
			AssertEquals("", shipmentWrapper.ConsigneeContactPhone);
			AssertEquals("", shipmentWrapper.ConsigneeContactFax);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));

			var contact1 = header.Contacts.AddNew();
			contact1.OC_ContactName = "Angie";
			contact1.OC_Phone = "000 - online";
			contact1.OC_Fax = "000 - offline";
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact1.Documents[0].OD_DefaultContact = true;

			var contact2 = header.Contacts.AddNew();
			contact2.OC_ContactName = "Some other consignee contact";
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact2.Documents[0].OD_DefaultContact = false;

			shipment.ConsigneePK = header.PK;

			AssertEquals("Wrong contact for Consignee", "Angie", shipmentWrapper.ConsigneeContactName);
			AssertEquals("Wrong phone for Consignee", "000 - online", shipmentWrapper.ConsigneeContactPhone);
			AssertEquals("Wrong fax for Consignee", "000 - offline", shipmentWrapper.ConsigneeContactFax);

			contact1.OC_Phone = "";
			contact1.OC_Fax = "";
			AssertEquals("Consignee contact phone is Consignee's phone", shipment.Consignee.MainAddress.OA_Phone_Formatted, shipmentWrapper.ConsigneeContactPhone);
			AssertEquals("Consignee contact fax is Consignee's phone", shipment.Consignee.MainAddress.OA_Fax_Formatted, shipmentWrapper.ConsigneeContactFax);

			shipment.Consignee.Contacts.RemoveAll();
			shipment.ConsigneePK = ZGuid.Empty;
			shipment.ConsigneePK = header.PK;
			AssertEquals("Consignee contact name should be 'The Import Manager'", "The Import Manager", shipmentWrapper.ConsigneeContactName);
			AssertEquals("Consignee contact phone is Consignee's phone", shipment.Consignee.MainAddress.OA_Phone_Formatted, shipmentWrapper.ConsigneeContactPhone);
			AssertEquals("Consignee contact fax is Consignee's fax", shipment.Consignee.MainAddress.OA_Fax_Formatted, shipmentWrapper.ConsigneeContactFax);
		}

		public void TestConsignorContacts()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("", shipmentWrapper.ConsignorContactName);
			AssertEquals("", shipmentWrapper.ConsignorContactPhone);
			AssertEquals("", shipmentWrapper.ConsignorContactFax);

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));

			var contact1 = consignor.Contacts.AddNew();
			contact1.OC_ContactName = "Michael";
			contact1.OC_Phone = "000 - online";
			contact1.OC_Fax = "000 - offline";
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			var contact2 = consignor.Contacts.AddNew();
			contact2.OC_ContactName = "Some other consignor contact";
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_DefaultContact = false;

			shipment.ConsignorPK = consignor.PK;

			AssertEquals("Wrong contact for Consignor", "Michael", shipmentWrapper.ConsignorContactName);
			AssertEquals("Wrong phone for Consignor", "000 - online", shipmentWrapper.ConsignorContactPhone);
			AssertEquals("Wrong fax for Consignor", "000 - offline", shipmentWrapper.ConsignorContactFax);

			contact1.OC_Phone = "";
			contact1.OC_Fax = "";
			AssertEquals("Consignor contact phone is Consignor's phone", shipment.Consignor.MainAddress.OA_Phone_Formatted, shipmentWrapper.ConsignorContactPhone);
			AssertEquals("Consignor contact fax is Consignor's fax", shipment.Consignor.MainAddress.OA_Fax_Formatted, shipmentWrapper.ConsignorContactFax);

			shipment.Consignor.Contacts.RemoveAll();
			shipment.ConsignorPK = ZGuid.Empty;
			shipment.ConsignorPK = consignor.PK;
			AssertEquals("Consignor contact name should be 'The Export Manager'", "The Export Manager", shipmentWrapper.ConsignorContactName);
			AssertEquals("Consignor contact phone is Consignor's phone", shipment.Consignor.MainAddress.OA_Phone_Formatted, shipmentWrapper.ConsignorContactPhone);
			AssertEquals("Consignor contact fax is Consignor's fax", shipment.Consignor.MainAddress.OA_Fax_Formatted, shipmentWrapper.ConsignorContactFax);
		}

		public void TestConsignorContact()
		{
			AssertEquals("", ShipmentWrapper.ConsignorContact);
			Shipment.ConsignorDocumentaryAddress.E2_Contact = "ME";
			AssertEquals("ConsignorContact", "ME", ShipmentWrapper.ConsignorContact);
		}

		public void TestConsigneeContact()
		{
			AssertEquals("", ShipmentWrapper.ConsigneeContact);
			Shipment.ConsigneeDocumentaryAddress.E2_Contact = "YOU";
			AssertEquals("ConsigneeContact", "YOU", ShipmentWrapper.ConsigneeContact);
		}

		public void TestConsigneeKennitala()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Consignee Kennitala", ZString.Empty, shipmentWrapper.ConsigneeKennitala);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.ConsigneePK = header.PK;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			OrgCusCode code = header.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.IcelandCodeTypes.Kennitala;
			code.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			code.OK_CustomsRegNo = "Kennitala";
			code.OK_OH = shipment.ConsigneePK;
			Factory.Save();

			AssertEquals("Consignee Kennitala", "Kennitala", shipmentWrapper.ConsigneeKennitala);
		}

		#endregion

		#region Organisations Tests

		public void TestConsignee()
		{
			var shipment = Factory.New<CommonShipment>();
			var wrapper = DocShipment.New(shipment, Factory);

			AssertNull("Consignee should be null", wrapper.Consignee);

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Consignor";
			shipment.ConsignorPK = consignor.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			shipment.ConsigneePK = consignee.PK;

			AssertEquals("Consignee should be a shipment consignee", consignee, wrapper.Consignee.OrgHeader);
			AssertEquals("Consignee should be a shipment consignee", "Consignee", wrapper.Consignee.Name);

			wrapper = DocShipment.New(shipment, Factory);

			((IBODocDataProvider)wrapper).SetDocWrapperContext(new Dictionary<string, object>
				{
					{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Legacy Manufacturer Bill Of Lading" }
				});

			AssertEquals("For manufacturer bill of lading Consignee should be a shipment consignor", consignor, wrapper.Consignee.OrgHeader);
			AssertEquals("For manufacturer bill of lading Consignee should be a shipment consignor", "Consignor", wrapper.Consignee.Name);
		}

		public void TestConsignor()
		{
			var shipment = Factory.New<CommonShipment>();
			var wrapper = DocShipment.New(shipment, Factory);

			AssertNull("Consignor should be null", wrapper.Consignor);

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Consignor";

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer";

			shipment.ConsignorPK = consignor.PK;
			shipment.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;

			AssertEquals("Consignor should be a shipment consignor", consignor, wrapper.Consignor.OrgHeader);
			AssertEquals("Consignor should be a shipment consignor", "Consignor", wrapper.Consignor.Name);

			wrapper = DocShipment.New(shipment, Factory);

			((IBODocDataProvider)wrapper).SetDocWrapperContext(new Dictionary<string, object>
				{
					{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Legacy Manufacturer Bill Of Lading" }
				});

			AssertEquals("For manufacturer bill of lading Consignor should be a shipment manufacturer", manufacturer, wrapper.Consignor.OrgHeader);
			AssertEquals("For manufacturer bill of lading Consignor should be a shipment manufacturer", "Manufacturer", wrapper.Consignor.Name);
		}

		public void TestManufacturerAddress()
		{
			var shipment = Factory.New<CommonShipment>();
			var wrapper = DocShipment.New(shipment, Factory);

			AssertEquals("Manufacturer address should be a shipment manufacturer address",
				shipment.ManufacturerDocAddress, wrapper.ManufacturerAddress.WrappedObject);
		}

		public void TestDeliveryAgent()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull("Delivery Agent should be null", shipmentWrapper.DeliveryAgent);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.JS_OH_DeliveryAgent = header.PK;
			AssertNotNull("Delivery Agent should not be null", shipmentWrapper.DeliveryAgent);
			AssertEquals("Delivery Agent should be a DocOrganisation", typeof(DocOrganisation), shipmentWrapper.DeliveryAgent.GetType());
		}

		public void TestExportBroker()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull("Export Broker should be null", shipmentWrapper.ExportBroker);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.JS_OH_ExportBroker = header.PK;
			AssertNotNull("Export Broker should not be null", shipmentWrapper.ExportBroker);
			AssertEquals("Export Broker should be a DocOrganisation", typeof(DocOrganisation), shipmentWrapper.ExportBroker.GetType());
		}

		public void TestExportCartage()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull("Export Cartage should be null", shipmentWrapper.ExportCartage);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.DocsAndCartage.PickupCartageCoPK = header.PK;
			AssertNotNull("Export Cartage should not be null", shipmentWrapper.ExportCartage);
			AssertEquals("Export Cartage should be a DocOrganisation", typeof(DocOrganisation), shipmentWrapper.ExportCartage.GetType());
		}

		public void TestHandledOnBehalfOfForwarder()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull("Handled On Behalf Of Forwarder should be null", shipmentWrapper.HandledOnBehalfOfForwarder);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.JS_OH_HandledOnBehalfOfForwarder = header.PK;
			AssertNotNull("Handled On Behalf Of Forwarder should not be null", shipmentWrapper.HandledOnBehalfOfForwarder);
			AssertEquals("Handled On Behalf Of Forwarder should be a DocOrganisation", typeof(DocOrganisation), shipmentWrapper.HandledOnBehalfOfForwarder.GetType());
		}

		public void TestImportBroker()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull("Import Broker should be null", shipmentWrapper.ImportBroker);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.JS_OH_ImportBroker = header.PK;
			AssertNotNull("Import Broker should not be null", shipmentWrapper.ImportBroker);
			AssertEquals("Import Broker should be a DocOrganisation", typeof(DocOrganisation), shipmentWrapper.ImportBroker.GetType());
		}

		public void TestImportCartage()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull("Import Cartage should be null", shipmentWrapper.ImportCartage);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.DocsAndCartage.DeliveryCartageCoPK = header.PK;
			AssertNotNull("Import Cartage should not be null", shipmentWrapper.ImportCartage);
			AssertEquals("Import Cartage should be a DocOrganisation", typeof(DocOrganisation), shipmentWrapper.ImportCartage.GetType());
		}

		public void TestTranshipAgent()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertNull("Tranship Agent should be null", shipmentWrapper.TranshipAgent);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.JS_OH_TranshipAgent = header.PK;
			AssertNotNull("Tranship Agent should not be null", shipmentWrapper.TranshipAgent);
			AssertEquals("Tranship Agent should be a DocOrganisation", typeof(DocOrganisation), shipmentWrapper.TranshipAgent.GetType());
		}

		public void TestImportCartageOrganisation()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			shipment.ConsignorPK = header.PK;

			var pADAddress = shipment.Consignor.Addresses.AddNew();
			pADAddress.OA_CompanyNameOverride = "Pad Co";
			pADAddress.OA_Address1 = "123 Pad St.";
			pADAddress.OA_Address2 = "Padville";
			pADAddress.AddressCapability.SetCapabilityEnabled("PAD");

			pADAddress.OA_OH = header.PK;

			shipment.DocsAndCartage.DeliveryCartageCoPK = header.PK;
			AssertEquals("Import org is Import cartage", DocOrganisation.New(header, Factory).Name, shipmentWrapper.ImportCartageOrganisation.Name);
		}

		public void TestExportCartageOrganisation()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			shipment.ConsignorPK = header.PK;

			var pADAddress = shipment.Consignor.Addresses.AddNew();
			pADAddress.OA_CompanyNameOverride = "Pad Co";
			pADAddress.OA_Address1 = "123 Pad St.";
			pADAddress.OA_Address2 = "Padville";
			pADAddress.AddressCapability.SetCapabilityEnabled("PAD");

			pADAddress.OA_OH = header.PK;

			shipment.DocsAndCartage.PickupCartageCoPK = header.PK;
			AssertEquals("Export org is Import cartage", DocOrganisation.New(header, Factory).Name, shipmentWrapper.ExportCartageOrganisation.Name);
		}

		public void TestBillTo()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_UniqueConsignRef = "9876";

			JobHeader jobHeaderBisObj = CreateJobHeader(shipment);

			AssertEquals("Bill To is empty", null, shipmentWrapper.BillTo);

			var orgHeaderBisObj = Factory.New<OrgHeader>();
			orgHeaderBisObj.OH_Code = "TTT";
			orgHeaderBisObj.OH_FullName = "Test";
			orgHeaderBisObj.MainAddress.OA_Address1 = "Address 1";
			orgHeaderBisObj.MainAddress.OA_Address2 = "Address 2";
			orgHeaderBisObj.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgHeader imFreightBillTo = Factory.New<OrgHeader>();
			imFreightBillTo.OH_Code = "IMFreight";
			imFreightBillTo.OH_FullName = "imFreightBillTo";
			imFreightBillTo.MainAddress.OA_Address1 = "imFreightBillTo 1";
			imFreightBillTo.MainAddress.OA_Address2 = "imFreightBillTo 2";

			OrgHeader exFreightBillTo = Factory.New<OrgHeader>();
			exFreightBillTo.OH_Code = "exFreight";
			exFreightBillTo.OH_FullName = "exFreightBillTo";
			exFreightBillTo.MainAddress.OA_Address1 = "exFreightBillTo 1";
			exFreightBillTo.MainAddress.OA_Address2 = "exFreightBillTo 2";

			Factory.Save();

			jobHeaderBisObj.LocalChargesPK = orgHeaderBisObj.PK;
			DocOrganisation organisation = DocOrganisation.New(orgHeaderBisObj, Factory);
			AssertEquals("Bill to is org header address", organisation.PostalAddress, shipmentWrapper.BillTo.PostalAddress);

			orgHeaderBisObj.SetRelatedParty(imFreightBillTo, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			orgHeaderBisObj.SetRelatedParty(exFreightBillTo, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var unlocoBisObject = Factory.LoadTop1<RefUNLOCO>(filter);
			shipment.JS_RL_NKDestination = unlocoBisObject.RL_Code;
			AssertEquals("Bill to is IM Freight Bill To address", "IMFREIGHTBILLTO\nIMFREIGHTBILLTO 1\nIMFREIGHTBILLTO 2\nAUSTRALIA", shipmentWrapper.BillTo.PostalAddress);

			orgHeaderBisObj.SetRelatedParty(orgHeaderBisObj, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			shipment.JS_RL_NKDestination = "";
			shipment.JS_RL_NKOrigin = unlocoBisObject.RL_Code;
			AssertEquals("Bill to is EX Freight Bill To address", "EXFREIGHTBILLTO\nEXFREIGHTBILLTO 1\nEXFREIGHTBILLTO 2\nAUSTRALIA", shipmentWrapper.BillTo.PostalAddress);
		}

		public void TestRequestedBillToAddress()
		{
			var requestedBillToAddress = Factory.NewWithValidTestData<JobDocAddress>();
			requestedBillToAddress.DocAddressType = DocAddressType.ClientRequestedBillingParty;
			requestedBillToAddress.E2_CompanyName = "BILL TO";

			Shipment.DocAddresses.Add(requestedBillToAddress);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Should return requested bill to address", requestedBillToAddress.E2_CompanyName, ShipmentWrapper.RequestedBillToAddress.CompanyName);
		}

		public void TestSupplier()
		{
			AssertNull(ShipmentWrapper.Supplier);

			var cNR = Factory.New<OrgHeader>();
			cNR.OH_FullName = "CNR ORG";
			var cNE = Factory.New<OrgHeader>();
			cNE.OH_FullName = "CNE ORG";
			Shipment.ConsignorPK = cNR.PK;
			AssertEquals("CNR ORG", ShipmentWrapper.Supplier.Name);
		}

		public void TestInterimReceiptConsignee()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsigneePK = consignee.PK;
			AssertEquals("Should Return the Consignee", consignee.OH_FullName, ShipmentWrapper.InterimReceiptConsignee.Name);
			AssertEquals("Should return type DocOrganisation", typeof(DocOrganisation), ShipmentWrapper.InterimReceiptConsignee.GetType());
		}
		public void TestInterimReceiptConsignor()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsignorPK = consignor.PK;
			AssertEquals("Should Return the Consignor", consignor.OH_FullName, ShipmentWrapper.InterimReceiptConsignor.Name);
			AssertEquals("Should return type DocOrganisation", typeof(DocOrganisation), ShipmentWrapper.InterimReceiptConsignor.GetType());
		}

		public void TestConsigneeOrg()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsigneePK = header.PK;
			AssertEquals("Should return Consignee", ShipmentWrapper.Consignee.Code, ShipmentWrapper.ConsigneeOrg.Code);
		}

		public void TestConsignorOrg()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsignorPK = header.PK;
			AssertEquals("Should return Consignor", ShipmentWrapper.Consignor.Code, ShipmentWrapper.ConsignorOrg.Code);
		}

		public void TestCustomsAgentStaffAssignmentName()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			var consol = shipment.Consols.AddNew();
			consol.SetDefaultReceivingForwarderAddress(Factory.LoadTop1<OrgHeader>(new ZQuery()));

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Sergey Gordok";

			OrgStaffAssignments staffAssignments = consol.ReceivingForwarder.StaffAssignments.AddNew();
			staffAssignments.O8_Role = StaffAssignmentRoles.Codes.CustomsAgent;
			staffAssignments.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			staffAssignments.O8_OH = consol.ReceivingForwarderPK;
			staffAssignments.O8_GS_NKPersonResponsible = staff.GS_Code;
			staffAssignments.O8_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AssertEquals("CustomsAgentStaffAssignmentName shoud be Sergey Gordok", "Sergey Gordok", shipmentWrapper.CustomsAgentStaffAssignmentName);
		}

		#endregion

		#region Notes Tests

		public void TestMarksAndNumbers()
		{
			AssertEquals("No Marks and numbers entered", "", ShipmentWrapper.MarksAndNumbers);

			Shipment.OuterPackLines.AddNew().JL_MarksAndNumbers = "PackMarks1";
			Shipment.OuterPackLines.AddNew().JL_MarksAndNumbers = "PackMarks2";
			AssertEquals("Get the marks and numbers from the packlines when not on the shipment", "PackMarks1\r\nPackMarks2", ShipmentWrapper.MarksAndNumbers);

			StmNote marksAndNumberNote1 = AddNotes(Shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Marks and numbers Line One\nLine Two");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.BookingNotes.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Get marks and numbers from the shipment when available.", "Marks and numbers Line One\nLine Two", ShipmentWrapper.MarksAndNumbers);
		}

		public void TestMarksAndNumbersArray()
		{
			ZString text = "Marks and numbers Line One Line Two with no new line characters";
			StmNote marksAndNumberNote1 = AddNotes(Shipment, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, text);
			AssertEquals("Marks and numbers Array Length", 1, ShipmentWrapper.MarksAndNumbersArray.Length);

			text = "Marks and numbers Line One\nLine Two with new line characters";
			marksAndNumberNote1.ST_NoteDataAsText = text;
			AssertEquals("Marks and numbers Array Length", 2, ShipmentWrapper.MarksAndNumbersArray.Length);

			text = "Marks and numbers Line One\nLine Two with\nno new line characters";
			marksAndNumberNote1.ST_NoteDataAsText = text;
			AssertEquals("Marks and numbers Array Length", 3, ShipmentWrapper.MarksAndNumbersArray.Length);
		}

		public void TestDetailedGoodsDescription()
		{
			StmNote detailedDescription = AddNotes(Shipment, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Detailed goods description\nLine Two");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Detailed goods description", "Detailed goods description\nLine Two", ShipmentWrapper.DetailedDescriptionOfGoods);
		}

		public void TestDetailedDescription()
		{
			ZString shortGoodsDesc = "GOODS DESCRIPTIONS.";
			ZString longGoodsDesc = "GOODS DESCRIPTIONS THAT EXCEEDS 35 CHARACTERS IN LENGTH.";

			AssertEquals("", ShipmentWrapper.DetailedDescription);

			Shipment.JS_GoodsDescription = shortGoodsDesc;
			AssertEquals("", ShipmentWrapper.DetailedDescription);

			Shipment.DetailedGoodsDescriptionNoteText = longGoodsDesc;
			AssertEquals(longGoodsDesc, ShipmentWrapper.DetailedDescription);
		}

		public void TestGoodsDescriptionArray()
		{
			StmNote detailedDescription = AddNotes(Shipment, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "GOODS DESCRIPTIONS THAT EXCEEDS 35 CHARACTERS IN LENGTH.");
			AssertEquals("Goods Description array length", 1, ShipmentWrapper.GoodsDescriptionArray.Length);

			detailedDescription.ST_NoteDataAsText = "GOODS DESCRIPTIONS THAT\nEXCEEDS 35 CHARACTERS IN LENGTH.";
			AssertEquals("Goods Description array length", 2, ShipmentWrapper.GoodsDescriptionArray.Length);

			detailedDescription.ST_NoteDataAsText = "GOODS DESCRIPTIONS THAT\nEXCEEDS 35 CHARACTERS\nIN LENGTH.";
			AssertEquals("Goods Description array length", 3, ShipmentWrapper.GoodsDescriptionArray.Length);
		}

		public void TestHandlingInstruction()
		{
			StmNote detailedDescription = AddNotes(Shipment, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling instruction\nLine Two");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Handling Instructions", "Handling instruction\nLine Two", ShipmentWrapper.HandlingInstructions);
		}

		public void TestFullHandlingInstructions_InspectionText()
		{
			AssertEquals("", ShipmentWrapper.FullHandlingInstructions);

			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "APPROVED");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NOT APPROVED");
			Shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("NOT APPROVED", ShipmentWrapper.FullHandlingInstructions);

			StmNote detailedDescription = AddNotes(Shipment, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling instruction\nLine Two");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Handling Instructions", "Handling instruction\nLine Two\nNOT APPROVED", ShipmentWrapper.FullHandlingInstructions);

			Shipment.JS_InspectionTypeCode = "APP";
			AssertEquals("Handling instruction\nLine Two\nAPPROVED", ShipmentWrapper.FullHandlingInstructions);

			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertEquals("Handling instruction\nLine Two", ShipmentWrapper.FullHandlingInstructions);
		}

		public void TestShipmentAndOrgHandlingInstructions()
		{
			StmNote shipmentNote = AddNotes(Shipment, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Shipment Handling Instruction");

			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeNote = orgConsignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_ParentID = orgConsignee.PK;
			consigneeNote.ST_Table = orgConsignee.TableName;
			consigneeNote.ST_NoteDataAsText = "OrgConsignee Note";

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorNote = orgConsignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNote.ST_ParentID = orgConsignor.PK;
			consignorNote.ST_Table = orgConsignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgConsignor Note";

			var orgConsigneeDelivery = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeDeliveryNote = orgConsigneeDelivery.Notes.AddNew();
			consigneeDeliveryNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeDeliveryNote.ST_ParentID = orgConsigneeDelivery.PK;
			consigneeDeliveryNote.ST_Table = orgConsigneeDelivery.TableName;
			consigneeDeliveryNote.ST_NoteDataAsText = "OrgConsigneeDelivery Note";

			var orgConsignorPickup = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupNote = orgConsignorPickup.Notes.AddNew();
			consignorPickupNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorPickupNote.ST_ParentID = orgConsignorPickup.PK;
			consignorPickupNote.ST_Table = orgConsignorPickup.TableName;
			consignorPickupNote.ST_NoteDataAsText = "OrgConsignorPickup Note";

			Shipment.ConsignorPK = orgConsignor.PK;
			Shipment.ConsignorPickupAddress.OrganisationPK = orgConsignorPickup.PK;
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsigneeDeliveryAddress.OrganisationPK = orgConsigneeDelivery.PK;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Handling Instructions", "Shipment Handling Instruction\nOrgConsignorPickup Note", ShipmentWrapper.ShipmentAndOrgHandlingInstructions);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Handling Instructions", "Shipment Handling Instruction\nOrgConsigneeDelivery Note", ShipmentWrapper.ShipmentAndOrgHandlingInstructions);
		}

		public void TestShipmentAndOrgHandlingInstructions_CheckForFreightMode()
		{
			StmNote shipmentNote = AddNotes(Shipment, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Shipment Handling Instruction");

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consigneeNote = consignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_ParentID = consignee.PK;
			consigneeNote.ST_Table = consignee.TableName;
			consigneeNote.ST_NoteDataAsText = "OrgConsignee Note";
			consigneeNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			consigneeNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.L);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorNote = consignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNote.ST_ParentID = consignor.PK;
			consignorNote.ST_Table = consignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgConsignor Note";
			consignorNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignorNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.L);

			OrgHeader orgConsigneeDelivery = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consigneeDeliveryNote = orgConsigneeDelivery.Notes.AddNew();
			consigneeDeliveryNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeDeliveryNote.ST_ParentID = orgConsigneeDelivery.PK;
			consigneeDeliveryNote.ST_Table = orgConsigneeDelivery.TableName;
			consigneeDeliveryNote.ST_NoteDataAsText = "OrgConsigneeDelivery Note";
			consigneeDeliveryNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			consigneeDeliveryNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.F);

			OrgHeader orgConsignorPickup = Factory.NewWithValidTestData<OrgHeader>();
			StmNote consignorPickupNote = orgConsignorPickup.Notes.AddNew();
			consignorPickupNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorPickupNote.ST_ParentID = orgConsignorPickup.PK;
			consignorPickupNote.ST_Table = orgConsignorPickup.TableName;
			consignorPickupNote.ST_NoteDataAsText = "OrgConsignorPickup Note";
			consignorPickupNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignorPickupNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.F);

			Shipment.ConsignorPK = consignor.PK;
			Shipment.ConsigneeDeliveryAddress.OrganisationPK = orgConsigneeDelivery.PK;
			Shipment.ConsigneePK = consignee.PK;
			Shipment.ConsignorPickupAddress.OrganisationPK = orgConsignorPickup.PK;
			Shipment.JS_TransportMode = Core.Constants.TransportCodes.Sea;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Handling Instructions", "Shipment Handling Instruction\nOrgConsignorPickup Note", ShipmentWrapper.ShipmentAndOrgHandlingInstructions);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Handling Instructions", "Shipment Handling Instruction\nOrgConsigneeDelivery Note", ShipmentWrapper.ShipmentAndOrgHandlingInstructions);
		}

		public void TestShipmentOrOrgHandlingInstructions()
		{
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeNote = orgConsignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_ParentID = orgConsignee.PK;
			consigneeNote.ST_Table = orgConsignee.TableName;
			consigneeNote.ST_NoteDataAsText = "OrgConsignee Note";

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorNote = orgConsignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNote.ST_ParentID = orgConsignor.PK;
			consignorNote.ST_Table = orgConsignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgConsignor Note";

			var orgConsigneeDelivery = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeDeliveryNote = orgConsigneeDelivery.Notes.AddNew();
			consigneeDeliveryNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeDeliveryNote.ST_ParentID = orgConsigneeDelivery.PK;
			consigneeDeliveryNote.ST_Table = orgConsigneeDelivery.TableName;
			consigneeDeliveryNote.ST_NoteDataAsText = "OrgConsigneeDelivery Note";

			var orgConsignorPickup = Factory.NewWithValidTestData<OrgHeader>();
			var consignorPickupNote = orgConsignorPickup.Notes.AddNew();
			consignorPickupNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorPickupNote.ST_ParentID = orgConsignorPickup.PK;
			consignorPickupNote.ST_Table = orgConsignorPickup.TableName;
			consignorPickupNote.ST_NoteDataAsText = "OrgConsignorPickup Note";

			Shipment.ConsignorPK = orgConsignor.PK;
			Shipment.ConsignorPickupAddress.OrganisationPK = orgConsignorPickup.PK;
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsigneeDeliveryAddress.OrganisationPK = orgConsigneeDelivery.PK;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Handling Instructions from organizations", "OrgConsignee Note\nOrgConsignor Note", ShipmentWrapper.ShipmentOrOrgHandlingInstructions);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Handling Instructions from organizations", "OrgConsignee Note\nOrgConsignor Note", ShipmentWrapper.ShipmentOrOrgHandlingInstructions);

			StmNote shipmentNote = AddNotes(Shipment, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Shipment Handling Instruction");
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Handling Instructions once note added to Shipment", "Shipment Handling Instruction", ShipmentWrapper.ShipmentOrOrgHandlingInstructions);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Handling Instructions once note added to Shipment", "Shipment Handling Instruction", ShipmentWrapper.ShipmentOrOrgHandlingInstructions);
		}

		public void TestCertificateOfOriginNote()
		{
			StmNote detailedDescription = AddNotes(Shipment, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "Certificate of Origin\nLine Two");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Certificate of Origin", "Certificate of Origin\nLine Two", ShipmentWrapper.CertificateOfOriginNote);
		}

		public void TestCartageInstructions()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			FreightHelperClass.AddNote(Shipment, pickupDesc, "Shipment Pickup Instructions");
			FreightHelperClass.AddNote(Shipment, deliveryDesc, "Shipment Delivery Instructions");

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Shipment Delivery Instructions", ShipmentWrapper.CartageInstructions);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Shipment Pickup Instructions", ShipmentWrapper.CartageInstructions);
		}

		public void TestShipmentAndOrgCartageInstruction()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgPickup = Factory.NewWithValidTestData<OrgHeader>();
			var orgDelivery = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsignorPK = orgConsignor.PK;
			Shipment.ConsignorPickupAddress.OrganisationPK = orgPickup.PK;
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsigneeDeliveryAddress.OrganisationPK = orgDelivery.PK;

			FreightHelperClass.AddNote(Shipment, pickupDesc, "Shipment Pickup Instructions");
			FreightHelperClass.AddNote(Shipment, deliveryDesc, "Shipment Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions");
			FreightHelperClass.AddNote(orgPickup, pickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgPickup, deliveryDesc, "Delivery Instructions");
			FreightHelperClass.AddNote(orgDelivery, pickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgDelivery, deliveryDesc, "Delivery Instructions");

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Cartage Instructions", "Shipment Pickup Instructions\nPickup Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Cartage Instructions", "Shipment Delivery Instructions\nDelivery Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);
		}

		public void TestShipmentOrOrgCartageInstruction()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgPickup = Factory.NewWithValidTestData<OrgHeader>();
			var orgDelivery = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsignorPK = orgConsignor.PK;
			Shipment.ConsignorPickupAddress.OrganisationPK = orgPickup.PK;
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsigneeDeliveryAddress.OrganisationPK = orgDelivery.PK;

			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions");
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions");
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions");
			FreightHelperClass.AddNote(orgPickup, pickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgPickup, deliveryDesc, "Delivery Instructions");
			FreightHelperClass.AddNote(orgDelivery, pickupDesc, "Pickup Instructions");
			FreightHelperClass.AddNote(orgDelivery, deliveryDesc, "Delivery Instructions");

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Cartage Instructions - should return instructions from organizations only", "Consignor Pickup Instructions\nConsignee Pickup Instructions", ShipmentWrapper.ShipmentOrOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Cartage Instructions - should return instructions from organizations only", "Consignor Delivery Instructions\nConsignee Delivery Instructions", ShipmentWrapper.ShipmentOrOrgCartageInstruction);

			FreightHelperClass.AddNote(Shipment, pickupDesc, "Shipment Pickup Instructions");
			FreightHelperClass.AddNote(Shipment, deliveryDesc, "Shipment Delivery Instructions");

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Cartage Instructions - now that notes have been entered on the shipment should return instructions from shipment only", "Shipment Pickup Instructions", ShipmentWrapper.ShipmentOrOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Cartage Instructions - now that notes have been entered on the shipment should return instructions from shipment only", "Shipment Delivery Instructions", ShipmentWrapper.ShipmentOrOrgCartageInstruction);
		}

		public void TestOutturnNotes()
		{
			StmNote orderRef = AddNotes(Shipment, PredefinedNoteTypes.Instance.OutturnNotes.Description, "Outturn Notes\nPilaged: 2\nDamaged: 4\n");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Wrong Outturn notes", "Outturn Notes\nPilaged: 2\nDamaged: 4", ShipmentWrapper.OutturnNotes);
		}

		public void TestPreAlertArrivalNoticeRemarks()
		{
			StmNote orderRef = AddNotes(Shipment, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "Pre Alert and Arrival Notice Remarks\nThis PreAlert goes to CNE\n");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Wrong notes", "Pre Alert and Arrival Notice Remarks\nThis PreAlert goes to CNE", ShipmentWrapper.PreAlertArrivalNoticeRemarks);
		}

		public void TestBookingNotes()
		{
			StmNote note = AddNotes(Shipment, PredefinedNoteTypes.Instance.BookingNotes.Description, "Booking Notes\nNote1\nNote2\nNote3.\n");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Wrong Booking notes", "Booking Notes\r\nNote1\r\nNote2\r\nNote3.", ShipmentWrapper.BookingNotes);
		}

		public void TestSpecialInstructions()
		{
			StmNote note = AddNotes(Shipment, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Special Instructions\nInstruction1\nInstruction2\nInstruction3.\n");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.BookingNotes.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Special Instructions", "Special Instructions\nInstruction1\nInstruction2\nInstruction3.", ShipmentWrapper.SpecialInstructions);
		}

		public void TestDeliveryRemarks()
		{
			StmNote note = AddNotes(Shipment, PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description, "Delivery Remarks\nRemark1\nRemark2\nRemark3.");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.BookingNotes.Description, "This is other notes and should not be included.");
			Factory.Save();

			AssertEquals("Delivery Remarks", "Delivery Remarks\nRemark1\nRemark2\nRemark3.", ShipmentWrapper.DeliveryRemarks);
		}

		public void TestDangerousGoodsHandlingInstruction()
		{
			AssertEquals("", ShipmentWrapper.DangerousGoodsHandlingInstruction);
			StmNote note = AddNotes(Shipment, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "contains very hazardous goods\nNext line\n");
			Factory.Save();

			AssertEquals("contains very hazardous goods\nNext line", ShipmentWrapper.DangerousGoodsHandlingInstruction);
		}
		#endregion

		#region CFS Labels Tests
		public void TestDestinationCode()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKDestination = "USLAX";
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("USLAX", shipmentWrapper.DestinationCode);
		}

		public void TestConsolReference()
		{
			Shipment.JS_ConsolReference = "C0001";
			AssertEquals("C0001", ShipmentWrapper.ConsolReference);
		}

		public void TestConNote()
		{
			AssertEquals("", ShipmentWrapper.ConNote);
			Shipment.JS_CartageWaybill = "Con note test";
			AssertEquals("Con note test", ShipmentWrapper.ConNote);
		}

		#endregion

		#region Labels Tests
		public void TestShipmentLabels()
		{
			AssertEquals("ShippingLabels w/ no packlines", 0, ShipmentWrapper.ShipmentLabels.Count);

			ShipmentWrapper.SetReportNameForTesting("IMPORT");
			var line = Shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line.JL_PackageCount = 10;
			AssertEquals("ShippingLabels w/ 1 packlines", 10, ShipmentWrapper.ShipmentLabels.Count);
			line.JL_Description = "Line";
			AssertEquals("ShippingLabels w/ 1 packlines Description", "Line", ShipmentWrapper.ShipmentLabels[0].PackLine.Description);
			AssertEquals("ShippingLabels w/ 1 packlines Number", 1, ShipmentWrapper.ShipmentLabels[0].Number);
			Shipment.JS_UniqueConsignRef = "S654321";
			AssertEquals("ShippingLabels w/ 1 packlines Barcode", "S654321", ShipmentWrapper.ShipmentLabels[0].PrimaryBarcodeDisplayText);

			var line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line2.JL_PackageCount = 20;
			line2.JL_Description = "Line2";
			AssertEquals("ShippingLabels w/ 2 packlines", 30, ShipmentWrapper.ShipmentLabels.Count);
		}

		public void TestShipmentLabels_NumberOfExportFreightLabelsToPrint()
		{
			AssertEquals("ShippingLabels w/ no packlines", 0, ShipmentWrapper.ShipmentLabels.Count);

			var line = Shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 10;
			AssertEquals("ShippingLabels w/ 1 packlines", 10, ShipmentWrapper.ShipmentLabels.Count);

			var line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 20;
			AssertEquals("ShippingLabels w/ 2 packlines", 30, ShipmentWrapper.ShipmentLabels.Count);

			DocumentShipment documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.FreightLabels);
			documentShipment.NumberOfLabelsToPrint = 17;
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("ShippingLabels w/ 2 packlines", 17, ShipmentWrapper.ShipmentLabels.Count);

			documentShipment.NumberOfLabelsToPrint = 35;
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("ShippingLabels w/ 2 packlines", 30, ShipmentWrapper.ShipmentLabels.Count);

			documentShipment.NumberOfLabelsToPrint = -1;
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("ShippingLabels w/ 2 packlines", 30, ShipmentWrapper.ShipmentLabels.Count);

			documentShipment.NumberOfLabelsToPrint = 0;
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("ShippingLabels w/ 2 packlines", 30, ShipmentWrapper.ShipmentLabels.Count);

			documentShipment.NumberOfLabelsToPrint = 5;
			ShipmentWrapper = DocShipment.New(documentShipment, Factory);
			AssertEquals("ShippingLabels w/ 2 packlines", 5, ShipmentWrapper.ShipmentLabels.Count);
		}

		#endregion

		#region Booking Test
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestBookingETD()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(ZString.Empty, ShipmentWrapper.BookingETD);

			CommonConsol consol = CreateExportConsol(Shipment);

			Transport transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today;
			AssertEquals(ZDateTime.Today.ToString("dd-MMM-yy"), ShipmentWrapper.BookingETD);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(ZDateTime.Today.ToLongTimeString(), ShipmentWrapper.BookingETD);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(ZDateTime.Today.ToLongTimeString(), ShipmentWrapper.BookingETD);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(ZDateTime.Today.ToLongTimeString(), ShipmentWrapper.BookingETD);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestBookingETA()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(ZString.Empty, ShipmentWrapper.BookingETA);

			CommonConsol consol = CreateExportConsol(Shipment);

			Transport transport = consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today;
			AssertEquals(ZDateTime.Today.ToString("dd-MMM-yy"), ShipmentWrapper.BookingETA);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(ZDateTime.Today.ToLongTimeString(), ShipmentWrapper.BookingETA);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(ZDateTime.Today.ToLongTimeString(), ShipmentWrapper.BookingETA);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(ZDateTime.Today.ToLongTimeString(), ShipmentWrapper.BookingETA);
		}
		public void TestIsBooking()
		{
			DocShipment shipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("N", shipmentWrapper.IsBooking.ToString());

			ForwardingShipment booking = Factory.New<ForwardingShipment>();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			shipmentWrapper = DocShipment.New(booking, Factory);
			AssertEquals("Y", shipmentWrapper.IsBooking.ToString());
		}

		public void TestIsBuyersConsolMaster()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("IsBuyersConsolMaster", false, ShipmentWrapper.IsBuyersConsolMaster);
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals("IsBuyersConsolMaster", true, ShipmentWrapper.IsBuyersConsolMaster);
		}

		public void TestCarrierFromConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_IsBooking = ZBool.False;
			AssertNull(shipmentWrapper.Carrier);

			CommonConsol consol = CreateExportConsol(shipment);
			var line = Factory.New<OrgHeader>();
			line.OH_Code = "TESTLINE";
			consol.SetDefaultShippingLineAddress(line);

			AssertEquals("TESTLINE", shipmentWrapper.Carrier.Code);
		}

		public void TestBookingPortOfLoadngConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_IsBooking = ZBool.False;
			AssertNull(shipmentWrapper.BookingPortOfLoading);

			CommonConsol consol = CreateExportConsol(shipment);
			consol.JK_RL_NKLoadPort = "AUBNE";

			AssertEquals("AUBNE", shipmentWrapper.BookingPortOfLoading.Code);
		}

		public void TestBookingPortOfDischargeConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_IsBooking = ZBool.False;
			AssertNull(shipmentWrapper.BookingPortOfDischarge);

			CommonConsol consol = CreateImportConsol(shipment);
			consol.JK_RL_NKDischargePort = "SGSIN";

			AssertEquals("SGSIN", shipmentWrapper.BookingPortOfDischarge.Code);
		}

		public void TestBookingExportReceivingDepotFromConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_IsBooking = ZBool.False;
			AssertNull(shipmentWrapper.ExportReceivingDepot);

			CommonConsol consol = CreateImportConsol(shipment);

			var cTO = Factory.New<OrgHeader>();
			var cTOAddress = cTO.Addresses.AddNew();
			cTOAddress.OA_Address1 = "CTOAddress";
			var depot = Factory.New<OrgHeader>();
			var depotAddress = depot.Addresses.AddNew();
			depotAddress.OA_Address1 = "DepotAddress";

			consol.JK_OA_DepartureCTOAddress = cTOAddress.PK;
			consol.JK_OA_PackDepotAddress = depotAddress.PK;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Should be Depot Address", "DepotAddress", shipmentWrapper.ExportReceivingDepot.Address1);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Should be CTO Address", "CTOAddress", shipmentWrapper.ExportReceivingDepot.Address1);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Should be Depot Address", "DepotAddress", shipmentWrapper.ExportReceivingDepot.Address1);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Should be CTO Address", "CTOAddress", shipmentWrapper.ExportReceivingDepot.Address1);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Should be Depot Address", "DepotAddress", shipmentWrapper.ExportReceivingDepot.Address1);
		}

		public void TestBookingTransportPlanning()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.AllowMultipleBusinessObjectsAroundOneRow = false;
			WriteSimpleContext(newFactory);

			JobVoyage voyage = newFactory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			newFactory.Save();
			DocTransportCollection collection;

			CommonShipment shipment1 = newFactory.New<CommonShipment>();
			shipment1.JS_IsBooking = true;
			shipment1.JS_JX = voyage.Sailings[0].PK;

			DocShipment shipment1Wrapper = DocShipment.New(shipment1, newFactory);
			collection = shipment1Wrapper.BookingTransportPlanning;

			AssertEquals("expecting the booking shipment to have 1 leg", 1, collection.Count);
			AssertEquals("expecting the details to come from the sailing", "AUBNE", collection[0].PortOfLoading.Code);
			AssertEquals("expecting the details to come from teh sailing", "SGSIN", collection[0].PortOfDischarge.Code);

			CommonShipment shipment2 = newFactory.New<ForwardingShipment>();
			shipment1.JS_JX = voyage.Sailings[0].PK;

			Transport transport1 = CreateShipmentTransport(shipment2, "SEA", Core.Constants.TransportPlanningType.PreCarriage,
					"NZAKL", "AUBNE", ZDateTime.Today, ZDateTime.Today.AddDays(1));

			Transport transport2 = CreateShipmentTransport(shipment2, "SEA", Core.Constants.TransportPlanningType.MainVessel,
					"SGSIN", "JPOSA", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6));

			CommonConsol consol = shipment2.Consols.AddNew();

			Transport transport3 = consol.Transports[0];
			transport3.JW_IsLinked = ZBool.False;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_Vessel = "Consol Vessel";
			transport3.JW_VoyageFlight = "Voyage NO";
			transport3.JW_RL_NKLoadPort = "AUBNE";
			transport3.JW_RL_NKDiscPort = "SGSIN";
			transport3.JW_ETD = ZDateTime.Today.AddDays(3);
			transport3.JW_ETA = ZDateTime.Today.AddDays(4);

			DocShipment shipment2Wrapper = DocShipment.New(shipment2, newFactory);
			collection = shipment2Wrapper.BookingTransportPlanning;

			AssertEquals(3, collection.Count);
			AssertEquals("First load", "NZAKL", collection[0].PortOfLoading.Code);
			AssertEquals("First discharge", "AUBNE", collection[0].PortOfDischarge.Code);
			AssertEquals("Second load", "AUBNE", collection[1].PortOfLoading.Code);
			AssertEquals("Second discharge", "SGSIN", collection[1].PortOfDischarge.Code);
			AssertEquals("Third load", "SGSIN", collection[2].PortOfLoading.Code);
			AssertEquals("Third discharge", "JPOSA", collection[2].PortOfDischarge.Code);
		}

		public void TestBookingTransportPlanningHeading()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("TRANSPORT PLANNING", shipmentWrapper.BookingTransportPlanningHeading);
		}

		public void TestBookingTransportPlanningWhenFlightDetailsSuppressed()
		{
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			DocTransportCollection coll = shipmentWrapper.BookingTransportPlanning;

			Transport tranship1 = CreateShipmentTransport(shipment, "AIR", Core.Constants.TransportPlanningType.PreCarriage,
					"AUMEL", "SGSIN", ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(6));

			Transport tranship2 = CreateShipmentTransport(shipment, "AIR", Core.Constants.TransportPlanningType.MainVessel,
					"SGSIN", "JPOSA", ZDateTime.Today.AddDays(7), ZDateTime.Today.AddDays(8));

			coll = shipmentWrapper.BookingTransportPlanning;
			AssertEquals("BookingTransportPlanning collection count", 2, coll.Count);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			coll = shipmentWrapper.BookingTransportPlanning;
			AssertEquals("BookingTransportPlanning collection should be suppressed", 0, coll.Count);
		}

		public void TestBookingTransportHeading()
		{
			AssertEquals("", ShipmentWrapper.BookingTransportHeading);
			CommonConsol consol = CreateImportConsol(Shipment);
			consol.JK_TransportMode = "SEA";

			AssertEquals("VESSEL / VOYAGE / IMO(Lloyds)", ShipmentWrapper.BookingTransportHeading);
		}

		public void TestBookingTransportConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			shipment.JS_IsBooking = ZBool.False;
			AssertEquals("", shipmentWrapper.BookingTransport);

			CommonConsol consol = CreateExportConsol(shipment);
			consol.JK_TransportMode = "SEA";
			Transport transport = consol.Transports[0];

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "111";

			AssertEquals(vessel.RV_Code + " / 111 / " + vessel.RV_LloydsNumber, shipmentWrapper.BookingTransport);
		}

		public void TestBookingDepartureReferenceFromConsol()
		{
			CommonConsol consol = CreateExportConsol(Shipment);
			CreateASailing();

			Transport transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("", ShipmentWrapper.BookingDepartureReference);

			BookingOrigin.JA_DepartReference = "DEP REF 123";
			BookingDestination.JB_ArrivalReference = "ARV REF 321";
			AssertEquals("DEP REF 123", ShipmentWrapper.BookingDepartureReference);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("DEP REF 123", ShipmentWrapper.BookingDepartureReference);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("", ShipmentWrapper.BookingDepartureReference);
		}

		public void TestMYBookingDepartureReference()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("MY");
				CommonConsol consol = Shipment.Consols.AddNew();
				CreateASailing();
				BookingOrigin.JA_RL_NKPortOfLoading = "MYKEL";

				consol.JK_RL_NKLoadPort = Sailing.JX_JA_RL_NKPortOfLoading;
				consol.JK_RL_NKDischargePort = Sailing.JX_JB_RL_NKPortOfDischarge;

				Transport transport = consol.Transports[0];
				transport.JW_JX = Sailing.PK;
				Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("", ShipmentWrapper.BookingDepartureReference);

				BookingOrigin.JA_DepartReference = "DEP REF 123";
				BookingDestination.JB_ArrivalReference = "ARV REF 321";
				BookingOrigin.JA_Berth = "DEPT BERTH";
				BookingDestination.JB_Berth = "ARV BERTH";

				AssertEquals("DEP REF 123 / DEPT BERTH", ShipmentWrapper.BookingDepartureReference);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestBookingDepartureReferenceHeading()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("DEPARTURE REFERENCE", shipmentWrapper.BookingDepartureReferenceHeading);

			CommonConsol consol = CreateImportConsol(shipment);

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertEquals("DEPARTURE REFERENCE", shipmentWrapper.BookingDepartureReferenceHeading);
		}

		public void TestBookingDepartureReferenceHeadingForMY()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("MY");

			CommonShipment shipment = CommonShipment.New(Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			CommonConsol consol = CreateImportConsol(shipment);

			consol.JK_RL_NKLoadPort = "MYKEL";
			AssertEquals("SCN / Berth", shipmentWrapper.BookingDepartureReferenceHeading);

			GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
		}

		public void TestDepotOrCTOHeading()
		{
			Shipment.JS_PackingMode = "FCL";
			AssertEquals("CTO", ShipmentWrapper.DepotOrCTOHeading);

			Shipment.JS_PackingMode = "LCL";
			AssertEquals("DEPOT", ShipmentWrapper.DepotOrCTOHeading);
		}

		public void TestContainerCountOnShipmentWithContainersNotAssigned()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var consol = Shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			AssertEquals("Should be empty string", "", ShipmentWrapper.ContainerCount);

			var container20PL = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20PL"));
			var container40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			container1.JC_RC = container20PL.PK;
			container2.JC_RC = container40GP.PK;

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();
			var line3 = Shipment.OuterPackLines.AddNew();
			line1.SetContainer(consol, null);
			line2.SetContainer(consol, null);
			line3.SetContainer(consol, null);

			ShipmentWrapper = GetNewShipmentWrapper();
			Assert("Should have 1 20PL container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("1 X 20PL"));
			Assert("Should have 1 40GP container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("1 X 40GP"));

			container1.JC_ContainerCount = 20;
			container2.JC_ContainerCount = 3;
			ShipmentWrapper = GetNewShipmentWrapper();
			Assert("Should have 20 20PL container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("20 X 20PL"));
			Assert("Should have 3 40GP container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("3 X 40GP"));

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals("Packlines not assigned to containers, container count should be empty", "", ShipmentWrapper.ContainerCount);
		}

		public void TestContainerCountOnShipmentWithAssignedContainers()
		{
			AssertEquals("", ShipmentWrapper.ContainerCount);

			var consol = Shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			AssertEquals("", ShipmentWrapper.ContainerCount);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			var container20PL = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20PL"));
			var container40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();
			var line3 = Shipment.OuterPackLines.AddNew();

			line1.SetContainer(container1.PK);
			line2.SetContainer(container1.PK);
			line3.SetContainer(container1.PK);

			container1.JC_RC = container20PL.PK;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			Assert("Should have only 1 20PL container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("1 X 20PL"));

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";
			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONT3";

			container2.JC_RC = container20PL.PK;
			line2.SetContainer(container2.PK);
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			Assert("Should have only 2 20PL container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("2 X 20PL"));

			container3.JC_RC = container40GP.PK;
			line3.SetContainer(container3.PK);
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			Assert("Should contain 2 20PL container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("2 X 20PL"));
			Assert("Should contain 1 40GP container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("1 X 40GP"));
		}

		public void TestContainerCountManyShipments()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var consol = Shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			var container20PL = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20PL"));
			var container40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));

			container1.JC_RC = container20PL.PK;
			container2.JC_RC = container20PL.PK;
			container3.JC_RC = container40GP.PK;
			container1.JC_ContainerCount = 3;
			container2.JC_ContainerCount = 4;
			container3.JC_ContainerCount = 5;

			//packlines not assigned, count container from container's count.
			Assert("Should contain 7 20PL container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("7 X 20PL"));
			Assert("Should contain 5 40GP container but was " + ShipmentWrapper.ContainerCount, ShipmentWrapper.ContainerCount.Contains("5 X 40GP"));

			consol.Shipments.AddNew();
			ShipmentWrapper = GetNewShipmentWrapper();
			AssertEquals("Packlines not assigned, consol contains more than 1 shipment, container count should be empty", "", ShipmentWrapper.ContainerCount);
		}

		public void TestIsBookingShipment()
		{
			DocShipment shipmentWrapper = DocShipment.New(Shipment, Factory);
			Shipment.JS_IsForwardRegistered = ZBool.False;
			Shipment.JS_IsBooking = ZBool.False;
			AssertEquals("N", shipmentWrapper.IsBookingShipment.ToString());

			Shipment.JS_IsForwardRegistered = ZBool.True;
			Shipment.JS_IsBooking = ZBool.False;
			AssertEquals("N", shipmentWrapper.IsBookingShipment.ToString());

			Shipment.JS_IsForwardRegistered = ZBool.False;
			Shipment.JS_IsBooking = ZBool.True;
			AssertEquals("Y", shipmentWrapper.IsBookingShipment.ToString());

			Shipment.JS_IsForwardRegistered = ZBool.True;
			Shipment.JS_IsBooking = ZBool.True;
			AssertEquals("N", shipmentWrapper.IsBookingShipment.ToString());
		}

		public void TestBookingCutOffDateOnConsol()
		{
			DocShipment shipmentWrapper = DocShipment.New(Shipment, Factory);
			Shipment.JS_IsForwardRegistered = ZBool.False;
			Shipment.JS_IsBooking = ZBool.False;
			AssertEquals(ZDateTime.Empty, shipmentWrapper.BookingCutOffDate);

			var consol = Shipment.Consols.AddNew();
			AssertEquals(ZDateTime.Empty, shipmentWrapper.BookingCutOffDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			Transport transport = consol.Transports[0];
			transport.JW_TerminalCutOff = ZDateTime.Today;
			transport.JW_DepotCutOff = ZDateTime.Today.AddDays(-2);
			AssertEquals(ZDateTime.Today.AddDays(-2), shipmentWrapper.BookingCutOffDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(ZDateTime.Today, shipmentWrapper.BookingCutOffDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(ZDateTime.Today.AddDays(-2), shipmentWrapper.BookingCutOffDate);
		}

		public void TestMarksAndNumbersHeading()
		{
			Env.Registry.MarksAndNumbersHeadingForFCL = "Heading For FCL";
			Env.Registry.MarksAndNumbersHeadingForNonFCL = "Heading For Non-FCL";
			Env.Registry.MarksAndNumbersHeadingForLSE = "Heading For LSE";
			Env.Registry.MarksAndNumbersHeadingForNonLSE = "Heading For Non-LSE";
			AssertEquals("MARKS AND NUMBERS", ShipmentWrapper.MarksAndNumbersHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("Heading For LSE", ShipmentWrapper.MarksAndNumbersHeading);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			AssertEquals("Heading For Non-LSE", ShipmentWrapper.MarksAndNumbersHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("Heading For LSE", ShipmentWrapper.MarksAndNumbersHeading);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			AssertEquals("Heading For Non-LSE", ShipmentWrapper.MarksAndNumbersHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("Heading For LSE", ShipmentWrapper.MarksAndNumbersHeading);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			AssertEquals("Heading For Non-LSE", ShipmentWrapper.MarksAndNumbersHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Heading For FCL", ShipmentWrapper.MarksAndNumbersHeading);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Heading For Non-FCL", ShipmentWrapper.MarksAndNumbersHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Heading For FCL", ShipmentWrapper.MarksAndNumbersHeading);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Heading For Non-FCL", ShipmentWrapper.MarksAndNumbersHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Heading For FCL", ShipmentWrapper.MarksAndNumbersHeading);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Heading For Non-FCL", ShipmentWrapper.MarksAndNumbersHeading);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Courier;
			AssertEquals("MARKS AND NUMBERS", ShipmentWrapper.MarksAndNumbersHeading);
		}

		public void TestShowMarksAndNumbersOnBookingConfirmation()
		{
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForLSE = false;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForNonLSE = false;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForLSE = true;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForNonLSE = true;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForLSE = false;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForNonLSE = false;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForFCL = false;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForNonFCL = false;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForFCL = true;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForNonFCL = true;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForFCL = false;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
			Env.Registry.ShowMarksAndNumbersForNonFCL = false;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", false, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Courier;
			AssertEquals("ShowMarksAndNumbersOnBookingConfirmation", true, ShipmentWrapper.ShowMarksAndNumbersOnBookingConfirmation);
		}

		#endregion

		#region IDoc Cartage Advice

		#region Headings

		public void TestJourneyOnePickUpHeading()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			//PrintTwoJourneys
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOnePickUpHeading: FCL Export", "PICKUP EMPTY", ShipmentWrapper.JourneyOnePickUpHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOnePickUpHeading: FCL Import", "PICKUP FULL", ShipmentWrapper.JourneyOnePickUpHeading);

			//! PrintTwoJourneys
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOnePickUpHeading: LCL export, No date", "PICKUP", ShipmentWrapper.JourneyOnePickUpHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOnePickUpHeading: LCL import, No date", "PICKUP", ShipmentWrapper.JourneyOnePickUpHeading);

			//! PrintTwoJourneys + Dates
			Shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now;
			Shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now.AddDays(1);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOnePickUpHeading: LCL Export + Date", "PICKUP DATE " + Shipment.DocsAndCartage.JP_PickupRequiredBy.ToLongTimeString(), ShipmentWrapper.JourneyOnePickUpHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOnePickUpHeading: LCL Import + Date", "PICKUP DATE " + Shipment.DocsAndCartage.JP_EstimatedDelivery.ToLongTimeString(), ShipmentWrapper.JourneyOnePickUpHeading);
		}

		public void TestJourneyOneDeliverToHeading()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			//PrintTwoJourneys
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOneDeliverToHeading: FCL Export", "DELIVER TO EMPTY", ShipmentWrapper.JourneyOneDeliverToHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOneDeliverToHeading: FCL import", "DELIVER TO FULL", ShipmentWrapper.JourneyOneDeliverToHeading);

			//! PrintTwoJourneys
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOneDeliverToHeading: LCL Export", "DELIVER TO", ShipmentWrapper.JourneyOneDeliverToHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOneDeliverToHeading: LCL Import", "DELIVER TO", ShipmentWrapper.JourneyOneDeliverToHeading);

			//! PrintTwoJourneys + Dates
			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_DepotCutOff = ZDateTime.Now;
			Shipment.DocsAndCartage.JP_DeliveryRequiredBy = ZDateTime.Now.AddDays(1);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOneDeliverToHeading: LCL Export + Date", "DELIVER TO DATE " + consol.JK_DepotCutOff.ToLongTimeString(), ShipmentWrapper.JourneyOneDeliverToHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOneDeliverToHeading: LCL Import + Date", "DELIVER TO DATE " + Shipment.DocsAndCartage.JP_DeliveryRequiredBy.ToLongTimeString(), ShipmentWrapper.JourneyOneDeliverToHeading);
		}

		public void TestJourneyTwoPickUpHeading()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			//PrintTwoJourneys
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoPickUpHeading: FCL Export", "PICKUP FULL", ShipmentWrapper.JourneyTwoPickUpHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoPickUpHeading: FCL Import", "PICKUP EMPTY", ShipmentWrapper.JourneyTwoPickUpHeading);

			//! PrintTwoJourneys
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoPickUpHeading: LCL Export", "PICKUP", ShipmentWrapper.JourneyTwoPickUpHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoPickUpHeading: LCL Import", "PICKUP", ShipmentWrapper.JourneyTwoPickUpHeading);
		}

		public void TestJourneyTwoDeliverToHeading()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			//PrintTwoJourneys
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoDeliverToHeading: FCL Export", "DELIVER TO FULL", ShipmentWrapper.JourneyTwoDeliverToHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoDeliverToHeading: FCL Import", "DELIVER TO EMPTY", ShipmentWrapper.JourneyTwoDeliverToHeading);

			//! PrintTwoJourneys
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoDeliverToHeading: LCL Export", "DELIVER TO", ShipmentWrapper.JourneyTwoDeliverToHeading);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoDeliverToHeading: LCL Import", "DELIVER TO", ShipmentWrapper.JourneyTwoDeliverToHeading);
		}
		#endregion

		public void TestEmailSubjectNumber()
		{
			Shipment.JS_UniqueConsignRef = "S12345";
			AssertEquals("EmailSubjectNumber", "S12345", ShipmentWrapper.EmailSubjectNumber);
		}

		public void TestReportNameWithoutCFS()
		{
			AssertEquals("ReportNameWithoutCFS", ZString.Empty, ShipmentWrapper.ReportNameWithoutCFS);

			ShipmentWrapper.SetReportNameForTesting("Report name");
			AssertEquals("ReportNameWithoutCFS", "Report name", ShipmentWrapper.ReportNameWithoutCFS);

			ShipmentWrapper.SetReportNameForTesting("Report name - CFS");
			AssertEquals("ReportNameWithoutCFS", "Report name", ShipmentWrapper.ReportNameWithoutCFS);
		}

		public void TestLCLAddressesWithWareHousing()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("AddressesWithWareHousing should be", 0, ShipmentWrapper.AddressesWithWareHousing.Count);

			var pickUpAddress = Factory.NewWithValidTestData<OrgAddress>();
			var deliverToAddress = Factory.NewWithValidTestData<OrgAddress>();
			Shipment.ConsignorPickupAddress.E2_OA_Address = pickUpAddress.PK;
			Shipment.JS_OA_ExportReceivingDepot = deliverToAddress.PK;
			AssertEquals("AddressesWithWareHousing should be", 0, ShipmentWrapper.AddressesWithWareHousing.Count);

			pickUpAddress.OA_ForkLift = ZBool.True;
			AssertEquals("AddressesWithWareHousing should have 1", 1, ShipmentWrapper.AddressesWithWareHousing.Count);

			deliverToAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("AddressesWithWareHousing should have 2", 2, ShipmentWrapper.AddressesWithWareHousing.Count);
			Hashtable hashtable = new Hashtable();

			hashtable.Add(pickUpAddress.PK, pickUpAddress);
			hashtable.Add(deliverToAddress.PK, deliverToAddress);
			foreach (DocDocAddress dAddress in ShipmentWrapper.AddressesWithWareHousing)
			{
				Assert("is in col", hashtable.ContainsKey(((JobDocAddress)dAddress.WrappedObject).Address.PK));
			}

			Shipment.ConsignorPickupAddress.E2_OA_Address = deliverToAddress.PK;
			AssertEquals("AddressesWithWareHousing both the same, should have 1, not 2", 1, ShipmentWrapper.AddressesWithWareHousing.Count);
		}

		public void TestFCLAddressesWithWareHousing()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			CommonConsol consol = CreateExportConsol(Shipment);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("AddressesWithWareHousing should be", 0, ShipmentWrapper.AddressesWithWareHousing.Count);

			var consignor = Factory.New<OrgHeader>();
			Shipment.ConsignorPK = consignor.PK;

			var j1PickUpAddress = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress j1J2ExporterAddress = consignor.Addresses.AddNew();
			var j2DeliverToAddress = Factory.NewWithValidTestData<OrgAddress>();

			consol.JK_OA_ContainerYardEmptyPickupAddress = j1PickUpAddress.PK;
			Shipment.ConsignorPickupAddress.E2_OA_Address = j1J2ExporterAddress.PK;
			consol.JK_OA_DepartureCTOAddress = j2DeliverToAddress.PK;

			AssertEquals("AddressesWithWareHousing should be", 0, ShipmentWrapper.AddressesWithWareHousing.Count);

			j1PickUpAddress.OA_ForkLift = ZBool.True;
			j1J2ExporterAddress.OA_DockLeveler = ZBool.True;
			j2DeliverToAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("AddressesWithWareHousing should have 3", 3, ShipmentWrapper.AddressesWithWareHousing.Count);

			Hashtable hashtable = new Hashtable();

			hashtable.Add(j1PickUpAddress.PK, j1PickUpAddress);
			hashtable.Add(j1J2ExporterAddress.PK, j1J2ExporterAddress);
			hashtable.Add(j2DeliverToAddress.PK, j2DeliverToAddress);
			foreach (DocDocAddress dAddress in ShipmentWrapper.AddressesWithWareHousing)
			{
				Assert("is in col", hashtable.ContainsKey(((JobDocAddress)dAddress.WrappedObject).Address.PK));
			}

			consol.JK_OA_DepartureCTOAddress = j1J2ExporterAddress.PK;
			AssertEquals("AddressesWithWareHousing has 2 the same, should have 2, not 3", 2, ShipmentWrapper.AddressesWithWareHousing.Count);
		}

		#region Addresses

		public void TestPickUpFromAddress()
		{
			AssertEquals("PickUpFromAddress", null, ShipmentWrapper.PickUpFromAddress);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("PickUpFromAddress", ShipmentWrapper.PickUpFromAddressForExport, ShipmentWrapper.PickUpFromAddress);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("PickUpFromAddress", ShipmentWrapper.PickUpFromAddressForImport, ShipmentWrapper.PickUpFromAddress);
		}

		public void TestPickUpFromAddressForExport()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsignorPK = header.PK;
			Shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("PickUpFromAddressForExport", DocOrganisation.New(header, Factory).PickUpAddress.PostalAddress, ShipmentWrapper.PickUpFromAddressForExport.PostalAddress);
			AssertEquals("PickUpFromAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.PickUpFromAddressForExport.GetType());

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsignorPickupAddress.E2_OA_Address = address.PK;
			AssertEquals("PickUpFromAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.PickUpFromAddressForExport.GetType());
			AssertEquals("PickUpFromAddressForExport", address.OA_Code, ShipmentWrapper.PickUpFromAddressForExport.Code);

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec.JE_JS = Shipment.PK;
			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.JE_MessageType = "EXP";

			var address2 = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, SQLComparisonOperator.NotEqual, address.OA_Code));
			dec.JE_OverrideFreightDefaults = true;
			dec.ClientPickupDeliveryAddressPK = address2.PK;
			Factory.Save();
			AssertEquals("PickUpFromAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.PickUpFromAddressForExport.GetType());
			AssertEquals("PickUpFromAddressForExport", address2.OA_Code, ShipmentWrapper.PickUpFromAddressForExport.Code);
		}

		public void TestPickUpFromAddressForImport()
		{
			var consol = Shipment.Consols.AddNew();
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_UnpackDepotAddress = address.PK;
			AssertEquals("PickUpFromAddressForImport", address.OA_Code, ShipmentWrapper.PickUpFromAddressForImport.Code);
			AssertEquals("PickUpFromAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.PickUpFromAddressForImport.GetType());

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec.JE_JS = Shipment.PK;
			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.JE_MessageType = "IMP";

			var address2 = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, SQLComparisonOperator.NotEqual, address.OA_Code));
			dec.WarehouseDocAddress.E2_OA_Address = address2.PK;

			Factory.Save();

			AssertEquals("PickUpFromAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.PickUpFromAddressForImport.GetType());
			AssertEquals("PickUpFromAddressForImport", address2.OA_Code, ShipmentWrapper.PickUpFromAddressForImport.Code);

			var address3 = Factory.NewWithValidTestData<OrgAddress>();
			Shipment.JS_OA_ImportReleaseDepot = address3.PK;

			AssertEquals("PickUpFromAddressForImport", address3.OA_Code, ShipmentWrapper.PickUpFromAddressForImport.Code);
		}

		public void TestDeliverToAddress()
		{
			AssertEquals("DeliverToAddress", null, ShipmentWrapper.DeliverToAddress);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("DeliverToAddress", ShipmentWrapper.DeliverToAddressForExport, ShipmentWrapper.DeliverToAddress);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("DeliverToAddress", ShipmentWrapper.DeliverToAddressForImport, ShipmentWrapper.DeliverToAddress);
		}

		public void TestDeliverToAddressForExport()
		{
			var consol = Shipment.Consols.AddNew();
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_PackDepotAddress = address.PK;
			AssertEquals("DeliverToAddressForExport", address.OA_Code, ShipmentWrapper.DeliverToAddressForExport.Code);
			AssertEquals("DeliverToAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.DeliverToAddressForExport.GetType());

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec.JE_JS = Shipment.PK;
			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.JE_MessageType = "EXP";

			var address2 = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, SQLComparisonOperator.NotEqual, address.OA_Code));
			dec.DepotDocAddress.E2_OA_Address = address2.PK;
			consol.JK_OA_PackDepotAddress = ZGuid.Empty;

			Factory.Save();

			AssertEquals("DeliverToAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.DeliverToAddressForExport.GetType());
			AssertEquals("DeliverToAddressForExport", address2.OA_Address1, ShipmentWrapper.DeliverToAddressForExport.Address1);

			var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();
			Shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;

			AssertEquals("DeliverToAddressForExport", exportReceivingDepot.OA_Address1, ShipmentWrapper.DeliverToAddressForExport.Address1);
		}

		public void TestDeliverToAddressForImport()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.OH_RL_NKClosestPort = "AUSYD";
			Shipment.ConsigneePK = header.PK;
			AssertEquals("DeliverToAddressForImport", DocOrganisation.New(header, Factory).DeliverAddress.PostalAddress, ShipmentWrapper.DeliverToAddressForImport.PostalAddress);
			AssertEquals("DeliverToAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.DeliverToAddressForImport.GetType());

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = address.PK;
			AssertEquals("DeliverToAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.DeliverToAddressForImport.GetType());
			AssertEquals("DeliverToAddressForImport", address.OA_Code, ShipmentWrapper.DeliverToAddressForImport.Code);

			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec.JE_JS = Shipment.PK;
			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.JE_MessageType = "IMP";
			AssertEquals("IMP", dec.JE_MessageType);

			var address2 = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, SQLComparisonOperator.NotEqual, address.OA_Code));
			dec.JE_OverrideFreightDefaults = true;
			dec.ClientPickupDeliveryAddressPK = address2.PK;
			Factory.Save();
			AssertEquals("DeliverToAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.DeliverToAddressForImport.GetType());
			AssertEquals("DeliverToAddressForImport", address2.OA_Code, ShipmentWrapper.DeliverToAddressForImport.Code);
		}

		#endregion

		#region Contacts
		public void TestPickUpFromContactDetails()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ContactName", "AAA", shipmentWrapperForCartageAdvice.PickUpFromContactName);
			AssertEquals("Phone", "111", shipmentWrapperForCartageAdvice.PickUpFromContactPhone);

			shipmentWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", shipmentWrapperForCartageAdvice.PickUpFromContactName);
			AssertEquals("Phone", "222", shipmentWrapperForCartageAdvice.PickUpFromContactPhone);

			shipmentWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.PickUpFromContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.PickUpFromContactPhone);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.PickUpFromContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.PickUpFromContactPhone);
		}

		public void TestDeliverToContactDetails()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ContactName", "AAA", shipmentWrapperForCartageAdvice.DeliverToContactName);
			AssertEquals("Phone", "111", shipmentWrapperForCartageAdvice.DeliverToContactPhone);

			shipmentWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", shipmentWrapperForCartageAdvice.DeliverToContactName);
			AssertEquals("Phone", "222", shipmentWrapperForCartageAdvice.DeliverToContactPhone);

			shipmentWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.DeliverToContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.DeliverToContactPhone);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.DeliverToContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.DeliverToContactPhone);
		}

		public void TestExportReceivingDepotContactDetails()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ContactName", "AAA", shipmentWrapperForCartageAdvice.ExportReceivingDepotContactName);
			AssertEquals("Phone", "111", shipmentWrapperForCartageAdvice.ExportReceivingDepotContactPhone);

			shipmentWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", shipmentWrapperForCartageAdvice.ExportReceivingDepotContactName);
			AssertEquals("Phone", "222", shipmentWrapperForCartageAdvice.ExportReceivingDepotContactPhone);

			shipmentWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.ExportReceivingDepotContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.ExportReceivingDepotContactPhone);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.ExportReceivingDepotContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.ExportReceivingDepotContactPhone);
		}

		#endregion

		#region Combined Cartage Advice

		#region Addresses

		public void TestJourneyOnePickUpAddress()
		{
			AssertNull("JourneyOnePickUpAddress", ShipmentWrapper.JourneyOnePickUpAddress);

			var consol = Shipment.Consols.AddNew();

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_ContainerYardEmptyPickupAddress = address.PK;

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertNotNull("JourneyOnePickUpAddress", ShipmentWrapper.JourneyOnePickUpAddress);
			AssertEquals("JourneyOnePickUpAddress is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyOnePickUpAddress.GetType());
			AssertEquals("JourneyOnePickUpAddress", address.OA_Code, ShipmentWrapper.JourneyOnePickUpAddress.Code);

			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, consol.JK_OA_ContainerYardEmptyPickupAddress));
			consol.JK_OA_ArrivalCTOAddress = address.PK;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertNotNull("JourneyOnePickUpAddress", ShipmentWrapper.JourneyOnePickUpAddress);
			AssertEquals("JourneyOnePickUpAddress is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyOnePickUpAddress.GetType());
			AssertEquals("JourneyOnePickUpAddress", address.OA_Code, ShipmentWrapper.JourneyOnePickUpAddress.Code);
		}

		public void TestJourneyOnePickUpAddressForBreakBulk()
		{
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			Shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			AssertNull("JourneyOnePickUpAddressForBreakBulk", ShipmentWrapper.JourneyOnePickUpAddress);
			var consol = Shipment.Consols.AddNew();
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_ArrivalCTOAddress = address.PK;
			AssertEquals("JourneyOnePickUpAddressForBreakBulk", address.OA_Code, ShipmentWrapper.JourneyOnePickUpAddress.Code);

			Shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			AssertEquals("JourneyOnePickUpAddressForBreakBulk", address.OA_Code, ShipmentWrapper.JourneyOnePickUpAddress.Code);

			Shipment.JS_PackingMode = Constants.ContainerModes.Liquid;
			AssertEquals("JourneyOnePickUpAddressForBreakBulk", address.OA_Code, ShipmentWrapper.JourneyOnePickUpAddress.Code);
		}

		public void TestJourneyOneDeliverToAddress()
		{
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOneDeliverToAddress", ShipmentWrapper.JourneyOneDeliverToAddressForExport, ShipmentWrapper.JourneyOneDeliverToAddress);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOneDeliverToAddress", ShipmentWrapper.JourneyOneDeliverToAddressForImport, ShipmentWrapper.JourneyOneDeliverToAddress);
		}

		public void TestJourneyOneDeliverToAddressForBreakBulk()
		{
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			Shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			AssertNull("JourneyOneDeliverToAddressForBreakBulk", ShipmentWrapper.JourneyOneDeliverToAddress);
			var consol = Shipment.Consols.AddNew();
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_DepartureCTOAddress = address.PK;
			AssertEquals("JourneyOneDeliverToAddressForBreakBulk", address.OA_Code, ShipmentWrapper.JourneyOneDeliverToAddress.Code);

			Shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			AssertEquals("JourneyOneDeliverToAddressForBreakBulk", address.OA_Code, ShipmentWrapper.JourneyOneDeliverToAddress.Code);

			Shipment.JS_PackingMode = Constants.ContainerModes.Liquid;
			AssertEquals("JourneyOneDeliverToAddressForBreakBulk", address.OA_Code, ShipmentWrapper.JourneyOneDeliverToAddress.Code);
		}

		public void TestJourneyOneDeliverToAddressForExport()
		{
			AssertNull("JourneyOneDeliverToAddressForExport", ShipmentWrapper.JourneyOneDeliverToAddressForExport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsignorPK = header.PK;
			AssertNotNull("JourneyOneDeliverToAddressForExport", ShipmentWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyOneDeliverToAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyOneDeliverToAddressForExport.GetType());
			AssertEquals("JourneyOneDeliverToAddressForExport", ShipmentWrapper.Consignor.PickUpAddress.PostalAddress, ShipmentWrapper.JourneyOneDeliverToAddressForExport.PostalAddress);

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsignorPickupAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyOneDeliverToAddressForExport", ShipmentWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyOneDeliverToAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyOneDeliverToAddressForExport.GetType());
			AssertEquals("JourneyOneDeliverToAddressForExport", DocAddress.New(address, Factory).PostalAddress, ShipmentWrapper.JourneyOneDeliverToAddressForExport.PostalAddress);
		}

		public void TestJourneyOneDeliverToAddressForImport()
		{
			AssertNull("JourneyOneDeliverToAddressForImport", ShipmentWrapper.JourneyOneDeliverToAddressForImport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsigneePK = header.PK;
			AssertNotNull("JourneyOneDeliverToAddressForImport", ShipmentWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyOneDeliverToAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyOneDeliverToAddressForImport.GetType());
			AssertEquals("JourneyOneDeliverToAddressForImport", ShipmentWrapper.Consignee.DeliverAddress.PostalAddress, ShipmentWrapper.JourneyOneDeliverToAddressForImport.PostalAddress);

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyOneDeliverToAddressForImport", ShipmentWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyOneDeliverToAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyOneDeliverToAddressForImport.GetType());
			AssertEquals("JourneyOneDeliverToAddressForImport", DocAddress.New(address, Factory).PostalAddress, ShipmentWrapper.JourneyOneDeliverToAddressForImport.PostalAddress);
		}

		public void TestJourneyTwoPickUpAddress()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoPickUpAddress", ShipmentWrapper.JourneyTwoPickUpAddressForExport, ShipmentWrapper.JourneyTwoPickUpAddress);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoPickUpAddress", ShipmentWrapper.JourneyTwoPickUpAddressForImport, ShipmentWrapper.JourneyTwoPickUpAddress);
		}

		public void TestJourneyTwoPickUpAddressForExport()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertNull("JourneyTwoPickUpAddressForExport", ShipmentWrapper.JourneyTwoPickUpAddressForExport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsignorPK = header.PK;
			AssertNotNull("JourneyTwoPickUpAddressForExport", ShipmentWrapper.JourneyTwoPickUpAddressForExport);
			AssertEquals("JourneyTwoPickUpAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyTwoPickUpAddressForExport.GetType());
			AssertEquals("JourneyTwoPickUpAddressForExport", ShipmentWrapper.Consignor.PickUpAddress.PostalAddress, ShipmentWrapper.JourneyTwoPickUpAddressForExport.PostalAddress);

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsignorPickupAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyTwoPickUpAddressForExport", ShipmentWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyTwoPickUpAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyTwoPickUpAddressForExport.GetType());
			AssertEquals("JourneyTwoPickUpAddressForExport", DocAddress.New(address, Factory).PostalAddress, ShipmentWrapper.JourneyTwoPickUpAddressForExport.PostalAddress);
		}

		public void TestJourneyTwoPickUpAddressForImport()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertNull("JourneyTwoPickUpAddressForImport", ShipmentWrapper.JourneyTwoPickUpAddressForImport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsigneePK = header.PK;
			AssertNotNull("JourneyTwoPickUpAddressForImport", ShipmentWrapper.JourneyTwoPickUpAddressForImport);
			AssertEquals("JourneyTwoPickUpAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyTwoPickUpAddressForImport.GetType());
			AssertEquals("JourneyTwoPickUpAddressForImport", ShipmentWrapper.Consignee.DeliverAddress.PostalAddress, ShipmentWrapper.JourneyTwoPickUpAddressForImport.PostalAddress);

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyTwoPickUpAddressForImport", ShipmentWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyTwoPickUpAddressForImport is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyTwoPickUpAddressForImport.GetType());
			AssertEquals("JourneyTwoPickUpAddressForImport", DocAddress.New(address, Factory).PostalAddress, ShipmentWrapper.JourneyTwoPickUpAddressForImport.PostalAddress);
		}

		public void TestJourneyTwoDeliverToAddress()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertNull("JourneyTwoDeliverToAddress", ShipmentWrapper.JourneyTwoDeliverToAddress);

			var consol = Shipment.Consols.AddNew();

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_DepartureCTOAddress = address.PK;

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertNotNull("JourneyTwoDeliverToAddress", ShipmentWrapper.JourneyTwoDeliverToAddress);
			AssertEquals("JourneyTwoDeliverToAddress is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyTwoDeliverToAddress.GetType());
			AssertEquals("JourneyTwoDeliverToAddress", address.OA_Code, ShipmentWrapper.JourneyTwoDeliverToAddress.Code);

			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, consol.JK_OA_ContainerYardEmptyPickupAddress));
			consol.JK_OA_ContainerYardEmptyReturnAddress = address.PK;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertNotNull("JourneyTwoDeliverToAddress", ShipmentWrapper.JourneyTwoDeliverToAddress);
			AssertEquals("JourneyTwoDeliverToAddress is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.JourneyTwoDeliverToAddress.GetType());
			AssertEquals("JourneyTwoDeliverToAddress", address.OA_Code, ShipmentWrapper.JourneyTwoDeliverToAddress.Code);
		}

		public void TestJourneyTwoDeliverToAddressWhenConsolIsNull()
		{
			var exportReceivingDepot = Factory.New<OrgAddress>();
			exportReceivingDepot.OA_Address1 = "Blah di blah";
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertNull("Precondition - consol is null", ShipmentWrapper.Consol);
			AssertEquals("When consol is null, JourneyTwoDeliverToAddress should fall back to shipment export receiving depot", exportReceivingDepot.OA_Address1, ShipmentWrapper.JourneyTwoDeliverToAddress.Address1);
		}

		#endregion

		#region Contacts

		public void TestJourneyOnePickUpContactDetails()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);

			AssertEquals("ContactName", "AAA", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactName);
			AssertEquals("Phone", "111", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactPhone);

			shipmentWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactName);
			AssertEquals("Phone", "222", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactPhone);

			shipmentWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.JourneyOnePickUpContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactDetails()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ContactName", "AAA", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			AssertEquals("Phone", "111", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);

			shipmentWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			AssertEquals("Phone", "222", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);

			shipmentWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);
		}

		public void TestJourneyTwoPickUpContactDetails()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ContactName", "AAA", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			AssertEquals("Phone", "111", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);

			shipmentWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			AssertEquals("Phone", "222", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);

			shipmentWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);

			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactDetails()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);

			AssertEquals("ContactName", "AAA", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
			AssertEquals("Phone", "111", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);

			shipmentWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
			AssertEquals("Phone", "222", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);

			shipmentWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
			AssertEquals("Phone", "333", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);
		}

		#region JourneyOne

		public void TestJourneyOnePickUpContactName()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);
			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)shipmentWrapperForCartageAdvice.JourneyOnePickUpAddress.WrappedObject;
			journeyOnePickupDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyOnePickUpSelectedContactName", "AAA", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactName);
			journeyOnePickupDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyOnePickUpSelectedContactName", "BBB", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactName);
		}

		public void TestJourneyOnePickUpContactPhone()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);
			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)shipmentWrapperForCartageAdvice.JourneyOnePickUpAddress.WrappedObject;
			journeyOnePickupDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyOnePickUpSelectedContactPhone", "111", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactPhone);
			journeyOnePickupDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyOnePickUpSelectedContactPhone", "222", shipmentWrapperForCartageAdvice.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactName()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);
			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)shipmentWrapperForCartageAdvice.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyOneDeliverToSelectedContactName", "AAA", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			journeyOneDeliverToDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyOneDeliverToSelectedContactName", "BBB", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactName);
		}

		public void TestJourneyOneDeliverToContactPhone()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);
			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)shipmentWrapperForCartageAdvice.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyOneDeliverToSelectedContactPhone", "111", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);
			journeyOneDeliverToDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyOneDeliverToSelectedContactPhone", "222", shipmentWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);
		}
		#endregion

		#region Journey Two

		public void TestJourneyTwoPickUpContactName()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);
			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)shipmentWrapperForCartageAdvice.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyTwoPickUpSelectedContactName", "AAA", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			journeyTwoPickupDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyTwoPickUpSelectedContactName", "BBB", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactName);
		}

		public void TestJourneyTwoPickUpContactPhone()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);
			shipmentWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)shipmentWrapperForCartageAdvice.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyTwoPickUpSelectedContactPhone", "111", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);
			journeyTwoPickupDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyTwoPickUpSelectedContactPhone", "222", shipmentWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactName()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)shipmentWrapperForCartageAdvice.JourneyTwoDeliverToAddress.WrappedObject;
			journeyTwoDeliverToDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyTwoDeliverToSelectedContactName", "AAA", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
			journeyTwoDeliverToDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyTwoDeliverToSelectedContactName", "BBB", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
		}

		public void TestJourneyTwoDeliverToContactPhone()
		{
			DocShipmentHelperClassForCartageAdvice shipmentWrapperForCartageAdvice = new DocShipmentHelperClassForCartageAdvice(Shipment, Factory);
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)shipmentWrapperForCartageAdvice.JourneyTwoDeliverToAddress.WrappedObject;
			journeyTwoDeliverToDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyTwoDeliverToSelectedContactPhone", "111", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);
			journeyTwoDeliverToDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyTwoDeliverToSelectedContactPhone", "222", shipmentWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);
		}

		#endregion

		#endregion

		#endregion

		public void TestIsEmptyLeg()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Journey One, Two Journeys, Export: Should be True", true, ShipmentWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Two Journeys, Export: Should be False", false, ShipmentWrapper.IsEmptyLeg(false));

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Journey One, Two Journeys, Import: Should be False", false, ShipmentWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Two Journeys, Import: Should be True", true, ShipmentWrapper.IsEmptyLeg(false));

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Journey One, One Journey, Import: Should be False", false, ShipmentWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, ShipmentWrapper.IsEmptyLeg(false));

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Journey One, One Journey, Import: Should be False", false, ShipmentWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, ShipmentWrapper.IsEmptyLeg(false));
		}

		public void TestIsFullLeg()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Journey One, Two Journeys, Export: Should be False", false, ShipmentWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Two Journeys, Export: Should be True", true, ShipmentWrapper.IsFullLeg(false));

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Journey One, Two Journeys, Import: Should be True", true, ShipmentWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Two Journeys, Import: Should be False", false, ShipmentWrapper.IsFullLeg(false));

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Journey One, One Journey, Import: Should be False", false, ShipmentWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, ShipmentWrapper.IsFullLeg(false));

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Journey One, One Journey, Import: Should be False", false, ShipmentWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, ShipmentWrapper.IsFullLeg(false));
		}

		#endregion

		#region IDocManagerBarcode

		public void TestBarcodeTextForFontWithData()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("mising data in shipment, BarcodeTextForFont property is empty", ZString.Empty, shipmentWrapper.BarcodeTextForFont);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("Still mising data in shipment, BarcodeTextForFont property is empty", ZString.Empty, shipmentWrapper.BarcodeTextForFont);

			shipment.JS_HouseBill = "12345678901234567890";
			shipmentWrapper = DocShipment.New(shipment, Factory);
			((IDocTypeCode)shipmentWrapper).DocTypeCode = "HBL";

			Assert("all data now valid, property shouldn't be empty", !shipmentWrapper.BarcodeTextForFont.IsEmpty);
			// not testing the font code - already tested in TextBarcode class and will change if we change the font
		}

		public void TestBarcodeTextWithData()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("precondition: Origin", "", shipment.JS_RL_NKOrigin);
			AssertEquals("precondition: Destination", "", shipment.JS_RL_NKDestination);
			AssertEquals("precondition: Housebill", "", shipment.JS_HouseBill);

			AssertEquals("mising data in shipment, BarcodeTextForFont property is empty", ZString.Empty, shipmentWrapper.BarcodeText);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipmentWrapper = DocShipment.New(shipment, Factory);

			AssertEquals("Still mising data in shipment, BarcodeTextForFont property is empty", ZString.Empty, shipmentWrapper.BarcodeText);

			shipment.JS_HouseBill = "12345678901234567890";
			shipmentWrapper = DocShipment.New(shipment, Factory);
			((IDocTypeCode)shipmentWrapper).DocTypeCode = "HBL";

			AssertEquals("all data now valid, barcode should exist", "[EDIHBLSYDLAX12345678901234567890]", shipmentWrapper.BarcodeText);
		}

		public void TestBarcodeTextWhenOriginOrDestinationDontHaveIATACodes()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "USWZV"; // no IATA code
			shipment.JS_RL_NKDestination = "USOGI"; // no IATA code
			shipment.JS_HouseBill = "12345";

			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			((IDocTypeCode)shipmentWrapper).DocTypeCode = "HBL";

			AssertEquals("Should generate a barcode based on the last 3 letters of UNLOCO codes", "[EDIHBLWZVOGI12345]", shipmentWrapper.BarcodeText);
		}

		public void TestBarcodeTextForFontPlaceholderWithData()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S12345678";
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			Factory.Save();

			Assert("BarcodeTextForFont placeholder should never be empty - a shipment will always have a UniqueConsignRef", !shipmentWrapper.BarcodeTextForFontPlaceholder.IsEmpty);
			// not testing the font code - already tested in TextBarcode class and will change if we change the font
		}

		public void TestBarcodeTextPlaceholderWithData()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S12345678";
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			Factory.Save();

			AssertEquals("^SHP=S12345678|", shipmentWrapper.BarcodeTextPlaceholder);

			shipment.JS_UniqueConsignRef = "S10101010";
			shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("^SHP=S10101010|", shipmentWrapper.BarcodeTextPlaceholder);
		}

		#endregion

		#region Total PackLine Outturned Weight Volume
		public void TestTotalWeightOutturned()
		{
			AssertEquals("OutTurned weight should be '0.00'", 0.00m, ShipmentWrapper.TotalWeightOutturned);

			PackLine testPackLineOne = Shipment.OuterPackLines.AddNew();
			PackLine testPackLineTwo = Shipment.OuterPackLines.AddNew();
			PackLine testPackLineThree = Shipment.OuterPackLines.AddNew();

			testPackLineOne.JL_OutturnedWeight = 101.01m;
			testPackLineTwo.JL_OutturnedWeight = 20.05m;
			testPackLineThree.JL_OutturnedWeight = 30.50m;

			AssertEquals("OutTurned weight should be '151.56'", 151.56m, ShipmentWrapper.TotalWeightOutturned);
		}

		public void TestTotalVolumeOutturned()
		{
			AssertEquals("OutTurned Volume should be '0.00'", 0.00m, ShipmentWrapper.TotalVolumeOutturned);

			PackLine testPackLineOne = Shipment.OuterPackLines.AddNew();
			PackLine testPackLineTwo = Shipment.OuterPackLines.AddNew();
			PackLine testPackLineThree = Shipment.OuterPackLines.AddNew();

			testPackLineOne.JL_OutturnedVolume = 10.01m;
			testPackLineTwo.JL_OutturnedVolume = 20.05m;
			testPackLineThree.JL_OutturnedVolume = 30.50m;

			AssertEquals("OutTurned Volume should be '60.56'", 60.56m, ShipmentWrapper.TotalVolumeOutturned);
		}

		public void TestTotalOuterPacksOutturnedIsNotLanguageDependent()
		{
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.French))
			{
				var shipment = Factory.New<CommonShipment>();

				var packline01 = shipment.OuterPackLines.AddNew();
				var packline02 = shipment.OuterPackLines.AddNew();
				var packline03 = shipment.OuterPackLines.AddNew();

				packline01.JL_Outturn = 10;
				packline02.JL_Outturn = 12;
				packline03.JL_Outturn = 6;

				var shipmentDoc = DocShipment.New(shipment, Factory);

				AssertEquals(28, shipmentDoc.TotalOuterPacksOutturned);
			}
		}

		#endregion

		#region Forwarding Instruction

		public void TestShippersReference()
		{
			Shipment.JS_UniqueConsignRef = "";
			Shipment.JS_BookingReference = "";
			AssertEquals("ShippersReference", ZString.Empty, ShipmentWrapper.ShippersReference);

			Shipment.JS_UniqueConsignRef = "S00001111";
			AssertEquals("ShippersReference", "S00001111", ShipmentWrapper.ShippersReference);

			Shipment.JS_BookingReference = "99999999";
			AssertEquals("ShippersReference", "99999999", ShipmentWrapper.ShippersReference);
		}

		public void TestExportPermit()
		{
			Shipment.CustomsEntryNumberType = "";
			Shipment.CustomsEntryNumber = "";
			AssertEquals("ExportPermits", "", ShipmentWrapper.ExportPermit);

			Shipment.CustomsEntryNumberType = "XXX";
			AssertEquals("ExportPermits", "", ShipmentWrapper.ExportPermit);

			Shipment.CustomsEntryNumber = "99999999";
			AssertEquals("ExportPermits", "XXX 99999999", ShipmentWrapper.ExportPermit);

			Shipment.CustomsEntryNumber = "";
			Shipment.CustomsEntryNumberType = "EX1";

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
			{
				AssertEquals("Entry Number Type is exempt so return it even though there is no number", "EX1", ShipmentWrapper.ExportPermit);
			}
			else
			{
				AssertEquals("Exemption codes only apply to AU exports", "", ShipmentWrapper.ExportPermit);
			}
		}

		public void TestFreightTerms()
		{
			Shipment.JS_INCO = "";
			AssertEquals("FreightTerms", ZString.Empty, ShipmentWrapper.FreightTerms);

			Shipment.JS_INCO = "XXX";
			AssertEquals("FreightTerms", "XXX ()", ShipmentWrapper.FreightTerms);

			Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			ZString expected = Core.Constants.IncoTerms.CostAndFreight + " (" + Shipment.Lookups.JS_INCO_List.GetDescriptionFromCode(Core.Constants.IncoTerms.CostAndFreight) + ") - Freight Prepaid";
			AssertEquals("FreightTerms", expected, ShipmentWrapper.FreightTerms);

			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			expected = Core.Constants.IncoTerms.FreeOnBoard + " (" + Shipment.Lookups.JS_INCO_List.GetDescriptionFromCode(Core.Constants.IncoTerms.FreeOnBoard) + ") - Freight Collect";
			AssertEquals("FreightTerms", expected, ShipmentWrapper.FreightTerms);
		}

		public void TestAdditionalTerms()
		{
			AssertEquals("Default", "", ShipmentWrapper.AdditionalTerms);

			Shipment.JS_AdditionalTerms = "Hello";
			AssertEquals("AdditionalTerms", "Hello", ShipmentWrapper.AdditionalTerms);

			Shipment.JS_AdditionalTerms = "Mamaliga cu brinza";
			AssertEquals("AdditionalTerms", "Mamaliga cu brinza", ShipmentWrapper.AdditionalTerms);
		}

		public void TestMakeBillOutTo()
		{
			AssertEquals("MakeBillOutTo", "Shipper", ShipmentWrapper.MakeBillOutTo);
		}

		public void TestGoodsSummary()
		{
			ZString expected = "Total Package(s): 0";
			AssertEquals("GoodsSummary", expected, ShipmentWrapper.GoodsSummary);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("GoodsSummary", ZString.Empty, ShipmentWrapper.GoodsSummary);

			CommonConsol consol = Shipment.Consols.AddNew();
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			container2.JC_ContainerNum = "2";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerMode = "";
			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			container1.JC_RC = refContainer1.PK;
			container2.JC_RC = refContainer2.PK;

			refContainer1.RC_Code = "CN1";
			refContainer2.RC_Code = "CN2";

			Shipment.JS_OuterPacks = 50;
			PackLine line1 = Shipment.OuterPackLines.AddNew();
			PackLine line2 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line2.JL_PackageCount = 20;

			container1.PackLines.Add(line1);
			container2.PackLines.RemoveAll();
			Factory.Save();

			ShipmentWrapper.ContainerPackageCountCollection.Reload();
			expected = "1 x " + refContainer1.RC_Code + " Container STC 10 Package(s); 40 LCL Package(s)";
			AssertEquals("GoodsSummary", expected, ShipmentWrapper.GoodsSummary);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Groupage;
			Factory.Save();
			AssertEquals("GoodsSummary", expected, ShipmentWrapper.GoodsSummary);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			Factory.Save();
			AssertEquals("GoodsSummary", expected, ShipmentWrapper.GoodsSummary);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();
			expected = "Total Package(s): 50";
			AssertEquals("GoodsSummary", expected, ShipmentWrapper.GoodsSummary);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.PackLines.Add(line2);
			Factory.Save();

			ShipmentWrapper.ContainerPackageCountCollection.Reload();
			expected = "1 x " + refContainer1.RC_Code + " Container STC 10 Package(s); 1 x " + refContainer2.RC_Code + " Container STC 20 Package(s); 20 LCL Package(s)";
			AssertEquals("GoodsSummary", expected, ShipmentWrapper.GoodsSummary);

			Shipment.JS_RL_NKDestination = "USLAX";
			expected = expected.Replace("STC ", "");
			AssertEquals("GoodsSummary when Shipment is bound for US should not show STC", expected, ShipmentWrapper.GoodsSummary);
		}

		public void TestContainerPackageCountCollectionReturnsCorrectPackages()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			CommonConsol consol = Shipment.Consols.AddNew();

			CommonContainer container = consol.Containers.AddNew();
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery());
			container.JC_RC = refContainer.PK;

			PackLine line1 = Shipment.OuterPackLines.AddNew();
			PackLine line2 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10; //
			line2.JL_PackageCount = 10; // do NOT change these two values, this test ensures the result when the JL_PackageCount is not "distinct"

			container.PackLines.Add(line1);
			container.PackLines.Add(line2);

			Factory.Save();

			AssertEquals(1, ShipmentWrapper.ContainerPackageCountCollection.Count);
			AssertEquals("The ContainerPackageCountCollection should have returned 20 as the total Packages count.", 20, ShipmentWrapper.ContainerPackageCountCollection[0]["Packages"]);
		}

		public void TestLocationOfGoods()
		{
			AssertNull("Consignor", ShipmentWrapper.Consignor);
			AssertEquals("LocationOfGoods", ZString.Empty, ShipmentWrapper.LocationOfGoods);

			var testConsignor = Factory.New<OrgHeader>();
			testConsignor.OH_FullName = "Test Consignor";
			testConsignor.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			Shipment.ConsignorPK = testConsignor.PK;
			AssertNotNull("Consignor", ShipmentWrapper.Consignor);
			AssertEquals("LocationOfGoods", "TEST CONSIGNOR", ShipmentWrapper.LocationOfGoods);

			var address = Factory.New<OrgAddress>();
			address.AddressCapability.SetCapabilityEnabled("PIC");
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_RN_NKCountryCode = ZString.Empty;
			testConsignor.Addresses.Add(address);
			Shipment.ConsignorPickupAddress.E2_OA_Address = address.PK;
			AssertEquals("LocationOfGoods", "TEST CONSIGNOR\nADDRESS 1\nADDRESS 2", ShipmentWrapper.LocationOfGoods);

			Shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			Shipment.ConsignorPickupAddress.E2_Address1 = "26 Myrtle Street";
			Shipment.ConsignorPickupAddress.E2_Address2 = "Prospect";
			Shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = ZString.Empty;
			AssertEquals("LocationOfGoods", "TEST CONSIGNOR\n26 MYRTLE STREET\nPROSPECT", ShipmentWrapper.LocationOfGoods);
		}

		public void TestLocationOfGoodsExcludeName()
		{
			AssertNull("Consignor", ShipmentWrapper.Consignor);
			AssertEquals("LocationOfGoodsExcludeName", ZString.Empty, ShipmentWrapper.LocationOfGoodsExcludeName);

			var testConsignor = Factory.New<OrgHeader>();
			testConsignor.OH_FullName = "Test Consignor";
			testConsignor.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			Shipment.ConsignorPK = testConsignor.PK;
			AssertNotNull("Consignor", ShipmentWrapper.Consignor);
			AssertEquals("LocationOfGoodsExcludeName", ZString.Empty, ShipmentWrapper.LocationOfGoodsExcludeName);

			var address = Factory.New<OrgAddress>();
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_RN_NKCountryCode = ZString.Empty;
			testConsignor.Addresses.Add(address);
			AssertEquals("LocationOfGoodsExcludeName", "ADDRESS 1\nADDRESS 2", ShipmentWrapper.LocationOfGoodsExcludeName);
		}

		public void TestGoodsReceivalPoint()
		{
			AssertNull("Consol", ShipmentWrapper.Consol);
			AssertEquals("GoodsReceivalPoint", ZString.Empty, ShipmentWrapper.GoodsReceivalPoint);

			CommonConsol consol = Shipment.Consols.AddNew();
			AssertNotNull("Consol", ShipmentWrapper.Consol);
			AssertEquals("GoodsReceivalPoint", ZString.Empty, ShipmentWrapper.GoodsReceivalPoint);

			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("GoodsReceivalPoint", ZString.Empty, ShipmentWrapper.GoodsReceivalPoint);

			var testDepartureCTO = Factory.New<OrgHeader>();
			testDepartureCTO.OH_FullName = "Test Departure CTO";
			testDepartureCTO.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			testDepartureCTO.MainAddress.OA_Address1 = "Departure CTO Address 1";
			testDepartureCTO.MainAddress.OA_Address2 = "Departure CTO Address 2";
			consol.JK_OA_DepartureCTOAddress = testDepartureCTO.Addresses[0].PK;
			AssertEquals("GoodsReceivalPoint", "TEST DEPARTURE CTO\nDEPARTURE CTO ADDRESS 1\nDEPARTURE CTO ADDRESS 2", ShipmentWrapper.GoodsReceivalPoint);

			Shipment.JS_PackingMode = "";
			AssertEquals("GoodsReceivalPoint", ZString.Empty, ShipmentWrapper.GoodsReceivalPoint);

			var testPackDepot = Factory.New<OrgHeader>();
			testPackDepot.OH_FullName = "Test Pack Depot";
			testPackDepot.MainAddress.OA_Address1 = "Pack Depot Address 1";
			testPackDepot.MainAddress.OA_Address2 = "Pack Depot Address 2";
			consol.JK_OA_PackDepotAddress = testPackDepot.Addresses[0].PK;
			AssertEquals("GoodsReceivalPoint", "TEST PACK DEPOT\nPACK DEPOT ADDRESS 1\nPACK DEPOT ADDRESS 2\n" + Env.CurrentCompany.Country.Description.ToUpper(), ShipmentWrapper.GoodsReceivalPoint);

			var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();
			Shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
			AssertEquals("GoodsReceivalPoint", DocAddress.New(exportReceivingDepot, Factory).PostalAddress, ShipmentWrapper.GoodsReceivalPoint);
		}

		public void TestRegistrationNumber()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Registration Number", ZString.Empty, shipmentWrapper.RegistrationNumber);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.ConsigneePK = header.PK;
			AssertEquals("Registration Number", ZString.Empty, shipmentWrapper.RegistrationNumber);

			RefCountry brazil = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);
			OrgCusCode code1 = header.CustomsCodes.AddNew();
			code1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			code1.OK_RN_NKCodeCountry = brazil.Code;
			code1.OK_CustomsRegNo = "REG1111";

			OrgCusCode code2 = header.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.CodeTypes.RebateUserCode;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code2.OK_CustomsRegNo = "REG2222";
			AssertEquals("Registration Number", ZString.Empty, shipmentWrapper.RegistrationNumber);

			header.OH_RL_NKClosestPort = "BRAAG";
			AssertEquals("Registration Number", "CNPJ: REG1111", shipmentWrapper.RegistrationNumber);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "";
			AssertEquals("Registration Number", ZString.Empty, shipmentWrapper.RegistrationNumber);
		}

		public void TestInspectionType()
		{
			AssertEquals("Default", "UNK", ShipmentWrapper.InspectionType);

			Shipment.JS_InspectionTypeCode = "ABC";
			AssertEquals("InspectionType", "ABC", ShipmentWrapper.InspectionType);

			Shipment.JS_InspectionTypeCode = "XYZ";
			AssertEquals("InspectionType", "XYZ", ShipmentWrapper.InspectionType);
		}

		#endregion

		#region FCR
		public void TestFCRClause()
		{
			Assert("FCR Clause default should not be empty", !ShipmentWrapper.FCRClause.IsEmpty);
			DocumentsDataRegistry.Instance.ForwardersCertificateOfReceiptFCRClause.SetValue(
					GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test FCR Clause");
			AssertEquals("FCR Clause taken from registry", "Test FCR Clause", ShipmentWrapper.FCRClause);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFCRLogo()
		{
			AssertNull("FCR Logo null by default", ShipmentWrapper.FCRLogo);

			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFile = resourceRetriever.GetStream("DocumentWrappers.DraftStatusImage.gif");
				System.Drawing.Image testImage = System.Drawing.Image.FromStream(testFile);
				try
				{
					DocumentsDataRegistry.Instance.ForwardersCertificateOfReceiptLogo.SetValue(
							GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testImage);
					AssertNotNull("FCR Logo taken from registry should not be null", ShipmentWrapper.FCRLogo);
				}
				finally
				{
					DocumentsDataRegistry.Instance.ForwardersCertificateOfReceiptLogo.SetValue(
							GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
				}
			}
		}
		#endregion

		#region Letter of Guarantee
		public void TestGuaranteeDeclarationText()
		{
			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("SG");
			try
			{
				Assert("Guarantee Declaration Text not empty by default", !ShipmentWrapper.GuaranteeDeclarationText.IsEmpty);
				DocumentsDataRegistry.Instance.GuaranteeDeclarationText.SetValue(
						GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test declaration text");
				AssertEquals("Guarantee Declaration Text from registry", "Test declaration text", ShipmentWrapper.GuaranteeDeclarationText);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountry);
			}
		}
		#endregion

		#region IPreAlert Fields

		public virtual void TestPreAlertReference()
		{
			AssertEquals("", ShipmentWrapper.PreAlertReferenceHeading);
			AssertEquals("", ShipmentWrapper.PreAlertReference);
		}

		public void TestCompleteRouting()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.AllowMultipleBusinessObjectsAroundOneRow = false;

			JobVoyage voyage = factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			factory.Save();
			DocTransportCollection collection;

			CommonShipment shipment1 = factory.New<CommonShipment>();
			shipment1.JS_JX = voyage.Sailings[0].PK;

			DocShipment shipment1Wrapper = DocShipment.New(shipment1, factory);
			collection = shipment1Wrapper.CompleteRouting;

			AssertEquals("expecting the booking shipment to have 1 leg", 1, collection.Count);
			AssertEquals("expecting the details to come from the sailing", "AUBNE", collection[0].PortOfLoading.Code);
			AssertEquals("expecting the details to come from teh sailing", "SGSIN", collection[0].PortOfDischarge.Code);

			CommonShipment shipment2 = factory.New<ForwardingShipment>();
			shipment1.JS_JX = voyage.Sailings[0].PK;

			Transport transport2 = CreateShipmentTransport(shipment2, "SEA", Core.Constants.TransportPlanningType.MainVessel,
					"SGSIN", "JPOSA", ZDateTime.Empty, ZDateTime.Today.AddDays(6));

			Transport transport1 = CreateShipmentTransport(shipment2, "SEA", Core.Constants.TransportPlanningType.PreCarriage,
					"NZAKL", "AUBNE", ZDateTime.Today, ZDateTime.Empty);

			CommonConsol consol = shipment2.Consols.AddNew();

			Transport transport3 = consol.Transports[0];
			transport3.JW_IsLinked = ZBool.False;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_Vessel = "Consol Vessel";
			transport3.JW_VoyageFlight = "Voyage NO";
			transport3.JW_RL_NKLoadPort = "AUBNE";
			transport3.JW_RL_NKDiscPort = "SGSIN";
			transport3.JW_ETD = ZDateTime.Today.AddDays(3);
			transport3.JW_ETA = ZDateTime.Today.AddDays(4);

			DocShipment shipment2Wrapper = DocShipment.New(shipment2, factory);
			collection = shipment2Wrapper.CompleteRouting;

			AssertEquals(3, collection.Count);
			AssertEquals("First load", "NZAKL", collection[0].PortOfLoading.Code);
			AssertEquals("First discharge", "AUBNE", collection[0].PortOfDischarge.Code);
			AssertEquals("Second load", "AUBNE", collection[1].PortOfLoading.Code);
			AssertEquals("Second discharge", "SGSIN", collection[1].PortOfDischarge.Code);
			AssertEquals("Third load", "SGSIN", collection[2].PortOfLoading.Code);
			AssertEquals("Third discharge", "JPOSA", collection[2].PortOfDischarge.Code);
		}

		public void TestPortDisplayMode()
		{
			AssertEquals("LoadDischargeCollectDeliver", ShipmentWrapper.PortDisplayMode);
		}

		public void TestShowChargesOnArrivalNotice()
		{
			AssertEquals(false, ShipmentWrapper.ShowChargesOnArrivalNotice);
		}

		public void TestShowExchangeRatesOnArrivalNotice()
		{
			AssertEquals(false, ShipmentWrapper.ShowExchangeRatesOnArrivalNotice);
		}

		public void TestCTOArrivalBerth()
		{
			AssertEquals("CTOArrivalBerth empty", "", ShipmentWrapper.CTOArrivalBerth);
			CommonConsol consol = Shipment.Consols.AddNew();
			var header = Factory.New<OrgHeader>();
			var address = header.Addresses.AddNew();
			header.OH_Code = "CODE123";
			consol.JK_OA_ArrivalCTOAddress = address.PK;

			AssertEquals("CTOArrivalBerth is CTO's Code", "CODE123", ShipmentWrapper.CTOArrivalBerth);

			CreateASailing();
			BookingDestination.JB_Berth = "BERTH123";

			Transport transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;
			AssertEquals("CTOArrivalBerth is CTO's Code plus Arrival Berth", "CODE123 / BERTH123", ShipmentWrapper.CTOArrivalBerth);
		}

		public void TestGoodsAvailableAt()
		{
			var shipment = Factory.New<CommonShipment>();
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);

			CommonConsol consol = CreateImportConsol(shipment);
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			var arrivalCTO = Factory.NewWithValidTestData<OrgAddress>();
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.PK;
			arrivalCTO.OA_Address1 = "ArrivalCTOAddress";

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("GoodsAvailableAt ArrivalCTO for BBK Shipment", arrivalCTO.OA_Address1.Trim(), shipmentWrapper.GoodsAvailableAt.Address1);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("GoodsAvailableAt ArrivalCTO for ROR Shipment", arrivalCTO.OA_Address1.Trim(), shipmentWrapper.GoodsAvailableAt.Address1);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("GoodsAvailableAt for Shipment FCL & Consol FCL", arrivalCTO.OA_Address1.Trim(), shipmentWrapper.GoodsAvailableAt.Address1);

			var unpackDepot = Factory.NewWithValidTestData<OrgAddress>();
			unpackDepot.OA_Address1 = "UnpackDepot";
			consol.JK_OA_UnpackDepotAddress = unpackDepot.PK;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("GoodsAvailableAt Shipment LCL & Consol LCL", unpackDepot.OA_Address1.Trim(), shipmentWrapper.GoodsAvailableAt.Address1);

			var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
			importReleaseDepot.OA_Address1 = "ImportReleaseDepot";

			AssertEquals("GoodsAvailableAt with Import Release Depot", importReleaseDepot.OA_Address1.Trim(), shipmentWrapper.GoodsAvailableAt.Address1);
		}

		public void TestUnpackAt()
		{
			AssertNull("UnpackAt null", ShipmentWrapper.UnpackAt);
			CommonConsol consol = Shipment.Consols.AddNew();
			var header = Factory.New<OrgHeader>();
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "Test Address1";
			consol.JK_OA_UnpackDepotAddress = address.PK;
			AssertEquals("UnpackAt Address", "Test Address1", ShipmentWrapper.UnpackAt.Address1);

			var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
			Shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
			AssertEquals("Unpack Address", importReleaseDepot.OA_Address1, ShipmentWrapper.UnpackAt.Address1);
		}

		#endregion

		#region Import/Export Transhipment Fields
		public void TestImportExportTranshipmentConsol()
		{
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";

			AssertEquals("No consol", null, ShipmentWrapper.ImportTranshipmentConsol);
			AssertEquals("No consol", null, ShipmentWrapper.ExportTranshipmentConsol);

			var consol1 = Shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "SGSIN";
			ShipmentWrapper = GetNewShipmentWrapper();

			AssertEquals("Import consol is the only consol", "AUSYD", ShipmentWrapper.ImportTranshipmentConsol.PortOfLoading.Code);
			AssertEquals("No Export consol", null, ShipmentWrapper.ExportTranshipmentConsol);

			var consol2 = Shipment.Consols.AddNew();
			Transport transport2 = consol2.Transports[0];
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "USLAX";
			ShipmentWrapper = GetNewShipmentWrapper();

			AssertEquals("Import consol should be AUSYD -> SGSIN", "AUSYD", ShipmentWrapper.ImportTranshipmentConsol.PortOfLoading.Code);
			AssertEquals("Export consol should be SGSIN -> USLAX", "SGSIN", ShipmentWrapper.ExportTranshipmentConsol.PortOfLoading.Code);

			var consol3 = Shipment.Consols.AddNew();
			consol3.JK_RL_NKLoadPort = "NZAKL";
			consol3.JK_RL_NKDischargePort = "AUSYD";
			ShipmentWrapper = GetNewShipmentWrapper();

			AssertEquals("Import consol should be AUSYD -> SGSIN", "AUSYD", ShipmentWrapper.ImportTranshipmentConsol.PortOfLoading.Code);
			AssertEquals("Export consol should be SGSIN -> USLAX", "SGSIN", ShipmentWrapper.ExportTranshipmentConsol.PortOfLoading.Code);
		}

		public void TestImportExportTranshipmentContainerNumbers()
		{
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";

			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();

			var consol1 = Shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			var cont1 = consol1.Containers.AddNew();
			cont1.JC_ContainerNum = "NUM1";
			cont1.AddPackLine(line1);
			cont1.AddPackLine(line2);

			AssertEquals("1 Container number for Import Tranship", 1, ShipmentWrapper.ImportTranshipmentContainers.Count);
			AssertEquals("Container number for Import Tranship", "NUM1", ShipmentWrapper.ImportTranshipmentContainerNumbers);

			var consol2 = Shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "USLAX";

			var cont2 = consol2.Containers.AddNew();
			cont2.JC_ContainerNum = "NUM2";
			var cont3 = consol2.Containers.AddNew();
			cont3.JC_ContainerNum = "NUM3";
			cont2.AddPackLine(line1);
			cont3.AddPackLine(line2);

			AssertEquals("1 Container number for Import Tranship", 1, ShipmentWrapper.ImportTranshipmentContainers.Count);
			AssertEquals("Container number for Import Tranship", "NUM1", ShipmentWrapper.ImportTranshipmentContainerNumbers);

			AssertEquals("2 Container number for Export Tranship", 2, ShipmentWrapper.ExportTranshipmentContainers.Count);
			AssertEquals("Container number for Export Tranship contains 'NUM2' container", ZBool.True, ShipmentWrapper.ExportTranshipmentContainerNumbers.Contains("NUM2"));
			AssertEquals("Container number for Export Tranship contains 'NUM3' container", ZBool.True, ShipmentWrapper.ExportTranshipmentContainerNumbers.Contains("NUM3"));
		}

		JobSailing TranshipmentLeg1;
		JobSailing TranshipmentLeg2;

		JobSailing CreateSailing(ZString voyage, ZString vessel, ZString portOfLoading, ZDateTime eTD, ZString portOfDischarge, ZDateTime eTA)
		{
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			jobVoyage.JV_VoyageFlight = voyage;
			jobVoyage.JV_RV_NKVessel = vessel;
			VoyageOrigin origin = jobVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = portOfLoading;
			origin.JA_E_DEP = eTD;
			VoyageDestination destination = jobVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_E_ARV = eTA;
			jobVoyage.GenerateSailings();
			return jobVoyage.Sailings[0];
		}

		void SetupTranshipmentLegs()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "TRANSHIP TEST IMPORT ";

			var vessel2 = Factory.New<RefVessel>();
			vessel1.RV_Code = "TRANSHIP TEST EXPORT";

			TranshipmentLeg1 = CreateSailing("34", vessel1.RV_Code, "USNYC", ZDateTime.Today.AddDays(-4), "SGSIN", ZDateTime.Today.AddDays(10));
			TranshipmentLeg2 = CreateSailing("56", vessel2.RV_Code, "SGSIN", ZDateTime.Today.AddDays(14), "HKHKG", ZDateTime.Today.AddDays(38));
		}

		public void TestImportExportTranshipmentAndNonForwardingBusinessObjects()
		{
			SetupTranshipmentLegs();
			var line1 = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();

			var consol1 = Shipment.Consols.AddNew();
			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = TranshipmentLeg1.PK;

			var cont1 = consol1.Containers.AddNew();
			cont1.JC_ContainerNum = "NUM1";
			cont1.AddPackLine(line1);
			cont1.AddPackLine(line2);

			var consol2 = Shipment.Consols.AddNew();
			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = TranshipmentLeg2.PK;

			var cont2 = consol2.Containers.AddNew();
			cont2.JC_ContainerNum = "NUM2";
			var cont3 = consol2.Containers.AddNew();
			cont3.JC_ContainerNum = "NUM3";
			cont2.AddPackLine(line1);
			cont3.AddPackLine(line2);
			Factory.Save();

			BusinessObjectFactory docFactory = new BusinessObjectFactory();
			DocShipment docShipment = DocShipment.New(docFactory, Shipment.PK);
			AssertNotNull("Should not blow up on accessing value", docShipment.ExportTranshipmentContainerNumbers);
		}

		#endregion

		#region IShipperDepartureNotice Members

		public void TestShipperDepartureNoticeDocumentHeader()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			ShipmentWrapper.SetReportNameForTesting("Test Report Name");
			AssertEquals("Shipper Departure Notice Document Header", "FCL Sea Freight Test Report Name", ShipmentWrapper.ShipperDepartureNoticeDocumentHeader);
		}

		public void TestAgentsBookingReference()
		{
			AssertEquals("AgentsBookingReference is empty", ZString.Empty, ShipmentWrapper.AgentsBookingReference);

			var consol = Shipment.Consols.AddNew();
			AssertEquals("AgentsBookingReference is empty", ZString.Empty, ShipmentWrapper.AgentsBookingReference);

			consol.JK_AgentsReference = "Test";
			AssertEquals("AgentsBookingReference is not empty", "Test", ShipmentWrapper.AgentsBookingReference);
		}

		public void TestDepartureReference()
		{
			AssertEquals("DepartureReference is empty", ZString.Empty, ShipmentWrapper.DepartureReference);

			var consol = Shipment.Consols.AddNew();
			AssertEquals("DepartureReference is empty", ZString.Empty, ShipmentWrapper.DepartureReference);

			var sailing = Factory.New<JobSailing>();
			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			Factory.Save();
			origin.JA_DepartReference = "Depart Reference";

			Transport transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;
			AssertEquals("DepartureReference is not empty", "Depart Reference", ShipmentWrapper.DepartureReference);
		}

		public void TestReceivingForwarder()
		{
			AssertNull("Receiving Forwarder is empty", ShipmentWrapper.ReceivingForwarder);

			var consol = Shipment.Consols.AddNew();
			AssertNull("Receiving Forwarder is empty", ShipmentWrapper.ReceivingForwarder);

			consol.SetDefaultReceivingForwarderAddress(Factory.LoadTop1<OrgHeader>(new ZQuery()));
			AssertNotNull("Receiving Forwarder is not empty", ShipmentWrapper.ReceivingForwarder);
		}

		public void TestSendingForwarder()
		{
			AssertNull("Sending Forwarder is empty", ShipmentWrapper.SendingForwarder);

			var consol = Shipment.Consols.AddNew();
			AssertNull("Sending Forwarder is empty", ShipmentWrapper.SendingForwarder);

			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			consol.SetDefaultSendingForwarderAddress(org);
			AssertNotNull("Sending Forwarder is not empty", ShipmentWrapper.SendingForwarder);
			AssertEquals("Sending Forwarder Name", ShipmentWrapper.SendingForwarder.Name, org.OH_FullName);
		}

		#endregion

		#region Containers Test

		public void TestContainerLine()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CNSHA";

			CommonContainer container1 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR1");
			CommonContainer container2 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR2");
			CommonContainer container3 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR3");
			CommonContainer container4 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR4");
			CommonContainer container5 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR5");
			CommonContainer container6 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR6");

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Consol", consol.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);
			AssertEquals("Containers count", 6, ShipmentWrapper.Containers.Count);
			ZString expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6";
			AssertEquals("ContainerLine", expectedContainerLine, ShipmentWrapper.ContainerLine);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Consol", consol.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);
			CommonContainer container7 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR7");
			AssertEquals("Containers count", 7, ShipmentWrapper.Containers.Count);
			expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6, CONTNMBR7";
			AssertEquals("ContainerLine", expectedContainerLine, ShipmentWrapper.ContainerLine);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Consol", consol.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber);
			CommonContainer container8 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR8");
			AssertEquals("Containers count", 8, ShipmentWrapper.Containers.Count);
			expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6, CONTNMBR7 ...";
			AssertEquals("ContainerLine", expectedContainerLine, ShipmentWrapper.ContainerLine);
		}

		public void TestContainerNumberAndTypeLine()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CNSHA";
			CommonContainer container1 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR1");
			CommonContainer container2 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR2");
			CommonContainer container3 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR3");
			CommonContainer container4 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR4");
			CommonContainer container5 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR5");
			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "10FR";
			RefContainer ref2 = Factory.New<RefContainer>();
			ref2.RC_Code = "20FR";
			RefContainer ref3 = Factory.New<RefContainer>();
			ref3.RC_Code = "30FR";
			RefContainer ref4 = Factory.New<RefContainer>();
			ref4.RC_Code = "40FR";
			RefContainer ref5 = Factory.New<RefContainer>();
			ref5.RC_Code = "50FR";
			container1.JC_RC = ref1.PK;
			container2.JC_RC = ref2.PK;
			container3.JC_RC = ref3.PK;
			container4.JC_RC = ref4.PK;
			container5.JC_RC = ref5.PK;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			ZString expectedContainerNumberAndTypeLine = "CONTNMBR1 (10FR), CONTNMBR2 (20FR), CONTNMBR3 (30FR), CONTNMBR4 (40FR), CONTNMBR5 (50FR)";
			AssertEquals("ContainerNumberAndTypeLine and etalon must be equal", expectedContainerNumberAndTypeLine, ShipmentWrapper.ContainerNumberAndTypeLine);
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			CommonContainer container6 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR6");
			expectedContainerNumberAndTypeLine = "CONTNMBR1 (10FR), CONTNMBR2 (20FR), CONTNMBR3 (30FR), CONTNMBR4 (40FR), CONTNMBR5 (50FR) ...";
			AssertEquals("ContainerNumberAndTypeLine and etalon must be equal", expectedContainerNumberAndTypeLine, ShipmentWrapper.ContainerNumberAndTypeLine);

			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			expectedContainerNumberAndTypeLine = "See attached Container Details list.";
			AssertEquals("ContainerNumberAndTypeLine and etalon must be equal", expectedContainerNumberAndTypeLine, ShipmentWrapper.ContainerNumberAndTypeLine);
		}

		public void TestContainerNumberOnNewLine()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CNSHA";
			CommonContainer container1 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR1");
			CommonContainer container2 = MakeNewContainerForShipment(consol, Shipment, null);
			CommonContainer container3 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR3");
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			ZString expectedLine = "CONTNMBR1" + System.Environment.NewLine + System.Environment.NewLine + "CONTNMBR3" + System.Environment.NewLine;
			AssertEquals("ContainerNumberOnNewLine and etalon must be equal", expectedLine, ShipmentWrapper.ContainerNumberOnNewLine);
		}

		public void TestContainerTypeOnNewLine()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CNSHA";
			CommonContainer container1 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR1");
			CommonContainer container2 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR2");
			CommonContainer container3 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR3");
			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			container1.JC_RC = ref1.PK;
			RefContainer ref3 = Factory.New<RefContainer>();
			ref3.RC_Code = "40FR";
			container3.JC_RC = ref3.PK;
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			ZString expectedLine = "20FR" + System.Environment.NewLine + System.Environment.NewLine + "40FR" + System.Environment.NewLine;
			AssertEquals("ContainerTypeOnNewLine and etalon must be equal", expectedLine, ShipmentWrapper.ContainerTypeOnNewLine);
		}

		public void TestPrintPageWithContainerNumber()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "CNSHA";
			CommonContainer container1 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR1");
			CommonContainer container2 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR2");
			CommonContainer container3 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR3");
			CommonContainer container4 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR4");
			CommonContainer container5 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR5");
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)ShipmentWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)ShipmentWrapper.PrintPageWithContainerNumber);
			CommonContainer container6 = MakeNewContainerForShipment(consol, Shipment, "CONTNMBR6");
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)ShipmentWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be true", true, (bool)ShipmentWrapper.PrintPageWithContainerNumber);
		}

		CommonContainer MakeNewContainerForShipment(CommonConsol consol, CommonShipment shipment, ZString containerNumber)
		{
			PackLine packLine = shipment.OuterPackLines.AddNew();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = containerNumber;
			packLine.SetContainer(consol, container);
			return container;
		}

		#endregion

		#region IDocJobDetail Tests

		public void TestOurReference()
		{
			Shipment.JS_UniqueConsignRef = "S12345678";
			AssertEquals(ShipmentWrapper.ShipmentNumber, ShipmentWrapper.OurReference);
		}

		public void TestSupplierAsString()
		{
			AssertNull("Consignor", ShipmentWrapper.Consignor);
			AssertEquals("Supplier As String", ZString.Empty, ShipmentWrapper.SupplierAsString);

			Shipment.ConsignorPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Consignor", ShipmentWrapper.Consignor);
			AssertEquals("Supplier As String", Shipment.Consignor.OH_FullName, ShipmentWrapper.SupplierAsString);
		}

		public void TestVesselAndVoyage()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			CommonConsol consol1 = Shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;

			Transport transport = consol1.Transports[0];
			transport.JW_VoyageFlight = "FLY 123";

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertEquals("Vessel and Voyage", "FLY 123 /     /    ", ShipmentWrapper.VesselAndVoyage);
		}

		public void TestVesselAndVoyageWithDate()
		{
			AssertEquals("Precondition", ZString.Empty, ShipmentWrapper.VesselAndVoyageWithDate);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("No Consol", ZString.Empty, ShipmentWrapper.VesselAndVoyageWithDate);

			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "VOY123";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_ETD = new ZDateTime(2009, 1, 2, 3, 4, 5);
			AssertEquals("Transport", vessel.RV_Code + " / " + "VOY123" + " / 02-Jan-09", ShipmentWrapper.VesselAndVoyageWithDate);

			transport.JW_ATD = new ZDateTime(2009, 6, 7, 8, 9, 10);
			AssertEquals("Transport", vessel.RV_Code + " / " + "VOY123" + " / 07-Jun-09", ShipmentWrapper.VesselAndVoyageWithDate);

			transport.JW_ATD = ZDateTime.Empty;

			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			transport.JW_VoyageFlight = "JOU123";
			transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / " + "JOU123 / 02-Jan-09 03:04", ShipmentWrapper.VesselAndVoyageWithDate);

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_VoyageFlight = "JOU123";
			transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / " + "JOU123 / 02-Jan-09 03:04", ShipmentWrapper.VesselAndVoyageWithDate);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "FL123";
			RefUNLOCO uNLOCOBisObject = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			transport.JW_RL_NKDiscPort = uNLOCOBisObject.RL_Code;
			AssertEquals("Transport", "FL123 / " + uNLOCOBisObject.RL_Code + " / 02-Jan-09 03:04", ShipmentWrapper.VesselAndVoyageWithDate);

			transport.JW_ATD = new ZDateTime(2009, 6, 7, 8, 9, 10);
			AssertEquals("Transport", "FL123 / " + uNLOCOBisObject.RL_Code + " / 07-Jun-09 08:09", ShipmentWrapper.VesselAndVoyageWithDate);
		}

		public void TestMasterBillNumber()
		{
			AssertEquals("Master Bill Number", ShipmentWrapper.MasterBillNum, ShipmentWrapper.MasterBillNumber);
		}

		public void TestETAPortName()
		{
			SetUpShipmentWithValidPortCodes();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ETD Port Name", ShipmentWrapper.DestinationLoco.PortName, ShipmentWrapper.ETAPortName);
		}

		public void TestETDPortName()
		{
			SetUpShipmentWithValidPortCodes();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ETD Port Name", ShipmentWrapper.OriginLoco.PortName, ShipmentWrapper.ETDPortName);
		}

		public void TestService()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("PackingMode", ShipmentWrapper.PackingMode, ShipmentWrapper.Service);
		}

		public void TestPackageQuantity()
		{
			Shipment.JS_OuterPacks = 100;
			AssertEquals("Package Quantity", ShipmentWrapper.OuterPacks.ToString(), ShipmentWrapper.PackageQuantity);
		}

		public void TestPackageType()
		{
			Shipment.JS_F3_NKPackType = "CNT";
			AssertEquals("Package Quantity", ShipmentWrapper.OuterPacksPackTypeDescription, ShipmentWrapper.PackageType);
		}

		public void TestConsolDepot()
		{
			AssertEquals(ZString.Empty, ShipmentWrapper.ConsolDepot);
		}

		public void TestWeightAsString()
		{
			Shipment.JS_ActualWeight = 123;
			Shipment.JS_UnitOfWeight = "KG";
			AssertEquals(ShipmentWrapper.Weight + " " + ShipmentWrapper.WeightUnit, ShipmentWrapper.WeightAsString);
		}

		public void TestVolumeAsString()
		{
			Shipment.JS_ActualVolume = 123456789;
			Shipment.JS_UnitOfVolume = "M3";
			AssertEquals(ShipmentWrapper.Volume + " " + ShipmentWrapper.VolumeUnit, ShipmentWrapper.VolumeAsString);
		}

		public void TestConsignorAsString()
		{
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsignorPK = organisation.PK;
			AssertEquals(ShipmentWrapper.Consignor.Name, ShipmentWrapper.ConsignorAsString);
		}

		public void TestConsigneeAsString()
		{
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsigneePK = organisation.PK;
			AssertEquals(ShipmentWrapper.Consignee.Name, ShipmentWrapper.ConsigneeAsString);
		}

		public void TestShortContainerAndSealNumbers()
		{
			PackLine packLine1 = Shipment.OuterPackLines.AddNew();

			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "CNSHA";

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			container1.JC_SealNum = "1234";
			var rContainer1 = Factory.New<RefContainer>();
			rContainer1.RC_Code = "ABC";
			container1.JC_RC = rContainer1.PK;
			container1.JC_ContainerNum = "Container1";
			packLine1.SetContainer(consol, container1);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));

			AssertEquals("Short Container and Seal Numbers", "CONTAINER1 / 1234 / ABC", ShipmentWrapper.ShortContainerAndSealNumbersForInvoice);
		}

		public void TestLongContainerAndSealNumbers()
		{
			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();

			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "CNSHA";

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			container1.JC_SealNum = "1234";
			var rContainer1 = Factory.New<RefContainer>();
			rContainer1.RC_Code = "ABC";
			container1.JC_RC = rContainer1.PK;
			container1.JC_ContainerNum = "Container1";
			packLine1.SetContainer(consol, container1);

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "5";
			container2.JC_SealNum = "4321";
			var rContainer2 = Factory.New<RefContainer>();
			rContainer2.RC_Code = "XYZ";
			container2.JC_RC = rContainer2.PK;
			container2.JC_ContainerNum = "Container2";
			packLine2.SetContainer(consol, container2);

			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));

			ZString resultShouldBe = "- CONTAINER1      - 1234                 - ABC            \n";
			resultShouldBe += "- CONTAINER2      - 4321                 - XYZ            \n";
			AssertEquals("Long Container and Seal Numbers", resultShouldBe, ShipmentWrapper.LongContainerAndSealNumbersForInvoice);
		}

		public void TestNumberOfContainers()
		{
			AssertEquals("Number of Containers ", ShipmentWrapper.Containers.Count, ShipmentWrapper.NumberOfContainers);

			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();

			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "CNSHA";

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";
			packLine1.SetContainer(consol, container1);

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "Container2";
			packLine2.SetContainer(consol, container2);

			AssertEquals("Number of Containers ", ShipmentWrapper.Containers.Count, ShipmentWrapper.NumberOfContainers);
		}

		public void TestMarksAndNumbersForInvoice()
		{
			var marksAndNumberNote1 = Shipment.Notes.AddNew();
			marksAndNumberNote1.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumberNote1.ST_ParentID = Shipment.PK;
			marksAndNumberNote1.ST_Table = Shipment.TableName;
			marksAndNumberNote1.ST_NoteDataAsText = "Marks and numbers Line One\nLine Two";

			var otherNotes = Shipment.Notes.AddNew();
			otherNotes.ST_ParentID = Shipment.PK;
			otherNotes.ST_Table = Shipment.TableName;
			otherNotes.ST_Description = PredefinedNoteTypes.Instance.BookingNotes.Description;
			otherNotes.ST_NoteDataAsText = "This is other notes and should not be included.";
			Factory.Save();

			AssertEquals("Marks and Numbers for Invoice", ShipmentWrapper.MarksAndNumbers, ShipmentWrapper.MarksAndNumbersForInvoice);
		}

		public void TestShortGoodsDescription()
		{
			Shipment.JS_GoodsDescription = "Test short desc";

			AssertEquals("Short Goods Description for Invoice", ShipmentWrapper.GoodsDescription, ShipmentWrapper.ShortGoodsDescriptionForInvoice);
		}

		public void TestLongGoodsDescription()
		{
			Shipment.JS_GoodsDescription = "Test short desc";

			var detailedDescription = Shipment.Notes.AddNew();
			detailedDescription.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			detailedDescription.ST_ParentID = Shipment.PK;
			detailedDescription.ST_Table = Shipment.TableName;
			detailedDescription.ST_NoteDataAsText = "Detailed goods description\nLine Two";
			Factory.Save();

			AssertEquals("Long Goods Description for Invoice", ShipmentWrapper.DescriptionForGoods, ShipmentWrapper.LongGoodsDescriptionForInvoice);
		}

		public void TestETADate()
		{
			Shipment.JS_E_ARV = ZDateTime.Now;
			AssertEquals("ETA Date", ShipmentWrapper.ETA, ShipmentWrapper.ETADate);
		}

		public void TestETDDate()
		{
			Shipment.JS_E_DEP = ZDateTime.Now;
			AssertEquals("ETD Date", ShipmentWrapper.ETD, ShipmentWrapper.ETDDate);
		}

		public void TestTransportModeIsAir()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TransportModeIsAir", true, ShipmentWrapper.TransportModeIsAir);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("TransportModeIsAir", false, ShipmentWrapper.TransportModeIsAir);
		}

		public void TestTransportModeIsSea()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportModeIsSea", true, ShipmentWrapper.TransportModeIsSea);

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("TransportModeIsSea", false, ShipmentWrapper.TransportModeIsSea);
		}
		#endregion

		#region ITimeSlotRequest Members

		public void TestConsolNumber()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "testtt";
			AssertEquals("testtt", ShipmentWrapper.ConsolNumber);
		}

		public void TestFullCartageInstructions()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsignorPK = orgConsignor.PK;
			Shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Preconditions: Shipment is Sea", true, Shipment.IsSea);

			StmNoteContexts seaStmNoteContext = new StmNoteContexts();
			seaStmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			seaStmNoteContext.Module |= StmNoteContextModule.A;
			seaStmNoteContext.Direction |= StmNoteContextDirection.A;
			FreightHelperClass.AddNote(Shipment, pickupDesc, "Shipment Pickup Instructions", seaStmNoteContext);
			FreightHelperClass.AddNote(Shipment, deliveryDesc, "Shipment Delivery Instructions", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions", seaStmNoteContext);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Cartage Instructions", "Shipment Pickup Instructions\nConsignee Pickup Instructions\nConsignor Pickup Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Cartage Instructions", "Shipment Delivery Instructions\nConsignor Delivery Instructions\nConsignee Delivery Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Preconditions: Shipment is Air", true, Shipment.IsAir);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Export Cartage Instructions for Air should return ALL cartage instructions entered on the shipment regardless of context.", "Shipment Pickup Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Import Cartage Instructions for Air should return ALL cartage instructions entered on the shipment regardless of context.", "Shipment Delivery Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			StmNoteContexts airStmNoteContext = new StmNoteContexts();
			airStmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions - Air", airStmNoteContext);
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions - Air", airStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions - Air", airStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions - Air", airStmNoteContext);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ShipmentAndOrgCartageInstruction Cartage Instructions", "Shipment Pickup Instructions\nConsignee Pickup Instructions - Air\nConsignor Pickup Instructions - Air", ShipmentWrapper.ShipmentAndOrgCartageInstruction);
			AssertEquals("ShipmentOrOrgCartageInstruction Cartage Instructions", "Shipment Pickup Instructions", ShipmentWrapper.ShipmentOrOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ShipmentAndOrgCartageInstruction Cartage Instructions", "Shipment Delivery Instructions\nConsignor Delivery Instructions - Air\nConsignee Delivery Instructions - Air", ShipmentWrapper.ShipmentAndOrgCartageInstruction);
			AssertEquals("ShipmentOrOrgCartageInstruction Cartage Instructions", "Shipment Delivery Instructions", ShipmentWrapper.ShipmentOrOrgCartageInstruction);
		}

		public void TestFullCartageInstructions_NoShipmentNote()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsignorPK = orgConsignor.PK;
			Shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Preconditions: Shipment is Sea", true, Shipment.IsSea);

			StmNoteContexts seaStmNoteContext = new StmNoteContexts();
			seaStmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			seaStmNoteContext.Module |= StmNoteContextModule.A;
			seaStmNoteContext.Direction |= StmNoteContextDirection.A;
			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions", seaStmNoteContext);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Cartage Instructions", "Consignor Pickup Instructions\nConsignee Pickup Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Cartage Instructions", "Consignor Delivery Instructions\nConsignee Delivery Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Preconditions: Shipment is Air", true, Shipment.IsAir);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("All Cartage Instructions for Air should not contain Shipment or Consignor SEA cartage instructions.", "Consignor Pickup Instructions\nConsignee Pickup Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("All Cartage Instructions for Air should not contain Shipment or Consignee SEA cartage instructions.", "Consignor Delivery Instructions\nConsignee Delivery Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			StmNoteContexts airStmNoteContext = new StmNoteContexts();
			airStmNoteContext.FreightMode |= StmNoteContextFreightMode.I;
			FreightHelperClass.AddNote(orgConsignor, pickupDesc, "Consignor Pickup Instructions", airStmNoteContext);
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "Consignor Delivery Instructions", airStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, pickupDesc, "Consignee Pickup Instructions", airStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "Consignee Delivery Instructions", airStmNoteContext);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ShipmentAndOrgCartageInstruction Cartage Instructions", "Consignor Pickup Instructions\nConsignee Pickup Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ShipmentAndOrgCartageInstruction Cartage Instructions", "Consignor Delivery Instructions\nConsignee Delivery Instructions", ShipmentWrapper.ShipmentAndOrgCartageInstruction);
		}

		public void TestFullCartageInstructionsAreNotDuplicatedWhenInstructionsAreDefinedInManyPlaces()
		{
			// Check that notes defined in two places appear only once if the content is identical

			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsignorPK = orgConsignor.PK;

			StmNoteContexts seaStmNoteContext = new StmNoteContexts();
			seaStmNoteContext.FreightMode |= StmNoteContextFreightMode.S;
			seaStmNoteContext.Module |= StmNoteContextModule.A;
			seaStmNoteContext.Direction |= StmNoteContextDirection.A;

			FreightHelperClass.AddNote(Shipment, deliveryDesc, "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only", seaStmNoteContext);
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only", seaStmNoteContext);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("A string defined in two places must appear only once.", "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only",
				ShipmentWrapper.ShipmentAndOrgCartageInstruction);
		}

		public void TestCartageInstructionsDuplicates()
		{
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.ConsigneePK = orgConsignee.PK;
			Shipment.ConsignorPK = orgConsignor.PK;

			StmNoteContexts allStmNoteContext = new StmNoteContexts();
			allStmNoteContext.FreightMode |= StmNoteContextFreightMode.A;
			allStmNoteContext.Module |= StmNoteContextModule.A;
			allStmNoteContext.Direction |= StmNoteContextDirection.A;

			FreightHelperClass.AddNote(orgConsignee, deliveryDesc, "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only", allStmNoteContext);
			FreightHelperClass.AddNote(orgConsignor, deliveryDesc, "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only", allStmNoteContext);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ShipmentOrOrgCartageInstruction duplicate testing: A string defined in multiple places must appear only once.", "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only", ShipmentWrapper.ShipmentOrOrgCartageInstruction);

			FreightHelperClass.AddNote(Shipment, deliveryDesc, "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only", allStmNoteContext);
			AssertEquals("ShipmentOrOrgCartageInstruction duplicate testing: A string defined in multiple places must appear only once.", "I am the same literal string, defined on shipment, shipper and consignee.  I must appear once only", ShipmentWrapper.ShipmentOrOrgCartageInstruction);
		}

		public void TestFullHandlingInstructionsAreNotDuplicatedWhenInstructionsAreDefinedInManyPlaces()
		{
			var shipmentNote = Shipment.Notes.AddNew();
			shipmentNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			shipmentNote.ST_ParentID = Shipment.PK;
			shipmentNote.ST_Table = Shipment.TableName;
			shipmentNote.ST_NoteDataAsText = "I must appear only once";

			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeNote = orgConsignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_ParentID = orgConsignee.PK;
			consigneeNote.ST_Table = orgConsignee.TableName;
			consigneeNote.ST_NoteDataAsText = "I must appear only once";
			Shipment.ConsigneePK = orgConsignee.PK;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("I must appear only once", ShipmentWrapper.ShipmentAndOrgHandlingInstructions);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("I must appear only once", ShipmentWrapper.ShipmentAndOrgHandlingInstructions);
		}

		public void TestFullHandlingInstructions()
		{
			var shipmentNote = Shipment.Notes.AddNew();
			shipmentNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			shipmentNote.ST_ParentID = Shipment.PK;
			shipmentNote.ST_Table = Shipment.TableName;
			shipmentNote.ST_NoteDataAsText = "Shipment Handling Instruction";

			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeNote = orgConsignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_ParentID = orgConsignee.PK;
			consigneeNote.ST_Table = orgConsignee.TableName;
			consigneeNote.ST_NoteDataAsText = "OrgConsignee Note";
			Shipment.ConsigneePK = orgConsignee.PK;

			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorNote = orgConsignor.Notes.AddNew();
			consignorNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consignorNote.ST_ParentID = orgConsignor.PK;
			consignorNote.ST_Table = orgConsignor.TableName;
			consignorNote.ST_NoteDataAsText = "OrgConsignor Note Don't Show";
			Shipment.ConsignorPK = consignorNote.PK;

			AssertEquals(ShipmentWrapper.ShipmentAndOrgHandlingInstructions, ShipmentWrapper.FullHandlingInstructions);
		}

		public void TestCTOAddress()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			var arrivalCTO = Factory.NewWithValidTestData<OrgAddress>();
			arrivalCTO.OA_Address1 = "import CTO";
			var departureCTO = Factory.NewWithValidTestData<OrgAddress>();
			departureCTO.OA_Address1 = "export CTO";
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.PK;
			consol.JK_OA_DepartureCTOAddress = departureCTO.PK;

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals(arrivalCTO.OA_Address1, ShipmentWrapper.CTOAddress.Address1);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(departureCTO.OA_Address1, ShipmentWrapper.CTOAddress.Address1);
		}

		public void TestSimpleContainers()
		{
			CommonConsol consol = Shipment.Consols.AddNew();

			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "ctr1";
			packLine1.SetContainer(consol, container1);

			AssertEquals(1, ShipmentWrapper.SimpleContainers.Count);
			AssertEquals(container1.JC_ContainerNum, ShipmentWrapper.SimpleContainers[0].ContainerNumber);
		}

		#endregion

		#region IContainsSuppressedFields

		public void TestSuppressFlightDetails()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			AssertEquals("SuppressFlightDetails should be false", false, ShipmentWrapper.SuppressFlightDetails);

			Shipment.DocsAndCartage.JP_PickupCartageCompleted = DateTime.Today.AddDays(2);
			AssertEquals("SuppressFlightDetails should be false", false, Suppression.EnabledForAnyOfFields(Shipment, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			AssertEquals("SuppressFlightDetails should be true", true, Suppression.EnabledForAnyOfFields(Shipment, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			Shipment.DocsAndCartage.JP_PickupCartageCompleted = DateTime.Today.AddDays(-1);
			var consol = CreateExportConsol(Shipment);

			Transport transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today.AddDays(-1);
			AssertEquals("SuppressFlightDetails should be false", false, Suppression.EnabledForAnyOfFields(Shipment, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			transport.JW_ETD = ZDateTime.Today.AddDays(2);
			AssertEquals("SuppressFlightDetails should be true", false, Suppression.EnabledForAnyOfFields(Shipment, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			AssertEquals("SuppressFlightDetails should be false", false, Suppression.EnabledForAnyOfFields(Shipment, new[] { SuppressFields.MasterBill }, ContactType.Consignor));

			Shipment.DocsAndCartage.JP_PickupCartageCompleted = DateTime.Today.AddDays(2);
			transport.JW_ETD = ZDateTime.Today.AddDays(2);
			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			AssertEquals("SuppressFlightDetails should be true", true, Suppression.EnabledForAnyOfFields(Shipment, new[] { SuppressFields.MasterBill }, ContactType.Consignor));
		}

		public void TestMasterBill_OrSuppressed()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("MasterBill_OrSuppressed", "", ShipmentWrapper.MasterBill_OrSuppressed);

			var consol = CreateExportConsol(Shipment);
			consol.JK_MasterBillNum = "12345";
			AssertEquals("MasterBill_OrSuppressed", "123-45", ShipmentWrapper.MasterBill_OrSuppressed);

			consol.JK_MasterBillNum = "12345678901";
			AssertEquals("MasterBill_OrSuppressed", "123-4567 8901", ShipmentWrapper.MasterBill_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			AssertEquals("MasterBill_OrSuppressed", "*", ShipmentWrapper.MasterBill_OrSuppressed);
		}

		public void TestTransportInfo_OrSuppressed()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("TransportInfo_OrSuppressed", "", ShipmentWrapper.TransportInfo_OrSuppressed);

			Shipment.JS_IsBooking = ZBool.False;
			AssertEquals("", ShipmentWrapper.BookingTransport);

			var consol = CreateExportConsol(Shipment);
			Transport transport = consol.Transports[0];
			consol.JK_TransportMode = "SEA";
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "111";

			AssertEquals(vessel.RV_Code + " / 111 / " + vessel.RV_LloydsNumber, ShipmentWrapper.BookingTransport);
			AssertEquals("TransportInfo_OrSuppressed", vessel.RV_Code + " / 111 / " + vessel.RV_LloydsNumber, ShipmentWrapper.TransportInfo_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			AssertEquals("TransportInfo_OrSuppressed", "*", ShipmentWrapper.TransportInfo_OrSuppressed);
		}

		public void TestETD_OrSuppressed()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("ETD_OrSuppressed", "", ShipmentWrapper.ETD_OrSuppressed);

			Shipment.JS_E_DEP = ZDateTime.Today;
			AssertEquals("ETD_OrSuppressed only shows date", ShipmentWrapper.CollectedFromETDString, ShipmentWrapper.ETD_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			AssertEquals("ETD_OrSuppressed", "*", ShipmentWrapper.ETD_OrSuppressed);
		}

		public void TestATD_OrSuppressed()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("ATD_OrSuppressed", "", ShipmentWrapper.ATD_OrSuppressed);

			Shipment.JS_E_DEP = ZDateTime.Today;
			AssertEquals("ATD_OrSuppressed only shows date", ShipmentWrapper.CollectedFromATDString, ShipmentWrapper.ATD_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			AssertEquals("ATD_OrSuppressed", "*", ShipmentWrapper.ATD_OrSuppressed);
		}

		public void TestLoadingETD_OrSuppressed()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("LoadingETD_OrSuppressed", "", ShipmentWrapper.LoadingETD_OrSuppressed);

			var consol = CreateExportConsol(Shipment);

			Transport transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today;
			AssertEquals("LoadingETD_OrSuppressed only shows date", ShipmentWrapper.LoadingETDString, ShipmentWrapper.LoadingETD_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			transport.JW_ETD = DateTime.Today.AddDays(2);
			AssertEquals("LoadingETD_OrSuppressed", "*", ShipmentWrapper.LoadingETD_OrSuppressed);
		}

		public void TestLoadingATD_OrSuppressed()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("LoadingATD_OrSuppressed", "", ShipmentWrapper.LoadingATD_OrSuppressed);

			var consol = CreateExportConsol(Shipment);

			Transport transport = consol.Transports[0];
			transport.JW_ATD = ZDateTime.Today;
			AssertEquals("LoadingATD_OrSuppressed only shows date", ShipmentWrapper.Consol.ATDString, ShipmentWrapper.LoadingATD_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			transport.JW_ATD = DateTime.Today.AddDays(2);
			AssertEquals("LoadingATD_OrSuppressed", "*", ShipmentWrapper.LoadingATD_OrSuppressed);
		}

		public void TestCarrierName_OrSuppressed()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("CarrierName_OrSuppressed", "", ShipmentWrapper.CarrierName_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			AssertEquals("CarrierName_OrSuppressed", "*", ShipmentWrapper.CarrierName_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			var consol = CreateExportConsol(Shipment);
			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consol.SetDefaultShippingLineAddress(shippingLine);
			AssertEquals("CarrierName_OrSuppressed", ShipmentWrapper.Carrier.Name, ShipmentWrapper.CarrierName_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			AssertEquals("CarrierName_OrSuppressed", "*", ShipmentWrapper.CarrierName_OrSuppressed);
		}

		public void TestCarrierCCC_OrSuppressed()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("CarrierCCC_OrSuppressed", "", ShipmentWrapper.CarrierCCC_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields> { SuppressFields.Carrier });
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			AssertEquals("CarrierCCC_OrSuppressed", "*", ShipmentWrapper.CarrierCCC_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			var consol = CreateExportConsol(Shipment);
			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consol.SetDefaultShippingLineAddress(shippingLine);
			AssertEquals("CarrierName_OrSuppressed", ShipmentWrapper.Carrier.CCC, ShipmentWrapper.CarrierCCC_OrSuppressed);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, new List<SuppressFields> { SuppressFields.Carrier });
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);
			AssertEquals("CarrierCCC_OrSuppressed", "*", ShipmentWrapper.CarrierCCC_OrSuppressed);
		}

		public void TestSuppressFlightDetailsFooter()
		{
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "INBOM";

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, false);
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("SuppressFlightDetailsFooter", "", ShipmentWrapper.SuppressFlightDetailsFooter);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			Shipment.JS_A_RCV = DateTime.Today.AddDays(2);

			AssertEquals("SuppressFlightDetailsFooter", "*Flight details suppressed for security reasons.", ShipmentWrapper.SuppressFlightDetailsFooter);
		}

		#endregion

		#region ITrackingBusinessObject

		public void TestTrackingBusinessContext()
		{
			AssertEquals(TrackingConstants.BusinessContext.Shipment, ShipmentWrapper.TrackingBusinessContext);
		}

		public void TestTrackingBusinessObjectPK()
		{
			AssertEquals(Shipment.PK, ShipmentWrapper.TrackingBusinessObjectPK);
		}

		#endregion

		#region STC Label For US Bound

		public void TestSTC_Label()
		{
			var consol = Shipment.Consols.AddNew();

			Transport originalTransport = consol.Transports[0];
			Transport transport1 = consol.Transports.AddNew();
			Transport transport2 = consol.Transports.AddNew();
			originalTransport.JW_RL_NKLoadPort = "AUSYD";
			originalTransport.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "CACAD";

			AssertEquals("STC Label should not have STC", "", ShipmentWrapper.STC_Label);

			transport1.JW_RL_NKDiscPort = "ZAAAM";
			transport2.JW_RL_NKLoadPort = "ZAAAM";

			AssertEquals("STC Label should have STC", "STC ", ShipmentWrapper.STC_Label);

			var consol2 = Shipment.Consols.AddNew();
			Transport transport3 = consol2.Transports[0];
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_RL_NKDiscPort = "AGANU";
			AssertEquals("STC Label should not have STC", "", ShipmentWrapper.STC_Label);

			transport3.JW_RL_NKLoadPort = "AGANU";
			transport3.JW_RL_NKDiscPort = "AGANU";
			AssertEquals("STC Label should have STC", "STC ", ShipmentWrapper.STC_Label);

			transport3.JW_RL_NKLoadPort = "AGANU";
			transport3.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("STC Label should not have STC", "", ShipmentWrapper.STC_Label);
		}

		#endregion

		#region Export Statement
		public void TestExportStatementAndSetting()
		{
			CountryExportStatementSettingCollection countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			CountryExportStatementSetting countrySetting = countrySettingCollection.AddNew();
			countrySetting.CountryCode = GlbBranch.CurrentBranch.Country.Code;
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "AES";
			statementSetting.Statement = "STATEMENT FOR TESTING";
			statementSetting.Visibility = "UDF";
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);
			CommonConsol consol = CreateExportConsol(Shipment);
			Shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			Shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
			AssertEquals("", ShipmentWrapper.ExportStatementSetting.Name);
			AssertEquals("", ShipmentWrapper.ExportStatementSetting.Statement);
			Shipment.DocsAndCartage.JP_ExportStatement = "AES";
			AssertEquals(Shipment.ExportStatement, ShipmentWrapper.ExportStatement);
			AssertEquals(statementSetting.Code, ShipmentWrapper.ExportStatementSetting.Name);
			AssertEquals(statementSetting.Statement, ShipmentWrapper.ExportStatementSetting.Statement);
			AssertNotNull(ShipmentWrapper.ExportStatementSetting);
		}
		#endregion

		#region Freight Location

		public void TestFreightLocation()
		{
			Shipment.JS_WarehouseLocation = "123";
			Factory.Save();
			AssertEquals("123", ShipmentWrapper.FreightLocation);
			Shipment.JS_WarehouseLocation = "";
			Shipment.JS_RL_NKOrigin = "AUNYC";
			Shipment.JS_RL_NKDestination = "AUSYD";
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address1 = org.Addresses[0];
			address1.OA_Address1 = "Address1";
			Shipment.JS_OA_ImportReleaseDepot = address1.PK;
			Factory.Save();
			AssertEquals("ADDRESS1", ShipmentWrapper.FreightLocation);
		}

		#endregion

		#region Flight No & Flight Date

		public void TestFlightNo()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "123";
			Factory.Save();
			AssertEquals("123", ShipmentWrapper.FlightNo);
		}

		public void TestFlightDate()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			Transport transport = consol.Transports[0];
			transport.JW_ATA = new ZDateTime("1-OCT-2007");
			Factory.Save();
			AssertEquals(new ZDateTime("1-OCT-2007"), ShipmentWrapper.FlightDate);
			transport.JW_ATA = ZDateTime.Empty;
			transport.JW_ETA = new ZDateTime("2-OCT-2007");
			Factory.Save();
			AssertEquals(new ZDateTime("2-OCT-2007"), ShipmentWrapper.FlightDate);
		}

		#endregion

		#region Not Cleared by Agent

		public void TestNotClearedByAgent()
		{
			FreightDataRegistry.Instance.NotClearedByAgentStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "~~~IIIIII");

			AssertEquals("", ShipmentWrapper.NotClearedByAgentStatement);
			AssertEquals("", ShipmentWrapper.NotClearedByAgentNumber);
			AssertEquals(ZDateTime.Empty, ShipmentWrapper.NotClearedByAgentIssueDate);
			AssertEquals(ZDateTime.Empty, ShipmentWrapper.NotClearedByAgentExpiryDate);

			CusEntryNumber num = Shipment.CusEntryNumbers.AddNew();
			num.CE_EntryType = CusEntryNumberTypes.EU.T1;
			num.CE_EntryNum = "asdasd";
			num.CE_IssueDate = new ZDateTime(2008, 10, 9);
			num.CE_ExpiryDate = new ZDateTime(2008, 10, 15);

			AssertEquals("", ShipmentWrapper.NotClearedByAgentStatement);
			AssertEquals("", ShipmentWrapper.NotClearedByAgentNumber);
			AssertEquals(ZDateTime.Empty, ShipmentWrapper.NotClearedByAgentIssueDate);
			AssertEquals(ZDateTime.Empty, ShipmentWrapper.NotClearedByAgentExpiryDate);

			Shipment.CusEntryNumbers.RemoveAndDeleteAll();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Germany);

			AssertEquals("", ShipmentWrapper.NotClearedByAgentStatement);
			AssertEquals("", ShipmentWrapper.NotClearedByAgentNumber);
			AssertEquals(ZDateTime.Empty, ShipmentWrapper.NotClearedByAgentIssueDate);
			AssertEquals(ZDateTime.Empty, ShipmentWrapper.NotClearedByAgentExpiryDate);

			num = Shipment.CusEntryNumbers.AddNew();
			num.CE_EntryType = CusEntryNumberTypes.EU.T1;
			num.CE_EntryNum = "asdasd";
			num.CE_IssueDate = new ZDateTime(2008, 10, 9);
			num.CE_ExpiryDate = new ZDateTime(2008, 10, 15);

			AssertEquals("~~~IIIIII", ShipmentWrapper.NotClearedByAgentStatement);
			AssertEquals("asdasd", ShipmentWrapper.NotClearedByAgentNumber);
			AssertEquals(new ZDateTime(2008, 10, 9), ShipmentWrapper.NotClearedByAgentIssueDate);
			AssertEquals(new ZDateTime(2008, 10, 15), ShipmentWrapper.NotClearedByAgentExpiryDate);
		}

		#endregion

		#region Implementation

		protected CommonShipment Shipment;
		VoyageOrigin BookingOrigin;
		VoyageDestination BookingDestination;
		JobVoyage Voyage;
		JobSailing Sailing;
		protected DocShipment ShipmentWrapper;

		CommonConsol CreateConsol(ZString consolNumber, ZString transportMode, ZString consolMode, ZString portOfLoading, ZString portOfDischarge)
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = consolNumber;
			consol.JK_RL_NKLoadPort = portOfLoading;
			consol.JK_RL_NKDischargePort = portOfDischarge;
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = consolMode;

			return consol;
		}

		CommonConsol CreateConsol(ZString consolNumber, ZString transportMode, ZString consolMode, ZString agentType, ZString portOfLoading, ZString portOfDischarge, ZDateTime etd, ZDateTime eta)
		{
			var consol = CreateConsol(consolNumber, transportMode, consolMode, portOfLoading, portOfDischarge);
			consol.JK_AgentType = agentType;
			consol.Transports[0].JW_ETD = etd;
			consol.Transports[0].JW_ETA = eta;
			return consol;
		}

		protected CommonConsol CreateExportConsol(CommonShipment shipment)
		{
			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKDischargePort = "USCHI";
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;
			return consol;
		}

		CommonConsol CreateImportConsol(CommonShipment shipment)
		{
			var consol = shipment.Consols.AddNew();

			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;
			return consol;
		}

		JobHeader CreateJobHeader(CommonShipment shipment)
		{
			var jobHeaderBisObj = Factory.NewJobForTesting<JobHeader>();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			var departmentBisObj = Factory.LoadTop1<GlbDepartment>(new ZQuery());
			jobHeaderBisObj.JH_GE = departmentBisObj.PK;
			jobHeaderBisObj.JH_JobNum = shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			return jobHeaderBisObj;
		}

		AccChargeCode CreateChargeCode(string chargeCode)
		{
			var code = Factory.New<AccChargeCode>();
			code.AC_Code = chargeCode;
			code.AC_Desc = "Test Charge Code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;
			code.AC_IsActive = ZBool.True;
			code.FillWithValidTestData();
			return code;
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBisObj.PK;
			lineCharge.JR_GE = jobHeaderBisObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBisObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_OH_SellAccount = localChargesPK;
			lineCharge.JR_OSSellAmt = amount;
			lineCharge.JR_LocalSellAmt = amount;

			return lineCharge;
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK, ZInt gSTRate)
		{
			JobCharge lineCharge = CreateLineCharge(jobHeaderBisObj, localChargesPK, amount, chargeCodePK);
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(gSTRate);
			lineCharge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			return lineCharge;
		}

		CommonShipment CreateShipment(CommonConsol consol, ZString shipmentNumber, ZString shipmentType, ZString transportMode, ZString packingMode, ZString origin, ZString destination)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_ShipmentType = shipmentType;
			shipment.JS_PackingMode = packingMode;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_UniqueConsignRef = shipmentNumber;

			return shipment;
		}

		Transport CreateShipmentTransport(CommonShipment shipment, ZString mode, ZString type, ZString load, ZString disch, ZDateTime eTD, ZDateTime eTA)
		{
			Transport leg = shipment.Transports.AddNew();
			return CreateTransport(leg, mode, type, load, disch, eTD, eTA);
		}

		Transport CreateTransport(Transport leg, ZString mode, ZString type, ZString load, ZString disch, ZDateTime eTD, ZDateTime eTA)
		{
			leg.JW_TransportMode = mode;
			leg.JW_TransportType = type;
			leg.JW_RL_NKLoadPort = load;
			leg.JW_RL_NKDiscPort = disch;
			leg.JW_ETD = eTD;
			leg.JW_ETA = eTA;
			return leg;
		}

		void CreateASailing()
		{
			Sailing = Factory.New<JobSailing>();
			Voyage = Factory.New<JobVoyage>();
			BookingOrigin = Factory.New<VoyageOrigin>();
			BookingDestination = Factory.New<VoyageDestination>();
			BookingOrigin.JA_JV = Voyage.PK;
			BookingDestination.JB_JV = Voyage.PK;
			Sailing.JX_JA = BookingOrigin.PK;
			Sailing.JX_JB = BookingDestination.PK;
		}

		protected override void SetUp()
		{
			Shipment = GetNewShipment();
			ShipmentWrapper = GetNewShipmentWrapper();

			base.SetUp();
		}

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
		}

		protected virtual CommonConsol GetNewConsol()
		{
			return Factory.New<CommonConsol>();
		}

		protected virtual CommonShipment GetNewShipment()
		{
			return CommonShipment.New(Factory);
		}

		protected virtual DocShipment GetNewShipmentWrapper()
		{
			return DocShipment.New(Shipment, Factory);
		}

		void SetUpShipmentWithValidPortCodes()
		{
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.PopulatePortCodes);
			ShipmentWrapper = DocShipment.New(Shipment, Factory);
			AssertNotNull("PreCondition: Valid Shipment", ShipmentWrapper);
		}

		#endregion

	}
}
