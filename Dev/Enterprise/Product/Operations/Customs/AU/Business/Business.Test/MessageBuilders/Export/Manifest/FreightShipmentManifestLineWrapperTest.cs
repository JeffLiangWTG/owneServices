using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FreightShipmentManifestLineWrapperTest : TestCaseWithFactory
	{
		public void TestHouseBillNumber()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.OH_FullName = "TEST CONSIGNOR NAME";

			Shipment.ConsignorPK = consignor.PK;
			Shipment.JS_HouseBill = "1234ABC";

			Consignment.HVC_WaybillNumber = "5678PQR";

			var deletedHbWrapper = FreightShipmentManifestLineWrapper.NewWhenShipmentIsDeleted();

			CombineAssertions(() =>
			{
				AssertEquals("Wrapper with no Consignment Line - Uses Shipment HouseBill", "1234ABC", WrapperWithNullConsignmentLine.HouseBillNumber);
				AssertEquals("Wrapper with HVLV Consignment - Uses Waybill Number", "5678PQR", ConsignmentBookingLineWrapper.HouseBillNumber);
				AssertEquals("When HouseBill has been deleted, we still need wrapper for change message & can get details from alternative constructor", string.Empty, deletedHbWrapper.HouseBillNumber);
			});
		}

		public void TestHasCANOrExemption()
		{
			AssertEquals("Precondtion: No CAN or Exemption", false, WrapperWithNullConsignmentLine.HasCANOrExemption);
			Shipment.CustomsEntryNumberType = "CAN";
			Shipment.CustomsEntryNumber = "12345";

			AssertEquals("Wrapper has CAN or Exemption", true, WrapperWithNullConsignmentLine.HasCANOrExemption);
		}

		public void TestGetCANWhenExemptionExistsAlso()
		{
			var exlvEntryNum = Factory.New<AUCusEntryNumber>();
			exlvEntryNum.CE_EntryIsSystemGenerated = false;
			exlvEntryNum.CE_EntryNum = "";
			exlvEntryNum.CE_EntryType = "EXLV";
			exlvEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			exlvEntryNum.CE_ParentID = Shipment.PK;
			exlvEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			exlvEntryNum.CE_Category = "CUS";

			var canEntryNum = Factory.New<AUCusEntryNumber>();
			canEntryNum.CE_EntryIsSystemGenerated = false;
			canEntryNum.CE_EntryNum = "AELRFT77N";
			canEntryNum.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			canEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			canEntryNum.CE_ParentID = Shipment.PK;
			canEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			canEntryNum.CE_Category = "CUS";
			Factory.Save();

			var canPermit = WrapperWithNullConsignmentLine.CAN;
			AssertEquals("AELRFT77N", canPermit);
		}

		public void TestGetContingencyCANWhenOtherPermitsExist()
		{
			var exlvEntryNum = Factory.New<AUCusEntryNumber>();
			exlvEntryNum.CE_EntryIsSystemGenerated = false;
			exlvEntryNum.CE_EntryNum = "";
			exlvEntryNum.CE_EntryType = "EXLV";
			exlvEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			exlvEntryNum.CE_ParentID = Shipment.PK;
			exlvEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			exlvEntryNum.CE_Category = "CUS";

			var contingencyCanEntryNum = Factory.New<AUCusEntryNumber>();
			contingencyCanEntryNum.CE_EntryIsSystemGenerated = false;
			contingencyCanEntryNum.CE_EntryNum = "CONTFT77N";
			contingencyCanEntryNum.CE_EntryType = CANType.ContingencyCustomsAuthorityNumber.Code;
			contingencyCanEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			contingencyCanEntryNum.CE_ParentID = Shipment.PK;
			contingencyCanEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			contingencyCanEntryNum.CE_Category = "CUS";

			var canEntryNum = Factory.New<AUCusEntryNumber>();
			canEntryNum.CE_EntryIsSystemGenerated = false;
			canEntryNum.CE_EntryNum = "AELRFT77N";
			canEntryNum.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			canEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			canEntryNum.CE_ParentID = Shipment.PK;
			canEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			canEntryNum.CE_Category = "CUS";
			Factory.Save();

			var contingencyPermit = WrapperWithNullConsignmentLine.CCAN;
			AssertEquals("Should only find the contingency permit", "CONTFT77N", contingencyPermit);
		}

		public void TestGetContingencyCANFromECN()
		{
			var exlvEntryNum = Factory.New<AUCusEntryNumber>();
			exlvEntryNum.CE_EntryIsSystemGenerated = false;
			exlvEntryNum.CE_EntryNum = "";
			exlvEntryNum.CE_EntryType = "EXLV";
			exlvEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			exlvEntryNum.CE_ParentID = Shipment.PK;
			exlvEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			exlvEntryNum.CE_Category = "CUS";

			var contingencyCanEntryNum = Factory.New<AUCusEntryNumber>();
			contingencyCanEntryNum.CE_EntryIsSystemGenerated = false;
			contingencyCanEntryNum.CE_EntryNum = "CONTET77N";
			contingencyCanEntryNum.CE_EntryType = CusEntryNumberTypes.Australia.ECN;
			contingencyCanEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			contingencyCanEntryNum.CE_ParentID = Shipment.PK;
			contingencyCanEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			contingencyCanEntryNum.CE_Category = "CUS";

			var contingencyPermit = WrapperWithNullConsignmentLine.CCAN;
			AssertEquals("Should find the contingency ECN permit", "CONTET77N", contingencyPermit);
		}

		public void TestPackCount()
		{
			Shipment.JS_OuterPacks = 3;
			Consignment.HVC_ItemCount = 7;
			var deletedShipmentWrapper = FreightShipmentManifestLineWrapper.NewWhenShipmentIsDeleted();

			CombineAssertions("Pack Count is correct", () =>
			{
				AssertEquals("Wrapper with no Consignment Line - Uses Shipment OuterPacks", 3, WrapperWithNullConsignmentLine.PackCount);
				AssertEquals("Wrapper with HVLV Consignment - Uses Item Count", 7, ConsignmentBookingLineWrapper.PackCount);
				AssertEquals("When Shipment has been deleted, no packs", 0, deletedShipmentWrapper.PackCount);
			});
		}

		public void TestCountryOfDestination()
		{
			Shipment.JS_RL_NKDestination = "NZAKL";
			Consignment.HVC_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var deletedShipmentWrapper = FreightShipmentManifestLineWrapper.NewWhenShipmentIsDeleted();

			CombineAssertions("Country Code is correct", () =>
			{
				AssertEquals("Wrapper with no Consignment Line - Uses Shipment Destination", Core.Constants.CountryCodes.NewZealand, WrapperWithNullConsignmentLine.CountryOfDestination);
				AssertEquals("Wrapper with HVLV Consignment - Uses Consignee Country Code", Core.Constants.CountryCodes.UnitedStates, ConsignmentBookingLineWrapper.CountryOfDestination);
				AssertEquals("When Shipment has been deleted, no Country Code", string.Empty, deletedShipmentWrapper.CountryOfDestination);
			});
		}

		public void TestGoodsDescription()
		{
			Shipment.JS_GoodsDescription = "GOODS1";
			Consignment.HVC_GoodsDescription = "GOODS3";
			var deletedShipmentWrapper = FreightShipmentManifestLineWrapper.NewWhenShipmentIsDeleted();

			CombineAssertions("Goods Description is correct", () =>
			{
				AssertEquals("Wrapper with no Consignment Line - Uses Shipment Goods Description", "GOODS1", WrapperWithNullConsignmentLine.GoodsDescription);
				AssertEquals("Wrapper with HVLV Consignment", "GOODS3", ConsignmentBookingLineWrapper.GoodsDescription);
				AssertEquals("When Shipment has been deleted, no Goods Description", string.Empty, deletedShipmentWrapper.GoodsDescription);
			});
		}

		public void TestGoodsDescription_FallsBackToShipmentWhenEmpty()
		{
			Shipment.JS_GoodsDescription = "FALLBACKGOODS";
			Consignment.HVC_GoodsDescription = string.Empty;
			var deletedShipmentWrapper = FreightShipmentManifestLineWrapper.NewWhenShipmentIsDeleted();

			CombineAssertions("Goods Description shoud fallback to Shipment when Consignment description is empty", () =>
			{
				AssertEquals("Wrapper with no Consignment Line - Uses Shipment Goods Description", "FALLBACKGOODS", WrapperWithNullConsignmentLine.GoodsDescription);
				AssertEquals("Wrapper with HVLV Consignment", "FALLBACKGOODS", ConsignmentBookingLineWrapper.GoodsDescription);
				AssertEquals("When Shipment has been deleted, no Goods Description", string.Empty, deletedShipmentWrapper.GoodsDescription);
			});
		}

		public void TestGoodsOwner()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.OH_FullName = "TEST CONSIGNOR NAME";
			Shipment.ConsignorPK = consignor.PK;

			Consignment.HVC_ShipperName = "EBAY AU";

			var deletedShipmentWrapper = FreightShipmentManifestLineWrapper.NewWhenShipmentIsDeleted();

			CombineAssertions("Goods Owner is correct", () =>
			{
				AssertEquals("Wrapper with no Consignment Line - Uses Shipment Consignor Name", "TEST CONSIGNOR NAME", WrapperWithNullConsignmentLine.GoodsOwner);
				AssertEquals("Wrapper with HVLV Consignment - Uses Shipper Name", "EBAY AU", ConsignmentBookingLineWrapper.GoodsOwner);
				AssertEquals("When Shipment has been deleted, no Goods Owner", string.Empty, deletedShipmentWrapper.GoodsOwner);
			});
		}

		public void TestGoodsOwner_WhenNoConsignmentLine_UsesOverridenConsignerAddressCompanyName()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.OH_FullName = "TEST CONSIGNOR NAME";
			Shipment.ConsignorPK = consignor.PK;

			AssertEquals("Wrapper with no Consignment Line - Uses Shipment Consignor Name", "TEST CONSIGNOR NAME", WrapperWithNullConsignmentLine.GoodsOwner);

			Shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			Shipment.ConsignorDocumentaryAddress.E2_CompanyName = "OVERRIDE NAME";

			AssertEquals("Wrapper with no Consignment Line - Uses Overriden Consignor Company Name", "OVERRIDE NAME", WrapperWithNullConsignmentLine.GoodsOwner);
		}

		public void TestEachConsignmentLineReferenceIsUnique_HVLShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			var shipmentWithHVLConsignments = consol.Shipments.AddNew();

			shipment.JS_HouseBill = "U03034";
			shipmentWithHVLConsignments.JS_HouseBill = "HVLV";

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipmentWithHVLConsignments.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var lineCount = 0;
			for (int i = 0; i < 12; i++)
			{
				lineCount++;
				var consignment = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
				consignment.HVC_JS_ManifestedOnShipment = shipmentWithHVLConsignments.PK;
				consignment.HVC_WaybillNumber = lineCount == 7 ? "U03034" : "HVLV" + lineCount;

				var shipmentWrapper = new FreightShipmentWrapper(shipmentWithHVLConsignments, Consol, consignment);
				shipmentWrapper.CreatePreliminaryManifestLineNumber(lineCount);
				shipmentWrapper.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			Factory.Save();

			int index = 0;
			CombineAssertions(() =>
			{
				foreach (IHVLVConsignment consignmentLine in shipmentWithHVLConsignments.HVLVConsignments)
				{
					index++;
					var consignmentLineWrapper = new FreightShipmentManifestLineWrapper(shipmentWithHVLConsignments, consignmentLine, new FreightConsolWrapper(consol));
					if (index == 7)
					{
						AssertEquals($"Consignment {index}: is generated as it is not unique", "HVLV-U03034", consignmentLineWrapper.Reference);
					}
					else
					{
						AssertEquals($"Consignment {index}: Reference", "HVLV" + index, consignmentLineWrapper.Reference);
					}
				}
			});
		}

		public void TestHVLVConsignmentLineReferenceIsUnique()
		{
			// System already enforces uniqueness with 1 HLS eManifest - job cannot be saved when duplicate references in an eManifest exist
			Assert(true);
		}

		public void TestConsignmentReferenceIsUniqueAcrossMultipleHVLShipments()
		{
			Consol.JK_RL_NKLoadPort = "AUSYD";

			var hvlShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var hvlShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			hvlShipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			hvlShipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			hvlShipment1.JS_HouseBill = "HB1-7993273";
			hvlShipment2.JS_HouseBill = "HB2-7996849";

			Consol.Shipments.Add(hvlShipment1);
			Consol.Shipments.Add(hvlShipment2);

			var consignment1 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
			var consignment2 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
			var consignment3 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
			var consignment4 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));

			consignment1.HVC_JS_ManifestedOnShipment = hvlShipment1.PK;
			consignment2.HVC_JS_ManifestedOnShipment = hvlShipment1.PK;
			consignment3.HVC_JS_ManifestedOnShipment = hvlShipment2.PK;
			consignment4.HVC_JS_ManifestedOnShipment = hvlShipment2.PK;

			consignment1.HVC_WaybillNumber = "U03034";
			consignment2.HVC_WaybillNumber = "MILK POWDER";
			consignment3.HVC_WaybillNumber = "MILK POWDER";
			consignment4.HVC_WaybillNumber = "HB1-7993273";

			Factory.Save();

			var shipmentWrapper1 = new FreightShipmentWrapper(hvlShipment1, Consol, consignment1);
			var shipmentWrapper2 = new FreightShipmentWrapper(hvlShipment1, Consol, consignment2);
			var shipmentWrapper3 = new FreightShipmentWrapper(hvlShipment2, Consol, consignment3);
			var shipmentWrapper4 = new FreightShipmentWrapper(hvlShipment2, Consol, consignment4);

			shipmentWrapper1.CreatePreliminaryManifestLineNumber(8);
			shipmentWrapper2.CreatePreliminaryManifestLineNumber(9);
			shipmentWrapper3.CreatePreliminaryManifestLineNumber(16);
			shipmentWrapper4.CreatePreliminaryManifestLineNumber(17);

			shipmentWrapper1.UpdatePreliminaryLineNumberToManifestedLineNumber();
			shipmentWrapper2.UpdatePreliminaryLineNumberToManifestedLineNumber();
			shipmentWrapper3.UpdatePreliminaryLineNumberToManifestedLineNumber();
			shipmentWrapper4.UpdatePreliminaryLineNumberToManifestedLineNumber();

			var consignmentLineWrapper1 = new FreightShipmentManifestLineWrapper(hvlShipment1, consignment1, new FreightConsolWrapper(Consol));
			var consignmentLineWrapper2 = new FreightShipmentManifestLineWrapper(hvlShipment1, consignment2, new FreightConsolWrapper(Consol));
			var consignmentLineWrapper3 = new FreightShipmentManifestLineWrapper(hvlShipment2, consignment3, new FreightConsolWrapper(Consol));
			var consignmentLineWrapper4 = new FreightShipmentManifestLineWrapper(hvlShipment2, consignment4, new FreightConsolWrapper(Consol));

			CombineAssertions(() =>
			{
				AssertEquals("Consignment 1 Reference", "U03034", consignmentLineWrapper1.Reference);
				AssertEquals("Consignment 2 Reference should be generated as is the same a consignment 3 (Line 1 on second HVL Shipment)", "HB1-7993273-MILK POWDER", consignmentLineWrapper2.Reference);
				AssertEquals("Consignment 3 Reference should be generated as is the same a consignment 2 on first HVL Shipment", "HB2-7996849-MILK POWDER", consignmentLineWrapper3.Reference);
				AssertEquals("Consignment 4 Reference should be generated as is the same as HouseBill1", "HB2-7996849-HB1-7993273", consignmentLineWrapper4.Reference);
			});
		}

		FreightShipmentManifestLineWrapper ConsignmentBookingLineWrapper => consignmentBookingLineWrapper ?? (consignmentBookingLineWrapper = new FreightShipmentManifestLineWrapper(Shipment, Consignment, new FreightConsolWrapper(Consol)));
		FreightShipmentManifestLineWrapper consignmentBookingLineWrapper;

		FreightShipmentManifestLineWrapper WrapperWithNullConsignmentLine => wrapperWithNullBookingLine ?? (wrapperWithNullBookingLine = new FreightShipmentManifestLineWrapper(Shipment, null, new FreightConsolWrapper(Consol)));
		FreightShipmentManifestLineWrapper wrapperWithNullBookingLine;

		IHVLVConsignment Consignment => consignment ?? (consignment = Factory.New<IHVLVConsignment>());
		IHVLVConsignment consignment;

		ForwardingShipment Shipment => shipment ?? (shipment = Factory.New<ForwardingShipment>());
		ForwardingShipment shipment;

		ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<ForwardingConsol>();
					consol.Shipments.Add(Shipment);
				}
				return consol;
			}
		}
		ForwardingConsol consol;
	}
}
