using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocAWB))]
	sealed class DocAWBTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocAWB.New(Shipment.AWBHeader, Factory),
			};
		}

		public void TestTSASecurityStatementCountriesText()
		{
			var wrapper = (DocAWB)GetDocumentWrappers().First();

			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Australia }))
			{
				AssertEquals("Australia", wrapper.TSASecurityStatementCountriesText);
			}

			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Australia, Core.Constants.CountryGuids.Austria }))
			{
				AssertEquals("Australia, Austria", wrapper.TSASecurityStatementCountriesText);
			}

			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>()))
			{
				AssertEquals("", wrapper.TSASecurityStatementCountriesText);
			}
		}

		public void TestSpanishTitle()
		{
			Shipment.Consols.Add(CreateConsol());
			ExportAWBHeader header = Shipment.Consols[0].AWBHeader;
			DocAWB awb = DocAWB.New(header, Factory);
			AssertEquals("LanguageOfTitle", ZString.Empty, awb.LanguageOfTitle);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.LanguageOfTitle, "Spanish");

			((IBODocDataProvider)awb).SetDocWrapperContext(constants);
			awb = DocAWB.New(header, Factory);
			AssertEquals("LanguageOfTitle", "Spanish", awb.LanguageOfTitle);

			Shipment.Consols[0].JK_AgentType = Core.Constants.AgentType.Direct;
			awb = DocAWB.New(header, Factory);
			var stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.AWBSpanishMasterTitle.png");
			var expectedImage = new Bitmap(stream);
			AssertImageEquals("SpanishMasterHouseTitle", expectedImage, awb.IssuedByImage);

			Shipment.Consols[0].JK_AgentType = Core.Constants.AgentType.CoLoad;
			awb = DocAWB.New(header, Factory);
			stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.AWBSpanishMasterHouseTitle.png");
			expectedImage = new Bitmap(stream);
			AssertImageEquals("SpanishMasterHouseTitle", expectedImage, awb.IssuedByImage);
		}

		public void TestGetCustomField()
		{
			Shipment.SetUserDefinedValue("Custom1", (ZString)"Hello World");
			var awb = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("Hello World", awb.GetCustomField("Custom1"));
		}

		public void TestFrenchTitle()
		{
			Shipment.Consols.Add(CreateConsol());
			ExportAWBHeader header = Shipment.Consols[0].AWBHeader;
			DocAWB awb = DocAWB.New(header, Factory);
			AssertEquals("LanguageOfTitle", ZString.Empty, awb.LanguageOfTitle);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.LanguageOfTitle, "French");

			((IBODocDataProvider)awb).SetDocWrapperContext(constants);
			awb = DocAWB.New(header, Factory);
			AssertEquals(DocumentEngineIntegration.Constants.TemplateDefined.LanguageOfTitle, "French", awb.LanguageOfTitle);

			Shipment.Consols[0].JK_AgentType = Core.Constants.AgentType.Direct;
			awb = DocAWB.New(header, Factory);
			var stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.AWBFrenchMasterTitle.png");
			var expectedImage = new Bitmap(stream);
			AssertImageEquals("FrenchMasterHouseTitle", expectedImage, awb.IssuedByImage);

			Shipment.Consols[0].JK_AgentType = Core.Constants.AgentType.CoLoad;
			awb = DocAWB.New(header, Factory);
			stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.AWBFrenchMasterHouseTitle.png");
			expectedImage = new Bitmap(stream);
			AssertImageEquals("FrenchMasterHouseTitle", expectedImage, awb.IssuedByImage);
		}

		public void TestNullIssuedByImage()
		{
			Shipment.Consols.Add(CreateConsol());
			ExportAWBHeader header = Shipment.Consols[0].AWBHeader;
			DocAWB awb = DocAWB.New(header, Factory);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.LanguageOfTitle, "XXX");
			((IBODocDataProvider)awb).SetDocWrapperContext(constants);

			AssertNull("does not throw excepton when resource cannot be found", awb.IssuedByImage);
		}

		public void TestNullAWBHeader()
		{
			AssertNull("It is possible for NEW to return a null", DocAWB.New(null, null));
		}

		public void TestIssuedByImage()
		{
			ExportAWBHeader header = Shipment.AWBHeader;
			DocAWB awb = DocAWB.New(header, Factory);
			Image houseImage = awb.IssuedByImage;
			AssertNotNull("When HAWB logo is NOT specified, Issued by image should be retrieved for HOUSE", houseImage);

			var stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.AWBHouseTitle.png");
			Env.Registry.Freight.AirWaybill.HAWBLogo = Image.FromStream(stream);

			awb = DocAWB.New(header, Factory);
			AssertNull("When HAWB logo is specified, Issued by image should be null", awb.IssuedByImage);

			Shipment.Consols.Add(CreateConsol());
			header = Shipment.Consols[0].AWBHeader;
			awb = DocAWB.New(header, Factory);
			Image masterImage = awb.IssuedByImage;
			AssertNotNull("Issued by image should be retrieved for MASTER", masterImage);
			AssertNotEquals("Different image", masterImage, houseImage);

			Shipment.Consols[0].JK_AgentType = Core.Constants.AgentType.CoLoad;
			awb = DocAWB.New(header, Factory);
			Image masterHouseImage = awb.IssuedByImage;
			AssertNotNull("Issued by image should be retrieved for MASTER HOUSE", masterHouseImage);
			AssertNotEquals("Different image", masterHouseImage, houseImage);
			AssertNotEquals("Different image", masterHouseImage, masterImage);
		}

		public void TestSignatureImageForHAWB()
		{
			var stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.Signature.png");
			var image = Image.FromStream(stream);
			FreightDataRegistry.Instance.PrintSignatureForHAWBDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var staff = GlbStaff.CurrentUser;

			ExportAWBHeader header = Shipment.AWBHeader;
			DocAWB awb = DocAWB.New(header, Factory);

			var documentUsageReporter = new DocumentUsageReporter();
			Factory.ServiceContainer.AddService(new DocumentUsageDetailsCollector(documentUsageReporter));

			AssertNull("Current user has no signature image.", awb.SignatureImage);
			AssertEquals("IsUserSignatureUsed", false, documentUsageReporter.IsUserSignatureUsed);

			staff.SignatureImage = image;

			awb = DocAWB.New(header, Factory);
			AssertNotNull("Current user signature image.", awb.SignatureImage);
			AssertNotNull("SignatureImage", awb.SignatureImage);
			Assert("IsUserSignatureUsed", documentUsageReporter.IsUserSignatureUsed);

			FreightDataRegistry.Instance.PrintSignatureForHAWBDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			awb = DocAWB.New(header, Factory);

			AssertNull("PrintSignatureForHAWBDocuments registry is set to false.", awb.SignatureImage);
			Assert("Previously collected IsUserSignatureUsed", documentUsageReporter.IsUserSignatureUsed);
		}

		public void TestSignatureImageForMAWB()
		{
			var stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.Signature.png");
			var image = Image.FromStream(stream);
			FreightDataRegistry.Instance.PrintSignatureForMAWBDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var staff = GlbStaff.CurrentUser;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var mawb = DocAWB.New(consol.AWBHeader, Factory);
			AssertNull("Current user has no signature image.", mawb.SignatureImage);

			staff.SignatureImage = image;

			mawb = DocAWB.New(consol.AWBHeader, Factory);
			AssertNull("DocAWB is not house bill", mawb.SignatureImage);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			mawb = DocAWB.New(consol.AWBHeader, Factory);
			AssertNotNull("Current user signature image.", mawb.SignatureImage);

			FreightDataRegistry.Instance.PrintSignatureForMAWBDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			mawb = DocAWB.New(consol.AWBHeader, Factory);
			AssertNull("PrintSignatureForMAWBDocuments registry is set to false.", mawb.SignatureImage);
		}

		public void TestRelatedObjectsCorrectType()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			DocAWB aWB = DocAWB.New(aWBHeader, Factory);
			AssertEquals(typeof(DocForwardingShipment), aWB.DocShipment.GetType());

			Shipment.Consols.Add(CreateConsol());
			aWBHeader = Shipment.Consols[0].AWBHeader;
			aWB = DocAWB.New(aWBHeader, Factory);
			AssertEquals(typeof(DocForwardingConsol), aWB.DocConsol.GetType());
		}

		public void TestUseChargeSet2CantAccessErrorReporter()
		{
			try
			{
				var aWBHeader = Shipment.AWBHeader;
				var aWB = DocAWB.New(aWBHeader, Factory);
				aWB.ForceUseChargeSet2IssueReporting = true;
				var useChargeSet2 = aWB.UseChargeSet2;
			}
			catch (Exception)
			{
				AssertEquals(true, ErrorReporter.LastKeyReported.Contains("WI00064568 International Logistics"));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestCODFeePlusChargesAndDescriptionsForDirectShipment()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertCODFeeChargesAndDescriptions();

			Shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Assert(Shipment.IsPrepaid);
			Shipment.Consols.Add(CreateConsol());
			AssertCODFeeChargesAndDescriptions();

			Shipment.Consols[0].JK_AgentType = Core.Constants.AgentType.Direct;
			Assert(aWBHeader.Consol.IsDirect);
			AssertCODFeeChargesAndDescriptions();

			AccChargeCode pickupCharge = Factory.New<AccChargeCode>();
			pickupCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Loading;
			AccChargeCode deliveryCharge = Factory.New<AccChargeCode>();
			deliveryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Unloading;
			AccChargeCode originCharge = Factory.New<AccChargeCode>();
			originCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			AccChargeCode destinationCharge = Factory.New<AccChargeCode>();
			destinationCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			AccChargeCode cODCharge = Factory.New<AccChargeCode>();
			cODCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			cODCharge.AC_ChargeSubGroup = ChargeCodeSubGroupList.Cod;
			AccChargeCode otherCharge = Factory.New<AccChargeCode>();
			otherCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSLoadList;

			JobCharge pickup1 = Factory.New<JobCharge>();
			pickup1.JR_AC = pickupCharge.PK;
			pickup1.JR_LocalSellAmt = 1m;
			JobCharge pickup2 = Factory.New<JobCharge>();
			pickup2.JR_AC = pickupCharge.PK;
			pickup2.JR_LocalSellAmt = 10m;

			JobCharge delivery1 = Factory.New<JobCharge>();
			delivery1.JR_AC = deliveryCharge.PK;
			delivery1.JR_LocalSellAmt = 20m;
			JobCharge delivery2 = Factory.New<JobCharge>();
			delivery2.JR_AC = deliveryCharge.PK;
			delivery2.JR_LocalSellAmt = 100m;

			JobCharge origin1 = Factory.New<JobCharge>();
			origin1.JR_AC = originCharge.PK;
			origin1.JR_LocalSellAmt = 300m;
			JobCharge origin2 = Factory.New<JobCharge>();
			origin2.JR_AC = originCharge.PK;
			origin2.JR_LocalSellAmt = 1000m;
			origin2.JR_Desc = "Origin2";

			JobCharge destination1 = Factory.New<JobCharge>();
			destination1.JR_AC = destinationCharge.PK;
			destination1.JR_LocalSellAmt = 4000m;
			JobCharge destination2 = Factory.New<JobCharge>();
			destination2.JR_AC = destinationCharge.PK;
			destination2.JR_LocalSellAmt = 10000m;
			destination2.JR_Desc = "Destination2";

			JobCharge cOD1 = Factory.New<JobCharge>();
			cOD1.JR_AC = cODCharge.PK;
			cOD1.JR_LocalSellAmt = 50000m;
			JobCharge cOD2 = Factory.New<JobCharge>();
			cOD2.JR_AC = cODCharge.PK;
			cOD2.JR_LocalSellAmt = 100000m;

			JobCharge other1 = Factory.New<JobCharge>();
			other1.JR_AC = otherCharge.PK;
			other1.JR_LocalSellAmt = 500000m;
			JobCharge other2 = Factory.New<JobCharge>();
			other2.JR_AC = otherCharge.PK;
			other2.JR_LocalSellAmt = 1000000m;
			other2.JR_Desc = "Other2";

			AssertCODFeeChargesAndDescriptions();

			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = Shipment.PK;
			AssertEquals(Shipment.ShipmentJobHeader.PK, header.PK);

			pickup1.JR_JH = header.PK;
			pickup2.JR_JH = header.PK;
			delivery1.JR_JH = header.PK;
			delivery2.JR_JH = header.PK;
			origin1.JR_JH = header.PK;
			origin2.JR_JH = header.PK;
			destination1.JR_JH = header.PK;
			destination2.JR_JH = header.PK;
			cOD1.JR_JH = header.PK;
			cOD2.JR_JH = header.PK;
			other1.JR_JH = header.PK;
			other2.JR_JH = header.PK;

			AssertEquals(11m, AWB.PickupChargesForDirectShipment);
			AssertEquals(120m, AWB.DeliveryChargesForDirectShipment);
			AssertEquals(1300m, AWB.OriginAdvancedChargesForDirectShipment);
			AssertEquals("Miscellaneous", AWB.DescriptionOfOriginAdvance);
			AssertEquals("Miscellaneous", AWB.DescriptionOfDestinationAdvance);
			AssertEquals(14000m, AWB.DestinationAdvancedChargesForDirectShipment);
			AssertEquals("Miscellaneous", AWB.OtherChargesDescriptionForDirectShipment);
			AssertEquals(150000m, AWB.CODFeeForDirectShipment);

			origin1.JR_JH = ZGuid.Empty;
			destination1.JR_JH = ZGuid.Empty;
			other1.JR_JH = ZGuid.Empty;

			AWB.DebugOnlySetChargesForDirectShipmentToNull();

			AssertEquals(11m, AWB.PickupChargesForDirectShipment);
			AssertEquals(120m, AWB.DeliveryChargesForDirectShipment);
			AssertEquals(1000m, AWB.OriginAdvancedChargesForDirectShipment);
			AssertEquals("Origin2", AWB.DescriptionOfOriginAdvance);
			AssertEquals("Destination2", AWB.DescriptionOfDestinationAdvance);
			AssertEquals(10000m, AWB.DestinationAdvancedChargesForDirectShipment);
			AssertEquals("Other2", AWB.OtherChargesDescriptionForDirectShipment);
			AssertEquals(150000m, AWB.CODFeeForDirectShipment);

			JobChargeCollectionTest.AssertNoJobChargesContainedInJobChargeCollection(Factory);
		}

		void AssertCODFeeChargesAndDescriptions()
		{
			AssertEquals(0m, AWB.CODFeeForDirectShipment);
			AssertEquals(0m, AWB.PickupChargesForDirectShipment);
			AssertEquals(0m, AWB.DeliveryChargesForDirectShipment);
			AssertEquals(0m, AWB.OriginAdvancedChargesForDirectShipment);
			AssertEquals(ZString.Empty, AWB.DescriptionOfOriginAdvance);
			AssertEquals(ZString.Empty, AWB.DescriptionOfDestinationAdvance);
			AssertEquals(0m, AWB.DestinationAdvancedChargesForDirectShipment);
			AssertEquals(ZString.Empty, AWB.OtherChargesDescriptionForDirectShipment);

			JobChargeCollectionTest.AssertNoJobChargesContainedInJobChargeCollection(Factory);
		}

		public void TestShippersCODForDirectShipment()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(0m, AWB.ShippersCODForDirectShipment);

			Shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Assert(Shipment.IsPrepaid);
			Shipment.Consols.Add(CreateConsol());
			AssertEquals(0m, AWB.ShippersCODForDirectShipment);

			Shipment.Consols[0].JK_AgentType = Core.Constants.AgentType.Direct;
			Assert(aWBHeader.Consol.IsDirect);
			AssertEquals(0m, AWB.ShippersCODForDirectShipment);

			Shipment.JS_ShipperCODAmount = 10m;
			AssertEquals(0m, AWB.ShippersCODForDirectShipment);

			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals(10m, AWB.ShippersCODForDirectShipment);
		}

		public void TestItemsPrepaid()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(0, AWB.ItemsPrepaid);

			Shipment.Consols.Add(CreateConsol());
			Shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Assert(Shipment.IsPrepaid);
			AssertNotNull(aWBHeader.Consol);
			AssertEquals(0, AWB.ItemsPrepaid);

			Shipment.JS_OuterPacks = 2;
			AssertEquals(2, AWB.ItemsPrepaid);

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.Consols.Add(Shipment.Consols[0]);
			shipment1.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Assert(shipment1.IsCollect);
			AssertEquals(2, AWB.ItemsPrepaid);

			shipment1.JS_OuterPacks = 1;
			AssertEquals(2, AWB.ItemsPrepaid);

			shipment1.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Assert(shipment1.IsPrepaid);
			AssertEquals(3, AWB.ItemsPrepaid);
		}

		public void TestItemsCollect()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(0, AWB.ItemsCollect);

			Shipment.Consols.Add(CreateConsol());
			Shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			Assert(Shipment.IsPrepaid);
			AssertNotNull(aWBHeader.Consol);
			AssertEquals(0, AWB.ItemsCollect);

			Shipment.JS_OuterPacks = 2;
			AssertEquals(0, AWB.ItemsCollect);

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.Consols.Add(Shipment.Consols[0]);
			shipment1.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Assert(shipment1.IsCollect);
			AssertEquals(0, AWB.ItemsCollect);

			shipment1.JS_OuterPacks = 1;
			AssertEquals(1, AWB.ItemsCollect);

			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Assert(Shipment.IsCollect);
			AssertEquals(3, AWB.ItemsCollect);
		}

		public void TestIsCopy()
		{
			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			aWBWrapper.SetReportNameForTesting("Test Copy");
			AssertEquals("should be a copy - has 'Copy' in report name text ", true, aWBWrapper.IsCopy);

			aWBWrapper.SetReportNameForTesting("Test Report Name");
			AssertEquals("Shouldn't be a copy - doesn't have 'Copy' in report name text", false, aWBWrapper.IsCopy);
		}

		public void TestItemsUnderSixteenOunces()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(0, AWB.ItemsUnderSixteenOunces);

			Shipment.Consols.Add(CreateConsol());
			AssertNotNull(aWBHeader.Consol);
			AssertEquals(0, AWB.ItemsUnderSixteenOunces);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines[0].JL_ActualWeight = 1m;
			Shipment.OuterPackLines[0].JL_ActualWeightUQ = "KG";
			AssertEquals(0, AWB.ItemsUnderSixteenOunces);

			Shipment.OuterPackLines[0].JL_ActualWeightUQ = "G";
			AssertEquals(1, AWB.ItemsUnderSixteenOunces);

			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines[1].JL_ActualWeight = 2m;
			Shipment.OuterPackLines[1].JL_ActualWeightUQ = "KG";
			AssertEquals(1, AWB.ItemsUnderSixteenOunces);

			Shipment.OuterPackLines[1].JL_ActualWeightUQ = "G";
			AssertEquals(2, AWB.ItemsUnderSixteenOunces);
		}

		public void TestNumberOfKnownShippers()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(0, AWB.NumberOfKnownShippers);

			Shipment.Consols.Add(CreateConsol());
			AssertNotNull(aWBHeader.Consol);
			AssertNull(Shipment.Consignor);
			AssertEquals(0, AWB.NumberOfKnownShippers);

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();

			OrgHeader consignor2 = Factory.New<OrgHeader>();
			consignor2.FillWithValidTestData();
			Factory.Save();

			Shipment.ConsignorPK = consignor.PK;
			OrgCountryData countryData = Factory.New<OrgCountryData>();
			consignor.CountryData.OV_OH_OrgHeader = Shipment.Consignor.PK;
			AssertEquals(0, AWB.NumberOfKnownShippers);

			consignor.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			AssertEquals(1, AWB.NumberOfKnownShippers);

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.Consols.Add(Shipment.Consols[0]);
			AssertNull(shipment1.Consignor);
			AssertEquals(1, AWB.NumberOfKnownShippers);

			shipment1.ConsignorPK = consignor2.PK;
			OrgCountryData countryData1 = Factory.New<OrgCountryData>();
			countryData1.OV_OH_OrgHeader = shipment1.Consignor.PK;
			AssertEquals(1, AWB.NumberOfKnownShippers);

			countryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			AssertEquals(2, AWB.NumberOfKnownShippers);

			consignor.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;
			AssertEquals(1, AWB.NumberOfKnownShippers);

			OrgCountryData addressData = consignor.MainAddress.KnownShipperDetails.AddNew();
			addressData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;
			AssertEquals(1, AWB.NumberOfKnownShippers);

			addressData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			AssertEquals(2, AWB.NumberOfKnownShippers);
		}

		public void TestNumberOfUnKnownShippers()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(0, AWB.NumberOfUnKnownShippers);

			Shipment.Consols.Add(CreateConsol());
			AssertNotNull(aWBHeader.Consol);
			AssertNull(Shipment.Consignor);
			AssertEquals(0, AWB.NumberOfUnKnownShippers);

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();

			OrgHeader consignor2 = Factory.New<OrgHeader>();
			consignor2.FillWithValidTestData();
			Factory.Save();

			Shipment.ConsignorPK = consignor.PK;
			OrgCountryData countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = Shipment.Consignor.PK;
			AssertEquals(1, AWB.NumberOfUnKnownShippers);

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.Consols.Add(Shipment.Consols[0]);
			AssertNull(shipment1.Consignor);
			AssertEquals(1, AWB.NumberOfUnKnownShippers);

			shipment1.ConsignorPK = consignor2.PK;
			OrgCountryData countryData1 = Factory.New<OrgCountryData>();
			countryData1.OV_OH_OrgHeader = shipment1.Consignor.PK;
			AssertEquals(2, AWB.NumberOfUnKnownShippers);

			countryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
			AssertEquals(1, AWB.NumberOfUnKnownShippers);
		}

		public void TestInspectedShipmentText_Shipment()
		{
			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Approved");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Not Approved");

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";

			Shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			AssertEquals((string)FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value, AWB.InspectedShipmentText);

			Shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals((string)FreightDataRegistry.Instance.AWBApprovedExporterText.Value, AWB.InspectedShipmentText);

			Shipment.JS_InspectionTypeCode = "XRY";
			AssertEquals((string)FreightDataRegistry.Instance.AWBApprovedExporterText.Value, AWB.InspectedShipmentText);

			Shipment.JS_RL_NKDestination = "AUBNE";
			AssertEquals("Empty if domestic shipment", string.Empty, AWB.InspectedShipmentText);
		}

		[TestDate(2014, 3, 15)]
		public void TestApprovedTextFromRegistry()
		{
			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Approved : <Now>");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Not Approved : <Now>");

			Shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			AssertEquals("Not Approved : 15-Mar-14 00:00", AWB.InspectedShipmentText);

			Shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals("Approved : 15-Mar-14 00:00", AWB.InspectedShipmentText);
		}

		[TestDate(2015, 5, 8)]
		public void TestApprovedTextFromSpecialValueInRegistry()
		{
			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Approved : <Now><InspectedShipmentText>");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Not Approved : <Now><InspectedShipmentText>");

			Shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			AssertEquals("Not Approved : 08-May-15 00:00", AWB.InspectedShipmentText);
			AssertEquals("Not Approved : 08-May-15 00:00 ", AWB.HandlingInformation);

			Shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals("Approved : 08-May-15 00:00", AWB.InspectedShipmentText);
			AssertEquals("Approved : 08-May-15 00:00 ", AWB.HandlingInformation);

			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Approved : <Now><HandlingInformation>");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Not Approved : <Now><HandlingInformation>");

			Shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			AssertEquals("Not Approved : 08-May-15 00:00 ", AWB.InspectedShipmentText);
			AssertEquals("Not Approved : 08-May-15 00:00  ", AWB.HandlingInformation);

			Shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals("Approved : 08-May-15 00:00 ", AWB.InspectedShipmentText);
			AssertEquals("Approved : 08-May-15 00:00  ", AWB.HandlingInformation);
		}

		public void TestInspectedShipmentTextForCanaryIslands_Shipment()
		{
			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Approved");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Not Approved");

			Shipment.JS_RL_NKOrigin = "ESBCN";
			Shipment.JS_RL_NKDestination = "ESTCI";

			Shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			AssertEquals((string)FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value, AWB.InspectedShipmentText);

			Shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals((string)FreightDataRegistry.Instance.AWBApprovedExporterText.Value, AWB.InspectedShipmentText);

			Shipment.JS_InspectionTypeCode = "XRY";
			AssertEquals((string)FreightDataRegistry.Instance.AWBApprovedExporterText.Value, AWB.InspectedShipmentText);

			Shipment.JS_RL_NKDestination = "ESMAD";
			AssertEquals("Empty if shipment is not from/to Spain to/from Canary Islands", string.Empty, AWB.InspectedShipmentText);
		}

		public void TestInspectedShipmentText_Consol()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Approved");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Not Approved");

			ForwardingConsol consol = CreateConsol();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			AWB = DocAWB.New(consol.AWBHeader, Factory);

			shipment1.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			shipment2.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			AssertEquals("Not approved", (string)FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value, AWB.InspectedShipmentText);

			shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals("Not approved, all shipments must be approved", (string)FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value, AWB.InspectedShipmentText);

			shipment2.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals("Approved", (string)FreightDataRegistry.Instance.AWBApprovedExporterText.Value, AWB.InspectedShipmentText);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";
			AssertEquals("Empty if domestic consol", string.Empty, AWB.InspectedShipmentText);
		}

		public void TestInspectedShipmentTextForCanaryIslands_Consol()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Spain);
			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Approved");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Not Approved");

			ForwardingConsol consol = CreateConsol();
			consol.JK_RL_NKLoadPort = "ESBCN";
			consol.JK_RL_NKDischargePort = "ESTCI";
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			AWB = DocAWB.New(consol.AWBHeader, Factory);

			shipment1.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			shipment2.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
			AssertEquals("Not approved", (string)FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value, AWB.InspectedShipmentText);

			shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals("Not approved, all shipments must be approved", (string)FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value, AWB.InspectedShipmentText);

			shipment2.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
			AssertEquals("Approved", (string)FreightDataRegistry.Instance.AWBApprovedExporterText.Value, AWB.InspectedShipmentText);

			consol.JK_RL_NKDischargePort = "ESMAD";
			AssertEquals("Empty if consol is not from/to Spain to/from Canary Islands", string.Empty, AWB.InspectedShipmentText);
		}

		public void TestConsolContainsDG()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals("N", AWB.ConsolContainsDG);

			Shipment.Consols.Add(CreateConsol());
			AssertNotNull(aWBHeader.Consol);
			AssertEquals("N", AWB.ConsolContainsDG);

			Shipment.OuterPackLines.AddNew();
			AssertEquals("N", AWB.ConsolContainsDG);
			Shipment.OuterPackLines.AddNew();
			AssertEquals("N", AWB.ConsolContainsDG);
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "2478", "c", "IMO").First();
			Shipment.OuterPackLines[0].UNDGs.AddNew().DI_DG = subs.PK;
			AssertEquals("Y", AWB.ConsolContainsDG);

			Shipment.OuterPackLines[1].UNDGs.AddNew().DI_DG = subs.PK;
			AssertEquals("Y", AWB.ConsolContainsDG);
		}

		public void TestServiceLevel()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(ZString.Empty, AWB.ServiceLevel);

			Shipment.Consols.Add(CreateConsol());
			AssertNotNull(aWBHeader.Consol);
			AssertEquals("STD", AWB.ServiceLevel);

			Shipment.Consols[0].JK_AWBServiceLevel = "EXP";
			AssertEquals("EXP", AWB.ServiceLevel);
		}

		public void TestServiceLevelDescription()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(ZString.Empty, AWB.ServiceLevel);

			Shipment.Consols.Add(CreateConsol());
			AssertNotNull(aWBHeader.Consol);
			AssertEquals("Standard", AWB.ServiceLevelDescription);
		}

		public void TestFAAIndirectAirCarrierNumber()
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();

			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			AssertNull(aWBHeader.Consol);
			AssertEquals(ZString.Empty, AWB.FAAIndirectAirCarrierNumber);

			Shipment.Consols.Add(CreateConsol());
			AssertNotNull(aWBHeader.Consol);
			AssertEquals(ZString.Empty, AWB.FAAIndirectAirCarrierNumber);

			OrgCusCode newCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			newCode.OK_CustomsRegNo = "A123";
			AssertEquals(ZString.Empty, AWB.FAAIndirectAirCarrierNumber);

			newCode.OK_CodeType = OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber;
			newCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("A123", AWB.FAAIndirectAirCarrierNumber);
		}

		public void TestIVACode()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "IT";

			ExportAWBHeader awbHeader = Shipment.AWBHeader;
			Shipment.Consols.Add(CreateConsol());

			OrgHeader branchProxy = GlbCompany.CurrentCompany.FirstBranchForUnLoco(Shipment.Consols[0].Transports[0].LoadPort).OrgProxy;

			var ivaCC = branchProxy.CustomsCodes.AddNew();
			ivaCC.OK_RN_NKCodeCountry = "IT";
			ivaCC.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			ivaCC.OK_CustomsRegNo = "10987654321";

			awbHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			awbHeader.Consol.JK_OA_SendingForwarderAddress = branchProxy.MainAddress.PK;
			awbHeader.Populate();

			AssertEquals("10987654321", AWB.IssuingCarrierIVACode);
		}

		public void TestCodiceFiscale()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "IT";

			ExportAWBHeader awbHeader = Shipment.AWBHeader;
			Shipment.Consols.Add(CreateConsol());
			OrgHeader consignor = Factory.New<OrgHeader>();
			awbHeader.Consol.Shipments[0].ConsignorPK = consignor.PK;

			var consignorSIV = consignor.CustomsCodes.AddNew();
			consignorSIV.OK_RN_NKCodeCountry = "IT";
			consignorSIV.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			consignorSIV.OK_CustomsRegNo = "11223344551";
			awbHeader.Populate();

			AssertEquals("11223344551", AWB.ShipperCodiceFiscaleOrIVA);
		}

		public void TestAviationSecurityApprovalNumberAndExpiryDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var awbHeader = Shipment.AWBHeader;
				AssertNull(awbHeader.Consol);
				AssertEquals(ZString.Empty, AWB.AviationSecurityApprovalNumber);
				AssertEquals(ZDate.Empty, AWB.AviationSecurityApprovalExpiryDate);

				Shipment.Consols.Add(CreateConsol());
				AssertNotNull(awbHeader.Consol);
				AssertEquals(ZString.Empty, AWB.AviationSecurityApprovalNumber);
				AssertEquals(ZDate.Empty, AWB.AviationSecurityApprovalExpiryDate);

				GlbBranch.CurrentBranch.OrgProxy.CountryData.OV_EXApprovalNumber = "TEST123";
				GlbBranch.CurrentBranch.OrgProxy.CountryData.OV_EXApprovalExpiryDate = new ZDate(2010, 5, 15);

				AssertEquals("TEST123", AWB.AviationSecurityApprovalNumber);
				AssertEquals(new ZDate(2010, 5, 15), AWB.AviationSecurityApprovalExpiryDate);
			}
		}

		public void TestAviationSecurityApprovalNumberAndExpiryDate_AddressLevelScheme_MainAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				var consol = CreateConsol();
				consol.SetDefaultSendingForwarderAddress(GlbBranch.CurrentBranch.OrgProxy);
				var awbWrapper = DocAWB.New(consol.AWBHeader, Factory);

				AssertEquals(ZString.Empty, awbWrapper.AviationSecurityApprovalNumber);
				AssertEquals(ZDate.Empty, awbWrapper.AviationSecurityApprovalExpiryDate);

				var approval = GlbBranch.CurrentBranch.OrgProxy.Addresses.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_EXApprovalNumber = "TESTXYZ";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				AssertEquals("TESTXYZ", awbWrapper.AviationSecurityApprovalNumber);
				AssertEquals(ZDate.Today.AddDays(1), awbWrapper.AviationSecurityApprovalExpiryDate);
			}
		}

		public void TestAviationSecurityApprovalNumberAndExpiryDate_AddressLevelScheme_NonMainAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				var consol = CreateConsol();
				var orgAddress = GlbBranch.CurrentBranch.OrgProxy.Addresses.Cast<OrgAddress>().First(a => !a.IsMainAddress);
				consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
				var awbWrapper = DocAWB.New(consol.AWBHeader, Factory);

				AssertEquals(ZString.Empty, awbWrapper.AviationSecurityApprovalNumber);
				AssertEquals(ZDate.Empty, awbWrapper.AviationSecurityApprovalExpiryDate);

				var approval = orgAddress.KnownShipperDetails.AddNew();
				approval.OV_EXApprovalNumber = "TESTXYZ";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				AssertEquals("TESTXYZ", awbWrapper.AviationSecurityApprovalNumber);
				AssertEquals(ZDate.Today.AddDays(1), awbWrapper.AviationSecurityApprovalExpiryDate);
			}
		}

		public void TestAviationSecurityApprovalNumberAndExpiryDate_AddressLevelScheme_MainAddress_Fallback()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				AssertEquals(ZString.Empty, AWB.AviationSecurityApprovalNumber);
				AssertEquals(ZDate.Empty, AWB.AviationSecurityApprovalExpiryDate);

				var approval = GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_EXApprovalNumber = "TESTXYZ";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				AssertEquals("TESTXYZ", AWB.AviationSecurityApprovalNumber);
				AssertEquals(ZDate.Today.AddDays(1), AWB.AviationSecurityApprovalExpiryDate);
			}
		}

		public void TestShipmentExportAWBHeader()
		{
			ShipmentExportAWBHeader shipmentAWBHeader = (ShipmentExportAWBHeader)Shipment.AWBHeader;
			AssertEquals("Header should be of type ShipmentExportAWBHeader", typeof(ShipmentExportAWBHeader).ToString(), shipmentAWBHeader.GetType().ToString());
			DocAWB docAWB = DocAWB.New(shipmentAWBHeader, Factory);
			AssertNotNull("Should not be null", docAWB.ShipmentExportAWBHeader);
			AssertEquals("Header should be of type ShipmentExportAWBHeader", typeof(ShipmentExportAWBHeader).ToString(), docAWB.ShipmentExportAWBHeader.GetType().ToString());

			ForwardingConsol consol = CreateConsol();
			ConsolExportAWBHeader consolAWBHeader = (ConsolExportAWBHeader)consol.AWBHeader;
			docAWB = DocAWB.New(consolAWBHeader, Factory);
			AssertNull("Should be null", docAWB.ShipmentExportAWBHeader);
		}

		public void TestShippersSignature()
		{
			AssertEquals("SHIPPERSSIGNATURE", AWB.ShippersSignature);
		}

		public void TestAWBAgentsSignature()
		{
			AssertEquals(Shipment.AWBHeader.EH_AWBAgentsSignature.ToUpper() + " " + Shipment.AWBHeader.EH_AgentApprovedExporterNumber, AWB.AWBAgentsSignature);
		}

		public void TestOtherCharges_Empty()
		{
			Assert(AWB.OtherCharges1.IsEmpty);
			Assert(AWB.OtherCharges2.IsEmpty);

			Shipment.AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			Shipment.AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			Assert(AWB.OtherCharges1.IsEmpty);
			Assert(AWB.OtherCharges2.IsEmpty);
		}

		public void TestOtherCharges_FewChargesAreShownWithFullDescriptions()
		{
			Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;

			AddOtherCharge(Shipment.AWBHeader, "C", "AA", "AA Description", 10m);
			AddOtherCharge(Shipment.AWBHeader, "C", "BB", "BB Description with some very long redundant explanation", 20m);
			AddOtherCharge(Shipment.AWBHeader, "A", "CC", "CC Description", 300.33m);

			Shipment.AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			Shipment.AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			string expectedOtherCharges1 = @"
AAC AA DESCRIPTION                                     10.00
BBC BB DESCRIPTION WITH SOME VERY LONG REDUNDANT       20.00
    EXPLANATION                                             
CCA CC DESCRIPTION                                    300.33";

			AssertEquals(expectedOtherCharges1.TrimStart(), AWB.OtherCharges1);

			string expectedOtherCharges2 = @"
AAC AA DESCRIPTION                                 AS AGREED
BBC BB DESCRIPTION WITH SOME VERY LONG REDUNDANT   AS AGREED
    EXPLANATION                                             
CCA CC DESCRIPTION                                 AS AGREED";

			AssertEquals(expectedOtherCharges2.TrimStart(), AWB.OtherCharges2);
		}

		public void TestOtherCharges_LotsOfChargesAreShownAsCodeAmountTable()
		{
			Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;

			AddOtherCharge(Shipment.AWBHeader, "C", "AA", "AA Description", 10m);
			AddOtherCharge(Shipment.AWBHeader, "C", "BB", "BB Description with some very long redundant explanation", 20m);
			AddOtherCharge(Shipment.AWBHeader, "A", "CC", "CC Description", 300.33m);
			AddOtherCharge(Shipment.AWBHeader, "A", "DD", "DD Description", 400m);
			AddOtherCharge(Shipment.AWBHeader, "C", "EE", "EE Description", 50m);
			AddOtherCharge(Shipment.AWBHeader, "C", "FF", "FF Description", 6m);

			Shipment.AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			Shipment.AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			string expectedOtherCharges1 = @"
AAC  10.00   BBC  20.00   CCA 300.33   
DDA 400.00   EEC  50.00   FFC   6.00   
";
			AssertEquals(expectedOtherCharges1.TrimStart(), AWB.OtherCharges1);

			string expectedOtherCharges2 = @"
AAC AS AGREED   BBC AS AGREED   CCA AS AGREED   
DDA AS AGREED   EEC AS AGREED   FFC AS AGREED   
";

			AssertEquals(expectedOtherCharges2.TrimStart(), AWB.OtherCharges2);
		}

		public void TestOtherCharges_MissingChargeCode_HAWB()
		{
			AddOtherCharge(Shipment.AWBHeader, "C", "AA", "AA Description", 10m);
			AddOtherCharge(Shipment.AWBHeader, "C", "BB", "BB Description with some very long redundant explanation", 20m);
			AddOtherCharge(Shipment.AWBHeader, "A", "", "Wild description", 300.33m);

			Shipment.AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			Shipment.AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			string expectedOtherCharges1 = @"
C AA DESCRIPTION                                      10.00
C BB DESCRIPTION WITH SOME VERY LONG REDUNDANT EXPLA  20.00
A WILD DESCRIPTION                                   300.33
";

			AssertEquals(expectedOtherCharges1.TrimStart(), AWB.OtherCharges1);

			string expectedOtherCharges2 = @"
C AA DESCRIPTION                                  AS AGREED
C BB DESCRIPTION WITH SOME VERY LONG REDUNDANT EX AS AGREED
A WILD DESCRIPTION                                AS AGREED
";

			AssertEquals(expectedOtherCharges2.TrimStart(), AWB.OtherCharges2);
		}

		public void TestOtherCharges_MissingChargeCode_MAWB()
		{
			var consol = CreateConsol();
			AWB = DocAWB.New(consol.AWBHeader, Factory);

			AddOtherCharge(consol.AWBHeader, "C", "AA", "AA Description", 10m);
			AddOtherCharge(consol.AWBHeader, "C", "BB", "BB Description with some very long redundant explanation", 20m);
			AddOtherCharge(consol.AWBHeader, "A", "", "Wild description", 300.33m);

			consol.AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			consol.AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			string expectedOtherCharges1 = @"
AAC AA DESCRIPTION 10.00
BBC BB DESCRIPTION WITH SOME VERY LONG REDUNDANT E 20.00
A WILD DESCRIPTION 300.33
";

			AssertEquals(expectedOtherCharges1.TrimStart(), AWB.OtherCharges1);

			string expectedOtherCharges2 = @"
AAC AA DESCRIPTION AS AGREED
BBC BB DESCRIPTION WITH SOME VERY LONG REDUNDAN AS AGREED
A WILD DESCRIPTION AS AGREED
";

			AssertEquals(expectedOtherCharges2.TrimStart(), AWB.OtherCharges2);
		}

		public void OtherCharges_AsAgreed()
		{
			var consol = CreateConsol();
			AWB = DocAWB.New(consol.AWBHeader, Factory);

			var charge1 = AddOtherCharge(consol.AWBHeader, "A", "AA", "AA Description", 10m);
			charge1.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			var charge2 = AddOtherCharge(consol.AWBHeader, "B", "BB", "BB Description with some very long redundant explanation", 20m);
			charge2.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;

			consol.AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			consol.AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;

			var resultWithCharge = @"
AAA AA DESCRIPTION 10.00
BBB BB DESCRIPTION WITH SOME VERY LONG REDUNDANT E 20.00
";
			var resultWithAsAgreed = @"
AAA AA DESCRIPTION AS AGREED
BBB BB DESCRIPTION WITH SOME VERY LONG REDUNDANT E AS AGREED
";

			var resultWithCollectAsAgreed = @"
AAA AA DESCRIPTION AS AGREED
BBB BB DESCRIPTION WITH SOME VERY LONG REDUNDANT E 20.0
";

			var resultWithPrepaidAsAgreed = @"
AAA AA DESCRIPTION 10.00
BBB BB DESCRIPTION WITH SOME VERY LONG REDUNDANT E AS AGREED
";

			AssertEquals(resultWithCharge.TrimStart(), AWB.OtherCharges1);
			AssertEquals(resultWithCharge.TrimStart(), AWB.OtherCharges1);

			consol.AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			consol.AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			AssertEquals(resultWithAsAgreed.TrimStart(), AWB.OtherCharges1);
			AssertEquals(resultWithAsAgreed.TrimStart(), AWB.OtherCharges1);

			consol.AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			consol.AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;

			AssertEquals(resultWithCollectAsAgreed.TrimStart(), AWB.OtherCharges1);
			AssertEquals(resultWithPrepaidAsAgreed.TrimStart(), AWB.OtherCharges1);
		}

		ExportAWBOtherCharges AddOtherCharge(ExportAWBHeader awbHeader, ZString entitlement, ZString code, ZString description, ZDecimal amount)
		{
			var result = awbHeader.AWBOtherCharges.AddNew();
			result.EO_ChargeCode = code;
			result.EO_ChargeDescription = description;
			result.EO_Amount = amount;
			result.EO_EntitlementCode = entitlement;

			return result;
		}

		void SetupShipperAddress()
		{
			var shipper = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipper.OH_RL_NKClosestPort = "HKHKG";
			Shipment.ConsignorPK = shipper.PK;

			Shipment.AWBHeader.EH_ShipperName = "ShipperName";
			Shipment.AWBHeader.EH_ShipperAddress = "ShipperAddress";
			Shipment.AWBHeader.EH_ShipperAddress2 = "ShipperAddress2";
			Shipment.AWBHeader.EH_ShipperContactName = "ShipperContact";
			Shipment.AWBHeader.EH_ShipperContactCode = "CC";
			Shipment.AWBHeader.EH_ShipperContactDetail = "ShipperContactDetail";
			Shipment.AWBHeader.EH_ShipperPlace = "Place";
			Shipment.AWBHeader.EH_ShipperState = "State";
			Shipment.AWBHeader.EH_ShipperPostCode = "PCode";
			Shipment.AWBHeader.EH_ShipperCountryCode = "KO";

			Shipment.AWBHeader.EH_ShipperOverride1 = "ShipperOverride1";
			Shipment.AWBHeader.EH_ShipperOverride2 = "ShipperOverride2";
			Shipment.AWBHeader.EH_ShipperOverride3 = "ShipperOverride3";
			Shipment.AWBHeader.EH_ShipperOverride4 = "ShipperOverride4";
			Shipment.AWBHeader.EH_ShipperOverride5 = "ShipperOverride5";

			Shipment.AWBHeader.EH_IsShipperOverriden = false;

			Shipment.ConsignorDocumentaryAddress.E2_Phone = "+61 (9)876 54321";

			OrgCusCode code1 = Shipment.Consignor.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.HKCodeTypes.KnownConsignorNumber;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.HongKong;
			code1.OK_CustomsRegNo = "CON2222";
		}

		public void TestShipperName()
		{
			SetupShipperAddress();
			AssertEquals("ShipperName", AWB.ShipperNameLine1);

			Shipment.AWBHeader.EH_ShipperName = "ShipperName longer than 20 will be splited into two lines AAA BBB CCCW";
			AssertEquals("ShipperName longer ", AWB.ShipperNameLine1);
			AssertEquals("than 20 will be splited into two lines AAA BBB CCCW", AWB.ShipperNameLine2);

			var longName = "ThisIsAShipmentNameWithNoBlankSpaceAndThisNameIsLongerThan20";
			Shipment.AWBHeader.EH_ShipperName = longName;
			AssertEquals("ThisIsAShipmentNameW", AWB.ShipperNameLine1);
			AssertEquals("ithNoBlankSpaceAndThisNameIsLongerThan20", AWB.ShipperNameLine2);

			Shipment.AWBHeader.EH_IsShipperOverriden = true;
			AssertEquals("ShipperOverride1", AWB.ShipperNameLine1);

			Shipment.AWBHeader.EH_ShipperOverride1 = "ShipperOverride1 longer than 20 will split to 2 lines";
			AssertEquals("ShipperOverride1 ", AWB.ShipperNameLine1);
			AssertEquals("longer than 20 will split to 2 lines", AWB.ShipperNameLine2);
		}

		public void TestShipperAddress1()
		{
			SetupShipperAddress();
			AssertEquals("ShipperName", AWB.ShipperAddress1);

			Shipment.AWBHeader.EH_IsShipperOverriden = true;
			AssertEquals("ShipperOverride1", AWB.ShipperAddress1);
		}

		public void TestShipperAddress2()
		{
			SetupShipperAddress();
			AssertEquals("ShipperAddress", AWB.ShipperAddress2);

			Shipment.AWBHeader.EH_IsShipperOverriden = true;
			AssertEquals("ShipperOverride2", AWB.ShipperAddress2);
		}

		public void TestShipperAddress3()
		{
			SetupShipperAddress();
			AssertEquals("ShipperAddress2", AWB.ShipperAddress3);

			Shipment.AWBHeader.EH_IsShipperOverriden = true;
			AssertEquals("ShipperOverride3", AWB.ShipperAddress3);
		}

		public void TestShipperAddress4()
		{
			SetupShipperAddress();
			AssertEquals("Place State PCode KO", AWB.ShipperAddress4);

			Shipment.AWBHeader.EH_IsShipperOverriden = true;
			AssertEquals("ShipperOverride4", AWB.ShipperAddress4);

			Shipment.AWBHeader.EH_IsShipperOverriden = false;
			Shipment.AWBHeader.EH_ShipperPlace = string.Empty;
			Shipment.AWBHeader.EH_ShipperCountryCode = string.Empty;
			AssertEquals("State PCode", AWB.ShipperAddress4);
		}

		public void TestShipperAddress5()
		{
			SetupShipperAddress();
			AssertEquals("CC ShipperContactDetail ShipperContact KC: CON2222", AWB.ShipperAddress5);

			Shipment.AWBHeader.EH_IsShipperOverriden = true;
			AssertEquals("ShipperOverride5", AWB.ShipperAddress5);

			Shipment.AWBHeader.EH_IsShipperOverriden = false;
			Shipment.AWBHeader.EH_ShipperContactCode = string.Empty;
			AssertEquals("ShipperContactDetail ShipperContact KC: CON2222", AWB.ShipperAddress5);

			Shipment.AWBHeader.EH_ShipperContactName = string.Empty;
			AssertEquals("ShipperContactDetail KC: CON2222", AWB.ShipperAddress5);

			Shipment.AWBHeader.EH_ShipperContactDetail = string.Empty;
			Shipment.AWBHeader.EH_ShipperContactName = "ShipperContact";
			AssertEquals("ShipperContact KC: CON2222", AWB.ShipperAddress5);
		}

		void SetupConsigneeAddress()
		{
			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consignee.OH_RL_NKClosestPort = "BRRIO";
			Shipment.ConsigneePK = consignee.PK;

			Shipment.AWBHeader.EH_ConsigneeName = "ConsigneeName";
			Shipment.AWBHeader.EH_ConsigneeAddress = "ConsigneeAddress";
			Shipment.AWBHeader.EH_ConsigneeAddress2 = "ConsigneeAddress2";
			Shipment.AWBHeader.EH_ConsigneeContactName = "ConsigneeContact";
			Shipment.AWBHeader.EH_ConsigneeContactCode = "CC";
			Shipment.AWBHeader.EH_ConsigneeContactDetail = "ConsigneeContactDetail";
			Shipment.AWBHeader.EH_ConsigneePlace = "Place";
			Shipment.AWBHeader.EH_ConsigneeState = "State";
			Shipment.AWBHeader.EH_ConsigneePostCode = "PCode";
			Shipment.AWBHeader.EH_ConsigneeCountryCode = "KO";

			Shipment.AWBHeader.EH_ConsigneeOverride1 = "ConsigneeOverride1";
			Shipment.AWBHeader.EH_ConsigneeOverride2 = "ConsigneeOverride2";
			Shipment.AWBHeader.EH_ConsigneeOverride3 = "ConsigneeOverride3";
			Shipment.AWBHeader.EH_ConsigneeOverride4 = "ConsigneeOverride4";
			Shipment.AWBHeader.EH_ConsigneeOverride5 = "ConsigneeOverride5";

			Shipment.AWBHeader.EH_IsConsigneeOverriden = false;

			Shipment.ConsigneeDocumentaryAddress.E2_Phone = "+61 (1)234 56789";
		}

		public void TestConsigneeName()
		{
			SetupConsigneeAddress();
			AssertEquals("ConsigneeName", AWB.ConsigneeNameLine1);

			Shipment.AWBHeader.EH_ConsigneeName = "ConsigneeName longer than 20 will be splited into two lines AAA BBB CCCW";
			AssertEquals("ConsigneeName ", AWB.ConsigneeNameLine1);
			AssertEquals("longer than 20 will be splited into two lines AAA BBB CCCW", AWB.ConsigneeNameLine2);

			var longName = "ThisIsAConsigneeNameWithNoBlankSpaceAndThisNameIsLongerThan20";
			Shipment.AWBHeader.EH_ConsigneeName = longName;
			AssertEquals("ThisIsAConsigneeName", AWB.ConsigneeNameLine1);
			AssertEquals("WithNoBlankSpaceAndThisNameIsLongerThan20", AWB.ConsigneeNameLine2);

			Shipment.AWBHeader.EH_IsConsigneeOverriden = true;
			AssertEquals("ConsigneeOverride1", AWB.ConsigneeNameLine1);

			Shipment.AWBHeader.EH_ConsigneeOverride1 = "ConsigneeOverride1 longer than 20 will split to 2 lines";
			AssertEquals("ConsigneeOverride1 ", AWB.ConsigneeNameLine1);
			AssertEquals("longer than 20 will split to 2 lines", AWB.ConsigneeNameLine2);
		}

		public void TestConsigneeAddress1()
		{
			SetupConsigneeAddress();
			AssertEquals("ConsigneeName", AWB.ConsigneeAddress1);
			Shipment.AWBHeader.EH_IsConsigneeOverriden = true;
			AssertEquals("ConsigneeOverride1", AWB.ConsigneeAddress1);
		}

		public void TestConsigneeAddress2()
		{
			SetupConsigneeAddress();
			AssertEquals("ConsigneeAddress", AWB.ConsigneeAddress2);

			Shipment.AWBHeader.EH_IsConsigneeOverriden = true;
			AssertEquals("ConsigneeOverride2", AWB.ConsigneeAddress2);
		}

		public void TestConsigneeAddress3()
		{
			SetupConsigneeAddress();
			AssertEquals("ConsigneeAddress2", AWB.ConsigneeAddress3);

			Shipment.AWBHeader.EH_IsConsigneeOverriden = true;
			AssertEquals("ConsigneeOverride3", AWB.ConsigneeAddress3);
		}

		public void TestConsigneeAddress4()
		{
			SetupConsigneeAddress();
			AssertEquals("Place State PCode KO", AWB.ConsigneeAddress4);

			Shipment.AWBHeader.EH_IsConsigneeOverriden = true;
			AssertEquals("ConsigneeOverride4", AWB.ConsigneeAddress4);

			Shipment.AWBHeader.EH_IsConsigneeOverriden = false;
			Shipment.AWBHeader.EH_ConsigneePlace = string.Empty;
			Shipment.AWBHeader.EH_ConsigneeCountryCode = string.Empty;
			AssertEquals("State PCode", AWB.ConsigneeAddress4);
		}

		public void TestConsigneeAddress5()
		{
			SetupConsigneeAddress();
			Shipment.AWBHeader.EH_ConsigneeTraderNo = "REG1111";
			Shipment.AWBHeader.EH_ConsigneeTraderNoType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			AssertEquals("CC ConsigneeContactDetail ConsigneeContact CNPJ: REG1111", AWB.ConsigneeAddress5);

			Shipment.AWBHeader.EH_IsConsigneeOverriden = true;
			AssertEquals("ConsigneeOverride5", AWB.ConsigneeAddress5);

			Shipment.AWBHeader.EH_IsConsigneeOverriden = false;
			Shipment.AWBHeader.EH_ConsigneeContactCode = string.Empty;
			AssertEquals("ConsigneeContactDetail ConsigneeContact CNPJ: REG1111", AWB.ConsigneeAddress5);

			Shipment.AWBHeader.EH_ConsigneeContactName = string.Empty;
			AssertEquals("ConsigneeContactDetail CNPJ: REG1111", AWB.ConsigneeAddress5);

			Shipment.AWBHeader.EH_ConsigneeContactDetail = string.Empty;
			Shipment.AWBHeader.EH_ConsigneeContactName = "ConsigneeContact";
			AssertEquals("ConsigneeContact CNPJ: REG1111", AWB.ConsigneeAddress5);
		}

		public void TestEmptyAccountingInformationsGetReplacedByAlsoNotify()
		{
			FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("", "<AirportOfDestinationCode>") });

			Shipment.JS_RL_NKDestination = "INBOM";
			Shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "BP PLC";

			ExportAWBHeader exportAWBHeader = Shipment.AWBHeader;
			exportAWBHeader.EH_IsNotifyOverriden = false;
			exportAWBHeader.Populate();

			AssertEquals("BOM", AWB.AccountingInformation);

			Shipment.JS_RL_NKDestination = ZString.Empty;
			exportAWBHeader.Populate();

			AssertEquals("ALSO NOTIFY: BP PLC", AWB.AccountingInformation);
		}

		public void TestAccountingInformation1()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;

			aWBHeader.EH_IsNotifyOverriden = false;
			aWBHeader.EH_NotifyOverride1 = "OVERRIDE1";
			aWBHeader.EH_AlsoNotifyName = "NAME";
			AssertEquals("ALSO NOTIFY: NAME", AWB.AccountingInformation);

			aWBHeader.EH_IsNotifyOverriden = true;
			AssertEquals("OVERRIDE1", AWB.AccountingInformation);

			var accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "AC1";
			accInfo.EA_Information = "INFORMATION";
			AssertEquals("AC1 INFORMATION", AWB.AccountingInformation);
		}

		public void TestAccountingInformation2()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;

			aWBHeader.EH_IsNotifyOverriden = false;
			aWBHeader.EH_NotifyOverride2 = "OVERRIDE2";
			aWBHeader.EH_AlsoNotifyAddress = "ADDRESS";
			AssertEquals("ADDRESS", AWB.AccountingInformation);

			aWBHeader.EH_IsNotifyOverriden = true;
			AssertEquals("OVERRIDE2", AWB.AccountingInformation);

			var accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "AC2";
			accInfo.EA_Information = "INFORMATION";
			AssertEquals("AC2 INFORMATION", AWB.AccountingInformation);
		}

		public void TestAccountingInformation3()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;

			aWBHeader.EH_IsNotifyOverriden = false;
			aWBHeader.EH_NotifyOverride3 = "OVERRIDE3";
			aWBHeader.EH_AlsoNotifyPlace = "PLACE";
			aWBHeader.EH_AlsoNotifyState = "STATE";
			aWBHeader.EH_AlsoNotifyPostCode = "PCODE";
			aWBHeader.EH_AlsoNotifyCountryCode = "CC";

			AssertEquals("PLACE STATE PCODE CC", AWB.AccountingInformation);

			aWBHeader.EH_IsNotifyOverriden = true;
			AssertEquals("OVERRIDE3", AWB.AccountingInformation);

			var accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "AC3";
			accInfo.EA_Information = "INFORMATION";
			AssertEquals("AC3 INFORMATION", AWB.AccountingInformation);
		}

		public void TestAccountingInformation4()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;

			aWBHeader.EH_IsNotifyOverriden = false;
			aWBHeader.EH_NotifyOverride4 = "OVERRIDE4";
			aWBHeader.EH_AlsoNotifyContactCode = "CC";
			aWBHeader.EH_AlsoNotifyContactDetail = "CONTACTDETAIL";
			AssertEquals("CC CONTACTDETAIL", AWB.AccountingInformation);

			aWBHeader.EH_AlsoNotifyContactName = "CONTACTNAME";
			AssertEquals("CC CONTACTDETAIL CONTACTNAME", AWB.AccountingInformation);

			aWBHeader.EH_AlsoNotifyContactCode = "";
			aWBHeader.EH_AlsoNotifyContactDetail = "";
			AssertEquals("CONTACTNAME", AWB.AccountingInformation);

			aWBHeader.EH_IsNotifyOverriden = true;
			AssertEquals("OVERRIDE4", AWB.AccountingInformation);

			var accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "AC4";
			accInfo.EA_Information = "INFORMATION";
			AssertEquals("AC4 INFORMATION", AWB.AccountingInformation);
		}

		public void TestAccountingInformation5()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;

			aWBHeader.EH_IsNotifyOverriden = false;
			aWBHeader.EH_NotifyOverride5 = "OVERRIDE5";
			AssertEquals(string.Empty, AWB.AccountingInformation);

			aWBHeader.EH_IsNotifyOverriden = true;
			AssertEquals("OVERRIDE5", AWB.AccountingInformation);

			var accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo = aWBHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "AC5";
			accInfo.EA_Information = "INFORMATION";
			AssertEquals("AC5 INFORMATION", AWB.AccountingInformation);
		}

		public void TestAccountingInformation5WithVATNumber()
		{
			var aWBHeader = Shipment.AWBHeader;
			aWBHeader.EH_AlsoNotifyTraderNo = "ABC123";
			aWBHeader.EH_AlsoNotifyTraderNoType = "CUI";

			aWBHeader.EH_IsNotifyOverriden = false;
			aWBHeader.EH_NotifyOverride5 = "OVERRIDE5";
			AssertEquals("CUIT: ABC123", AWB.AccountingInformation);

			aWBHeader.EH_IsNotifyOverriden = true;
			AssertEquals("OVERRIDE5 CUIT: ABC123", AWB.AccountingInformation);
		}

		public void TestOvertypeRateLines()
		{
			StmNote note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note.ST_NoteText = "Test That\r\nthe note\r\n trims trailing control\r\nchars\r\nand\r\nthat\r\nthe\r\nnote\r\nspans\r\nall\r\nthe\r\nlines\r\nok!";

			AssertEquals("Test That", AWB.RateLine2Text);
			AssertEquals("the note", AWB.RateLine3Text);
			AssertEquals(" trims trailing control", AWB.RateLine4Text);
			AssertEquals("chars", AWB.RateLine5Text);
			AssertEquals("and", AWB.RateLine6Text);
			AssertEquals("that", AWB.RateLine7Text);
			AssertEquals("the", AWB.RateLine8Text);
			AssertEquals("note", AWB.RateLine9Text);
			AssertEquals("spans", AWB.RateLine10Text);
			AssertEquals("all the lines ok!", AWB.RateLine11Text);
		}

		public void TestOvertypeRateLines_WrappingText()
		{
			StmNote note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			ZString expectedFirstLine = "This is a really long line of text, it needs to be about 67";
			ZString expectedSecondLine = "characters to test that it will wrap abcdefghijklmnopqrstuvwxyz i";
			ZString expectedThirdLine = "don't know what else to write, i'm just filling it in so that i can";
			ZString expectedFourthLine = "test it will wrap twice";
			note.ST_NoteText = string.Format("{0} {1} {2} {3}", expectedFirstLine, expectedSecondLine, expectedThirdLine, expectedFourthLine);

			AssertEquals(expectedFirstLine, AWB.RateLine2Text);
			AssertEquals(expectedSecondLine, AWB.RateLine3Text);
			AssertEquals(expectedThirdLine, AWB.RateLine4Text);
			AssertEquals(expectedFourthLine, AWB.RateLine5Text);

			ZString lastLine = "This is another really long line of text to ensure the last line gets cut off after 67 characters of text blah blah";
			note.ST_NoteText += "5rd line\r\n6th line\r\n7th line\r\n8th line\r\n9th line\r\n10th line\r\n" + lastLine;
			AWB = DocAWB.New(Shipment.AWBHeader, Factory);

			AssertEquals(lastLine.Left(67), AWB.RateLine11Text);
		}

		public void TestOvertypedNotes_RemoveLinesWithWrapping()
		{
			StmNote note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note.ST_NoteText = @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line
This is really long line of text to ensure that all things are working fine with wrapping when removing empty lines";

			AssertMultilineASCIIEquals("OvertypedNotes", @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line
This is really long line of text to ensure that all things are
working fine with wrapping when removing empty lines", GetExpectedOvertypedNotes(AWB));

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;

			StmNote note1 = shipment1.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note1.ST_NoteText =
				@"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
This is really long line of text to ensure that all things are
working fine with wrapping when removing empty lines This is 
really another long line of text to ensure that things are";

			AWB = DocAWB.New(shipment1.AWBHeader, Factory);

			AssertMultilineASCIIEquals("OvertypedNotes", @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
This is really long line of text to ensure that all things are
working fine with wrapping when removing empty lines This is
really another long line of text to ensure that things are", GetExpectedOvertypedNotes(AWB));

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			StmNote note2 = shipment2.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note2.ST_NoteText =
				@"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS
SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
This is really long line of text to ensure that all things are
working fine with wrapping when removing empty lines This is
really another long line of text to ensure that things are";

			AWB = DocAWB.New(shipment2.AWBHeader, Factory);

			AssertMultilineASCIIEquals("OvertypedNotes", @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS
SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
This is really long line of text to ensure that all things are
working fine with wrapping when removing empty lines This is
really another long line of text to ensure that things are", GetExpectedOvertypedNotes(AWB));

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_TransportMode = Core.Constants.TransportModes.Air;
			StmNote note3 = shipment3.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note3.ST_NoteText =
				@"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
This is really long line of text to ensure that all things are
working fine with wrapping when removing empty lines This is
really another long line of text to ensure that things are
AgainSomething";

			AWB = DocAWB.New(shipment3.AWBHeader, Factory);

			AssertMultilineASCIIEquals("OvertypedNotes", @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS
SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
This is really long line of text to ensure that all things are
working fine with wrapping when removing empty lines This is
really another long line of text to ensure that things are
AgainSomething", GetExpectedOvertypedNotes(AWB));

			ForwardingShipment shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_TransportMode = Core.Constants.TransportModes.Air;
			StmNote note4 = shipment4.Notes.AddNew();
			note4.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note4.ST_NoteText =
				@"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

This is really long line of text to ensure that all things are 
working fine with wrapping to be done today                                                                                                                                                                                                                 
SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line";

			AWB = DocAWB.New(shipment4.AWBHeader, Factory);

			AssertMultilineASCIIEquals("OvertypedNotes", @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

This is really long line of text to ensure that all things are
working fine with wrapping to be done today
SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line
", GetExpectedOvertypedNotes(AWB));
		}

		public void TestOvertypedNotes_RemoveLinesWithNoWrapping()
		{
			StmNote note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note.ST_NoteText = @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line";

			AssertMultilineASCIIEquals("OvertypedNotes", @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line", GetExpectedOvertypedNotes(AWB));

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;

			StmNote note1 = shipment1.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note1.ST_NoteText =
				@"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line
10th line
																											 ";

			AWB = DocAWB.New(shipment1.AWBHeader, Factory);

			AssertMultilineASCIIEquals("OvertypedNotes", @"LC/ NO.:LC988616000084
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line
10th line", GetExpectedOvertypedNotes(AWB));

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			StmNote note2 = shipment2.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note2.ST_NoteText =
				@"                        
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line
10th line
																											  ";

			AWB = DocAWB.New(shipment2.AWBHeader, Factory);

			AssertMultilineASCIIEquals("OvertypedNotes", @"
DATE OF ISSUE:2012/05/26
CONTRACT NO.:12STTTE/0351DUS

SHIPPING MARK:12SBIOE/0351US
TIANJIN AIRPORT
8th line
9th line
10th line", GetExpectedOvertypedNotes(AWB));
		}

		string GetExpectedOvertypedNotes(DocAWB docAWB)
		{
			return docAWB.RateLine2Text + System.Environment.NewLine + docAWB.RateLine3Text + System.Environment.NewLine
				+ docAWB.RateLine4Text + System.Environment.NewLine + docAWB.RateLine5Text + System.Environment.NewLine
				+ docAWB.RateLine6Text + System.Environment.NewLine + docAWB.RateLine7Text + System.Environment.NewLine
				+ docAWB.RateLine8Text + System.Environment.NewLine + docAWB.RateLine9Text + System.Environment.NewLine
				+ docAWB.RateLine10Text + System.Environment.NewLine + docAWB.RateLine11Text;
		}

		public void TestOvertypeRateLinesWithNatureAndQtyOfGoods()
		{
			StmNote note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			note.ST_NoteText = @"SEL CO.               NOTIFY:1)SAMSUNG ELECTRONICS LOGITECH CO.
C/S NO.7159-7161,     21-4,SUNGNAE-RI,YEONGIN-MYEON,ASAN-CITY, 
7215-7229,7230-7244,  CHUNGCHEONGNAM-DO,KOREA ATTN:LEE,YOUNG KWAN 
7279-7282,7267-7278   PHONE:82415406484 ATTN:LEE,JIN HAN 
C/S NO.7283-7302,     PHONE:821022052013 2)YUSENAIR&SEASERVICE(KOREA)
7304-7315,7316-7321,  CO.,LTD.ATTN.JOHN PARK/ATTN.JUSTIN KIM 
7303                  TEL:8222077-3013/82-2-2077-3021";

			Shipment.AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text = "MOS MEMORY";
			Shipment.AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text = "IC(D-RAM)...282240PCS";
			Shipment.AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text = "INV#T20TNBM355~T20TNBM365";
			Shipment.AWBHeader.AWBRateLine4.NatureAndQtyOfGoods.Text = "Total: 88 Packs";
			Shipment.AWBHeader.AWBRateLine5.NatureAndQtyOfGoods.Text = "DIMS 39x37x25 CM x 82";
			Shipment.AWBHeader.AWBRateLine6.NatureAndQtyOfGoods.Text = "DIMS 39x20x25 CM x 6";
			Shipment.AWBHeader.AWBRateLine7.NatureAndQtyOfGoods.Text = "VOL 3.075 M3";

			AssertEquals("SEL CO.               NOTIFY:1)SAMSUNG ELECTRONICS LOGITECH CO.", AWB.RateLine2Text);
			AssertEquals("C/S NO.7159-7161,     21-4,SUNGNAE-RI,YEONGIN-MYEON,ASAN-CITY,", AWB.RateLine3Text);
			AssertEquals("7215-7229,7230-7244,  CHUNGCHEONGNAM-DO,KOREA ATTN:LEE,YOUNG KWAN", AWB.RateLine4Text);
			AssertEquals("7279-7282,7267-7278   PHONE:82415406484 ATTN:LEE,JIN HAN", AWB.RateLine5Text);
			AssertEquals("C/S NO.7283-7302,     PHONE:821022052013", AWB.RateLine6Text);
			AssertEquals("2)YUSENAIR&SEASERVICE(KOREA)", AWB.RateLine7Text);
			AssertEquals("7304-7315,7316-7321,  CO.,LTD.ATTN.JOHN PARK/ATTN.JUSTIN KIM", AWB.RateLine8Text);
			AssertEquals("7303                  TEL:8222077-3013/82-2-2077-3021", AWB.RateLine9Text);
			AssertEquals("", AWB.RateLine10Text);
			AssertEquals("", AWB.RateLine11Text);
		}

		public void TestBrandImage()
		{
			AssertNull("HAWB Image should be null.", AWB.BrandImage);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var mAWB = DocAWB.New(consol.AWBHeader, Factory);
			var masterBrandImage = new Bitmap(typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.AWBBrandingMaster_600b.png"));

			AssertNotNull(mAWB.BrandImage);
			AssertEquals(masterBrandImage.Height, mAWB.BrandImage.Height);
		}

		public void TestAdditionalClause()
		{
			ZString temp = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			AssertEquals("AdditionalClause", string.Empty, AWB.AdditionalClause);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertEquals("AdditionalClause", string.Empty, AWB.AdditionalClause);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.EH_ShippersSignature = "ShippersSignature";
			DocAWB aWB2 = DocAWB.New(consol.AWBHeader, Factory);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("AdditionalClause", string.Empty, aWB2.AdditionalClause);

			consol.JK_AgentType = Core.Constants.AgentType.Other;
			AssertEquals("AdditionalClause", string.Empty, aWB2.AdditionalClause);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = temp;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHAWBImage()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFile = resourceRetriever.GetStream("DocumentWrappers.Freight.AWBMasterTitle.png");
				Image testImage = Image.FromStream(testFile);
				Env.Registry.Freight.AirWaybill.HAWBLogo = testImage;
				AssertEquals("Default is Env.Registry.Freight.AirWaybill.HAWBLogo", testImage.Size, AWB.HAWBImage.Size);
			}

			//agent branding
			DocumentsDataRegistry.Instance.EnableAgentBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, HBLAndHAWBBrandingOptionEditorInfo.AgentBranded);
			BrandingTestHelperClass.SetHybridBrandRegistryImage(DocumentsDataRegistry.Instance.HAWBAgentBrandingImage);

			ForwardingConsol consol = Shipment.Consols.AddNew();
			OrgHeader receivingFwd = OrgHeader.New(Factory);
			receivingFwd.MiscServ.OM_FWAgentCategory = "55";
			consol.SetDefaultReceivingForwarderAddress(receivingFwd);

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("Brand image from registry", new Size(8, 8), AWB.HAWBImage.Size);

			//client branding
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, HBLAndHAWBBrandingOptionEditorInfo.ClientBranded);

			OrgHeader cnor = OrgHeader.New(Factory);
			cnor.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);
			Shipment.ConsignorPK = cnor.PK;

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("Brand image from registry", new Size(9, 9), AWB.HAWBImage.Size);
		}

		public void TestIsNotifyOverriden()
		{
			AssertEquals("IsNotifyOverriden should be false by default", ZBool.False, AWB.IsNotifyOverriden);
			Shipment.AWBHeader.EH_IsNotifyOverriden = ZBool.True;
			Assert("IsNotifyOverriden should be true", AWB.IsNotifyOverriden);
		}

		public void TestUseOldStyleAWBFormat()
		{
			Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;

			Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
			AssertEquals(false, AWB.UseOldStyleAWBFormat);

			Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
			Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
			AssertEquals(true, AWB.UseOldStyleAWBFormat);

			Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
			Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Letter;
			AssertEquals(false, AWB.UseOldStyleAWBFormat);

			Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
			AssertEquals(false, AWB.UseOldStyleAWBFormat);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AWB = DocAWB.New(consol.AWBHeader, Factory);

			Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;

			Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
			AssertEquals(false, AWB.UseOldStyleAWBFormat);

			Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
			Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
			AssertEquals(true, AWB.UseOldStyleAWBFormat);

			Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
			Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Letter;
			AssertEquals(false, AWB.UseOldStyleAWBFormat);

			Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
			AssertEquals(false, AWB.UseOldStyleAWBFormat);
		}

		public void TestNoPieces()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;

			aWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "12";
			aWBHeader.AWBRateLines[1].ER_NoOfPiecesOrRCP = "0";
			aWBHeader.AWBRateLines[2].ER_NoOfPiecesOrRCP = string.Empty;
			aWBHeader.AWBRateLines[3].ER_NoOfPiecesOrRCP = "XYZ";

			AssertEquals("12", AWB.NoPieces(0));
			AssertEquals(string.Empty, AWB.NoPieces(1));
			AssertEquals(string.Empty, AWB.NoPieces(2));
			AssertEquals("XYZ", AWB.NoPieces(3));
		}

		public void TestThatThereIsOnly2AWBBarcodeLabels()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			//This is to ensure that the excel template AWB Barcode Label printing of AWB barcode label works for consol and shipment
			ZQuery aWBLabelFilter = new ZQuery(StmMenuTemplatePivotSchema.SI_DocumentTitle, "AWB Barcode Label");
			StmMenuTemplatePivot[] menuTemplatePivot = (StmMenuTemplatePivot[])factory.Load(typeof(StmMenuTemplatePivot), aWBLabelFilter);
			AssertEquals(2, menuTemplatePivot.Length);
		}

		public void TestShipmentHazOuterPacks()
		{
			var aWBHeader = Shipment.AWBHeader;
			ShipmentExportAWBHeader shipmentAWBHeader = (ShipmentExportAWBHeader)aWBHeader;

			AssertEquals("ShippingHazOuterPacks w/ no packlines", 0, AWB.ShipmentHazOuterPacks.Count);

			var line = Shipment.OuterPackLines.AddNew();
			var line2 = Shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line.JL_PackageCount = 10;
			line2.JL_PackageCount = 20;
			line.JL_Description = "Line";
			line2.JL_Description = "Line2";
			AssertEquals("ShippingHazOuterPacks w/ 2 GEN packlines", 0, AWB.ShipmentHazOuterPacks.Count);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("ShippingHazOuterPacks w/ 1st:ComCode", 10, AWB.ShipmentHazOuterPacks.Count);
			AssertEquals("ShippingHazOuterPacks w/ 1st:ComCode Line Desc", 10, CountPackagesWithLineDescription("Line"));

			var uNDG = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			line2.UNDGs.AddNew().DI_DG = uNDG.PK;
			AssertEquals("ShippingHazOuterPacks w/ 1st:ComCode n 2nd:UNDG", 30, AWB.ShipmentHazOuterPacks.Count);
			AssertEquals("ShippingHazOuterPacks - Line Desc", 10, CountPackagesWithLineDescription("Line"));
			AssertEquals("ShippingHazOuterPacks - Line2 Desc", 20, CountPackagesWithLineDescription("Line2"));
		}

		public void TestMAWBStatus()
		{
			AssertEquals("HAWBs should not have DRAFT/REPRINT", string.Empty, AWB.MAWBStatus);

			Shipment.AWBHeader.IsPrintingFinalNeutralMAWB = ZBool.True;
			AssertEquals("HAWBs should not have DRAFT/REPRINT", string.Empty, AWB.MAWBStatus);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			ForwardingConsol consol = CreateConsol();
			JobMawb mawb = CreateJobMawbForConsol();
			Factory.Save();
			consol.JK_IsNeutralMaster = ZBool.True;

			DocAWB consolAWB = DocAWB.New(consol.AWBHeader, Factory);
			AssertEquals("MAWB Status is Draft", "DRAFT", consolAWB.MAWBStatus);

			consol.AWBHeader.IsPrintingFinalNeutralMAWB = ZBool.True;
			AssertEquals("Printing final master. MAWB Status is blank", string.Empty, consolAWB.MAWBStatus);

			consol.Factory.Save();

			mawb.JM_IsPrinted = ZBool.True;
			AssertEquals("MAWB Status is REPRINT", "REPRINT", consolAWB.MAWBStatus);

			FreightDataRegistry.Instance.IncludeReprintStatusOnMAWBReprints.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("MAWB Status is not REPRINT as registry is off", string.Empty, consolAWB.MAWBStatus);

			FreightDataRegistry.Instance.IncludeReprintStatusOnMAWBReprints.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("MAWB Status is REPRINT as registry is on", "REPRINT", consolAWB.MAWBStatus);

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			AssertEquals("Non neutral MAWBs Status should be blank", string.Empty, consolAWB.MAWBStatus);
		}

		[GuiTest]
		public void TestAgentNameForLabel()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "the agent pty ltd";
			AssertEquals("Agent Name should be complete but in CAPS", "the agent pty ltd", AWB.AgentNameForLabel);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "the agent more more pty ltd";
			AssertEquals("Agent Name should be complete and in same format", "the agent more more pty ltd", AWB.AgentNameForLabel);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "THE AGENT MORE MORE........... pty ltd";
			AssertEquals("Agent Name should be missing the non alpha-chars", "THE AGENT MORE MORE pty ltd", AWB.AgentNameForLabel);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "the agent more        more more......... pty ltd";
			AssertEquals("Agent Name should be missing the non alpha-chars and extra spaces", "the agent more more more pty ltd", AWB.AgentNameForLabel);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "the agent more     more more mo......... pty ltd";
			AssertEquals("Agent Name should be missing the non alpha-chars, extra spaces and Organisation words AND BE IN CAPS", "AGENT MORE MORE MORE MO", AWB.AgentNameForLabel);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "the agent more     more more more....... pty ltd";
			AssertEquals("Agent Name should be missing the non alpha-chars, extra spaces and Organisation words", "AGENT MORE MORE MORE MORE", AWB.AgentNameForLabel);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "AFC dba R+L Global Logistics";
			AssertEquals("Agent name should be same as specified", "AFC dba R+L Global Logistics", AWB.AgentNameForLabel);
		}

		ZInt CountPackagesWithLineDescription(ZString description)
		{
			ZInt count = 0;
			foreach (DocOuterPack outerPack in AWB.ShipmentHazOuterPacks)
			{
				if (outerPack.PackLine.Description.Equals(description.ToString()))
				{
					count++;
				}
			}
			return count;
		}

		[GuiTest]
		public void TestAirlineShortName()
		{
			var aWBHeader = Shipment.AWBHeader;
			ForwardingConsol consol = CreateConsol();
			Shipment.Consols.Add(consol);

			RefAirline airline = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			airline.RM_LabelShortName = "";
			airline.RM_AirlineName1 = "";
			AssertEquals("AirlineShortName should be blank", string.Empty, AWB.AirlineShortName);

			airline.RM_LabelShortName = string.Empty;
			airline.RM_AirlineName1 = "the agent pty ltd";
			AssertEquals("AirlineShortName should be complete but in CAPS", "THE AGENT PTY LTD", AWB.AirlineShortName);

			airline.RM_AirlineName1 = "the agent more mo pty ltd";
			AssertEquals("AirlineShortName should be complete but in Title CAPS", "The Agent More Mo Pty Ltd", AWB.AirlineShortName);

			airline.RM_AirlineName1 = "the agent more mo........... pty ltd";
			AssertEquals("AirlineShortName should be missing the non alpha-chars", "The Agent More Mo Pty Ltd", AWB.AirlineShortName);

			airline.RM_AirlineName1 = "the agent more      more mo....... pty";
			AssertEquals("AirlineShortName should be missing the non alpha-chars and extra spaces", "The Agent More More Mo Pty", AWB.AirlineShortName);

			airline.RM_AirlineName1 = "the agent more    more more....... pty";
			AssertEquals("AirlineShortName should be missing the non alpha-chars, extra spaces and Organisation words AND BE IN CAPS", "AGENT MORE MORE MORE", AWB.AirlineShortName);

			airline.RM_AirlineName1 = "the agent more    more more mo...... pty";
			AssertEquals("AirlineShortName should be missing the non alpha-chars, extra spaces and Organisation words And Be In Tile Case", "Agent More More More Mo", AWB.AirlineShortName);

			airline.RM_LabelShortName = "Short Airline Name";
			AssertEquals("AirlineShortName should be the short name in CAPS", "SHORT AIRLINE NAME", AWB.AirlineShortName);
		}

		#region Ultimate Destination

		public void TestUltimateDestination()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			ExportAWBHeader exportAWBHeaderShipment = shipment.AWBHeader;
			ExportAWBHeader exportAWBHeaderConsol = consol.AWBHeader;

			DocAWB shipmentAWBWrapper = DocAWB.New(exportAWBHeaderShipment, Factory);
			DocAWB consolAWBWrapper = DocAWB.New(exportAWBHeaderConsol, Factory);

			AssertEquals(ZString.Empty, shipmentAWBWrapper.UltimateDestination);
			AssertEquals(ZString.Empty, consolAWBWrapper.UltimateDestination);

			shipment.JS_RL_NKDestination = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			AssertEquals("Hong Kong", shipmentAWBWrapper.UltimateDestination);
			AssertEquals("United States", consolAWBWrapper.UltimateDestination);

			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = "DEHAM";
			consol.Shipments.Add(shipment);

			AssertEquals("Germany", shipmentAWBWrapper.UltimateDestination);
			AssertEquals("United States", consolAWBWrapper.UltimateDestination);

			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = ZString.Empty;
			AssertEquals("Hong Kong", shipmentAWBWrapper.UltimateDestination);
			AssertEquals("United States", consolAWBWrapper.UltimateDestination);
		}

		#endregion

		public void TestLabelTotalPacks()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var aWBHeader = consol.AWBHeader;
			aWBHeader.MAWBLabelTotalPacks = 40;
			aWBHeader.DocumentSettingsPopulated = true;
			var aWBWrapper = DocAWB.New(aWBHeader, Factory);

			AssertEquals("LabelTotalPacks should be 40", 40, aWBWrapper.LabelTotalPacks);
		}

		public void TestLabelSize()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var aWBHeader = consol.AWBHeader;
			aWBHeader.DocumentSize = "5 Inch";
			DocAWB aWBWrapper = DocAWB.New(aWBHeader, Factory);

			AssertEquals("LabelSize should be 5 Inch", "5 Inch", aWBWrapper.LabelSize);
		}

		public void TestBusinessObjectToLogAgainst()
		{
			var aWBHeader = Shipment.AWBHeader;
			IBODocDataProvider docWrapper = DocAWB.New(aWBHeader, Factory);
			AssertEquals("BusinessObjectToLogAgainst should return the Shipment", Shipment, docWrapper.BusinessObjectToLogAgainst);

			ForwardingConsol consol = CreateConsol();
			aWBHeader = consol.AWBHeader;
			docWrapper = DocAWB.New(aWBHeader, Factory);
			AssertEquals("BusinessObjectToLogAgainst should return the Consol", consol, docWrapper.BusinessObjectToLogAgainst);
		}

		#region TestOuterPacks

		public void TestOuterPacks_OnlySelectOuterPacksFromMasterShipment()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			ForwardingConsol consol = CreateConsol();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_OuterPacks = 5;

			CommonShipment subShipment = consol.Shipments.AddNew();
			subShipment.JS_OuterPacks = 9;
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;

			DocAWB consolAWB = DocAWB.New(consol.AWBHeader, Factory);

			AssertEquals("Only the mastershipments should be included", 5, consolAWB.OuterPacks.Count);
			AssertEquals(5, consolAWB.TotalOuterPackCount);

			AssertEquals(1, consolAWB.OuterPacks[0].Number);
			AssertEquals("1 of 5", consolAWB.OuterPacks[0].Shipment.OuterPacksDescription);
			AssertEquals(2, consolAWB.OuterPacks[1].Number);
			AssertEquals("2 of 5", consolAWB.OuterPacks[1].Shipment.OuterPacksDescription);
			AssertEquals(3, consolAWB.OuterPacks[2].Number);
			AssertEquals("3 of 5", consolAWB.OuterPacks[2].Shipment.OuterPacksDescription);
			AssertEquals(4, consolAWB.OuterPacks[3].Number);
			AssertEquals("4 of 5", consolAWB.OuterPacks[3].Shipment.OuterPacksDescription);
			AssertEquals(5, consolAWB.OuterPacks[4].Number);
			AssertEquals("5 of 5", consolAWB.OuterPacks[4].Shipment.OuterPacksDescription);
		}

		public void TestOuterPacks_IncludesLeadShipmentsSubs_DoesNotIncludeMasterShipmentSubs()
		{
			var consol = CreateConsol();

			var leadShipment = consol.Shipments.AddNew();
			leadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_OuterPacks = 1;

			var subShipment = consol.Shipments.AddNew();
			subShipment.JS_OuterPacks = 2;
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;

			Factory.Save();

			var consolAWB = DocAWB.New(consol.AWBHeader, Factory);

			AssertEquals("Should include BCN Lead and BCN Sub", 3, consolAWB.OuterPacks.Count);
			AssertEquals(3, consolAWB.TotalOuterPackCount);

			var standardShipment = consol.Shipments.AddNew();
			standardShipment.JS_OuterPacks = 4;

			Factory.Save();

			consolAWB = DocAWB.New(consol.AWBHeader, Factory);
			AssertEquals("Should include BCN Lead, BCN Sub and Standard Shipment", 7, consolAWB.OuterPacks.Count);
			AssertEquals(7, consolAWB.TotalOuterPackCount);

			var masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			masterShipment.JS_OuterPacks = 8;

			var subShipmentNotToBeCounted = consol.Shipments.AddNew();
			subShipmentNotToBeCounted.JS_OuterPacks = 16;
			subShipmentNotToBeCounted.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.Save();

			consolAWB = DocAWB.New(consol.AWBHeader, Factory);
			AssertEquals("Should include BCN Lead, BCN Sub, Standard Shipment and Master Shipment ONLY", 15, consolAWB.OuterPacks.Count);
			AssertEquals(15, consolAWB.TotalOuterPackCount);
		}

		public void TestOuterPacksWhenSubShipmentAttachedToTwoConsols()
		{
			var consol1 = CreateConsol();
			consol1.JK_MasterBillNum = "MBN1";
			var consol2 = CreateConsol();
			consol2.JK_MasterBillNum = "MBN2";

			var standardShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			standardShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			standardShipment.JS_OuterPacks = 2;

			standardShipment.Consols.Add(consol1);
			Factory.Save();

			var consolAWB = DocAWB.New(consol1.AWBHeader, Factory);
			AssertEquals("Should include standard shipment.", 2, consolAWB.OuterPacks.Count);

			var masterShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			masterShipment.JS_OuterPacks = 3;
			masterShipment.Consols.Add(consol2);

			Factory.Save();

			consolAWB = DocAWB.New(consol2.AWBHeader, Factory);
			AssertEquals("Should include master shipment.", 3, consolAWB.OuterPacks.Count);

			standardShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			Factory.Save();

			consolAWB = DocAWB.New(consol2.AWBHeader, Factory);
			AssertEquals("Should include master shipment.", 3, consolAWB.OuterPacks.Count);

			consolAWB = DocAWB.New(consol1.AWBHeader, Factory);
			AssertEquals("Should include standard shipment.", 2, consolAWB.OuterPacks.Count);

			consol1.Shipments.Add(masterShipment);

			Factory.Save();

			consolAWB = DocAWB.New(consol1.AWBHeader, Factory);
			AssertEquals("Should include master shipment.", 3, consolAWB.OuterPacks.Count);
		}

		public void TestOuterPacks()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			ForwardingConsol consol = CreateConsol();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_OuterPacks = 3;

			DocAWB consolAWB = DocAWB.New(consol.AWBHeader, Factory);

			AssertEquals(5, consolAWB.OuterPacks.Count);
			AssertEquals(5, consolAWB.TotalOuterPackCount);

			AssertEquals(1, consolAWB.OuterPacks[0].Number);
			AssertEquals("1 of 2", consolAWB.OuterPacks[0].Shipment.OuterPacksDescription);
			AssertEquals(2, consolAWB.OuterPacks[1].Number);
			AssertEquals("2 of 2", consolAWB.OuterPacks[1].Shipment.OuterPacksDescription);
			AssertEquals(3, consolAWB.OuterPacks[2].Number);
			AssertEquals("1 of 3", consolAWB.OuterPacks[2].Shipment.OuterPacksDescription);
			AssertEquals(4, consolAWB.OuterPacks[3].Number);
			AssertEquals("2 of 3", consolAWB.OuterPacks[3].Shipment.OuterPacksDescription);
			AssertEquals(5, consolAWB.OuterPacks[4].Number);
			AssertEquals("3 of 3", consolAWB.OuterPacks[4].Shipment.OuterPacksDescription);

			DocAWB shipmentAWB = DocAWB.New(shipment.AWBHeader, Factory);

			AssertEquals(2, shipmentAWB.OuterPacks.Count);
			AssertEquals(2, shipmentAWB.TotalOuterPackCount);

			AssertEquals(1, shipmentAWB.OuterPacks[0].Number);
			AssertEquals("1 of 2", shipmentAWB.OuterPacks[0].Shipment.OuterPacksDescription);
			AssertEquals(2, shipmentAWB.OuterPacks[1].Number);
			AssertEquals("2 of 2", shipmentAWB.OuterPacks[1].Shipment.OuterPacksDescription);
		}

		#endregion

		public void TestRegistrationNumber()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.OH_RL_NKClosestPort = "BRRIO";
			Shipment.ConsigneePK = header.PK;

			var brazil = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);
			var code1 = header.CustomsCodes.AddNew();
			code1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			code1.OK_RN_NKCodeCountry = brazil.Code;
			code1.OK_CustomsRegNo = "REG1111";

			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			orgCusCode.DOC_RN_NKRegulatingCountry = Core.Constants.CountryCodes.Brazil;
			orgCusCode.DOC_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			orgCusCode.DOC_Priority = 1;
			orgCusCode.DOC_ShortLabel = "CNPJ";
			orgCusCode.DOC_LongLabel = "CNPJ";
			orgCusCode.DOC_DocumentType = "HAW";

			var aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("Registration Number", "CNPJ: REG1111", aWBWrapper.RegistrationNumber);
		}

		public void TestExtraShipperData()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.OH_RL_NKClosestPort = "HKHKG";
			Shipment.ConsignorPK = header.PK;

			OrgCusCode code1 = header.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.HKCodeTypes.KnownConsignorNumber;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.HongKong;
			code1.OK_CustomsRegNo = "CON2222";

			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("Extra Shipper Data", "KC: CON2222", aWBWrapper.ExtraShipperData);
		}

		public void TestChargesAtDestination()
		{
			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals(ZString.Empty, aWBWrapper.ChargesAtDestination);
		}

		public void TestMoneyValuesFormatting()
		{
			RefCurrency cur1 = Factory.NewWithValidTestData<RefCurrency>();
			cur1.RX_SubUnitRatio = 10;
			cur1.RX_Code = "ZZZ";
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			aWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			aWBHeader.EH_TaxesPPD = 13.1M;
			aWBHeader.EH_TaxesCOL = 12.1M;
			aWBHeader.EH_ValuationPPD = 10.1M;
			aWBHeader.EH_ValuationCOL = 11.1M;
			aWBHeader.EH_DeclaredValue = 20.1M;
			aWBHeader.EH_CustomsValue = 21.1M;
			aWBHeader.EH_InsuranceValue = 22.1M;
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;

			ExportAWBOtherCharges otherCharges = aWBHeader.AWBOtherCharges.AddNew();
			otherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			otherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			otherCharges.EO_Amount = 14.1M;

			otherCharges = aWBHeader.AWBOtherCharges.AddNew();
			otherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			otherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			otherCharges.EO_Amount = 15.1M;

			otherCharges = aWBHeader.AWBOtherCharges.AddNew();
			otherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			otherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharges.EO_Amount = 16.1M;

			otherCharges = aWBHeader.AWBOtherCharges.AddNew();
			otherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			otherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharges.EO_Amount = 17.1M;

			ExportAWBRateLine rateLine = aWBHeader.AWBRateLines[0];
			rateLine.ER_RateClass = Core.Constants.AWB.RateClass.MinimumCharge;
			rateLine.ER_RateChargeOrDiscount = 18.1M;

			rateLine = aWBHeader.AWBRateLines[1];
			rateLine.ER_RateClass = Core.Constants.AWB.RateClass.MinimumCharge;
			rateLine.ER_RateChargeOrDiscount = 19.1M;

			aWBHeader.EH_Currency = ZString.Empty;
			aWBHeader.EH_HouseCustomsValueCurrency = ZString.Empty;
			aWBHeader.EH_HouseInsuranceValueCurrency = ZString.Empty;

			AssertEquals("18.10", AWB.RateCharge1_1);
			AssertEquals("18.10", AWB.RateCharge2_1);
			AssertEquals("18.10", AWB.RateLineTotal1_1);
			AssertEquals("18.10", AWB.RateLineTotal2_1);

			AssertEquals("37.20", AWB.TotalLineTotals1);
			AssertEquals("37.20", AWB.TotalLineTotals2);

			AssertEquals("90.60", AWB.TotalCOL1);
			AssertEquals("90.60", AWB.TotalCOL2);

			AssertEquals("55.40", AWB.TotalPPD1);
			AssertEquals("55.40", AWB.TotalPPD2);

			AssertEquals("37.20", AWB.TotalWeightCOL1);
			AssertEquals("37.20", AWB.TotalWeightCOL2);

			Shipment.JS_INCO = Core.Constants.IncoTerms.CarriagePaidTo;
			AssertEquals("37.20", AWB.TotalWeightPPD1);
			AssertEquals("37.20", AWB.TotalWeightPPD1);

			AssertContains("14.10", AWB.OtherCharges1);
			AssertContains("14.10", AWB.OtherCharges2);

			AssertEquals("14.10", AWB.OtherChargesDueAgentCOL1);
			AssertEquals("14.10", AWB.OtherChargesDueAgentCOL2);

			AssertEquals("15.10", AWB.OtherChargesDueAgentPPD1);
			AssertEquals("15.10", AWB.OtherChargesDueAgentPPD2);

			AssertEquals("16.10", AWB.OtherChargesDueCarrierCOL1);
			AssertEquals("16.10", AWB.OtherChargesDueCarrierCOL2);

			AssertEquals("17.10", AWB.OtherChargesDueCarrierPPD1);
			AssertEquals("17.10", AWB.OtherChargesDueCarrierPPD1);

			AssertEquals("12.10", AWB.TaxesCOL1);
			AssertEquals("12.10", AWB.TaxesCOL2);

			AssertEquals("13.10", AWB.TaxesPPD1);
			AssertEquals("13.10", AWB.TaxesPPD2);

			AssertEquals("11.10", AWB.ValuationCOL1);
			AssertEquals("11.10", AWB.ValuationCOL2);

			AssertEquals("10.10", AWB.ValuationPPD1);
			AssertEquals("10.10", AWB.ValuationPPD2);

			AssertEquals("20.10 ", AWB.DeclaredValue);
			AssertEquals("21.10 ", AWB.CustomsValue);
			AssertEquals("22.10 ", AWB.InsuranceValue);

			aWBHeader.EH_Currency = "ZZZ";
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AWB = DocAWB.New(aWBHeader, Factory);

			AssertEquals("18.1", AWB.RateCharge1_1);
			AssertEquals("18.1", AWB.RateCharge2_1);
			AssertEquals("18.1", AWB.RateLineTotal1_1);
			AssertEquals("18.1", AWB.RateLineTotal2_1);

			AssertEquals("37.2", AWB.TotalLineTotals1);
			AssertEquals("37.2", AWB.TotalLineTotals2);

			AssertEquals("90.6", AWB.TotalCOL1);
			AssertEquals("90.6", AWB.TotalCOL2);

			AssertEquals("55.4", AWB.TotalPPD1);
			AssertEquals("55.4", AWB.TotalPPD2);

			AssertEquals("37.2", AWB.TotalWeightCOL1);
			AssertEquals("37.2", AWB.TotalWeightCOL2);

			Shipment.JS_INCO = Core.Constants.IncoTerms.CarriagePaidTo;
			AssertEquals("37.2", AWB.TotalWeightPPD1);
			AssertEquals("37.2", AWB.TotalWeightPPD1);

			AssertContains("14.1", AWB.OtherCharges1);
			AssertContains("14.1", AWB.OtherCharges2);

			AssertEquals("12.1", AWB.TaxesCOL1);
			AssertEquals("12.1", AWB.TaxesCOL2);

			AssertEquals("13.1", AWB.TaxesPPD1);
			AssertEquals("13.1", AWB.TaxesPPD2);

			AssertEquals("11.1", AWB.ValuationCOL1);
			AssertEquals("11.1", AWB.ValuationCOL2);

			AssertEquals("10.1", AWB.ValuationPPD1);
			AssertEquals("10.1", AWB.ValuationPPD2);

			AssertEquals("14.1", AWB.OtherChargesDueAgentCOL1);
			AssertEquals("14.1", AWB.OtherChargesDueAgentCOL2);

			AssertEquals("15.1", AWB.OtherChargesDueAgentPPD1);
			AssertEquals("15.1", AWB.OtherChargesDueAgentPPD2);

			AssertEquals("16.1", AWB.OtherChargesDueCarrierCOL1);
			AssertEquals("16.1", AWB.OtherChargesDueCarrierCOL2);

			AssertEquals("17.1", AWB.OtherChargesDueCarrierPPD1);
			AssertEquals("17.1", AWB.OtherChargesDueCarrierPPD1);

			AssertEquals("20.1 ", AWB.DeclaredValue);
			AssertEquals("21.1 ", AWB.CustomsValue);
			AssertEquals("22.1 ", AWB.InsuranceValue);

			cur1 = Factory.NewWithValidTestData<RefCurrency>();
			cur1.RX_SubUnitRatio = 1;
			cur1.RX_Code = "ZZA";
			aWBHeader.EH_HouseDeclaredValueCurrency = "ZZA";

			cur1 = Factory.NewWithValidTestData<RefCurrency>();
			cur1.RX_SubUnitRatio = 1000;
			cur1.RX_Code = "ZZB";
			aWBHeader.EH_HouseCustomsValueCurrency = "ZZB";

			cur1 = Factory.NewWithValidTestData<RefCurrency>();
			cur1.RX_SubUnitRatio = 10000;
			cur1.RX_Code = "ZZC";
			aWBHeader.EH_HouseInsuranceValueCurrency = "ZZC";

			AssertContains("20 ", AWB.DeclaredValue);
			AssertContains("21.100 ", AWB.CustomsValue);
			AssertContains("22.1000 ", AWB.InsuranceValue);

			ForwardingConsol consol = CreateConsol();
			JobMawb mawb = CreateJobMawbForConsol();
			consol.JK_IsNeutralMaster = ZBool.True;
			aWBHeader = consol.AWBHeader;
			aWBHeader.EH_DeclaredValue = 20.1M;
			aWBHeader.EH_CustomsValue = 21.1M;
			aWBHeader.EH_InsuranceValue = 22.1M;
			aWBHeader.EH_Currency = "ZZZ";
			aWBHeader.EH_HouseDeclaredValueCurrency = "ZZA";
			aWBHeader.EH_HouseCustomsValueCurrency = "ZZB";
			aWBHeader.EH_HouseInsuranceValueCurrency = "ZZC";

			DocAWB consolAWB = DocAWB.New(consol.AWBHeader, Factory);
			AssertEquals("20.1", consolAWB.DeclaredValue);
			AssertEquals("21.1", consolAWB.CustomsValue);
			AssertEquals("22.1", consolAWB.InsuranceValue);
		}

		public void TestPrepaidAndCollect_AsAgreed()
		{
			var awbHeader = Shipment.AWBHeader;

			awbHeader.EH_ValuationPPD = 10.1M;
			awbHeader.EH_ValuationCOL = 11.1M;

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("10.10", AWB.ValuationPPD1);
			AssertEquals("11.10", AWB.ValuationCOL1);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			AssertEquals("As Agreed", AWB.ValuationPPD1);
			AssertEquals("As Agreed", AWB.ValuationCOL1);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AssertEquals("10.10", AWB.ValuationPPD1);
			AssertEquals("As Agreed", AWB.ValuationCOL1);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("11.10", AWB.ValuationCOL2);
			AssertEquals("10.10", AWB.ValuationPPD2);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertEquals("As Agreed", AWB.ValuationCOL2);
			AssertEquals("As Agreed", AWB.ValuationPPD2);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertEquals("As Agreed", AWB.ValuationPPD2);
			AssertEquals("11.10", AWB.ValuationCOL2);
		}

		public void TestRateCharge_AsAgreed_Prepaid()
		{
			var awbHeader = (ShipmentExportAWBHeader)Shipment.AWBHeader;
			awbHeader.Shipment.JS_OverrideWaybillDefaults = true;

			var rateLine1 = awbHeader.AWBRateLines[0];
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.MinimumCharge;
			rateLine1.ER_RateChargeOrDiscount = 18.1M;

			var rateLine2 = awbHeader.AWBRateLines[1];
			rateLine2.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			rateLine2.ER_ChargeableWeight = 1M;
			rateLine2.ER_RateChargeOrDiscount = 19.1M;

			var rateLine3 = awbHeader.AWBRateLines[2];
			rateLine3.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			rateLine3.ER_ChargeableWeight = 1M;
			rateLine3.ER_RateChargeOrDiscount = 20.1M;

			awbHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("18.10", AWB.RateCharge1_1);
			AssertEquals("18.10", AWB.RateLineTotal1_1);
			AssertEquals("19.10", AWB.RateCharge1_2);
			AssertEquals("19.10", AWB.RateLineTotal1_2);
			AssertEquals("20.10", AWB.RateCharge1_3);
			AssertEquals("20.10", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AssertEquals("18.10", AWB.RateCharge1_1);
			AssertEquals("18.10", AWB.RateLineTotal1_1);
			AssertEquals("19.10", AWB.RateCharge1_2);
			AssertEquals("19.10", AWB.RateLineTotal1_2);
			AssertEquals("20.10", AWB.RateCharge1_3);
			AssertEquals("20.10", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertEquals("", AWB.RateCharge1_1);
			AssertEquals("As Agreed", AWB.RateLineTotal1_1);
			AssertEquals("", AWB.RateCharge1_2);
			AssertEquals("", AWB.RateLineTotal1_2);
			AssertEquals("", AWB.RateCharge1_3);
			AssertEquals("", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("18.10", AWB.RateCharge2_1);
			AssertEquals("18.10", AWB.RateLineTotal2_1);
			AssertEquals("19.10", AWB.RateCharge2_2);
			AssertEquals("19.10", AWB.RateLineTotal2_2);
			AssertEquals("20.10", AWB.RateCharge2_3);
			AssertEquals("20.10", AWB.RateLineTotal2_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertEquals("", AWB.RateCharge2_1);
			AssertEquals("As Agreed", AWB.RateLineTotal2_1);
			AssertEquals("", AWB.RateCharge2_2);
			AssertEquals("As Agreed", AWB.RateLineTotal2_2);
			AssertEquals("", AWB.RateCharge2_3);
			AssertEquals("As Agreed", AWB.RateLineTotal2_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertEquals("", AWB.RateCharge2_1);
			AssertEquals("As Agreed", AWB.RateLineTotal2_1);
			AssertEquals("", AWB.RateCharge2_2);
			AssertEquals("", AWB.RateLineTotal2_2);
			AssertEquals("", AWB.RateCharge2_3);
			AssertEquals("", AWB.RateLineTotal2_3);
		}

		public void TestRateCharge_AsAgreed_Collect()
		{
			var awbHeader = (ShipmentExportAWBHeader)Shipment.AWBHeader;
			awbHeader.Shipment.JS_OverrideWaybillDefaults = true;

			var rateLine1 = awbHeader.AWBRateLines[0];
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.MinimumCharge;
			rateLine1.ER_RateChargeOrDiscount = 18.1M;

			var rateLine2 = awbHeader.AWBRateLines[1];
			rateLine2.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			rateLine2.ER_ChargeableWeight = 1M;
			rateLine2.ER_RateChargeOrDiscount = 19.1M;

			var rateLine3 = awbHeader.AWBRateLines[2];
			rateLine3.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			rateLine3.ER_ChargeableWeight = 1M;
			rateLine3.ER_RateChargeOrDiscount = 20.1M;

			awbHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("18.10", AWB.RateCharge1_1);
			AssertEquals("18.10", AWB.RateLineTotal1_1);
			AssertEquals("19.10", AWB.RateCharge1_2);
			AssertEquals("19.10", AWB.RateLineTotal1_2);
			AssertEquals("20.10", AWB.RateCharge1_3);
			AssertEquals("20.10", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AssertEquals("", AWB.RateCharge1_1);
			AssertEquals("As Agreed", AWB.RateLineTotal1_1);
			AssertEquals("", AWB.RateCharge1_2);
			AssertEquals("As Agreed", AWB.RateLineTotal1_2);
			AssertEquals("", AWB.RateCharge1_3);
			AssertEquals("As Agreed", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertEquals("", AWB.RateCharge1_1);
			AssertEquals("As Agreed", AWB.RateLineTotal1_1);
			AssertEquals("", AWB.RateCharge1_2);
			AssertEquals("", AWB.RateLineTotal1_2);
			AssertEquals("", AWB.RateCharge1_3);
			AssertEquals("", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("18.10", AWB.RateCharge2_1);
			AssertEquals("18.10", AWB.RateLineTotal2_1);
			AssertEquals("19.10", AWB.RateCharge2_2);
			AssertEquals("19.10", AWB.RateLineTotal2_2);
			AssertEquals("20.10", AWB.RateCharge2_3);
			AssertEquals("20.10", AWB.RateLineTotal2_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertEquals("18.10", AWB.RateCharge2_1);
			AssertEquals("18.10", AWB.RateLineTotal2_1);
			AssertEquals("19.10", AWB.RateCharge2_2);
			AssertEquals("19.10", AWB.RateLineTotal2_2);
			AssertEquals("20.10", AWB.RateCharge2_3);
			AssertEquals("20.10", AWB.RateLineTotal2_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertEquals("", AWB.RateCharge2_1);
			AssertEquals("As Agreed", AWB.RateLineTotal2_1);
			AssertEquals("", AWB.RateCharge2_2);
			AssertEquals("", AWB.RateLineTotal2_2);
			AssertEquals("", AWB.RateCharge2_3);
			AssertEquals("", AWB.RateLineTotal2_3);
		}

		public void TestRateCharge_AsAgreed_Both()
		{
			var awbHeader = (ShipmentExportAWBHeader)Shipment.AWBHeader;
			awbHeader.Shipment.JS_OverrideWaybillDefaults = true;

			var rateLine1 = awbHeader.AWBRateLines[0];
			rateLine1.ER_RateClass = Core.Constants.AWB.RateClass.MinimumCharge;
			rateLine1.ER_RateChargeOrDiscount = 18.1M;

			var rateLine2 = awbHeader.AWBRateLines[1];
			rateLine2.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			rateLine2.ER_ChargeableWeight = 1M;
			rateLine2.ER_RateChargeOrDiscount = 19.1M;

			var rateLine3 = awbHeader.AWBRateLines[2];
			rateLine3.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			rateLine3.ER_ChargeableWeight = 1M;
			rateLine3.ER_RateChargeOrDiscount = 20.1M;

			awbHeader.EH_WeightVPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both;

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("18.10", AWB.RateCharge1_1);
			AssertEquals("18.10", AWB.RateLineTotal1_1);
			AssertEquals("19.10", AWB.RateCharge1_2);
			AssertEquals("19.10", AWB.RateLineTotal1_2);
			AssertEquals("20.10", AWB.RateCharge1_3);
			AssertEquals("20.10", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AssertEquals("18.10", AWB.RateCharge1_1);
			AssertEquals("18.10", AWB.RateLineTotal1_1);
			AssertEquals("", AWB.RateCharge1_2);
			AssertEquals("As Agreed", AWB.RateLineTotal1_2);
			AssertEquals("20.10", AWB.RateCharge1_3);
			AssertEquals("20.10", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertEquals("", AWB.RateCharge1_1);
			AssertEquals("As Agreed", AWB.RateLineTotal1_1);
			AssertEquals("", AWB.RateCharge1_2);
			AssertEquals("", AWB.RateLineTotal1_2);
			AssertEquals("", AWB.RateCharge1_3);
			AssertEquals("", AWB.RateLineTotal1_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("18.10", AWB.RateCharge2_1);
			AssertEquals("18.10", AWB.RateLineTotal2_1);
			AssertEquals("19.10", AWB.RateCharge2_2);
			AssertEquals("19.10", AWB.RateLineTotal2_2);
			AssertEquals("20.10", AWB.RateCharge2_3);
			AssertEquals("20.10", AWB.RateLineTotal2_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertEquals("18.10", AWB.RateCharge2_1);
			AssertEquals("18.10", AWB.RateLineTotal2_1);
			AssertEquals("19.10", AWB.RateCharge2_2);
			AssertEquals("19.10", AWB.RateLineTotal2_2);
			AssertEquals("", AWB.RateCharge2_3);
			AssertEquals("As Agreed", AWB.RateLineTotal2_3);

			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			AssertEquals("", AWB.RateCharge2_1);
			AssertEquals("As Agreed", AWB.RateLineTotal2_1);
			AssertEquals("", AWB.RateCharge2_2);
			AssertEquals("", AWB.RateLineTotal2_2);
			AssertEquals("", AWB.RateCharge2_3);
			AssertEquals("", AWB.RateLineTotal2_3);
		}

		#region TestNatureAndQuantityOfGoods

		public void TestNatureAndQuantityOfGoods1()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "Desc 1";
			AssertEquals("Desc 1", AWB.NatureAndQuantityOfGoods1);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals("See Attached Follow On Page", AWB.NatureAndQuantityOfGoods1);

			constants.Clear();
			AWB.SetTemplateConstants(constants);
		}

		public void TestNatureAndQuantityOfGoods2()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "Desc 2";
			AssertEquals("Desc 2", AWB.NatureAndQuantityOfGoods2);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods2);
		}

		public void TestNatureAndQuantityOfGoods3()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "Desc 3";
			AssertEquals("Desc 3", AWB.NatureAndQuantityOfGoods3);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods3);
		}

		public void TestNatureAndQuantityOfGoods4()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "Desc 4";
			AssertEquals("Desc 4", AWB.NatureAndQuantityOfGoods4);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods4);
		}

		public void TestNatureAndQuantityOfGoods5()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription = "Desc 5";
			AssertEquals("Desc 5", AWB.NatureAndQuantityOfGoods5);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods5);
		}

		public void TestNatureAndQuantityOfGoods6()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription = "Desc 6";
			AssertEquals("Desc 6", AWB.NatureAndQuantityOfGoods6);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods6);
		}

		public void TestNatureAndQuantityOfGoods7()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription = "Desc 7";
			AssertEquals("Desc 7", AWB.NatureAndQuantityOfGoods7);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods7);
		}

		public void TestNatureAndQuantityOfGoods8()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription = "Desc 8";
			AssertEquals("Desc 8", AWB.NatureAndQuantityOfGoods8);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods8);
		}

		public void TestNatureAndQuantityOfGoods9()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription = "Desc 9";
			AssertEquals("Desc 9", AWB.NatureAndQuantityOfGoods9);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods9);
		}

		public void TestNatureAndQuantityOfGoods10()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription = "Desc 10";
			AssertEquals("Desc 10", AWB.NatureAndQuantityOfGoods10);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.NatureAndQuantityOfGoods10);
		}

		public void TestExtraNatureAndQtyOfGoods()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription = "Desc 11";
			aWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription = "Desc 12";
			AssertEquals("Desc 11\nDesc 12", AWB.ExtraNatureAndQtyOfGoods);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);
			AssertEquals(string.Empty, AWB.ExtraNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoodsContainsLithiumBattery()
		{
			var consol = CreateConsol();
			var aWBHeader = consol.AWBHeader;
			var consolAWB = DocAWB.New(aWBHeader, Factory);

			aWBHeader.AWBRateLine1.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			aWBHeader.AWBRateLine1.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI966;
			AssertEquals("Lithium ion batteries in compliance", consolAWB.NatureAndQuantityOfGoods1);
			AssertEquals(" with Section II of PI966", consolAWB.NatureAndQuantityOfGoods2);
			AssertEquals(string.Empty, consolAWB.NatureAndQuantityOfGoods3);

			aWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			aWBHeader.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI968;
			AssertEquals("Lithium metal batteries in ", consolAWB.NatureAndQuantityOfGoods3);
			AssertEquals("compliance with Section II of PI968", consolAWB.NatureAndQuantityOfGoods4);
			AssertEquals(" CAO", consolAWB.NatureAndQuantityOfGoods5);
			AssertEquals(string.Empty, consolAWB.NatureAndQuantityOfGoods6);

			aWBHeader.AWBRateLine11.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			aWBHeader.AWBRateLine11.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI966;
			AssertEquals("Lithium ion batteries in compliance\n with Section II of PI966", consolAWB.ExtraNatureAndQtyOfGoods);
		}

		public void TestDetailedGoodsDescription()
		{
			AssertEquals("Description should be empty", string.Empty, AWB.DetailedGoodsDescription);

			ShipmentExportAWBHeader aWBHeader = (ShipmentExportAWBHeader)Shipment.AWBHeader;
			StmNote detailedDescription = AddNotes(Shipment, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Detailed goods description\nLine Two");
			StmNote otherNotes = AddNotes(Shipment, PredefinedNoteTypes.Instance.InternalWorkNotes.Description, "This is other notes and should not be included.");
			AssertEquals("Detailed goods description", "Detailed goods description\nLine Two", AWB.DetailedGoodsDescription);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Laser HAWB with Follow on Page");
			AWB.SetTemplateConstants(constants);

			Shipment.Consols.AddNew();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.JS_RL_NKOrigin = "AUSYD";

			aWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			aWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			aWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			aWBHeader.Shipment.JS_ActualVolume = 10M;
			aWBHeader.Shipment.JS_UnitOfVolume = "M3";
			aWBHeader.Shipment.JS_OuterPacks = 20;
			aWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(aWBHeader, 5, 6, 7, 8, "M");
			aWBHeader.Populate();

			AssertEquals("Detailed goods description", "Detailed goods description\nLine Two\nDIMS 500x600x700 CM x 8", AWB.DetailedGoodsDescription);

			AddOuterPackLine(aWBHeader, 6, 7, 8, 9, "CM");
			aWBHeader.Populate();

			AssertEquals("Detailed goods description", "Detailed goods description\nLine Two\nDIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9", AWB.DetailedGoodsDescription);

			CountryExportStatementSettingCollection defaultValue = new CountryExportStatementSettingCollection();

			CountryExportStatementSetting sEDSetting = defaultValue.AddNew();
			sEDSetting.CountryCode = Core.Constants.CountryCodes.Australia;
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "GBH", "A user defined statement", string.Empty, string.Empty, string.Empty, "UDF", true, true, true, true, true, true));
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "MAT", "A mandatory statement", string.Empty, string.Empty, string.Empty, "MAN", true, true, true, true, true, true));
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			Factory.Save();

			aWBHeader.Populate();

			AssertContains("A mandatory statement", AWB.DetailedGoodsDescription);
		}

		public void TestExtraGoodsDescription()
		{
			AssertEquals("Extra Description should be empty", string.Empty, AWB.ExtraGoodsDescription);

			ShipmentExportAWBHeader awbHeader = (ShipmentExportAWBHeader)Shipment.AWBHeader;
			Shipment.JS_MarksAndNumbers = "Here are some marks and numbers for the shipment";

			awbHeader.Populate();
			AssertEquals("Marks and Numbers are included in the detailed goods description", "Here are some marks and numbers for the shipment", AWB.DetailedGoodsDescription);
			AssertEquals("Description fits on the first page", ZString.Empty, AWB.ExtraGoodsDescription);

			StmNote detailedDescription = AddNotes(Shipment, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Detailed goods description\nLine Two");

			awbHeader.Populate();
			AssertEquals("Detailed goods description", "Detailed goods description\nLine Two\nHere are some marks and numbers for the shipment", AWB.DetailedGoodsDescription);
			AssertEquals("Description fits on the first page", ZString.Empty, AWB.ExtraGoodsDescription);

			detailedDescription.ST_NoteText = "Here's a much longer goods description that won't fit onto the first page when printed out so it will have to be added to the extra goods description\nLine2\nLine3\nLine4\nLine5";
			Shipment.JS_MarksAndNumbers = "There are more marks and numbers now so we'll be needing a follow on page to fit everything on\nM&N2\nM&N3\nM&N4";
			awbHeader.Populate();
			awbHeader.EH_ShippingLoadAndCount = 24;

			AssertEquals("Here's a much longer goods description that won't fit onto the first page when printed out so it will have to be added to the extra goods description\nLine2\nLine3\nLine4\nLine5\nThere are more marks and numbers now so we'll be needing a follow on page to fit everything on\nM&N2\nM&N3\nM&N4",
				AWB.DetailedGoodsDescription);

			AssertEquals("Here's a much longer goods", AWB.NatureAndQuantityOfGoods1);
			AssertEquals("description that won't fit onto the", AWB.NatureAndQuantityOfGoods2);
			AssertEquals("first page when printed out so it", AWB.NatureAndQuantityOfGoods3);
			AssertEquals("will have to be added to the extra", AWB.NatureAndQuantityOfGoods4);
			AssertEquals("goods description", AWB.NatureAndQuantityOfGoods5);
			AssertEquals("Line2", AWB.NatureAndQuantityOfGoods6);
			AssertEquals("Line3", AWB.NatureAndQuantityOfGoods7);
			AssertEquals("Line4", AWB.NatureAndQuantityOfGoods8);
			AssertEquals("Line5", AWB.NatureAndQuantityOfGoods9);
			AssertEquals("There are more marks and numbers", AWB.NatureAndQuantityOfGoods10);
			AssertEquals("now so we'll be needing a follow on", AWB.NatureAndQuantityOfGoods11);
			AssertEquals("NatureAndQtyOfGoods12 contains the SLAC", "24 SLAC", AWB.NatureAndQuantityOfGoods12);
			AssertEquals("page to fit everything on\nM&N2\nM&N3\nM&N4", AWB.ExtraGoodsDescription);
		}

		void AddOuterPackLine(ShipmentExportAWBHeader aWBHeader, int length, int width, int height, int packageCount, string unitOfDimension)
		{
			PackLine outerPackLine = aWBHeader.Shipment.OuterPackLines.AddNew();

			outerPackLine.JL_Length = length;
			outerPackLine.JL_Width = width;
			outerPackLine.JL_Height = height;
			outerPackLine.JL_PackageCount = packageCount;
			outerPackLine.JL_UnitOfDimension = unitOfDimension;
		}

		public void TestIssuingCarrierNameAndAddress()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.EH_IssuingAgentName = "Test";
			aWBHeader.EH_IssuingAgentAddress1 = "Address1";
			aWBHeader.EH_IssuingAgentAddress2 = "Address2";

			AssertEquals(ZString.Format("TEST{0}ADDRESS1{0}ADDRESS2", System.Environment.NewLine), AWB.IssuingCarrierNameAndAddress);
		}

		public void TestSCI()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.EH_SpecialHandlingCode = "T1";

			AssertEquals("T1", AWB.SCI);
		}

		#endregion

		public void TestPrintOptionalInformation()
		{
			ExportAWBHeader aWBHeader = Shipment.AWBHeader;
			aWBHeader.PrintOptionalInformation = true;
			AssertEquals(true, AWB.PrintOptionalInformation);

			aWBHeader.PrintOptionalInformation = false;
			AssertEquals(false, AWB.PrintOptionalInformation);
		}

		[ExpectNoExceptions]
		public void TestAWBForLabel()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol1.Shipments.Add(shipment);
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.Shipments.Add(shipment);

			var awb = DocAWB.New(consol1.AWBHeader, Factory);
			AssertEquals(awb, awb.AWBForLabel);
			AssertEquals(consol1.AWBHeader, awb.AWBForLabel.WrappedObject);

			awb = DocAWB.New(consol2.AWBHeader, Factory);
			AssertEquals(awb, awb.AWBForLabel);
			AssertEquals(consol2.AWBHeader, awb.AWBForLabel.WrappedObject);

			awb = DocAWB.New(shipment.AWBHeader, Factory);
			AssertEquals(awb, awb.AWBForLabel);

			(shipment.AWBHeader as ShipmentExportAWBHeader).ConsolForAWBLabel = consol2;
			awb = DocAWB.New(shipment.AWBHeader, Factory);
			AssertEquals(consol2.AWBHeader, awb.AWBForLabel.WrappedObject);

			(shipment.AWBHeader as ShipmentExportAWBHeader).ConsolForAWBLabel = consol1;
			awb = DocAWB.New(shipment.AWBHeader, Factory);
			AssertEquals(consol1.AWBHeader, awb.AWBForLabel.WrappedObject);
		}

		public void TestOuterPacksNumberInConsol()
		{
			var awbLabel = FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Design1;
			awbLabel.OptionalInformation1 = AWBLabelOptionalInformationList.Codes.ShipmentPieceNumberOfCount;
			awbLabel.OptionalInformation2 = AWBLabelOptionalInformationList.Codes.ConsolPieceNumberOfCount;

			FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awbLabel);

			var consol = CreateConsol();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_OuterPacks = 3;

			consol.AWBHeader.MAWBLabelStartRange = 1;
			consol.AWBHeader.MAWBLabelTotalPacks = 5;
			consol.AWBHeader.DocumentSettingsPopulated = true;

			var consolAWB = DocAWB.New(consol.AWBHeader, Factory);

			AssertEquals(5, consolAWB.OuterPacks.Count);

			AssertEquals(1, ((OuterPack)consolAWB.OuterPacks[0].WrappedObject).NumberInConsol);
			AssertEquals("1 of 2", consolAWB.OuterPacks[0].OptionalInformation1);
			AssertEquals("1 of 5", consolAWB.OuterPacks[0].OptionalInformation2);

			AssertEquals(2, ((OuterPack)consolAWB.OuterPacks[1].WrappedObject).NumberInConsol);
			AssertEquals("2 of 2", consolAWB.OuterPacks[1].OptionalInformation1);
			AssertEquals("2 of 5", consolAWB.OuterPacks[1].OptionalInformation2);

			AssertEquals(3, ((OuterPack)consolAWB.OuterPacks[2].WrappedObject).NumberInConsol);
			AssertEquals("1 of 3", consolAWB.OuterPacks[2].OptionalInformation1);
			AssertEquals("3 of 5", consolAWB.OuterPacks[2].OptionalInformation2);

			AssertEquals(4, ((OuterPack)consolAWB.OuterPacks[3].WrappedObject).NumberInConsol);
			AssertEquals("2 of 3", consolAWB.OuterPacks[3].OptionalInformation1);
			AssertEquals("4 of 5", consolAWB.OuterPacks[3].OptionalInformation2);

			AssertEquals(5, ((OuterPack)consolAWB.OuterPacks[4].WrappedObject).NumberInConsol);
			AssertEquals("3 of 3", consolAWB.OuterPacks[4].OptionalInformation1);
			AssertEquals("5 of 5", consolAWB.OuterPacks[4].OptionalInformation2);

			shipment.AWBHeader.MAWBLabelStartRange = 4;
			shipment.AWBHeader.MAWBLabelTotalPacks = 5;
			shipment.AWBHeader.DocumentSettingsPopulated = true;

			var shipmentAWB = DocAWB.New(shipment.AWBHeader, Factory);

			AssertEquals(2, shipmentAWB.OuterPacks.Count);

			AssertEquals(4, ((OuterPack)shipmentAWB.OuterPacks[0].WrappedObject).NumberInConsol);
			AssertEquals("1 of 2", shipmentAWB.OuterPacks[0].OptionalInformation1);
			AssertEquals("4 of 5", shipmentAWB.OuterPacks[0].OptionalInformation2);

			AssertEquals(5, ((OuterPack)shipmentAWB.OuterPacks[1].WrappedObject).NumberInConsol);
			AssertEquals("2 of 2", shipmentAWB.OuterPacks[1].OptionalInformation1);
			AssertEquals("5 of 5", shipmentAWB.OuterPacks[1].OptionalInformation2);
		}

		public void TestShipmentPieceCountOverride()
		{
			var awbLabel = FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Design1;
			awbLabel.OptionalInformation1 = AWBLabelOptionalInformationList.Codes.ShipmentPieceNumberOfCount;
			awbLabel.OptionalInformation2 = AWBLabelOptionalInformationList.Codes.ConsolPieceNumberOfCount;

			FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awbLabel);

			var consol = CreateConsol();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;

			shipment.AWBHeader.LabelStartRange = 2;
			shipment.AWBHeader.LabelEndRange = 3;
			shipment.AWBHeader.LabelTotalPacks = 3;

			shipment.AWBHeader.MAWBLabelStartRange = 4;
			shipment.AWBHeader.MAWBLabelTotalPacks = 5;
			shipment.AWBHeader.DocumentSettingsPopulated = true;

			var shipmentAWB = DocAWB.New(shipment.AWBHeader, Factory);

			AssertEquals(2, shipmentAWB.OuterPacks.Count);

			AssertEquals(3, ((OuterPack)shipmentAWB.OuterPacks[0].WrappedObject).ShipmentTotalPieceCount);
			AssertEquals(4, ((OuterPack)shipmentAWB.OuterPacks[0].WrappedObject).NumberInConsol);
			AssertEquals("2 of 3", shipmentAWB.OuterPacks[0].OptionalInformation1);
			AssertEquals("4 of 5", shipmentAWB.OuterPacks[0].OptionalInformation2);

			AssertEquals(3, ((OuterPack)shipmentAWB.OuterPacks[1].WrappedObject).ShipmentTotalPieceCount);
			AssertEquals(5, ((OuterPack)shipmentAWB.OuterPacks[1].WrappedObject).NumberInConsol);
			AssertEquals("3 of 3", shipmentAWB.OuterPacks[1].OptionalInformation1);
			AssertEquals("5 of 5", shipmentAWB.OuterPacks[1].OptionalInformation2);
		}

		public void TestShipperContactCodeReturnsNoDescriptionWhenNoDetail()
		{
			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			ExportAWBHeader awb = Shipment.AWBHeader;

			awb.EH_ShipperContactDetail = string.Empty;
			awb.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			AssertEquals(string.Empty, aWBWrapper.ShipperContactCode);
		}

		public void TestShipperContactCodeStaysTheSame()
		{
			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			ExportAWBHeader awb = Shipment.AWBHeader;

			awb.EH_ShipperContactDetail = "1234";
			awb.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			AssertEquals("TE", aWBWrapper.ShipperContactCode);

			awb.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertEquals("FX", aWBWrapper.ShipperContactCode);

			awb.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertEquals("TL", aWBWrapper.ShipperContactCode);

			awb.EH_ShipperContactCode = "EML";
			AssertEquals("EML", aWBWrapper.ShipperContactCode);
		}

		public void TestAlsoNotifyContactCodeStaysTheSame()
		{
			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			ExportAWBHeader awb = Shipment.AWBHeader;

			awb.EH_AlsoNotifyContactDetail = "1234";
			awb.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			AssertEquals("TE", aWBWrapper.AlsoNotifyContactCode);

			awb.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertEquals("FX", aWBWrapper.AlsoNotifyContactCode);

			awb.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertEquals("TL", aWBWrapper.AlsoNotifyContactCode);

			awb.EH_AlsoNotifyContactCode = "TWT";
			AssertEquals("TWT", aWBWrapper.AlsoNotifyContactCode);
		}

		public void TestAlsoNotifyNameAndAddress()
		{
			var wrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			ExportAWBHeader awb = Shipment.AWBHeader;

			awb.EH_AlsoNotifyName = "ABCD Company";
			awb.EH_AlsoNotifyAddress = "1234 Street";
			awb.EH_AlsoNotifyPlace = "Sydney";
			awb.EH_AlsoNotifyState = "NSW";
			awb.EH_AlsoNotifyPostCode = "2020";
			awb.EH_AlsoNotifyCountryCode = "AU";
			awb.EH_AlsoNotifyContactCode = "Fax";
			awb.EH_AlsoNotifyContactDetail = "1234";

			ZString expectedFormatAddress = "ABCD Company" + "\n" + "1234 STREET, SYDNEY, NSW, 2020, AU, FAX, 1234";

			AssertEquals(expectedFormatAddress, wrapper.AlsoNotifyNameAndAddress);
		}

		public void TestConsigneeContactCodeStaysTheSame()
		{
			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			ExportAWBHeader awb = Shipment.AWBHeader;

			awb.EH_ConsigneeContactDetail = "1234";
			awb.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			AssertEquals("TE", aWBWrapper.ConsigneeContactCode);

			awb.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertEquals("FX", aWBWrapper.ConsigneeContactCode);

			awb.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertEquals("TL", aWBWrapper.ConsigneeContactCode);

			awb.EH_ConsigneeContactCode = "WEB";
			AssertEquals("WEB", aWBWrapper.ConsigneeContactCode);
		}

		public void TestNotifyPartyAddressReturnsContactDetailsAsEntered()
		{
			Shipment.NotifyPartyDocumentaryAddress.E2_Phone = "+44 (0)1234 987 654";

			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			Shipment.AWBHeader.Populate();

			AssertEquals("+44 (0)1234 987 654", aWBWrapper.AlsoNotifyContactDetail);
		}

		public void TestShipperAddressReturnsContactDetailsAsEntered()
		{
			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);

			Shipment.ConsignorDocumentaryAddress.E2_Phone = "+44 (0)1234 987 654";
			Shipment.AWBHeader.Populate();

			AssertEquals("+44 (0)1234 987 654", aWBWrapper.ShipperContactDetail);
		}

		public void TestConsigneeAddressReturnsContactDetailsAsEntered()
		{
			Shipment.ConsigneeDocumentaryAddress.E2_Phone = "+44 (0)1234 987 654";

			DocAWB aWBWrapper = DocAWB.New(Shipment.AWBHeader, Factory);
			Shipment.AWBHeader.Populate();

			AssertEquals("+44 (0)1234 987 654", aWBWrapper.ConsigneeContactDetail);
		}

		public void TestMAWBSendingForwarderContactDetailsAsEntered()
		{
			var orgSending = Factory.New<OrgHeader>();
			orgSending.FillWithValidTestData();
			orgSending.MainAddress.OA_Phone = "+44 (0)845 234234";

			var consol = CreateConsol();
			consol.SetDefaultSendingForwarderAddress(orgSending);

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_OA_OrgAddress = orgSending.MainAddress.PK;
			sendingForwarderContact.OC_OH = orgSending.PK;

			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			var aWBWrapper = DocAWB.New(consol.AWBHeader, Factory);
			AssertEquals(orgSending.MainAddress.OA_Phone, aWBWrapper.ShipperContactDetail);
		}

		public void TestMAWBReceivingForwarderContactDetailsAsEntered()
		{
			var orgReceiving = Factory.New<OrgHeader>();
			orgReceiving.FillWithValidTestData();
			orgReceiving.MainAddress.OA_Phone = "+44 (0)845 234234";

			var consol = CreateConsol();
			consol.SetDefaultReceivingForwarderAddress(orgReceiving);

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_OA_OrgAddress = orgReceiving.MainAddress.PK;
			receivingForwarderContact.OC_OH = orgReceiving.PK;

			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;

			var aWBWrapper = DocAWB.New(consol.AWBHeader, Factory);
			AssertEquals(orgReceiving.MainAddress.OA_Phone, aWBWrapper.ConsigneeContactDetail);
		}

		public void TestAWBHandlingInformation_InspectionText()
		{
			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "APPROVED");
			FreightDataRegistry.Instance.AWBNonApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NOT APPROVED");

			Shipment.AWBHeader.EH_HandlingInformation = "HANDLING INFO.";
			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("HANDLING INFO. NOT APPROVED ", AWB.HandlingInformation);

			Shipment.AWBHeader.EH_HandlingInformation = string.Empty;
			AssertEquals("NOT APPROVED ", AWB.HandlingInformation);

			Shipment.JS_InspectionTypeCode = "APP";
			AssertEquals("APPROVED ", AWB.HandlingInformation);

			FreightDataRegistry.Instance.AWBApprovedExporterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			AssertEquals(" ", AWB.HandlingInformation);
		}

		public void TestINCO()
		{
			AssertEquals(string.Empty, AWB.INCO);

			Shipment.JS_INCO = Core.Constants.IncoTerms.CarriagePaidTo;
			AssertEquals("INCO", Core.Constants.IncoTerms.CarriagePaidTo, AWB.INCO);

			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeCarrierSeller;
			AssertEquals("FC1 should be FCA", Core.Constants.IncoTerms.FreeCarrier, AWB.INCO);

			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeCarrierBuyer;
			AssertEquals("FC2 should be FCA", Core.Constants.IncoTerms.FreeCarrier, AWB.INCO);
		}

		public void TestUCR()
		{
			AssertEquals(string.Empty, AWB.UCR);

			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = Shipment.PK;
			declaration[JobDeclarationSchema.JE_AgentsReference] = "UCR Number";

			AssertEquals("UCR", "UCR Number", AWB.UCR);
		}

		public void TestAWBLabelOptionalDesignNumber()
		{
			AssertEquals("Default design", 0, AWB.AWBLabelOptionalDesignNumber);

			AWBLabelCustomisation awbLabel = FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.Value;
			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Design1;
			FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awbLabel);
			AssertEquals("Custom design 1", 1, AWB.AWBLabelOptionalDesignNumber);

			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Design2;
			FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awbLabel);
			AssertEquals("Custom design 2", 2, AWB.AWBLabelOptionalDesignNumber);

			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Design3;
			FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awbLabel);
			AssertEquals("Custom design 3", 3, AWB.AWBLabelOptionalDesignNumber);
		}

		public void TestGrossWeights()
		{
			Shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.1m;
			Shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.2m;
			Shipment.AWBHeader.AWBRateLines[2].ER_GrossWeight = 1.3m;
			Shipment.AWBHeader.AWBRateLines[3].ER_GrossWeight = 1.4m;
			Shipment.AWBHeader.AWBRateLines[4].ER_GrossWeight = 1.5m;
			Shipment.AWBHeader.AWBRateLines[5].ER_GrossWeight = 1.6m;
			Shipment.AWBHeader.AWBRateLines[6].ER_GrossWeight = 1.7m;
			Shipment.AWBHeader.AWBRateLines[7].ER_GrossWeight = 1.8m;
			Shipment.AWBHeader.AWBRateLines[8].ER_GrossWeight = 1.9m;
			Shipment.AWBHeader.AWBRateLines[9].ER_GrossWeight = 10.1m;
			Shipment.AWBHeader.AWBRateLines[10].ER_GrossWeight = 11.1m;

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("1.1", AWB.GrossWeight1);
			AssertEquals("1.2", AWB.GrossWeight2);
			AssertEquals("1.3", AWB.GrossWeight3);
			AssertEquals("1.4", AWB.GrossWeight4);
			AssertEquals("1.5", AWB.GrossWeight5);
			AssertEquals("1.6", AWB.GrossWeight6);
			AssertEquals("1.7", AWB.GrossWeight7);
			AssertEquals("1.8", AWB.GrossWeight8);
			AssertEquals("1.9", AWB.GrossWeight9);
			AssertEquals("10.1", AWB.GrossWeight10);
			AssertEquals("11.1", AWB.GrossWeight11);
		}

		public void TestOverriddenGrossWeight()
		{
			Shipment.AWBHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Pounds;
			Shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.1m;
			Shipment.AWBHeader.AWBRateLines[1].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			Shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.11m;

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("1.1", AWB.GrossWeight1);
			AssertEquals("1.2", AWB.GrossWeight2);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.1m;
			Shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.11m;

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("1.1", AWB.GrossWeight1);
			AssertEquals("1.11", AWB.GrossWeight2);
		}

		public void TestTotalGrossWeight()
		{
			Shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.1m;
			Shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.2m;
			Shipment.AWBHeader.AWBRateLines[2].ER_GrossWeight = 1.3m;
			AssertEquals("Precondition", 3.6m, Shipment.AWBHeader.EH_TotalGrossWeight);

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("3.6", AWB.TotalGrossWeight);
		}

		public void TestOverriddenTotalGrossWeight()
		{
			Shipment.AWBHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			Shipment.AWBHeader.AWBRateLines[0].ER_GrossWeight = 1.1m;
			Shipment.AWBHeader.AWBRateLines[1].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			Shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.11m;

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("2.3", AWB.TotalGrossWeight);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Shipment.AWBHeader.AWBRateLines[1].ER_GrossWeight = 1.11m;

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("2.21", AWB.TotalGrossWeight);
		}

		public void TestChargeableWeights()
		{
			Shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 1m;
			Shipment.AWBHeader.AWBRateLines[1].ER_ChargeableWeight = 2m;
			Shipment.AWBHeader.AWBRateLines[2].ER_ChargeableWeight = 3m;
			Shipment.AWBHeader.AWBRateLines[3].ER_ChargeableWeight = 4m;
			Shipment.AWBHeader.AWBRateLines[4].ER_ChargeableWeight = 5m;
			Shipment.AWBHeader.AWBRateLines[5].ER_ChargeableWeight = 6m;
			Shipment.AWBHeader.AWBRateLines[6].ER_ChargeableWeight = 7m;
			Shipment.AWBHeader.AWBRateLines[7].ER_ChargeableWeight = 8m;
			Shipment.AWBHeader.AWBRateLines[8].ER_ChargeableWeight = 9m;
			Shipment.AWBHeader.AWBRateLines[9].ER_ChargeableWeight = 10m;
			Shipment.AWBHeader.AWBRateLines[10].ER_ChargeableWeight = 11m;

			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			AssertEquals("1.0", AWB.ChargeableWeight1);
			AssertEquals("2.0", AWB.ChargeableWeight2);
			AssertEquals("3.0", AWB.ChargeableWeight3);
			AssertEquals("4.0", AWB.ChargeableWeight4);
			AssertEquals("5.0", AWB.ChargeableWeight5);
			AssertEquals("6.0", AWB.ChargeableWeight6);
			AssertEquals("7.0", AWB.ChargeableWeight7);
			AssertEquals("8.0", AWB.ChargeableWeight8);
			AssertEquals("9.0", AWB.ChargeableWeight9);
			AssertEquals("10.0", AWB.ChargeableWeight10);
			AssertEquals("11.0", AWB.ChargeableWeight11);
		}

		public void TestECNCRNNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CATOR";
				Shipment.JS_RL_NKOrigin = "CATOR";
				Shipment.JS_RL_NKDestination = "AUSYD";
				Shipment.Consols.Add(CreateConsol());

				var declaration = Factory.New<Integration.Customs.CA.IJobDeclaration>();
				declaration.JE_JS = Shipment.PK;
				declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_CERSProofOfReportNumber = "RC1792201232000019";
				AssertEquals(ZString.Empty, AWB.ECNCRNNumber);

				declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				Shipment.ResetCusEntryNumbers();
				AssertEquals(ZString.Empty, AWB.ECNCRNNumber);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					Shipment.ResetCusEntryNumbers();
					var num = Shipment.CusEntryNumbers.AddNew();
					num.CE_EntryType = "EXP";
					num.CE_EntryNum = "01682031001";
					AssertEquals("EXP: 01682031001", AWB.ECNCRNNumber);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
				{
					Shipment.ResetCusEntryNumbers();
					var num = Shipment.CusEntryNumbers.AddNew();
					num.CE_EntryType = "EXP";
					num.CE_EntryNum = "01682031002";
					AssertEquals("EXP: 01682031002", AWB.ECNCRNNumber);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
				{
					Shipment.ResetCusEntryNumbers();
					var num = Shipment.CusEntryNumbers.AddNew();
					num.CE_EntryType = "EXP";
					num.CE_EntryNum = "01682031003";
					AssertEquals("EXP: 01682031003", AWB.ECNCRNNumber);
				}
			}
		}

		public void TestThrowIfExportAWBHeaderIsDeleted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var awbHeader = shipment.AWBHeader;
			var awbWrapper = DocAWB.New(awbHeader, Factory);

			awbHeader.Delete();

			AssertEquals("Pre-condition: ", true, awbHeader.IsDeleted);

			AssertExceptionThrown(typeof(InvalidOperationException), () => { var exportAWBHeader = awbWrapper.ExportAWBHeader; });
		}

		public void TestWhenExportAWBDeletedAtFirst()
		{
			ErrorReporter.Clear();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var awbHeader = shipment.AWBHeader;

			var awbWrapper = DocAWB.New(null, Factory);
			AssertNull(awbWrapper);

			awbWrapper = DocAWB.New(awbHeader, Factory);
			AssertEquals(awbWrapper.IsExportAWBHeaderDeletedAtFirst.Value, false);

			awbHeader.Delete();

			awbWrapper = DocAWB.New(awbHeader, Factory);
			AssertEquals(awbWrapper.IsExportAWBHeaderDeletedAtFirst.Value, true);
		}

		[TestDate(2016, 10, 19)]
		public void TestShouldUseBOLClauseITAR()
		{
			AWB = DocAWB.New(Shipment.AWBHeader, Factory);
			Assert(AWB.ShouldUseBOLClauseITAR);

			TestDateAttribute.Date = new DateTime(2016, 11, 15);
			Assert(!AWB.ShouldUseBOLClauseITAR);
		}

		public void TestOuterPacks_AWBForLabelForPrimaryBarcodeDisplayText()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";
			shipment.JS_OuterPacks = 2;

			var consolA = shipment.Consols.AddNew();
			consolA.JK_TransportMode = Core.Constants.TransportModes.Air;
			consolA.JK_RL_NKLoadPort = "AUSYD";
			consolA.JK_RL_NKDischargePort = "AUBNE";
			consolA.JK_MasterBillNum = "17610000222";

			var consolB = shipment.Consols.AddNew();
			consolB.JK_TransportMode = Core.Constants.TransportModes.Air;
			consolB.JK_RL_NKLoadPort = "AUBNE";
			consolB.JK_RL_NKDischargePort = "CNSHA";
			consolB.JK_MasterBillNum = "19710000333";

			(shipment.AWBHeader as ShipmentExportAWBHeader).ConsolForAWBLabel = consolA;
			var awbA = DocAWB.New(shipment.AWBHeader, Factory);
			AssertEquals(consolA.AWBHeader, awbA.AWBForLabel.WrappedObject);
			AssertEquals("1761000022200001", awbA.OuterPacks[0].PrimaryBarcodeDisplayText);

			(shipment.AWBHeader as ShipmentExportAWBHeader).ConsolForAWBLabel = consolB;
			var awbB = DocAWB.New(shipment.AWBHeader, Factory);
			AssertEquals(consolB.AWBHeader, awbB.AWBForLabel.WrappedObject);
			AssertEquals("1971000033300001", awbB.OuterPacks[0].PrimaryBarcodeDisplayText);
		}

		#region Implementation

		protected override void SetUp()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.AWBHeader.EH_ShippersSignature = "ShippersSignature";
			AWB = DocAWB.New(Shipment.AWBHeader, Factory);

			base.SetUp();
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			return consol;
		}

		JobMawb CreateJobMawbForConsol()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_MAWB = "10000001";
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_IsPrinted = ZBool.False;
			return mawb;
		}

		ForwardingShipment Shipment;
		DocAWB AWB;

		#endregion
	}
}
