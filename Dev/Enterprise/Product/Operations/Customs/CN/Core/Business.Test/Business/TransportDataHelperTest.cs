using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.CN.Business.Testing
{
	class TransportDataHelperTest : TestCaseWithFactory
	{
		public void TestPopulateBillNumber()
		{
			testDeclaration.JE_TransitMode = TransitModeList.Codes.DeclaringInAdvance;

			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
			testDeclaration.JE_VesselInland = "Vessel Inland";
			AssertEquals("Populate JE_VesselInland", "Vessel Inland", testTransportDataHelper.PopulateBillNumber(testCusEntryHeader));

			testDeclaration.JE_VesselInland = "";
			string[] transitModes = { TransitModeList.Codes.DirectTransition, TransitModeList.Codes.Transshipment };
			string[] transportTypes = { Customs.Business.TransportTypeList.Codes.Sea, Customs.Business.TransportTypeList.Codes.Rail, Customs.Business.TransportTypeList.Codes.Air };
			foreach (var transitMode in transitModes)
			{
				foreach (var transportType in transportTypes)
				{
					testDeclaration.JE_TransitMode = transitMode;
					testDeclaration.JE_TransportMode = transportType;
					testInstruction.BillOfLading = "Bill Of Lading";
					AssertEquals("Populate EntryInstruction.BillOfLading", "Bill Of Lading", testTransportDataHelper.PopulateBillNumber(testCusEntryHeader));
				}
			}

			testDeclaration.JE_TransitMode = TransitModeList.Codes.DeclaringInAdvance;
			AssertEquals("Keep empty", true, testTransportDataHelper.PopulateBillNumber(testCusEntryHeader).IsEmpty);
			testDeclaration.JE_TransitMode = TransitModeList.Codes.Transshipment;
			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
			AssertEquals("Keep empty", true, testTransportDataHelper.PopulateBillNumber(testCusEntryHeader).IsEmpty);

			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			testDeclaration.JE_TransitMode = TransitModeList.Codes.DeclaringInAdvance;
			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
			testDeclaration.JE_VesselInland = "Vessel Inland";
			AssertEquals("Populate JE_VesselInland", "Vessel Inland", testTransportDataHelper.PopulateBillNumber(testCusEntryHeader));

			testDeclaration.JE_VesselInland = "";
			testDeclaration.JE_TransitMode = TransitModeList.Codes.Transshipment;
			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			testInstruction.BillOfLading = "Bill Of Lading";
			AssertEquals("Populate EntryInstruction.BillOfLading", "Bill Of Lading", testTransportDataHelper.PopulateBillNumber(testCusEntryHeader));

			testDeclaration.JE_TransitMode = TransitModeList.Codes.DeclaringInAdvance;
			AssertEquals("Keep empty", true, testTransportDataHelper.PopulateBillNumber(testCusEntryHeader).IsEmpty);
			testDeclaration.JE_TransitMode = TransitModeList.Codes.Transshipment;
			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
			AssertEquals("Keep empty", true, testTransportDataHelper.PopulateBillNumber(testCusEntryHeader).IsEmpty);
		}

		public void TestPopulateVesselName()
		{
			testDeclaration.JE_TransitMode = TransitModeList.Codes.Transshipment;

			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			testDeclaration.JE_VesselName = "Vessel Code";
			testDeclaration.JE_VesselInland = "Vessel Inland";
			AssertEquals("Populate JE_VesselName", "Vessel Code", testTransportDataHelper.PopulateVesselName());

			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			AssertEquals("Populate @", "@", testTransportDataHelper.PopulateVesselName());

			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Rail;
			testDeclaration.JE_VesselName = "Vessel Code";
			AssertEquals("Populate JE_VesselName", "Vessel Code", testTransportDataHelper.PopulateVesselName());

			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
			var gclNum = testDeclaration.AdditionalReferenceNumbers.AddNew();
			gclNum.CE_EntryType = AdditionalReferenceNumberTypes.Codes.GoodsCarriedListNo;
			gclNum.CE_EntryNum = "GCL";
			var dtdNum = testDeclaration.AdditionalReferenceNumbers.AddNew();
			dtdNum.CE_EntryType = AdditionalReferenceNumberTypes.Codes.DTDPreNumber;
			dtdNum.CE_EntryNum = "DTD";
			var pslNum = testDeclaration.AdditionalReferenceNumbers.AddNew();
			pslNum.CE_EntryType = AdditionalReferenceNumberTypes.Codes.ProposalNo;
			pslNum.CE_EntryNum = "PSL";
			AssertEquals("Populate '@' + Reference Number GCL", "@GCL", testTransportDataHelper.PopulateVesselName());

			testDeclaration.JE_TransitMode = TransitModeList.Codes.DirectTransition;
			AssertEquals("Populate '@' + Reference Number GCL", "@GCL", testTransportDataHelper.PopulateVesselName());

			gclNum.CE_EntryNum = "";
			AssertEquals("Fallback to DTD", "@DTD", testTransportDataHelper.PopulateVesselName());

			dtdNum.CE_EntryNum = "";
			AssertEquals("Keep empty", "", testTransportDataHelper.PopulateVesselName());

			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			testDeclaration.JE_TransitMode = TransitModeList.Codes.Transshipment;
			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;

			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			testDeclaration.JE_VesselInland = "Vessel Inland";
			AssertEquals("Populate JE_VesselInland", "Vessel Inland", testTransportDataHelper.PopulateVesselName());

			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
			testDeclaration.JE_CustomsOffice = "OFC";
			testDeclaration.JE_VesselInland = "Vessel Inland";
			AssertEquals("Populate JE_CustomsOffice + JE_VesselInland", "OFCVessel Inland", testTransportDataHelper.PopulateVesselName());

			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Rail;
			AssertEquals("Populate JE_CustomsOffice + JE_VesselInland", "OFCVessel Inland", testTransportDataHelper.PopulateVesselName());

			testDeclaration.JE_TransitMode = TransitModeList.Codes.DirectTransition;
			AssertForNotTransshipmentBySea(gclNum, dtdNum, pslNum);

			testDeclaration.JE_TransitMode = TransitModeList.Codes.Transshipment;
			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;

			AssertForNotTransshipmentBySea(gclNum, dtdNum, pslNum);

			void AssertForNotTransshipmentBySea(Common.CusEntryNumber gclNumber, Common.CusEntryNumber dtdNumber, Common.CusEntryNumber pslNumber)
			{
				gclNumber.CE_EntryNum = "GCL";
				dtdNumber.CE_EntryNum = "DTD";
				pslNumber.CE_EntryNum = "PSL";
				AssertEquals("Populate '@' + Reference Number GCL", "@GCL", testTransportDataHelper.PopulateVesselName());

				gclNumber.CE_EntryNum = "";
				AssertEquals("Fallback to DTD", "@DTD", testTransportDataHelper.PopulateVesselName());

				dtdNumber.CE_EntryNum = "";
				AssertEquals("Keep empty", "", testTransportDataHelper.PopulateVesselName());
			}

			gclNum.CE_EntryNum = "GCL";
			testDeclaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Keep empty for multiply instructions", "@", testTransportDataHelper.PopulateVesselName());
		}

		public void TestPopulateVoyage()
		{
			testDeclaration.JE_TransitMode = TransitModeList.Codes.Transshipment;

			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			testDeclaration.JE_VoyageFlightNo = "Voyage";
			testDeclaration.JE_VoyageInland = "Voyage Inland";
			AssertEquals("Populate JE_VoyageInland", "@Voyage", testTransportDataHelper.PopulateVoyage(testCusEntryHeader));

			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Rail;
			testDeclaration.JE_DateOfArrival = new ZDateTime(2020, 7, 31);
			AssertEquals("Populate '@' + JE_DateOfArrival('yyyyMMdd')", "@20200731", testTransportDataHelper.PopulateVoyage(testCusEntryHeader));

			testDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;

			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals("Populate JE_VoyageInland", "Voyage Inland", testTransportDataHelper.PopulateVoyage(testCusEntryHeader));

			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
			testDeclaration.JE_ExportDate = new ZDateTime(2020, 6, 30);
			AssertEquals("Populate '@' + JE_ExportDate('yyMMdd')", "200630", testTransportDataHelper.PopulateVoyage(testCusEntryHeader));

			testDeclaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Rail;
			AssertEquals("Populate '@' + JE_ExportDate('yyMMdd')", "200630", testTransportDataHelper.PopulateVoyage(testCusEntryHeader));

			testDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
			AssertEquals("Keep empty", "", testTransportDataHelper.PopulateVoyage(testCusEntryHeader));
		}

		public void TestBooleanProperties()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			AssertEquals(true, !declaration.TransportDataHelper.IsSea);
			AssertEquals(true, !declaration.TransportDataHelper.IsRail);
			AssertEquals(true, !declaration.TransportDataHelper.IsRoad);
			AssertEquals(true, !declaration.TransportDataHelper.IsAir);
			AssertEquals(true, !declaration.TransportDataHelper.IsMail);
			AssertEquals(true, !declaration.TransportDataHelper.IsPipeline);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(true, declaration.TransportDataHelper.IsSea);
			AssertEquals(true, !declaration.TransportDataHelper.IsRail);
			AssertEquals(true, !declaration.TransportDataHelper.IsRoad);
			AssertEquals(true, !declaration.TransportDataHelper.IsAir);
			AssertEquals(true, !declaration.TransportDataHelper.IsMail);
			AssertEquals(true, !declaration.TransportDataHelper.IsPipeline);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals(true, !declaration.TransportDataHelper.IsSea);
			AssertEquals(true, declaration.TransportDataHelper.IsRail);
			AssertEquals(true, !declaration.TransportDataHelper.IsRoad);
			AssertEquals(true, !declaration.TransportDataHelper.IsAir);
			AssertEquals(true, !declaration.TransportDataHelper.IsMail);
			AssertEquals(true, !declaration.TransportDataHelper.IsPipeline);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals(true, !declaration.TransportDataHelper.IsSea);
			AssertEquals(true, !declaration.TransportDataHelper.IsRail);
			AssertEquals(true, declaration.TransportDataHelper.IsRoad);
			AssertEquals(true, !declaration.TransportDataHelper.IsAir);
			AssertEquals(true, !declaration.TransportDataHelper.IsMail);
			AssertEquals(true, !declaration.TransportDataHelper.IsPipeline);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals(true, !declaration.TransportDataHelper.IsSea);
			AssertEquals(true, !declaration.TransportDataHelper.IsRail);
			AssertEquals(true, !declaration.TransportDataHelper.IsRoad);
			AssertEquals(true, declaration.TransportDataHelper.IsAir);
			AssertEquals(true, !declaration.TransportDataHelper.IsMail);
			AssertEquals(true, !declaration.TransportDataHelper.IsPipeline);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals(true, !declaration.TransportDataHelper.IsSea);
			AssertEquals(true, !declaration.TransportDataHelper.IsRail);
			AssertEquals(true, !declaration.TransportDataHelper.IsRoad);
			AssertEquals(true, !declaration.TransportDataHelper.IsAir);
			AssertEquals(true, declaration.TransportDataHelper.IsMail);
			AssertEquals(true, !declaration.TransportDataHelper.IsPipeline);

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals(true, !declaration.TransportDataHelper.IsSea);
			AssertEquals(true, !declaration.TransportDataHelper.IsRail);
			AssertEquals(true, !declaration.TransportDataHelper.IsRoad);
			AssertEquals(true, !declaration.TransportDataHelper.IsAir);
			AssertEquals(true, !declaration.TransportDataHelper.IsMail);
			AssertEquals(true, declaration.TransportDataHelper.IsPipeline);

			declaration.JE_RL_NKPortOfLoading = "CN";
			declaration.JE_RL_NKPortOfArrival = "CN";
			AssertEquals(true, !declaration.TransportDataHelper.IsSea);
			AssertEquals(true, !declaration.TransportDataHelper.IsRail);
			AssertEquals(true, !declaration.TransportDataHelper.IsRoad);
			AssertEquals(true, !declaration.TransportDataHelper.IsAir);
			AssertEquals(true, !declaration.TransportDataHelper.IsMail);
			AssertEquals(true, !declaration.TransportDataHelper.IsPipeline);
		}

		public void TestValidationModeProvider()
		{
			ValidationExtensionsTest.AssertValidationModeProvider(testDeclaration, testTransportDataHelper.ValidationModeProvider);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var setup = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());

			testCusEntryHeader = setup.EntryHeader;
			testDeclaration = setup.JobDeclaration;
			testInstruction = setup.EntryInstruction;
			testTransportDataHelper = new TransportDataHelper(testDeclaration);
		}
		CusEntryHeader testCusEntryHeader;
		JobDeclaration testDeclaration;
		CusEntryInstruction testInstruction;
		TransportDataHelper testTransportDataHelper;
	}
}
