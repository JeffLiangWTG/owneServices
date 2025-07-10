using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CustomsEntryWrapperCollection))]
	public class CustomsEntryWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<CustomsEntryWrapperCollection>
	{
		public void TestLoadFromConsol()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = Factory.New<ForwardingShipment>();
			var num1 = AddCusNumber(shipment1, ZString.Empty, "123456", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			consol.Shipments.Add(shipment1);

			var shipment2 = Factory.New<ForwardingShipment>();
			var num2 = AddCusNumber(shipment2, ZString.Empty, "567890", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var num3 = AddCusNumber(shipment2, ZString.Empty, "789.10", ZString.Empty);

			consol.Shipments.Add(shipment2);

			var collection = new CustomsEntryWrapperCollection(consol, Factory);
			AssertCusNumber(consol, "All from Shipments in Local Country", num1, num2);

			var num4 = AddCusNumber(consol, ZString.Empty, "246810", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertCusNumber(consol, "Should now include consol's numbers too", num1, num2, num4);
		}

		public void TestLoadFromDispatchConsignment()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var helper = new WhsTransitTestHelper(Factory);

			var packageJob = Factory.New<PkgPackageJob>();
			var package1 = packingHelper.CreatePackage(packageJob, "1", 4, "PLT");
			package1.KP_Weight = 10;
			package1.KP_Volume = 11;
			var package2 = packingHelper.CreatePackage(packageJob, "2", 5, "PLT");
			package2.KP_Weight = 12;
			package2.KP_Volume = 13;

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Bobs Warehouse";
			var area = Factory.New<WhsArea>();
			area.WA_Name = "area";
			area.WA_WW_Whs = warehouse.PK;

			var receiveUnit = helper.CreateReceiveTransportationUnit("RHRef", warehouse.PK, area.PK);
			var receiveConsignment = helper.CreateReceiveConsignment("RC", "STD", warehouse.PK);
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;

			var packageJob2 = Factory.New<PkgPackageJob>();
			var package3 = packingHelper.CreatePackage(packageJob2, "1", 4, "PLT");
			package3.KP_Weight = 10;
			package3.KP_Volume = 11;
			var receiveUnit2 = helper.CreateReceiveTransportationUnit("RHRef2", warehouse.PK, area.PK);
			var receiveConsignment2 = helper.CreateReceiveConsignment("RC2", "STD", warehouse.PK);
			packageJob2.KJ_ParentID = receiveConsignment2.PK;
			packageJob2.KJ_ParentTableCode = receiveConsignment2.TablePrefix;

			var dispatchUnit = helper.CreateDispatchTransportationUnit("DHRef", warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DC", warehouse.PK, "STD");

			var num1 = AddCusNumber(dispatchConsignment, "HSB", "123");
			var num2 = AddCusNumber(dispatchConsignment, "MSB", "456");
			var num3 = AddCusNumber(dispatchConsignment, "HSB", "789");

			var packageStateRow1 = helper.CreatePackageState(package1, "ARV", receiveConsignment, receiveUnit, dispatchConsignment, dispatchUnit); // ARV - needs header but no location
			var packageStateRow2 = helper.CreatePackageState(package2, "ARV", receiveConsignment, receiveUnit, dispatchConsignment, dispatchUnit);
			var packageStateRow3 = helper.CreatePackageState(package3, "ARV", receiveConsignment2, receiveUnit2, dispatchConsignment, dispatchUnit);

			var collection = new CustomsEntryWrapperCollection(dispatchConsignment, Factory);
			AssertCusNumber(dispatchConsignment, "Grab from Receive Consignment", num1, num2, num3);
		}

		public void TestLoadFromWarehouseDocket()
		{
			var order = Factory.New<WhsOrder>();

			var ref1 = order.References.AddNew();
			var ref2 = order.References.AddNew();
			var ref3 = Factory.New<WhsDocketReference>();

			var collection = new CustomsEntryWrapperCollection(order, Factory);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ref1, ref2 }, collection.Cast<GenericWrapper>().Select(w => w.WrappedObject));
		}

		public void TestLoadFromPkgPackage()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var package = Factory.New<PkgPackage>();
			var cusEntry = helper.CreateAdditionalReference(package, "REF1", "IOT");

			var collection = new CustomsEntryWrapperCollection(package, Factory);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cusEntry }, collection.Cast<GenericWrapper>().Select(w => w.WrappedObject));
		}

		public void TestConsignmentCustomsEntryWrapperCollection()
		{
			var consignment = Factory.New<DtbBookingConsignment>();

			CusEntryNumber entryNumber1 = (CusEntryNumber)consignment.AdditionalReferenceNumbers.AddNew();
			entryNumber1.CE_EntryNum = "TEST1";
			entryNumber1.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNumber1.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber1.CE_ParentID = consignment.PK;
			entryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			CusEntryNumber entryNumber2 = (CusEntryNumber)consignment.AdditionalReferenceNumbers.AddNew();
			entryNumber2.CE_EntryNum = "TEST2";
			entryNumber2.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			entryNumber2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber2.CE_ParentID = consignment.PK;
			entryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			CusEntryNumber entryNumber3 = (CusEntryNumber)consignment.AdditionalReferenceNumbers.AddNew();
			entryNumber3.CE_EntryNum = "TEST3";
			entryNumber3.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			entryNumber3.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber3.CE_ParentID = consignment.PK;
			entryNumber3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			CustomsEntryWrapperCollection collection = new CustomsEntryWrapperCollection(consignment, Factory);
			AssertEquals("collecection.Count", 3, collection.Count);
		}

		public virtual void TestCustomsEntryWrapperTypeIndexer()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			CusEntryNumber entryNumber1 = shipment.CusEntryNumbers.AddNew();
			entryNumber1.CE_EntryNum = "TEST1";
			entryNumber1.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNumber1.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber1.CE_ParentID = shipment.PK;
			entryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			CusEntryNumber entryNumber2 = shipment.CusEntryNumbers.AddNew();
			entryNumber2.CE_EntryNum = "TEST2";
			entryNumber2.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			entryNumber2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber2.CE_ParentID = shipment.PK;
			entryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			CusEntryNumber entryNumber3 = shipment.CusEntryNumbers.AddNew();
			entryNumber3.CE_EntryNum = "TEST3";
			entryNumber3.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			entryNumber3.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber3.CE_ParentID = shipment.PK;
			entryNumber3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			CusEntryNumber entryNumber4 = shipment.CusEntryNumbers.AddNew();
			entryNumber4.CE_EntryNum = "TEST4";
			entryNumber4.CE_EntryType = "AAA";
			entryNumber4.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber4.CE_ParentID = shipment.PK;
			entryNumber4.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			CusEntryNumber entryNumber5 = shipment.CusEntryNumbers.AddNew();
			entryNumber5.CE_EntryNum = "TEST5";
			entryNumber5.CE_EntryType = "AAA";
			entryNumber5.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber5.CE_ParentID = shipment.PK;
			entryNumber5.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			CustomsEntryWrapperCollection collection = new CustomsEntryWrapperCollection(shipment, Factory);
			AssertEquals("collecection.Count", 5, collection.Count);

			CustomsEntryWrapper wrapper = collection[CusEntryNumberTypes.Australia.ECN];
			AssertNotNull("Wrapper Indexer Works", wrapper);
			AssertEquals("Entry Number", "TEST2", wrapper.EntryNumber);

			wrapper = collection[CusEntryNumberTypes.Australia.CAN];
			AssertNotNull("Wrapper Indexer Works", wrapper);
			AssertEquals("Entry Number", "TEST1", wrapper.EntryNumber);

			wrapper = collection["ClEarAnCe"];
			AssertNotNull("Wrapper Indexer Works", wrapper);
			AssertEquals("Entry Number", "TEST3", wrapper.EntryNumber);

			wrapper = collection["AAA"];
			AssertNotNull("Wrapper Indexer Works", wrapper);
			AssertEquals("Entry Number", "TEST4", wrapper.EntryNumber);

			AssertEquals("prerequisite", "ER", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			wrapper = collection["ER:AAA"];
			AssertNotNull("Wrapper Indexer Works", wrapper);
			AssertEquals("Entry Number", "TEST4", wrapper.EntryNumber);

			wrapper = collection["US:AAA"];
			AssertNull("Wrapper Indexer Works", wrapper);

			wrapper = collection["OTH:ER:AAA"];
			AssertNotNull("Wrapper Indexer Works", wrapper);
			AssertEquals("Entry Number", "TEST4", wrapper.EntryNumber);

			wrapper = collection["CUS:ER:AAA"];
			AssertNotNull("Wrapper Indexer Works", wrapper);
			AssertEquals("Entry Number", "TEST5", wrapper.EntryNumber);

			wrapper = collection["ZZZ:ER:AAA"];
			AssertNull("Wrapper Indexer Works", wrapper);
		}

		public void TestLoadFromBusinessObject()
		{
			var businessObject = Factory.New<DummyBusinessObject>();
			var collection1 = new CustomsEntryWrapperCollection(businessObject, Factory);
			AssertEquals("collection1.Count", 0, collection1.Count);

			var booking = Factory.New<DtbBooking>();
			var entryNumber1 = booking.AdditionalReferenceNumbers.AddNew();
			var collection2 = new CustomsEntryWrapperCollection(booking, Factory);
			AssertEquals("collection2.Count", 1, collection2.Count);
		}

		public void TestCustomsEntryNumberSummary()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var collection = new CustomsEntryWrapperCollection(shipment, Factory);
			AssertEquals("Empty Summary", "", collection.CustomsEntryNumberSummary);

			var entryNumber1 = shipment.CusEntryNumbers.AddNew();
			entryNumber1.CE_EntryNum = "NUMBER1";
			entryNumber1.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber1.CE_ParentID = shipment.PK;

			collection = new CustomsEntryWrapperCollection(shipment, Factory);

			AssertEquals("Single Customs Entry Number", "NUMBER1", collection.CustomsEntryNumberSummary);

			var entryNumber2 = shipment.CusEntryNumbers.AddNew();
			entryNumber2.CE_EntryNum = "NUMBER2";
			entryNumber2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber2.CE_ParentID = shipment.PK;

			collection = new CustomsEntryWrapperCollection(shipment, Factory);

			AssertEquals("Two Customs Entry Numbers", "NUMBER1, NUMBER2", collection.CustomsEntryNumberSummary);
		}

		public void TestGetCustomsEntryNumberSummaryForType()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var entryNumber1 = shipment.CusEntryNumbers.AddNew();
			entryNumber1.CE_EntryNum = "NUMBER1";
			entryNumber1.CE_EntryType = "A";
			entryNumber1.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber1.CE_ParentID = shipment.PK;

			var entryNumber2 = shipment.CusEntryNumbers.AddNew();
			entryNumber2.CE_EntryNum = "NUMBER2";
			entryNumber2.CE_EntryType = "B";
			entryNumber2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber2.CE_ParentID = shipment.PK;

			var entryNumber3 = shipment.CusEntryNumbers.AddNew();
			entryNumber3.CE_EntryNum = "NUMBER3";
			entryNumber3.CE_EntryType = "B";
			entryNumber3.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber3.CE_ParentID = shipment.PK;

			var collection = new CustomsEntryWrapperCollection(shipment, Factory);

			AssertEquals("Should only show 'A' reference.", "NUMBER1", collection.GetCustomsEntryNumberSummaryForType("A"));
			AssertEquals("Should only show 'B' references.", "NUMBER2, NUMBER3", collection.GetCustomsEntryNumberSummaryForType("B"));
		}

		public void TestContainsType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var entryNumber = shipment.Numbers.AddNew();
			entryNumber.CE_EntryType = "XXX";
			entryNumber.CE_EntryNum = "123456";
			CustomsEntryWrapperCollection collection = new CustomsEntryWrapperCollection(shipment, Factory);
			Assert("Collection should contain code XXX", collection.ContainsType("XXX"));
			Assert("Collection should not contain code ZZZ", !collection.ContainsType("ZZZ"));
		}

		public void TestLoadFromShipment()
		{
			var aU = Core.Constants.CountryCodes.Australia;
			var uS = Core.Constants.CountryCodes.UnitedStates;

			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(uS);
			try
			{
				var shipment = Factory.New<ForwardingShipment>();
				AssertCusNumber(shipment, "Should be Empty");

				// From Shipment
				var num1 = AddCusNumber(shipment, "T1", "#1", uS);
				var num1a = AddCusNumber(shipment, "T1", "#1", uS);
				var num2 = AddCusNumber(shipment, "T2", "#2", aU);
				AssertCusNumber(shipment, "All from Shipment", num1, num1a);

				// From Declaration
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				AssertCusNumber(shipment, "All from Shipment", num1, num1a);

				var num3 = AddCusNumber(declaration, "T3", "#3", uS);
				var num4 = AddCusNumber(declaration, "T4", "#4", aU);
				AssertCusNumber(shipment, "All from Declaration + All from Shipment", num3, num4, num1, num1a);

				var num5 = AddCusNumber(declaration, "T1", "#5", aU);
				AssertCusNumber(shipment, "All from Declaration", num3, num4, num5);

				// From Declaration.CustomsEntryHeaders
				var num6 = AddCusNumber(declaration, "T6", "#6");
				AssertCusNumber(shipment, "All from Declaration.CustomsEntryHeaders + All from Shipment", num6, num1, num1a);

				var num7 = AddCusNumber(declaration, "T1", "#7");
				AssertCusNumber(shipment, "All from Declaration.CustomsEntryHeaders", num6, num7);

				// InbondTransitUS
				var typeIT = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_RL_NKLoadPort = "AUMEL";
				consol1.JK_RL_NKDischargePort = "USLAX";
				var num8 = AddCusNumber(consol1, typeIT, "#8", uS);

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_RL_NKLoadPort = "USLAX";
				consol2.JK_RL_NKDischargePort = "NZAKL";
				var num9 = AddCusNumber(consol2, typeIT, "#9", uS);

				var consol3 = shipment.Consols.AddNew();
				consol3.JK_RL_NKLoadPort = "NZAKL";
				consol3.JK_RL_NKDischargePort = "USNYC";
				var num10 = AddCusNumber(consol3, typeIT, "#10", uS);
				var num11 = AddCusNumber(consol3, typeIT, "#11", aU);

				var consol4 = shipment.Consols.AddNew();
				consol4.JK_RL_NKLoadPort = "USNYC";
				consol4.JK_RL_NKDischargePort = "USBOS";
				var num12 = AddCusNumber(consol4, typeIT, "#12", uS);

				AssertCusNumber(shipment, "All from Declaration.CustomsEntryHeaders + IT from Consol", num6, num7, num10);

				var num13 = AddCusNumber(shipment, typeIT, "#13", aU);
				AssertCusNumber(shipment, "All from Declaration.CustomsEntryHeaders + IT from Consol", num6, num7, num10);

				var num14 = AddCusNumber(shipment, typeIT, "#14", uS);
				AssertCusNumber(shipment, "All from Declaration.CustomsEntryHeaders + IT from Shipment", num6, num7, num14);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountry);
			}
		}

		public void TestShipmentEntryTypeWithoutNumberLikeEXLVShouldPrint_CS00149369()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				AssertEquals(ZString.Empty, shipment.CustomsEntryNumber);
				shipment.CustomsEntryNumberType = "XLV";
				AssertEquals("EXLV", shipment.ShipmentCustomsEntryNumber.EntryType);
				AssertEquals(ZString.Empty, shipment.ShipmentCustomsEntryNumber.EntryNumber);
				var collection = new CustomsEntryWrapperCollection(shipment, Factory);
				AssertEquals("Count", 1, collection.Count);
				AssertEquals("EntryType", "EXLV", collection[0].EntryType.Code);
				AssertEquals("EntryNumber", ZString.Empty, collection[0].EntryNumber);
			}
		}

		public void TestShipmentITNWithMergedExportDeclarationWithNoEntryNumber_CS00133448()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var entryNumber = shipment.Numbers.AddNew();
				entryNumber.CE_EntryType = "ITN";
				entryNumber.CE_EntryNum = "123456";
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = "ITN";//no entry number.

				var collection = new CustomsEntryWrapperCollection(shipment, Factory);
				AssertEquals("Count", 1, collection.Count);
				AssertEquals("EntryNumber", "123456", collection[0].EntryNumber);
				AssertEquals("123456", collection["CLEARANCE"].EntryNumber);

				entryNumber.Delete();
				entry.EntryNumber = "234567";
				collection = new CustomsEntryWrapperCollection(shipment, Factory);
				AssertEquals("Count", 1, collection.Count);
				AssertEquals("EntryNumber", "234567", collection[0].EntryNumber);
				AssertEquals("234567", collection["CLEARANCE"].EntryNumber);
			}
		}

		public void TestLoadFromDeclaration()
		{
			var aU = Core.Constants.CountryCodes.Australia;
			var uS = Core.Constants.CountryCodes.UnitedStates;
			var cA = Core.Constants.CountryCodes.Canada;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(uS))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertCusNumber(declaration, "Should be Empty");

				var shipment = Factory.New<ForwardingShipment>();
				var num1 = AddCusNumber(shipment, "T1", "#1", uS);
				var num1a = AddCusNumber(shipment, "T1", "#1", uS);
				var num2 = AddCusNumber(shipment, "T2", "#2", aU);
				declaration.JE_JS = shipment.PK;
				AssertCusNumber(declaration, "All from Shipment", num1, num1a);

				var num3 = AddCusNumber(declaration, "T3", "#3", uS);
				var num4 = AddCusNumber(declaration, "T4", "#4", aU);
				AssertCusNumber(declaration, "All from Declaration + All from Shipment", num3, num4, num1, num1a);

				var num5 = AddCusNumber(declaration, "T1", "#5", aU);
				var num5a = AddCusNumber(declaration, "T1", "#5", aU);
				AssertCusNumber(declaration, "All from Declaration", num3, num4, num5, num5a);

				// From Declaration.CustomsEntryHeaders
				var num6 = AddCusNumber(declaration, "T6", "#6");
				AssertCusNumber(declaration, "All from Declaration.CustomsEntryHeaders + All from Shipment", num6, num1, num1a);

				var num7 = AddCusNumber(declaration, "T1", "#7");
				AssertCusNumber(declaration, "All from Declaration.CustomsEntryHeaders", num6, num7);

				// InbondTransitUS
				var typeIT = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_RL_NKLoadPort = "AUMEL";
				consol1.JK_RL_NKDischargePort = "USLAX";
				var num8 = AddCusNumber(consol1, typeIT, "#8", uS);

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_RL_NKLoadPort = "USLAX";
				consol2.JK_RL_NKDischargePort = "NZAKL";
				var num9 = AddCusNumber(consol2, typeIT, "#9", uS);

				var consol3 = shipment.Consols.AddNew();
				consol3.JK_RL_NKLoadPort = "NZAKL";
				consol3.JK_RL_NKDischargePort = "USNYC";
				var num10 = AddCusNumber(consol3, typeIT, "#10", uS);
				var num11 = AddCusNumber(consol3, typeIT, "#11", aU);

				var consol4 = shipment.Consols.AddNew();
				consol4.JK_RL_NKLoadPort = "USNYC";
				consol4.JK_RL_NKDischargePort = "USBOS";
				var num12 = AddCusNumber(consol4, typeIT, "#12", uS);

				AssertCusNumber(declaration, "All from Declaration.CustomsEntryHeaders + IT from Consol", num6, num7, num10);

				var num13 = AddCusNumber(shipment, typeIT, "#13", aU);
				AssertCusNumber(declaration, "All from Declaration.CustomsEntryHeaders + IT from Consol", num6, num7, num10);

				var num14 = AddCusNumber(shipment, typeIT, "#14", uS);
				AssertCusNumber(declaration, "All from Declaration.CustomsEntryHeaders + IT from Shipment", num6, num7, num14);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(cA))
				{
					declaration.JE_JS = ZGuid.Empty;
					declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_JS = shipment.PK;
					declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
					num6 = AddCusNumber(declaration, "T6", "#6");
					AssertCusNumber(declaration, "No POR defined yet", num6);
					var num15 = AddCusNumber(shipment, CanadaAdditionalReferenceNumberTypes.Codes.CTN, "#15", cA);
					AssertCusNumber(declaration, "POR from shipment numbers for CA export", num15);
					var num16 = AddCusNumber(declaration, CanadaAdditionalReferenceNumberTypes.Codes.CTN, "#16", cA);
					AssertCusNumber(declaration, "POR from declaration for CA export", num16);
				}
			}
		}

		public void TestLoadFromDeclaration_AllFromDeclaration_AllFromShipmentExceptThoseFromDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var uS = Core.Constants.CountryCodes.UnitedStates;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(uS))
			{
				var shipmentNumber1 = AddCusNumber(shipment, "AAA", "#1", uS);
				var shipmentNumber2 = AddCusNumber(shipment, "AAA", "#2", uS);
				var shipmentNumber3 = AddCusNumber(shipment, "AAB", "#3", uS);
				var shipmentNumber4 = AddCusNumber(shipment, "AAC", "#4", uS);
				var shipmentNumber5 = AddCusNumber(shipment, "AAD", "#5", uS);
				var shipmentNumber6 = AddCusNumber(shipment, "AAE", "#6", uS);
				var shipmentNumber7 = AddCusNumber(shipment, "AAE", "#7", uS);

				var declarationNumber1 = AddCusNumber(declaration, "AAA", "#8", uS);
				var declarationNumber2 = AddCusNumber(declaration, "AAD", "#9", uS);
				var declarationNumber3 = AddCusNumber(declaration, "AAD", "#10", uS);

				var expectedNumbers = new[]
				{
					declarationNumber1,
					declarationNumber2,
					declarationNumber3,

					shipmentNumber3,
					shipmentNumber4,
					shipmentNumber6,
					shipmentNumber7,
				};
				AssertCusNumber(shipment, "All from declaration + All Shipments not in declaration", expectedNumbers);
			}
		}

		#region Implementation

		KeyValuePair<ZString, ZString> AddCusNumber(BaseJobDeclaration declaration, ZString type, ZString number)
		{
			var cusNumber = declaration.CustomsEntryHeaders.AddNew();
			cusNumber.CH_MessageType = type;
			cusNumber.EntryNumber = number;

			return new KeyValuePair<ZString, ZString>(type, number);
		}

		KeyValuePair<ZString, ZString> AddCusNumber(BusinessObject parent, ZString type, ZString number, string country = "")
		{
			var cusNumber = parent.Factory.New<CusEntryNumber>();
			cusNumber.CE_EntryType = type;
			cusNumber.CE_EntryNum = number;
			cusNumber.CE_ParentID = parent.PK;
			cusNumber.CE_RN_NKCountryCode = country;

			return new KeyValuePair<ZString, ZString>(type, number);
		}

		void AssertCusNumber(CommonShipment shipment, ZString message, params KeyValuePair<ZString, ZString>[] expected)
		{
			AssertCusNumber(new CustomsEntryWrapperCollection(shipment, shipment.Factory), message, expected);
		}

		void AssertCusNumber(CommonConsol consol, ZString message, params KeyValuePair<ZString, ZString>[] expected)
		{
			AssertCusNumber(new CustomsEntryWrapperCollection(consol, consol.Factory), message, expected);
		}

		void AssertCusNumber(BaseJobDeclaration declaration, ZString message, params KeyValuePair<ZString, ZString>[] expected)
		{
			AssertCusNumber(new CustomsEntryWrapperCollection(declaration, declaration.Factory), message, expected);
		}

		void AssertCusNumber(WhsItemDispatchConsignment dispatchConsignment, ZString message, params KeyValuePair<ZString, ZString>[] expected)
		{
			AssertCusNumber(new CustomsEntryWrapperCollection(dispatchConsignment, dispatchConsignment.Factory), message, expected);
		}

		void AssertCusNumber(CustomsEntryWrapperCollection collection, ZString message, params KeyValuePair<ZString, ZString>[] expected)
		{
			AssertContainsExactElementsInAnyOrder
			(
				message,
				Array.ConvertAll(expected, (x) => ZString.Format("{0} - {1}", x.Key, x.Value)),
				Array.ConvertAll(collection.ToArray<CustomsEntryWrapper>(), (x) => ZString.Format("{0} - {1}", x.EntryType.Code, x.EntryNumber))
			);
		}

		protected override CustomsEntryWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new CustomsEntryWrapperCollection((BaseJobDeclaration)null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new CustomsEntryWrapperFromCusEntryHeader(null, Factory);
		}

		#endregion
	}
}
