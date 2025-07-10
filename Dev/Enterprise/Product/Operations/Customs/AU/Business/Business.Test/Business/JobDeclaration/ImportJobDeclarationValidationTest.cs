using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ImportJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestValidateUPEAndSAC()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrors("A Self-Assessed Clearance cannot be used for goods that are Unaccompanied Personal Effects", declaration.JE_MessageSubTypeInfo);
			declaration.JE_MessageSubType = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrors("A Self-Assessed Clearance cannot be used for goods that are Unaccompanied Personal Effects", declaration.JE_MessageSubTypeInfo);
			declaration.JE_MessageSubType = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.MessageSubType.FormalEntry;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageErrors("A Self-Assessed Clearance cannot be used for goods that are Unaccompanied Personal Effects", declaration.JE_MessageSubTypeInfo);
		}

		public void TestTransportModeForOtherValidation()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertNoMessageErrors("Other transport mode is valid", declaration.JE_TransportModeInfo);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration.Validation.ValidateJE_TransportMode();
			AssertHasMessageErrors("Other transport mode is invalid", declaration.JE_TransportModeInfo);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			declaration.Validation.ValidateJE_TransportMode();
			AssertHasMessageErrors("Other transport mode is invalid", declaration.JE_TransportModeInfo);

			declaration.JE_MessageSubType = "FRM";
			declaration.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrors("Other transport mode is valid", declaration.JE_TransportModeInfo);
		}

		public override void TestMergeByForExport()
		{
			// No export testing for import
			Assert(true);
		}

		public void TestValidateImporter()
		{
			var mandatoryMsgErr = "Please enter";
			var ddpWarning = "An invoice on this job has been entered under a DDP transaction but the Importer of Record does not match the Supplier listed on the Declaration.";

			var info = declaration.JE_OH_ImporterInfo;
			declaration.JE_OH_Importer = ZGuid.Empty;
			var inv = declaration.Invoices.AddNew();
			AssertHasMessageErrorContaining(info, mandatoryMsgErr);
			AssertNoWarning(info, ddpWarning);

			var supplier = Factory.New<OrgHeader>();
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageErrorContaining(info, mandatoryMsgErr);

			inv.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasWarning(info, ddpWarning);
		}

		public void TestValidateFlightNo()
		{
			var info = declaration.JE_VoyageFlightNoInfo;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			CombineAssertions(() =>
			{
				declaration.JE_VoyageFlightNo = ZString.Empty;
				AssertHasMessageError(info, "Please enter a Flight/Folio.");

				declaration.JE_VoyageFlightNo = "A1"; // Invalid 2-char code
				AssertHasWarning(info, "The airline code is not recognized.");

				declaration.JE_VoyageFlightNo = "AU"; // Valid 2-char code
				AssertHasWarning(info, "Flight Number must be greater than 3 characters long");

				declaration.JE_VoyageFlightNo = "AB34567";
				AssertHasWarning(info, "Flight Number must be less than 6 characters long");

				declaration.JE_VoyageFlightNo = "AU3!4";
				AssertHasWarning(info, "Flight Numbers should be in a format of AN, NA or AA followed by up to four numbers(A: Alpha, N:Numeric).");

				declaration.JE_VoyageFlightNo = "AU34";
				AssertNoNotifications(info);

				declaration.JE_VoyageFlightNo = "9B34";
				AssertNoNotifications(info);

				declaration.JE_VoyageFlightNo = "9B3";
				AssertNoNotifications(info);
			});
		}

		public void TestWarehouseHasCCP()
		{
			declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).MainAddress.PK;
			AssertHasMessageErrorContaining(declaration.WarehouseDocAddress.E2_OA_AddressInfo, "The bonded warehouse doesn't have a warehouse code entered.");

			declaration.WarehouseAddress.LocalControlledPremisesID = "code";
			declaration.WarehouseDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.E2_OA_AddressInfo, "The bonded warehouse doesn't have a warehouse code entered.");
		}

		public void TestValidateJE_ExportDate()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(-1);
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, expectedExportDateErrorText);
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(1);
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, expectedExportDateErrorText);
			invoiceHeader1.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(-1);
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(2);
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, expectedExportDateErrorText);
			invoiceHeader2.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(1);
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(3);
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, expectedExportDateErrorText);

			invoiceHeader2.JZ_ValuationDateOverride = ZDateTime.Empty;
			declaration.JE_EDITransmitDate = ZDateTime.Now.AddDays(2);
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(2);
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, expectedExportDateErrorText);
			declaration.JE_EDITransmitDate = ZDateTime.Now.AddDays(1);
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(3);
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, expectedExportDateErrorText);
		}
		const string expectedExportDateErrorText = "The shipment export date is used as the date of valuation for the goods and cannot be in the future.";

		public void TestDTARMustNotBeMoreThanThreeMonthsInTheFuture()
		{
			declaration.JE_DateOfArrival = ZDateTime.Today.AddMonths(3);
			Assert(!declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			declaration.JE_DateOfArrival = declaration.JE_DateOfArrival.AddDays(1);
			Assert(declaration.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestValidateVoyageNumber()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VoyageFlightNo = "";
			AssertEquals("MessageErrors", true, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_VoyageFlightNo = "123";
			AssertEquals("MessageErrors", false, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_VoyageFlightNo = "AUA3";
			AssertEquals("MessageErrors", false, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_VoyageFlightNo = "AU000012A3";
			AssertEquals("MessageErrors", true, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_VoyageFlightNo = "AU34567";
			AssertEquals("MessageErrors", true, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_VoyageFlightNo = "AU3456";
			AssertEquals("MessageErrors", false, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_VoyageFlightNo = "AU";
			AssertEquals("MessageErrors", true, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_VoyageFlightNo = "AU12345";
			AssertEquals("MessageErrors", true, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
		}

		public void TestValidateOwnersRef()
		{
			declaration.Validation.ValidateJE_OwnerRef();
			Assert("Owners ref has an error", declaration.JE_OwnerRefInfo.HasMessageErrors());

			declaration.JE_OwnerRef = "Owners Ref";
			declaration.Validation.ValidateJE_OwnerRef();
			Assert("Owners ref does not have an error", !declaration.JE_OwnerRefInfo.HasMessageErrors());
		}

		public void TestValidateFinalDestination()
		{
			declaration.Validation.ValidateJE_RL_NKFinalDestination();
			Assert("Final Destination has an error", declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());

			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.Validation.ValidateJE_RL_NKFinalDestination();
			Assert("Final Destination doesn't have an error", !declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
		}

		public void TestValidateVessel()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_VesselName();
			Assert("Vessel doesn't have an error", !declaration.JE_VesselNameInfo.HasMessageErrors());

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_VesselName();
			Assert("Vessel has an error", declaration.JE_VesselNameInfo.HasMessageErrors());
			declaration.ZA_CustShipNo_Hidden = "XX";
			Assert("Vessel doesn't have an error", !declaration.JE_VesselNameInfo.HasMessageErrors());
			declaration.ZA_CustShipNo_Hidden = ZString.Empty;
			Assert("Vessel has an error", declaration.JE_VesselNameInfo.HasMessageErrors());

			ZQuery filter = new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, ZString.Empty);
			var vessel = Factory.LoadTop1<RefVessel>(filter);
			declaration.JE_VesselName = vessel.RV_Code;
			declaration.Validation.ValidateJE_VesselName();
			Assert("Vessel doesn't have an error", !declaration.JE_VesselNameInfo.HasMessageErrors());
		}

		public void TestValidateJE_DateOfFirstArrivalNonWEA()
		{
			ValidateDate(declaration, declaration.JE_DateOfFirstArrivalInfo, false);
		}

		public void TestValidateJE_DateOfFirstArrivalWEA()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.WarehousedByExternalAgent;
			declaration.JE_DateOfFirstArrival = ZDateTime.Today.AddMonths(-3);
			Assert("NoMessageErrors", !declaration.JE_DateOfFirstArrivalInfo.HasMessageErrors());
			declaration.JE_DateOfFirstArrival = ZDateTime.Today.AddMonths(-6);
			Assert("NoMessageErrors", !declaration.JE_DateOfFirstArrivalInfo.HasMessageErrors());
			declaration.JE_DateOfFirstArrival = ZDateTime.Today.AddMonths(-6).AddDays(-1);
			Assert("MessageErrors", !declaration.JE_DateOfFirstArrivalInfo.HasMessageErrors());
			declaration.JE_DateOfFirstArrival = ZDateTime.Today.AddMonths(-24);
			Assert("MessageErrors", !declaration.JE_DateOfFirstArrivalInfo.HasMessageErrors());
		}

		public void TestDontMessageErrorOnEmptyOwnerRefIfItIsAutoAssigned()
		{
			declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			declaration.JE_OwnerRef = ZString.Empty;
			AssertEquals("MessageErrors", true, declaration.JE_OwnerRefInfo.HasMessageErrors());
			declaration.Importer.MiscServ.OM_IMAutoImpJobRefered = true;
			declaration.JE_OwnerRef = ZString.Empty;
			AssertEquals("MessageErrors", false, declaration.JE_OwnerRefInfo.HasMessageErrors());
		}

		public void TestCheckJE_TotalNoOfPieces()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPieces = 10;
			AssertNoWarning("Warning should only apply to Import Sea", declaration.JE_TotalNoOfPiecesInfo, ImportJobDeclarationValidation.UnitsPackingUnitsWarning);

			declaration.JE_TotalNoOfPieces = 0;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNoWarning("Values are empty", declaration.JE_TotalNoOfPiecesInfo, ImportJobDeclarationValidation.UnitsPackingUnitsWarning);

			declaration.JE_TotalNoOfPieces = 5;
			declaration.Validation.ValidateJE_TotalNoOfPieces();
			AssertNoWarning("Values are initially defaulted, so should be the same", declaration.JE_TotalNoOfPiecesInfo, ImportJobDeclarationValidation.UnitsPackingUnitsWarning);

			declaration.Packages.RemoveAll();
			declaration.Validation.ValidateJE_TotalNoOfPieces();
			AssertHasWarning("Declaration units does not match with packing outer units", declaration.JE_TotalNoOfPiecesInfo, ImportJobDeclarationValidation.UnitsPackingUnitsWarning);

			var packing1 = declaration.Packages.AddNew();
			packing1.CW_OuterPacks = 7;
			declaration.Validation.ValidateJE_TotalNoOfPieces();
			AssertHasWarning(declaration.JE_TotalNoOfPiecesInfo, ImportJobDeclarationValidation.UnitsPackingUnitsWarning);

			var packing2 = declaration.Packages.AddNew();
			packing1.CW_OuterPacks = 3;
			declaration.Validation.ValidateJE_TotalNoOfPieces();
			AssertHasWarning(declaration.JE_TotalNoOfPiecesInfo, ImportJobDeclarationValidation.UnitsPackingUnitsWarning);

			declaration.JE_TotalNoOfPieces = 10;
			declaration.Validation.ValidateJE_TotalNoOfPieces();
			AssertNoWarning("Values match again", declaration.JE_TotalNoOfPiecesInfo, ImportJobDeclarationValidation.UnitsPackingUnitsWarning);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		}

		void ValidateDate(JobDeclaration declaration, ZPropertyInfo propertyInfo, bool futureDateShouldBeError)
		{
			declaration.ForceValidationInTest = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			propertyInfo.Value = ZDateTime.Today.AddMonths(-3);
			Assert("NoMessageErrors", !propertyInfo.HasMessageErrors());
			propertyInfo.Value = ZDateTime.Today.AddMonths(-6);
			Assert("NoMessageErrors", !propertyInfo.HasMessageErrors());
			propertyInfo.Value = ZDateTime.Today.AddMonths(-6).AddDays(-1);
			Assert("MessageErrors", propertyInfo.HasMessageErrors());
			if (!futureDateShouldBeError)
			{
				propertyInfo.Value = ZDateTime.Today.AddMonths(3);
				Assert("NoMessageErrors", !propertyInfo.HasMessageErrors());
			}
			propertyInfo.Value = ZDateTime.Today.AddMonths(3).AddDays(1);
			Assert("MessageErrors", propertyInfo.HasMessageErrors());
		}

		protected override JobDeclarationValidation GetNewValidationProvider(JobDeclaration jobDeclaration)
		{
			return new ImportJobDeclarationValidation(jobDeclaration);
		}

		#endregion
	}
}
