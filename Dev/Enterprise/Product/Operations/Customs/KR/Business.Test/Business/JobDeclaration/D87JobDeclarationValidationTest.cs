using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class D87JobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public override void TestJE_ShipmentIncoTerm()
		{
			Declaration.JE_ShipmentIncoTerm = "XXX";
			AssertNoMessageErrors(Declaration.JE_ShipmentIncoTermInfo);

			Declaration.JE_ShipmentIncoTerm = IncotermList.Codes.CarriageAndInsurancePaidTo;
			AssertNoMessageErrors(Declaration.JE_ShipmentIncoTermInfo);
		}

		public void TestJE_AgentsReference()
		{
			var temp_Company = Factory.New<GlbCompany>();
			temp_Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var temp_Branch = temp_Company.Branches.AddNew();
			temp_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth)).RL_Code;

			var declaration0 = Factory.New<JobDeclaration>();
			declaration0.JE_GC = temp_Company.PK;
			declaration0.JE_GB = temp_Branch.PK;
			declaration0.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			declaration0.JE_AgentsReference = "US899916191";
			Factory.Save();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			declaration1.JE_AgentsReference = "US899916191";
			AssertNoMessageErrors(declaration1.JE_AgentsReferenceInfo);
			Factory.Save();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			declaration2.JE_AgentsReference = "US899916191";
			var messageError = string.Format("[{0}] has this carnet certificate number. Please check if this number is correct.", declaration1.JE_DeclarationReference);
			AssertHasMessageErrorContaining(declaration2.JE_AgentsReferenceInfo, messageError);
			declaration2.JE_AgentsReference = "";
			Factory.Save();

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			declaration3.JE_AgentsReference = "";
			AssertNoMessageErrors(declaration3.JE_AgentsReferenceInfo);
		}

		public override void TestCheckJE_DateOfArrival()
		{
			Declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 10, 10, 10, 10);
			Declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 10);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_TransportMode = "SEA";
			Declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 0, 0, 10);
			Declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 11, 23, 10, 10);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_TransportMode = "AIR";
			Declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 0, 0, 10);
			Declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
			AssertNoWarnings(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 0, 0, 10);
			Declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 11, 23, 10, 10);
			AssertNoWarnings(Declaration.JE_DateOfArrivalInfo);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
			Declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 9, 23, 10, 10);
			AssertNoWarnings(Declaration.JE_DateOfArrivalInfo);
			AssertNoMessageErrors(Declaration.JE_DateOfArrivalInfo);
		}

		public override void TestEmptyWeightUnit()
		{
			Declaration.JE_TotalWeight = 10m;
			Declaration.JE_TotalWeightUnit = "";
			AssertNoWarnings(Declaration.JE_TotalWeightUnitInfo);
		}

		public new void TestJE_TotalNoOfPacks_OM_IMBalanceInvoicePackage()
		{
			Declaration.JE_TotalNoOfPacks = 100;
			Declaration.Invoices.AddNew().JZ_NoOfPacks = 12.5m;

			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MiscServ = Factory.New<OrgMiscServ>();
			Declaration.JE_OH_Importer = orgHeader.PK;
			Declaration.Importer.MiscServ.OM_IMBalanceInvoicePackage = true;

			var validation = (D87JobDeclarationValidation)Declaration.Validation;
			validation.ValidateJE_TotalNoOfPacks();
			AssertNoMessageErrors(Declaration.JE_TotalNoOfPacksInfo);
			AssertNoWarnings(Declaration.JE_TotalNoOfPacksInfo);

			Declaration.Importer.MiscServ.OM_IMBalanceInvoicePackage = false;
			validation.ValidateJE_TotalNoOfPacks();
			AssertNoWarnings(Declaration.JE_TotalNoOfPacksInfo);
			AssertNoMessageErrors(Declaration.JE_TotalNoOfPacksInfo);
		}

		public new void TestInvalidWeightUnit()
		{
			Declaration.JE_TotalWeight = 0m;
			Declaration.JE_TotalWeightUnit = "XY";
			AssertNoMessageErrors(Declaration.JE_TotalWeightUnitInfo);
		}

		public new void TestNegativeWeight()
		{
			Declaration.JE_TotalWeight = -1m;
			AssertNoErrors(Declaration.JE_TotalWeightInfo);
		}

		public void TestNoMessageErrorsForUnusedFieldsOfD87()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			Declaration.JE_OA_SupplierAddress = Factory.New<OrgAddress>().PK;
			Declaration.JE_ExportGoodsType = "A";
			Declaration.JE_CustomsOffice = "010";
			Declaration.JE_TotalNoOfPieces = 1;
			Declaration.JE_TotalWeight = 1;
			Declaration.JE_TotalWeightUnit = "KG";
			Declaration.JE_TotalNoOfPacks = 1;
			Declaration.JE_TotalNoOfPacksPackType = "CT";
			Declaration.JE_CustomsDivision = "10";
			Declaration.JE_HouseBill = "HB1";

			Declaration.Validation.ValidateAll();
			Assert(!Declaration.HasMessageErrors);

			var declarationNote = Declaration.Notes.AddNew();
			declarationNote.ST_NoteType = StmNoteDescription.Pub;
			declarationNote.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			declarationNote.ST_NoteText = "VISAGE COSMETIC SURGERY SYSTEM";

			declarationNote.Validation.ValidateAll();
			Assert(!declarationNote.HasMessageErrors);
		}

		public void TestJE_ContainerMode()
		{
			Declaration.JE_ContainerMode = ZString.Empty;
			AssertNoMessageErrors(Declaration.JE_ContainerModeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
		}
		JobDeclaration Declaration => (JobDeclaration)declaration;
	}
}
