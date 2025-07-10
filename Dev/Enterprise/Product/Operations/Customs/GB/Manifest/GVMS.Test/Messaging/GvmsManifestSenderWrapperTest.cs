using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	class GvmsManifestSenderWrapperTest : TestCaseWithFactory
	{
		public void TestDirection()
		{
			manifestHeader.AMA_Nature = "ABC";
			AssertEquals("", wrappedManifestHeader.direction);

			manifestHeader.AMA_Nature = "IMP";
			AssertEquals("UK_INBOUND", wrappedManifestHeader.direction);

			manifestHeader.AMA_Nature = "EXP";
			AssertEquals("UK_OUTBOUND", wrappedManifestHeader.direction);

			manifestHeader.AMA_Nature = "G2N";
			AssertEquals("GB_TO_NI", wrappedManifestHeader.direction);

			manifestHeader.AMA_Nature = "N2G";
			AssertEquals("NI_TO_GB", wrappedManifestHeader.direction);
		}

		public void TestIsUnaccompanied()
		{
			manifestHeader.IsUnaccompanied = ZBool.False;
			Assert(!wrappedManifestHeader.isUnaccompanied);
			manifestHeader.IsUnaccompanied = ZBool.True;
			Assert(wrappedManifestHeader.isUnaccompanied);
		}

		public void TestVehicleRegNum()
		{
			manifestHeader.AMA_VehicleRegistration = "ABC123";
			AssertEquals("ABC123", wrappedManifestHeader.vehicleRegNum);
		}

		public void TestTrailerRegistrationNums()
		{
			manifestHeader.AMA_Trailer1RegNo = "trailer1";
			manifestHeader.AMA_Trailer2RegNo = "trailer2";
			AssertEquals("trailer1", wrappedManifestHeader.trailerRegistrationNums[0]);
			AssertEquals("trailer2", wrappedManifestHeader.trailerRegistrationNums[1]);
		}

		public void TestPlannedCrossing()
		{
			manifestHeader.AMA_E_DEP = new ZDateTime(2020, 12, 02, 10, 00, 00);
			manifestHeader.RouteId = "RT123";
			AssertEquals("RT123", wrappedManifestHeader.plannedCrossing.routeId);
			AssertEquals("2020-12-02T10:00", wrappedManifestHeader.plannedCrossing.localDateTimeOfDeparture);
		}

		public void TestEmptyVehicle()
		{
			var customsRef1 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var customsRef2 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			customsRef1.CSI_Code = "CDS";
			customsRef1.CSI_ReferenceNumber = "EMPTYREF1";
			customsRef1.CSI_ReferenceNumber2 = "EMPTYREF2";
			customsRef2.CSI_Code = GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle;
			customsRef2.CSI_ReferenceNumber = "EMPTYREF3";
			customsRef2.CSI_ReferenceNumber2 = "EMPTYREF4";

			manifestHeader.EmptyVehicle = "";
			AssertNull(wrappedManifestHeader.emptyVehicle);
			manifestHeader.EmptyVehicle = "OWN";
			AssertEquals(true, wrappedManifestHeader.emptyVehicle.isOwnVehicle);
			AssertEquals("EMPTYREF4", wrappedManifestHeader.emptyVehicle.sAndSMasterRefNum);
			manifestHeader.EmptyVehicle = "CON";
			customsRef2.CSI_ReferenceNumber2 = "";
			AssertEquals("EMPTYREF3", wrappedManifestHeader.emptyVehicle.sAndSMasterRefNum);
			AssertEquals(false, wrappedManifestHeader.emptyVehicle.isOwnVehicle);
		}

		public void TestCustomsDeclarations()
		{
			AssertNull(wrappedManifestHeader.customsDeclarations);
			var customsDeclaration1 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var customsDeclaration2 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var customsDeclaration3 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var customsDeclaration4 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var customsDeclaration5 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var customsDeclaration6 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			customsDeclaration1.CSI_Code = "CDS";
			customsDeclaration1.CSI_ReferenceNumber = "CUSREF1";
			customsDeclaration1.CSI_ReferenceNumber2 = "S&SREF1";
			customsDeclaration2.CSI_Code = GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;
			customsDeclaration2.CSI_ReferenceNumber = "CUSREF2";
			customsDeclaration2.CSI_ReferenceNumber2 = "S&SREF2";
			customsDeclaration3.CSI_Code = "TIR";
			customsDeclaration3.CSI_ReferenceNumber = "CUSREF3";
			customsDeclaration3.CSI_ReferenceNumber2 = "S&SREF_TIR";
			customsDeclaration4.CSI_Code = "DUC";
			customsDeclaration4.CSI_ReferenceNumber = "CUSREF4/34J";
			customsDeclaration4.CSI_ReferenceNumber2 = "";
			customsDeclaration5.CSI_Code = "DUC";
			customsDeclaration5.CSI_ReferenceNumber = "CUSREF434J";
			customsDeclaration5.CSI_ReferenceNumber2 = "";
			customsDeclaration6.CSI_Code = "CDS";
			customsDeclaration6.CSI_ReferenceNumber = "";
			customsDeclaration6.CSI_ReferenceNumber2 = "SKIP-ME-COS-REF-IS-EMPTY";

			AssertEquals(4, wrappedManifestHeader.customsDeclarations.Count);
			AssertEquals("CUSREF1", wrappedManifestHeader.customsDeclarations[0].customsDeclarationId);
			AssertEquals("S&SREF1", wrappedManifestHeader.customsDeclarations[0].sAndSMasterRefNum);
			AssertEquals("CUSREF2", wrappedManifestHeader.customsDeclarations[1].customsDeclarationId);
			AssertEquals("S&SREF2", wrappedManifestHeader.customsDeclarations[1].sAndSMasterRefNum);
			AssertEquals("CUSREF4", wrappedManifestHeader.customsDeclarations[2].customsDeclarationId);
			AssertEquals("34J", wrappedManifestHeader.customsDeclarations[2].customsDeclarationPartId);
			AssertNull(wrappedManifestHeader.customsDeclarations[2].sAndSMasterRefNum);
			AssertNull(wrappedManifestHeader.customsDeclarations[3].customsDeclarationPartId);
		}

		public void TestATADeclarations()
		{
			AssertNull(wrappedManifestHeader.ataDeclarations);
			var ataDeclaration1 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var ataDeclaration2 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var ataDeclaration3 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			ataDeclaration1.CSI_Code = GVMSCustomsReference.Codes.AtaCarnet;
			ataDeclaration1.CSI_ReferenceNumber = "ATACARNET1";
			ataDeclaration1.CSI_ReferenceNumber2 = "S&SREF3";
			ataDeclaration2.CSI_ReferenceNumber = "ATACARNET2";
			ataDeclaration2.CSI_ReferenceNumber2 = "S&SREF4";
			ataDeclaration3.CSI_Code = GVMSCustomsReference.Codes.AtaCarnet;
			ataDeclaration3.CSI_ReferenceNumber = "ATACARNET3";
			ataDeclaration3.CSI_ReferenceNumber2 = "";
			AssertEquals(2, wrappedManifestHeader.ataDeclarations.Count);
			AssertEquals("ATACARNET1", wrappedManifestHeader.ataDeclarations[0].ataCarnetId);
			AssertEquals("S&SREF3", wrappedManifestHeader.ataDeclarations[0].sAndSMasterRefNum);
			AssertEquals("ATACARNET3", wrappedManifestHeader.ataDeclarations[1].ataCarnetId);
			AssertNull(wrappedManifestHeader.ataDeclarations[1].sAndSMasterRefNum);
		}

		public void TestTIRDeclarations()
		{
			AssertNull(wrappedManifestHeader.tirDeclarations);
			var tirDeclaration1 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var tirDeclaration2 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			tirDeclaration1.CSI_Code = "TIR";
			tirDeclaration1.CSI_ReferenceNumber = "TIRCARNET1";
			tirDeclaration2.CSI_ReferenceNumber = "TIRCARNET2";
			AssertEquals(1, wrappedManifestHeader.tirDeclarations.Count);
			AssertEquals("TIRCARNET1", wrappedManifestHeader.tirDeclarations[0].tirCarnetId);
		}

		public void TestEIDRDeclarations()
		{
			AssertNull("No initial EIDRDeclarations", wrappedManifestHeader.eidrDeclarations);
			var eidrDeclaration1 = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();
			var eidrDeclaration2 = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();
			var eidrDeclaration3 = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();
			eidrDeclaration1.CSI_Code = GVMSCustomsReference.Codes.EntryInDeclarantsRecord;
			eidrDeclaration1.CSI_ReferenceNumber = "EIDRTRADEREORI1";
			eidrDeclaration1.CSI_ReferenceNumber2 = "S&SREF5";
			eidrDeclaration1.CSI_Procedure = "Pro1";
			eidrDeclaration1.CSI_Description = "CSIDescription1";
			eidrDeclaration2.CSI_ReferenceNumber = "EIDRTRADEREORI2";
			eidrDeclaration2.CSI_ReferenceNumber2 = "S&SREF6";
			eidrDeclaration2.CSI_Procedure = "Pro2";
			eidrDeclaration2.CSI_Description = "CSIDescription1";
			eidrDeclaration3.CSI_Code = GVMSCustomsReference.Codes.EntryInDeclarantsRecord;
			eidrDeclaration3.CSI_ReferenceNumber = "EIDRTRADEREORI3";
			eidrDeclaration3.CSI_ReferenceNumber2 = "";
			eidrDeclaration3.CSI_Procedure = "";
			eidrDeclaration3.CSI_Description = "";

			AssertEquals("2 EIDRDeclarations", 2, wrappedManifestHeader.eidrDeclarations.Count);
			AssertEquals("traderEORI1 set", "EIDRTRADEREORI1", wrappedManifestHeader.eidrDeclarations[0].traderEORI);
			AssertEquals("sAndSMasterREfNum set", "S&SREF5", wrappedManifestHeader.eidrDeclarations[0].sAndSMasterRefNum);
			AssertEquals("procedureCode set", eidrDeclaration1.CSI_Procedure, wrappedManifestHeader.eidrDeclarations[0].procedureCode);
			AssertEquals("localReferenceNumber set", eidrDeclaration1.CSI_Description, wrappedManifestHeader.eidrDeclarations[0].localReferenceNumber);
			AssertEquals("traderEORI3 set", "EIDRTRADEREORI3", wrappedManifestHeader.eidrDeclarations[1].traderEORI);
			AssertNull("sAndsMasterRefNum empty", wrappedManifestHeader.eidrDeclarations[1].sAndSMasterRefNum);
			AssertNull("procedureCode empty", wrappedManifestHeader.eidrDeclarations[1].procedureCode);
			AssertNull("localReferenceNumber empty", wrappedManifestHeader.eidrDeclarations[1].localReferenceNumber);
		}

		public void TestUKIMSEIDRdeclaration()
		{
			AssertNull("No initial UKIMSEIDRDeclarations", wrappedManifestHeader.ukimsEidrDeclarations);

			var ukimsEidrDeclaration1 = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();
			ukimsEidrDeclaration1.CSI_ReferenceNumber = "IMSRTRADEREORI1";
			ukimsEidrDeclaration1.CSI_Code = GVMSCustomsReference.Codes.UkInternalMarketSchemeEntryInDeclarantsRecordsDeclaration;
			ukimsEidrDeclaration1.CSI_Description = "CSIDescription1";
			var ukimsEidrDeclaration2 = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();
			ukimsEidrDeclaration2.CSI_ReferenceNumber = "IMSRTRADEREORI2";
			ukimsEidrDeclaration2.CSI_Description = "CSIDescription2";
			var ukimsEidrDeclaration3 = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();
			ukimsEidrDeclaration3.CSI_ReferenceNumber = "IMSRTRADEREORI3";
			ukimsEidrDeclaration3.CSI_Code = GVMSCustomsReference.Codes.UkInternalMarketSchemeEntryInDeclarantsRecordsDeclaration;
			ukimsEidrDeclaration3.CSI_Description = "";

			AssertEquals("2 UKIMSEIDRDeclarations", 2, wrappedManifestHeader.ukimsEidrDeclarations.Count);
			AssertEquals("traderEORI1 set", ukimsEidrDeclaration1.CSI_ReferenceNumber, wrappedManifestHeader.ukimsEidrDeclarations[0].traderEORI);
			AssertEquals("LRN set", ukimsEidrDeclaration1.CSI_Description, wrappedManifestHeader.ukimsEidrDeclarations[0].localReferenceNumber);
			AssertEquals("LRN set, nopWaiver = false", false, wrappedManifestHeader.ukimsEidrDeclarations[0].nopWaiver);
			AssertEquals("traderEORI3 set", ukimsEidrDeclaration3.CSI_ReferenceNumber, wrappedManifestHeader.ukimsEidrDeclarations[1].traderEORI);
			AssertNull("LRN empty", wrappedManifestHeader.ukimsEidrDeclarations[1].localReferenceNumber);
			AssertEquals("LRN empty, nopWaiver = true", true, wrappedManifestHeader.ukimsEidrDeclarations[1].nopWaiver);
		}

		public void TestTransitDeclarations()
		{
			AssertNull(wrappedManifestHeader.transitDeclarations);
			var transitDeclaration1 = manifestHeader.GvmsTransitReferenceCollection.AddNew();
			var transitDeclaration2 = manifestHeader.GvmsTransitReferenceCollection.AddNew();
			var transitDeclaration3 = manifestHeader.GvmsTransitReferenceCollection.AddNew();
			transitDeclaration1.CSI_Code = GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber;
			transitDeclaration1.CSI_ReferenceNumber = "TRANSITDECLARATIONID1";
			transitDeclaration1.CSI_ReferenceNumber2 = "S&SREF7";
			transitDeclaration1.CSI_Status = "Y";
			transitDeclaration2.CSI_ReferenceNumber = "TRANSITDECLARATIONID2";
			transitDeclaration2.CSI_ReferenceNumber2 = "S&SREF8";
			transitDeclaration1.CSI_Status = "Y";
			transitDeclaration3.CSI_Code = GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber;
			transitDeclaration3.CSI_ReferenceNumber = "TRANSITDECLARATIONID3";
			transitDeclaration3.CSI_ReferenceNumber2 = "";
			transitDeclaration3.CSI_Status = "N";
			AssertEquals(2, wrappedManifestHeader.transitDeclarations.Count);
			AssertEquals("TRANSITDECLARATIONID1", wrappedManifestHeader.transitDeclarations[0].transitDeclarationId);
			AssertEquals("S&SREF7", wrappedManifestHeader.transitDeclarations[0].sAndSMasterRefNum);
			AssertEquals(true, wrappedManifestHeader.transitDeclarations[0].isTSAD);
			AssertEquals(false, wrappedManifestHeader.transitDeclarations[1].isTSAD);
			AssertNull(wrappedManifestHeader.transitDeclarations[1].sAndSMasterRefNum);
		}

		public void TestICSDeclaration()
		{
			AssertNull(wrappedManifestHeader.sAndSMasterRefNum);
			var icsDeclaration1 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			icsDeclaration1.CSI_Code = GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration;
			icsDeclaration1.CSI_ReferenceNumber = "";
			icsDeclaration1.CSI_ReferenceNumber2 = "S&SP001";

			AssertEquals("S&SP001", wrappedManifestHeader.sAndSMasterRefNum);
		}

		public void TestExemptionDeclaration()
		{
			AssertEquals(null, wrappedManifestHeader.exemptionDeclaration);
			var declaration1 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			declaration1.CSI_Code = GVMSCustomsReference.Codes.ExemptGoods;
			declaration1.CSI_ReferenceNumber2 = "EXES&SREF";
			var declaration2 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			declaration2.CSI_ReferenceNumber = "REF";
			declaration2.CSI_ReferenceNumber2 = "S&SREF";
			AssertEquals(1, wrappedManifestHeader.customsDeclarations.Count);
			AssertEquals("EXES&SREF", wrappedManifestHeader.exemptionDeclaration.exemptedGoods[0].sAndSMasterRefNum);
			AssertEquals(1, wrappedManifestHeader.exemptionDeclaration.exemptedGoods.Count);
		}

		public void TestSSReferenceForEmptyVehicleDeclaration()
		{
			var declaration1 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			declaration1.CSI_Code = GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle;
			declaration1.CSI_ReferenceNumber = "REF1_1";
			manifestHeader.EmptyVehicle = "";
			AssertNull(wrappedManifestHeader.emptyVehicle);
			manifestHeader.EmptyVehicle = "OWN";
			AssertEquals(true, wrappedManifestHeader.emptyVehicle.isOwnVehicle);
			AssertNull(wrappedManifestHeader.customsDeclarations);
			AssertEquals("REF1_1", wrappedManifestHeader.emptyVehicle.sAndSMasterRefNum);
		}

		public void TestMtpDeclaration()
		{
			AssertNull("No MTPs", wrappedManifestHeader.mtpDeclaration);

			var reference1 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var reference2 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			var reference3 = manifestHeader.GvmsCustomsReferenceCollection.AddNew();
			reference1.CSI_Code = GVMSCustomsReference.Codes.ManualTransitProcedure;
			reference1.CSI_ReferenceNumber2 = "MTP1";
			reference2.CSI_Code = GVMSCustomsReference.Codes.AtaCarnet;
			reference2.CSI_ReferenceNumber2 = "SomethingElse";
			reference3.CSI_Code = GVMSCustomsReference.Codes.ManualTransitProcedure;
			reference3.CSI_ReferenceNumber2 = "MTP2";

			AssertNotNull("2 MTPs", wrappedManifestHeader.mtpDeclaration);
			AssertEquals("mtpDeclaration.mtpGoods Count", 2, wrappedManifestHeader.mtpDeclaration.mtpGoods.Count);
			AssertEquals("mtpDeclaration.mtpGoods[0].sAndSMasterRefNum", "MTP1", wrappedManifestHeader.mtpDeclaration.mtpGoods[0].sAndSMasterRefNum);
			AssertEquals("mtpDeclaration.mtpGoods[1].sAndSMasterRefNum", "MTP2", wrappedManifestHeader.mtpDeclaration.mtpGoods[1].sAndSMasterRefNum);
		}

		public void TestUkcDeclaration()
		{
			AssertNull("No UKC", wrappedManifestHeader.ukcDeclaration);
			var reference1 = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();
			var reference2 = manifestHeader.GvmsEidrAndOralReferenceCollection.AddNew();

			reference1.CSI_Code = GVMSCustomsReference.Codes.EntryInDeclarantsRecord;
			reference1.CSI_ReferenceNumber = "GB123456789011";
			reference2.CSI_Code = GVMSCustomsReference.Codes.UkCarrier;
			reference2.CSI_ReferenceNumber = "GB123456789012";

			AssertNotNull("UKC present", wrappedManifestHeader.ukcDeclaration);
			AssertEquals("ukcDeclaration.fpoEORI", "GB123456789012", wrappedManifestHeader.ukcDeclaration.fpoEORI);
		}

		public void TestHaulierType()
		{
			manifestHeader.HaulierType = ZString.Empty;
			AssertNull("HaulierType empty", wrappedManifestHeader.haulierType);

			foreach (var (code, expected) in new[]
			{
				(GVMSHaulierType.Codes.Standard, "STANDARD"),
				(GVMSHaulierType.Codes.FastParcelOperatorsThatAreMembersOfTheAntiSmugglingNetwork, "FPO_ASN"),
				(GVMSHaulierType.Codes.FastParcelOperatorsThatAreNotMembersOfTheAsnAndAreNotMovingGoodsWithAMemorandumOfUnderstanding, "FPO_OTHER"),
				(GVMSHaulierType.Codes.NatoOrMinistryOfDefence, "NATO_MOD"),
				(GVMSHaulierType.Codes.RoyalMailGroup, "RMG"),
				(GVMSHaulierType.Codes.ExtraTerritorialOfficeOfExchange, "ETOE"),
			})
			{
				manifestHeader.HaulierType = code;
				AssertEquals($"HaulierType {manifestHeader.HaulierType}", expected, wrappedManifestHeader.haulierType);
			}
		}

		public void TestContainerReferenceNums()
		{
			AssertNull("No containers", wrappedManifestHeader.containerReferenceNums);
			var container1 = manifestHeader.Containers.AddNew();
			AssertNull("1 container but empty reference", wrappedManifestHeader.containerReferenceNums);
			container1.ACN_ContainerNumber = "C1230";
			AssertContainsExactElementsInAnyOrder("1 containter C1230", new[] { "C1230" }, wrappedManifestHeader.containerReferenceNums);
			manifestHeader.Containers.AddNew();
			AssertContainsExactElementsInAnyOrder("Containters C1230,(empty)", new[] { "C1230" }, wrappedManifestHeader.containerReferenceNums);
			var container3 = manifestHeader.Containers.AddNew();
			container3.ACN_ContainerNumber = "C1231";
			AssertContainsExactElementsInAnyOrder("Containters C1230,(empty),C1231", new[] { "C1230", "C1231" }, wrappedManifestHeader.containerReferenceNums);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			wrappedManifestHeader = new GvmsManifestSenderWrapper(manifestHeader);
		}

		AsycudaManifestHeader manifestHeader;
		IGvmsMessageBuilder wrappedManifestHeader;
	}
}
